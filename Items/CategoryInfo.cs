// using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
// using System.Dynamic;
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
        public ISet<Trait> traits = new HashSet<Trait>();

        /// until we nail down exactly what traits we want to use, we'll
        /// just reference them by name. Later on we'll define enums
        private ISet<string> S_traits;

        public ISet<string> Traits
        {
            get
            {
                return S_traits ?? new HashSet<String>();
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

    }

    /// use this class to make the categorization checks cleaner/easier
    internal class ItemWithInfo
    {
        public Item item;
        // public CategoryInfo info;


        // for the multi-tag operations, this can be checked to see if the operation was successful for that given scenario.
        /// Holds whether the most recent tagging attempt was successful
        /// (i.e. the trait's condition was satisfied)
        public bool Success { get; private set; }

        public ISet<string> TraitList { get; private set; }

        private IDictionary<string, Func<Item, bool>> conditionTable;

        public ItemWithInfo(Item item)
        {
            this.item = item;
            this.conditionTable = Rules.ConditionTable;
            this.TraitList = new HashSet<string>();
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
        public ItemWithInfo AddTag(string trait)
        {
            TraitList.Add(trait);

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

        /// tag this instance with the given trait
        /// IFF condition is true; return the instance,
        /// modified or not.
        public ItemWithInfo TagIf(bool condition, string trait)
        {
            Success = false;
            return condition ? this.AddTag(trait) : this;
        }

        /// goes through the list of traits in the params list, attempting to tag each one; when a tag is successful,
        /// return without checking the remaining. Should be used for mutually-exclusive traits.
        public ItemWithInfo TagFirst(params string[] traits)
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

        #endregion bool returns

    }




}
