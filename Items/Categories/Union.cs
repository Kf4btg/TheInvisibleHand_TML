using System.Collections.Generic;
using Terraria;

namespace InvisibleHand.Items.Categories
{
    // public class UnionCategory : ItemCategory
    public class UnionCategory : ItemCategory, IUnion<ItemCategory>
    {

        /// this may be unnecessary; intended to hold the list of
        /// categories that have been merged to this one if this is a
        /// merge-container
        /// Item Category implements IComparable and GetHashCode, so this should be efficient
        public ISet<ItemCategory> UnionMembers { get; private set; }

        /// Returns the member category that matched the most recent Match()/Matches() check,
        /// or null if none did
        public ItemCategory Matched { get; private set; }


        public UnionCategory(string name, int cat_id, int parent_id = 0, int priority = 0, IEnumerable<ItemCategory> members=null) : base(name, cat_id, parent_id, priority)
        {
            UnionMembers = members == null ? new SortedSet<ItemCategory>() : new SortedSet<ItemCategory>(members);
        }

        public UnionCategory(string name, int cat_id, ItemCategory parent = null, int priority = 0, IEnumerable<ItemCategory> members=null) : this(name, cat_id, parent?.ID ?? 0, priority, members) {}


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

        public override bool Matches(Item item) => Matches(item.GetFlagInfo().Flags);

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

            c1 = c2 = null;
            if (Matches(t1))
                c1 = Matched;
            if (Matches(t2))
                c2 = Matched;

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
