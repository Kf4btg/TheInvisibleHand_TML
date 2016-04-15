// using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Collections;
using System;
// using System.Dynamic;
using Terraria;
// using Terraria.ID;
using Terraria.ModLoader;

namespace InvisibleHand.Items
{

    internal struct FlagInfo
    {
        internal ItemFlags.general general;

        internal ItemFlags.placeable placeable;
        internal ItemFlags.housing meetsHousingNeed;
        internal ItemFlags.furniture furniture;

        internal ItemFlags.weapon weapon;
        internal ItemFlags.tool tool;
        internal ItemFlags.ammo ammo;

        internal ItemFlags.equip equip;
        internal ItemFlags.dye dye;
        internal ItemFlags.consumable consumable;

    }

    /// store the information about an item's category here.
    public class CategoryInfo : ItemInfo
    {
        // public List<int> categories = new List<int>();
        //
        // TODO: create enums or use ints for the categories
        // public List<string> categories = new List<string>();
        private ISet<Trait> traits;

        public ISet<Trait> Traits
        {
            get
            {
                return traits ?? new HashSet<Trait>();
            }
            set
            {
                if (traits==null) traits = value;
            }
        }

        internal FlagInfo flags;

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



        public void AddTrait(Trait t)
        {
            Traits.Add(t);
        }
        public void AddTraits(params Trait[] ts)
        {
            Traits.UnionWith(ts);
        }

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
        public Trait LastTag { get; private set; }

        public ISet<Trait> TraitList { get; private set; }

        private IDictionary<Trait, Func<Item, bool>> conditionTable;

        public ItemWithInfo(Item item, CategoryInfo info)
        {
            this.item = item;
            this.info = info;
            this.conditionTable = Rules.TConditionTable;
            this.TraitList = new HashSet<Trait>();
        }

        public ItemWithInfo(Item item)
        {
            this.item = item;
            this.conditionTable = Rules.TConditionTable;
            this.TraitList = new HashSet<Trait>();
        }

        // public bool TryAddTrait(Func<Item, bool> check, string trait)
        // {
        //     if (check(item))
        //     {
        //         TraitList.Add(trait);
        //         return true;
        //     }
        //     return false;
        // }
        //
        // public bool TryAddTrait(bool condition, string trait)
        // {
        //     if (condition)
        //     {
        //         TraitList.Add(trait);
        //         return true;
        //     }
        //     return false;
        // }
        //
        // public void AddTrait(string trait)
        // {
        //     TraitList.Add(trait);
        // }

        /// tag this instance with the given trait;
        /// return the modified instance
        public ItemWithInfo AddTag(Trait trait)
        {
            TraitList.Add(trait);

            // if we've gotten here, then the condition check (if any)
            // was successful, so we set LastResult=true
            Success = true;
            return this;
        }

        public ItemWithInfo SetFlag(string type, Enum flag)
        {
            switch (type)
            {
                case "general":
                    info.flags.general |= (ItemFlags.general)flag;
                    break;
                case "placeable":
                    info.flags.placeable |= (ItemFlags.placeable)flag;
                    break;
                case "housing":
                    info.flags.meetsHousingNeed |= (ItemFlags.housing)flag;
                    break;
                case "furniture":
                    info.flags.furniture |= (ItemFlags.furniture)flag;
                    break;
                case "weapon":
                    info.flags.weapon |= (ItemFlags.weapon)flag;
                    break;
                case "tool":
                    info.flags.tool |= (ItemFlags.tool)flag;
                    break;
                case "ammo":
                    info.flags.ammo |= (ItemFlags.ammo)flag;
                    break;
                case "equip":
                    info.flags.equip |= (ItemFlags.equip)flag;
                    break;
                case "consumable":
                    info.flags.consumable |= (ItemFlags.consumable)flag;
                    break;
                case "dye":
                    info.flags.dye |= (ItemFlags.dye)flag;
                    break;

            }
            return this;
        }

        public ItemWithInfo Flag(Enum flag)
        {

            return this;
        }

        /// tag this instance with the given trait iff the condition
        /// for the trait (as found in the Condition Table) is true;
        ///return the instance, whether modified or not.
        public ItemWithInfo Tag(Trait trait)
        {
            Success = false; // reset
            return conditionTable[trait](item) ? this.AddTag(trait) : this;
        }

        /// tag this instance with the given trait
        /// IFF condition is true; return the instance,
        /// modified or not.
        public ItemWithInfo TagIf(bool condition, Trait trait)
        {
            Success = false;
            return condition ? this.AddTag(trait) : this;
        }

        /// goes through the list of traits in the params list, attempting to tag each one; when a tag is successful,
        /// return without checking the remaining. Should be used for mutually-exclusive traits.
        public ItemWithInfo TagFirst(params Trait[] traits)
        {
            foreach (var trait in traits)
            {
                if (this.Tag(trait).Success)
                    break;
            }
            return this;
        }

        /// attempts to tag each of the traits given in the params list
        /// without regard to the success of each tag operation.
        /// Can be used to try tagging related but not mutually-exclusive traits.
        public ItemWithInfo TagAny(params Trait[] traits)
        {
            bool res = false;
            foreach (var trait in traits)
                res |= this.Tag(trait).Success;

            // we want to know if any of the operations succeeded, not just
            // the most recent one, so we catch any True value in res
            Success = res;
            return this;
        }

        #region bool returns

        public bool TryTag(Trait trait)
        {
            return this.Tag(trait).Success;
        }

        #endregion bool returns

    }




}
