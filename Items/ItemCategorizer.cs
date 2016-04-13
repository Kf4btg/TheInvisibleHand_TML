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
    using rtypes = Rules.Types;

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

            // item.TryAddTrait(isBait, "bait");
            weapon = item.TryTag(isWeapon, "weapon");
            if (weapon)
            {
                if (!item.TagFirst(
                    tup(_item.melee, "melee"),
                    tup(_item.ranged, "ranged"),
                    tup(_item.magic, "magic"),
                    tup(_item.summon, "summon"),
                    tup(_item.thrown, "thrown")
                    ).LastResult)
                    item.Tag("otherWeapon");

                // foreach (var weapcheck in new[]
                // {
                //     new {check=_item.melee, trait="melee"},
                //     new {check=_item.ranged, trait="ranged"},
                //     new {check=_item.magic, trait="magic"},
                //     new {check=_item.summon, trait="summon"},
                //     new {check=_item.thrown, trait="thrown"},
                // })
                //     if (item.TryTag(weapcheck.check, weapcheck.trait))
                //         break;

                // switch(Rules.Types.Weapon(_item))
                // {
                //     case WeaponType.Melee:
                //         item.AddTrait("melee");
                //         // further categorization here
                //         break;
                //     case WeaponType.Ranged:
                //         item.AddTrait("ranged");
                //         // further categorization here
                //         break;
                //     case WeaponType.Magic:
                //         item.AddTrait("magic");
                //         // further categorization here
                //         break;
                //     case WeaponType.Thrown:
                //         item.AddTrait("thrown");
                //         // further categorization here
                //         break;
                //     case WeaponType.Summon:
                //         item.AddTrait("summon");
                //         // further categorization here
                //         break;
                //     case WeaponType.Other:
                //         // i think this is things that just do "damage",
                //         // like the flare gun?
                //     default:
                //         break;
                // }
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
                            ftup(i => i.balloonSlot > 0, "balloonSlot"),
                            ftup(i => i.frontSlot > 0, "frontSlot")
                        );

                    if (!item.LastResult)
                        ErrorLogger.Log($"Unknown accessory type for item '{_item.name}', type {_item.type}");

                    // var slots = new[] { _item.faceSlot, _item.neckSlot, _item.backSlot, _item.wingSlot, _item.shoeSlot, _item.handOnSlot, _item.handOffSlot, _item.shieldSlot, _item.waistSlot, _item.balloonSlot, _item.frontSlot, };

                    // foreach (var slot in new[]{
                    //     "faceSlot", "neckSlot",
                    //     "wingSlot", "shoeSlot",
                    //     "handOnSlot", "handOffSlot",
                    //     "shieldSlot", "waistSlot",
                    //     "balloonSlot", "frontSlot"
                    //     }) {
                    //     if (item.TryTag(rtypes.AccySlot(_item, slot), slot)) break;
                    // }



                }
                else
                    foreach (var equipcheck in new Dictionary<string, Func<Item, bool>>
                    {
                        // {"accessory", (i) => i.accessory},
                        {"lightPet", isLightPet},
                        {"vanityPet", isVanityPet},
                        {"grapplingHook", isHook},
                        {"mount", isMount},
                    })
                        if (item.TryTag(equipcheck.Value, equipcheck.Key))
                            break;

                // if (item.TryTag(_item.accessory, "accessory"))
                // {
                //     // get accessory type/slot
                // }
                // else if (item.TryTag(isLightPet, "lightPet") ||
                //     item.TryTag(isVanityPet, "vanityPet") ||
                //     item.TryTag(isMount, "mount"))
                // {
                //     // something about pets?
                // }
                // else if (item.TryTag(isHook, "grapplingHook"))
                // {
                //
                // }
                // else
                // {
                //     item.Tag(MusicBox, "musicbox");
                // }



                // get equip type
                //
                // get armor type/slot
                //
            }
            else
            {
                tool |= item.TryAddTrait(isPick, "pick");
                tool |= item.TryAddTrait(isAxe, "axe");
                tool |= item.TryAddTrait(isHammer, "hammer");

                if (!tool) tool |= item.TryAddTrait(isWand, "wand");
                if (!tool) tool |= item.TryAddTrait(isFishingPole, "fishingPole");

                if (tool)
                    item.AddTrait("tool");
            }

            if (!(weapon || tool) && item.TryAddTrait(CanBePlaced, "placeable"))
            {
                placeable = true;

                if (item.TryAddTrait(isFurniture, "housingFurniture"))
                {
                    if (item.TryAddTrait(housingDoor, "housingDoor"))
                    {//break down
                        item.TryAddTrait(Door, "door");
                    }
                    else if (item.TryAddTrait(housingTable, "housingTable"))
                    {
                        if (item.TryAddTrait(Table, "table")
                        ||item.TryAddTrait(WorkBench, "workbench")
                        || item.TryAddTrait(Dresser, "dresser")
                        ||item.TryAddTrait(Piano, "piano")
                        ||item.TryAddTrait(Bookcase, "bookcase"))
                        {}
                    }
                    else if (item.TryAddTrait(housingChair, "housingChair"))
                    {
                        if (item.TryAddTrait(Chair, "chair")
                        ||item.TryAddTrait(Bed, "bed")
                        ||item.TryAddTrait(Bench, "bench")
                        )
                        {}
                    }
                    else if (item.TryAddTrait(housingTorch, "lighting"))
                    {}
                }
                else if (item.TryAddTrait(isOre, "ore"))
                {
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
