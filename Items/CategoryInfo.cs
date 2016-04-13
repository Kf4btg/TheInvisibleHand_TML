// using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Dynamic;
using Terraria;
// using Terraria.ID;
using Terraria.ModLoader;

namespace InvisibleHand.Items
{
    /// store the information about an item's category here.
    public class CategoryInfo : ItemInfo
    {
        // public List<int> categories = new List<int>();
        //
        // TODO: create enums or use ints for the categories
        // public List<string> categories = new List<string>();
        public HashSet<Trait> traits = new HashSet<Trait>();

        // some frequently needed properties
        // public ToolType tool = ToolType.None;
        // public WeaponType weapon = WeaponType.None;

        /// until we nail down exactly what traits we want to use, we'll
        /// just reference them by name. Later on we'll define enums
        private HashSet<string> S_traits;

        public HashSet<string> Traits
        {
            get
            {
                if (S_traits==null)
                    S_traits = new HashSet<String>();
                return S_traits;
            }
            set
            {
                if (S_traits==null) S_traits = value;
            }
        }


        public void AddTrait(Trait t)
        {
            traits.Add(t);
        }
        public void AddTraits(params Trait[] ts)
        {
            traits.UnionWith(ts);
        }

        public bool AddTrait(string trait_name)
        {
            return Traits.Add(trait_name);
            // Trait t;
            // if (Enum.TryParse(trait_name, out t))
            // traits.Add(t);
        }

        public void AddTraits(string[] trait_names)
        {

            Traits.UnionWith(trait_names);

            // foreach (var tn in trait_names)
            // {
            //     Trait t;
            //     if (Enum.TryParse(tn, out t))
            //         traits.Add(t);
            // }
        }

        // public void addWeaponTrait(Trait t)
        // {
        //     AddTrait(t);
        //     AddTrait(Trait.weapon);
        // }
        // public void addToolTrait(Trait t)
        // {
        //     AddTrait(t);
        //     AddTrait(Trait.tool);
        // }
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

        private IDictionary<string, Func<Item, bool>> conditionTable;

        public ItemWithInfo()
        {
            this.conditionTable = Rules.ConditionTable;
        }

        public bool TryAddTrait(Func<Item, bool> check, string trait)
        {
            if (check(item))
            {
                info.AddTrait(trait);
                return true;
            }
            return false;
        }

        public bool TryAddTrait(bool condition, string trait)
        {
            if (condition)
            {
                info.AddTrait(trait);
                return true;
            }
            return false;
        }

        public void AddTrait(string trait)
        {
            info.AddTrait(trait);
        }

        /// tag this instance with the given trait;
        /// return the modified instance
        public ItemWithInfo AddTag(string trait)
        {
            info.AddTrait(trait);

            // if we've gotten here, then the condition check (if any)
            // was successful, so we set LastResult=true
            Success = true;
            return this;
        }

        /// tag this instance with the given trait iff the condition
        /// for the trait (as found in the Condition Table) is true;
        ///return the instance, whether modified or not.
        public ItemWithInfo Tag(string trait)
        {
            Success = false; // reset
            return conditionTable[trait](item) ? this.AddTag(trait) : this;
        }

        /// tag this instance with the given trait;
        /// IFF check() evaluates true; return the instance,
        /// modified or not.
        // public ItemWithInfo Tag(Func<Item, bool> check, string trait)
        // {
        //     return this.Tag(check(item), trait);
        // }

        /// tag this instance with the given trait
        /// IFF condition is true; return the instance,
        /// modified or not.
        public ItemWithInfo TagIf(bool condition, string trait)
        {
            Success = false;
            return condition ? this.AddTag(trait) : this;
        }


        /// tag this instance with given trait_if_true;
        /// if condition is true; otherwise, tag this
        /// instance with trait_if_false
        // public ItemWithInfo Tag(bool condition, string trait_if_true, string trait_if_false)
        // public ItemWithInfo Tag(string trait_if_true, string trait_if_false)
        // {
            // return condition ? this.AddTag(trait_if_true) : this.AddTag(trait_if_false);
        // }

        /// tag this instance with given trait_if_true;
        /// if check() evaluates true; otherwise, tag this
        /// instance with trait_if_false
        // public ItemWithInfo Tag(Func<Item, bool> check, string trait_if_true, string trait_if_false)
        // {
        //     return this.Tag(check(item), trait_if_true, trait_if_false);
        // }

        public ItemWithInfo TagFirst(params string[] traits)
        {
            foreach (var trait in traits)
            {
                if (this.Tag(trait).Success)
                    break;
            }
            return this;
        }

        /// goes through the list of pairs in the params list, attempting to tag each given trait; when one tag is successful, the function returns without checking the remaining. Should be used for mutually-exclusive traits.
        // public ItemWithInfo TagFirst(params Tuple<Func<Item, bool>, string>[] bool_trait_pairs)
        // {
        //     foreach (var pair in bool_trait_pairs)
        //     {
        //         if (this.Tag(pair.Item1, pair.Item2).LastResult)
        //             break;
        //     }
        //     return this;
        // }

        /// attempts to tag each of the traits given in the params list, not caring whether each tag operation is successful or not. Can be used to try tagging related but not mutually-exclusive traits.
        // public ItemWithInfo TagAny(params Tuple<Func<Item, bool>, string>[] check_trait_pairs)
        public ItemWithInfo TagAny(params string[] traits)
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

        public bool TryTag(string trait)
        {
            return this.Tag(trait).Success;
        }

        // public bool TryTag(bool condition, string trait)
        // {
        //     if (condition)
        //     {
        //         AddTrait(trait);
        //         return true;
        //     }
        //     return false;
        // }
        //
        // public bool TryTag(Func<Item, bool> check, string trait)
        // {
        //     return TryTag(check(item), trait);
        // }

        #endregion bool returns

    }




}
