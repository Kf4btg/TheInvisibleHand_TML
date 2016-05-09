using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Input;
using InvisibleHand.Utils;
using InvisibleHand.Items.Categories;
using InvisibleHand.Items.Categories.Types;

namespace InvisibleHand
{

    using _flagCollection = Dictionary<string, IDictionary<string, int>>;

    public class IHBase : Mod
    {
        internal static int[] itemCategories;

        public static IHBase Instance { get; private set; }

        // public static KeyboardState lastState = Keyboard.GetState();

        private static Dictionary<string, Keys> modhotkeys;


        internal static bool holding_hotkey = false;
        internal static Keys HeldHotKey { get; set; } = Keys.None;
        /// This is a tModLoader implementation detail and I know it's not safe
        /// to rely on it...but here we are.
        internal static string ConfiguredHotkey(string action_name) =>
            Main.Configuration.Get<string>("TheInvisibleHand_HotKey_"+action_name.Replace(' ', '_'), Constants.DefaultKeys[action_name]);


        /// Mapping of:
        /// 	Item Trait-Group Name (e.g. "Weapon") ->
        ///				(Mapping of: Trait Name (e.g "melee") ->int flag value (power of 2))
        ///
        /// Examples (arbitrary values used):
        /// 	var mel_weapon = FlagCollection["Weapon"]["style_melee"] // == 4;
        ///		var tombstone = FlagCollection["Furniture.Other"]["tombstone"] // == 1048576;
        ///
        ///
        /// This allows such operations as (this fictional, bad idea):
        ///		Func&lt;Item, bool&gt; TombSledge = (item) =>
        ///				(item.Flags["Weapon"] &amp; mel_weapon) != 0
        ///				&amp;&amp; (item.Flags["Furniture.Other"] &amp; tombstone) != 0;
        public static _flagCollection FlagCollection;

        /// This holds the loaded values for how to match an item to given category;
        /// It is a mapping of "Category_Name" -> {"TraitGroup": combined_value_of_flags_for_group}
        // public static IDictionary<string, ItemCategory> CategoryDefs => Parser.CategoryDefinitions;

        // public static SortedAutoTree<string, ItemCategory> CategoryTree => CategoryParser.CategoryTree;
        // public static SortedAutoTree<int, ItemCategory> CategoryTree => Parser.CategoryTree;
        /// returns the traversal tree used to assign a category to an item.
        /// uses keys based on a category's ordinal (ordering rank).
        public static SortedAutoTree<int, ItemCategory> CategoryTree;
        // public static SortedAutoTree<int, ICategory<Item>> CategoryTree;


        // holds the game's original strings for loot-all, dep-all, quick-stack, etc;
        // we're going to be removing these later on, but will use their
        // original values to replace them with newer, better buttons.
        // public static Dictionary<int, string> OriginalButtonLabels { get; private set; }

        internal static OptionManager<bool> ModOptions = new OptionManager<bool>();
        // public static readonly Dictionary<string, bool> ModOptions = new Dictionary<string, bool>();
        // public static readonly Dictionary<string, Keys> ActionKeys = new Dictionary<string, Keys>();

        private IHCommandHandler commandHandler;

        public IHPlayer localplayer { get; internal set; }

        public override string Name => "TheInvisibleHand";

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
            modhotkeys = new Dictionary<string, Keys>();


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
                // ModOptions.RegisterOption(kvp.Key, kvp.Value, onOptionChanged);
                ModOptions.RegisterOption(kvp.Key, kvp.Value);
                // ModOptions[kvp.Key] = kvp.Value;
            }

            // setup default hotkeys
            foreach (var kvp in Constants.DefaultKeys)
            {
                RegisterHotKey(kvp.Key, kvp.Value);

                Keys actualkey;
                string configval = ConfiguredHotkey(kvp.Key);
                if (Enum.TryParse(configval, out actualkey))
                    modhotkeys[kvp.Key] = actualkey;
                else
                    // TODO: this should throw an error
                    modhotkeys[kvp.Key] = Keys.None;
                // ActionKeys[kvp.Key]=kvp.Value;
            }

            // setup help output
            commandHandler.Initialize();

            // load the item flags and category definitions
            FlagCollection = new _flagCollection();
            CategoryTree = new SortedAutoTree<int, ItemCategory>() {Label=0};
            Parser.Parse();
        }

        internal static bool ShiftHeld() => Keys.LeftShift.Down() || Keys.RightShift.Down();

        public override void HotKeyPressed(string name)
        {
            // do nothing if the inventory isn't shown.
            if (!Main.playerInventory) return;

            // Main.Configuration.Get<string>(this.Name+"_HotKey_"+name.Replace(' ', '_'), Constants.DefaultKeys[name]);

            // don't rapid-fire
            if (holding_hotkey)
                return;

            holding_hotkey = true;

            // TODO: track when the key-configuration changes
            HeldHotKey = modhotkeys[name];


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

        /// utilize chat commands to set mod options
        public override void ChatInput(string text)
        {
            if (text[0] != '/' || text.Length==1) return;

            commandHandler.HandleCommand(text);
        }

        /// just for convenience
        internal void UpdateOption(string option, bool new_value)
        {
            ModOptions.UpdateOption(option, new_value);
        }

        // option-change callbacks
        // EDIT: this is now redundant.
        // private static void onOptionChanged(string name, bool value)
        // {
        //     ModOptions[name] = value;
        // }

        // private static void onKeyBindChanged(string name, Keys value)
        // {
        //     ActionKeys[name] = value;
        // }
    }
}
