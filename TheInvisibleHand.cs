using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Input;

namespace InvisibleHand
{
    public class IHBase : Mod
    {

        internal static int[] itemCategories;

        /// holds the game's original strings for loot-all, dep-all, quick-stack, etc;
        /// we're going to be removing these later on, but will use their
        /// original values to replace them with newer, better buttons.
        public static Dictionary<TIH, string> OriginalButtonLabels { get; private set; }

        public static readonly Dictionary<string, BoolOption> ModOptions = new Dictionary<string, BoolOption>();
        public static readonly Dictionary<string, KeyOption> HotKeys = new Dictionary<string, KeyOption>();

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

            // default options
            ModOptions["UseReplacers"] = new BoolOption(this, true);

            // default hotkeys
            HotKeys["DepositAll"] = new KeyOption(this, Keys.Z);
            HotKeys["LootAll"] = new KeyOption(this, Keys.X);
            HotKeys["QuickStack"] = new KeyOption(this, Keys.C);
            HotKeys["SmartDeposit"] = new KeyOption(this, Keys.V);
            HotKeys["SmartLoot"] = new KeyOption(this, Keys.B);
        }

        public override void Load()
        {
            OriginalButtonLabels = new Dictionary<TIH, string>(6);
            // pull values out of Lang.inter to populate OBL
            foreach (var kvp in Constants.LangInterIndices)
            {
                OriginalButtonLabels[kvp.Key] = Lang.inter[kvp.Value];
            }
        }
    }
}
