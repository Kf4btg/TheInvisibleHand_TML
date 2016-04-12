// using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
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
        // public List<string> categories = new List<string>();
        public HashSet<Trait> traits = new HashSet<Trait>();

        // some frequently needed properties
        // public ToolType tool = ToolType.None;
        // public WeaponType weapon = WeaponType.None;

        /// until we nail down exactly what traits we want to use, we'll
        /// just reference them by name. Later on we'll define enums
        public HashSet<string> S_traits = new HashSet<string>();


        public void addTrait(Trait t)
        {
            traits.Add(t);
        }
        public void addTraits(params Trait[] ts)
        {
            traits.UnionWith(ts);
        }

        public bool addTrait(string trait_name)
        {
            return S_traits.Add(trait_name);
            // Trait t;
            // if (Enum.TryParse(trait_name, out t))
            // traits.Add(t);
        }

        public void addTraits(string[] trait_names)
        {

            S_traits.UnionWith(trait_names);

            // foreach (var tn in trait_names)
            // {
            //     Trait t;
            //     if (Enum.TryParse(tn, out t))
            //         traits.Add(t);
            // }
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
