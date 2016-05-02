using System.Collections.Generic;
using System.Linq;
// using System;
// using System.Reflection;
using Terraria;
using Terraria.ID;
using System.Text.RegularExpressions;
// using InvisibleHand.Utils;

namespace InvisibleHand.Items
{
    public static class ClassificationRules
    {

        // static bool contains(IEnumerable<int> idlist, int id) => id > 0 && idlist.Contains(id);
        /// helper method to avoid index out of range exceptions
        static bool contains(IList<bool> idlist, int id) => id > 0 && idlist[id];

        /// prevents attempting to index Main.projectile w/ -1
        /// Also, it seems that a projectile doesn't actually HAVE an aiStyle (or much else)
        /// until it exists; thus, this will always return false (because aiStyle will be 0)
        /// unless the given projectile...
        /// I don't know if it just has to have its defaults set once? or if every time a new
        /// proj of this type is created it gets run through setdefaults. But either way, we
        /// need a populated projectile to make this work.
        public static bool TestProjectileAI(int projid, int style_id)
        {
            if (projid < 1) return false;

            var proj = Main.projectile[projid];

            proj.SetDefaults(projid);
            return proj.aiStyle == style_id;
        }

        /*
        ██████  ██ ███    ██  █████  ██████  ██    ██
        ██   ██ ██ ████   ██ ██   ██ ██   ██  ██  ██
        ██████  ██ ██ ██  ██ ███████ ██████    ████
        ██   ██ ██ ██  ██ ██ ██   ██ ██   ██    ██
        ██████  ██ ██   ████ ██   ██ ██   ██    ██
        */

        /// Every method/property in Rules.Binary is a simple yes/no (boolean) query
        internal static class Binary
        {
            public static bool isWeapon(Item item) => (item.damage > 0 && (!item.notAmmo || item.useStyle > 0));
            public static bool isArmor(Item item)  => !item.vanity && (item.headSlot > 0 || item.bodySlot > 0 || item.legSlot > 0);

            // also includes the wire cutter
            public static bool isWrench(Item item) => item.mech && item.tileBoost == 20;

            public static bool isHook(Item item)     => contains(Main.projHook, item.shoot);
            public static bool isMount(Item item)    => item.mountType != -1;
            public static bool isLightPet(Item item) => contains(Main.lightPet, item.buffType);

            public static bool isVanityPet(Item item) => contains(Main.vanityPet, item.buffType);

            public static bool isEquipable(Item item) => (item.headSlot > 0 || item.bodySlot > 0 || item.legSlot > 0 || item.accessory || contains(Main.projHook, item.shoot) || item.mountType != -1 || (item.buffType > 0 && (Main.lightPet[item.buffType] || Main.vanityPet[item.buffType])));

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


            public static bool isConsumable(Item item) => !(isAmmo(item) || CanBePlaced(item)) && item.consumable;
            public static bool timedBuff(Item item) => item.buffTime > 0;
            public static bool isFood(Item item)    => item.buffType == BuffID.WellFed; //26

            public static bool isFlask(Item item) => contains(Main.meleeBuff, item.buffType);

            public static bool isPotion(Item item) => item.useStyle == 2 && !isFood(item) && !isFlask(item);
            // /*timedBuff(item) && !isFood(item);*/ item.potion; // -> not always helpful

            // it seems that Item.potion is actually the flag that tells the game when to enforce the potion cooldown
            // (meaning that only consumables with a healLife effect get it)
            public static bool HealingPotion(Item item) => item.potion;


            public static bool falling(Item item)   => contains(TileID.Sets.Falling, item.createTile);

            public static bool Explosive(Item item) => TestProjectileAI(item.shoot, Constants.ProjectileAI.Explosive);

            public static bool oneDropYoyo(Item item) => new[] { 3315, 3316, 3317, 3262, 3282, 3283, 3284, 3285, 3286, 3389 }.Contains(item.type);


            /// this returns true for all the '... Rope' items (web, silk, regular, etc) AND their coil-counterparts
            public static bool makesRope(Item item) => contains(Main.tileRope, item.createTile) || TestProjectileAI(item.shoot, Constants.ProjectileAI.RopeCoil);

            /// anything that produces any level of light, even a weak one
            public static bool givesLight(Item item) => contains(Main.tileLighted, item.createTile);

            public static bool showsOnMetalDetector(Item item) => item.createTile > 0 && Main.tileValue[item.createTile] > 0;

            /// due to...difficulties, we  have to special-case the Material category
            public static bool isMaterial(Item item)
            {
                // if this isn't even true, get out of here
                // if (!item.material) return false;

                // any item that is a potion but NOT an alchemy ingredient will not be listed as a material;
                // Justification:
                //      Lesser healing potion has item.material & item.potion both set to true.
                //      So does the common (orange) mushroom.
                //      However, do the way we've defined AlchemyIngredients (see the ItemSets class for info on that),
                //      only the mushroom is also an AlchemyIngredient.
                //      Thus, with this rule in place, "mushroom" will fall into the Material category
                //      and then be further categorized as an alchemy ingredient, while the
                //      "lesser healing potion" will go on to be categorized elsewhere (like Consumable.Potion).
                // Which is exactly what we want.
                // if (item.potion && !Sets.AlchemyIngredient(item)) return false;

                // return true;

                // condense the above into a single statement
                // return item.material && !(item.potion && !Sets.AlchemyIngredient(item));

                // change item.potion (only true for heal-life potions) to item.usestyle == 2
                // (true for food, potions, and flasks, none of which we want here)
                return item.material && !(item.useStyle == 2 && !Sets.AlchemyIngredient(item));
            }
        }

        /// For when there's just about no other way to do it...(except for matching type ids,
        /// but I'd rather not do that, either)
        internal static class NameMatch
        {
            internal static bool match(Item item, string pattern)
            {
                return Regex.Match(item.name, pattern).Success;
            }

            internal static bool match_end(Item item, string substr)
            {
                return item.name.EndsWith(substr);
            }

            // include the Super Absorbant sponge
            public static bool Bucket(Item item) => match(item, @"(Bucket|Sponge)$");

            // these seems to match "Paintbrush" and "Paint [Scraper|Roller]", including "Spectre Paint..."
            // but not "Painting" or "<Color> Paint". Excellent.
            public static bool PaintTool(Item item) => match(item, @"(Paint(\s|brush))");
        }

        /*
        ██████  ██    ██ ████████ ██ ██      ███████ ██ ██████
        ██   ██  ██  ██     ██    ██ ██      ██      ██ ██   ██
        ██████    ████      ██    ██ ██      █████   ██ ██   ██
        ██   ██    ██       ██    ██ ██      ██      ██ ██   ██
        ██████     ██       ██    ██ ███████ ███████ ██ ██████
        */

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

            public static bool Anvil(Item item)            => item.createTile == TileID.Anvils;
            public static bool CookingPot(Item item)       => item.createTile == TileID.CookingPots;
            public static bool Sink(Item item)             => item.createTile == TileID.Sinks;
            public static bool GrandfatherClock(Item item) => item.createTile == TileID.GrandfatherClocks;
            public static bool Cannon(Item item)           => item.createTile == TileID.Cannon;
            public static bool Campfire(Item item)         => item.createTile == TileID.Campfire;

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

            /// green, blue, &amp; yellow rockets
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

        /*
        ███████ ███████ ████████ ███████
        ██      ██         ██    ██
        ███████ █████      ██    ███████
             ██ ██         ██         ██
        ███████ ███████    ██    ███████
        */

        internal static class Sets
        {
            #region from Sets

            /// These contain an overloaded Contains() method that checks the appropriate Item property
            /// (either item.createTile or item.type, depending on whether it's a set of TileIDs or ItemIDs)
            public static bool Furniture(Item item) => ItemSets.Furniture.Contains(item);
            public static bool CraftingStation(Item item) => ItemSets.CraftingStations.Contains(item);
            public static bool AlchemyIngredient(Item item) => ItemSets.AlchemyIngredients.Contains(item);
            public static bool Wood(Item item) => ItemSets.Wood.Contains(item);


            #endregion

            public static bool housingDoor(Item item) =>
                    TileID.Sets.RoomNeeds.CountsAsDoor.Contains(item.createTile);
            public static bool housingTable(Item item) =>
                    TileID.Sets.RoomNeeds.CountsAsTable.Contains(item.createTile);
            public static bool housingChair(Item item) =>
                    TileID.Sets.RoomNeeds.CountsAsChair.Contains(item.createTile);
            public static bool housingTorch(Item item) =>
                    TileID.Sets.RoomNeeds.CountsAsTorch.Contains(item.createTile);

            //TODO: consider:
            // Main.tileTable[] // seems to be things that you can walk on like a platform
            // Main.tileHammer[] -> shadow orbs & demon altars
            // tileAxe[] // choppable things, like trees, cacti, big shrooms...
            // tileStone[] // things that look like stone, i guess (gems & active-stone)
            // tileSand // ...sand.
            // tileFlame // things what are on fire (mainly candles)
            // tileAlch[] // immatureherbs (seeds), + mature/blooming which aren't applicable here
            // tileCut[] // can be cut with weapon; e.g. grass, lifefruit, bee larva, etc.
            // tileContainer[] => containers! looks like it contains TileID.Container && TileID.Dresser
            // tileSign[] => signs, tombstones
            // tileLighted[] => tiles that produce light (of some sort, even weakly)
            // tileDungeon[] => blue, green, & pink dungeon brick
            // tileSpelunker[] => glows when spelunker potion active
            // tileBouncy[] => just pinkslimeblock
            // int[] tileValue[] => tiles shown by the metal detector are given a 'value' that determines which override which;
            //                      contains (most) ores, hearts, pots, chests, lifefruit
            //
            // tileLargeFrames[]
            // wallLargeFrames[]
            //
            // wallHouse[]
            // wallDungeon[]
            // wallLight[]
            // wallBlend[]
            //
            // npcCatchable
            //
            // bool Main.critterCage;
            //
            // int[] anglerQuestItemNetIDs
            //
            // public static Tile[,] tile = new Tile[Main.maxTilesX, Main.maxTilesY];
            //
            // ...
            // Maybe more!
            //

            //

            // public static bool Block(Item item) => Main.tileBrick[item.createTile];

            #region blocktypes
            /// despite the name of the array using the word 'brick', this appears to hold most
            /// items that could be considered a 'block', including bricks, but also e.g. mud, wood, grass-seeds,
            /// glass, etc. It also includes sands, but notably seems to missing DIRT...
            public static bool Block(Item item) => contains(Main.tileBrick, item.createTile) || item.createTile == 0;



            public static bool Sand(Item item) => contains(Main.tileSand, item.createTile);
            public static bool DungeonBrick(Item item) => contains(Main.tileDungeon, item.createTile);
            public static bool BouncyBlock(Item item) => contains(Main.tileBouncy, item.createTile);

            public static bool Ore(Item item) => contains(TileID.Sets.Ore, item.createTile);
            public static bool Ice(Item item) => contains(TileID.Sets.Ices, item.createTile);


            public static bool HallowBlock(Item item)     => contains(TileID.Sets.Hallow, item.createTile);
            public static bool CrimsonBlock(Item item)    => contains(TileID.Sets.Crimson, item.createTile);
            public static bool CorruptionBlock(Item item) => contains(TileID.Sets.Corrupt, item.createTile);

            // public static bool Sand(Item item)          => contains(TileID.Sets.Conversion.Sand, item.createTile);
            public static bool HardenedSand (Item item) => contains(TileID.Sets.Conversion.HardenedSand, item.createTile);
            public static bool Sandstone(Item item)     => contains(TileID.Sets.Conversion.Sandstone, item.createTile);

            public static bool Stone(Item item) => contains(TileID.Sets.Conversion.Stone, item.createTile);
            #endregion

            public static bool CanPlaceOnWall(Item item) => contains(TileID.Sets.FramesOnKillWall, item.createTile);

            public static bool Rope(Item item) => contains(Main.tileRope, item.createTile);
            public static bool RopeCoil(Item item) => TestProjectileAI(item.shoot, Constants.ProjectileAI.RopeCoil);

            public static bool StrangePlant(Item item) => contains(ItemID.Sets.ExoticPlantsForDyeTrade, item.type);

            /// Yoraizor's stuff is also in this item set, but will likely get caught by an 'isAccessory'
            /// check or something before it gets here.
            public static bool Soul(Item item) => contains(ItemID.Sets.AnimatesAsSoul, item.type);
            /// Seems the Trophies fall in these categories, too. All the vanilla ones, at
            /// least, also end in "Trophy", so that could be a way to distinguish them;
            /// AND the decorative racks (blacksmith, helmet, spear, etc.), are here too;
            /// they all seem to end in "Rack"; Some other decorative wall hangings (like
            /// Ship's wheel, Compass Rose, etc.), will likely be difficult to distinguish
            /// from paintings, though.
            private static readonly int[] wall_deco_tileIDs = new int[] { TileID.Painting3X3, TileID.Painting4X3, TileID.Painting6X4, TileID.Painting2X3, TileID.Painting3X2 };
            public static bool WallDeco(Item item) => wall_deco_tileIDs.Contains(item.createTile);

            // public static bool Painting3X3(Item item) => item.createTile == TileID.Painting3X3;
            // public static bool Painting4X3(Item item) => item.createTile == TileID.Painting4X3;
            // public static bool Painting6X4(Item item) => item.createTile == TileID.Painting6X4;
            // public static bool Painting2X3(Item item) => item.createTile == TileID.Painting2X3;
            // public static bool Painting3X2(Item item) => item.createTile == TileID.Painting3X2;

            public static bool NebulaPickup(Item item) => contains(ItemID.Sets.NebulaPickup, item.type);

            public static bool GoesInExtractinator(Item item) => ItemID.Sets.ExtractinatorMode.Contains(item.type);

            #region npcs

            public static bool Critter(Item item) => item.makeNPC != 0;
            public static bool Butterfly(Item item) => item.makeNPC == 356;

            #endregion
        }

        /*
        ██████  ██    ██ ███████ ███████
        ██   ██  ██  ██  ██      ██
        ██   ██   ████   █████   ███████
        ██   ██    ██    ██           ██
        ██████     ██    ███████ ███████
        */

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
            public static bool BlackDyes(Item item) =>  black_dye_ids.Contains(item.type);

            public static bool BrightDyes(Item item) => bright_dye_ids.Contains(item.type);

            /// the "... and Silver" dyes
            public static bool SilverDyes(Item item) =>  silver_dye_ids.Contains(item.type);

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

        /*
        ██     ██ ███████  █████  ██████   ██████  ███    ██ ███████
        ██     ██ ██      ██   ██ ██   ██ ██    ██ ████   ██ ██
        ██  █  ██ █████   ███████ ██████  ██    ██ ██ ██  ██ ███████
        ██ ███ ██ ██      ██   ██ ██      ██    ██ ██  ██ ██      ██
         ███ ███  ███████ ██   ██ ██       ██████  ██   ████ ███████
        */

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
                public static bool Boomerang(Item item) => TestProjectileAI(item.shoot, Constants.ProjectileAI.Boomerang);

                /// could be combined with flail
                public static bool ChainWeapon(Item item) => TestProjectileAI(item.shoot, Constants.ProjectileAI.Chained);


                //***
                //Dependent on Swing()==true
                //***
                /// also includes some things like Purple Clubberfish, slap hand, etc., that aren't
                /// technically swords, but are swung overhead &amp; there's really no way to tell them apart...
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
                public static bool Spear(Item item) => TestProjectileAI(item.shoot, Constants.ProjectileAI.Spear);

                // all (vanilla) flails (other than Flairon, which is just...weird) use animations 40 or 45
                // NOTE: also, flails have a projectile id (item.shoot) of 15;
                // public static bool Flail(Item item) => /*Directional(item) && */ (item.useAnimation == 40 || item.useAnimation == 45);
                public static bool Flail(Item item) => TestProjectileAI(item.shoot, Constants.ProjectileAI.Flail);

                // Yoyos; the 'usetime' check is necessary to distinguish from the Arkhalis
                // public static bool Yoyo(Item item) => /*Directional(item) &&*/ (item.channel && item.useAnimation == 25 && item.useTime == 25);
                public static bool Yoyo(Item item) => TestProjectileAI(item.shoot, Constants.ProjectileAI.Yoyo);
            }


            internal static class Ranged
            {
                public static bool ArrowConsuming(Item item)  => item.useAmmo == Constants.AmmoID.Arrow;
                public static bool BulletConsuming(Item item) => item.useAmmo == Constants.AmmoID.Bullet;
                public static bool RocketConsuming(Item item) => item.useAmmo == Constants.AmmoID.Rocket;
                public static bool DartConsuming(Item item)   => item.useAmmo == Constants.AmmoID.Dart;
                public static bool GelConsuming(Item item)    => item.useAmmo == Constants.AmmoID.Gel;

                public static bool NoAmmo(Item item) => item.useAmmo < 1;

                /// this isn't entirely accurate...some of the highlevel bows also have auto-reuse set True;
                /// it's probably the best we can do, though.
                public static bool Repeater(Item item) => ArrowConsuming(item) && item.autoReuse;
                public static bool AutomaticGun(Item item) => BulletConsuming(item) && item.autoReuse;

                public static bool FlameThrower(Item item) => TestProjectileAI(item.shoot, Constants.ProjectileAI.FlameThrower);

            }

            internal static class Magic
            {
                public static bool Homing(Item item) => contains(ProjectileID.Sets.Homing, item.shoot);

                public static bool Area(Item item)
                {
                    if (item.shoot < 1) return false;
                    var projectile = Main.projectile[item.shoot];
                    return projectile.maxPenetrate < 0 && !projectile.tileCollide;
                }

                // e.g. magic missile
                public static bool Controllable(Item item) => TestProjectileAI(item.shoot, Constants.ProjectileAI.FollowCursor);
                // e.g. water bolt
                public static bool Bouncing(Item item)
                {
                    if (item.shoot < 1) return false;
                    var projectile = Main.projectile[item.shoot];
                    return projectile.aiStyle == Constants.ProjectileAI.HyperBounce ||
                           projectile.aiStyle == Constants.ProjectileAI.HeavyBounce;
                }

                // e.g. aqua scepter
                public static bool Stream(Item item) => TestProjectileAI(item.shoot, Constants.ProjectileAI.Stream);
                // e.g. a lot of things; anything that passes through at least one enemy
                public static bool Piercing(Item item) => item.shoot > 0 && Main.projectile[item.shoot].maxPenetrate > 1;

                // i.e. straight(ish) line that passes through blocks
                public static bool VilethornAI(Item item) => TestProjectileAI(item.shoot, Constants.ProjectileAI.Vilethorn);
            }

            internal static class Summon
            {
                public static bool Minion(Item item) => item.shoot > 0 && Main.projectile[item.shoot].minion;

                // there's nothing like a 'sentry' field, but that's all that's left after taking
                // out the 'minion' weapons, so...
                public static bool Sentry(Item item) => !Minion(item);

            }

        }

        internal static class MiscTools
        {
            // bugnet.useanimation == 25; golden bugnet.useanuimation == 18
            // this is ridiculous...
            public static bool BugNet(Item item) => (new[] { 25, 18 }).Contains(item.useAnimation) && item.useStyle == 1 && item.useSound == 1 && item.shoot < 1 && item.damage < 1 && !item.noMelee;

            /// TODO: get some sleep and come up with a better name than this...
            /// This is intended to cover the umbrella and breathing reed
            public static bool OverheadThings(Item item) => item.holdStyle == 2;

            /// the name thing will hopefully be enough, but let's add the consumable check just to make sure.
            // For reference, also:
            // useStyle = 1, useTurn = True, useAnimation = 15,
            // useTime = 10, autoReuse = True, value=10000
            public static bool PaintingTools(Item item) => NameMatch.PaintTool(item) && !item.consumable;

            // torches, candles, glowsticks, flare gun...unicorn on a stick...marshmallow on a stick...
            // nebula arcanum...
            // more features: glowstick/flare gun have a 'shoot' value; torches, candles, gsticks are consumable;
            // torches, candles & glowstich have useanimation of 15; flare is 18;
            //
            // // for now, just includes glowsticks and flare gun. Maybe torches should go here too?
            public static bool HandLights(Item item) => item.holdStyle == 1 && item.shoot > 0 && !item.magic;

            /// magic mirrors, cell phone
            /// note: the mirrors, phone, and other items that teleport the player
            /// have useSound=6;
            public static bool Recall(Item item) => item.useAnimation == 90;

            /// Bombs, Dynamite...
            public static bool Demolitions(Item item) => Binary.Explosive(item) && item.consumable && !item.thrown;

        }


        // Ideas for dealing with mod items:
        //  1) Use Recipe.ItemMatches() for added materials
        //

        //
        // /// C# just doesn't understand ducks...
        // internal static class Conditions
        // {
        //     public static readonly Dictionary<ItemFlags.Type, condition_table> Matrix =
        //     new Dictionary<ItemFlags.Type, condition_table>()

    }

    public enum WallDecoType
    {
        Painting,
        Trophy,
        Rack,
        Other
    }
}
