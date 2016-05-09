using System;
using System.Collections.Generic;
using Terraria;

namespace InvisibleHand.Items.Categories.Types
{
    /// A category that has no requirements, but is simply meant to be a "parent" to other categories.
    /// Intended mostly as an organizational aid.
    /// Inherits the "Sorter" class so that it can have rules defined on it to pass down to children
    public class ContainerCategory : Sorter
    {
        public override bool Enabled
        {
            /// these do not get added to "ActiveCategories"
            set { _enabled = value; }
        }

        public ContainerCategory(string name, int category_id, int parent_id = 0, int priority = 0) : base(name, category_id, parent_id, priority) {}

        // these methods shouldn't be called for these categories

        public override bool Matches(IDictionary<string, int> item_flags)
        {
            throw new NotImplementedException();
            // return false;
        }

        public override bool Matches(Item item)
        {
            // return false;
            throw new NotImplementedException();
            // return Matches(item.GetFlagInfo().Flags);
        }

        public override ICategory<Item> Match(IDictionary<string, int> item_flags)
        {
            // return null;
            throw new NotImplementedException();
        }

        /// Some sort of default comparison: sort by type then stack then value
        public override int Compare(Item t1, Item t2)
        {
            int res = t1.type.CompareTo(t2.type);

            if (res == 0)
                res = t1.stack.CompareTo(t2.stack);
            return res == 0 ? t1.value.CompareTo(t2.value) : res;
            // throw new NotImplementedException();
            // return -1;
        }
    }
}
