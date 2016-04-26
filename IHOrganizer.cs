using System.Collections.Generic;
using System;
using System.Linq;
using Terraria;
using InvisibleHand.Utils;

namespace InvisibleHand
{
    public static class IHOrganizer
    {
        /// maximum container indices
        internal static readonly int InventoryMax;
        internal static readonly int ChestMax;

        /// minumun container indices
        internal static readonly int InventoryMin;
        internal static readonly int ChestMin;

        /// minimum index when excluding the hotbar
        internal static readonly int InventoryNoHBMin;

        static IHOrganizer()
        {
            InventoryMax = Main.realInventory - 1;
            ChestMax = Chest.maxItems - 1;

            InventoryMin = ChestMin = 0;
            InventoryNoHBMin = 10;
        }

        /// this will sort the categorized items first by category, then by
        /// more specific traits. Sorting Rules defined in CategoryDef class.
        private static IEnumerable<Item> OrganizeItems(IEnumerable<Item> source, bool reverse)
        {
            if (source == null) return null;

            try
            {
                var catquery =

                    reverse ?
                            (source.GroupBy(item => item.GetCategory())
                            .OrderByDescending(g => g.Key)
                            .SelectMany(g => g.OrderByDescending(i => i, g.Key)))
                        :
                            (source.GroupBy(item => item.GetCategory())
                            .OrderBy(g => g.Key)
                            .SelectMany(g => g.OrderBy(i => i, g.Key))) ;

                // var catlist = catquery.ToList();
                //
                ConsoleHelper.PrintList(catquery.Select(i => new { Category = i.GetCategory(), i }), "Sorted items", true);

                return catquery; //.Select(c => c.Item);

            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

       /// <summary>
       /// Construct a list containing cloned copies of items in the given container, skipping blank (and optionally locked) slots.
       /// </summary>
       /// <param name="source_container">The Item[] array of the container</param>
       /// <param name="source_is_chest">Is the source container a chest? </param>
       /// <returns> The new list of copied items, or null if no items were
       /// applicable to be copied (NOT an empty list!).</returns>
        private static IList<Item> GetItemCopies(Item[] source_container, bool source_is_chest)
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
        private static IList<Item> GetItemCopies(Item[] source_container, bool source_is_chest, int rangeStart, int rangeEnd)
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
            // Console.WriteLine($"GetItemCopies: Copied {count} items.");
            // return null if no items were copied to new list
            return count > 0 ? itemList : null;
        }

        /**
          SortPlayerInv - perform the sort operation on the items in the player's
        	inventory, excluding the hotbar and optionally any slots marked as locked

          @param player: The player whose inventory needs sorting.
          @param reverse: whether to reverse the list of items once it's sorted
        */
        public static void SortPlayerInv(Player player, bool reverse=false)
        {
            // Console.WriteLine("SortPlayerInv({0}, {1})", player, reverse);
            ConsolidateStacks(player.inventory, InventoryMin, InventoryMax); //include hotbar in this step

            // Console.WriteLine("Calling Sort -- {0}, {1}, {2}, {3}, {4}", player.inventory, false, reverse, InventoryNoHBMin, InventoryMax);
            Sort(player.inventory, false, reverse, InventoryNoHBMin, InventoryMax);
        }

        // as above, but for the Item[] array of a chest
        public static void SortChest(Item[] chestitems, bool reverse=false)
        {
            // Console.WriteLine("SortChest");
            ConsolidateStacks(chestitems);

            Sort(chestitems, true, reverse, ChestMin, ChestMax);
        }

        // as above, but given the actual chest object, pull out the array
        public static void SortChest(Chest chest, bool reverse=false)
        {
            // Console.WriteLine("Calling Sortchest -- {0}, {1}", chest.item, reverse);
            SortChest(chest.item, reverse);
        }

        /**
          Sort Container

          @param container: The container whose contents to sort.
          @param chest : whether the container is a chest (otherwise the player inventory)
          @param rangeStart: starting index of the sort operation
          @param rangeEnd: end index of the sort operation

          Omitting both range arguments will sort the entire container.

        TODO (maybe?): the "item-moved" sound plays even if the order doesn't change.
        */
        // FIXME: currently doesn't work for banks
        public static bool Sort(Item[] container, bool chest, bool reverse, int rangeStart, int rangeEnd)
        {
            Console.WriteLine("Sort(cont={0}, chest={1}, reverse={2}, rStart={3}, rEnd={4})",
             container, chest, reverse, rangeStart, rangeEnd);

            // get copies of the items and send them off to be sorted
            var sortedItemList = OrganizeItems(GetItemCopies(container, chest, rangeStart, rangeEnd), reverse);

            return sortedItemList != null &&
                chest ?
                    // depending on user settings, decide if we copy items to end or beginning of container
                    IHBase.ModOptions["SortToEndChest"] ?
                        // use decrementing iterators
                        _doSort_Chest(container, sortedItemList, x => rangeEnd - x, x => x - 1, x => x >= rangeStart) :
                        // use incrementing iterators
                        _doSort_Chest(container, sortedItemList, x => rangeStart + x, x => x + 1, x => x <= rangeEnd)
                : //player
                    IHBase.ModOptions["SortToEndPlayer"] ?
                        _doSort_Player(container, sortedItemList, x => rangeEnd - x, x => x - 1, x => x >= rangeStart) :
                        _doSort_Player(container, sortedItemList, x => rangeStart + x, x => x + 1, x => x <= rangeEnd);
        }

        /// perform the sort for the player inventory, taking favorited items into consideration.
        // The Func<> arguments will depend on the value of the "SortToEndPlayer" ModOption
        // (i.e. they will represent either incrementing or decrementing iterators)
        private static bool _doSort_Player(Item[] container, IEnumerable<Item> sortedList, Func<int, int> getIndex, Func<int, int> getIter, Func<int, bool> getCond)
        {
            int filled = 0; // num of slots filled (or locked) so far
            // copy the sorted items back to the original container
            // (overwriting the current, unsorted contents)
            foreach (var item in sortedList)
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

                // Sound.ItemMoved.Play();
            }
            // and the rest of the slots should be empty
            for (int i=getIndex(filled); getCond(i); i=getIter(i))
            {
                // find the first unlocked slot.
                if (container[i].favorited) continue;

                container[i] = new Item();
            }

            // TODO: catch any exceptions during sorting and return false
            return true;
        }

        /// perform the sort for a chest inventory; no need to worry about favorited items here.
        /// The Func<> arguments will depend on the value of the "SortToEndChest" ModOption
        /// (i.e. they will represent either incrementing or decrementing iterators)
        private static bool _doSort_Chest(Item[] container, IEnumerable<Item> sortedList, Func<int, int> getIndex, Func<int, int> getIter, Func<int, bool> getCond)
        {
            int filled = 0; // num of slots filled (or locked) so far

            foreach (var item in sortedList)
            {
                container[getIndex(filled++)] = item.Clone();
                // Sound.ItemMoved.Play();
            }
            // and the rest of the slots should be empty
            for (int i=getIndex(filled); getCond(i); i=getIter(i))
            {
                container[i] = new Item();
            }

            // TODO: catch any exceptions during sorting and return false
            return true;
        }

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

                    // Sound.ItemMoved.Play();

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
