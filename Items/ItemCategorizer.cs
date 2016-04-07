// using Microsoft.Xna.Framework;
// using System.Collections.Generic;
// using System;
// using TAPI;
using Terraria;
// using Terraria.ID;
using Terraria.ModLoader;

namespace InvisibleHand.Items
{
    public class ItemTweaker : GlobalItem
    {
        public CategoryInfo getCategoryInfo(Item item) => (CategoryInfo)item.GetModInfo(mod, "CategoryInfo");

        public override void SetDefaults(Item item)
        {
            // var cinfo = getCategoryInfo(item);

            categorize(item, getCategoryInfo(item));

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

        private void categorize(Item item, CategoryInfo cinfo)
        {
            var clist = cinfo.categories;
            if (item.placeStyle > 0)
            {
                // placeable items, eg furniture, banners, paintings, etc. May also include some accessories
                clist.Add("placeable");
            }

            else if (item.melee)
            {
                cinfo.weapon |= WeaponType.Melee;

                clist.Add("weapon");
                clist.Add("melee");

                // bitmask for determining tooltype
                // pick, axe, hammer, channel (automatic, ie drill, chainsaw, etc.)

                ToolType tooltype = ToolType.None;

                if (item.pick > 0)   tooltype |= ToolType.Pick;
                if (item.axe > 0)    tooltype |= ToolType.Axe;
                if (item.hammer > 0) tooltype |= ToolType.Hammer;

                if (!tooltype.Equals(ToolType.None))
                {
                    clist.Add("tool");

                    if (item.channel) tooltype |= ToolType.Auto;

                    // set the value on the info object
                    cinfo.tool = tooltype;
                }
            }
            else if (item.ranged) // could also include ammo!
            {

            }
            else if (item.magic)
            {
                cinfo.weapon |= WeaponType.Magic;

                // further categorization is likely to rely on examination of the projectile for the item
                //
                // vilethorn-types have aistyle==4, tileCollide=False, IgnoreWater=True
            }
            else if (item.thrown)
            {
                cinfo.weapon |= WeaponType.Thrown;
                // things like the bone glove will be non-consumable,
                // as well as probably some modded items
                if (item.consumable) clist.Add("consumable");

            }


        }

        private void getToolType(Item item, ref CategoryInfo cinfo)
        {

        }
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
