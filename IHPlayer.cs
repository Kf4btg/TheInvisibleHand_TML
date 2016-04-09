using System;
using System.IO;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using InvisibleHand.Utils;

namespace InvisibleHand
{

    public class IHPlayer : ModPlayer
    {
        private static IHPlayer Instance;
        public static Player LocalPlayer
        {
            get
            {
                return Main.player[Main.myPlayer];
            }
        }

        // track Keyboard state (pressed keys)
        internal static KeyboardState prevState = Keyboard.GetState();
        // can just use Main.keyState for the current
        // internal static KeyboardState currState = Keyboard.GetState();

        public static IHPlayer GetLocalIHPlayer(Mod mod)
        {
            return Main.player[Main.myPlayer].GetModPlayer<IHPlayer>(mod);
        }

        public Item[] chestItems
        {
            get { return Main.chest[player.chest].item; }
        }

        /// map of user-locked item slots in player inventory
        // private bool[] lockedSlots;

        /// Holds status of flag indicating whether a particular
        /// action will respect (i.e. ignore) locked slots.
        /// Some actions are unaffected by this flag.
        private Dictionary<TIH, bool> LockedActions;

        public override void Initialize()
        {
            Instance = this;

            // MUST use "new", as tAPI derps with clearing (quoth: Miraimai)
            // lockedSlots = new bool[40]; //not the hotbar

            // init "locked" status of all available actions;
            // not all actions are affected by this flag
            LockedActions = new Dictionary<TIH, bool>();
            foreach (TIH aID in Enum.GetValues(typeof(TIH)))
            {
                LockedActions.Add(aID, false);
            }
        }

        public override void SaveCustomData(BinaryWriter writer)
        {
            if (Main.gameMenu)
            {
                // TODO: find somewhere else to do this
                var lii = Constants.LangInterIndices;
                // reset original chest-button strings if we're quitting to main
                // menu, which should be indicated by checking:
                //     if (Main.gameMenu == true)
                // as this is set during the SaveAndQuit() method of the worldgen
                // immediately before player save. So:
                Lang.inter[lii[TIH.LootAll]] = IHBase.OriginalButtonLabels[TIH.LootAll];
                Lang.inter[lii[TIH.DepositAll]] = IHBase.OriginalButtonLabels[TIH.DepositAll];
                Lang.inter[lii[TIH.QuickStack]] = IHBase.OriginalButtonLabels[TIH.QuickStack];

                if (IHBase.ModOptions["UseReplacers"])
                {
                    Lang.inter[lii[TIH.Rename]] = IHBase.OriginalButtonLabels[TIH.Rename];
                    Lang.inter[lii[TIH.SaveName]] = IHBase.OriginalButtonLabels[TIH.SaveName];
                    Lang.inter[lii[TIH.CancelEdit]] = IHBase.OriginalButtonLabels[TIH.CancelEdit];
                }
                // should take care of it and make sure the strings are set
                // correctly if the mod is unloaded/the replacer-button option
                // is disabled.
            }
            // if (!IHBase.oLockingEnabled) return; //maybe?

            // save locked-slot state with player
            // foreach (var l in lockedSlots)
            // {
            //     bb.Write(l);
            // }

            // TODO: refactor option-handling into separate lib, save the options there; query
            // that during load to retrieve options

            // Save modoptions
            writer.Write(IHBase.ModOptions.Count);
            foreach (var kvp in IHBase.ModOptions)
            {
                writer.Write(kvp.Key); // string optionname
                writer.Write((bool)kvp.Value);
            }

            // save user-defined hotkeys
            writer.Write(IHBase.ActionKeys.Count);
            foreach (var kvp in IHBase.ActionKeys)
            {
                writer.Write(kvp.Key); // string action_name
                writer.Write((int)kvp.Value); // int val of Keys enum value
            }

            // save locked action types
            writer.Write(LockedActions.Count);
            //KeyValuePair<TIH, bool>
            foreach (var kvp in LockedActions)
            {
                writer.Write((int)kvp.Key);
                writer.Write(kvp.Value);
            }
        }

        ///load back locked-slot state
        public override void LoadCustomData(BinaryReader reader)
        {
            // if (bb.PeekChar()) return;

            // for (int i=0; i<lockedSlots.Length; i++)
            // {
            //     lockedSlots[i]=bb.ReadBool();
            // }
            // if (bb.IsEmpty) return;

            try //mod options
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    string optionName = reader.ReadString();
                    bool state = reader.ReadBoolean();
                    mod.UpdateOption(optionName, state);
                }
            }
            catch (Exception e)
            {
                ErrorLogger.Log("Read Error: " + e.ToString());
            }

            try //key options
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    string actionName = reader.ReadString();
                    int keyval = reader.ReadInt32();
                    if (Enum.IsDefined(typeof(Keys), keyval))
                        mod.UpdateOption(actionName, (Keys)keyval);
                    else
                        ErrorLogger.Log($"Invalid value {keyval} for keybind {actionName} found in player save data");

                    // IHBase.ActionKeys[actionName] = new KeyOption(mod, (Keys)keyval);
                }
            }
            catch (Exception e)
            {
                ErrorLogger.Log("Read Error: " + e.ToString());
            }

            // load locked Actions
            try
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    int actionID = reader.ReadInt32();
                    bool state = reader.ReadBoolean();
                    if (Enum.IsDefined(typeof(TIH), actionID))
                        LockedActions[(TIH)actionID] = state;
                }
            }
            catch (Exception e)
            {
                ErrorLogger.Log("Read Error: " + e.ToString());
            }
        }

        public static bool KeyboardBusy() => Main.chatMode || Main.editSign || Main.editChest;
        public static bool ShiftHeld() => Keys.LeftShift.Down() || Keys.RightShift.Down();

        /// During this phase we check if the player has pressed any hotkeys;
        /// if so, the corresponding action is called, with the chest-related
        /// actions wrapped in special net-update code to prevent syncing
        /// issues during multiplayer.
        public override void PreUpdate()
        {

            //activate only if:
            // not typing
            // inventory is open
            // not shopping
            // not talking to an npc
            if (!KeyboardBusy() && Main.playerInventory && Main.npcShop == 0 && player.talkNPC == -1)
            {
                // Sort inventory/chest
                if (IHBase.ActionKeys["Sort"].Pressed(prevState))
                {
                    Sort(ShiftHeld());
                }

                //Consolidate Stacks
                else if (IHBase.ActionKeys["Clean"].Pressed(prevState))
                {
                    CleanStacks();
                }
                else
                {
                    if (player.chest == -1) return; //no action w/o open container

                    // smartloot or quickstack
                    if (IHBase.ActionKeys["QuickStack"].Pressed(prevState))
                    {
                        QuickStack(ShiftHeld());
                    }
                    // smart-deposit or deposit-all
                    else if (IHBase.ActionKeys["DepositAll"].Pressed(prevState))
                    {
                        DepositAll(ShiftHeld());
                    }
                    // loot all
                    else if (IHBase.ActionKeys["LootAll"].Pressed(prevState))
                        ChestUI.LootAll();
                    // DoChestUpdateAction(IHUtils.DoLootAll);
                }
            }
            prevState = Main.keyState;
        }

        /// <summary>
        /// Takes an Action and will perform it wrapped in some net update code if we are a client. Otherwise it just does whatever it is.
        /// </summary>
        /// <param name="action">An Action (a lambda with no output)</param>
        // public static void DoChestUpdateAction(Action action)
        public void DoChestUpdateAction(Action action)
        {

            var chestContents = chestItems; // cache this
            // var player = Main.localPlayer;
            // check net status and make sure a non-bank chest is open
            // (bank-chests, i.e. piggy-bank & safe, are handled solely client-side)
            if (Main.netMode == 1 && player.chest > -1)
            {
                Item[] oldItems = new Item[chestContents.Length];

                // make an exact copy of the chest's original contents
                for (int i = 0; i < oldItems.Length; i++)
                {
                    oldItems[i] = chestContents[i].Clone();
                }

                // perform the requested action
                action();

                // compare each item in the old copy of the original contents
                // to the chest's new contents and send net-update message
                // if they do not match.
                for (int i = 0; i < oldItems.Length; i++)
                {
                    // var oldItem = oldItems[i];
                    // var newItem = chestContents[i];

                    if (oldItems[i].IsNotTheSameAs(chestContents[i]) || oldItems[i].stack != chestContents[i].stack)
                    {
                        IHUtils.SendNetMessage(i);
                    }
                }
            }
            else // And this is important...
            {
                action();
            }
        }

        /// performs the most appropriate sort action
        public void Sort(bool reverse = false)
        {
            // NOTE: this used to check player.chestItems==null, but I once got a
            // "object reference not set to instance of object" or whatever kind of error
            // with that check elsewhere in the code. This should be safer and have the exact same result.
            if (player.chest == -1) // no valid chest open, sort player inventory
                // shift-pressed XOR Reverse-sort-mod-option:
                //   this will reverse the sort IFF exactly one of these two bools is true
                IHOrganizer.SortPlayerInv(player,
                    reverse ^ IHBase.ModOptions["ReverseSortPlayer"]);
            else
                // call sort on the Item[] array returned by chestItems
                DoChestUpdateAction(() =>
                   IHOrganizer.SortChest(chestItems,
                   reverse ^ IHBase.ModOptions["ReverseSortChest"])
                );
        }

        /// performs the most appropriate clean action
        public void CleanStacks()
        {

            if (player.chest == -1)
                IHOrganizer.ConsolidateStacks(player.inventory, 0, 50);
            else
                DoChestUpdateAction(
                    () => IHOrganizer.ConsolidateStacks(chestItems));
        }

        /// <summary>
        /// Performs QuickStack or SmartLoot action if an appropriate container is open.
        /// </summary>
        /// <param name="smartLoot">Perform SmartLoot action instead </param>
        public void QuickStack(bool smartLoot = false)
        {
            if (player.chest == -1) return;

            if (smartLoot)
                DoChestUpdateAction(IHSmartStash.SmartLoot);
            else
                ChestUI.QuickStack();
                // DoChestUpdateAction(IHUtils.DoQuickStack);
        }

        /// <summary>
        /// Performs DepositAll or smartDeposit action if an appropriate container is open.
        /// </summary>
        /// <param name="smartDeposit">Perform smartDeposit action instead </param>
        public void DepositAll(bool smartDeposit = false)
        {
            if (player.chest == -1) return;

            if (smartDeposit)
                DoChestUpdateAction(IHSmartStash.SmartDeposit);
            else
                ChestUI.DepositAll();
            // DoChestUpdateAction(IHUtils.DoDepositAll);
        }

        public void LootAll()
        {
            if (player.chest == -1) return;

            ChestUI.LootAll();
        }

        /// Only valid for the 40 Player inventory slots below the hotbar.
        /// <returns>True if indicated slot is locked</returns>
        // public static bool SlotLocked(Player player, int slotIndex)
        // {
        //     // pull IHPlayer subclass from the current Player-object's
        //     // list of subclasses
        //     IHPlayer mp = player.GetSubClass<IHPlayer>();
        //     // subtract 10 since our array only contains 40 items and
        //     // we're ignoring the first 10 actual slots (the hotbar).
        //     return slotIndex > 9 && slotIndex < 50 && mp.lockedSlots[slotIndex - 10];
        // }

        /// Only valid for the 40 Player inventory slots below the hotbar.
        /// <returns>True if indicated slot is locked</returns>
        // public static bool SlotLocked(int slotIndex)
        // {
        //     return slotIndex > 9 && slotIndex < 50 && Instance.lockedSlots[slotIndex - 10];
        // }
        //
        // /// Locks/unlocks indicated slot depending on current status.
        // public static void ToggleLock(Player player, int slotIndex)
        // {
        //     if (slotIndex<10 || slotIndex>49) return;
        //     IHPlayer mp = player.GetSubClass<IHPlayer>();
        //
        //     mp.lockedSlots[slotIndex - 10] = !mp.lockedSlots[slotIndex - 10];
        // }
        //
        // /// Locks/unlocks indicated slot depending on current status.
        // public static void ToggleLock(int slotIndex)
        // {
        //     if (slotIndex<10 || slotIndex>49) return;
        //
        //     Instance.lockedSlots[slotIndex - 10] = !Instance.lockedSlots[slotIndex - 10];
        // }

        // public bool SlotLocked(int slot)
        // {
        //     return player.inventory[slot].favorited;
        // }
        //
        //
        // /// <returns>True if indicated action is set to respect locked slots.</returns>
        // public static bool ActionLocked(Player player, TIH actionID)
        // {
        //     // IHPlayer mp = player.GetModPlayer<IHPlayer>(Instance.mod);
        //     return player.GetModPlayer<IHPlayer>(Instance.mod).LockedActions[actionID];
        // }
        //
        // public bool ActionLocked(TIH actionID)
        // {
        //     return this.LockedActions[actionID];
        // }
        //
        // /// Set indicated action to respect/not-respect locked slots,
        // /// depending on current status.
        // public static void ToggleActionLock(Player player, TIH actionID)
        // {
        //     IHPlayer mp = player.GetModPlayer<IHPlayer>(Instance.mod);
        //     mp.LockedActions[actionID] = !mp.LockedActions[actionID];
        // }
        //
        // public static void ToggleActionLock(TIH actionID)
        // {
        //     Instance.LockedActions[actionID] = !Instance.LockedActions[actionID];
        // }
        //
        // #region inventory actions
        //
        // //Moving these here from IHUtils; they're tied to the player, anyway, so they really should be here
        //
        // private const int R_START = 49;   //start from end of main inventory
        // private const int R_END   = 10;     //don't include hotbar
        //
        // public void DoDepositAll()
        // {
        //     // if no chest open, just return;
        //     // this shouldn't happen if method if everything is working correctly
        //     if (player.chest == -1)
        //         return;
        //
        //     Func<Item, bool> can_move;
        //
        //     if (ActionLocked(TIH.DepositAll))
        //         // skip favorited items if the action is locked
        //         can_move = item => !(item.IsBlank() || item.favorited);
        //     else
        //         can_move = item => !item.IsBlank();
        //
        //     for (int i = R_START; i >= R_END; i--)
        //     {
        //         if (can_move(player.inventory[i]))
        //             IHUtils.MoveItem(ref player.inventory[i], chestItems, 0, Chest.maxItems);
        //     }
        //
        //     // XXX: Is this necessary anymore?
        //     Recipe.FindRecipes();
        //
        //     // skip favorited items if the action is locked
        //     // if (ActionLocked(TIH.DepositAll))
        //     // {
        //     //     // iterate from end to beginning
        //     //     for (int i = R_START; i >= R_END; i--)
        //     //     {
        //     //         // var item = player.inventory[i];
        //     //         if (player.inventory[i].IsBlank() || player.inventory[i].favorited)
        //     //             continue;
        //     //
        //     //         // IHUtils.MoveItemToChest(int iPlayer, bool sendMessage)
        //     //         IHUtils.MoveItem(ref player.inventory[i], chestItems, 0, Chest.maxItems);
        //     //     }
        //     // }
        //     // else
        //     // {
        //     // }
        // }
        //
        // /// for simplicity, this just uses the vanilla player.GetItem()
        // /// to do its work.
        // public void DoLootAll()
        // {
        //     //this shouldn't happen if method is called correctly
        //     if (player.chest == -1) return;
        //
        //     // var sendNetMsg = player.chest > -1;
        //     var container = chestItems;
        //
        //     for (int i=0; i<Chest.maxItems; i++)
        //     {
        //         if (!container[i].IsBlank())
        //         {
        //             container[i] = player.GetItem(player.whoAmI, container[i]);
        //
        //             // if (sendNetMsg) SendNetMessage(i);
        //         }
        //     }
        //     Recipe.FindRecipes(); // !ref:Main:#22640.36#
        // }
        //
        // public void DoQuickStack(Player aplayer)
        // {
        //     if (player.chest == -1) return;
        //
        //     var inventory = player.inventory;
        //     var container = chestItems;
        //     // bool sendMessage = player.chest > -1;
        //     var checkLocks  = ActionLocked(TIH.QuickStack);  //boolean
        //
        //
        //     for (int iC = 0; iC < Chest.maxItems; iC++)                                         // go through entire chest inventory.
        //     {                                                                                   //if chest item is not blank && not a full stack, then
        //         if (!container[iC].IsBlank() && container[iC].stack < container[iC].maxStack)
        //         {                                                                               //for each item in inventory (including coins, ammo, hotbar),
        //             for (int iP=0; iP<58; iP++)
        //             {
        //                 if (inventory[iP].favorited) continue;                    // if we're checking locks ignore the locked ones
        //
        //                 if (container[iC].IsTheSameAs(inventory[iP]))                           //if chest item matches inv. item...
        //                 {
        //                     Sound.ItemMoved.Play();
        //                                                                                         // ...merge inv. item stack to chest item stack
        //                     if (StackMerge(ref inventory[iP], container, iC))
        //                     {                                                                   // do merge & check return (inv stack empty) status
        //                         inventory[iP] = new Item();                                     // reset slot if all inv stack moved
        //                     }
        //                     else if (container[iC].IsBlank())
        //                     {                                                                   // else, inv stack not empty after merge, but (because of DoCoins() call),
        //                                                                                         // chest stack could be.
        //                         container[iC] = inventory[iP].Clone();                          // move inv item to chest slot
        //                         inventory[iP] = new Item();                                     // and reset inv slot
        //                     }
        //                     // if (sendMessage) SendNetMessage(iC);                             //send net message if regular chest
        //                 }
        //             }
        //         }
        //     }
        //     Recipe.FindRecipes(); // !ref:Main:#22640.36#
        // }
        //
        //
        // #endregion


    }
}
