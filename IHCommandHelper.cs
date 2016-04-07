using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Input;
using InvisibleHand.Utils;

namespace InvisibleHand
{

    public class IHCommandHelper
    {
        public readonly Mod modbase;

        public IHCommandHelper(Mod modbase)
        {
            this.modbase = modbase;
        }
    }
}
