// using Microsoft.Xna.Framework;
using System.Collections.Generic;
// using System;
// using System.Linq;
using Terraria;
// using Terraria.ID;
using Terraria.ModLoader;
// using InvisibleHand.Utils;

namespace InvisibleHand.Items
{
    public static class ItemCategorizer
    {
        // private static IDictionary<int, HashSet<string>> item_cache = new Dictionary<int, HashSet<string>>();

        public static ISet<Trait> Classify(Item item)
        {
            var iwi = new ItemWithInfo(item);
            classify(iwi);

            return iwi.TraitList;
        }

        private static void classify(ItemWithInfo item)
        {
            var _item = item.item;
            bool weapon, tool, placeable;
            weapon = tool = placeable = false;

            // some generic traits to begin with
            item.Tag(Trait.quest_item)
                .Tag(Trait.expert)
                .Tag(Trait.material)
                .Tag(Trait.bait)
                .Tag(Trait.mech)
                .Tag(Trait.defense);

            if (!item.TryTag(Trait.reach_boost))
                item.Tag(Trait.reach_penalty);

            weapon = item.TryTag(Trait.weapon);
            if (weapon)
            {
                item.Tag(Trait.has_projectile);
                if (item.TagFirst(
                    Trait.melee,
                    Trait.ranged,
                    Trait.magic,
                    Trait.summon,
                    Trait.thrown
                    ).Success)
                {
                    switch(item.LastTag)
                    {
                        case Trait.melee:
                        case Trait.ranged:
                        case Trait.magic:
                        case Trait.summon:
                        case Trait.thrown:
                            break;
                    }
                }
                else
                    item.AddTag(Trait.weapon_other);
            }

            // equipables
            if (item.TryTag(Trait.equipable))
            {

                bool vanity = item.TryTag(Trait.vanity);

                if (item.TagFirst(Trait.slot_head, Trait.slot_body, Trait.slot_leg).Success)
                {
                    item.TagIf(!vanity, Trait.armor);
                }
                else if (item.TryTag(Trait.accessory))
                {
                    item.Tag(Trait.musicbox)
                        .TagFirst(
                            Trait.slot_face,
                            Trait.slot_neck,
                            Trait.slot_back,
                            Trait.wings,
                            Trait.slot_shoe,
                            Trait.slot_handon,
                            Trait.slot_handoff,
                            Trait.slot_shield,
                            Trait.slot_waist,
                            Trait.balloon,
                            Trait.slot_front
                        );

                    if (!item.Success)
                        ErrorLogger.Log($"Unknown accessory type for item '{_item.name}', type {_item.type}");
                }
                else
                {
                    item.TagFirst(
                        Trait.pet_light,
                        Trait.pet_vanity,
                        Trait.grapple,
                        Trait.mount
                    );
                }

            }
            else
                item.TagIf(item.TagAny(Trait.pick, Trait.axe, Trait.hammer).Success
                        || item.TagFirst(Trait.wand, Trait.fishing_pole, Trait.wrench).Success,
                    Trait.tool);

            placeable = !(weapon || tool) && item.TryTag(Trait.placeable);
            if (placeable)
            {
                item.Tag(Trait.crafting_station);

                if (item.TryTag(Trait.housing_furniture))
                {
                    if (item.TryTag(Trait.housing_door))
                    {//break down
                        item.Tag(Trait.door);
                        // TODO: platforms, tall gate,
                        // TrapdoorClosed
                    }
                    else if (item.TryTag(Trait.housing_table))
                    {
                        item.TagFirst(
                            Trait.table,
                            Trait.workbench,
                            Trait.dresser,
                            Trait.piano,
                            Trait.bookcase,
                            Trait.bathtub
                            // TODO: bewitching table, alchemy table, tinkerer's bench
                        );

                    }
                    else if (item.TryTag(Trait.housing_chair))
                    {
                        item.TagFirst( Trait.chair, Trait.bed, Trait.bench
                            // TODO: thrones
                        );
                    }
                    else if (item.TryTag(Trait.housing_light))
                    {
                        item.TagFirst(
                            Trait.torch,
                            Trait.candle,
                            Trait.chandelier,
                            Trait.hanging_lantern,
                            Trait.lamp,
                            Trait.holiday_light,
                            Trait.candelabra
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
                    item.TagFirst(  Trait.container, Trait.statue, Trait.sink,
                                    Trait.clock, Trait.statue_alphabet, Trait.tombstone,
                                    Trait.crate, Trait.planter, Trait.cannon,
                                    Trait.campfire, Trait.fountain, Trait.bottle,
                                    Trait.bowl, Trait.beachstuff, Trait.cooking_pot,
                                    Trait.anvil, Trait.track, Trait.trap, Trait.timer,
                                    Trait.pressure_plate, Trait.firework, Trait.plant_dye,
                                    Trait.plant_seed, Trait.ore, Trait.bar, Trait.gem,
                                    Trait.monolith);

                    // item.Tag(Trait.ore)
                        // .Tag(Trait.gem);
                }
            }
            else if (item.TryTag(Trait.ammo))
            {
                item.TagFirst(Trait.arrow, Trait.bullet, Trait.rocket, Trait.dart, Trait.ammo_sand, Trait.ammo_solution)
                    .TagIf(!_item.consumable, Trait.endless); // endless quiver, musket pouch, etc
            }
            else if (item.TryTag(Trait.consumable))
            {
                item.Tag(Trait.buff)
                    .TagFirst(Trait.food, Trait.potion);
                                    // or possibly flask...
            }

        }

    }
}
