using System.Collections.Generic;
// using System.Linq;
using Terraria.ModLoader;
using Terraria;
// using InvisibleHand.Definitions;
using InvisibleHand.Utils;
using InvisibleHand.Items.Categories;

namespace InvisibleHand.Items
{
    public class ItemFlagInfo : ItemInfo
    {
        /// Storage for this item's flags (determined by classification);
        /// keyed by the Trait-family name (e.g. "General", "Weapon", "Consumable", etc.)
        public IDictionary<string, int> Flags { get; set; }

        private ICategory<Item> _category = null;
        /// The category for an item is looked up the first time it is queried, then cached.
        public ICategory<Item> Category => _category ?? find_category();

        /// Conflict resolution is currently done by using the first matching category
        /// (at each tree level), as determined by the ordinal value of the categories
        private ICategory<Item> find_category()
        {
            // create the eventual return value with a default 'unknown' category
            ICategory<Item> match = ItemCategory.None;
            CheckTree(IHBase.CategoryTree, ref match);

            // cache the value so we don't do the look up again
            _category = match;
            return match;
        }

        // private void CheckTree(SortedAutoTree<string, ItemCategory> tree, ref ItemCategory match)
        private void CheckTree(SortedAutoTree<int, ICategory<Item>> tree, ref ICategory<Item> match)
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
