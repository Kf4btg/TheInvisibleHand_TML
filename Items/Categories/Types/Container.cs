// using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;

namespace InvisibleHand.Items.Categories.Types
{
    /// A category that has no requirements, but is simply meant to be a "parent" to other categories.
    /// Intended mostly as an organizational aid.
    /// Inherits the "Sorter" class so that it can have rules defined on it to pass down to children
    public class ContainerCategory : ItemSorter
    {
        public override bool Enabled
        {
            /// these do not get added to "ActiveCategories"
            set { _enabled = value; }
        }


        private List<ItemCategory> direct_children = null;
        /// don't use this very often...
        /// the result is cached, but the initial lookup may be slow
        public IEnumerable<ItemCategory> Children
        {
            get {
                if (direct_children == null)
                    direct_children = ItemCategory.Registry.Values.Where(cat => cat.ParentID == this.ID).ToList();
                return direct_children;
            }
        }

        public ContainerCategory(string name, int category_id, int parent_id = 0, int priority = 0) : base(name, category_id, parent_id, priority) {}

        // these methods shouldn't be called for these categories

        public override bool Matches(IDictionary<string, int> item_flags)
        {
            foreach (var c in Children)
            {
                if (c.Matches(item_flags)) return true;
            }

            // throw new NotImplementedException();
            return false;
        }

        public override bool Matches(Item item)
        {
            // return false;
            // throw new NotImplementedException();
            return Matches(item.GetFlagInfo().Flags);
        }

        public override ICategory<Item> Match(IDictionary<string, int> item_flags)
        {
            // return null;
            foreach (var c in Children)
            {
                if (c.Matches(item_flags)) return c;
            }
            return null;
            // throw new NotImplementedException();
        }

        /// This should find the child category that owns each item and use those (or that) to
        /// determine the return value; this is a somewhat involved call, so shouldn't be used all that often
        public override int Compare(Item t1, Item t2)
        {
            ItemCategory c1 = null;
            ItemCategory c2 = null;

            bool both_valid = false;
            foreach (var c in Children)
            {
                if (c1 == null && c.Matches(t1))
                    c1 = c;
                if (c2 == null && c.Matches(t2))
                    c2 = c;

                // stop when both are filled
                both_valid = (c1 != null && c2 != null);

                if (both_valid) break;
            }

            if (both_valid)
            {
                return (c1 == c2) ? c1.Compare(t1, t2) : c1.CompareTo(c2);
            }

            // Some sort of default comparison: sort by type then stack then value
            int res = t1.type.CompareTo(t2.type);

            if (res == 0)
                res = t1.stack.CompareTo(t2.stack);
            return res == 0 ? t1.value.CompareTo(t2.value) : res;
            // throw new NotImplementedException();
            // return -1;
        }
    }
}
