using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;

namespace InvisibleHand
{
    public class IHCommandHelper
    {
        public readonly Mod modbase;

        private IDictionary<string, IList<string>> helpStrings = new Dictionary<string, IList<string>>();

        public IHCommandHelper(Mod modbase)
        {
            this.modbase = modbase;
        }

        public void setHelp(string key, params string[] help_lines)
        {
            helpStrings[key] = new List<string>(help_lines);
        }

        public void setHelp(string key, IList<string> help_lines)
        {
            helpStrings[key] = new List<string>(help_lines);
        }

        public void printHelp(string key)
        {
            foreach (string line in helpStrings[key])
                Main.NewText(line);
        }

        public void ErrorMsg(string msg, string source = "")
        {
            if (source==String.Empty) source = modbase.Name;
            Main.NewText($"[{source}] Error: {msg}");
        }
    }
}
