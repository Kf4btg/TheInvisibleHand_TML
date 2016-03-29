using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace InvisibleHand
{
	public class IHBase : Mod
    {

        internal static int[] itemCategories;

        public override void SetModInfo(out string name, ref ModProperties properties)
		{
			name = "TheInvisibleHand";
			properties.Autoload = true;
			properties.AutoloadGores = true;
			properties.AutoloadSounds = true;
		}
	}
}
