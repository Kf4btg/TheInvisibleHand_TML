// ﻿using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Input;
using InvisibleHand.Utils;

namespace InvisibleHand
{
    public class IHBase : Mod
    {

        internal static int[] itemCategories;

        /// holds the game's original strings for loot-all, dep-all, quick-stack, etc;
        /// we're going to be removing these later on, but will use their
        /// original values to replace them with newer, better buttons.
        public static Dictionary<int, string> OriginalButtonLabels { get; private set; }

        public static readonly Dictionary<string, bool> ModOptions = new Dictionary<string, bool>();
        public static readonly Dictionary<string, Keys> ActionKeys = new Dictionary<string, Keys>();

        private IHCommandHandler commandHandler;

        public override string Name
        {
            get { return "TheInvisibleHand"; }
        }

        public IHBase()
        {
            Properties = new ModProperties()
            {
                Autoload = true
                // AutoloadGores = true,
                // AutoloadSounds = true,
            };

            commandHandler = new IHCommandHandler(this);



            // default options
            // ModOptions["UseReplacers"] = new BoolOption(this, true);
            //
            // // default hotkeys
            // ActionKeys["Sort"] = new KeyOption(this, Keys.R);
            // ActionKeys["Clean"] = new KeyOption(this, Keys.T);
            //
            // ActionKeys["DepositAll"] = new KeyOption(this, Keys.Z);
            // ActionKeys["LootAll"] = new KeyOption(this, Keys.X);
            // ActionKeys["QuickStack"] = new KeyOption(this, Keys.C);
            // ActionKeys["SmartDeposit"] = new KeyOption(this, Keys.V);
            // ActionKeys["SmartLoot"] = new KeyOption(this, Keys.B);
        }

        public override void Load()
        {
            OriginalButtonLabels = new Dictionary<int, string>(Constants.LangInterIndices.Count);
            // pull values out of Lang.inter to populate OBL
            foreach (var kvp in Constants.LangInterIndices)
            {
                OriginalButtonLabels[kvp.Key] = Lang.inter[kvp.Value];
            }

            foreach (var kvp in Constants.DefaultOptionValues)
            {
                this.RegisterOption(kvp.Key, kvp.Value, onOptionChanged);
                ModOptions[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in Constants.DefaultKeys)
            {
                this.RegisterOption(kvp.Key, kvp.Value, onKeyBindChanged);
                ActionKeys[kvp.Key]=kvp.Value;
            }

            // setup help output
            commandHandler.Initialize();
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

        private static void onKeyBindChanged(string name, Keys value)
        {
            ActionKeys[name] = value;
        }
    }
}
