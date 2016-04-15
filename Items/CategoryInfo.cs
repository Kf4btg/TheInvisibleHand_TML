// using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
// using System.Dynamic;
using Terraria;
// using Terraria.ID;
using Terraria.ModLoader;

namespace InvisibleHand.Items
{

    public struct FlagInfo
    {
        public int general;

        public int placeable;
        public int meetsHousingNeed;
        public long furniture;
        public long weapon;

        public int tool;
        public int ammo;

        public int equip;
        public int dye;
        public int consumable;
        public int mech;

    }

    /// store the information about an item's category here.
    public class CategoryInfo : ItemInfo
    {
        public FlagInfo Flags;
    }

    /// use this class to make the categorization checks cleaner/easier
    internal class ItemWithInfo
    {
        public Item item;
        public CategoryInfo info;

        // for the multi-tag operations, this can be checked to see if the operation was successful for that given scenario.
        /// Holds whether the most recent tagging attempt was successful
        /// (i.e. the trait's condition was satisfied)
        public bool Success { get; private set; }

        /// holds the trait that was most recently tagged
        // public Trait LastTag { get; private set; }
        public Tuple<string, long> LastFlag { get; private set; }

        public ItemWithInfo(Item item, CategoryInfo info)
        {
            this.item = item;
            this.info = info;
        }

        public ItemWithInfo(Item item)
        {
            this.item = item;
        }




        #region flagoverloads
        // It's really stupid that there seems to be no better way to
        // handle this situation in C#. There are OTHER ways, sure...
        // but none of them seem to be any better.

        /// tag this instance with the given trait;
        /// return the modified instance
        public ItemWithInfo SetFlag(ItemFlags.Type type, int flag)
        {
            switch (type)
            {
                case ItemFlags.Type.General:
                    info.Flags.general |= flag;
                    break;
                case ItemFlags.Type.Placeable:
                    info.Flags.placeable |= flag;
                    break;
                case ItemFlags.Type.Housing:
                    info.Flags.meetsHousingNeed |= flag;
                    break;
                case ItemFlags.Type.Tool:
                    info.Flags.tool |= flag;
                    break;
                case ItemFlags.Type.Ammo:
                    info.Flags.ammo |= flag;
                    break;
                case ItemFlags.Type.Equip:
                    info.Flags.equip |= flag;
                    break;
                case ItemFlags.Type.Consumable:
                    info.Flags.consumable |= flag;
                    break;
                case ItemFlags.Type.Dye:
                    info.Flags.dye |= flag;
                    break;
                case ItemFlags.Type.Mech:
                    info.Flags.mech |= flag;
                    break;

                /// because someone could pass an int while still referring
                /// to these flag types, check them here, too:
                case ItemFlags.Type.Furniture:
                    info.Flags.furniture |= (long)flag;
                    break;
                case ItemFlags.Type.Weapon:
                    info.Flags.weapon |= (long)flag;
                    break;
            }
            this.Success = true;
            return this;
        }

        public ItemWithInfo SetFlag(ItemFlags.Type type, long flag)
        {

            switch (type)
            {
                case ItemFlags.Type.Furniture:
                    info.Flags.furniture |= flag;
                    break;
                case ItemFlags.Type.Weapon:
                    info.Flags.weapon |= flag;
                    break;
            }
            this.Success = true;
            return this;
        }

        #endregion flagoverloads

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

        /// tag this instance with the given trait iff the condition
        /// for the trait (as found in the Condition Table) is true;
        ///return the instance, whether modified or not.
        public ItemWithInfo Flag(ItemFlags.Type type, long flag)
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

        /// tag this instance with the given trait
        /// IFF condition is true; return the instance,
        /// modified or not.
        public ItemWithInfo FlagIf(bool condition, ItemFlags.Type type, long flag)
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

        /// goes through the list of traits in the params list, attempting to tag each one; when a tag is successful,
        /// return without checking the remaining. Should be used for mutually-exclusive traits of the same
        /// ItemFlags type.
        public ItemWithInfo FlagFirst(ItemFlags.Type type, params long[] flags)
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

        /// attempts to tag each of the traits given in the params list
        /// without regard to the success of each tag operation.
        /// Can be used to try tagging related but not mutually-exclusive traits
        /// of the same ItemFlags type.
        public ItemWithInfo FlagAny(ItemFlags.Type type, params long[] flags)
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

        /// convenience function for checking the value of the .Success
        /// attribute after a Flag() operation
        public bool TryFlag(ItemFlags.Type type, long flag)
        {
            return this.Flag(type, flag).Success;
        }
    }
}
