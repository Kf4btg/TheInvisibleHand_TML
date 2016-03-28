using System.Collections.Generic;
using System;
// using Terraria.ModLoader;
using Terraria;
// using Terraria.ID;

namespace InvisibleHand
{

    /// A Class defining a group of items that can be considered 'similar', for an arbitary definition of similar, and will grouped together in an inventory container. A category can contain sub-categories to further define the sorting relationships. Each sub-category must be a true subset of the items in the super-category.
    /// For example, a 'Tool' Category may contain all pickaxes, hammers, and axes, but 'Pick', 'Hammer', and 'Axe' could each be subcategories of 'Tool'.
    public class Category : IComparable<Category>
    {
        // private string _name;
        // private int _primarykey;

        public string Name { get; private set; }

        /// This defines the primary order by which this category will sorted relative to other categories at its level. Should be unique for each category.
        public int PrimarySortKey { get; set; }

        /// list of item ids that fall within this category
        public ISet<int> Members = new HashSet<int>();

        /// This is the function that will be called to sort the items in the player's inventory matching this category relative to one another.
        public Func<Item, Item, int> ItemSortKey { get; protected set; }

        /// The function passed for this parameter will determine what Item Ids/Types get associated with this category.
        public Func<Item, bool> Matches { get; protected set; }


        public Category(string name, int primary_key, Func<Item, bool> matcher, Func<Item, Item, int> sorter)
        {
            // TODO: verify uniqueness of primary_key

            Name = name;
            PrimarySortKey = primary_key;

            Matches = matcher;
            ItemSortKey = sorter;
        }

        /// Associate an item id with this category; it is added to the Category's 'Members' list
        public void IncludeItem(int item_type) => Members.Add(item_type);

        /// Whether an item-type belongs to this category
        public bool Includes(int item) => Members.Contains(item);



        public int CompareTo(Category other)
        {
            // check if other is valid (is this unnecessary?)
            return PrimarySortKey.CompareTo(other?.PrimarySortKey);
        }


    }



}
