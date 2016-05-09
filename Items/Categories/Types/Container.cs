using System;
using System.Collections.Generic;
using Terraria;

namespace InvisibleHand.Items.Categories.Types
{
    /// A category that has no requirements, but is simply meant to be a "parent" to other categories.
    /// Intended mostly for organizational aid
    public class ContainerCategory : ItemCategory
    {

        public ContainerCategory(string name, int category_id, int parent_id = 0, int priority = 0) : base(name, category_id, parent_id, priority) {}

        public override bool Matches(IDictionary<string, int> item_flags)
        {
            return false;
        }

        public override bool Matches(Item item)
        {
            return false;
            // return Matches(item.GetFlagInfo().Flags);
        }

        public override ICategory<Item> Match(IDictionary<string, int> item_flags)
        {
            return null;
        }

        public override int Compare(Item t1, Item t2)
        {
            return -1;
        }
    }
}
