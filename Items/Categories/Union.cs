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

        // private bool _enabled = true;
        public override bool Enabled {
            // get { return _enabled; }
            set
            {
                // if we're disabling this union,
                // unmerge all the merged categories
                if (_enabled && !value)
                {
                    foreach (var mem in UnionMembers)
                        mem.Unmerge();
                }
                // or if we're [re]enabling it, activate the members
                else if (value && !_enabled)
                {
                    foreach (var mem in UnionMembers)
                        mem.Merge(this);
                }
                // don't call base.Enabled=... because we don't want
                // to add UnionCategories to the ActiveCategories list
                _enabled = value;
            }
        }


        public UnionCategory(string name, int cat_id, int parent_id = 0, int priority = 0, IEnumerable<ItemCategory> members=null) : base(name, cat_id, parent_id, priority)
        {
            UnionMembers = members == null ? new SortedSet<ItemCategory>() : new SortedSet<ItemCategory>(members);
        }

        public UnionCategory(string name, int cat_id, ItemCategory parent = null, int priority = 0, IEnumerable<ItemCategory> members=null) : this(name, cat_id, parent?.ID ?? 0, priority, members) {}


        // Tracking member Categories
        // ---------------------------
        // public void AddMember(ItemCategory new_member)
        // {
        //     this.UnionMembers.Add(new_member);
        // }

        /// Add a category as a member of this union. If the given category is already in a different
        /// union, an exception will be thrown unless `force_replace` is true, in which case
        /// the category will be removed from the memberset of its current union before being added
        /// to this one.
        public void AddMember(ItemCategory new_member, bool force_replace = false)
        {
            if (new_member.UnionID > 0 && new_member.UnionID != this.ID)
            {
                if (force_replace)
                    new_member.CurrentUnion.RemoveMember(new_member);
                else
                    throw new NoDualUnionsException($"The category {new_member.Name} already belongs to a union and cannot join another.");
            }
            this.UnionMembers.Add(new_member);

        }

        /// remove a category from the memberset of this Union. No error will be thrown
        /// if the category is not a member.
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
            if (Enabled)
                foreach (var mem in UnionMembers)
                    if (mem.Matches(item_flags))
                    {
                        Matched = mem;
                        return true;
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
        public override ICategory<Item> Match(IDictionary<string, int> item_flags)
        {
            return this.Matches(item_flags) ? this.Category : null;
        }

        /// IComparer<Item> implementation
        /// using the sorting rules for the union members
        public override int Compare(Item t1, Item t2)
        {
            // ItemCategory c1, c2;
            //
            // c1 = c2 = null;
            // if (Matches(t1))
            //     c1 = Matched;
            // if (Matches(t2))
            //     c2 = Matched;
            //
            // // if either or both of the items do not belong to this union:
            // if (c1 == null)
            //     return (c2 == null) ? 0 : -1;
            // if (c2 == null)
            //     return 1;

            // get the (sub-)categories for the items; for now we're just going to assume
            // that each item belongs to one of the UnionMembers
            // I thought that might happen...infinite recursive oblivion!


            // Ok, since the Unions are no longer in the Category Tree, it should
            // be safe to do this:
            var c1 = t1.GetFlagInfo().ActualCategory;
            var c2 = t2.GetFlagInfo().ActualCategory;

            // if the items belong to the same category, get the comparison value from that category;
            // if they're different categories, just return the category order:
            return c1 == c2 ? c1.Compare(t1, t2) : c1.CompareTo(c2);


            // if the items belong to the same category:
            // if (c1 == c2)
            //     return c1.Compare(t1, t2);
            // or, if they're different categories, return the category order:

            // return c1.CompareTo(c2);
        }
    }

}
