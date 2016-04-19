using System.Collections.Generic;
// using System;
// using System.Linq;
using Terraria;
// using Terraria.ID;
using Terraria.ModLoader;
// using InvisibleHand.Utils;

namespace InvisibleHand.Items
{
    public class ClassifiedItem : GlobalItem
    {
        private static IDictionary<int, FlagInfo> item_cache = new Dictionary<int, FlagInfo>();
        public CategoryInfo getCategoryInfo(Item item) => (CategoryInfo)item.GetModInfo(mod, "CategoryInfo");


        public static IDictionary<int, IDictionary<string, int>> flag_cache = new Dictionary<int, IDictionary<string, int>>();
        public ItemFlagInfo getFlagInfo(Item item) => (ItemFlagInfo)item.GetModInfo(mod, "ItemFlagInfo");


        // public override void SetDefaults(Item item)
        // {
        //     var cinfo = getCategoryInfo(item);
        //
        //     /// check to see if we've seen this type of item before
        //     if (item_cache.ContainsKey(item.type))
        //     {
        //         cinfo.Flags = item_cache[item.type];
        //     }
        //     else
        //     {
        //         // cinfo.Traits = ItemCategorizer.Classify(item);
        //         ItemCategorizer.ClassifyItem(item, cinfo);
        //         // classify(item, getCategoryInfo(item));
        //     }
        // }
        //
        public override void SetDefaults(Item item)
        {
            var finfo = getFlagInfo(item);

            if (flag_cache.ContainsKey(item.type))
            {
                finfo.Flags = flag_cache[item.type];
            }
            else
            {
                ItemClassifier.ClassifyItem(item, finfo);
                flag_cache[item.type] = finfo.Flags;
            }
        }
    }
}
