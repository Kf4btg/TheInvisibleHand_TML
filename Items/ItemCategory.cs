using System;
using System.Collections.Generic;
using Terraria;

namespace InvisibleHand.Items
{

    using ItemCompRule = Func<Item, Item, int>;

    public enum PriorityType
    {
        UNSET = 0,
        DERIVED,
        EXPLICIT,
    }

    public abstract class ItemCategory : IComparable<ItemCategory>, IEquatable<ItemCategory>, IComparer<Item>
    {
        // Static stuff
        //------------------------------------------------------------------
        //------------------------------------------------------------------

        /// static registry of all categories keyed by their unique ID
        /// number (an unsigned short);
        /// hopefully 65,000 categories will be enough...
        /// Each category is registered here during construction
        public static IDictionary<ushort, ItemCategory> Registry =
            new Dictionary<ushort, ItemCategory>();

        /// <summary>
        /// Add to the static Category Register (keyed by Category ID)
        /// </summary>
        public static void Register(ItemCategory newcategory)
        {
            Registry.Add(newcategory.ID, newcategory);
        }

        //------------------------------------------------------------------
        //------------------------------------------------------------------

        /// the sort-priority for this category
        public short Priority;
        public PriorityType priority_type = PriorityType.UNSET;

        internal int childCount = 0;
        internal int thisChildIndex = 0;

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

        /// the parent's requirements will have already been merged into
        /// this Category's requirements, so this property is really only
        /// here to help with determining sort-order.
        public ItemCategory Parent
        {
            get { return Registry[this.ParentID]; }
            set { this.ParentID = value?.ID ?? 0; }
        }

        /// a parentID of 0 means this is a toplevel category
        public ushort ParentID { get; set; } = 0;

        /// the ID of the UnionCategory to which this category belongs.
        /// If this is 0, then the category has not joined a Union.
        protected ushort UnionID = 0;

        /// if not null, then this is the Wrapper-Category that this
        /// category has been merged into
        protected UnionCategory _union => UnionID > 0 ? Registry[UnionID] as UnionCategory : null;

        /// this is what should be used to get "this" category; will return the proper
        /// instance depending on whether this category has been merged or not.
        // public ItemCategory GetCategory => _union ?? this;
        public ItemCategory GetCategory => UnionID > 0 ? Registry[UnionID] : this;

        /*
         ██████  ██████  ███    ██ ███████ ████████ ██████  ██    ██  ██████ ████████  ██████  ██████  ███████
        ██      ██    ██ ████   ██ ██         ██    ██   ██ ██    ██ ██         ██    ██    ██ ██   ██ ██
        ██      ██    ██ ██ ██  ██ ███████    ██    ██████  ██    ██ ██         ██    ██    ██ ██████  ███████
        ██      ██    ██ ██  ██ ██      ██    ██    ██   ██ ██    ██ ██         ██    ██    ██ ██   ██      ██
         ██████  ██████  ██   ████ ███████    ██    ██   ██  ██████   ██████    ██     ██████  ██   ██ ███████
        */

        public ItemCategory(string name, ushort category_id, ushort parent_id = 0, short priority = 0)
        {
            Name = name;
            ParentID = parent_id;
            Priority = priority;
            ID = category_id;
            Register(this);
        }

        public ItemCategory(string name, ushort category_id, ItemCategory parent = null, short priority = 0)
            : this(name, category_id, parent?.ID ?? 0, priority) { }


        /*
        ███    ███ ███████ ██████   ██████  ██ ███    ██  ██████
        ████  ████ ██      ██   ██ ██       ██ ████   ██ ██
        ██ ████ ██ █████   ██████  ██   ███ ██ ██ ██  ██ ██   ███
        ██  ██  ██ ██      ██   ██ ██    ██ ██ ██  ██ ██ ██    ██
        ██      ██ ███████ ██   ██  ██████  ██ ██   ████  ██████
        */

        public void Merge(UnionCategory union)
        {
            if (union != null)
            {
                // this._union = union;
                this.UnionID = union.ID;
                union.AddMember(this);
            }
        }
        public void Merge(ushort union_id)
        {
            try {
                Merge(Registry[union_id] as UnionCategory);
            }
            catch (KeyNotFoundException knfe)
            {
                throw new UsefulKeyNotFoundException(union_id.ToString(), nameof(Registry), knfe,
                    "The category '" + this.Name + "' could not be added to the Union with ID '{0}' because no category with that ID exists in '{1}'." );
            }
        }

        public void Unmerge()
        {
            this._union?.RemoveMember(this);
            this.UnionID = 0;
        }

        /*
         █████  ██████  ███████ ████████ ██████   █████   ██████ ████████
        ██   ██ ██   ██ ██         ██    ██   ██ ██   ██ ██         ██
        ███████ ██████  ███████    ██    ██████  ███████ ██         ██
        ██   ██ ██   ██      ██    ██    ██   ██ ██   ██ ██         ██
        ██   ██ ██████  ███████    ██    ██   ██ ██   ██  ██████    ██
        */

        #region abstract methods

        public abstract bool Matches(Item item);
        public abstract bool Matches(IDictionary<string, int> item_flags);

        public abstract ItemCategory Match(IDictionary<string, int> item_flags);

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


        public abstract int Compare(Item t1, Item t2);

        #endregion

        /// this is what will be returned when an item somehow doesn't match any defined category
        public static readonly ItemCategory None = new RegularCategory("Unknown", ushort.MaxValue, null, null, short.MaxValue);


        public override string ToString()
        {
            return $"{{{QualifiedName}: p{Priority}, ID#{ID}";
        }
    }

    /*
    ██████  ███████  ██████  ██    ██ ██       █████  ██████
    ██   ██ ██      ██       ██    ██ ██      ██   ██ ██   ██
    ██████  █████   ██   ███ ██    ██ ██      ███████ ██████
    ██   ██ ██      ██    ██ ██    ██ ██      ██   ██ ██   ██
    ██   ██ ███████  ██████   ██████  ███████ ██   ██ ██   ██
    */

    public class RegularCategory : ItemCategory
    {

        public IDictionary<string, int> Requirements { get; set; }


        // constructors
        // ------------------
        public RegularCategory(string name, ushort cat_id,
                               IDictionary<string, int> requirements = null,
                               ushort parent_id = 0, short priority = 0)
                               : base (name, cat_id, parent_id, priority)
        {
            Requirements = requirements ?? new Dictionary<string, int>();
        }

        public RegularCategory(string name, ushort cat_id,
                               IDictionary<string, int> requirements = null,
                               ItemCategory parent = null, short priority = 0)
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

                // if ever the item does not have a required trait or it does
                // have the correct flag value for the trait, return false to indicate no match
                if (!item_flags.TryGetValue(kvp.Key, out flagval) || ((flagval & reqval) != reqval))
                    return false;
            }
            return true;
        }

        public override bool Matches(Item item)
        {
            return Matches(item.GetFlagInfo().Flags);
        }


        public override ItemCategory Match(IDictionary<string, int> item_flags)
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
            return this.GetCategory;
        }

        // Handle the Sorting Rules
        // ------------------
        #region internal item-sorting rules

        /// saved independently in order to make iteration more efficient
        private int ruleCount = 0;

        private IList<ItemCompRule> _rules;
        public IList<ItemCompRule> SortRules
        {
            get { return _rules; }
            set {
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
        public void CopySortRules(ItemCategory other)
        {
            var target = other as RegularCategory;

            this.SortRules = target?.SortRules;
        }

        #endregion

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

    /*
    ██    ██ ███    ██ ██  ██████  ███    ██      ██████  █████  ████████ ███████  ██████   ██████  ██████  ██    ██
    ██    ██ ████   ██ ██ ██    ██ ████   ██     ██      ██   ██    ██    ██      ██       ██    ██ ██   ██  ██  ██
    ██    ██ ██ ██  ██ ██ ██    ██ ██ ██  ██     ██      ███████    ██    █████   ██   ███ ██    ██ ██████    ████
    ██    ██ ██  ██ ██ ██ ██    ██ ██  ██ ██     ██      ██   ██    ██    ██      ██    ██ ██    ██ ██   ██    ██
     ██████  ██   ████ ██  ██████  ██   ████      ██████ ██   ██    ██    ███████  ██████   ██████  ██   ██    ██
    */

    public class UnionCategory : ItemCategory
    {

        /// this may be unnecessary; intended to hold the list of
        /// categories that have been merged to this one if this is a
        /// merge-container
        /// Item Category implements IComparable and GetHashCode, so this should be efficient
        public ISet<ItemCategory> UnionMembers { get; private set; }

        /// Returns the member category that matched the most recent Match()/Matches() check,
        /// or null if none did
        public ItemCategory Matched { get; private set; }


        public UnionCategory(string name, ushort cat_id, ushort parent_id = 0, short priority = 0, IEnumerable<ItemCategory> members=null) : base(name, cat_id, parent_id, priority)
        {
            if (members == null)
                UnionMembers = new SortedSet<ItemCategory>();
            else
                UnionMembers = new SortedSet<ItemCategory>(members);
        }

        public UnionCategory(string name, ushort cat_id, ItemCategory parent = null, short priority = 0, IEnumerable<ItemCategory> members=null) : this(name, cat_id, parent?.ID ?? 0, priority, members) {}


        // Tracking member Categories
        // ---------------------------
        public void AddMember(ItemCategory newMember)
        {
            this.UnionMembers.Add(newMember);
        }

        public void RemoveMember(ItemCategory member)
        {
            this.UnionMembers.Remove(member);
        }

        // Abstract method overrides
        // ------------------------

        /// <summary>
        /// Check if the given flags are a fit for this category
        /// </summary>
        /// <param name="item_flags"> </param>
        /// <returns>True if the flags match any of the contained categories</returns>
        public override bool Matches(IDictionary<string, int> item_flags)
        {
            foreach (var mem in UnionMembers)
            {
                if (mem.Matches(item_flags))
                {
                    Matched = mem;
                    return true;
                }
            }
            Matched = null;
            return false;
        }

        public override bool Matches(Item item)
        {
            return Matches(item.GetFlagInfo().Flags);
        }

        /// <summary>
        /// Check if the given flags are a fit for this category
        /// </summary>
        /// <param name="item_flags"> </param>
        /// <returns>This category if matched, null if not</returns>
        public override ItemCategory Match(IDictionary<string, int> item_flags)
        {
            return this.Matches(item_flags) ? this : null;
        }


        /// IComparer<Item> implementation
        /// using the sorting rules for the union members
        public override int Compare(Item t1, Item t2)
        {
            ItemCategory c1, c2;

            // I guess we'll just assume for now that the items actually belong in this union
            c1 = c2 = null;
            if (Matches(t1))
                c1 = Matched;
            if (Matches(t2))
                c2 = Matched;
            //
            // if either or both of the items do not belong to this union:
            if (c1 == null)
                return (c2 == null) ? 0 : -1;
            if (c2 == null)
                return 1;

            // get the (sub-)categories for the items; for now we're just going to assume
            // that each item belongs to one of the UnionMembers
            // I thought that might happen...infinite recursive oblivion!
            // var c1 = t1.GetCategory();
            // var c2 = t2.GetCategory();

            // if the items belong to the same category:
            if (c1 == c2)
                return c1.Compare(t1, t2);

            // or, if they're different categories, return the category order:
            return c1.CompareTo(c2);
        }

    }










}
