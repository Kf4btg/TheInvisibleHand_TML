using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace InvisibleHand.Items
{
    public class ClassifiedItem : GlobalItem
    {

        public static IDictionary<int, IDictionary<string, int>> flag_cache = new Dictionary<int, IDictionary<string, int>>();
        public ItemFlagInfo getFlagInfo(Item item) => (ItemFlagInfo)item.GetModInfo(mod, "ItemFlagInfo");

        public override void SetDefaults(Item item)
        {
            var finfo = getFlagInfo(item);

            if (flag_cache.ContainsKey(item.type))
            {
                finfo.Flags = flag_cache[item.type];
            }
            else
            {
                ItemClassifier.ClassifyItem(new ItemClassificationWrapper(item, finfo));
                flag_cache[item.type] = finfo.Flags;
            }
        }
    }
}
