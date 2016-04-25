using System.Collections.Generic;
using System;
using System.Linq;
// using System.Linq.Dynamic;
using Terraria;
using InvisibleHand.Utils;

namespace InvisibleHand
{
    public static class IHOrganizer
    {
        /// this will sort the categorized items first by category, then by
        /// more specific traits. Sorting Rules defined in CategoryDef class.
        public static IEnumerable<Item> OrganizeItems(IEnumerable<Item> source, bool reverse)
        {
            // Console.WriteLine($"OrganizeItems: {source}");
            // ConsoleHelper.PrintList(source, "OrganizeItems: source");
            if (source == null) return null;

            // returns an IEnumerable<IGrouping<ItemCat,Item>>
            // var byCategory =
            //     from item in source
            //     group item by item.GetCategory() into category
            //     orderby category.Key
            //     select category;

            // IEnumerable<IGrouping<string,Item>>

            try
            {

                // var catquery =
                //     from item in source
                //     let category = item.GetCategory()
                //     orderby category.Priority, category.Name
                //     group item by new { prio =  category.Priority, name = category.Name } into catgroup
                //     // orderby catgroup.Key, category.Name
                //     select new
                //     {
                //         Priority = catgroup.Key.prio,
                //         Name = catgroup.Key.name,
                //         // until we replace Dynamic Linq, just sort the categorized items by name
                //         Members = from im in catgroup
                //                   orderby im.name
                //                   select im
                //     };
                //

                // var catquery = //reverse ?
                // var catgroup = //reverse ?
                //     (from item in source
                //          //  let category = item.GetCategory()
                //      group item by item.GetCategory());

                var catquery =

                    reverse ?
                            (source.GroupBy(item => item.GetCategory())
                            .OrderByDescending(g => g.Key)
                            .SelectMany(g => g.OrderByDescending(i => i, g.Key)))
                        :
                            (source.GroupBy(item => item.GetCategory())
                            .OrderBy(g => g.Key)
                            .SelectMany(g => g.OrderBy(i => i, g.Key))) ;


                // var sortedcatgroup = catgroup.OrderByDescending(g => g.Key);

                //  orderby catgroup.Key descending
                //  select catgroup
                // .OrderBy(g => g.)
                // )
                // ;
                // orderby item.GetCategory(), item.name // TODO: item-sorting needs far more detailed rules than .name
                // orderby category, item.name // TODO: item-sorting needs far more detailed rules than .name
                // orderby category descending, category
                // select item)
                // select new { Category = category.QualifiedName, Priority = category.Priority, Item = item })
                // :
                //     (from item in source
                //     let category = item.GetCategory()
                //     // orderby item.GetCategory(), item.name // TODO: item-sorting needs far more detailed rules than .name
                //     orderby category, item.name // TODO: item-sorting needs far more detailed rules than .name
                //     select item);
                // select new { Category = category.QualifiedName, Priority = category.Priority, Item = item });


                var catlist = catquery.ToList();

                ConsoleHelper.PrintList(catlist, "Sorted items", true);

                // return catlist.Select(c => c.Item);
                return catquery; //.Select(c => c.Item);
                // return catquery.ToList();

                // orderby category.ordinal
                // group item by new { prio =  category.Priority, name = category.Name } into catgroup
                // group item by category.ordinal into catgroup
                // orderby catgroup.Key
                // from im in catgroup
                //     orderby im.name
                //     select im;

                // orderby catgroup.Key, category.Name
                // select new
                // {
                //     // Priority = catgroup.Key.prio,
                //     // Name = catgroup.Key.name,
                //     // until we replace Dynamic Linq, just sort the categorized items by name
                //     // Members =
                //     from im in catgroup
                //               orderby im.name
                //               select im
                // };



                // Console.WriteLine($"OrganizeItems: {catquery}");
                // ConsoleHelper.PrintList(catquery, "query");

                // var sortedList = new List<Item>();

                //
                // foreach (var category in catquery)
                // {
                //     Console.WriteLine($"OrganizeItems: category_name = {category.Name}");
                //     Console.WriteLine($"OrganizeItems: category_prio = {category.Priority}");
                //     // Console.WriteLine($"OrganizeItems: {category.Members}");
                //     // ConsoleHelper.PrintList(category.Members, "OrganizeItems: category_members");
                //
                //     sortedList.AddRange(category.Members);
                //     }
                //     return sortedList;
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

            // foreach (var category in catquery)
            // {
            //     // until we replace Dynamic Linq, just sort the categorized items by name
            //     var result =
            //         from item in category.Members
            //         orderby item.name
            //         select item;
            //
            //     sortedList.AddRange(result);
            // }

            //  Now we can dynamically construct Queries using Dynamic LINQ
            // expression methods with arbitrary (maybe later user-defined) sorting parameters.
            // foreach (var category in byCategory)
            // {
            //     // until we replace Dynamic Linq, just sort the categorized items by name
            //
            //     var result =
            //         from item in category
            //         orderby item.name
            //         select item;
            //
            //     sortedList.AddRange(result);
            //
            //     // var result = category.AsQueryable().OrderBy(Func<Item, TKey> keySelector, IComparer<TKey> comparer)
            //
            //     //     // pull the sorting rules for this category from the ItemSortRules dictionary, convert them to a
            //     //     // single string using "String.Join()", and pass it to the Dynamic LINQ OrderBy() method.
            //     //     var result = category.AsQueryable().OrderBy(String.Join(", ", CategoryDef.ItemSortRules[category.Key]));
            //     //
            //     //     // execute the query and put the result in a list to return
            //     //     foreach (var s_item in result)
            //     //         sortedList.Add(s_item);
            // }

        }

       /// <summary>
       /// Construct a list containing cloned copies of items in the given container, skipping blank (and optionally locked) slots.
       /// </summary>
       /// <param name="source_container">The Item[] array of the container</param>
       /// <param name="source_is_chest">Is the source container a chest? </param>

       /// <returns> The new list of copied items, or null if no items were
       /// applicable to be copied (NOT an empty list!).</returns>
        public static IList<Item> GetItemCopies(Item[] source_container, bool source_is_chest)
        {
            return GetItemCopies(source_container, source_is_chest, 0, source_container.Length -1);
        }

        /// <summary>
        /// Construct a list containing cloned copies of items in the given
        /// container, skipping blank (and optionally locked) slots.
        /// </summary>
        /// <param name="source_container">The Item[] array of the container</param>
        /// <param name="source_is_chest">Is the source container a chest? </param>
        /// <param name="rangeStart">index in source to start looking for items </param>
        /// <param name="rangeEnd">index in source to stop looking for items </param>
        /// <returns> The new list of copied items, or null if no items were
        /// applicable to be copied (NOT an empty list!).</returns>
        // public static List<Item> GetItemCopies(Item[] source_container, bool source_is_chest, Tuple<int,int> range = null)
        public static IList<Item> GetItemCopies(Item[] source_container, bool source_is_chest, int rangeStart, int rangeEnd)
        {

            // initialize the list that will hold the copied items
            var itemList = new List<Item>();

            int count = 0; //having trouble with empty lists...

            // get copies of viable items from container.
            // will need a different list if locking is enabled
            if (!source_is_chest)
            {
                for (int i=rangeStart; i<=rangeEnd; i++)
                {
                    // "favorited" is now what was "locked"
                    if (source_container[i].favorited || source_container[i].IsBlank()) continue;

                    itemList.Add(source_container[i].Clone());
                    count++;
                }
            }
            else //only skip blank slots
            {
                for (int i=rangeStart; i<=rangeEnd; i++)
                {
                    if (!source_container[i].IsBlank())
                    {
                        itemList.Add(source_container[i].Clone());
                        count++;
                    }
                }
            }

            Console.WriteLine($"GetItemCopies: Copied {count} items.");
            // return null if no items were copied to new list
            return count > 0 ? itemList : null;
        }

        /**
          SortPlayerInv - perform the sort operation on the items in the player's
        	inventory, excluding the hotbar and optionally any slots marked as locked

          @param player: The player whose inventory to sort.
          @param reverse: whether to reverse the list of item once it's sorted
        */
        public static void SortPlayerInv(Player player, bool reverse=false)
        {
            Console.WriteLine("SortPlayerInv({0}, {1})", player, reverse);
            ConsolidateStacks(player.inventory, 0, 49); //include hotbar in this step
            Console.WriteLine("return from consolidate stacks");

            Console.WriteLine("Calling Sort -- {0}, {1}, {2}, {3}, {4}", player.inventory, false, reverse, 10, 49);
            Sort(player.inventory, false, reverse, 10, 49);
        }

        // as above, but for the Item[] array of a chest
        public static void SortChest(Item[] chestitems, bool reverse=false)
        {
            Console.WriteLine("SortChest");
            ConsolidateStacks(chestitems);

            Sort(chestitems, true, reverse);
        }

        // as above, but given the actual chest object, pull out the array
        public static void SortChest(Chest chest, bool reverse=false)
        {
            Console.WriteLine("Calling Sortchest -- {0}, {1}", chest.item, reverse);
            SortChest(chest.item, reverse);
        }

        /**
          Sort Container

          @param container: The container whose contents to sort.
          @param chest : whether the container is a chest (otherwise the player inventory)
          @param rangeStart: starting index of the sort operation
          @param rangeEnd: end index of the sort operation

          Omitting both range arguments will sort the entire container.

        FIXME (maybe?): the "item-moved" sound plays even if the order doesn't change.
        */
        public static void Sort(Item[] container, bool chest, bool reverse)
        {
            Console.WriteLine("Sort called w/o range: {0}, {1}, {2}", container, chest, reverse);

            Sort(container, chest, reverse, 0, container.Length - 1);
        }

        public static void Sort(Item[] container, bool chest, bool reverse, int rangeStart, int rangeEnd)
        {
            Console.WriteLine("Sort(cont={0}, chest={1}, reverse={2}, rstart={3}, rend={4})",
             container, chest, reverse, rangeStart, rangeEnd);


            // get copies of the items and send them off to be sorted
            var sortedItemList = OrganizeItems(GetItemCopies(container, chest, rangeStart, rangeEnd), reverse);
            // Console.WriteLine("soretedlist = {0}", sortedItemList);
            if (sortedItemList == null)
                return;

            // if (reverse)
            //     sortedItemList.Reverse(); //reverse on user request

            // depending on user settings, decide if we copy items to end or beginning of container
            var fillFromEnd = chest ? IHBase.ModOptions["SortToEndChest"] : IHBase.ModOptions["SortToEndPlayer"];

            // set up the functions that will be used in the iterators ahead
            Func<int, int> getIndex, getIter;
            Func<int, bool> getCond; //, getWhileCond;

            if (fillFromEnd)	// use decrementing iterators
            {
                getIndex = x => rangeEnd - x;
                getIter = x => x-1;
                getCond = x => x >= rangeStart;
                // getWhileCond = x => x>rangeStart && container[x].favorited;
            }
            else 	// use incrementing iterators
            {
                getIndex = y => rangeStart + y;
                getIter = y => y+1;
                getCond = y => y <= rangeEnd;
                // getWhileCond = y => y<rangeEnd && container[y].favorited;
            }

            int filled = 0; // num of slots filled (or locked) so far
            if (!chest) // player inventory
            {
                // copy the sorted items back to the original container
                // (overwriting the current, unsorted contents)
                foreach (var item in sortedItemList)
                {
                    // find the first unlocked slot. this would throw an
                    // exception if rangeStart+filled somehow went over 49, but
                    // if the categorizer and favorting are functioning
                    // correctly, that _shouldn't_ be possible. Shouldn't.
                    // Probably.
                    while (container[getIndex(filled)].favorited)
                        filled++;

                    // now that we've found an unlocked slot, clone
                    // the next sorted item into it.
                    container[getIndex(filled++)] = item.Clone();
                    Sound.ItemMoved.Play();
                }
                // and the rest of the slots should be empty
                for (int i=getIndex(filled); getCond(i); i=getIter(i))
                {
                    // find the first unlocked slot.
                    if (container[i].favorited) continue;

                    container[i] = new Item();
                }
            }
            else // just throw 'em back in the box
            {
                foreach (var item in sortedItemList)
                {
                    container[getIndex(filled++)] = item.Clone();
                    Sound.ItemMoved.Play();
                }
                // and the rest of the slots should be empty
                for (int i=getIndex(filled); getCond(i); i=getIter(i))
                {
                    container[i] = new Item();
                }
            }
        } // sort()

        /*************************************************************************
        *  @params container, rangeStart, rangeEnd
        */
        public static void ConsolidateStacks(Item[] container, int rangeStart, int rangeEnd)
        {
            Console.WriteLine("ConsolidateStacks({0}, {1}, {2})", container, rangeStart, rangeEnd);
            for (int i = rangeEnd; i>=rangeStart; i--) //iterate in reverse
            {
                var item = container[i];

                //found non-blank item in a <full stack
                if (!item.IsBlank() && item.stack < item.maxStack)
                {
                    // search the remaining slots for other stacks of this item
                    StackItems(ref item, container, rangeStart, i-1);
                }
            }
        }

        public static void ConsolidateStacks(Item[] container)
        {
            Console.WriteLine("ConsolidateStacks({0})", container);
            ConsolidateStacks(container, 0, container.Length - 1);
        }

        /// called by ConsolidateStacks, this takes a single item and searches a
        /// subset of the original range for other non-max stacks of that item
        private static void StackItems(ref Item item, IList<Item> container, int rangeStart, int rangeEnd)
        {
            for (int j=rangeEnd; j>=rangeStart; j--) //iterate in reverse
            {
                var item2 = container[j];
                // found another <full stack of a matching item
                if (item.CanStackWith(item2))
                {
                    int diff = Math.Min(item2.maxStack - item2.stack, item.stack);
                    item2.stack += diff;
                    item.stack -= diff;

                    Sound.ItemMoved.Play();

                    if (item.stack == 0)
                    {
                        item = new Item();
                        break;
                    }
                }
            }
        }//\StackItems()
    }
}
