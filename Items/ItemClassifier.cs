using System.Collections.Generic;
// using System;
using Terraria;

namespace InvisibleHand.Items
{
    using static ICWFluent;
    using FlagValues = IDictionary<string, int>;
    public static class ItemClassifier
    {

        /// stash known flag results here, keyed by ItemID; this way only one item of
        /// a given type need be classified per game session.
        public static IDictionary<int, FlagValues> flag_cache = new Dictionary<int, FlagValues>();

        /// analyze the given item and assign flag values based on its traits. These flags
        /// will later be used to determine the proper category for this item.
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
                    ClassifyItem(new ClassificationWrapper(item, flaginfo));
                    flag_cache[item.type] = flaginfo.Flags;
                }
            }
        }

        internal static void ClassifyItem(ClassificationWrapper item)
        {
            bool _weapon, _tool;
            _weapon = _tool = false;

            // some generic or unique traits that don't really fall under other categories
            item.FlagAny("General", "quest_item",
                                    "expert",
                                    "bait",
                                    "explosive",
                                    "defense",
                                    "boost_mana",
                                    "use_mana",
                                    "coin",
                                    "paint",
                                    "hair_dye",
                                    "material_notcategory"
            );

            // Items of all types can be materials, so run them all through this check
            if (item.Try(Flag, "General", "material"))
            {
                // But for items that are **primarily** used as materials, tag them here
                item.FlagAny("Material", "ore",
                                         "bar",
                                         "gem",
                                         "dye_plant",
                                         "alchemy",
                                         "soul",
                                         "critter",
                                         "butterfly")
                    .If(item.Success, SetFlag, "Material", "material_primary");
                    // if any of those materials were flagged, mark the item as primarily a material
            }

            // various weapons, tools, equipment, and placeables can have a reach-boost
            if (!item.Try(Flag,"General", "reach_boost"))
                item.Flag("General", "reach_penalty");

            //
            // weapons
            //------------
            _weapon = item.Try(Flag, "General", "weapon");
            if (_weapon)
            {
                item.Flag("Weapon", "has_projectile")
                    .FlagFirst("Weapon", "type_melee",
                                             "type_ranged",
                                             "type_magic",
                                             "type_summon",
                                             "type_throwing");
                if (item.Success)
                    classifyWeapon(item, item.LastFlag.Key, item.LastFlag.Value);
                // else
                //     item.SetFlag("Weapon", "type_other");
            }

            // equipables
            //------------
            if (item.Try(Flag, "General", "equipable"))
            {
                // first, tag vanity status
                bool vanity = item.Try(Flag, "General", "vanity");

                // then the armor slots
                if (item.Try(FlagFirst, "Equip", "slot_head", "slot_body", "slot_leg"))
                    item.If(!vanity, SetFlag, "Equip", "armor");
                else if (item.Try(Flag, "Equip", "accessory"))
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
                else item.FlagFirst("Equip", "pet_light",
                                            "pet_vanity",
                                            "grapple",
                                            "mount"
                    );
            }
            //Tools
            //------------
            else
            {
                // is this too dense?
                // anyway:
                // IF the item is [pick/axe/hammer]
                // OR
                // the item is a (wand|fishingpole|wrench|bucket|painting-tool)
                // OR
                // the item is an Exploration aid (in which case it shall be marked as such)
                // THEN
                // flag the item as a Tool.
                _tool = item.TryIf(
                            item.Try(FlagAny, "Tool", "pick", "axe", "hammer")
                            ||
                            item.Try(FlagFirst, "Tool", "wand", "fishing_pole", "wrench", "bucket", "painting")
                            ||
                            item.TryIf(
                                item.Try(FlagFirst, "Tool.Exploration", "hand_light",
                                                               "rope",
                                                               "rope_coil",
                                                               "demolitions",
                                                               "survival",
                                                               "recall"
                                ), SetFlag, "Tool", "exploration_aid"
                        ), SetFlag, "General", "tool");

                // if (!_tool &&
                //     item.Try(FlagFirst, "Supplies", "hand_light",
                //     "rope",
                //     "rope_coil",
                //     "bucket",
                //     "demolitions",
                //     "exploration",
                //     "recall"
                //     )
                // {
                //     // like "Tool", there's not much these items have in common
                //     // other than the abstraction of their purpose, so their parent
                //     // is created after they are, only if they exist. I'm sure
                //     // there's some sort of Grand Lesson in that.
                //     item.SetFlag("General", "supplies");
                //
                // }
            }



            // placeables, ammo, consumables, dyes
            if (!(_weapon || _tool))
            {
                if (item.Try(Flag, "General", "placeable"))
                    classifyPlaceable(item);

                // else if (item.Try(Flag,"General", "ammo"))
                else if (item.Try(Flag, "General", "ammo"))
                    classifyAmmo(item);

                    // else if (item.Try(Flag,"General", "consumable"))
                else if (item.Try(Flag, "General", "consumable"))
                    classifyConsumable(item);

                else if (item.Try(Flag, "General", "dye"))
                    item.FlagFirst("Dye", "basic",
                                          "black",
                                          "flame",
                                          "gradient",
                                          "strange",
                                          "bright",
                                          "silver",
                                          "lunar");
            }

            item.If(item.Try(Flag, "General", "mech"),
                FlagFirst, "Mech", "trap",
                                   "pressure_plate",
                                   "timer",
                                   "firework",
                                   "track");
        }

        private static void classifyAmmo(ClassificationWrapper item)
        {
            item.FlagFirst("Ammo", "arrow",
                                   "bullet",
                                   "rocket",
                                   "dart",
                                   "sand",
                                   "solution")
                .If(!item.item.consumable, SetFlag, "Ammo", "endless"); // endless quiver, musket pouch, etc
        }

        private static void classifyConsumable(ClassificationWrapper item)
        {
            item.Flag("Consumable", "buff")
                .FlagFirst("Consumable", "food", "flask", "potion")
                // only flag the healing properties if the thing is a potion
                .If(item.LastFlag.Value == "potion", FlagAny, "Consumable", "heal_life", "heal_mana");
        }

        /// <summary>
        /// Use to add traits relating to furniture, block-type, tile properties, etc.
        /// </summary>
        /// <param name="item"> </param>
        private static void classifyPlaceable(ClassificationWrapper item)
        {
            // flag blocks, walls, misc
            // TODO: separate bricks from blocks

            item.FlagAny("Placeable", "lighted", "metal_detector");
            if (item.Try(Flag, "Placeable", "block"))
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
            else if (item.Try(FlagFirst, "Placeable", "wall",
                                       "banner",
                                       "seed",
                                       "strange_plant",
                                       "track",
                                       "rope",
                                       "rope_coil")) return;


            // Wall-hangables
            if (item.Try(Flag, "Placeable", "wall_deco"))
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
            bool is_furniture = item.Try(FlagAny, "Furniture", "crafting_station", "container");

            if (item.Try(Flag,"Furniture",  "valid_housing"))
            {
                is_furniture = true;
                //break down general->specific

                if (item.Try(Flag, "Furniture", "housing_door"))
                    item.Flag("Furniture.Doors", "door");
                    // TODO: platforms, tall gate, TrapdoorClosed

                else if (item.Try(Flag, "Furniture", "housing_table"))
                    item.FlagFirst("Furniture.Tables", "table",
                                                       "workbench",
                                                       "dresser",
                                                       "piano",
                                                       "bookcase",
                                                       "bathtub"
                        // TODO: bewitching table, alchemy table, tinkerer's bench
                    );

                else if (item.Try(Flag, "Furniture", "housing_chair"))
                    item.FlagFirst("Furniture.Chairs", "chair",
                                                       "bed",
                                                       "bench"
                       // TODO: thrones
                    );
                else if (item.Try(Flag, "Furniture", "housing_light"))
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
                is_furniture |= item.Try(FlagFirst, "Furniture.Other", "statue",
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
                                                                  "monolith");

            item.If(is_furniture, SetFlag, "Placeable", "furniture");
        }

        /// <summary>
        /// Examine and assign traits for the weapon represented by `item`
        /// </summary>
        /// <param name="item"> </param>
        /// <param name="type"> should only be "Weapon" (sanity check)</param>
        /// <param name="flag"> should be the type of the weapon (e.g. "type_melee")</param>
        private static void classifyWeapon(ClassificationWrapper item, string type, string flag)
        {
            if (type != "Weapon") return;

            string weaponType;
            switch (flag)
            {
                case "type_melee":
                    weaponType = "Weapon.Melee";

                    if (item.Try(Flag, weaponType, "style_directional"))
                    {
                        if (item.Try(FlagFirst, weaponType, "flail", "yoyo", "spear"))
                            break;
                    }
                    else if (item.Try(Flag, weaponType, "style_swing"))
                    {
                        if (item.Try(Flag, weaponType, "broadsword"))
                            break;
                    }
                    else if (item.Try(Flag, weaponType, "style_thrown"))
                    {
                        if (item.Try(Flag, weaponType, "boomerang"))
                            break;
                    }
                    else item.Flag(weaponType, "style_jab");


                    // the 'chain' weapons can be various styles, but they all
                    // have the same item.shoot projectileAI value. So check for
                    // that if all the other specific-weapon checks failed.
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

                    item.If(!item.Try(Flag, weaponType, "minion"),
                        SetFlag, weaponType, "sentry");

                    break;
                // case "type_throwing":
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
