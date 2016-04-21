using System.Collections.Generic;
// using System.Linq;
using Terraria.ModLoader;
using InvisibleHand.Definitions;
using InvisibleHand.Utils;


namespace InvisibleHand.Items
{
    public class ItemFlagInfo : ItemInfo
    {
        public IDictionary<string, int> Flags { get; set; }

        private ItemCategory _category;
        /// The category for an item is looked up the first time it is queried, then cached.
        public ItemCategory Category
        {
            get
            {
                return _category ?? getCategory();
            }
            // set;
        }

        /// Conflict resolution is currently done by using the first matching category
        // (at each tree level). To ensure that this is a meaningful action,
        // we need to assign priorities to the categories to sort them (or make sure
        // that they are placed into the tree in the same order they were loaded from
        // the definitions files)
        private ItemCategory getCategory()
        {
            var match = ItemCategory.None;
            CheckTree(IHBase.CategoryTree, ref match);

            // cache the value so we don't do the look up again
            _category = match;
            return match;

            // while (tree.HasChildren)
            // {
            //     // an enumerable of matching sub-trees for this level;
            //     // this equates to a breadth-first search
            //     var matches = from branch in tree
            //                   where branch.Value.Data.Matches(Flags)
            //                   select branch.Value;
            // }
        }

        private void CheckTree(SortedAutoTree<string, ItemCategory> tree, ref ItemCategory match)
        {
            foreach (var branch in tree)
            {
                var childtree = branch.Value;
                var category = childtree.Data;
                if (category.Matches(Flags))
                {
                    // if we found a match, update the reference
                    match = category;
                    // then check all the children of the matching category (if any)
                    // for a more specific match
                    CheckTree(childtree, ref match);

                    // whether a more specific match was found or not, break out of here
                    // because we've done our job.
                    break;
                }
            }


            // var matches = from branch in tree
            //               where branch.Value.Data.Matches(Flags)
            //               select branch.Value;
        }
    }
}
