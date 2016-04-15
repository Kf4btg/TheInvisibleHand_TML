// using Microsoft.Xna.Framework;
// using System.Collections.Generic;
// using System;
// using System.Linq;
using Terraria;
// using Terraria.ID;
using Terraria.ModLoader;
// using InvisibleHand.Utils;

namespace InvisibleHand.Items
{
    using static ItemFlags;

    public static class ItemCategorizer
    {
        // private static IDictionary<int, HashSet<string>> item_cache = new Dictionary<int, HashSet<string>>();

        // public static ISet<Trait> Classify(Item item)
        // {
        //     var iwi = new ItemWithInfo(item);
        //     classify(iwi);
        //
        //     return iwi.TraitList;
        // }

        public static void ClassifyItem(Item item, CategoryInfo info)
        {
            classifyitem(new ItemWithInfo(item, info));
        }

        private static void classifyitem(ItemWithInfo item)
        {
            var _item = item.item;
            bool _weapon, _tool, _placeable;
            _weapon = _tool = _placeable = false;

            item.FlagAny(Type.General,
                general.quest_item,
                general.expert,
                general.material,
                general.bait,
                general.mech,
                general.explosive,
                general.defense,
                general.heal_life,
                general.heal_mana,
                general.boost_mana,
                general.use_mana
            );

            if (!item.TryFlag(Type.General, general.reach_boost))
                item.Flag(Type.General, general.reach_penalty);

            _weapon = item.TryFlag(Type.General, general.weapon);
            if (_weapon)
            {
                item.Flag(Type.Weapon, weapon.has_projectile);
                if (item.FlagFirst(Type.Weapon,
                    weapon.melee,
                    weapon.ranged,
                    weapon.magic,
                    weapon.summon,
                    weapon.throwing
                    ).Success)
                {
                    classifyWeapon(item, item.LastFlag.Item2);
                }
                else
                    item.SetFlag(Type.Weapon, weapon.weapon_other);
            }

            // equipables
            if (item.TryFlag(Type.General, general.equipable))
            {
                bool vanity = item.TryFlag(Type.General, general.vanity);

                if (item.FlagFirst(Type.Equip, equip.slot_head, equip.slot_body, equip.slot_leg).Success)
                    item.FlagIf(!vanity, Type.Equip, equip.armor);
                else if (item.TryFlag(Type.Equip, equip.accessory))
                {
                    item.Flag(Type.Placeable, placeable.musicbox)
                        .FlagFirst(Type.Equip,
                            equip.slot_face,
                            equip.slot_neck,
                            equip.slot_back,
                            equip.wings,
                            equip.slot_shoe,
                            equip.slot_handon,
                            equip.slot_handoff,
                            equip.slot_shield,
                            equip.slot_waist,
                            equip.balloon,
                            equip.slot_front
                        );

                    if (!item.Success)
                        ErrorLogger.Log($"Unknown accessory type for item '{_item.name}', type {_item.type}");
                }
                else
                    item.FlagFirst(Type.Equip,
                        equip.pet_light,
                        equip.pet_vanity,
                        equip.grapple,
                        equip.mount
                    );

            }
            else
                _tool = item.FlagIf(item.FlagAny(Type.Tool, tool.pick, tool.axe, tool.hammer).Success
                        || item.FlagFirst(Type.Tool, tool.wand, tool.fishing_pole, tool.wrench).Success,
                    Type.General, general.tool).Success;

            _placeable = !(_weapon || _tool) && item.TryFlag(Type.General, general.placeable);

            if (_placeable)
            {
                item.Flag(Type.Furniture, furniture.crafting_station);

                if (item.TryFlag(Type.Furniture,  furniture.valid_housing))
                {
                    if (item.TryFlag(Type.Housing, housing.door))
                    {//break down
                        item.Flag(Type.Furniture, furniture.door);
                        // TODO: platforms, tall gate,
                        // TrapdoorClosed
                    }
                    else if (item.TryFlag(Type.Housing, housing.table))
                    {
                        item.FlagFirst(Type.Furniture,
                            furniture.table,
                            furniture.workbench,
                            furniture.dresser,
                            furniture.piano,
                            furniture.bookcase,
                            furniture.bathtub
                            // TODO: bewitching table, alchemy table, tinkerer's bench
                        );

                    }
                    else if (item.TryFlag(Type.Housing, housing.chair))
                    {
                        item.FlagFirst(Type.Furniture, furniture.chair, furniture.bed, furniture.bench
                            // TODO: thrones
                        );
                    }
                    else if (item.TryFlag(Type.Housing, housing.light))
                    {
                        item.FlagFirst(Type.Furniture,
                            furniture.torch,
                            furniture.candle,
                            furniture.chandelier,
                            furniture.hanging_lantern,
                            furniture.lamp,
                            furniture.holiday_light,
                            furniture.candelabra
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
                else if (item.TryFlag(Type.Placeable, placeable.wall_deco))
                {
                    switch (ClassificationRules.Types.WallDeco(_item))
                    {
                        // TODO: these flag values should probably be somewhere else,
                        // like maybe the furniture enum, or even a new enum for decorations
                        case WallDecoType.Painting:
                            item.SetFlag(Type.Placeable, placeable.painting);
                            break;
                        case WallDecoType.Trophy:
                            item.SetFlag(Type.Placeable, placeable.trophy);
                            break;
                        case WallDecoType.Rack:
                            item.SetFlag(Type.Placeable, placeable.rack);
                            break;
                    }
                }
                else
                {
                    item.FlagFirst(Type.Furniture, furniture.container, furniture.statue, furniture.sink,
                                        furniture.clock, furniture.statue_alphabet, furniture.tombstone,
                                        furniture.crate, furniture.planter, furniture.cannon,
                                        furniture.campfire, furniture.fountain, furniture.bottle,
                                        furniture.bowl, furniture.beachstuff, furniture.cooking_pot,
                                        furniture.anvil, furniture.monolith)
                        .FlagFirst(Type.Placeable, placeable.wall, placeable.dye_plant,
                                        placeable.strange_plant, placeable.banner,
                                        placeable.seed, placeable.ore, placeable.bar, placeable.gem);

                }
            }
            else if (item.TryFlag(Type.General, general.ammo))
            {
                item.FlagFirst(Type.Ammo, ammo.arrow, ammo.bullet, ammo.rocket, ammo.dart, ammo.sand, ammo.solution)
                    .FlagIf(!_item.consumable, Type.Ammo, ammo.endless); // endless quiver, musket pouch, etc
            }
            else if (item.TryFlag(Type.General, general.consumable))
            {
                item.Flag(Type.Consumable, consumable.buff)
                    .FlagFirst(Type.Consumable, consumable.food, consumable.potion);
                // or possibly flask...
            }
            else if (item.TryFlag(Type.General, general.dye))
            {
                item.FlagFirst(Type.Dye, dye.basic, dye.black, dye.flame, dye.gradient,
                                dye.strange, dye.bright, dye.silver, dye.lunar);
            }
            else
            {
                item.FlagFirst(Type.General, general.coin, general.soul, general.paint, general.hair_dye);
            }

            if ((item.info.Flags.general & general.mech) != 0)
            {
                item.FlagFirst(Type.Mech, mech.trap, mech.pressure_plate, mech.timer, mech.firework, mech.track);
            }

        }

        // private static void classify(ItemWithInfo item)
        // {
        //     var _item = item.item;
            // bool _weapon, tool, placeable;
            // weapon = tool = placeable = false;

            // some generic traits to begin with
            // item.Tag(Trait.quest_item)
            //     .Tag(Trait.expert)
            //     .Tag(Trait.material)
            //     .Tag(Trait.bait)
            //     .Tag(Trait.mech)
            //     .Tag(Trait.explosive)
            //     .Tag(Trait.defense)
            //     .Tag(Trait.heal_life)
            //     .Tag(Trait.heal_mana)
            //     .Tag(Trait.regen_life)
            //     .Tag(Trait.boost_mana)
            //     .Tag(Trait.use_mana);
            //
            // if (!item.TryTag(Trait.reach_boost))
            //     item.Tag(Trait.reach_penalty);
            //
            // weapon = item.TryTag(Trait.weapon);
            // if (weapon)
            // {
            //     item.Tag(Trait.has_projectile);
            //     if (item.TagFirst(
            //         Trait.melee,
            //         Trait.ranged,
            //         Trait.magic,
            //         Trait.summon,
            //         Trait.thrown
            //         ).Success)
            //     {
            //         classifyWeapon(item, item.LastTag);
            //     }
            //     else
            //         item.AddTag(Trait.weapon_other);
            // }

            // equipables
            // if (item.TryTag(Trait.equipable))
            // {
            //
            //     bool vanity = item.TryTag(Trait.vanity);
            //
            //     if (item.TagFirst(Trait.slot_head, Trait.slot_body, Trait.slot_leg).Success)
            //     {
            //         item.TagIf(!vanity, Trait.armor);
            //     }
            //     else if (item.TryTag(Trait.accessory))
            //     {
            //         item.Tag(Trait.musicbox)
            //             .TagFirst(
            //                 Trait.slot_face,
            //                 Trait.slot_neck,
            //                 Trait.slot_back,
            //                 Trait.wings,
            //                 Trait.slot_shoe,
            //                 Trait.slot_handon,
            //                 Trait.slot_handoff,
            //                 Trait.slot_shield,
            //                 Trait.slot_waist,
            //                 Trait.balloon,
            //                 Trait.slot_front
            //             );
            //
            //         if (!item.Success)
            //             ErrorLogger.Log($"Unknown accessory type for item '{_item.name}', type {_item.type}");
            //     }
            //     else
            //     {
            //         item.TagFirst(
            //             Trait.pet_light,
            //             Trait.pet_vanity,
            //             Trait.grapple,
            //             Trait.mount
            //         );
            //     }
            //
            // }
            // else
            //     item.TagIf(item.TagAny(Trait.pick, Trait.axe, Trait.hammer).Success
            //             || item.TagFirst(Trait.wand, Trait.fishing_pole, Trait.wrench).Success,
            //         Trait.tool);

            // placeable = !(weapon || tool) && item.TryTag(Trait.placeable);
            // if (placeable)
            // {
            //     item.Tag(Trait.crafting_station);
            //
            //     if (item.TryTag(Trait.housing_furniture))
            //     {
            //         if (item.TryTag(Trait.housing_door))
            //         {//break down
            //             item.Tag(Trait.door);
            //             // TODO: platforms, tall gate,
            //             // TrapdoorClosed
            //         }
            //         else if (item.TryTag(Trait.housing_table))
            //         {
            //             item.TagFirst(
            //                 Trait.table,
            //                 Trait.workbench,
            //                 Trait.dresser,
            //                 Trait.piano,
            //                 Trait.bookcase,
            //                 Trait.bathtub
            //                 // TODO: bewitching table, alchemy table, tinkerer's bench
            //             );
            //
            //         }
            //         else if (item.TryTag(Trait.housing_chair))
            //         {
            //             item.TagFirst( Trait.chair, Trait.bed, Trait.bench
            //                 // TODO: thrones
            //             );
            //         }
            //         else if (item.TryTag(Trait.housing_light))
            //         {
            //             item.TagFirst(
            //                 Trait.torch,
            //                 Trait.candle,
            //                 Trait.chandelier,
            //                 Trait.hanging_lantern,
            //                 Trait.lamp,
            //                 Trait.holiday_light,
            //                 Trait.candelabra
            //                 // TODO: TileID.WaterCandle,
        	// 				// TileID.ChineseLanterns,
            //                 // TileID.Jackolanterns,
        	// 				// TileID.SkullLanterns,
        	// 				// TileID.PlatinumCandelabra,
        	// 				// TileID.PlatinumCandle,
        	// 				// TileID.FireflyinaBottle,
        	// 				// TileID.LightningBuginaBottle,
        	// 				// TileID.BlueJellyfishBowl,
        	// 				// TileID.GreenJellyfishBowl,
        	// 				// TileID.PinkJellyfishBowl,
        	// 				// TileID.PeaceCandle,
        	// 				// TileID.Fireplace
            //             );
            //         }
            //     }
            //     else if (item.TryTag(Trait.wall_deco))
            //     {
            //         switch (Rules.Types.WallDeco(_item))
            //         {
            //             case WallDecoType.Painting:
            //                 item.AddTag(Trait.painting);
            //                 break;
            //             case WallDecoType.Trophy:
            //                 item.AddTag(Trait.trophy);
            //                 break;
            //             case WallDecoType.Rack:
            //                 item.AddTag(Trait.rack);
            //                 break;
            //         }
            //     }
            //     else
            //     {
            //         item.TagFirst(  Trait.wall, Trait.container, Trait.statue, Trait.sink,
            //                         Trait.clock, Trait.statue_alphabet, Trait.tombstone,
            //                         Trait.crate, Trait.planter, Trait.cannon,
            //                         Trait.campfire, Trait.fountain, Trait.bottle,
            //                         Trait.bowl, Trait.beachstuff, Trait.cooking_pot,
            //                         Trait.anvil, Trait.track, Trait.coin, Trait.trap, Trait.timer,
            //                         Trait.pressure_plate, Trait.firework, Trait.plant_dye,
            //                         Trait.plant_seed, Trait.ore, Trait.bar, Trait.gem,
            //                         Trait.monolith);

                    // item.Tag(Trait.ore)
                        // .Tag(Trait.gem);
                // }
            // }
            // else if (item.TryTag(Trait.ammo))
            // {
            //     item.TagFirst(Trait.arrow, Trait.bullet, Trait.rocket, Trait.dart, Trait.ammo_sand, Trait.ammo_solution)
            //         .TagIf(!_item.consumable, Trait.endless); // endless quiver, musket pouch, etc
            // }
            // else if (item.TryTag(Trait.consumable))
            // {
            //     item.Tag(Trait.buff)
            //         .TagFirst(Trait.food, Trait.potion);
            //     // or possibly flask...
            // }
            // else if (item.TryTag(Trait.dye))
            // {
            //     item.TagFirst(Trait.dye_basic, Trait.dye_basic, Trait.dye_flame, Trait.dye_gradient,
            //                     Trait.dye_strange, Trait.dye_bright, Trait.dye_silver, Trait.dye_lunar);
            // }
            // else
            // {
            //     item.TagFirst(Trait.soul, Trait.paint, Trait.hair_dye, Trait.banner);
            // }

        // }

        private static void classifyWeapon(ItemWithInfo item, long weaponType)
        {
            var _item = item.item;
            switch (weaponType)
            {
                case weapon.melee:
                    // item.Tag(has_projectile);
                    if (item.TryFlag(Type.Weapon, weapon.style_directional))
                        item.FlagFirst(Type.Weapon,
                                        weapon.flail,
                                        weapon.yoyo);
                    else if (item.TryFlag(Type.Weapon, weapon.style_swing))
                        item.Flag(Type.Weapon,
                                    weapon.broadsword);
                    else if (item.TryFlag(Type.Weapon, weapon.style_thrown))
                        item.Flag(Type.Weapon,
                                    weapon.boomerang);
                    else
                        item.FlagIf(item.TryFlag(Type.Weapon, weapon.style_jab),
                                    Type.Weapon, weapon.shortsword);
                    break;

                case weapon.ranged:
                    if (item.TryFlag(Type.Weapon, weapon.arrow_consuming))
                        item.Flag(Type.Weapon,
                                    weapon.repeater);
                    else if (item.TryFlag(Type.Weapon, weapon.bullet_consuming))
                        item.Flag(Type.Weapon,
                                    weapon.automatic_gun);
                    else
                        item.FlagFirst(Type.Weapon,
                                        weapon.rocket_consuming,
                                        weapon.dart_consuming,
                                        weapon.gel_consuming,
                                        weapon.no_ammo);
                    break;

                case weapon.magic:
                    item.FlagFirst(Type.Weapon,
                                    weapon.area,
                                    weapon.homing,
                                    weapon.controlled,
                                    weapon.bouncing,
                                    weapon.stream);
                    break;
                case weapon.summon:
                    item.FlagIf(!item.TryFlag(Type.Weapon, weapon.minion),
                                Type.Weapon, weapon.sentry);
                    break;
                case weapon.throwing:
                default:
                    break;
            }
        }

        // private static void classifyWeapon(ItemWithInfo item, Trait weaponType)
        // {
        //     var _item = item.item;
        //     switch (weaponType)
        //     {
        //         case Trait.melee:
        //             // item.Tag(has_projectile);
        //             if (item.TryTag(Trait.melee_style_directional))
        //                 item.TagFirst(Trait.spear, Trait.flail, Trait.yoyo);
        //             else if (item.TryTag(Trait.melee_style_swing))
        //                 item.Tag(Trait.broadsword);
        //             else if (item.TryTag(Trait.melee_style_thrown))
        //                 item.Tag(Trait.boomerang);
        //             else
        //                 item.TagIf(item.TryTag(Trait.melee_style_jab), Trait.shortsword);
        //             break;
        //         case Trait.ranged:
        //             if (item.TryTag(Trait.arrow_consuming))
        //                 item.Tag(Trait.repeater);
        //             else if (item.TryTag(Trait.bullet_consuming))
        //                 item.Tag(Trait.automatic_gun);
        //             else
        //                 item.TagFirst(Trait.rocket_consuming, Trait.dart_consuming, Trait.gel_consuming, Trait.no_ammo);
        //             break;
        //         case Trait.magic:
        //             item.TagFirst(Trait.area, Trait.homing, Trait.controlled, Trait.bouncing, Trait.stream);
        //             break;
        //         case Trait.summon:
        //             item.TagIf(!item.TryTag(Trait.minion), Trait.sentry);
        //             break;
        //         case Trait.thrown:
        //         default:
        //             break;
        //     }
        // }
    }
}
