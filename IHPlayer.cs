using System;
using System.IO;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Input;
using InvisibleHand.Utils;

namespace InvisibleHand
{

    public class IHPlayer : ModPlayer
    {
        private static IHPlayer Instance;
        public static Player LocalPlayer {
            get {
                return Main.player[Main.myPlayer];
            }
        }

        public static IHPlayer GetLocalIHPlayer(Mod mod)
        {
            return Main.player[Main.myPlayer].GetModPlayer<IHPlayer>(mod);
        }

        public Item[] chestItems {
            get { return Main.chest[player.chest].item; }
        }

        /// map of user-locked item slots in player inventory
        // private bool[] lockedSlots;

        /// Holds status of flag indicating whether a particular
        /// action will respect (i.e. ignore) locked slots.
        /// Some actions are unaffected by this flag.
        private Dictionary<TIH,bool> LockedActions;

        public override void Initialize()
        {
            Instance = this;

            // MUST use "new", as tAPI derps with clearing (quoth: Miraimai)
            // lockedSlots = new bool[40]; //not the hotbar

            // init "locked" status of all available actions;
            // not all actions are affected by this flag
            LockedActions = new Dictionary<TIH,bool>();
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
                  IHBase.ModOptions[optionName] = new BoolOption(mod, state);
              }
            }
            catch (Exception e)
            {
              ErrorLogger.Log("Read Error: "+e.ToString());
            }

            try //key options
            {
              int count = reader.ReadInt32();
              for (int i = 0; i < count; i++)
              {
                  string actionName = reader.ReadString();
                  int keyval = reader.ReadInt32();
                  if (Enum.IsDefined(typeof(Keys), keyval))
                    IHBase.ActionKeys[actionName] = new KeyOption(mod, (Keys)keyval);
              }
            }
            catch (Exception e)
            {
              ErrorLogger.Log("Read Error: "+e.ToString());
            }

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
              ErrorLogger.Log("Read Error: "+e.ToString());
            }
        }

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
            if (!API.KeyboardInputFocused() && Main.playerInventory && Main.npcShop==0 && player.talkNPC==-1)
            {
                // Sort inventory/chest
                if (IHBase.ActionKeys["Sort"].Pressed())
                {
                    Sort(KState.Special.Shift.Down());
                }

                //Consolidate Stacks
                else if (IHBase.ActionKeys["Clean"].Pressed())
                {
                    CleanStacks();
                }
                else
                {
                    if ( player.chest == -1 ) return; //no action w/o open container

                    // smartloot or quickstack
                    if (IHBase.ActionKeys["QuickStack"].Pressed()) {
                        QuickStack(KState.Special.Shift.Down());
                    }
                    // smart-deposit or deposit-all
                    else if (IHBase.ActionKeys["DepositAll"].Pressed()) {
                        DepositAll(KState.Special.Shift.Down());
                    }
                    // loot all
                    else if (IHBase.ActionKeys["LootAll"].Pressed())
                        DoChestUpdateAction( IHUtils.DoLootAll );
                }
            }
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
            if ( player.chest == -1 ) // no valid chest open, sort player inventory
                // shift-pressed XOR Reverse-sort-mod-option:
                //   this will reverse the sort IFF exactly one of these two bools is true
                IHOrganizer.SortPlayerInv(player,
                    reverse ^ IHBase.ModOptions["ReverseSortPlayer"]);
            else
                // call sort on the Item[] array returned by chestItems
                DoChestUpdateAction( () =>
                    IHOrganizer.SortChest(chestItems,
                    reverse ^ IHBase.ModOptions["ReverseSortChest"])
                );
        }

        /// performs the most appropriate clean action
        public void CleanStacks()
        {

            if ( player.chest == -1 )
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
            if ( player.chest == -1 ) return;

            if (smartLoot)
                DoChestUpdateAction( IHSmartStash.SmartLoot );
            else
                DoChestUpdateAction( IHUtils.DoQuickStack );
        }

        /// <summary>
        /// Performs DepositAll or smartDeposit action if an appropriate container is open.
        /// </summary>
        /// <param name="smartDeposit">Perform smartDeposit action instead </param>
        public void DepositAll(bool smartDeposit = false)
        {
            if ( player.chest == -1 ) return;

            if (smartDeposit)
                DoChestUpdateAction( IHSmartStash.SmartDeposit );
            else
                DoChestUpdateAction( IHUtils.DoDepositAll );
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

        public bool SlotLocked(int slot)
        {
            return player.inventory[slot].favorited;
        }


        /// <returns>True if indicated action is set to respect locked slots.</returns>
        public static bool ActionLocked(Player player, TIH actionID)
        {
            IHPlayer mp = player.GetSubClass<IHPlayer>();
                return mp.LockedActions[actionID];
        }

        public static bool ActionLocked(TIH actionID)
        {
            return Instance.LockedActions[actionID];
        }

        /// Set indicated action to respect/not-respect locked slots,
        /// depending on current status.
        public static void ToggleActionLock(Player p, TIH actionID)
        {
            IHPlayer mp = p.GetSubClass<IHPlayer>();
            mp.LockedActions[actionID] = !mp.LockedActions[actionID];
        }

        public static void ToggleActionLock(TIH actionID)
        {
            Instance.LockedActions[actionID] = !Instance.LockedActions[actionID];
        }

    }
}
