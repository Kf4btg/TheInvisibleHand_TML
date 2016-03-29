// using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
// using TAPI;
using Terraria.ID;

namespace InvisibleHand
{
    public static class CategoryInit
    {
        public static IList<Category> Categories = new List<Category>();


        // Item classification hierarchy:
        //  Kingdom     (e.g. Useable)
        //      |-Class     (e.g. Weapon)
        //          |-Order     (e.g. Melee)
        //              |-Family    (e.g. Sword)
        //                  |-Genus     (e.g. BroadSword)
        //                      |-Species   (e.g. Silver Sword)
        //


        public static Category Useable, Placeable;
        public static Category Weapon, Tool, Accessory, Material, Consumable, Armor, Tile;

        // TOOLS
        //
        // The following are all also weapons:
        public static Category Pick, Pickaxe, Drill;
        public static Category Axe, Handaxe, Chainsaw;
        public static Category Hammer, Sledge, Jackhammer;

        public static Category Hook, SingleHook, MultiHook;

        public static Category Wrench; // includes wire cutter

        public static Category PaintingTool; // brushes, rollers, scrapers

        public static Category FishingPole;

        public static Category Wand; // Leaf, Hive, Bone, etc.

        public static Category Keys;
        public static Category Mount, LightPets;

        public static Category MiscTools; // buckets, mirrors, rope, etc.


        // WEAPONS
        public static Category Melee;
        // alias:  stab      swing  dir.stab  throw   spin
        public static Category Shortsword, Sword, Spear, Boomerang, Flail, Yoyo;

        public static Category Ranged;
        public static Category Guns, ManualGuns, AutomaticGuns;
        public static Category Archery, Bows, Repeaters;
        public static Category Launchers; // Rockets
        public static Category Dart, Flamethrower;
        public static Category MiscRanged; // flaregun, sandgun, toxikarp, ...

        public static Category Magic;
        public static Category Direct, Staves; // staves=amethyst/topaz...staff
        public static Category Area;
        public static Category Homing;
        public static Category Bouncing;
        public static Category Controlled;

        //
        // public static Weapon = new Category("Weapon", 0,
        //     (item) => item.damage > 0 && item.ammo<1,
        //     (i1, i2) => i1.damage.CompareTo(i2.damage)
        // );

        public static void Initialize()
        {
            
        }

    }

}
