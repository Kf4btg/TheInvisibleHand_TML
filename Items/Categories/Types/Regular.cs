// using System;
using System.Collections.Generic;
using Terraria;

namespace InvisibleHand.Items.Categories.Types
{
    // using ItemCompRule = Func<Item, Item, int>;

    public class RegularCategory : Sorter
    {
        public IDictionary<string, int> Requirements { get; set; }
        public IDictionary<string, int> Exclusions { get; set; }

        // constructors
        // ------------------
        public RegularCategory(string name, int cat_id,
                                int parent_id = 0, int priority = 0,
                               IDictionary<string, int> requirements = null,
                               IDictionary<string, int> exclusions = null
                               )
                               : base (name, cat_id, parent_id, priority)
        {
            Requirements = requirements ?? new Dictionary<string, int>();
            Exclusions = exclusions ?? new Dictionary<string, int>();
        }

        public RegularCategory(string name, int cat_id,
                                ItemCategory parent = null, int priority = 0,
                               IDictionary<string, int> requirements = null,
                               IDictionary<string, int> exclusions = null
                               )
                               : this(name, cat_id, parent?.ID ?? 0, priority,
                                requirements, exclusions) {}


        // Adding/excluding a trait
        // ------------------
        public void RequireTrait(string trait_family, int flag_value)
        {
            SetTraitValue(Requirements, trait_family, flag_value);

            // if (Requirements.ContainsKey(trait_family))
            //     Requirements[trait_family] |= flag_value;
            // else
            //     Requirements[trait_family] = flag_value;
        }

        public void ExcludeTrait(string trait_family, int flag_value)
        {
            SetTraitValue(Exclusions, trait_family, flag_value);
        }

        /// internal helper for Require-/ExcludeTrait()
        private void SetTraitValue(IDictionary<string, int> flagmap, string trait_family, int flag_value)
        {
            if (flagmap.ContainsKey(trait_family))
                flagmap[trait_family] |= flag_value;
            else
                flagmap[trait_family] = flag_value;
        }

        // Abstract method overrides
        // ---------------------------
        public override bool Matches(IDictionary<string, int> item_flags)
        {
            // if (!Enabled) return false;

            foreach (var kvp in Requirements)
            {
                var req_val = kvp.Value;
                int flagval;

                // if ever the item does not have a required trait OR has
                // an incompatible flag value for the trait, return false to indicate no match
                if (!item_flags.TryGetValue(kvp.Key, out flagval) || ((flagval & req_val) != req_val))
                    return false;
            }
            foreach (var kvp in Exclusions)
            {
                var ex_value = kvp.Value;
                int flagval;

                // if the category for the excluded trait is present AND the given flag is set, return false
                if (item_flags.TryGetValue(kvp.Key, out flagval) && ((flagval & ex_value) == ex_value))
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
            // if the item passed all the required checks,
            // we return the matched category by returning this.Category:
            // If this category has been marked as part of a Union category,
            // return that category; otherwise, return this instance.
            return this.Matches(item_flags) ? this.Category : null;
        }

        // Handle the Sorting Rules
        // ------------------

        /// saved independently in order to make iteration more efficient
        // private int ruleCount = 0;
        //
        // private IList<ItemCompRule> _rules;
        // public IList<ItemCompRule> SortRules
        // {
        //     get { return _rules; }
        //     set
        //     {
        //         _rules = value;
        //         ruleCount = _rules?.Count ?? 0;
        //     }
        // }
        // public List<Expression<Func<T, T, int>>> ruleExpressions; // for debugging

        /// Given an enumerable of Terraria.Item property names, build and compile
        /// lambda expressions that will return the result of comparing those properties
        /// on two distinct Item objects
        // public void BuildSortRules(IEnumerable<string> properties)
        // {
        //     this.SortRules = ItemRuleBuilder.BuildSortRules(properties);
        // }

        /// copy the sorting rules from another category
        // public void CopySortRules(RegularCategory other)
        // {
        //     // var target = other as RegularCategory;
        //     this.SortRules = other?.SortRules;
        // }

        // public void CopyParentRules()
        // {
        //     this.SortRules = ((RegularCategory)Parent)?.SortRules;
        // }


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
