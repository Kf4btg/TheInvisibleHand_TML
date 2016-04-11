// using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
// using TAPI;
using Terraria;
// using Terraria.ID;
using Terraria.ModLoader;

namespace InvisibleHand.Items
{
    public class CategorizedItem : GlobalItem
    {
        public CategoryInfo getCategoryInfo(Item item) => (CategoryInfo)item.GetModInfo(mod, "CategoryInfo");

        public override void SetDefaults(Item item)
        {
            // var cinfo = getCategoryInfo(item);

            classify(item, getCategoryInfo(item));

            // if (item.placeStyle > 0)
            // {
            //     // placeable items, eg furniture, banners, paintings, etc. May also include some accessories
            //     cinfo.categories.Add("placeable");
            // }
            //
            // else if (item.melee)
            // {
            //     cinfo.categories.Add("weapon");
            //
            //     if (item.pick>0 || item.axe>0 || item.hammer>0)
            //     {
            //         cinfo.categories.Add("tool");
            //     }
            //
            // }
        }

        private void classify(Item item, CategoryInfo cinfo)
        {
            var clist = cinfo.categories;
            // var traits = cinfo.traits;

            // some generic traits
            if (item.channel) cinfo.addTrait(Trait.auto);
            if (item.consumable) cinfo.addTrait(Trait.consumable);

            if (item.placeStyle > 0)
            {
                // placeable items, eg furniture, banners, paintings, etc. May also include some accessories
                // clist.Add("placeable");
                cinfo.addTrait(Trait.placeable);
            }

            else if (item.melee)
            {
                cinfo.addWeaponTrait(Trait.melee);

                // cinfo.weapon |= WeaponTypes.Melee;

                // clist.Add("weapon");
                // clist.Add("melee");

                // bitmask for determining tooltype
                // pick, axe, hammer, channel (automatic, ie drill, chainsaw, etc.)

                // ToolTypes tooltype = ToolTypes.None;

                if (item.pick > 0) cinfo.addToolTrait(Trait.pick);

                if (item.axe > 0) cinfo.addToolTrait(Trait.axe);

                if (item.hammer > 0) cinfo.addToolTrait(Trait.hammer);

            }
            else if (item.ranged) // could also include ammo!
            {
                if (item.ammo > 0)
                {
                    if (!item.notAmmo)
                        cinfo.addTrait(Trait.ammo);
                }
                else
                    cinfo.addWeaponTrait(Trait.ranged);

            }
            else if (item.magic)
            {
                cinfo.addWeaponTrait(Trait.magic);
                // cinfo.weapon |= WeaponType.Magic;

                // further categorization is likely to rely on examination of the projectile for the item
                //
                // vilethorn-types have aistyle==4, tileCollide=False, IgnoreWater=True
            }
            else if (item.thrown)
            {
                cinfo.addWeaponTrait(Trait.thrown);
                // things like the bone glove will be non-consumable,
                // as well as probably some modded items
                // if (item.consumable) clist.Add("consumable");

            }
        }


        public static IDictionary<Trait, Func<Item, bool>> MatchingRules = new Dictionary<Trait, Func<Item, bool>>()
        {
            { Trait.weapon, item => false}
        };

    }


    // public class ItemCategory
    // {
    //
    //     public string CategoryName { get; private set; }
    //
    //     private Func<Item, bool> matchCondition;
    //
    //     private IDictionary<string, ItemCategory> subCategories = new Dictionary<string, ItemCategory>();
    //
    //
    //     public ItemCategory(string name, Func<Item, bool> condition)
    //     {
    //         this.CategoryName = name;
    //         this.matchCondition = condition;
    //     }
    //
    //     /// Returns itself so that these calls can be chained if need be.
    //     public ItemCategory addSubCategory(ItemCategory sub)
    //     {
    //         this.subCategories.Add(sub.CategoryName, sub);
    //
    //         return this;
    //     }
    //
    //     public bool CategorizeItem(Item item)
    //     {
    //         return true;
    //     }
    // }

}
