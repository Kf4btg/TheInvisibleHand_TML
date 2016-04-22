// ﻿using System;
using System.Collections.Generic;
using Terraria.ModLoader;
// using Terraria;
using Microsoft.Xna.Framework.Input;
using InvisibleHand.Utils;
using InvisibleHand.Definitions;

namespace InvisibleHand
{

    using _flagCollection = IDictionary<string, IDictionary<string, int>>;

    public class IHBase : Mod
    {
        internal static int[] itemCategories;

        public static IHBase Instance { get; private set; }

        /// Mapping of:
        /// 	Item Trait-Group Name (e.g. "Weapon") ->
        ///				(Mapping of: Trait Name (e.g "melee") -> int flag value (power of 2))
        ///
        /// Examples (arbitrary values used):
        /// 	var mel_weapon = FlagCollection["Weapon"]["style_melee"] // == 4;
        ///		var tombstone = FlagCollection["Furniture.Other"]["tombstone"] // == 1048576;
        ///
        ///
        /// This allows such operations as (this fictional, bad idea):
        ///		Func<Item, bool> TombSledge = (item) =>
        ///				(item.Flags["Weapon"] &amp; mel_weapon) > 0
        ///				&amp;&amp; (item.Flags["Furniture.Other"] &amp; tombstone) > 0;
        public static _flagCollection FlagCollection => CategoryParser.FlagCollection;

        /// This holds the loaded values for how to match an item to given category;
        /// It is a mapping of "Category_Name" -> {"TraitGroup": combined_value_of_flags_for_group}
        public static IDictionary<string, ItemCategory> CategoryDefs => CategoryParser.CategoryDefinitions;

        /// And this returns the traversal tree used to assign a category to an item
        public static SortedAutoTree<string, ItemCategory> CategoryTree => CategoryParser.CategoryTree;


        /// holds the game's original strings for loot-all, dep-all, quick-stack, etc;
        /// we're going to be removing these later on, but will use their
        /// original values to replace them with newer, better buttons.
        // public static Dictionary<int, string> OriginalButtonLabels { get; private set; }

        public static readonly Dictionary<string, bool> ModOptions = new Dictionary<string, bool>();
        // public static readonly Dictionary<string, Keys> ActionKeys = new Dictionary<string, Keys>();

        private IHCommandHandler commandHandler;

        public IHPlayer localplayer { get; internal set; }

        public override string Name
        {
            get { return "TheInvisibleHand"; }
        }

        public IHBase()
        {
            Instance = this;

            Properties = new ModProperties()
            {
                Autoload = true
                // AutoloadGores = true,
                // AutoloadSounds = true,
            };

            commandHandler = new IHCommandHandler(this);

            // default options
            // ModOptions["UseReplacers"] = new BoolOption(this, true);

        }

        public override void Load()
        {
            // OriginalButtonLabels = new Dictionary<int, string>(Constants.LangInterIndices.Count);
            // pull values out of Lang.inter to populate OBL
            // foreach (var kvp in Constants.LangInterIndices)
            // {
            //     OriginalButtonLabels[kvp.Key] = Lang.inter[kvp.Value];
            // }

            foreach (var kvp in Constants.DefaultOptionValues)
            {
                this.RegisterOption(kvp.Key, kvp.Value, onOptionChanged);
                ModOptions[kvp.Key] = kvp.Value;
            }

            // setup default hotkeys
            foreach (var kvp in Constants.DefaultKeys)
            {
                RegisterHotKey(kvp.Key, kvp.Value);
                // ActionKeys[kvp.Key]=kvp.Value;
            }

            // setup help output
            commandHandler.Initialize();

            // load the item flags and category definitions
            CategoryParser.Parse();
        }

        public static bool ShiftHeld() => Keys.LeftShift.Down() || Keys.RightShift.Down();

        public override void HotKeyPressed(string name)
        {
            switch (name)
            {
                case "Sort":
                    localplayer.Sort(ShiftHeld());
                    break;
                case "Clean":
                    localplayer.CleanStacks();
                    break;

                case "DepositAll":
                    if (localplayer.player.chest != -1)
                        localplayer.DepositAll(ShiftHeld());
                    break;
                case "QuickStack":
                    if (localplayer.player.chest != -1)
                        localplayer.QuickStack(ShiftHeld());
                    break;
                case "LootAll":
                    if (localplayer.player.chest != -1)
                        localplayer.LootAll();
                        break;
            }
        }

        // utilize chat commands to set mod options
        public override void ChatInput(string text)
        {
            if (text[0] != '/' || text.Length==1) return;

            commandHandler.HandleCommand(text);
        }

        // option-change callbacks
        private static void onOptionChanged(string name, bool value)
        {
            ModOptions[name] = value;
        }

        // private static void onKeyBindChanged(string name, Keys value)
        // {
        //     ActionKeys[name] = value;
        // }
    }
}
