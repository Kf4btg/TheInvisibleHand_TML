using System;
using System.Collections.Generic;
using Terraria;

namespace InvisibleHand.Items.Categories.Types
{
    // public abstract class ItemCategory : IComparable<ItemCategory>, IEquatable<ItemCategory>, IComparer<Item>
    public abstract class ItemCategory : IMergeable<Item>
    {
        // Static stuff
        //------------------------------------------------------------------
        //------------------------------------------------------------------

        /// static registry of all categories keyed by their unique ID (an int)
        /// Each category is registered here during construction
        public static IDictionary<int, ItemCategory> Registry =
            new Dictionary<int, ItemCategory>();

        public static ISet<int> ActiveCategories { get; private set; } = new HashSet<int>();


        /// <summary>
        /// Add `newcategory` to the static Category Register (keyed by Category ID)
        /// </summary>
        public static void Register(ItemCategory newcategory)
        {
            Registry.Add(newcategory.ID, newcategory);
        }

        /// this is what will be returned when an item somehow doesn't match any defined category
        public static readonly ItemCategory None = new RegularCategory("Unknown", int.MaxValue, 0, priority: int.MaxValue);

        //------------------------------------------------------------------
        //------------------------------------------------------------------


        /*
        ██████  ██████   ██████  ██████  ███████ ██████  ████████ ██ ███████ ███████
        ██   ██ ██   ██ ██    ██ ██   ██ ██      ██   ██    ██    ██ ██      ██
        ██████  ██████  ██    ██ ██████  █████   ██████     ██    ██ █████   ███████
        ██      ██   ██ ██    ██ ██      ██      ██   ██    ██    ██ ██           ██
        ██      ██   ██  ██████  ██      ███████ ██   ██    ██    ██ ███████ ███████
        */

        /// the sort-priority for this category;
        /// this is now more of a "weight", used to modify the order of a
        /// category with regards to its siblings, thus changing the
        /// calculated ordinal value.
        // TODO: this probably needs a different name, seeing as how *increasing* priority values
        // actually *decreases* the sorting rank of a category, which is somewhat counterintuitive.
        // Something like "offset" or "shift" maybe.
        public int Priority { get; set; }

        /// A unique identifying number for this category, usually
        /// the order in which it was loaded from the definition file;
        /// used to solve conflicts in Priority and calculate the sort-ordinal.
        public int ID { get; private set; }

        /// a combination of the priority and ID that can be used
        /// to properly sort this category amongst a list of other categories.
        /// This is calculated and assigned when the category definitions are read
        /// from their configuration files. If the User changes the order of a category
        /// during gameplay (not yet implemented), this will need to be recalculated
        public int Ordinal { get; internal set; } // TODO: add checks to make sure this is always unique

        // public int Ordinal => (Priority << 16) | (ID);
        // id is now involved in the creation of this number, so no need to shift anything

        /// TODO: this will likely need to trigger a tree rebuild. If we can find a way to just rebuild
        /// a part of the tree rather than the whole thing, that would be preferable.
        protected bool _enabled = false;
        public virtual bool Enabled
        {
            get { return _enabled; }
            set {
                if (value && !_enabled)
                    ActiveCategories.Add(this.ID);
                else if (_enabled && !value)
                    ActiveCategories.Remove(this.ID);

                _enabled = value;
            }
        }

        public void ToggleEnabled() { Enabled ^= true; }

        /// The simple name of this category, not including any parent qualifiers.
        /// e.g.: 'Broadswords'
        public string Name { get; private set; }

        /// The full 'path name' of this category, starting with its toplevel ancestor;
        /// For example, if this is the 'Broadswords' category,
        /// the qualified name might be 'Weapon.Melee.Broadswords'
        public string QualifiedName {
            get {
                if (ParentID > 0)
                    return Parent.QualifiedName + "." + Name;
                return Name;
            }
        }

        public ICategory<Item> Parent
        {
            get { return ParentID > 0 ? Registry[this.ParentID] : null; }
            set { this.ParentID = value?.ID ?? 0; } // just save the ID, or 0 if null was passed
        }

        /// a parentID of 0 means this is a toplevel category
        public int ParentID { get; set; } = 0;

        /// the ID of the UnionCategory to which this category belongs.
        /// If this is 0, then the category has not been merged
        public int UnionID { get; protected set; } = 0;

        /// if not null, then this is the Wrapper-Category that this
        /// category has been merged into
        public IUnion<Item> CurrentUnion => UnionID > 0 ? Registry[UnionID] as UnionCategory : null;

        /// this is what should be used to get "this" category; will return the proper
        /// instance depending on whether this category has been merged or not.
        // public ItemCategory GetCategory => _union ?? this;
        // public ItemCategory GetCategory => UnionID > 0 ? Registry[UnionID] : this;

        public ICategory<Item> Category => UnionID > 0
                                        ? Registry[UnionID].Category // pass the 'Category' call up.
                                        : this;

        /*
         ██████  ██████  ███    ██ ███████ ████████ ██████  ██    ██  ██████ ████████  ██████  ██████  ███████
        ██      ██    ██ ████   ██ ██         ██    ██   ██ ██    ██ ██         ██    ██    ██ ██   ██ ██
        ██      ██    ██ ██ ██  ██ ███████    ██    ██████  ██    ██ ██         ██    ██    ██ ██████  ███████
        ██      ██    ██ ██  ██ ██      ██    ██    ██   ██ ██    ██ ██         ██    ██    ██ ██   ██      ██
         ██████  ██████  ██   ████ ███████    ██    ██   ██  ██████   ██████    ██     ██████  ██   ██ ███████
        */

        public ItemCategory(string name, int category_id, int parent_id = 0, int priority = 0)
        {
            Name = name;
            ParentID = parent_id;
            Priority = priority;
            ID = category_id;

            // default the ordinal to the ID; this is largely just to make sure the
            // 'None' category doesn't have an ordinal of 0
            Ordinal = ID;

            Register(this);
        }

        public ItemCategory(string name, int category_id, ItemCategory parent = null, int priority = 0)
            : this(name, category_id, parent?.ID ?? 0, priority) { }


        /*
        ███    ███ ███████ ██████   ██████  ██ ███    ██  ██████
        ████  ████ ██      ██   ██ ██       ██ ████   ██ ██
        ██ ████ ██ █████   ██████  ██   ███ ██ ██ ██  ██ ██   ███
        ██  ██  ██ ██      ██   ██ ██    ██ ██ ██  ██ ██ ██    ██
        ██      ██ ███████ ██   ██  ██████  ██ ██   ████  ██████
        */

        // public void Merge(UnionCategory union)
        /// Add this Category to a new Union. A category can only belong to
        /// one union at a time, though Unions themselves can belong to other unions.
        public void Merge(IUnion<Item> union)
        {
            Merge(union?.ID ?? 0);
        }
        public void Merge(int union_id)
        {
            if (union_id > 0) // && this.UnionID != union_id)
            {
                this.UnionID = union_id;
            }

            // try {
            //     Merge(Registry[union_id] as UnionCategory);
            // }
            // catch (KeyNotFoundException knfe)
            // {
            //     throw new UsefulKeyNotFoundException(union_id.ToString(), nameof(Registry), knfe,
            //         "The category '" + this.Name + "' could not be added to the Union with ID '{0}' because no category with that ID exists in '{1}'." );
            // }
        }

        /// Remove this Category from its Union
        public void Unmerge()
        {
            // this.current_union?.RemoveMember(this);
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

        /// <summary>
        /// This overload should return true if the matches the requirements for this category
        /// </summary>
        /// <param name="item"> Terraria.Item instance</param>
        /// <returns>  True if the item fits, false if not</returns>
        public abstract bool Matches(Item item);

        /// <summary>
        /// This overload should return true if the mapping of Trait-family-names to flag values
        /// satisfies the requirements for this category. These flags are pulled from the
        /// ItemInfo for a Terraria.Item
        /// </summary>
        /// <param name="item_flags"> mapping of Trait-family-names to flag values for an arbitrary Terraria.Item instance;
        /// which item the flags came from is irrelevant, as the return value only depends on checking the flag values
        /// help within the mapping. </param>
        /// <returns>True if the flags satisfy the conditions, false if not</returns>
        public abstract bool Matches(IDictionary<string, int> item_flags);

        /// <summary>
        /// If the mapping `item_flags` of Trait-family-names to flag values
        /// satisfies the requirements for this category, then a ICategory&lt;Item&gt; instance is returned.
        /// Whether it is this instance or, perhaps, a Union or other redirected instance depends on the
        /// circumstances and implementation.
        /// </summary>
        /// <param name="item_flags"> mapping of Trait-family-names to flag values for an arbitrary Terraria.Item instance;
        /// which item the flags came from is irrelevant, as the return value only depends on checking the flag values
        /// help within the mapping. </param>
        /// <returns>  An ICategory&lt;Item&gt; instance for which the given flags match, or `null` if the flags do not match</returns>
        public abstract ICategory<Item> Match(IDictionary<string, int> item_flags);

        /// <summary>
        /// Compares two Terraria.Item instances and determines if Item t1 is less than, greater than, or equal to Item t2.
        /// How the items are compared and what constitutes their values is determined by the deriving class.
        /// </summary>
        /// <param name="t1">A Terraria.Item instance </param>
        /// <param name="t2">A Terraria.Item instance </param>
        /// <returns>
        ///
        /// 0 if t1==t2,
        /// &lt; 0 if `t1` &lt; `t2`,
        /// &gt; 0 if `t1` &gt; `t2`.</returns>
        public abstract int Compare(Item t1, Item t2);

        #endregion


        #region interface implementations

        // public int CompareTo(ItemCategory other) => this.Ordinal.CompareTo(other.Ordinal);
        public int CompareTo(ICategory<Item> other) => this.Ordinal.CompareTo(other?.Ordinal);

        // public bool Equals(ItemCategory other) => this.ID == other?.ID;
        public bool Equals(ICategory<Item> other) => this.ID == other?.ID;

        public override bool Equals(Object other)
        {
            if (other == null) return false;

            ItemCategory other_cat = other as ItemCategory;

            return Equals(other_cat);
        }

        /// priority might change if the user modifies their preferred
        /// sort order; ID should remain immutable throughout the lifetime
        /// of this category.
        public override int GetHashCode() => this.ID;

        // public static bool operator ==(ItemCategory cat1, ItemCategory cat2)
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

        /// <summary>
        /// Print the category as:
        /// 	'Parent.Child1.Child2.This Category'(ID=100, ord=0A1B2C3F)
        /// Where the name in quotes is the fully-qualified name (the path
        /// from root to here) of this Category instance. `ID` is the category's unique
        /// identification number, and `ord` is the sorting-ordinal/order for this
        /// category (an integer encoded as 8 hexadecimal digits).
        /// </summary>
        public override string ToString()
        {
            return $"'{QualifiedName}'(ID={ID}, ord={Ordinal:X8})";
        }
    }
}
