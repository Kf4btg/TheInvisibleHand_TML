using System.Collections.Generic;
// using System.Linq;
using Terraria.ModLoader;
using InvisibleHand.Definitions;
using InvisibleHand.Utils;


namespace InvisibleHand.Items
{
    public class ItemFlagInfo : ItemInfo
    {
        // FIXME: Pre-existing items (i.e. loaded w/ the player) do not get run through set-defaults,
        // and so do not receive any flags.
        public IDictionary<string, int> Flags { get; set; }

        private ItemCategory _category = null;
        /// The category for an item is looked up the first time it is queried, then cached.
        public ItemCategory Category => _category ?? find_category();

        /// Conflict resolution is currently done by using the first matching category
        // (at each tree level), as determined by the ordinal value of the categories
        private ItemCategory find_category()
        {
            var match = ItemCategory.None;
            CheckTree(IHBase.CategoryTree, ref match);

            // cache the value so we don't do the look up again
            _category = match;
            return match;
        }

        // private void CheckTree(SortedAutoTree<string, ItemCategory> tree, ref ItemCategory match)
        private void CheckTree(SortedAutoTree<int, ItemCategory> tree, ref ItemCategory match)
        {
            // sorted by category.ordinal
            foreach (var branch in tree)
            {
                var childtree = branch.Value; // The AutoDict container
                var category = childtree.Data; // The Category object this AutoDict represents
                if (category.Matches(Flags))
                {
                    // if we found a match, update the reference
                    // (use .Category to make sure we get the wrapper-category
                    // if the matching one has been merged)
                    // TODO: or should "Merge" categories only matter for smart-deposit?
                    // match = category.Category;

                    match = category.Category;

                    // then check all the children of the matching category (if any)
                    // for a more specific match
                    // if (childtree.HasChildren)
                    // {
                        CheckTree(childtree, ref match);

                    // }

                    // whether or not we found a more specific match, break out of here
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
