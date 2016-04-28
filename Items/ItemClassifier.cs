using System.Collections.Generic;
// using System;
// using System.Linq;
using Terraria;
// using Terraria.ModLoader;

namespace InvisibleHand.Items
{
    using FlagValues = IDictionary<string, int>;
    public static class ItemClassifier
    {

        public static IDictionary<int, FlagValues> flag_cache = new Dictionary<int, FlagValues>();

        // public static void ClassifyItem(Item item, ItemFlagInfo info)
        // {
        //     classifyitem(new ItemClassificationWrapper(item, info));
        // }
        //
        public static void ClassifyItem(Item item, ItemFlagInfo flag_info = null)
        {

            if (item.type != 0)
            {
                var flaginfo = flag_info ?? item.GetFlagInfo();

                FlagValues flags;
                if (flag_cache.TryGetValue(item.type, out flags))
                {
                    flaginfo.Flags = flags;
                }
                else
                {
                    ClassifyItem(new ItemClassificationWrapper(item, flaginfo));
                    flag_cache[item.type] = flaginfo.Flags;
                }
            }


        }

        internal static void ClassifyItem(ItemClassificationWrapper item)
        {
            // var _item = item.item;
            bool _weapon, _tool;
            _weapon = _tool = false;


            // some generic or unique traits that don't really fall under other categories
            item.FlagAny("General", "quest_item",
                                    "expert",
                                    "bait",
                                    "explosive",
                                    "defense",
                                    "heal_life",
                                    "heal_mana",
                                    "boost_mana",
                                    "use_mana",
                                    "coin",
                                    "paint",
                                    "hair_dye"
            );

            // Items of all types can be materials, so run them all through this check
            if (item.TryFlag("General", "material"))
            {
                // But for items that are **primarily** used as materials, tag them here
                item.FlagAny("Material", "ore",
                                         "bar",
                                         "gem",
                                         "dye_plant",
                                         "alchemy",
                                         "soul",
                                         "critter",
                                         "butterfly"
                                        );
            }

            // various weapons, tools, equipment, and placeables can have a reach-boost
            if (!item.TryFlag("General", "reach_boost"))
                item.Flag("General", "reach_penalty");


            /////////////
            // weapons //
            /////////////

            _weapon = item.TryFlag("General", "weapon");
            if (_weapon)
            {
                item.Flag("Weapon", "has_projectile");
                if (item.FlagFirst("Weapon", "type_melee",
                                             "type_ranged",
                                             "type_magic",
                                             "type_summon",
                                             "type_throwing"
                ).Success)
                {
                    classifyWeapon(item, item.LastFlag.Key, item.LastFlag.Value);
                }
                // else
                //     item.SetFlag("Weapon", "type_other");
            }

            // equipables
            if (item.TryFlag("General", "equipable"))
            {
                bool vanity = item.TryFlag("General", "vanity");

                if (item.FlagFirst("Equip", "slot_head", "slot_body", "slot_leg").Success)
                    item.FlagIf(!vanity, "Equip", "armor");
                else if (item.TryFlag("Equip", "accessory"))
                {
                    item.Flag("Placeable", "musicbox")
                        // turns out these aren't all mutually exclusive...
                        .FlagAny("Equip", "slot_face",
                                            "slot_neck",
                                            "slot_back",
                                            "slot_wings",
                                            "slot_shoe",
                                            "slot_handon",
                                            "slot_handoff",
                                            "slot_shield",
                                            "slot_waist",
                                            "slot_balloon",
                                            "slot_front");
                }
                else
                    item.FlagFirst("Equip", "pet_light",
                                            "pet_vanity",
                                            "grapple",
                                            "mount"
                    );
            }
            else if (item.FlagAny("Tool", "pick", "axe", "hammer").Success
                    || item.FlagFirst("Tool", "wand", "fishing_pole", "wrench").Success)
            {
                _tool = item.SetFlag("General", "tool").Success;
            }


            // placeables, ammo, consumables, dyes
            if (!(_weapon | _tool))
            {
                if (item.TryFlag("General", "placeable"))
                    classifyPlaceable(item);

                else if (item.TryFlag("General", "ammo"))
                    classifyAmmo(item);

                else if (item.TryFlag("General", "consumable"))
                    classifyConsumable(item);

                else if (item.TryFlag("General", "dye"))
                    item.FlagFirst("Dye", "basic",
                                          "black",
                                          "flame",
                                          "gradient",
                                          "strange",
                                          "bright",
                                          "silver",
                                          "lunar");
            }

            if (item.TryFlag("General", "mech"))
                item.FlagFirst("Mech", "trap", "pressure_plate", "timer", "firework", "track");
        }

        private static void classifyAmmo(ItemClassificationWrapper item)
        {
            item.FlagFirst("Ammo", "arrow",
                                   "bullet",
                                   "rocket",
                                   "dart",
                                   "sand",
                                   "solution")
                .FlagIf(!item.item.consumable, "Ammo", "endless"); // endless quiver, musket pouch, etc
        }

        // FIXME: doesn't seem to work
        private static void classifyConsumable(ItemClassificationWrapper item)
        {
            item.Flag("Consumable", "buff")
                .FlagFirst("Consumable", "food", "flask", "potion");
        }

        /// <summary>
        /// Use to add traits relating to furniture, block-type, tile properties, etc.
        /// </summary>
        /// <param name="item"> </param>
        private static void classifyPlaceable(ItemClassificationWrapper item)
        {
            // flag blocks, walls, misc
            // TODO: separate bricks from blocks

            item.FlagAny("Placeable", "lighted", "metal_detector");
            if (item.TryFlag("Placeable", "block"))
            {
                item.FlagAny("Placeable.Block", "bouncy",
                                                // "brick",
                                                "dungeon_brick",
                                                "hallow",
                                                "crimson",
                                                "corrupt",
                                                "wood",
                                                "sand",
                                                "hardened_sand",
                                                "sandstone",
                                                "ice");
                return;
            }
            else if (item.FlagFirst("Placeable", "wall",
                                       "banner",
                                       "seed",
                                       "strange_plant",
                                       "track",
                                       "rope",
                                       "rope_coil").Success) return;

            // if (item.Flag("Placeable", "block").FlagFirst("Placeable", "wall",
            //                                                            "banner",
            //                                                            "seed",
            //                                                            "strange_plant",
            //                                                            "track",
            //                                                            "rope",
            //                                                            "rope_coil"
            // ).Success)
            //     return;

            // Wall-hangables
            if (item.TryFlag("Placeable", "wall_deco"))
            {
                switch (ClassificationRules.Types.WallDeco(item.item))
                {
                    // TODO: these flag values should probably be somewhere else,
                    // like maybe the furniture enum, or even a new enum for decorations
                    case WallDecoType.Painting:
                        item.SetFlag("Placeable", "painting");
                        break;
                    case WallDecoType.Trophy:
                        item.SetFlag("Placeable", "trophy");
                        break;
                    case WallDecoType.Rack:
                        item.SetFlag("Placeable", "rack");
                        break;
                }
                return;
            }

            // track whether this fits any of the furniture traits
            bool is_furniture = item.FlagAny("Furniture", "crafting_station", "container").Success;

            if (item.TryFlag("Furniture",  "valid_housing"))
            {
                is_furniture = true;
                //break down general->specific
                if (item.TryFlag("Furniture", "housing_door"))
                    item.Flag("Furniture.Doors", "door");
                    // TODO: platforms, tall gate, TrapdoorClosed
                else if (item.TryFlag("Furniture", "housing_table"))
                    item.FlagFirst("Furniture.Tables", "table",
                                                       "workbench",
                                                       "dresser",
                                                       "piano",
                                                       "bookcase",
                                                       "bathtub"
                        // TODO: bewitching table, alchemy table, tinkerer's bench
                    );

                else if (item.TryFlag("Furniture", "housing_chair"))
                    item.FlagFirst("Furniture.Chairs", "chair",
                                                       "bed",
                                                       "bench"
                       // TODO: thrones
                    );
                else if (item.TryFlag("Furniture", "housing_light"))
                    item.FlagFirst("Furniture.Lighting", "torch",
                                                         "candle",
                                                         "chandelier",
                                                         "hanging_lantern",
                                                         "lamp",
                                                         "holiday_light",
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
            else
                is_furniture |= item.FlagFirst("Furniture.Other", "statue",
                                                                  "sink",
                                                                  "clock",
                                                                  "statue_alphabet",
                                                                  "tombstone",
                                                                  "crate",
                                                                  "planter",
                                                                  "cannon",
                                                                  "campfire",
                                                                  "fountain",
                                                                  "bottle",
                                                                  "bowl",
                                                                  "beachstuff",
                                                                  "cooking_pot",
                                                                  "anvil",
                                                                  "monolith").Success;

            item.FlagIf(is_furniture, "Placeable", "furniture");
        }

        /// <summary>
        /// Examine and assign traits for the weapon represented by `item`
        /// </summary>
        /// <param name="item"> </param>
        /// <param name="type"> should only be "Weapon" (sanity check)</param>
        /// <param name="flag"> should be the type of the weapon (e.g. "type_melee")</param>
        private static void classifyWeapon(ItemClassificationWrapper item, string type, string flag)
        {
            if (type != "Weapon") return;

            string weaponType;
            switch (flag)
            {
                case "type_melee":
                    weaponType = "Weapon.Melee";

                    if (item.TryFlag(weaponType, "style_directional"))
                        item.FlagFirst(weaponType, "flail", "yoyo", "chain");

                    else if (item.TryFlag(weaponType, "style_swing"))
                        item.Flag(weaponType, "broadsword");

                    else if (item.TryFlag(weaponType, "style_thrown"))
                        item.Flag(weaponType, "boomerang");

                    // the 'chain' weapons can be various styles, but they all
                    // have the same item.shoot projectileAI value
                    item.Flag(weaponType, "chain");

                    break;

                case "type_ranged":
                    weaponType = "Weapon.Ranged";
                    item.FlagFirst(weaponType, "arrow_consuming",
                                               "bullet_consuming",
                                               "rocket_consuming",
                                               "dart_consuming",
                                               "flamethrower",
                                               "gel_consuming",
                                               "no_ammo");
                    break;

                case "type_magic":
                    weaponType = "Weapon.Magic";
                    item.FlagFirst(weaponType, "area",
                                               "homing",
                                               "controlled",
                                               "bouncing",
                                               "stream");
                    break;
                case "type_summon":
                    weaponType = "Weapon.Summon";
                    if (!item.TryFlag(weaponType, "minion"))
                        item.SetFlag(weaponType, "sentry");

                    break;
                case "type_throwing":
                    // (there are no sub-categories for thrown weapons yet,
                    // so there's no Flag Type or extra condititions for them either)
                    // break;
                default:
                    // item.Flag("Weapon", "type_other");
                    break;
            }
        }
    }
}
