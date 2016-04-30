using System;
using System.Collections.Generic;
using Terraria;

namespace InvisibleHand.Items.Categories
{
    // public abstract class ItemCategory : IComparable<ItemCategory>, IEquatable<ItemCategory>, IComparer<Item>
    public abstract class ItemCategory : ICategory<Item>, IMergeable<IUnion<ItemCategory>, ItemCategory>
    {
        // Static stuff
        //------------------------------------------------------------------
        //------------------------------------------------------------------

        /// static registry of all categories keyed by their unique ID (an int)
        /// Each category is registered here during construction
        public static IDictionary<int, ItemCategory> Registry =
            new Dictionary<int, ItemCategory>();

        /// <summary>
        /// Add to the static Category Register (keyed by Category ID)
        /// </summary>
        public static void Register(ItemCategory newcategory)
        {
            Registry.Add(newcategory.ID, newcategory);
        }

        //------------------------------------------------------------------
        //------------------------------------------------------------------

        /// the sort-priority for this category;
        /// this is now more of a "weight", used to modify the order of a
        /// category with regards to its siblings, thus changing the
        /// calculated ordinal value.
        public int Priority { get; set; }

        /// A unique identifying number for this category; usually
        /// the order in which this item was loaded from the definition file;
        /// used to help solve conflicts in Priority.
        public int ID { get; private set; }

        /// a combination of the priority and ID that can be used
        /// to properly sort this category amongst a list of other categories
        // public int Ordinal => (Priority << 16) | (ID);
        // id is now included in the creation of this number, so no need to shift anything
        // TODO: add checks to make sure this is always unique
        public int Ordinal { get; set; }

        public string Name { get; private set; }
        public string QualifiedName {
            get {
                if (ParentID > 0)
                    return Parent.QualifiedName + "." + Name;
                return Name;
            }
        }

        /// the parent's requirements will have already been merged into
        /// this Category's requirements, so this property is really only
        /// here to help with determining sort-order.
        // public ItemCategory Parent
        public ICategory<Item> Parent
        {
            get { return ParentID > 0 ? Registry[this.ParentID] : null; }
            set { this.ParentID = value?.ID ?? 0; }
        }

        /// a parentID of 0 means this is a toplevel category
        public int ParentID { get; set; } = 0;

        /// the ID of the UnionCategory to which this category belongs.
        /// If this is 0, then the category has not joined a Union.
        public int UnionID { get; protected set; } = 0;

        /// if not null, then this is the Wrapper-Category that this
        /// category has been merged into
        protected UnionCategory _union => UnionID > 0 ? Registry[UnionID] as UnionCategory : null;

        /// this is what should be used to get "this" category; will return the proper
        /// instance depending on whether this category has been merged or not.
        // public ItemCategory GetCategory => _union ?? this;
        public ItemCategory GetCategory => UnionID > 0 ? Registry[UnionID] : this;
        public ICategory<Item> Category => UnionID > 0 ? Registry[UnionID] : this;

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
        public void Merge(IUnion<ItemCategory> union)
        {
            if (union != null)
            {
                // this._union = union;
                this.UnionID = union.ID;
                union.AddMember(this);
            }
        }
        public void Merge(int union_id)
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

        // public int CompareTo(ItemCategory other) => this.Ordinal.CompareTo(other.Ordinal);
        public int CompareTo(ICategory<Item> other) => this.Ordinal.CompareTo(other.Ordinal);

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

        public abstract int Compare(Item t1, Item t2);

        #endregion

        /// this is what will be returned when an item somehow doesn't match any defined category
        public static readonly ItemCategory None = new RegularCategory("Unknown", int.MaxValue, null, null, int.MaxValue);

        public override string ToString()
        {
            return $"'{QualifiedName}'(ID={ID}, ord={Ordinal:X8})";
        }
    }
}
