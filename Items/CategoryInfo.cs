// using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
// using System.Dynamic;
using Terraria;
// using Terraria.ID;
using Terraria.ModLoader;

namespace InvisibleHand.Items
{
    using static ItemFlags.Type;

    public struct FlagInfo
    {
        public int general;

        public int placeable;
        // public int meetsHousingNeed;

        public int weapon;
        public int w_melee;
        public int w_ranged;
        public int w_magic;
        public int w_summon;

        public int tool;
        public int ammo;

        public int equip;
        public int dye;
        public int consumable;
        public int mech;

        public int furniture;
        public int f_door;
        public int f_light;
        public int f_table;
        public int f_chair;
        public int f_other;

    }

    /// store the information about an item's category here.
    public class CategoryInfo : ItemInfo
    {
        public FlagInfo Flags;
    }

    /// use this class to make the categorization checks cleaner/easier
    internal class ItemWithInfo
    {
        public readonly Dictionary<ItemFlags.Type, int> FlagTracker = new Dictionary<ItemFlags.Type, int>
        {
            {General, 0},
            {Placeable,  0},
            {Ammo, 0},
            {Dye, 0},
            {Equip, 0},
            {Weapon, 0},
            {WeaponMelee, 0},
            {WeaponRanged, 0},
            {WeaponMagic, 0},
            {WeaponSummon, 0},
            {Tool, 0},
            {Consumable, 0},
            {Mech, 0},
            {Furniture, 0},
            {FurnitureDoor, 0},
            {FurnitureLight, 0},
            {FurnitureTable, 0},
            {FurnitureChair, 0},
            {FurnitureOther, 0},
        };

        public Item item;
        public CategoryInfo info;

        // for the multi-tag operations, this can be checked to see if the operation was successful for that given scenario.
        /// Holds whether the most recent tagging attempt was successful
        /// (i.e. the trait's condition was satisfied)
        public bool Success { get; private set; }

        /// holds the trait that was most recently tagged
        // public Trait LastTag { get; private set; }
        public Tuple<ItemFlags.Type, int> LastFlag { get; private set; }
        private Dictionary<ItemFlags.Type, Action<int>> Setters;

        public ItemWithInfo(Item item, CategoryInfo info)
        {
            this.item = item;
            this.info = info;
        }

        /// tag this instance with the given trait;
        /// return the modified instance
        public ItemWithInfo SetFlag(ItemFlags.Type type, int flag)
        {
            this.FlagTracker[type] |= flag;
            this.Success = true;
            this.LastFlag = Tuple.Create(type, this.FlagTracker[type]);
            return this;
        }

        /// tag this instance with the given trait iff the condition
        /// for the trait (as found in the Condition Table) is true;
        ///return the instance, whether modified or not.
        public ItemWithInfo Flag(ItemFlags.Type type, int flag)
        {
            Success = false; // reset
            if (ClassificationRules.Conditions.Check(type, item, flag))
                return SetFlag(type, flag);
            return this;
        }

        /// tag this instance with the given trait
        /// IFF condition is true; return the instance,
        /// modified or not.
        public ItemWithInfo FlagIf(bool condition, ItemFlags.Type type, int flag)
        {
            Success = false; // reset
            return condition ? this.SetFlag(type, flag) : this;
        }

        /// goes through the list of traits in the params list, attempting to tag each one; when a tag is successful,
        /// return without checking the remaining. Should be used for mutually-exclusive traits of the same
        /// ItemFlags type.
        public ItemWithInfo FlagFirst(ItemFlags.Type type, params int[] flags)
        {
            foreach (var f in flags)
            {
                if (this.Flag(type, f).Success)
                    break;
            }
            return this;
        }

        /// attempts to tag each of the traits given in the params list
        /// without regard to the success of each tag operation.
        /// Can be used to try tagging related but not mutually-exclusive traits
        /// of the same ItemFlags type.
        public ItemWithInfo FlagAny(ItemFlags.Type type, params int[] flags)
        {
            bool res = false;
            foreach (var f in flags)
                res |= this.Flag(type, f).Success;

            // we want to know if any of the operations succeeded, not just
            // the most recent one, so we catch any True value in res
            Success = res;
            return this;
        }

        /// convenience function for checking the value of the .Success
        /// attribute after a Flag() operation
        public bool TryFlag(ItemFlags.Type type, int flag)
        {
            return this.Flag(type, flag).Success;
        }

        /// set the values of the FlagInfo struct to the accumulated flag values
        public void Finish()
        {
            info.Flags.general    = this.FlagTracker[General];
            info.Flags.placeable  = this.FlagTracker[Placeable];
            info.Flags.consumable = this.FlagTracker[Consumable];
            info.Flags.mech       = this.FlagTracker[Mech];
            info.Flags.dye        = this.FlagTracker[Dye];
            info.Flags.equip      = this.FlagTracker[Equip];
            info.Flags.tool       = this.FlagTracker[Tool];
            info.Flags.ammo       = this.FlagTracker[Ammo];

            info.Flags.weapon     = this.FlagTracker[Weapon];
            info.Flags.w_melee    = this.FlagTracker[WeaponMelee];
            info.Flags.w_magic    = this.FlagTracker[WeaponMagic];
            info.Flags.w_ranged   = this.FlagTracker[WeaponRanged];
            info.Flags.w_summon   = this.FlagTracker[WeaponSummon];

            info.Flags.furniture  = this.FlagTracker[Furniture];
            info.Flags.f_door     = this.FlagTracker[FurnitureDoor];
            info.Flags.f_light    = this.FlagTracker[FurnitureLight];
            info.Flags.f_table    = this.FlagTracker[FurnitureTable];
            info.Flags.f_chair    = this.FlagTracker[FurnitureChair];
            info.Flags.f_other    = this.FlagTracker[FurnitureOther];
        }
    }
}
