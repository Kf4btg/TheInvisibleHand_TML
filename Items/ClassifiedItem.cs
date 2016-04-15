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

        public override void SetDefaults(Item item)
        {
            var cinfo = getCategoryInfo(item);

            /// check to see if we've seen this type of item before
            if (item_cache.ContainsKey(item.type))
            {
                cinfo.Flags = item_cache[item.type];
            }
            else
            {
                // cinfo.Traits = ItemCategorizer.Classify(item);
                ItemCategorizer.ClassifyItem(item, cinfo);
                // classify(item, getCategoryInfo(item));
            }
        }
    }
}
