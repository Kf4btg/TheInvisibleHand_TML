using System;
using System.Collections.Generic;
using Terraria;

namespace InvisibleHand.Items.Categories.Types
{

    ///<summary>
    /// The most common type of category, this is the "worker" category.
    /// It contains rules defining how to match and sort Items based
    /// on their properties.
    ///</summary>
    public class MatchCategory : ItemSorter
    {
        /// If this returns true, this category should be skipped when matching items
        public bool SkipMatch { get; private set; }

        ///<summary>A swappable function to define how to perform the match on a flag collection</summary>
        private Func<IDictionary<string, int>, bool> checkFlagMatch;
        ///<summary>A swappable function to define how to perform the match on an Item instance</summary>
        private Func<Item, bool> checkItemMatch;

        public IDictionary<string, int> Requirements { get; set ; }
        public IDictionary<string, int> Exclusions { get; set; }

        // constructors
        // ------------------
        /// Create a MatchCategory that explicitly has no requirements or exclusions.
        /// Its only purpose is to act as a container to other categories. The "Matches"
        /// function for the flag_collection will be short-circuited to return false.
        /// The Matches function for an Item object will return whether the given Item
        /// belongs to one of the children of this ItemCategory.
        /// Item-sorting will function as normal, using any defined Sort Rules
        public MatchCategory(string name, int cat_id, int parent_id = 0, int priority = 0)
                            : base(name, cat_id, parent_id, priority)
        {
            SkipMatch = true;
            //short-circuit to false
            checkFlagMatch = (d) => false;
            checkItemMatch = this._MatchesChild;
        }

        /// create a new MatchCategory and optionally set its requirement and exclusion collections
        public MatchCategory(string name, int cat_id,
                                int parent_id, int priority,
                               IDictionary<string, int> requirements,
                               IDictionary<string, int> exclusions
                               )
                               : base (name, cat_id, parent_id, priority)
        {
            Requirements = requirements ?? new Dictionary<string, int>();
            Exclusions = exclusions ?? new Dictionary<string, int>();

            //enable the check function
            SkipMatch = false;
            checkFlagMatch = this._Matches;
            checkItemMatch = (i) => this.Matches(i.GetFlagInfo().Flags);
        }

        // Adding/excluding a trait
        // ------------------
        ///<summary>Add a trait to this category's Requirements map</summary>
        public void RequireTrait(string trait_family, int flag_value)
        {
            SetTraitValue(Requirements, trait_family, flag_value);
        }

        ///<summary>Add a trait to this category's Exclusions map</summary>
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
        public override bool Matches(IDictionary<string, int> item_flags) => checkFlagMatch(item_flags);

        /// this is NOT intended to be called when initially setting an item's category.
        /// That may lead to infinite recursion! It's intended purpose is for querying whether
        /// an already-categorized item matches this category.
        public override bool Matches(Item item) => checkItemMatch(item);
        // {
        //     // return Matches(item.GetFlagInfo().Flags);
        //     return checkItemMatch(item);
        // }

        /// If this category contains Reqs/Excls, this function will called for checkMatch()
        private bool _Matches(IDictionary<string, int> item_flags)
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

        /// using the ordinal of the item's calculated category, decide if the item belongs
        /// to one of the child (or grandchild, etc.) categories of this container. This method
        /// can be used as the callable for checkItemMatch
        public bool _MatchesChild(Item item)
        {
            return this.Contains(item.GetFlagInfo().ActualCategory);
        }

        public override ICategory<Item> Match(IDictionary<string, int> item_flags)
        {
            // if the item passed all the required checks,
            // we return the matched category by returning this.Category:
            // If this category has been marked as part of a Union category,
            // return that category; otherwise, return this instance.
            return this.Matches(item_flags) ? this.Category : null;
        }

        ///<summary>
        /// IComparer<Item> implementation using the pre-compiled Sorting rules
        ///</summary>
        // public override int Compare(Item t1, Item t2)
        // {
        //     int res;
        //     for (int i = 0; i < ruleCount; i++)
        //     {
        //         res = SortRules[i](t1, t2);
        //         if (res != 0)
        //             return res;
        //     }
        //     return 0;
        // }
    }
}
