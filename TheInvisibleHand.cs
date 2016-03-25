using System;
using Terraria.ModLoader;

namespace InvisibleHand
{
	public class IHBase : Mod
	{
		public override void SetModInfo(out string name, ref ModProperties properties)
		{
			name = "TheInvisibleHand";
			properties.Autoload = true;
			properties.AutoloadGores = true;
			properties.AutoloadSounds = true;
		}
	}
}
