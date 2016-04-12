// using System.Collections.Generic;
// using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using System.Text.RegularExpressions;
// using Terraria.ModLoader;
// using InvisibleHand.Utils;

namespace InvisibleHand.Items
{
    public static class Rules
    {
        /// Every method/property in Rules.Binary is a simple yes/no (boolean) query
        internal static class Binary
        {
            // this three are a bit redundant...
            public static bool questItem(Item item) => item.questItem;
            public static bool isMaterial(Item item) => item.material;
            public static bool expert(Item item) => item.expert;
            // could add item.vanity here

            public static bool isWeapon(Item item)
            {
                return (item.damage > 0 && (!item.notAmmo || item.useStyle > 0));
            }

            public static bool isFishingPole(Item item) => item.fishingPole > 0;
            public static bool isBait(Item item) => item.bait > 0;

            public static bool isHook(Item item) => Main.projHook[item.shoot];
            public static bool isMount(Item item) => item.mountType != -1;
            public static bool isLightPet(Item item) => item.buffType > 0 && (Main.lightPet[item.buffType]);

            public static bool isVanityPet(Item item) => item.buffType > 0 && (Main.vanityPet[item.buffType]);

            public static bool isEquipable(Item item) => (item.headSlot > 0 || item.bodySlot > 0 || item.legSlot > 0 || item.accessory || Main.projHook[item.shoot] || item.mountType != -1 || (item.buffType > 0 && (Main.lightPet[item.buffType] || Main.vanityPet[item.buffType])));

            public static bool isWand(Item item) => item.tileWand > 0;

            public static bool isPick(Item item) => item.pick > 0;
            public static bool isAxe(Item item) => item.axe > 0;
            public static bool isHammer(Item item) => item.hammer > 0;

            public static bool increasedReach(Item item) => item.tileBoost > 0;
            public static bool decreasedReach(Item item) => item.tileBoost < 0;

            public static bool healsLife(Item item) => item.healLife > 0;
            public static bool healsMana(Item item) => item.healMana > 0;

            public static bool costsMana(Item item) => item.mana > 0;

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

            public static bool isAmmo(Item item) => !CanBePlaced(item) && item.ammo > 0 && !item.notAmmo;
            public static bool isConsumable(Item item) => !isAmmo(item) && !CanBePlaced(item) && item.consumable;

            public static bool isFood(Item item) => item.buffType == 26;

            public static bool grantsBuff(Item item) => item.buffTime > 0;

            // maybe?
            public static bool isPotion(Item item) => grantsBuff(item) && !isFood(item);

            public static bool falling(Item item) => TileID.Sets.Falling[item.createTile];



            public static bool oneDropYoyo(Item item) => new[] { 3315, 3316, 3317, 3262, 3282, 3283, 3284, 3285, 3286, 3389 }.Contains(item.type);
        }

        /// these rules are dependent on Binary.CanBePlaced()
        internal static class TileIDGroups
        {

            #region roomneeds doors
    		public static bool Door(Item item) => item.createTile == TileID.ClosedDoor;
            public static bool Platform(Item item) => item.createTile == TileID.Platforms;
            #endregion roomneeds doors

            #region roomneeds chairs
            public static bool Chair(Item item) => item.createTile == TileID.Chairs;
            public static bool Bed(Item item) => item.createTile == TileID.Beds;
            /// also sofas
            public static bool Bench(Item item) => item.createTile == TileID.Benches;
            #endregion roomneeds chairs

            #region roomneeds tables
    		public static bool WorkBench(Item item) => item.createTile == TileID.WorkBenches;
            public static bool Table(Item item) => item.createTile == TileID.Tables;
            public static bool Piano(Item item) => item.createTile == TileID.Pianos;
            public static bool Dresser(Item item) => item.createTile == TileID.Dressers;
            public static bool Bookcase(Item item) => item.createTile == TileID.Bookcases;

            #endregion roomneeds tables

            /// wait, is this a table or a chair?
    		public static bool Bathtub(Item item) => item.createTile == TileID.Bathtubs;
            /// and what about this?
            public static bool Sink(Item item) => item.createTile == TileID.Sinks;


            #region lighting
            public static bool Candle(Item item) => item.createTile == TileID.Candles;
            public static bool Chandelier(Item item) => item.createTile == TileID.Chandeliers;
            public static bool HangingLantern(Item item) => item.createTile == TileID.HangingLanterns;
    		public static bool Lamp(Item item) => item.createTile == TileID.Lamps;
            public static bool Candelabra(Item item) => item.createTile == TileID.Candelabras;
            public static bool Torch(Item item) => item.createTile == TileID.Torches;

            #endregion lighting


            // chests, barrels, and trash can
            public static bool Container(Item item) => item.createTile == TileID.Containers;

            /// these are the seeds for the herbs
            public static bool ImmatureHerb(Item item) => item.createTile == TileID.ImmatureHerbs;
    		public static bool Tombstone(Item item) => item.createTile == TileID.Tombstones;

            /// important (and there's a lot of these...)
            public static bool Banner(Item item) => item.createTile == TileID.Banners;

            // includes cups and mugs
            public static bool Bottle(Item item) => item.createTile == TileID.Bottles;
            // only the lead anvil?
            public static bool Anvil(Item item) => item.createTile == TileID.Anvils;

            // only the cauldron?
            public static bool CookingPot(Item item) => item.createTile == TileID.CookingPots;


            /// three of them!
            public static bool Bowl(Item item) => item.createTile == TileID.Bowls;
    		public static bool GrandfatherClock(Item item) => item.createTile == TileID.GrandfatherClocks;

            /// also includes vases
            public static bool Statue(Item item) => item.createTile == TileID.Statues;
    		public static bool PressurePlate(Item item) => item.createTile == TileID.PressurePlates;

            /// only the lihzard traps (not the regular dart trap)
            public static bool Trap(Item item) => item.createTile == TileID.Traps;
    		public static bool MusicBox(Item item) => item.createTile == TileID.MusicBoxes;
    		public static bool Explosive(Item item) => item.createTile == TileID.Explosives;

            /// only 3 & 5
            public static bool Timer(Item item) => item.createTile == TileID.Timers;

            // red and green
            public static bool HolidayLight(Item item) => item.createTile == TileID.HolidayLights;
    		public static bool Gem(Item item) => item.createTile == TileID.ExposedGems;
    		public static bool WaterFountain(Item item) => item.createTile == TileID.WaterFountain;

            // confetti and bunny, but not the...cannon.
            public static bool Cannon(Item item) => item.createTile == TileID.Cannon;
    		public static bool Campfire(Item item) => item.createTile == TileID.Campfire;

            /// green, blue, & yellow rockets
            public static bool Firework(Item item) => item.createTile == TileID.Firework;

            /// NOTE: Strange Plants also have a 'rarity' of -11
            public static bool DyePlant(Item item) => item.createTile == TileID.DyePlants;
            public static bool MetalBar(Item item) => item.createTile == TileID.MetalBars;

            /// Seems the Trophies fall in these categories, too. All the vanilla ones, at
            /// least, also end in "Trophy", so that could be a way to distinguish them;
            /// AND the decorative racks (blacksmith, helmet, spear, etc.), are here too;
            /// they all seem to end in "Rack"; Some other decorative wall hangings (like
            /// Ship's wheel, Compass Rose, etc.), will likely be difficult to distinguish
            /// from paintings, though.
            public static bool WallDeco(Item item) => new int[]{ TileID.Painting3X3, TileID.Painting4X3, TileID.Painting6X4, TileID.Painting2X3, TileID.Painting3X2 }.Contains(item.createTile);
            // public static bool Painting3X3(Item item) => item.createTile == TileID.Painting3X3;
    		// public static bool Painting4X3(Item item) => item.createTile == TileID.Painting4X3;
    		// public static bool Painting6X4(Item item) => item.createTile == TileID.Painting6X4;
            // public static bool Painting2X3(Item item) => item.createTile == TileID.Painting2X3;
            // public static bool Painting3X2(Item item) => item.createTile == TileID.Painting3X2;

    		public static bool MinecartTrack(Item item) => item.createTile == TileID.MinecartTrack;

            /// "Starfish"
            public static bool BeachPile(Item item) => item.createTile == TileID.BeachPiles;

            /// ABC, 123
            public static bool AlphabetStatue(Item item) => item.createTile == TileID.AlphabetStatues;
    		public static bool FishingCrate(Item item) => item.createTile == TileID.FishingCrate;
    		public static bool PlanterBox(Item item) => item.createTile == TileID.PlanterBox;
            /// all 3 of them
            public static bool LunarMonolith(Item item) => item.createTile == TileID.LunarMonolith;
        }

        internal static class Groupings
        {
            public static bool isFurniture(Item item) => TileSets.Furniture.Contains(item.createTile);

            public static bool housingDoor(Item item) =>
                    TileID.Sets.RoomNeeds.CountsAsDoor.Contains(item.createTile);
            public static bool housingTable(Item item) =>
                    TileID.Sets.RoomNeeds.CountsAsTable.Contains(item.createTile);
            public static bool housingChair(Item item) =>
                    TileID.Sets.RoomNeeds.CountsAsChair.Contains(item.createTile);
            public static bool housingTorch(Item item) =>
                    TileID.Sets.RoomNeeds.CountsAsTorch.Contains(item.createTile);

            public static bool isOre(Item item) => TileID.Sets.Ore[item.createTile];
            public static bool isIce(Item item) => TileID.Sets.Ices[item.createTile];
        }

        /// narrows down a generic grouping (e.g. 'Weapon') to a more specific classifier (e.g. 'Melee')
        internal static class Types
        {
            /// The item should have been passed through Binary.isWeapon() before getting here,
            /// the result of this may not have much real meaning.
            public static WeaponType Weapon(Item item)
            {
                return item.melee   ? WeaponType.Melee  :
                        item.ranged ? WeaponType.Ranged :
                        item.magic  ? WeaponType.Magic  :
                        item.thrown ? WeaponType.Thrown :
                        item.summon ? WeaponType.Summon :
                        WeaponType.Other;
            }

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
    }
}
