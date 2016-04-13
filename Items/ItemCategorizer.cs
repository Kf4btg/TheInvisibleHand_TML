// using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
// using System.Linq;
using Terraria;
// using Terraria.ID;
using Terraria.ModLoader;
// using InvisibleHand.Utils;

namespace InvisibleHand.Items
{
    public class CategorizedItem : GlobalItem
    {
        private static IDictionary<int, HashSet<string>> item_cache = new Dictionary<int, HashSet<string>>();

        public CategoryInfo getCategoryInfo(Item item) => (CategoryInfo)item.GetModInfo(mod, "CategoryInfo");

        public override void SetDefaults(Item item)
        {
            var cinfo = getCategoryInfo(item);

            /// check to see if we've seen this type of item before
            if (item_cache.ContainsKey(item.type))
            {
                cinfo.Traits = item_cache[item.type];
            }
            else
            {
                var iwi = new ItemWithInfo
                {
                    item = item,
                    info = getCategoryInfo(item)
                };

                classify(iwi);
                // classify(item, getCategoryInfo(item));
            }
        }

        // private void classify(Item item, CategoryInfo cinfo)
        private void classify(ItemWithInfo item)
        {
            var _item = item.item;
            bool weapon, tool, placeable;
            weapon = tool = placeable = false;

            // some generic traits to begin with
            item.Tag("questItem")
                .Tag("expert")
                .Tag("material")
                .Tag("bait")
                .Tag("defense");

            if (!item.TryTag("reachBoost"))
                item.Tag("reachPenalty");

            weapon = item.TryTag("weapon");
            if (weapon)
            {
                if (!item.TagFirst(
                    "melee",
                    "ranged",
                    "magic",
                    "summon",
                    "thrown"
                    ).LastResult)
                    item.AddTag("otherWeapon");
            }

            // equipables
            if (item.TryTag("equipable"))
            {
                item.Tag("vanity");

                if (item.TryTag("accessory"))
                {
                    item.Tag("musicbox")
                        .TagFirst(
                            "faceSlot",
                            "neckSlot",
                            "backSlot",
                            "wings",
                            "shoeSlot",
                            "handOnSlot",
                            "handOffSlot",
                            "shieldSlot",
                            "waistSlot",
                            "balloon",
                            "frontSlot"
                        );

                    if (!item.LastResult)
                        ErrorLogger.Log($"Unknown accessory type for item '{_item.name}', type {_item.type}");
                }
                else
                    item.TagFirst(
                        "lightPet",
                        "vanityPet",
                        "grapplingHook",
                        "mount"
                    );

            }
            else if (item.TagAny(
                "pick",
                "axe",
                "hammer"
                ).LastResult ||
                item.TagFirst("wand","fishingPole")
                    .LastResult)
            {
                // FIXME: also add wrenches & stuff to this category
                item.AddTag("tool");
                tool = true;

            }

            placeable = !(weapon || tool) && item.TryTag("placeable");
            if (placeable)
            {
                if (item.TryTag("housingFurniture"))
                {
                    if (item.TryTag("housingDoor"))
                    {//break down
                        item.Tag("door");
                        // TODO: platforms, tall gate,
                        // TrapdoorClosed
                    }
                    else if (item.TryTag("housingTable"))
                    {
                        item.TagFirst(
                            "table",
                            "workbench",
                            "dresser",
                            "piano",
                            "bookcase",
                            "bathtub"
                            // TODO: bewitching table, alchemy table, tinkerer's bench
                        );

                    }
                    else if (item.TryTag("housingChair"))
                    {
                        item.TagFirst( "chair", "bed", "bench"
                            // TODO: thrones
                        );
                    }
                    else if (item.TryTag("lighting"))
                    {
                        item.TagFirst(
                            "torch",
                            "candle",
                            "chandelier",
                            "hangingLantern",
                            "lamp",
                            "holidayLight",
                            "candelabra"
                            // TODO: TileID.WaterCandle,
        					// TileID.ChineseLanterns,
                            // TileID.Jackolanterns,
        					// TileID.SkullLanterns,
        					// TileID.PlatinumCandelabra,
        					// TileID.PlatinumCandle,
        					// TileID.FireflyinaBottle,
        					// TileID.LightningBuginaBottle,
        					// TileID.BlueJellyfishBowl,
        					// TileID.GreenJellyfishBowl,
        					// TileID.PinkJellyfishBowl,
        					// TileID.PeaceCandle,
        					// TileID.Fireplace
                        );
                    }
                }
                else
                {
                    item.Tag("ore")
                        .Tag("gem");
                }
            }
            else if (item.TryTag("ammo"))
            {

            }
            else if (item.TryTag("consumable"))
            {
                item.Tag("buff")
                    .TagFirst("food", "potion");
                                    // or possibly flask...
            }

        }

    }
}
