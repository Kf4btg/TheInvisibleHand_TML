using System;
using System.Collections.Generic;
using Terraria;

namespace InvisibleHand.Items.Categories
{
    using ItemCompRule = Func<Item, Item, int>;

    public class RegularCategory : ItemCategory
    {
        public IDictionary<string, int> Requirements { get; set; }

        // constructors
        // ------------------
        public RegularCategory(string name, int cat_id,
                               IDictionary<string, int> requirements = null,
                               int parent_id = 0, int priority = 0)
                               : base (name, cat_id, parent_id, priority)
        {
            Requirements = requirements ?? new Dictionary<string, int>();
        }

        public RegularCategory(string name, int cat_id,
                               IDictionary<string, int> requirements = null,
                               ItemCategory parent = null, int priority = 0)
                               : this(name, cat_id, requirements,
                                      parent?.ID ?? 0, priority) {}


        // Adding a trait
        // ------------------
        public void AddTraitRequirement(string trait_family, int flag_value)
        {
            if (Requirements.ContainsKey(trait_family))
                Requirements[trait_family] |= flag_value;
            else
                Requirements[trait_family] = flag_value;
        }

        // Abstract method overrides
        // ---------------------------
        public override bool Matches(IDictionary<string, int> item_flags)
        {
            foreach (var kvp in Requirements)
            {
                var reqval = kvp.Value;
                int flagval;

                // if ever the item does not have a required trait or
                // an incompatible flag value for the trait, return false to indicate no match
                if (!item_flags.TryGetValue(kvp.Key, out flagval) || ((flagval & reqval) != reqval))
                    return false;
            }
            return true;
        }

        public override bool Matches(Item item)
        {
            return Matches(item.GetFlagInfo().Flags);
        }

        public override ICategory<Item> Match(IDictionary<string, int> item_flags)
        {
            foreach (var kvp in Requirements)
            {
                var reqval = kvp.Value;
                int flagval;

                // if ever the item does not have a required trait or it does
                // have the correct flag value for the trait, return null to indicate no match
                if (!item_flags.TryGetValue(kvp.Key, out flagval) || ((flagval & reqval) != reqval))
                    return null;
            }

            // if we made it here, than the item passed all the required checks.
            // Now we return the matched category:
            // If this category has been marked as part of a merged category,
            // return that category; otherwise, return this instance.
            // return this.GetCategory;
            return this.Category;
        }

        // Handle the Sorting Rules
        // ------------------

        /// saved independently in order to make iteration more efficient
        private int ruleCount = 0;

        private IList<ItemCompRule> _rules;
        public IList<ItemCompRule> SortRules
        {
            get { return _rules; }
            set
            {
                _rules = value;
                ruleCount = _rules?.Count ?? 0;
            }
        }
        // public List<Expression<Func<T, T, int>>> ruleExpressions; // for debugging

        /// Given an enumerable of Terraria.Item property names, build and compile
        /// lambda expressions that will return the result of comparing those properties
        /// on two distinct Item objects
        public void BuildSortRules(IEnumerable<string> properties)
        {
            this.SortRules = ItemRuleBuilder.BuildSortRules(properties);
        }

        /// copy the sorting rules from another category
        public void CopySortRules(RegularCategory other)
        {
            // var target = other as RegularCategory;
            this.SortRules = other?.SortRules;
        }

        public void CopyParentRules()
        {
            this.SortRules = ((RegularCategory)Parent)?.SortRules;
        }


        /// IComparer<Item> implementation
        /// using the pre-compiled Sorting rules
        public override int Compare(Item t1, Item t2)
        {
            int res;
            for (int i = 0; i < ruleCount; i++)
            {
                res = SortRules[i](t1, t2);
                if (res != 0)
                    return res;
            }
            return 0;
        }
    }

}
