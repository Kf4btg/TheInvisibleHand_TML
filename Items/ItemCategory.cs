using System;
using System.Linq;
using System.Collections.Generic;
using InvisibleHand.Rules;
using Terraria;

namespace InvisibleHand.Items
{

    using ItemCompRule = Func<Item, Item, int>;

    public class ItemCategory : IComparable<ItemCategory>, IEquatable<ItemCategory>, IComparer<Item>
    {

        // private short _priority;

        /// the explicitly set sort-priority for this category
        public short Priority;

        /// A unique identifying number for this category; usually
        /// the order in which this item was loaded from the definition file;
        /// used to help solve conflicts in Priority.
        public readonly ushort ID;

        /// a combination of the priority and ID that can be used
        /// to properly sort this category amongst a list of other categories
        public int Ordinal => (Priority << 16) | (ID);

        public string Name { get; private set; }
        public string QualifiedName {
            get {
                if (Parent != null)
                    return Parent.QualifiedName + "." + Name;
                return Name;
            }
        }

        public IDictionary<string, int> Requirements { get; set; }


        private ISet<string> _wrapped_cats;
        /// this may be unnecessary; intended to hold the list of
        /// category names that have been merged to this one if this is a
        /// merge-container; only the names are stored because it should
        /// not be necessary to directly access the merged categories from here.
        public ISet<string> MergedCategories => _wrapped_cats;

        /// the parent's requirements will have already been merged into
        /// this Category's requirements, so this property is really only
        /// here to help with determining sort-order.
        public ItemCategory Parent { get; private set; }

        /// indicates whether this Category is simply a container
        /// for the categories that were merged into it. If so, it
        /// cannot have Requirements, and Matches() will always return null.
        /// When a category that has been merged into this one has its
        /// Matches() method called, it will a reference to this instance
        /// rather than itself.
        public readonly bool IsMergeWrapper;

        /// if not null, then this is the Wrapper-Category that this
        /// category has been merged into
        private ItemCategory _wrapper_category;


        /// this is what should be used to get "this" category; will return the proper
        /// instance depending on whether this category has been merged or not.
        public ItemCategory GetCategory => _wrapper_category ?? this;


        public ItemCategory(string name, ushort cat_id, ItemCategory parent = null, bool is_merge_wrapper = false, short priority = 0)
        {
            Name = name;
            Parent = parent;
            Priority = priority;
            ID = cat_id;

            IsMergeWrapper = is_merge_wrapper;

            if (IsMergeWrapper)
                _wrapped_cats = new HashSet<string>();
            else
                Requirements = new Dictionary<string, int>();
        }

        /// initialize the category with a pre-created requirements dict
        public ItemCategory(string name, ushort cat_id, IDictionary<string, int> requirements, ItemCategory parent = null, short priority = 0)
        {
            Name = name;
            Parent = parent;
            Priority = priority;
            ID = cat_id;

            IsMergeWrapper = false;

            Requirements = requirements;
        }

        public void AddTraitRequirement(string trait_family, int flag_value)
        {
            // TODO: throw a proper exception when attempting to add req to merge containers
            if (IsMergeWrapper)
                return;

            if (Requirements.ContainsKey(trait_family))
                Requirements[trait_family] |= flag_value;
            else
                Requirements[trait_family] = flag_value;
        }

        public bool Matches(IDictionary<string, int> item_flags)
        {
            if (this.IsMergeWrapper) return false;
            // return this.Matches(item_flags) != null;
            foreach (var kvp in Requirements)
            {
                var reqval = kvp.Value;
                int flagval;

                // if ever the item does not have a required trait or it does
                // have the correct flag value for the trait, return false to indicate no match
                if (!item_flags.TryGetValue(kvp.Key, out flagval) || ((flagval & reqval) != reqval))
                    return false;
            }
            return true;
        }

        public ItemCategory Match(IDictionary<string, int> item_flags)
        {
            if (this.IsMergeWrapper) return null;
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
            return this.GetCategory;
        }

        public void Merge(ItemCategory wrapper)
        {
            this._wrapper_category = wrapper;
            wrapper.MergedCategories.Add(this.Name);
        }
        public void Unmerge()
        {
            this._wrapper_category = null;
        }

        #region internal item-sorting rules
        // private IList<Func<T, bool>> SortRules = new List<Func<T, bool>>();

        /// caches the compiled rules by the name of the property being compared
        private static IDictionary<string, ItemCompRule> RuleCache = new Dictionary<string, ItemCompRule>();


        private IList<ItemCompRule> _rules;
        public IList<ItemCompRule> SortRules => _rules;

        /// saved independently in order to make iteration more efficient
        private int ruleCount = 0;

        // public List<Expression<Func<T, T, int>>> ruleExpressions;

        /// Given an enumerable of Terraria.Item property names, build and compile
        /// lambda expressions that will return the result of comparing those properties
        /// on two distinct Item objects
        public void BuildSortRules(IEnumerable<string> properties)
        {

            // if the cache contains any of the requested properties, we'll build the rules 1by1
            if (properties.Any(p => RuleCache.ContainsKey(p)))
            {
                _rules = new List<ItemCompRule>();

                foreach (var prop in properties)
                {
                    // check the cache first
                    ItemCompRule newrule;
                    if (!RuleCache.TryGetValue(prop, out newrule))
                    {
                        // if the rule wasn't there, create it with the RuleBuilder
                        newrule = RuleBuilder.CompileVsRule(new List<Item>(), prop);
                        // and cache it
                        RuleCache[prop] = newrule;
                    }
                    // add to list
                    _rules.Add(newrule);
                }

                this.ruleCount = this._rules.Count;
            }
            else if (properties.Count() > 0) // make sure list isn't empty
            {
                var plist = new List<string>(properties);
                // if all are uncached, call the multi-rule builder (for efficiency)
                _rules = RuleBuilder.CompileVsRules(new List<Item>(), plist);

                // because the rules SHOULD be in the same order as the properties given:
                foreach (var newrule in _rules.Select((r, i) => new { rule = r, index = i }))
                    RuleCache[plist[newrule.index]] = newrule.rule;

                this.ruleCount = this._rules.Count;
            }
        }

        /// copy the sorting rules from another category
        public void CopySortRules(ItemCategory other)
        {
            this._rules = other?.SortRules;
            this.ruleCount = this._rules?.Count ?? 0;
        }

        #endregion

        #region interface implementations
        public int CompareTo(ItemCategory other)
        {
            return this.Ordinal.CompareTo(other.Ordinal);
        }

        public bool Equals(ItemCategory other)
        {
            return this.ID == other?.ID;
        }

        public override bool Equals(Object other)
        {
            if (other == null) return false;

            ItemCategory other_cat = other as ItemCategory;

            return Equals(other_cat);
        }

        /// priority might change if the user modifies their preferred
        /// sort order; ID should remain immutable throughout the lifetime
        /// of this category.
        public override int GetHashCode()
        {
            return this.ID;
        }

        public static bool operator ==(ItemCategory cat1, ItemCategory cat2)
        {
            if ((object)cat1 == null || (object)cat2 == null)
                return Object.Equals(cat1, cat2);

            return cat1.Equals(cat2);
        }

        public static bool operator !=(ItemCategory cat1, ItemCategory cat2)
        {
            if ((object)cat1 == null || (object)cat2 == null)
                return ! Object.Equals(cat1, cat2);

            return ! (cat1.Equals(cat2));
        }

        /// IComparer<Item> implementation
        /// using the pre-compiled Sorting rules
        public int Compare(Item t1, Item t2)
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
        #endregion

        /// this is what will be returned when an item somehow doesn't match any defined category
        public static readonly ItemCategory None = new ItemCategory("Unknown", ushort.MaxValue, null, false, short.MaxValue);


        public override string ToString()
        {
            return $"{{{QualifiedName}: p{Priority}, ID#{ID}";
        }


    }
}
