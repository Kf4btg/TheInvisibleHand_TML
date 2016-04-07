using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Input;
using InvisibleHand.Utils;

namespace InvisibleHand
{
    public class IHBase : Mod
    {

        internal static int[] itemCategories;

        /// holds the game's original strings for loot-all, dep-all, quick-stack, etc;
        /// we're going to be removing these later on, but will use their
        /// original values to replace them with newer, better buttons.
        public static Dictionary<TIH, string> OriginalButtonLabels { get; private set; }

        public static readonly Dictionary<string, bool> ModOptions = new Dictionary<string, bool>();
        public static readonly Dictionary<string, Keys> ActionKeys = new Dictionary<string, Keys>();

        private IHCommandHelper commandHelper;

        public override string Name
        {
            get { return "TheInvisibleHand"; }
        }

        public IHBase()
        {
            Properties = new ModProperties()
            {
                Autoload = true
                // AutoloadGores = true,
                // AutoloadSounds = true,
            };

            commandHelper = new IHCommandHelper(this);



            // default options
            // ModOptions["UseReplacers"] = new BoolOption(this, true);
            //
            // // default hotkeys
            // ActionKeys["Sort"] = new KeyOption(this, Keys.R);
            // ActionKeys["Clean"] = new KeyOption(this, Keys.T);
            //
            // ActionKeys["DepositAll"] = new KeyOption(this, Keys.Z);
            // ActionKeys["LootAll"] = new KeyOption(this, Keys.X);
            // ActionKeys["QuickStack"] = new KeyOption(this, Keys.C);
            // ActionKeys["SmartDeposit"] = new KeyOption(this, Keys.V);
            // ActionKeys["SmartLoot"] = new KeyOption(this, Keys.B);
        }

        public override void Load()
        {
            OriginalButtonLabels = new Dictionary<TIH, string>(Constants.LangInterIndices.Count);
            // pull values out of Lang.inter to populate OBL
            foreach (var kvp in Constants.LangInterIndices)
            {
                OriginalButtonLabels[kvp.Key] = Lang.inter[kvp.Value];
            }

            foreach (var kvp in Constants.DefaultOptionValues)
            {
                this.RegisterOption(kvp.Key, kvp.Value, onOptionChanged);
                ModOptions[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in Constants.DefaultKeys)
            {
                this.RegisterOption(kvp.Key, kvp.Value, onKeyBindChanged);
                ActionKeys[kvp.Key]=kvp.Value;
            }
        }

        // utilize chat commands to set mod options
        public override void ChatInput(string text)
        {
            if (text[0] != '/' || text.Length==1) return;

            var command_line_parts =
                (from s in text.Substring(1).Split(null)
                where s != String.Empty
                 select s).ToArray();

            Queue<string> qcmd = new Queue<string>(command_line_parts);

            string next = qcmd.Dequeue();
            // int args_len = command_line_parts.Length;


            // this is the only command for now...maybe more later?
            if (next == "ihconfig")
            {
                if (qcmd.Count==0)
                {
                    commandHelper.printHelp(next);
                    // Main.NewText("Usage: /ihconfig <subcommand> <arguments>");
                    // Main.NewText("Valid subcommands are \"h[elp]\", \"set-opt\", \"set-key\", \"query-opt\",  and \"query-key\".");
                    // Main.NewText("These have aliases of 'h', 'so', 'sk', 'qo', and 'qk', respectively");
                    // Main.NewText("Use \"/ihconfig help <subcommand>\" for more information.");

                    return;
                }

                // string subcommand = qcmd.Dequeue();
                // subcommand
                next = qcmd.Dequeue();
                switch (next)
                {
                    case "h":
                    case "help":
                        if (qcmd.Count==0)
                            commandHelper.printHelp("help");
                        else
                        {
                            switch (qcmd.Dequeue())
                            {
                                case "so":
                                case "set-opt":
                                    commandHelper.printHelp("set-opt");
                                    break;

                                case "sk":
                                case "set-key":
                                    commandHelper.printHelp("set-key");
                                    break;

                                case "qo":
                                case "query-opt":
                                    commandHelper.printHelp("query-opt");
                                    break;

                                case "qk":
                                case "query-key":
                                    commandHelper.printHelp("query-key");
                                    break;

                            }
                            // Main.NewText("Usage: /ihconfig set-opt <option name> true|false|0|1");
                            // Main.NewText("  Example: /ihconfig set opt ReverseSortPlayer true");
                        }
                        break;
                    case "so":
                    case "set-opt":
                        if (qcmd.Count == 0)
                            commandHelper.printHelp("set-opt");
                        else
                        {
                            string opt = qcmd.Dequeue();
                            if (ModOptions.ContainsKey(opt))
                            {
                                if (qcmd.Count == 0)
                                    {commandHelper.ErrorMsg("no value provided for option '{opt}'", "ihconfig");}
                                else
                                {
                                    bool newval;
                                    int boolint;
                                    string value = qcmd.Dequeue();
                                    if (bool.TryParse(value, out newval))
                                        this.UpdateOption(opt, newval);
                                    else if (Int32.TryParse(value, out boolint))
                                    {
                                        if (boolint == 0)
                                            this.UpdateOption(opt, false);
                                        else if (boolint == 1)
                                            this.UpdateOption(opt, true);
                                        else
                                        {
                                            commandHelper.ErrorMsg($"could not parse value '{value}'", "ihconfig");
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                                commandHelper.ErrorMsg($"invalid option name '{opt}'", "ihconfig");
                        }
                        break;
                    case "sk":
                    case "set-key":
                        break;
                    case "q":
                    case "query":
                        break;
                }

                string[] args;
                if (command_line_parts.Count() > 1)
                    args = command_line_parts.slice(1);
                else
                    args = new string[0];

            }
        }

        public void setupChatCommands()
        {
            var helpKeys = new string[]
            {
                "ihconfig", "help", "set-key", "set-opt", "query-opt", "query-key"
            };

            // setup help output

            commandHelper.setHelp("ihconfig",
                "Usage: /ihconfig <subcommand> <arguments>",
                "Valid subcommands are \"h[elp]\", \"set-opt\", \"set-key\", \"query-opt\",  and \"query-key\".",
                "These have aliases of 'h', 'so', 'sk', 'qo', and 'qk', respectively",
                "Use \"/ihconfig help <subcommand>\" for more information.");

            commandHelper.setHelp("help",
                "Usage: /ihconfig set-opt <option_name> {true|1,false|0,default}",
                "/ihconfig set-key <action_name> <key>",
                "/ihconfig query-opt [option_name]...",
                "/ihconfig query-key [action_name]...",
                "Use \"/ihconfig help <subcommand>\" for more information"
                );

            commandHelper.setHelp("set-opt",
                "Usage: /ihconfig set-opt <option> {false,true,0,1,default}",
                "  Examples: /ihconfig set-opt ReverseSortPlayer true",
                "            /ihconfig set-opt ReverseSortChest 0",
                "Use \"/ihconfig query-opt\" for a list of all options."
            );

            commandHelper.setHelp("set-key",
                "Usage: /ihconfig set-key <action> <key>",
                "  Examples: /ihconfig set-key DepositAll X",
                "            /ihconfig set-key Sort Tab",
                "Use \"/ihconfig query-key\" for a list of all actions and current key-binds."
            );

            commandHelper.setHelp("query-opt",
                "Usage: /ihconfig query-opt [option_name]...",
                "With no arguments, prints a list of all option names.",
                "With one or more option names, shows current state of those options.",
                "  Example: /ihconfig query-opt SortToEndPlayer ReverseSortPlayer"
            );

            commandHelper.setHelp("query-key",
                "Usage: /ihconfig query-key [action_name]...",
                "With no arguments, prints a list of all action names.",
                "With one or more action names, shows currently bound key for those actions.",
                "  Example: /ihconfig query-opt Sort LootAll"
            );


        }

        // option-change callbacks
        private static void onOptionChanged(string name, bool value)
        {
            ModOptions[name] = value;
        }

        private static void onKeyBindChanged(string name, Keys value)
        {
            ActionKeys[name] = value;
        }
    }
}
