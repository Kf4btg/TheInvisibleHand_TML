// using System.Collections.Generic;
// using System;
// using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace InvisibleHand.Items
{
    public static class ItemClassifier
    {
        public static void ClassifyItem(Item item, ItemFlagInfo info)
        {
            classifyitem(new ItemClassificationWrapper(item, info));
        }

        private static void classifyitem(ItemClassificationWrapper item)
        {
            var _item = item.item;
            bool _weapon, _tool;
            _weapon = _tool = false;

            item.FlagAny("General", "quest_item",
                                    "expert",
                                    "material",
                                    "bait",
                                    "explosive",
                                    "defense",
                                    "heal_life",
                                    "heal_mana",
                                    "boost_mana",
                                    "use_mana"
            );

            if (!item.TryFlag("General", "reach_boost"))
                item.Flag("General", "reach_penalty");

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
                else
                    item.SetFlag("Weapon", "type_other");
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
                                            "slot_front"
                        );

                    if (!item.Success)
                        ErrorLogger.Log($"Unknown accessory type for item '{_item.name}', type {_item.type}");
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

            // _placeable = !(_weapon || _tool) && item.TryFlag("General", "placeable");

            // if (_placeable)
            if (!(_weapon || _tool) && item.TryFlag("General", "placeable"))
            {
                item.Flag("Furniture", "crafting_station");

                if (item.TryFlag("Furniture",  "valid_housing"))
                {
                    if (item.TryFlag("Furniture", "housing_door"))
                    {
                        //break down
                        item.Flag("Furniture.Doors", "door");
                        // TODO: platforms, tall gate,
                        // TrapdoorClosed
                    }
                    else if (item.TryFlag("Furniture", "housing_table"))
                    {
                        item.FlagFirst("Furniture.Tables", "table",
                                                           "workbench",
                                                           "dresser",
                                                           "piano",
                                                           "bookcase",
                                                           "bathtub"
                            // TODO: bewitching table, alchemy table, tinkerer's bench
                        );

                    }
                    else if (item.TryFlag("Furniture", "housing_chair"))
                    {
                        item.FlagFirst("Furniture.Chairs", "chair",
                                                           "bed",
                                                           "bench"
                           // TODO: thrones
                        );
                    }
                    else if (item.TryFlag("Furniture", "housing_light"))
                    {
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
                }
                else if (item.TryFlag("Placeable", "wall_deco"))
                {
                    switch (ClassificationRules.Types.WallDeco(_item))
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
                }
                else
                {
                    item.FlagFirst("Furniture", "container")
                        .FlagFirst("Furniture.Other", "statue",
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
                                                      "monolith")
                        .FlagFirst("Placeable", "wall",
                                                "dye_plant",
                                                "strange_plant",
                                                "banner",
                                                "seed",
                                                "ore",
                                                "bar",
                                                "gem");
                }
            }
            else if (item.TryFlag("General", "ammo"))
                item.FlagFirst("Ammo", "arrow",
                                       "bullet",
                                       "rocket",
                                       "dart",
                                       "sand",
                                       "solution")
                    .FlagIf(!_item.consumable, "Ammo", "endless"); // endless quiver, musket pouch, etc

            else if (item.TryFlag("General", "consumable"))
                item.Flag("Consumable", "buff")
                    .FlagFirst("Consumable", "food", "potion");
                // or possibly flask...

            else if (item.TryFlag("General", "dye"))
                item.FlagFirst("Dye", "basic",
                                      "black",
                                      "flame",
                                      "gradient",
                                      "strange",
                                      "bright",
                                      "silver",
                                      "lunar");
            else
                item.FlagFirst("General", "coin", "soul", "paint", "hair_dye");

            if (item.TryFlag("General", "mech"))
                item.FlagFirst("Mech", "trap", "pressure_plate", "timer", "firework", "track");

        }

        private static void classifyWeapon(ItemClassificationWrapper item, string type, string flag)
        {
            if (type != "Weapon") return;

            string weaponType;
            switch (flag)
            {
                case "type_melee":
                    weaponType = "Weapon.Melee";

                    if (item.TryFlag(weaponType, "style_directional"))
                        item.FlagFirst(weaponType, "flail", "yoyo");

                    else if (item.TryFlag(weaponType, "style_swing"))
                        item.Flag(weaponType, "broadsword");

                    else if (item.TryFlag(weaponType, "style_thrown"))
                        item.Flag(weaponType, "boomerang");


                    // else
                        // item.FlagIf(item.TryFlag(weaponType, "style_jab"),
                                    // weaponType, "shortsword");
                    break;

                case "type_ranged":
                    weaponType = "Weapon.Ranged";
                    // if (item.TryFlag(weaponType, "arrow_consuming"))
                    //     item.Flag(weaponType,
                    //                 "repeater");
                    // else if (item.TryFlag(weaponType, "bullet_consuming"))
                    //     item.Flag(weaponType,
                    //                 "automatic_gun");
                    // else
                    item.FlagFirst(weaponType, "arrow_consuming",
                                               "bullet_consuming",
                                               "rocket_consuming",
                                               "dart_consuming",
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
                    item.FlagIf(!item.TryFlag(weaponType, "minion"),
                                weaponType, "sentry");
                    break;
                case "type_throwing":
                    // break;
                default:
                    // tag as 'other' if not a throwing weapon
                    // (there are no sub-categories for thrown weapons yet,
                    // so there's no Flag Type or extra condititions for them either)
                    // item.Flag("Weapon", "type_other");
                    break;
            }
        }
    }
}
