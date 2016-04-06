using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;

namespace InvisibleHand
{
	public class IHBase : Mod
    {

        internal static int[] itemCategories;

        /// holds the game's original strings for loot-all, dep-all, quick-stack, etc;
        /// we're going to be removing these later on, but will use their
        /// original values to replace them with newer, better buttons.
        public static Dictionary<TIH, string> OriginalButtonLabels { get; private set; }


		public static Dictionary<string, BoolOption> ModOptions { get; private set; }


        public override string Name {
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
