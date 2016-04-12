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
        private HashSet<string> S_traits;

        public HashSet<string> Traits
        {
            get
            {
                if (S_traits==null)
                    S_traits = new HashSet<String>();
                return S_traits;
            }
            set
            {
                if (S_traits==null) S_traits = value;
            }
        }


        public void AddTrait(Trait t)
        {
            traits.Add(t);
        }
        public void AddTraits(params Trait[] ts)
        {
            traits.UnionWith(ts);
        }

        public bool AddTrait(string trait_name)
        {
            return Traits.Add(trait_name);
            // Trait t;
            // if (Enum.TryParse(trait_name, out t))
            // traits.Add(t);
        }

        public void AddTraits(string[] trait_names)
        {

            Traits.UnionWith(trait_names);

            // foreach (var tn in trait_names)
            // {
            //     Trait t;
            //     if (Enum.TryParse(tn, out t))
            //         traits.Add(t);
            // }
        }

        // public void addWeaponTrait(Trait t)
        // {
        //     AddTrait(t);
        //     AddTrait(Trait.weapon);
        // }
        // public void addToolTrait(Trait t)
        // {
        //     AddTrait(t);
        //     AddTrait(Trait.tool);
        // }
    }
}
