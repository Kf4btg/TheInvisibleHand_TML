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
    // using BRules=InvisibleHand.Items.Rules.Binary;
    //
    using static Rules.Binary;
    using static Rules.TileIDGroups;
    using static Rules.Groupings;

    public class CategorizedItem : GlobalItem
    {
        internal static Func<bool, string, Tuple<bool, string>> tup = Tuple.Create;
        internal static Func<Func<Item, bool>, string, Tuple<Func<Item, bool>, string>> ftup = Tuple.Create;


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
            item.Tag(_item.questItem, "questItem")
                .Tag(_item.expert, "expert")
                .Tag(_item.material, "material")
                .Tag(isBait, "bait")
                .Tag(givesDefense, "defense");

            if (!item.TryTag(increasedReach, "reachBoost"))
                item.Tag(decreasedReach, "reachPenalty");

            weapon = item.TryTag(isWeapon, "weapon");
            if (weapon)
            {
                if (!item.TagFirst(
                    tup(_item.melee,  "melee"),
                    tup(_item.ranged, "ranged"),
                    tup(_item.magic,  "magic"),
                    tup(_item.summon, "summon"),
                    tup(_item.thrown, "thrown")
                    ).LastResult)
                    item.Tag("otherWeapon");
            }

            // equipables
            if (item.TryTag(isEquipable, "equipable"))
            {
                item.Tag(_item.vanity, "vanity");

                if (item.TryTag(_item.accessory, "accessory"))
                {
                    item.Tag(MusicBox, "musicbox")
                        .TagFirst(
                            ftup(i => i.faceSlot > 0, "faceSlot"),
                            ftup(i => i.neckSlot > 0, "neckSlot"),
                            ftup(i => i.backSlot > 0, "backSlot"),
                            ftup(i => i.wingSlot > 0, "wings"),
                            ftup(i => i.shoeSlot > 0, "shoeSlot"),
                            ftup(i => i.handOnSlot > 0, "handOnSlot"),
                            ftup(i => i.handOffSlot > 0, "handOffSlot"),
                            ftup(i => i.shieldSlot > 0, "shieldSlot"),
                            ftup(i => i.waistSlot > 0, "waistSlot"),
                            ftup(i => i.balloonSlot > 0, "balloon"),
                            ftup(i => i.frontSlot > 0, "frontSlot")
                        );

                    if (!item.LastResult)
                        ErrorLogger.Log($"Unknown accessory type for item '{_item.name}', type {_item.type}");
                }
                else
                    item.TagFirst(
                        ftup(isLightPet, "lightPet"),
                        ftup(isVanityPet, "vanityPet"),
                        ftup(isHook, "grapplingHook"),
                        ftup(isMount, "mount")
                    );

            }
            else if (item.TagAny(
                ftup(isPick, "pick"),
                ftup(isAxe, "axe"),
                ftup(isHammer, "hammer")
                ).LastResult ||
                item.TagFirst(
                    ftup(isWand, "wand"),
                    ftup(isFishingPole, "fishingPole"))
                    .LastResult)
            {
                // FIXME: also add wrenches & stuff to this category
                item.Tag("tool");
                tool = true;

            }

            placeable = !(weapon || tool) && item.TryTag(CanBePlaced, "placeable");
            if (placeable)
            {
                if (item.TryTag(Furniture, "housingFurniture"))
                {
                    if (item.TryTag(housingDoor, "housingDoor"))
                    {//break down
                        item.Tag(Door, "door");
                        // TODO: platforms, tall gate,
                        // TrapdoorClosed
                    }
                    else if (item.TryTag(housingTable, "housingTable"))
                    {
                        item.TagFirst(
                            ftup(Table, "table"),
                            ftup(WorkBench, "workbench"),
                            ftup(Dresser, "dresser"),
                            ftup(Piano, "piano"),
                            ftup(Bookcase, "bookcase"),
                            ftup(Bathtub, "bathtub")
                            // TODO: bewitching table, alchemy table, tinkerer's bench
                        );

                    }
                    else if (item.TryAddTrait(housingChair, "housingChair"))
                    {
                        item.TagFirst(
                            ftup(Chair, "chair"),
                            ftup(Bed, "bed"),
                            ftup(Bench, "bench")
                            // TODO: thrones
                        );
                    }
                    else if (item.TryAddTrait(housingTorch, "lighting"))
                    {
                        item.TagFirst(
                            ftup(Torch, "torch"),
                            ftup(Candle, "candle"),
                            ftup(Chandelier, "chandelier"),
                            ftup(HangingLantern, "hangingLantern"),
                            ftup(Lamp, "lamp"),
                            ftup(HolidayLight, "holidayLight"),
                            ftup(Candelabra, "candelabra")
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
                    item.Tag(Ore, "ore")
                        .Tag(Gem, "gem");
                }
            }
            else if (item.TryAddTrait(isAmmo, "ammo"))
            {

            }
            else if (item.TryAddTrait(isConsumable, "consumable"))
            {
                item.TryAddTrait(timedBuff, "buff");
                item.TryAddTrait(isFood, "food");
                // or possibly flask...
                item.TryAddTrait(isPotion, "potion");
            }

        }

    }
}
