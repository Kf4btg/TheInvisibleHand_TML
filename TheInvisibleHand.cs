using System;
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
        public static Dictionary<TIH, string> OriginalButtonLabels { get; private set; }

        public static readonly Dictionary<string, BoolOption> ModOptions = new Dictionary<string, BoolOption>();
        public static readonly Dictionary<string, KeyOption> ActionKeys = new Dictionary<string, KeyOption>();

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
            ActionKeys["Sort"] = new KeyOption(this, Keys.R);
            ActionKeys["Clean"] = new KeyOption(this, Keys.T);
            
            ActionKeys["DepositAll"] = new KeyOption(this, Keys.Z);
            ActionKeys["LootAll"] = new KeyOption(this, Keys.X);
            ActionKeys["QuickStack"] = new KeyOption(this, Keys.C);
            ActionKeys["SmartDeposit"] = new KeyOption(this, Keys.V);
            ActionKeys["SmartLoot"] = new KeyOption(this, Keys.B);
        }

        public override void Load()
        {
            OriginalButtonLabels = new Dictionary<TIH, string>(Constants.LangInterIndices.Count);
            // pull values out of Lang.inter to populate OBL
            foreach (var kvp in Constants.LangInterIndices)
            {
                OriginalButtonLabels[kvp.Key] = Lang.inter[kvp.Value];
            }
        }

        public void onOptionChanged(string name, bool value)
        {

        }

        public void onOptionChanged(string name, Keys value)
        {

        }
    }
}
