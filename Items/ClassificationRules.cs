using System.Collections.Generic;
using System;
using System.Linq;
// using System.Reflection;
using Terraria;
using Terraria.ID;
using System.Text.RegularExpressions;
// using Terraria.ModLoader;
// using InvisibleHand.Utils;

namespace InvisibleHand.Items
{
    using static ItemFlags;
    using ConditionTable = Dictionary<int, Func<Item, bool>>;

    public static class ClassificationRules
    {

        /// prevents attempting to index Main.projectile w/ -1
        public static bool TestProjectileAI(int projid, int otherid)
        {
            return projid > 0 && Main.projectile[projid].aiStyle == otherid;
        }

        /// Every method/property in Rules.Binary is a simple yes/no (boolean) query
        internal static class Binary
        {
            public static bool isWeapon(Item item) => (item.damage > 0 && (!item.notAmmo || item.useStyle > 0));
            public static bool isArmor(Item item)  => item.headSlot > 0 || item.bodySlot > 0 || item.legSlot > 0; // && !item.vanity

            // also includes the wire cutter
            public static bool isWrench(Item item) => item.mech && item.tileBoost == 20;

            public static bool isHook(Item item)     => Main.projHook[item.shoot];
            public static bool isMount(Item item)    => item.mountType != -1;
            public static bool isLightPet(Item item) => item.buffType > 0 && (Main.lightPet[item.buffType]);

            public static bool isVanityPet(Item item) => item.buffType > 0 && (Main.vanityPet[item.buffType]);

            public static bool isEquipable(Item item) => (item.headSlot > 0 || item.bodySlot > 0 || item.legSlot > 0 || item.accessory || Main.projHook[item.shoot] || item.mountType != -1 || (item.buffType > 0 && (Main.lightPet[item.buffType] || Main.vanityPet[item.buffType])));

            /// NOTE: not everything that has a "placeStyle" has a "createTile" (the noteable
            /// exceptions are the butterflies, which have a "createNPC" instead), and
            /// not everything with a "createTile" necessarily has a "placeStyle", either,
            /// even when it seems like it really should. E.g., "Workbenches" have createTile == 18;
            /// every workbench EXCEPT for the regular, plain old "Work Bench" also have a
            /// placeStyle value; but no, not the original...and that seems to be the trend:
            /// original (or early) items have no placestyle, but items added in later updates do.
            // public static bool placeable(Item item) => item.placeStyle > 0;

            /// Here's something, pulled from Main.MouseText:
            /// these are the conditions applied to see if an item gets the "Can be placed" tooltip
            /// it is mutually-exclusive with:
            //      item.ammo && !item.notAmmo
            //      item.consumable
            public static bool CanBePlaced(Item item) => (item.createTile > -1 || item.createWall > 0) && (item.type != 213 && item.tileWand < 1);

            public static bool isAmmo(Item item) => /*!CanBePlaced(item) &&*/ item.ammo > 0 && !item.notAmmo;
            public static bool isConsumable(Item item) => /*!isAmmo(item) && !CanBePlaced(item) && */ item.consumable;

            public static bool isFood(Item item)    => item.buffType == BuffID.WellFed;

            public static bool timedBuff(Item item) => item.buffTime > 0;

            // maybe?
            public static bool isPotion(Item item)  => timedBuff(item) && !isFood(item);

            public static bool falling(Item item)   => TileID.Sets.Falling[item.createTile];

            public static bool Explosive(Item item) => TestProjectileAI(item.shoot, 16);

            public static bool oneDropYoyo(Item item) => new[] { 3315, 3316, 3317, 3262, 3282, 3283, 3284, 3285, 3286, 3389 }.Contains(item.type);
        }

        /// these rules are dependent on Binary.CanBePlaced()
        internal static class ByTileID
        {

            #region roomneeds doors
    		public static bool Door(Item item)     => item.createTile == TileID.ClosedDoor;
            public static bool Platform(Item item) => item.createTile == TileID.Platforms;
            #endregion roomneeds doors

            #region roomneeds chairs
            public static bool Chair(Item item) => item.createTile == TileID.Chairs;
            public static bool Bed(Item item)   => item.createTile == TileID.Beds;
            /// also sofas
            public static bool Bench(Item item) => item.createTile == TileID.Benches;
            #endregion roomneeds chairs

            #region roomneeds tables
    		public static bool WorkBench(Item item) => item.createTile == TileID.WorkBenches;
            public static bool Table(Item item)     => item.createTile == TileID.Tables;
            public static bool Piano(Item item)     => item.createTile == TileID.Pianos;
            public static bool Dresser(Item item)   => item.createTile == TileID.Dressers;
            public static bool Bookcase(Item item)  => item.createTile == TileID.Bookcases;
            // ... why is the bathtub a table? (I think I know why...)
            public static bool Bathtub(Item item)   => item.createTile == TileID.Bathtubs;
            #endregion roomneeds tables

            #region lighting
            public static bool Candle(Item item)         => item.createTile == TileID.Candles;
            public static bool Chandelier(Item item)     => item.createTile == TileID.Chandeliers;
            public static bool HangingLantern(Item item) => item.createTile == TileID.HangingLanterns;
    		public static bool Lamp(Item item)           => item.createTile == TileID.Lamps;
            public static bool Candelabra(Item item)     => item.createTile == TileID.Candelabras;
            public static bool Torch(Item item)          => item.createTile == TileID.Torches;
            // red and green
            public static bool HolidayLight(Item item)   => item.createTile == TileID.HolidayLights;
            #endregion lighting

            public static bool Anvil(Item item) => item.createTile == TileID.Anvils;
            public static bool CookingPot(Item item) => item.createTile == TileID.CookingPots;
            public static bool Sink(Item item) => item.createTile == TileID.Sinks;
            public static bool GrandfatherClock(Item item) => item.createTile == TileID.GrandfatherClocks;
            public static bool Cannon(Item item)   => item.createTile == TileID.Cannon;
            public static bool Campfire(Item item) => item.createTile == TileID.Campfire;

            // chests, barrels, and trash can
            public static bool Container(Item item) => item.createTile == TileID.Containers;

            /// important (and there's a lot of these...)
            public static bool Banner(Item item) => item.createTile == TileID.Banners;

            // includes cups and mugs
            public static bool Bottle(Item item) => item.createTile == TileID.Bottles;
            /// three of them!
            public static bool Bowl(Item item) => item.createTile == TileID.Bowls;
            /// "Starfish"
            public static bool BeachPile(Item item) => item.createTile == TileID.BeachPiles;

            // mech stuff
            public static bool Trap(Item item)          => item.createTile == TileID.Traps;
    		public static bool PressurePlate(Item item) => item.createTile == TileID.PressurePlates;
            public static bool Timer(Item item)         => item.createTile == TileID.Timers;

            public static bool MusicBox(Item item) => item.createTile == TileID.MusicBoxes;

            /// green, blue, & yellow rockets
            public static bool Firework(Item item) => item.createTile == TileID.Firework;

            /// NOTE: Strange Plants have a special Set in ItemID.Sets
            public static bool DyePlant(Item item)     => item.createTile == TileID.DyePlants;
            /// these are the seeds for the herbs
            public static bool ImmatureHerb(Item item) => item.createTile == TileID.ImmatureHerbs;
            public static bool MetalBar(Item item)     => item.createTile == TileID.MetalBars;

    		// public static bool MinecartTrack(Item item) => item.createTile == TileID.MinecartTrack;

            /// also includes vases
            public static bool Statue(Item item)        => item.createTile == TileID.Statues;
            public static bool Tombstone(Item item)     => item.createTile == TileID.Tombstones;
            public static bool Gem(Item item)           => item.createTile == TileID.ExposedGems;
            public static bool WaterFountain(Item item) => item.createTile == TileID.WaterFountain;
            /// ABC, 123
            public static bool AlphabetStatue(Item item) => item.createTile == TileID.AlphabetStatues;
    		public static bool FishingCrate(Item item)   => item.createTile == TileID.FishingCrate;
    		public static bool PlanterBox(Item item)     => item.createTile == TileID.PlanterBox;
            public static bool LunarMonolith(Item item)  => item.createTile == TileID.LunarMonolith;
        }

        internal static class Groupings
        {
            public static bool Furniture(Item item) => TileSets.Furniture.Contains(item.createTile);

            public static bool housingDoor(Item item) =>
                    TileID.Sets.RoomNeeds.CountsAsDoor.Contains(item.createTile);
            public static bool housingTable(Item item) =>
                    TileID.Sets.RoomNeeds.CountsAsTable.Contains(item.createTile);
            public static bool housingChair(Item item) =>
                    TileID.Sets.RoomNeeds.CountsAsChair.Contains(item.createTile);
            public static bool housingTorch(Item item) =>
                    TileID.Sets.RoomNeeds.CountsAsTorch.Contains(item.createTile);

            public static bool Ore(Item item) => TileID.Sets.Ore[item.createTile];
            public static bool Ice(Item item) => TileID.Sets.Ices[item.createTile];

            public static bool HallowBlock(Item item)     => TileID.Sets.Hallow[item.createTile];
            public static bool CrimsonBlock(Item item)    => TileID.Sets.Crimson[item.createTile];
            public static bool CorruptionBlock(Item item) => TileID.Sets.Corrupt[item.createTile];

            public static bool Sand(Item item)          => TileID.Sets.Conversion.Sand[item.createTile];
            public static bool HardenedSand (Item item) => TileID.Sets.Conversion.HardenedSand[item.createTile];
            public static bool Sandstone(Item item)     => TileID.Sets.Conversion.Sandstone[item.createTile];

            public static bool Stone(Item item) => TileID.Sets.Conversion.Stone[item.createTile];

            public static bool CanPlaceOnWall(Item item) => TileID.Sets.FramesOnKillWall[item.createTile];

            public static bool StrangePlant(Item item) => ItemID.Sets.ExoticPlantsForDyeTrade[item.type];

            /// Yoraizor's stuff is also in this item set, but will likely get caught by an 'isAccessory'
            /// check or something before it gets here.
            public static bool Soul(Item item) => ItemID.Sets.AnimatesAsSoul[item.type];
            /// Seems the Trophies fall in these categories, too. All the vanilla ones, at
            /// least, also end in "Trophy", so that could be a way to distinguish them;
            /// AND the decorative racks (blacksmith, helmet, spear, etc.), are here too;
            /// they all seem to end in "Rack"; Some other decorative wall hangings (like
            /// Ship's wheel, Compass Rose, etc.), will likely be difficult to distinguish
            /// from paintings, though.
            public static bool WallDeco(Item item) => new int[] { TileID.Painting3X3, TileID.Painting4X3, TileID.Painting6X4, TileID.Painting2X3, TileID.Painting3X2 }.Contains(item.createTile);
            // public static bool Painting3X3(Item item) => item.createTile == TileID.Painting3X3;
            // public static bool Painting4X3(Item item) => item.createTile == TileID.Painting4X3;
            // public static bool Painting6X4(Item item) => item.createTile == TileID.Painting6X4;
            // public static bool Painting2X3(Item item) => item.createTile == TileID.Painting2X3;
            // public static bool Painting3X2(Item item) => item.createTile == TileID.Painting3X2;

            public static bool NebulaPickup(Item item) => ItemID.Sets.ExtractinatorMode.Contains(item.type);

            public static bool GoesInExtractinator(Item item) => ItemID.Sets.NebulaPickup[item.type];
        }

        internal static class Dyes
        {
            private const short base_dye_start = 1007; // ItemID.RedDye
            private const short base_dye_end = 1018; // ItemID.PinkDye
            private const short dye_range_length = base_dye_end - base_dye_start + 1;

            private static readonly IEnumerable<int> basic_dye_ids =
                (Enumerable.Range(base_dye_start, dye_range_length)
                    .ToList()
                    .Concat(new int[3] { ItemID.BrownDye, ItemID.BlackDye, ItemID.SilverDye }))
                    .ToList();

            private static readonly IEnumerable<int> black_dye_ids =
                (Enumerable.Range(base_dye_start + 12, dye_range_length)
                .ToList()
                .Concat(new int[2] { ItemID.BrownDye + 1, ItemID.BlackAndWhiteDye }))
                .ToList();

            private static readonly IEnumerable<int> bright_dye_ids =
                (Enumerable.Range(base_dye_start + 31, dye_range_length)
                .ToList()
                .Concat(new int[2] { ItemID.BrownDye + 2, ItemID.BrightSilverDye }))
                .ToList();

            private static readonly IEnumerable<int> silver_dye_ids =
                (Enumerable.Range(base_dye_start + 44, dye_range_length)
                .ToList()
                .Concat(new int[2] { ItemID.BrownDye + 3, ItemID.SilverAndBlackDye }))
                .ToList();

            private static readonly IEnumerable<int>[] flame_dye_ids = new[]
            {
                Enumerable.Range(1031, 1036-1031+1),
                Enumerable.Range(1063, 1065-1063+1),
                Enumerable.Range(3550, 3552-3550+1)
            };

            private static readonly IEnumerable<int> gradient_dye_ids = Enumerable.Range(1066, 1070 - 1066 + 1).ToList();

            public static bool BasicDyes(Item item) => basic_dye_ids.Contains(item.type);

            /// the "... and Black" dyes
            public static bool BlackDyes(Item item) =>  basic_dye_ids.Contains(item.type);

            public static bool BrightDyes(Item item) => basic_dye_ids.Contains(item.type);

            /// the "... and Silver" dyes
            public static bool SilverDyes(Item item) =>  basic_dye_ids.Contains(item.type);

            /// gradient and rainbow dyes
            public static bool GradientDyes(Item item) => gradient_dye_ids.Contains(item.type);

            /// including intense versions
            public static bool FlameDyes(Item item)
            {
                var itemid = item.type;
                foreach (var range in flame_dye_ids)
                {
                    if (range.Contains(itemid)) return true;
                }
                return false;
            }

            private static readonly IEnumerable<int>[] strange_dye_ranges = new[]
            {
                Enumerable.Range(2869, 2873-2869+1),
                Enumerable.Range(2883, 2885-2883+1),
                Enumerable.Range(3024, 3028-3024+1),
                Enumerable.Range(3038, 3042-3038+1),
                Enumerable.Range(3533, 3535-3533+1),
                Enumerable.Range(3553, 3556-3553+1),
                Enumerable.Range(3560, 3562-3560+1),
                Enumerable.Range(3597, 3600-3597+1),
                new[] {2878, 2879, 3190, 3530}
            };

            public static bool StrangeDyes(Item item)
            {
                var itemid = item.type;

                foreach (var range in strange_dye_ranges)
                {
                    if (range.Contains(itemid)) return true;
                }
                return false;
            }

            public static bool LunarDyes(Item item) => (item.type >= 3526 && item.type <= 3529);
        }

        /// narrows down a generic grouping (e.g. 'Weapon') to a more specific classifier (e.g. 'Melee')
        internal static class Types
        {

            /// call with an Item instance and the name of a
            /// property such as "faceSlot", "wingsSlot", etc.
            // public static bool AccySlot(Item item, string slot_type)
            // {
            //     return (int)(typeof(Item).GetField(slot_type, BindingFlags.Public | BindingFlags.Instance).GetValue(item)) > 0;
            // }

            public static WallDecoType WallDeco(Item item)
            {
                var last = item.name.Split(null).Last();

                if (last == "Trophy")
                    return WallDecoType.Trophy;
                else if (last == "Rack")
                    return WallDecoType.Rack;
                else if (Regex.Match(Lang.toolTip(item.type), @"^'[A-Z]\.").Success)
                    // the tooltip for paintings includes the artist's name as
                    // their first initial followed by maybe another initial and lastname,
                    // but we can just check for that first initial &_assume painting.
                    return WallDecoType.Painting;

                    //.Net note: according to MSDN (I'll assume Mono is the same), regexes in
                    //static methods are cached and reused rather than reparsed for each execution.
                    //Since compiled regexes are apparently never released to GC, this may be a
                    //decent compromise between speed and memory

                return WallDecoType.Other;
            }
        }

        internal static class Weapons
        {
            internal static class Melee
            {
                // broadswords and most tools
                public static bool Swing(Item item) => item.useStyle == 1 && !item.noMelee;
                // shortswords
                public static bool Jab(Item item) => item.useStyle == 3;
                // spear, flail, yoyo, mech tools
                public static bool Directional(Item item) => item.useStyle == 5;

                // boomerangs, daybreak, flying knife...
                public static bool Thrown(Item item) => item.useStyle == 1 && item.noMelee;

                // picks, hammers, axes
                public static bool isTool(Item item) => item.pick > 0 || item.axe > 0 || item.hammer > 0;
                // drills, jackhammers, chainsaws
                // OR: => Main.projectile[item.shoot].aiStyle==20;
                public static bool mechTool(Item item) => isTool(item) && item.channel;

                /// alias for 'Jab'
                public static bool ShortSword(Item item) => Jab(item);

                /// Dependent on 'Thrown' (well. maybe not really)
                public static bool Boomerang(Item item) => TestProjectileAI(item.shoot, 3);


                //***
                //Dependent on Swing()==true
                //***
                /// also includes some things like Purple Clubberfish, slap hand, etc., that aren't
                /// technically swords, but are swung overhead & there's really no way to tell them apart...
                // the !noUseGraphic check is for excluding things like chain guillotines, flairon;
                public static bool BroadSword(Item item) => /*Swing(item) &&*/ !(isTool(item) || item.noUseGraphic);


                //***
                //Dependent on Directional()==true
                //***

                // spears are kind of a "if it doesn't fit anything else" category...but this seems to single them out nicely.
                // This also includes Scourge of the Corruptor, which is kinda iffy, but close enough
                // public static bool Spear(Item item) =>/* Directional(item) &&*/ (item.noMelee && item.noUseGraphic && !item.channel);
                //
                // Actually, I think using the aiStyle of the item.shoot projectile is better.
                // NOTE: spears and mech tools also have a projectiles w/ tileCollide==False
                public static bool Spear(Item item) => TestProjectileAI(item.shoot, 19);

                // all (vanilla) flails (other than Flairon, which is just...weird) use animations 40 or 45
                // NOTE: also, flails have a projectile id (item.shoot) of 15;
                // public static bool Flail(Item item) => /*Directional(item) && */ (item.useAnimation == 40 || item.useAnimation == 45);
                public static bool Flail(Item item) => TestProjectileAI(item.shoot, 15);

                // Yoyos; the 'usetime' check is necessary to distinguish from the Arkhalis
                // public static bool Yoyo(Item item) => /*Directional(item) &&*/ (item.channel && item.useAnimation == 25 && item.useTime == 25);
                public static bool Yoyo(Item item) => TestProjectileAI(item.shoot, 99);
            }


            internal static class Ranged
            {
                public static bool ArrowConsuming(Item item)  => item.useAmmo == 1;
                public static bool BulletConsuming(Item item) => item.useAmmo == 14;
                public static bool RocketConsuming(Item item) => item.useAmmo == 771;
                public static bool DartConsuming(Item item)   => item.useAmmo == 51;
                public static bool GelConsuming(Item item)    => item.useAmmo == 23;

                public static bool NoAmmo(Item item) => item.useAmmo < 1;

                /// this isn't entirely accurate...some of the highlevel bows also have auto-reuse set True;
                /// it's probably the best we can do, though.
                public static bool Repeater(Item item) => ArrowConsuming(item) && item.autoReuse;
                public static bool AutomaticGun(Item item) => BulletConsuming(item) && item.autoReuse;

            }

            internal static class Magic
            {
                public static bool Homing(Item item) => ProjectileID.Sets.Homing[item.shoot];

                public static bool Area(Item item)
                {
                    if (item.shoot < 1) return false;
                    var projectile = Main.projectile[item.shoot];
                    return projectile.maxPenetrate < 0 && !projectile.tileCollide;
                }

                // e.g. magic missile
                public static bool Controllable(Item item) => TestProjectileAI(item.shoot, 9);
                // e.g. water bolt
                public static bool Bouncing(Item item)
                {
                    if (item.shoot < 1) return false;
                    var projectile = Main.projectile[item.shoot];
                    return projectile.aiStyle == 8 || projectile.aiStyle == 14;
                }

                // e.g. aqua scepter
                public static bool Stream(Item item) => TestProjectileAI(item.shoot, 12);
                // e.g. a lot of things; anything that passes through at least one enemy
                public static bool Piercing(Item item) => Main.projectile[item.shoot].maxPenetrate > 1;

                // i.e. straight(ish) line that passes through blocks
                public static bool VilethornAI(Item item) => TestProjectileAI(item.shoot, 4);
            }

            internal static class Summon
            {
                public static bool Minion(Item item) => Main.projectile[item.shoot].minion;

                // there's nothing like a 'sentry' field, but that's all that's left after taking
                // out the 'minion' weapons, so...
                public static bool Sentry(Item item) => !Minion(item);
            }

        }
        // Ideas for dealing with mod items:
        //  1) Use Recipe.ItemMatches() for added materials
        //

        internal static bool REPLACE_ME(Item item) => true;

        /// C# just doesn't understand ducks...
        internal static class Conditions
        {
            public static readonly Dictionary<ItemFlags.Type, ConditionTable> Matrix =
            new Dictionary<ItemFlags.Type, ConditionTable>()
            {
                {Type.General, new ConditionTable
                    {
                        {general.quest_item, (i)    => i.questItem},
                        {general.expert, (i)        => i.expert},
                        {general.material, (i)      => i.material},
                        {general.mech, (i)          => i.mech},
                        {general.channeled, (i)     => i.channel},
                        {general.bait, (i)          => i.bait > 0},
                        {general.reach_boost, (i)   => i.tileBoost > 0},
                        {general.reach_penalty, (i) => i.tileBoost < 0},
                        {general.heal_life, (i)     => i.healLife > 0},
                        {general.regen_life, (i)    => i.lifeRegen > 0},
                        {general.heal_mana, (i)     => i.healMana > 0},
                        {general.boost_mana, (i)    => i.manaIncrease > 0},
                        {general.use_mana, (i)      => i.mana > 0},
                        {general.vanity, (i)        => i.vanity},
                        {general.dye, (i)           => i.dye > 0},
                        {general.equipable, Binary.isEquipable},
                        {general.placeable, Binary.CanBePlaced},
                        {general.consumable, Binary.isConsumable},
                        {general.weapon, Binary.isWeapon},
                        {general.ammo, Binary.isAmmo},
                        {general.soul, Groupings.Soul},
                        {general.hair_dye, (i) => i.hairDye > 0},
                        {general.coin, (i)     => i.ammo == 71},
                        {general.paint, (i)    => i.paint > 0},
                    }
                },
                {Type.Placeable, new ConditionTable
                    {
                        // {placeable.furniture, REPLACE_ME},
                        {placeable.seed, ByTileID.ImmatureHerb},
                        {placeable.dye_plant, ByTileID.DyePlant},
                        {placeable.strange_plant, Groupings.StrangePlant},
                        // {placeable.block, REPLACE_ME},
                        // {placeable.brick, REPLACE_ME},
                        {placeable.ore, Groupings.Ore},
                        {placeable.bar, ByTileID.MetalBar},
                        // {placeable.wood, REPLACE_ME},
                        {placeable.wall, (i) => i.createWall > 0},
                        {placeable.wall_deco, Groupings.WallDeco},
                        {placeable.gem, ByTileID.Gem},
                        {placeable.musicbox, ByTileID.MusicBox},
                        {placeable.banner, ByTileID.Banner},
                    }
                },
                {Type.Ammo, new ConditionTable
                    {
                        {ammo.arrow, (i)    => i.ammo == 1},
                        {ammo.bullet, (i)   => i.ammo == 14},
                        {ammo.rocket, (i)   => i.ammo == 771},
                        {ammo.dart, (i)     => i.ammo == 51},
                        {ammo.sand, (i)     => i.ammo == 42},
                        {ammo.coin, (i)     => i.ammo == 71},
                        {ammo.solution, (i) => i.ammo == 780},
                        {ammo.endless, (i)  => i.ammo > 0 && !i.consumable},
                    }
                },
                {Type.Dye, new ConditionTable
                    {
                        {dye.basic, Dyes.BasicDyes},
                        {dye.black, Dyes.BlackDyes},
                        {dye.bright, Dyes.BrightDyes},
                        {dye.silver, Dyes.SilverDyes},
                        {dye.flame, Dyes.FlameDyes},
                        {dye.gradient, Dyes.GradientDyes},
                        {dye.strange, Dyes.StrangeDyes},
                        {dye.lunar, Dyes.LunarDyes},
                    }
                },
                {Type.Equip, new ConditionTable
                    {
                        // {equip.armor, REPLACE_ME},
                        {equip.accessory, (i) => i.accessory},
                        // {equip.pet, REPLACE_ME},
                        {equip.vanity, (i)    => i.vanity},
                        {equip.mount, Binary.isMount},
                        {equip.grapple, Binary.isHook},
                        {equip.slot_head, (i)    => i.headSlot > 0},
                        {equip.slot_body, (i)    => i.bodySlot > 0},
                        {equip.slot_leg, (i)     => i.legSlot > 0},
                        {equip.slot_face, (i)    => i.faceSlot > 0},
                        {equip.slot_neck, (i)    => i.neckSlot > 0},
                        {equip.slot_back, (i)    => i.backSlot > 0},
                        {equip.wings, (i)        => i.wingSlot > 0},
                        {equip.slot_shoe, (i)    => i.shoeSlot > 0},
                        {equip.slot_handon, (i)  => i.handOnSlot > 0},
                        {equip.slot_handoff, (i) => i.handOffSlot > 0},
                        {equip.slot_shield, (i)  => i.shieldSlot > 0},
                        {equip.slot_waist, (i)   => i.waistSlot > 0},
                        {equip.balloon, (i)      => i.balloonSlot > 0},
                        {equip.slot_front, (i)   => i.frontSlot > 0},
                        {equip.pet_light, Binary.isLightPet},
                        {equip.pet_vanity, Binary.isVanityPet},
                        // {equip.grapple_single, REPLACE_ME},
                        // {equip.grapple_multi, REPLACE_ME},
                        // {equip.mount_cart, REPLACE_ME},
                    }
                },
                {Type.Weapon, new ConditionTable
                    {
                        {weapon.automatic, (i) => i.autoReuse},
                        {weapon.type_melee, (i) => i.melee},
                        {weapon.has_projectile, (i) => i.shoot > 0},
                        {weapon.type_ranged, (i) => i.ranged},
                        {weapon.type_magic, (i) => i.magic},
                        {weapon.type_summon, (i) => i.summon},
                        {weapon.type_throwing, (i) => i.thrown},
                        // {weapon.weapon_other, REPLACE_ME},
                    }
                },
                {Type.WeaponMelee, new ConditionTable
                    {
                        {weapon.melee.style_swing, Weapons.Melee.Swing},
                        {weapon.melee.style_jab, Weapons.Melee.Jab},
                        {weapon.melee.style_directional, Weapons.Melee.Directional},
                        {weapon.melee.style_thrown, Weapons.Melee.Thrown},
                        {weapon.melee.broadsword, Weapons.Melee.BroadSword},
                        {weapon.melee.boomerang, Weapons.Melee.Boomerang},
                        {weapon.melee.spear, Weapons.Melee.Spear},
                        {weapon.melee.flail, Weapons.Melee.Flail},
                        {weapon.melee.yoyo, Weapons.Melee.Yoyo},
                    }
                },
                {Type.WeaponRanged, new ConditionTable
                    {
                        {weapon.ranged.bullet_consuming, Weapons.Ranged.BulletConsuming},
                        {weapon.ranged.arrow_consuming, Weapons.Ranged.ArrowConsuming},
                        {weapon.ranged.rocket_consuming, Weapons.Ranged.RocketConsuming},
                        {weapon.ranged.dart_consuming, Weapons.Ranged.DartConsuming},
                        {weapon.ranged.gel_consuming, Weapons.Ranged.GelConsuming},
                        {weapon.ranged.no_ammo, (i) => i.useAmmo < 0},
                    }
                },
                {Type.WeaponMagic, new ConditionTable
                    {
                        {weapon.magic.area, Weapons.Magic.Area},
                        {weapon.magic.homing, Weapons.Magic.Homing},
                        {weapon.magic.bouncing, Weapons.Magic.Bouncing},
                        {weapon.magic.controlled,Weapons.Magic.Controllable},
                        {weapon.magic.stream, Weapons.Magic.Stream},
                        {weapon.magic.piercing, Weapons.Magic.Piercing},
                    }
                },
                {Type.WeaponSummon, new ConditionTable
                    {
                        {weapon.summon.minion, Weapons.Summon.Minion},
                        {weapon.summon.sentry, Weapons.Summon.Sentry},
                    }
                },
                {Type.Tool, new ConditionTable
                    {
                        {tool.pick, (i)         => i.pick > 0},
                        {tool.axe, (i)          => i.axe > 0},
                        {tool.hammer, (i)       => i.hammer > 0},
                        {tool.fishing_pole, (i) => i.fishingPole > 0},
                        {tool.wand, (i)         => i.tileWand > 0},
                        {tool.wrench, Binary.isWrench},
                        // {tool.recall, REPLACE_ME},
                        // {tool.other, REPLACE_ME},
                    }
                },
                {Type.Consumable, new ConditionTable
                    {
                        {consumable.buff, Binary.timedBuff},
                        {consumable.food, Binary.isFood},
                        {consumable.potion, Binary.isPotion},
                        // {consumable.heal, REPLACE_ME},
                        // {consumable.regen, REPLACE_ME},
                        // {consumable.life, REPLACE_ME},
                        // {consumable.mana, REPLACE_ME},
                    }
                },
                {Type.Mech, new ConditionTable
                    {
                        {mech.timer, ByTileID.Timer},
                        // {mech.Switch, ByTileID.L},
                        {mech.trap, ByTileID.Trap},
                        {mech.track, (i) => i.cartTrack},
                        {mech.firework, ByTileID.Firework},
                        // {mech.lever, REPLACE_ME},
                        {mech.pressure_plate, ByTileID.PressurePlate},
                    }
                },
                {Type.Furniture, new ConditionTable
                    {
                        {furniture.valid_housing, Groupings.Furniture},
                        {furniture.housing_door, Groupings.housingDoor},
                        {furniture.housing_light, Groupings.housingTorch},
                        {furniture.housing_chair, Groupings.housingChair},
                        {furniture.housing_table, Groupings.housingTable},
                        // {furniture.clutter, REPLACE_ME},
                        {furniture.crafting_station, (i) => TileSets.CraftingStations.Contains(i.createTile)},
                        {furniture.container, ByTileID.Container},
                        // {furniture.useable, REPLACE_ME},
                        // {furniture.decorative, REPLACE_ME},
                    }
                },
                {Type.FurnitureDoor, new ConditionTable
                    {
                        {furniture.doors.door, ByTileID.Door},
                    }
                },
                {Type.FurnitureLight, new ConditionTable
                    {
                        {furniture.lighting.torch, ByTileID.Torch},
                        {furniture.lighting.candle, ByTileID.Candle},
                        {furniture.lighting.chandelier, ByTileID.Chandelier},
                        {furniture.lighting.hanging_lantern, ByTileID.HangingLantern},
                        {furniture.lighting.lamp, ByTileID.Lamp},
                        {furniture.lighting.holiday_light, ByTileID.HolidayLight},
                        {furniture.lighting.candelabra, ByTileID.Candelabra}
                    }
                },
                {Type.FurnitureTable, new ConditionTable
                    {
                        {furniture.tables.table, ByTileID.Table},
                        {furniture.tables.workbench, ByTileID.WorkBench},
                        {furniture.tables.dresser, ByTileID.Dresser},
                        {furniture.tables.piano, ByTileID.Piano},
                        {furniture.tables.bookcase, ByTileID.Bookcase},
                        {furniture.tables.bathtub, ByTileID.Bathtub},
                    }
                },
                {Type.FurnitureChair, new ConditionTable
                    {
                        {furniture.chairs.chair, ByTileID.Chair},
                        {furniture.chairs.bed, ByTileID.Bed},
                        {furniture.chairs.bench, ByTileID.Bench}
                    }
                },
                {Type.FurnitureOther, new ConditionTable
                    {
                        {furniture.other.sink, ByTileID.Sink},
                        {furniture.other.clock, ByTileID.GrandfatherClock},
                        {furniture.other.bottle, ByTileID.Bottle},
                        {furniture.other.bowl, ByTileID.Bowl},
                        {furniture.other.beachstuff, ByTileID.BeachPile},
                        {furniture.other.tombstone, ByTileID.Tombstone},
                        {furniture.other.campfire, ByTileID.Campfire},
                        {furniture.other.statue, ByTileID.Statue},
                        {furniture.other.statue_alphabet, ByTileID.AlphabetStatue},
                        {furniture.other.crate, ByTileID.FishingCrate},
                        {furniture.other.monolith, ByTileID.LunarMonolith},
                        {furniture.other.cooking_pot, ByTileID.CookingPot},
                        {furniture.other.anvil, ByTileID.Anvil},
                        {furniture.other.cannon, ByTileID.Cannon},
                        {furniture.other.planter, ByTileID.PlanterBox},
                        {furniture.other.fountain, ByTileID.WaterFountain},
                    }
                }
            };

            public static bool Check(ItemFlags.Type table, Item item, int flag)
            {
                return Matrix[table][flag](item);
            }
        }
    }
}
