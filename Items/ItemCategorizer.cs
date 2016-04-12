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

    /// use this class to make the categorization checks cleaner/easier
    internal class ItemWithInfo
    {
        public Item item;
        public CategoryInfo info;

        public bool TryAddTrait(Func<Item, bool> check, string trait)
        {
            if (check(item))
            {
                info.AddTrait(trait);
                return true;
            }
            return false;
        }

        public bool TryAddTrait(bool condition, string trait)
        {
            if (condition)
            {
                info.AddTrait(trait);
                return true;
            }
            return false;
        }

        public void AddTrait(string trait)
        {
            info.AddTrait(trait);
        }
    }

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
            item.TryAddTrait(_item.questItem, "questItem");
            item.TryAddTrait(_item.expert, "expert");
            item.TryAddTrait(_item.material, "material");

            if (!item.TryAddTrait(increasedReach, "reachBoost"))
                item.TryAddTrait(decreasedReach, "reachPenalty");

            item.TryAddTrait(isBait, "bait");

            if (item.TryAddTrait(isWeapon, "weapon"))
            {
                weapon = true;
                switch(Rules.Types.Weapon(_item))
                {
                    case WeaponType.Melee:
                        item.AddTrait("melee");
                        // further categorization here
                        break;
                    case WeaponType.Ranged:
                        item.AddTrait("ranged");
                        // further categorization here
                        break;
                    case WeaponType.Magic:
                        item.AddTrait("magic");
                        // further categorization here
                        break;
                    case WeaponType.Thrown:
                        item.AddTrait("thrown");
                        // further categorization here
                        break;
                    case WeaponType.Summon:
                        item.AddTrait("summon");
                        // further categorization here
                        break;
                    case WeaponType.Other:
                        // i think this is things that just do "damage",
                        // like the flare gun?
                    default:
                        break;
                }
            }

            // equipables
            if (item.TryAddTrait(isEquipable, "equipable"))
            {
                item.TryAddTrait(_item.vanity, "vanity");

                if (item.TryAddTrait(_item.accessory, "accessory"))
                {
                    // get accessory type/slot
                }
                else if (item.TryAddTrait(isLightPet, "lightPet") ||
                    item.TryAddTrait(isVanityPet, "vanityPet") ||
                    item.TryAddTrait(isMount, "mount"))
                {
                    // something about pets?
                }
                else if (item.TryAddTrait(isHook, "grapplingHook"))
                {

                }
                else
                {
                    item.TryAddTrait(MusicBox, "musicbox");
                }



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
                item.TryAddTrait(grantsBuff, "buff");
                item.TryAddTrait(isFood, "food");
                // or possibly flask...
                item.TryAddTrait(isPotion, "potion");
            }

        }

    }
}
