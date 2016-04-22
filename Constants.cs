using System.Collections.Generic;
// using Terraria.ID;
using Terraria.UI;

namespace InvisibleHand
{
    public class ActionID
    {
        public static readonly int LootAll           = ChestUI.ButtonID.LootAll;
        public static readonly int DepositAll        = ChestUI.ButtonID.DepositAll;
        public static readonly int QuickStack        = ChestUI.ButtonID.QuickStack;
        public static readonly int Restock           = ChestUI.ButtonID.Restock;
        public static readonly int RenameChest       = ChestUI.ButtonID.RenameChest;
        public static readonly int RenameChestCancel = ChestUI.ButtonID.RenameChestCancel;
        // public static readonly int Count          = ChestUI.ButtonID.Count;

        public static readonly int Sort              = ChestUI.ButtonID.Count + 1;
        public static readonly int CleanStacks       = ChestUI.ButtonID.Count + 2;
        public static readonly int SmartDeposit      = ChestUI.ButtonID.Count + 3;

        public static readonly int ReverseSort       = ChestUI.ButtonID.Count + 4;
        public static readonly int SaveChestName     = ChestUI.ButtonID.Count + 5;
    }

    ///the ItemCat Enum defines the actual Sort Order of the categories
    // public enum ItemCat
    // {
    //     COIN,
    //     PICK,
    //     AXE,
    //     HAMMER,
    //     TOOL,
    //     MECH,
    //     MELEE,
    //     RANGED,
    //     BOMB,
    //     AMMO,
    //     MAGIC,
    //     SUMMON,
    //     PET,
    //     HEAD,
    //     BODY,
    //     LEGS,
    //     ACCESSORY,
    //     VANITY,
    //     POTION,
    //     CONSUME,
    //     BAIT,
    //     DYE,
    //     PAINT,
    //     ORE,
    //     BAR,
    //     GEM,
    //     SEED,
    //     LIGHT,
    //     CRAFT,
    //     FURNITURE,
    //     STATUE,
    //     WALLDECO,
    //     BANNER,
    //     CLUTTER,
    //     WOOD,
    //     BLOCK,
    //     BRICK,
    //     TILE,
    //     WALL,
    //     MISC_MAT,
    //     SPECIAL,    // Boss summoning items, heart containers, mana crystals
    //     OTHER
    // }

    public static class Constants
    {
        /// This is obviously not all of the possible AIs, but just
        /// a subset of them that I've found useful for identifying items.
        public static class ProjectileAI
        {
            /// A simple, ballistic curve. Nothing special.
            /// Think arrows, javelins, bullets, North-Pole ice shards.
            public const int Ballistic = 1;

            /// not quite sure what to call this one...
            /// It's for projectiles that work like the throwing knife:
            /// they fly fairly straight for a while, then begin
            /// to spin and quickly drop to the ground & disappear
            public const int SpinDrop = 2;

            /// includes the LightDiscs
            public const int Boomerang = 3;

            /// anything that fires in a straight line and passes through
            /// blocks, just like the vilethorn
            public const int Vilethorn = 4;

            /// It's usually easier to check Main.projHook[item.shoot]
            /// than dig out the item's AI style to check this
            public const int GrapplingHook = 7;

            /// e.g. Flower of Fire, Meowmere
            public const int HyperBounce = 8;

            /// e.g. magic missile, magic knife
            public const int FollowCursor = 9;

            /// e.g. aqua scepter
            public const int Stream = 12;

            /// Literally projectiles attached to chains:
            /// e.g. harpoon, chain-knife/guillotine, golem-fist
            public const int Chained = 13;

            ///e.g. Beenade, boulder
            public const int HeavyBounce = 14;

            /// Note: Flairon does NOT match this AI type (it is unique)
            public const int Flail = 15;

            /// grenades, bombs, dynamite, etc.
            public const int Explosive = 16;

            /// when they pop out at death.
            public const int Tombstone = 17;

            /// demon scythe, death sickle...
            public const int Sickle = 18;

            public const int Spear = 19;

            /// matches all drills, chainsaws, jackhammers
            public const int PowerTool = 20;

            /// Note: there's also a Projectile named 'Flamethrower'
            /// but it uses a different AI...It doesn't match up with the
            /// Item.shoot value of either the flamethrower or the Elf Melter, either.
            public const int FlameThrower = 23;

            /// TerraBlade, Enchanted Sword, Unholy Trident
            public const int SwordBeam = 27;

            /// also minions
            public const int PetSpawn = 26;

            /// Clentaminator
            public const int Spray = 31;

            /// Rockets also match Explosive...
            public const int Rocket = 34;

            public const int RopeCoil = 35;

            /// also not quite sure what to call this one;
            /// matches items that create little critters on use,
            /// e.g. Bat, Bee, Wasps, Tiny Eaters, etc.
            public const int CritterSpawn = 36;

            /// I don't know if this is used for the actual Rain,
            /// but it is used for the Blood cloud and Nimbus Staff
            public const int Rain = 45;

            public const int Bobber = 61;

            /// Note: the counterweight accessory also has a .shoot value that
            /// corresponds to this ai-style
            public const int Yoyo = 99;
        }

        public static class AmmoID
        {
            public const int Arrow  = 1;
            public const int Bullet = 14;
            public const int Gel    = 23;
            public const int Dart   = 51;
            public const int Coin   = 71;
            public const int Rocket = 771;
            public const int Solution = 780;
        }

        ///the ItemCat Enum defines the actual Sort Order of the categories,
        /// but this defines in which order an item will be checked against
        /// the category matching rules. This is important due to a kind
        /// of "sieve" or "cascade" effect, where items that would have matched
        /// a certain category were instead caught by an earlier one.
        // public static readonly ItemCat[] CheckOrder =
        // {
        //     ItemCat.COIN,
        //     ItemCat.MECH,
        //     ItemCat.TOOL,
        //     ItemCat.PICK,
        //     ItemCat.AXE,
        //     ItemCat.HAMMER,
        //     ItemCat.HEAD,
        //     ItemCat.BODY,
        //     ItemCat.LEGS,
        //     ItemCat.ACCESSORY,
        //     ItemCat.SPECIAL,
        //     ItemCat.PET,
        //     ItemCat.VANITY,
        //     ItemCat.MELEE,
        //     ItemCat.BOMB,
        //     ItemCat.RANGED,
        //     ItemCat.AMMO,
        //     ItemCat.MAGIC,
        //     ItemCat.SUMMON,
        //     ItemCat.POTION,
        //     ItemCat.CONSUME,
        //     ItemCat.BAIT,
        //     ItemCat.DYE,
        //     ItemCat.PAINT,
        //     ItemCat.ORE,
        //     ItemCat.BAR,
        //     ItemCat.GEM,
        //     ItemCat.SEED,
        //     ItemCat.CRAFT,
        //     ItemCat.LIGHT,
        //     ItemCat.FURNITURE,
        //     ItemCat.STATUE,
        //     ItemCat.WALLDECO,
        //     ItemCat.BANNER,
        //     ItemCat.CLUTTER,
        //     ItemCat.WOOD,
        //     ItemCat.BRICK,
        //     ItemCat.BLOCK,
        //     ItemCat.TILE,
        //     ItemCat.WALL,
        //     ItemCat.MISC_MAT,
        //     ItemCat.OTHER
        // };

        // A large number of "Tile"-type items will share a .createTile
        // attribute with items that fulfill a similar purpose. This gives us a
        // handy way to sort and possibly even categorize these item types.
        /** ***********************************************************************
        Create several hashsets to quickly check values of "item.createTile" to
        aid in categorization/sorting. Initialize them here with an anonymous
        array to avoid the resizing penalty of .Add()
        */
        // public static readonly HashSet<int>
        //     TileGroupFurniture = new HashSet<int>(new int[] {
        //             TileID.ClosedDoor,
        //             TileID.Tables,
        //             TileID.Chairs,
        //             TileID.Platforms,
        //             TileID.Beds,
        //             TileID.Pianos,
        //             TileID.Dressers,
        //             TileID.Benches,
        //             TileID.Bathtubs,
        //             TileID.Bookcases,
        //             TileID.GrandfatherClocks,
        //             TileID.Containers,
        //             TileID.PiggyBank,
        //             TileID.Signs,
        //             TileID.Safes,
        //             TileID.Thrones,
        //             // TileID.WoodenPlank,
        //             TileID.Mannequin,
        //             TileID.Womannequin,
        //
        //     });
        //
        //
        // public static readonly HashSet<int>
        //     TileGroupLighting = new HashSet<int>(new int[] {
        //             TileID.Torches,
        //             TileID.Candles,
        //             TileID.Chandeliers,
        //             TileID.HangingLanterns,
        //             TileID.Lamps,
        //             TileID.Candelabras,
        //             TileID.Jackolanterns,
        //             TileID.ChineseLanterns,
        //             TileID.SkullLanterns,
        //             TileID.Campfire,
        //             TileID.FireflyinaBottle,
        //             TileID.LightningBuginaBottle,
        //             TileID.WaterCandle
        //     });
        //
        // public static readonly HashSet<int>
        //     TileGroupStatue = new HashSet<int>(new int[] {
        //             TileID.Tombstones,
        //             TileID.Statues,
        //             TileID.WaterFountain,
        //             TileID.AlphabetStatues,
        //             TileID.BubbleMachine
        //     });
        //
        // public static readonly HashSet<int>
        //     TileGroupWallDeco = new HashSet<int>(new int[] {
        //             // TileID.Painting2x3,
        //             // TileID.Painting3x2,
        //             // TileID.Painting3x3,
        //             // TileID.Painting4x3,
        //             // TileID.Painting6x4
        //     });
        //
        // public static readonly HashSet<int>
        //     TileGroupClutter = new HashSet<int>(new int[] {
        //             TileID.Bottles,
        //             TileID.Bowls,
        //             TileID.BeachPiles,
        //             TileID.Books,
        //             TileID.Coral,
        //             TileID.ShipInABottle,
        //             TileID.BlueJellyfishBowl,
        //             TileID.GreenJellyfishBowl,
        //             TileID.PinkJellyfishBowl,
        //             TileID.SeaweedPlanter,
        //             TileID.ClayPot,
        //             TileID.BunnyCage,
        //             TileID.SquirrelCage,
        //             TileID.MallardDuckCage,
        //             TileID.DuckCage,
        //             TileID.BirdCage,
        //             TileID.BlueJay,
        //             TileID.CardinalCage,
        //             TileID.FishBowl,
        //             TileID.SnailCage,
        //             TileID.GlowingSnailCage,
        //             TileID.MonarchButterflyJar,
        //             TileID.PurpleEmperorButterflyJar,
        //             TileID.RedAdmiralButterflyJar,
        //             TileID.UlyssesButterflyJar,
        //             TileID.SulphurButterflyJar,
        //             TileID.TreeNymphButterflyJar,
        //             TileID.ZebraSwallowtailButterflyJar,
        //             TileID.JuliaButterflyJar,
        //             TileID.ScorpionCage,
        //             TileID.BlackScorpionCage,
        //             TileID.FrogCage,
        //             TileID.MouseCage,
        //             TileID.PenguinCage,
        //             TileID.WormCage,
        //             TileID.GrasshopperCage
        //     }); // blergh
        //
        // public static readonly HashSet<int>
        //     TileGroupCrafting = new HashSet<int>(new int[] {
        //             TileID.WorkBenches,
        //             TileID.Anvils,
        //             TileID.MythrilAnvil,
        //             TileID.AdamantiteForge,
        //             TileID.CookingPots,
        //             TileID.Furnaces,
        //             TileID.Hellforge,
        //             TileID.Loom,
        //             TileID.Kegs,
        //             TileID.Sawmill,
        //             TileID.TinkerersWorkbench,
        //             TileID.CrystalBall,
        //             TileID.Blendomatic,
        //             TileID.MeatGrinder,
        //             TileID.Extractinator,
        //             TileID.Solidifier,
        //             TileID.DyeVat,
        //             TileID.ImbuingStation,
        //             TileID.Autohammer,
        //             TileID.HeavyWorkBench,
        //             TileID.BoneWelder,
        //             TileID.FleshCloningVat,
        //             TileID.GlassKiln,
        //             TileID.LihzahrdFurnace,
        //             TileID.LivingLoom,
        //             TileID.SkyMill,
        //             TileID.IceMachine,
        //             TileID.SteampunkBoiler,
        //             TileID.HoneyDispenser
        //     }); // also blergh
        //
        // // public static readonly HashSet<int>
        //     // TileGroupOre = new HashSet<int>(TileID.Sets.Ore);
        // // (new int[] {
        // //         TileID.Meteorite,
        // //         TileID.Obsidian,
        // //         TileID.Hellstone
        // // }); // (get others by name)
        //
        // public static readonly HashSet<int>
        //     TileGroupCoin = new HashSet<int>(new int[] {
        //             TileID.CopperCoinPile,
        //             TileID.SilverCoinPile,
        //             TileID.GoldCoinPile,
        //             TileID.PlatinumCoinPile
        //     });
        //
        // public static readonly HashSet<int>
        //     TileGroupSeed = new HashSet<int>(new int[] {
        //             TileID.ImmatureHerbs,
        //             TileID.Saplings, // Acorn
        //             TileID.Pumpkins  // Pumpkin Seed
        //     });                      // get the rest by EndsWith("Seeds")


        // ///////////////////////////////// //
        //          Dictionaries             //
        // ///////////////////////////////// //


        /// holds the index in Terraria.Lang.inter[] corresponding
        /// to the paired actions; used to get original button lobels.
        // public static readonly Dictionary<int, int> LangInterIndices;

        // public static readonly Dictionary<int, string> DefaultButtonLabels;

        /// maps actions to the modoption defining their keybind
        // public static readonly Dictionary<int, string> ButtonActionToKeyBindOption;

        public static readonly Dictionary<string, bool> DefaultOptionValues;

        public static readonly Dictionary<string, string> DefaultKeys;

        /// help strings for the chat commands
        public static readonly Dictionary<string, IList<string>> CommandHelpStrings;


        static Constants()
        {
            // TileGroupFurniture.UnionWith(TileID.Sets.RoomNeeds.CountsAsDoor);
            // TileGroupFurniture.UnionWith(TileID.Sets.RoomNeeds.CountsAsChair);
            // TileGroupFurniture.UnionWith(TileID.Sets.RoomNeeds.CountsAsTable);

            DefaultOptionValues = new Dictionary<string, bool>
            {
                // {"UseReplacers", true},
                {"SortToEndPlayer", false},
                {"SortToEndChest", false},
                {"ReverseSortPlayer", false},
                {"ReverseSortChest", false},

            };

            DefaultKeys = new Dictionary<string, string>
            {
                {"Sort", "R"},
                {"Clean", "Y"},
                {"DepositAll", "V"},
                {"LootAll", "C"},
                {"QuickStack", "Q"},
            };

            CommandHelpStrings = new Dictionary<string, IList<string>>(){
                {"ihconfig", new string[3]
                    {
                        "Usage: /ihconfig <subcommand> <arguments>",
                        "Valid subcommands are \"h[elp]\", \"s[et]\", and \"q[uery]\".",
                        "Use \"/ihconfig help <subcommand>\" for more information."
                    }
                },
                {"help", new string[3]
                    {"Usage: /ihconfig set <option_name> {true|1,false|0,default}",
                     "       /ihconfig query [option_name]...",
                    "Use \"/ihconfig help <subcommand>\" for more information"}
                },
                {"set", new string[4]
                    {"Usage: /ihconfig set <option> {false,true,0,1,default}",
                    "  Examples: /ihconfig set ReverseSortPlayer true",
                    "            /ihconfig s ReverseSortChest 0",
                    "Use \"/ihconfig query\" for a list of all options."}
                },
                // {"set-key", new string[4]
                //     {"Usage: /ihconfig set-key <action> <key>",
                //     "  Examples: /ihconfig set-key DepositAll X",
                //     "            /ihconfig set-key Sort Tab",
                //     "Use \"/ihconfig query-key all\" for a list of all actions and current key-binds."}
                // },
                {"query", new string[4]
                    {"Usage: /ihconfig query [<option_name>]...",
                    "With no arguments, prints a list of all option names.",
                    "With one or more option names, shows current state of those options.",
                    "  Example: /ihconfig query SortToEndPlayer ReverseSortPlayer"}
                }
                // {"query-key", new string[4]
                //     {"Usage: /ihconfig query-key [all] [<action_name>]...",
                //     "With no arguments (or 'all'), prints a list of all action names.",
                //     "With one or more action names, shows currently bound key for those actions.",
                //     "  Example: /ihconfig query Sort LootAll"}
                // }
            };

            // LangInterIndices = new Dictionary<int, int>
            // {
            //     {ActionID.LootAll,    29},
            //     {ActionID.DepositAll, 30},
            //     {ActionID.QuickStack, 31},
            //     {ActionID.SaveChestName,   47},
            //     {ActionID.RenameChest,     61},
            //     {ActionID.RenameChestCancel, 63}
            // };


            // DefaultButtonLabels = new Dictionary<int, string>
            // {
            //     // Player Inventory
            //     {ActionID.Sort,         "Sort"},
            //     {ActionID.ReverseSort,  "Sort (Reverse)"},
            //
            //     {ActionID.CleanStacks,     "Clean Stacks"},
            //     // Chests
            //     {ActionID.Restock,    "Restock"},
            //     {ActionID.QuickStack,   "Quick Stack"},
            //     {ActionID.SmartDeposit, "Smart Deposit"},
            //     {ActionID.DepositAll,   "Deposit All"},
            //     {ActionID.LootAll,      "Loot All"},
            //     {ActionID.RenameChest,       "Rename"},
            //     {ActionID.SaveChestName,     "Save"},
            //     {ActionID.RenameChestCancel,   "Cancel"}
            // };

            /*************************************************
            * Map action types to the string used for the corresponding
            * keybind in Modoptions.json
            */
            // ButtonActionToKeyBindOption = new Dictionary<int, string>()
            // {
            //     {ActionID.CleanStacks,   "cleanStacks"},
            //
            //     {ActionID.DepositAll,     "depositAll"},
            //     {ActionID.SmartDeposit,   "depositAll"},
            //
            //     {ActionID.LootAll,    "lootAll"},
            //
            //     {ActionID.QuickStack, "quickStack"},
            //     {ActionID.Restock,  "quickStack"},
            //
            //     {ActionID.Sort,       "sort"},
            //     {ActionID.ReverseSort,"sort"},
            //     // edit chest doesn't get a keyboard shortcut. So there.
            // };


        }
    }
}
