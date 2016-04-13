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
                .Tag("mech")
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
                    ).Success)
                    item.AddTag("otherWeapon");
            }

            // equipables
            if (item.TryTag("equipable"))
            {

                bool vanity = item.TryTag("vanity");

                if (item.TagFirst("headSlot", "bodySlot", "legSlot").Success)
                {
                    item.TagIf(!vanity, "armor");
                }
                else if (item.TryTag("accessory"))
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

                    if (!item.Success)
                        ErrorLogger.Log($"Unknown accessory type for item '{_item.name}', type {_item.type}");
                }
                else
                {
                    item.TagFirst(
                        "lightPet",
                        "vanityPet",
                        "grapplingHook",
                        "mount"
                    );
                }

            }
            else
                item.TagIf(item.TagAny("pick", "axe", "hammer").Success
                        || item.TagFirst("wand", "fishingPole", "wrench").Success,
                    "tool");

            placeable = !(weapon || tool) && item.TryTag("placeable");
            if (placeable)
            {
                item.Tag("craftingStation");

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
                    item.TagFirst("container", "statue", "sink", "clock", "alphabetStatue", "tombstone",
                                    "crate", "planter", "cannon", "campfire", "fountain", "bottle", "bowl",
                                    "beachstuff", "cookingPot", "anvil", "track", "trap", "timer", "pressurePlate",
                                    "firework", "dyePlant", "seed", "ore", "bar", "gem",
                                    "monolith");

                    // item.Tag("ore")
                        // .Tag("gem");
                }
            }
            else if (item.TryTag("ammo"))
            {
                item.TagFirst("arrow", "bullet", "rocket", "dart", "sandAmmo", "solution")
                    .TagIf(!_item.consumable, "endless"); // endless quiver, musket pouch, etc
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
