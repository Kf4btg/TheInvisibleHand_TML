// using Microsoft.Xna.Framework;
using System.Collections.Generic;
// using System;
// using Terraria;
// using Terraria.ID;
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
        public HashSet<Trait> traits = new HashSet<Trait>();

        // some frequently needed properties
        // public ToolType tool = ToolType.None;
        // public WeaponType weapon = WeaponType.None;



        public void addTrait(Trait t)
        {
            traits.Add(t);
        }

        public void addWeaponTrait(Trait t)
        {
            addTrait(t);
            addTrait(Trait.weapon);
        }
        public void addToolTrait(Trait t)
        {
            addTrait(t);
            addTrait(Trait.tool);
        }
    }
}
