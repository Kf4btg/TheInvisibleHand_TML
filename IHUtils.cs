using System;
// using Microsoft.Xna.Framework;
// using TAPI.UIKit;
using Terraria;
// using Terraria.UI;
using InvisibleHand.Utils;

namespace InvisibleHand
{

    public static class IHUtils
    {

        // FIXME: ITEMSLOT STUFF IS HAAAARRRD NOW!!! I'm not sure how to get the item in the slot...I'm not sure about anything related to this, anymore, to be honest...

        //------------------------------------------------------//
        //-----------CALLABLES FOR ITEM SLOT ACTIONS------------//
        //------------------------------------------------------//
        //  These methods are intended for use when             //
        //  "shifting" the item contained in an ItemSlot to a   //
        //  different container, as via the capability bound to //
        //  Shift+Right-click in IHInterface.                   //
        //------------------------------------------------------//
        //  Members:                                            //
        //    bool ShiftToChest(ref ItemSlot)                   //
        //    bool ShiftToPlayer(ref ItemSlot, bool)            //
        //    bool ShiftToPlayer(ref ItemSlot, int, int,        //
        //          bool, bool)                                 //
        //------------------------------------------------------//

        /**************************************************************
        *   move item from player inventory slot to chest
        *   returns true if item moved/itemstack emptied
        */
        // public static bool ShiftToChest(ref ItemSlot slot)
        // {
        //     // var player = IHPlayer.LocalPlayer;
        //     bool sendMessage = player.chest > -1;
        //     var chestItems = Main.chest[player.chest].item;
        //
        //     var pItem = slot.MyItem;
        //
        //     int retIdx = -1;
        //     if (pItem.stack == pItem.maxStack) //move non-stackable items or full stacks to empty slot.
        //     {
        //         retIdx = MoveToFirstEmpty( pItem, chestItems, 0,
        //                                     i => i<Chest.maxItems,
        //                                     i => i+1
        //                                 );
        //     }
        //
        //     // if we didn't find an empty slot...
        //     if (retIdx < 0)
        //     {
        //         //we can't stack it, so we already know there's no place for it.
        //         if (pItem.maxStack == 1) return false;
        //
        //         retIdx = MoveItemP2C(ref pItem, chestItems, sendMessage);
        //
        //         // still didn't find an empty slot...
        //         if (retIdx < 0)
        //         {
        //             // ...but, partial success (stack amt changed), though we
        //             // don't want to reset the item.
        //             if (retIdx == -1)
        //             {
        //                 Sound.ItemMoved.Play();
        //                 Recipe.FindRecipes();
        //             }
        //             return false;
        //         }
        //     }
        //     //else, success!
        //     Sound.ItemMoved.Play();
        //     slot.MyItem = new Item();
        //     // if (sendMessage) SendNetMessage(retIdx);
        //     return true;
        // }
        //
        // /// MoveChestSlotItem - moves item from chest/guide slot to player inventory
        // public static bool ShiftToPlayer(ref ItemSlot slot, bool sendMessage)
        // {
        //     //TODO: check for quest fish (item.uniqueStack && player.HasItem(item.type))
        //     var cItem = slot.MyItem;
        //
        //     if (cItem.IsBlank()) return false;
        //
        //     if (cItem.Matches(ItemCat.COIN)) {
        //         // don't bother with "shifting", just move it as usual
        //         slot.MyItem = player.GetItem(Main.myPlayer, slot.MyItem);
        //         return (slot.MyItem.IsBlank());
        //     }
        //
        //     // reminder: ShiftToPlayer returns true if original item ends up empty
        //     if (cItem.Matches(ItemCat.AMMO)) {
        //         if (cItem.maxStack > 1
        //         && cItem.stack == cItem.maxStack
        //         && ShiftToPlayer(ref slot, 54, 57, sendMessage, false)) //ammo goes top-to-bottom
        //             return true;
        //     }
        //
        //     // if it's a stackable item and the stack is *full*, just shift it.
        //     else if (cItem.maxStack > 1 && cItem.stack==cItem.maxStack){
        //         if (ShiftToPlayer(ref slot,  0,  9, sendMessage, false) //try hotbar first, ascending order (vanilla parity)
        //         ||  ShiftToPlayer(ref slot, 10, 49, sendMessage,  true)) return true; //then the other slots, descending
        //     }
        //
        //     //if all of the above failed, then we have no empty slots.
        //     // Let's save some work and go traditional:
        //     slot.MyItem = player.GetItem(Main.myPlayer, slot.MyItem);
        //     return (slot.MyItem.IsBlank());
        // }
        //
        // /// attempts to move an item to an empty slot (returns success status)
        // private static bool ShiftToPlayer(ref ItemSlot slot, int ixStart, int ixStop, bool sendMessage, bool desc)
        // {
        //     int iStart;
        //     Func<int,bool> iCheck;
        //     Func<int,int> iNext;
        //
        //     if (desc) {
        //         iStart =  ixStop;
        //         iCheck = i => i >= ixStart;
        //         iNext = i => i-1;
        //     }
        //     else {
        //         iStart = ixStart;
        //         iCheck = i => i <=  ixStop;
        //         iNext = i => i+1;
        //     }
        //
        //     int retIdx = MoveToFirstEmpty( slot.MyItem, player.inventory, iStart, iCheck, iNext );
        //     if (retIdx >= 0)
        //     {
        //         Sound.ItemMoved.Play();
        //         slot.MyItem = new Item();
        //         // if (sendMessage) SendNetMessage(retIdx);
        //         return true;
        //     }
        //     return false;
        // }

    #region helperfunctions

        //------------------------------------------------------//
        //--------------------HELPER METHODS--------------------//
        //------------------------------------------------------//
        //  Common pieces of the callables broken down into     //
        //  smaller functions.                                  //
        //------------------------------------------------------//
        //  Members:                                            //
        //    int MoveItemP2C(ref Item, Item[], bool, bool)     //
        //    int MoveToFirstEmpty(Item, Item[], int,           //
        //          Func<int,bool>, Func<bool,bool>)            //
        //    int TryStackMerge(ref Item, Item[], bool, int,    //
        //          Func<int,bool>, Func<bool,bool>)            //
        //------------------------------------------------------//

        public static void MoveItem(ref Item item, Item[] target_container, int min_index, int max_index, bool desc = false)
        {
            // if the item is fully or partially moved, play the ItemMoved Sound
            if (moveItem(ref item, target_container, min_index, max_index, desc))
                Sound.ItemMoved.Play();
        }

        /// returns True if item is fully or partially moved, false if no movement or stack change occurs
        private static bool moveItem(ref Item item, Item[] target_container, int min_index, int max_index, bool desc)
        {
            int moved_to_index;
            int orig_stack_amt = item.stack; // save current stack amount

            if ((item.maxStack > 1 &&
                    TryMergeStackToContainer(item, target_container, min_index, max_index,
                                             desc, out moved_to_index))
                || MoveToFirstEmpty(item, target_container, min_index, max_index, desc, out moved_to_index))
            {
                // item fully moved
                item = new Item(); // replace it
            }
            return orig_stack_amt != item.stack; //exit status
        }

        /********************************************************
        *   Helper Helpers? - functions used in the above helper methods
        */

        public static bool CopyIfEmpty(Item src_item, ref Item dest_item)
        {
            if (dest_item.IsBlank())
            {
                dest_item = src_item.Clone();
                return true;
            }
            return false;
        }

        // @return: >=0 (index) if move succeeded; -1 if failed
        /// <summary>
        /// Will move the given item into a destination array.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dest"></param>
        /// <param name="iStart"></param>
        /// <param name="iCheck"></param>
        /// <param name="iNext"></param>
        /// <returns>bigger than -1 if move succeeded (index in destination to which item was moved); -1 if failed</returns>
        // public static int MoveToFirstEmpty(Item item, Item[] dest, int iStart, Func<int, bool> iCheck, Func<int,int> iNext)
        //

        /// Attempts to move the item into an empty slot in the destination. Destination slots will be checked between min_index and max_index; which direction they are checked in depends on the value of `desc`. If an empty slot is found and the item is successfully copied, `dest_index` is set to the index of the found slot and the function returns true. If no empty slot is found, dest_index is set to -1 and the method returns false.
        public static bool MoveToFirstEmpty(Item item, Item[] dest, int min_index, int max_index, bool desc, out int dest_index)
        {
            dest_index = -1;
            if (desc)
                for (int i = max_index; i >= min_index; i--)
                {
                    if (CopyIfEmpty(item, ref dest[i]))
                    {
                        dest_index = i;
                        break;
                    }
                }
            else
                for (int i = min_index; i <= max_index; i++)
                {
                    if (CopyIfEmpty(item, ref dest[i]))
                    {
                        dest_index = i;
                        break;
                    }
                }
            return dest_index >= 0;
        }

        public static bool TryMergeStackToContainer(Item item, Item[] dest, int min_index, int max_index, bool desc, out int dest_index)
        {
            dest_index = -1;
            if (desc)
                for (int i = max_index; i >= min_index; i--)
                {
                    if (TryMergeStacks(item, dest, i))
                    {
                        dest_index = i;
                        break;
                    }
                }
            else
                for (int i = min_index; i <= max_index; i++)
                {
                    if (TryMergeStacks(item, dest, i))
                    {
                        dest_index = i;
                        break;
                    }
                }
            return dest_index >= 0;
        }

        /// returns true if the entire stack of src_item is reduced to 0; otherwise returns false
        public static bool TryMergeStacks(Item src_item, Item[] dest_container, int dest_index)
        {
            return src_item.CanStackWith(dest_container[dest_index]) && (StackMerge(src_item, dest_container, dest_index) || CopyIfEmpty(src_item, ref dest_container[dest_index]));
        }


        /******************************************************
        // Moves as much of itemSrc.stack to itemDest.stack as possible.
        // Returns true if itemSrc.stack is reduced to 0; false otherwise.
        // Does not check for item equality or existence of passed items;
        // that must be ensured by the calling method.
        */
        // public static bool StackMerge(ref Item itemSrc, ref Item itemDest)
        public static bool StackMerge(Item itemSrc, Item itemDest)
        {
            int diff = Math.Min(itemDest.maxStack - itemDest.stack, itemSrc.stack);
            itemDest.stack += diff;
            itemSrc.stack  -= diff;
            // return true if stack has been emptied
            return itemSrc.IsBlank();
        }

        // takes the entire destination container as a parameter so as to check coin stacks
        // public static bool StackMerge(ref Item itemSrc, Item[] dest, int dIndex )
        public static bool StackMerge(Item itemSrc, Item[] dest, int dIndex )
        {
            int diff = Math.Min(dest[dIndex].maxStack - dest[dIndex].stack, itemSrc.stack);
            dest[dIndex].stack += diff;
            itemSrc.stack  -= diff;
            DoContainerCoins(dest, dIndex);
            // return true if stack has been emptied
            return itemSrc.IsBlank();
        }

        // as the first StackMerge above, but returns the amount transferred
        // public static int StackMergeD(ref Item itemSrc, ref Item itemDest)
        public static int StackMergeD(Item itemSrc, ref Item itemDest)
        {
            int diff = Math.Min(itemDest.maxStack - itemDest.stack, itemSrc.stack);
            itemDest.stack += diff;
            itemSrc.stack  -= diff;

            return diff;
        }

        /**********************************************************
        *   DoContainerCoins
        *
        *   Adapted from the Player.DoCoins(int i) method because Main.MoveCoins() wasn't doing what I wanted.
        */
        public static void DoContainerCoins(Item[] container, int i)
        {
            // stack contains 100 items && item matches copper/silver/gold item-type id
            if (container[i].stack == 100 && (container[i].type == 71 || container[i].type == 72 || container[i].type == 73))
            {
                //replace stack with 1 coin of next-higher type
                container[i].SetDefaults(container[i].type + 1);

                // search the rest of the container for more coins of this type and move it there if found.
                for (int j = 0; j < container.Length; j++)
                {
                    if (container[j].IsTheSameAs(container[i]) && j != i && container[j].type == container[i].type && container[j].stack < container[j].maxStack)
                    {
                        container[j].stack++;
                        container[i] = new Item();

                        //recursively call this method again as move may have maxed new stack
                        DoContainerCoins(container, j);
                    }
                }
            }
        }

        /// calls the NetMessage.sendData method for the current chest
        /// at the given index. Called on all of loot, deposit, stack
        public static void SendNetMessage(int index)
        {
            if (Main.netMode == 1)
            {
                NetMessage.SendData(32, -1, -1, "", Main.player[Main.myPlayer].chest, (float)index, 0, 0, 0);
            }
        }


    #endregion

    #region gui_stuff

        /* ************************************************
        *	 Unrelated to any of the above, these methods assist in the GUI/buttons part of the mod.
        *	 TODO: make these extension methods.
        */

        /// get source texels based on an index (defined in Constants class).
        /// the "+2" is due to the 2-pixel gap between the button faces in the grid png
        // public static Rectangle? RectFromGridIndex(int gIndex, bool active=false)
        // {
        //     return active ?
        //         new Rectangle(Constants.ButtonW + 2, (Constants.ButtonH + 2) * gIndex, Constants.ButtonW, Constants.ButtonH) : //mouse-over
        //         new Rectangle(0, (Constants.ButtonH + 2) * gIndex, Constants.ButtonW, Constants.ButtonH); //inactive
        // }
        //
        // ///Get source Texels for the button based what @action it performs
        // /// @param action
        // /// @param active - false = default/inactive button appearance;
        // ///                  true = focused/mouseover/active appearance
        // public static Rectangle? GetSourceRect(TIH action, bool active=false)
        // {
        //     return RectFromGridIndex( Constants.ButtonGridIndexByActionType[action], active );
        // }
        //
        // /// returns the key-bind (as a string) for the button with the given action.
        // /// return value will be something like "(X)"
        // public static string GetKeyTip(TIH action)
        // {
        //     string kbopt;
        //     if (Constants.ButtonActionToKeyBindOption.TryGetValue(action, out kbopt))
        //         return IHBase.ButtonKeyTips[kbopt];
        //
        //     return "";
        // }
        //
        // /// add Button to main mod-wide collection of all extant buttons
        // public static void AddToButtonStore(ICoreButton btn)
        // {
        //     IHBase.Instance.ButtonStore.Add(btn.ID, btn);
        // }

    #endregion

    }
}
