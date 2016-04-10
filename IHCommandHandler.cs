using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using InvisibleHand.Utils;

namespace InvisibleHand
{
    /// this contains the InvisibleHand-specific command stuff
    public class IHCommandHandler
    {
        private CommandHelper helper;

        private IHBase modbase;

        public IHCommandHandler(IHBase modbase)
        {
            this.modbase = modbase;
            this.helper = new CommandHelper(modbase);
        }

        public void Initialize()
        {
            this.RegisterHelp();
        }

        public void RegisterHelp()
        {
            foreach (var kvp in Constants.CommandHelpStrings)
                helper.setHelp(kvp.Key, kvp.Value);
        }

        public Queue<String> SplitCommandLine(string line)
        {
            var command_line_parts =
                (from s in line.Substring(1).Split(null)
                where s != String.Empty
                 select s).ToArray();

            return new Queue<string>(command_line_parts);

        }

        /// doest the brute work of handling the command
        public void HandleCommand(string line)
        {
            // TODO: split this monster up; maybe parse command-line beforehand and not actively here

            // this is the only command for now...maybe more later?
            var qcmd = SplitCommandLine(line);

            string next = qcmd.Dequeue();

            if (next == "ihconfig")
            {
                if (qcmd.Count==0)
                {
                    helper.printHelp(next);
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
                            helper.printHelp("help");
                        else
                        {
                            switch (qcmd.Dequeue())
                            {
                                case "s":
                                case "set":
                                    helper.printHelp("set");
                                    break;

                                case "q":
                                case "query":
                                    helper.printHelp("query");
                                    break;
                            }
                        }
                        break;
                    case "s":
                    case "set":
                        if (qcmd.Count == 0)
                            helper.printHelp("set");
                        else
                        {
                            string opt = qcmd.Dequeue();
                            if (IHBase.ModOptions.ContainsKey(opt))
                            {
                                if (qcmd.Count == 0)
                                    {helper.ErrorMsg("no value provided for option '{opt}'", "ihconfig");}
                                else
                                {
                                    bool newval;
                                    int boolint;
                                    string value = qcmd.Dequeue();
                                    if (bool.TryParse(value, out newval))
                                        modbase.UpdateOption(opt, newval);
                                    else if (Int32.TryParse(value, out boolint) && (boolint == 0 || boolint == 1))
                                        modbase.UpdateOption(opt, boolint == 1);
                                    else
                                        helper.ErrorMsg($"could not parse value '{value}'", "ihconfig");
                                }
                            }
                            else
                                helper.ErrorMsg($"invalid option name '{opt}'", "ihconfig");
                        }
                        break;

                    case "q":
                    case "query":
                        if (qcmd.Count == 0 || qcmd.Peek()=="all")
                        {
                            // print all available options and current values
                            StringBuilder printopts = new StringBuilder("Options:");
                            foreach (var kvp in IHBase.ModOptions)
                            {
                                printopts.AppendFormat(" {0} [{1}],", kvp.Key, kvp.Value);
                            }
                            // print all but last comma
                            Main.NewText(printopts.ToString(0, printopts.Length-1));
                        }
                        else
                        {
                            while (qcmd.Count > 0)
                            {
                                string opt = qcmd.Dequeue();
                                bool current;
                                if (IHBase.ModOptions.TryGetValue(opt, out current))
                                    Main.NewText($"Option {opt}: {current}");
                                else
                                    helper.ErrorMsg("invalid option name '{opt}'", "ihconfig");
                            }
                        }
                        break;
                }
            }
        }
    }
}
