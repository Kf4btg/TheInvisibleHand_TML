using System;
using System.Linq;
using System.IO;
// using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework.Input;
// using InvisibleHand.Utils;

namespace InvisibleHand
{
    public class IHPlayer : ModPlayer
    {
        // track Keyboard state (pressed keys)
        // internal static KeyboardState prevState = Keyboard.GetState();
        // can just use Main.keyState for the current
        // internal static KeyboardState currState = Keyboard.GetState();

        public static Player LocalPlayer => Main.player[Main.myPlayer];
        public static IHPlayer GetLocalIHPlayer(Mod mod) => Main.player[Main.myPlayer].GetModPlayer<IHPlayer>(mod);
        public Item[] chestItems => Main.chest[player.chest].item;

        public override void Initialize()
        {
            ((IHBase)mod).localplayer = this;
        }

        public override void SaveCustomData(BinaryWriter writer)
        {
            // if (Main.gameMenu)
            // {
            //     // TODO: find somewhere else to do this
            //     var lii = Constants.LangInterIndices;
            //     // reset original chest-button strings if we're quitting to main
            //     // menu, which should be indicated by checking:
            //     //     if (Main.gameMenu == true)
            //     // as this is set during the SaveAndQuit() method of the worldgen
            //     // immediately before player save. So:
            //     Lang.inter[lii[ActionID.LootAll]] = IHBase.OriginalButtonLabels[ActionID.LootAll];
            //     Lang.inter[lii[ActionID.DepositAll]] = IHBase.OriginalButtonLabels[ActionID.DepositAll];
            //     Lang.inter[lii[ActionID.QuickStack]] = IHBase.OriginalButtonLabels[ActionID.QuickStack];
            //
            //     if (IHBase.ModOptions["UseReplacers"])
            //     {
            //         Lang.inter[lii[ActionID.RenameChestCancel]] = IHBase.OriginalButtonLabels[ActionID.RenameChestCancel];
            //         Lang.inter[lii[ActionID.SaveChestName]] = IHBase.OriginalButtonLabels[ActionID.SaveChestName];
            //         Lang.inter[lii[ActionID.RenameChestCancel]] = IHBase.OriginalButtonLabels[ActionID.RenameChestCancel];
            //     }
            //     // should take care of it and make sure the strings are set
            //     // correctly if the mod is unloaded/the replacer-button option
            //     // is disabled.
            // }

            // TODO: refactor option-handling into separate lib, save the options there; query
            // that during load to retrieve options

            // Save modoptions
            var currentOptions = IHBase.ModOptions.Options;
            writer.Write(currentOptions.Count);
            foreach (var kvp in currentOptions)
            {
                writer.Write(kvp.Key); // string optionname
                writer.Write(kvp.Value.Value); // bool option.Value
            }
        }

        ///load back locked-slot state
        public override void LoadCustomData(BinaryReader reader)
        {
            // try //mod options
            // {
            int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
            {
                string optionName = reader.ReadString();
                bool state = reader.ReadBoolean();
                    // Console.WriteLine("mod.updateoption({0}, {1})", optionName, state);
                IHBase.ModOptions.UpdateOption(optionName, state);
                }
            // }
            // catch (Exception e)
            // {
            //     ErrorLogger.Log("Read Error: " + e.ToString());
            // }
        }

        // public static bool KeyboardBusy() => Main.chatMode || Main.editSign || Main.editChest;
        // public static bool ShiftHeld() => Keys.LeftShift.Down() || Keys.RightShift.Down();

        /// During this phase we check if the player has pressed any hotkeys;
        /// if so, the corresponding action is called
        // public override void PreUpdate()
        // {
        //
        //     //activate only if:
        //     // not typing
        //     // inventory is open
        //     // not shopping
        //     // not talking to an npc
        //     if (!KeyboardBusy() && Main.playerInventory && Main.npcShop == 0 && player.talkNPC == -1)
        //     {
        //         // Sort inventory/chest
        //         if (IHBase.ActionKeys["Sort"].Pressed(prevState))
        //             Sort(ShiftHeld());
        //
        //         //Consolidate Stacks
        //         else if (IHBase.ActionKeys["Clean"].Pressed(prevState))
        //             CleanStacks();
        //
        //         else if (player.chest != -1) //no action w/o open container
        //         {
        //             // restock or quickstack
        //             if (IHBase.ActionKeys["QuickStack"].Pressed(prevState))
        //             {
        //                 QuickStack(ShiftHeld());
        //             }
        //             // smart-deposit or deposit-all
        //             else if (IHBase.ActionKeys["DepositAll"].Pressed(prevState))
        //             {
        //                 DepositAll(ShiftHeld());
        //             }
        //             // loot all
        //             else if (IHBase.ActionKeys["LootAll"].Pressed(prevState))
        //                 LootAll();
        //             // DoChestUpdateAction(IHUtils.DoLootAll);
        //         }
        //     }
        //     prevState = Main.keyState;
        // }


        // track Keyboard state (pressed keys)
        // internal static KeyboardState prevState = Keyboard.GetState();


        // NOTE: bool Player.ItemSpace(item) => is there room in the player's inventory for them to pick up this item
        // NOTE: Player.QuickStackAllChests()

        public override void PostUpdate()
        {
            // reset the "holding-key" state, if needed
            // if the key has been released
            if (IHBase.holding_hotkey && IHBase.HeldHotKey.Up())
            {
                IHBase.holding_hotkey = false;
                IHBase.HeldHotKey = Keys.None;
            }
        }


        /// <summary>
        /// Takes an Action and will perform it wrapped in some net update code if we are a client. Otherwise it just does whatever it is.
        /// </summary>
        /// <param name="action">An Action (a lambda with no output)</param>
        // public static void DoChestUpdateAction(Action action)
        public void DoChestUpdateAction(Action action)
        {
            /// TODO: see if we can get rid of this. Only sort and clean use it anymore.

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
                    if (oldItems[i].IsNotTheSameAs(chestContents[i]) || oldItems[i].stack != chestContents[i].stack)
                    {
                        NetMessage.SendData(32, -1, -1, "", player.chest, (float)i, 0, 0, 0);
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
            Console.WriteLine("Sort");
            Console.WriteLine(" player.chest = {0}", player.chest);
            // NOTE: this used to check player.chestItems==null, but I once got a
            // "object reference not set to instance of object" or whatever kind of error
            // with that check elsewhere in the code. This should be safer and have the exact same result.
            if (player.chest == -1) // no valid chest open, sort player inventory
            {
                Console.WriteLine("sort player inv");
                if (player.noItems) return;
                Console.WriteLine("Player.noItems=false");


                // shift-pressed XOR Reverse-sort-mod-option:
                //   this will reverse the sort IFF exactly one of these two bools is true
                IHOrganizer.SortPlayerInv(player,
                reverse ^ IHBase.ModOptions["ReverseSortPlayer"]);

            }
            else
            {
                Console.WriteLine("sort chest");
                // call sort on the Item[] array returned by chestItems
                DoChestUpdateAction(() =>
                IHOrganizer.SortChest(chestItems,
                reverse ^ IHBase.ModOptions["ReverseSortChest"])
                );

            }
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
        /// Performs QuickStack or Restock action if an appropriate container is open.
        /// </summary>
        /// <param name="smartLoot">Perform Restock action instead </param>
        public void QuickStack(bool restock = false)
        {
            // if (player.chest == -1) return;

            if (restock)
                ChestUI.Restock();
            else
                ChestUI.QuickStack();
        }

        /// <summary>
        /// Performs DepositAll or smartDeposit action if an appropriate container is open.
        /// </summary>
        /// <param name="smartDeposit">Perform smartDeposit action instead </param>
        public void DepositAll(bool smartDeposit = false)
        {
            // if (player.chest == -1) return;
            if (player.noItems) return;

            if (smartDeposit)
                this.SmartDeposit();
            else
                ChestUI.DepositAll();
        }

        public void LootAll()
        {
            // if (player.chest != -1)
            ChestUI.LootAll();
        }

        /****************************************************
        *   This will compare the categories of items in the player's
            inventory to those of items in the open container and
            deposit any items of matching categories.
        */
        public void SmartDeposit()
        {
            if (player.chest == -1) return;

            // define a query that creates category groups for the items in the chests,
            // then pulls out the category keys into a distinct list (List<ItemCat>)
            var catList =
                    (from item in this.chestItems
                        where !item.IsBlank()
                        group item by item.GetCategory() into catGroup
                        from cat in catGroup
                        select catGroup.Key).Distinct() //no duplicates
                        .ToList(); //store the query results

            Item current;
            for (int i = 49; i >= 10; i--)  // reverse through player inv
            {
                current = player.inventory[i];
                if (!current.IsBlank() && !current.favorited
                    && catList.Contains(current.GetCategory()))
                {
                    ChestUI.TryPlacingInChest(current, false);
                }
            }
            Recipe.FindRecipes();
        }
    }
}
