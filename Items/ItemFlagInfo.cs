using System.Collections.Generic;
// using System.Linq;
using Terraria.ModLoader;
// using Terraria;
// using InvisibleHand.Definitions;
using InvisibleHand.Utils;
using InvisibleHand.Items.Categories.Types;

namespace InvisibleHand.Items
{
    public class ItemFlagInfo : ItemInfo
    {
        /// Storage for this item's flags (determined by classification);
        /// keyed by the Trait-family name (e.g. "General", "Weapon", "Consumable", etc.)
        public IDictionary<string, int> Flags { get; set; }

        private ItemCategory _category = null;

        /// if something needs access to the REAL, non-redirected category, use this.
        public ItemCategory ActualCategory => _category ?? find_category();

        /// The category for an item is looked up the first time it is queried, then cached.
        /// The category returned may be redirected if the item's primary category has been
        /// merged into a different union category.
        public ItemCategory Category => ActualCategory.Category;
        // public ItemCategory Category => _category?.Category ?? find_category().Category;

        /// Conflict resolution is currently done by using the first matching category
        /// (at each tree level), as determined by the ordinal value of the categories
        private ItemCategory find_category()
        {
            // create the eventual return value with a default 'unknown' category
            ItemCategory match = ItemCategory.None;
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

                // when traversing the tree, always be sure to call Matches() with the Flags
                // collection rather than the Item itself
                if (category.Matches(Flags))
                {
                    // if we found a match, update the reference
                    // (use .Category to make sure we get the wrapper-category
                    // if the matching one has been merged)
                    // TODO: or should "Merge" categories only matter for smart-deposit?
                    // match = category.Category;

                    // match = category.Category;
                    // actually, store the REAL category, but report the wrapper, if any
                    match = category;

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
