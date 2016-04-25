using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using InvisibleHand.Rules;

namespace InvisibleHand.Definitions
{
    public class ItemCategory : IComparable<ItemCategory>, IEquatable<ItemCategory>
    {
        // private short _priority;

        /// the Child-depth of this category; 0 for top-level categories (no parents),
        /// +1 for each parent above this.
        // private short depth;

        /// we SUBTRACT the depth from the priority to make sure that more specific categories are
        /// sorted first; for example, if "Weapon" has a priority of 500:
        //      "Weapon.Melee.Broadsword" {P=498} < "Weapon.Melee" {P=499} < "Weapon.Throwing" {P=499} < "Weapon" {P=500}
        // Note: weapon.melee is sorted before weapon.throwing (even though they have the same priority)
        // because the weapon.melee category was created before weapon.throwing.
        // public short Priority { get { return (short)(_priority - depth); } set { _priority = value; } }
        /// this indicates whether the priority was explicitly specified
        /// in a configuration file or was assigned a default value.
        // internal bool explicit_priority = false;

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
        public ItemCategory Category => _wrapper_category ?? this;


        public ItemCategory(string name, ushort cat_id, ItemCategory parent = null, bool is_merge_wrapper = false, short priority = 0)
        {
            Name = name;
            Parent = parent;
            Priority = priority;
            ID = cat_id;

            // short _depth = 0;
            // while (parent != null)
            // {
            //     depth++;
            //     parent = parent.Parent;
            // }
            //
            // this.depth = _depth;

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

            // short _depth = 0;
            // while (parent != null)
            // {
            //     depth++;
            //     parent = parent.Parent;
            // }
            //
            // this.depth = _depth;

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
            return this.Category;
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

        #region interface implementation
        public int CompareTo(ItemCategory other)
        {
            return this.Ordinal.CompareTo(other.Ordinal);
            // var val = this.Priority - other.Priority;
            // if (val != 0) return val;
            //
            // return this.ID.CompareTo(other.ID);
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


        #endregion
        /// this is what will be returned when an item somehow doesn't match any defined category
        public static readonly ItemCategory None = new ItemCategory("Unknown", ushort.MaxValue, null, false, short.MaxValue);
    }

    /// contains a list of compiled sorting rules
    public class CategoryOf<T> : ItemCategory
    {

        // private IList<Func<T, bool>> SortRules = new List<Func<T, bool>>();
        public IList<Func<T, T, int>> SortRules;

        public List<Expression<Func<T, T, int>>> ruleExpressions;




        public CategoryOf(string name, ushort cat_id, IDictionary<string, int> requirements, ItemCategory parent = null, short priority = 0) : base (name, cat_id, requirements, parent, priority) {}

        public void BuildSortRules(IEnumerable<string> properties)
        {
            // List<Expression<Func<T, T, int>>> exprs;
            if (properties.Count() > 0)
            {
                this.SortRules = RuleBuilder.CompileVsRules(new List<T>(), new List<string>(properties), out ruleExpressions);
                // ruleExpressions = exprs;

            }


        }
    }
}
