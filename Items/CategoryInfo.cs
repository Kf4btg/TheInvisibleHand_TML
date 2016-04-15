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

    }

    /// store the information about an item's category here.
    public class CategoryInfo : ItemInfo
    {
        // public List<int> categories = new List<int>();
        //
        // TODO: create enums or use ints for the categories
        // public List<string> categories = new List<string>();
        // private ISet<Trait> traits;

        // public ISet<Trait> Traits
        // {
        //     get
        //     {
        //         return traits ?? new HashSet<Trait>();
        //     }
        //     set
        //     {
        //         if (traits==null) traits = value;
        //     }
        // }

        public FlagInfo Flags;

        // public BitArray TraitArray = new BitArray((int)Trait.COUNT, false);
        //
        // internal ItemFlags.general flags_general = ItemFlags.general.none;
        //
        // internal ItemFlags.placeable flags_placeable = ItemFlags.placeable.none;
        // internal ItemFlags.housing meetsHousingNeed = ItemFlags.housing.none;
        // internal ItemFlags.furniture flags_furniture = ItemFlags.furniture.none;
        //
        // internal ItemFlags.weapon flags_weapon = ItemFlags.weapon.none;
        // internal ItemFlags.tool flags_tool = ItemFlags.tool.none;
        // internal ItemFlags.ammo flags_ammo = ItemFlags.ammo.none;
        //
        // internal ItemFlags.equip flags_equip = ItemFlags.equip.none;
        // internal ItemFlags.dye flags_dye = ItemFlags.dye.none;
        // internal ItemFlags.consumable flags_consumable = ItemFlags.consumable.none;



        // public void AddTrait(Trait t)
        // {
        //     Traits.Add(t);
        // }
        // public void AddTraits(params Trait[] ts)
        // {
        //     Traits.UnionWith(ts);
        // }

        // public bool AddTrait(string trait_name)
        // {
        //     return Traits.Add(trait_name);
        //     // Trait t;
        //     // if (Enum.TryParse(trait_name, out t))
        //     // traits.Add(t);
        // }
        //
        // public void AddTraits(string[] trait_names)
        // {
        //
        //     Traits.UnionWith(trait_names);
        //
        //     // foreach (var tn in trait_names)
        //     // {
        //     //     Trait t;
        //     //     if (Enum.TryParse(tn, out t))
        //     //         traits.Add(t);
        //     // }
        // }

    }

    /// use this class to make the categorization checks cleaner/easier
    internal class ItemWithInfo
    {
        public Item item;
        public CategoryInfo info;

        private static IDictionary<string, IList<Enum>> traitstore;

        // for the multi-tag operations, this can be checked to see if the operation was successful for that given scenario.
        /// Holds whether the most recent tagging attempt was successful
        /// (i.e. the trait's condition was satisfied)
        public bool Success { get; private set; }

        /// holds the trait that was most recently tagged
        // public Trait LastTag { get; private set; }
        public Tuple<string, long> LastFlag { get; private set; }

        // public ISet<Trait> TraitList { get; private set; }

        // private IDictionary<Trait, Func<Item, bool>> conditionTable;

        public ItemWithInfo(Item item, CategoryInfo info)
        {
            this.item = item;
            this.info = info;
            // this.conditionTable = Rules.TConditionTable;
            // this.TraitList = new HashSet<Trait>();
        }

        public ItemWithInfo(Item item)
        {
            this.item = item;
            // this.conditionTable = Rules.TConditionTable;
            // this.TraitList = new HashSet<Trait>();
        }

        /// tag this instance with the given trait;
        /// return the modified instance
        // public ItemWithInfo AddTag(Trait trait)
        // {
        //     TraitList.Add(trait);
        //
        //     // if we've gotten here, then the condition check (if any)
        //     // was successful, so we set LastResult=true
        //     Success = true;
        //     return this;
        // }


        #region flagoverloads
        // It's really stupid that there seems to be no better way to
        // handle this situation in C#. There are OTHER ways, sure...
        // but none of them seem to be any better.


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


        public ItemWithInfo Flag(ItemFlags.Type type, int flag)
        {
            Success = false; // reset
            if (Rules.Conditions.Check(type, item, flag))
                return SetFlag(type, flag);
            return this;
        }

        public ItemWithInfo Flag(ItemFlags.Type type, long flag)
        {
            Success = false; // reset
            if (Rules.Conditions.Check(type, item, flag))
                return SetFlag(type, flag);
            return this;
        }

        public ItemWithInfo FlagIf(bool condition, ItemFlags.Type type, int flag)
        {
            Success = false; // reset
            return condition ? this.SetFlag(type, flag) : this;
        }

        public ItemWithInfo FlagIf(bool condition, ItemFlags.Type type, long flag)
        {
            Success = false; // reset
            return condition ? this.SetFlag(type, flag) : this;
        }

        public ItemWithInfo FlagFirst(ItemFlags.Type type, params int[] flags)
        {
            foreach (var f in flags)
            {
                if (this.Flag(type, f).Success)
                    break;
            }
            return this;
        }

        public ItemWithInfo FlagFirst(ItemFlags.Type type, params long[] flags)
        {
            foreach (var f in flags)
            {
                if (this.Flag(type, f).Success)
                    break;
            }
            return this;
        }

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

        public bool TryFlag(ItemFlags.Type type, int flag)
        {
            return this.Flag(type, flag).Success;
        }

        public bool TryFlag(ItemFlags.Type type, long flag)
        {
            return this.Flag(type, flag).Success;
        }


        /// tag this instance with the given trait iff the condition
        /// for the trait (as found in the Condition Table) is true;
        ///return the instance, whether modified or not.
        // public ItemWithInfo Tag(Trait trait)
        // {
        //     Success = false; // reset
        //     return conditionTable[trait](item) ? this.AddTag(trait) : this;
        // }
        //
        // /// tag this instance with the given trait
        // /// IFF condition is true; return the instance,
        // /// modified or not.
        // public ItemWithInfo TagIf(bool condition, Trait trait)
        // {
        //     Success = false;
        //     return condition ? this.AddTag(trait) : this;
        // }
        //
        // /// goes through the list of traits in the params list, attempting to tag each one; when a tag is successful,
        // /// return without checking the remaining. Should be used for mutually-exclusive traits.
        // public ItemWithInfo TagFirst(params Trait[] traits)
        // {
        //     foreach (var trait in traits)
        //     {
        //         if (this.Tag(trait).Success)
        //             break;
        //     }
        //     return this;
        // }
        //
        // /// attempts to tag each of the traits given in the params list
        // /// without regard to the success of each tag operation.
        // /// Can be used to try tagging related but not mutually-exclusive traits.
        // public ItemWithInfo TagAny(params Trait[] traits)
        // {
        //     bool res = false;
        //     foreach (var trait in traits)
        //         res |= this.Tag(trait).Success;
        //
        //     // we want to know if any of the operations succeeded, not just
        //     // the most recent one, so we catch any True value in res
        //     Success = res;
        //     return this;
        // }
        //
        // #region bool returns
        //
        // public bool TryTag(Trait trait)
        // {
        //     return this.Tag(trait).Success;
        // }
        //
        // #endregion bool returns

    }




}
