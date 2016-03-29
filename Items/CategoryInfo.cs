// using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InvisibleHand.Items
{



    /// store the information about an item's category here.
    public class CategoryInfo : ItemInfo
    {
        // public List<int> categories = new List<int>();
        //
        // TODO: create enums or use ints for the categories
        public List<string> categories = new List<string>();

        // some frequently needed properties
        public ToolType tool = ToolType.None;
        public WeaponType weapon = WeaponType.None;
        

    }
}
