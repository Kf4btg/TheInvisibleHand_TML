// using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
// using System.Dynamic;
using Terraria;
// using Terraria.ID;
using Terraria.ModLoader;

namespace InvisibleHand.Items
{
    internal class ItemClassificationWrapper
    {
        public Item item;
        public ItemFlagInfo info;

        public Dictionary<string, int> Item_Flags => info.Flags;

        public bool Success { get; private set; }
        public KeyValuePair<string, string> LastFlag { get; private set; }

        public ItemClassificationWrapper(Item item, ItemFlagInfo info)
        {
            this.item = item;
            this.info = info;

            this.info.Flags = new Dictionary<string, int>();
        }

        public ItemClassificationWrapper SetFlag(string type, string flag)
        {
            this.Item_Flags[type] |= IHBase.FlagCollection[type][flag];
            this.Success = true;
            this.LastFlag = new KeyValuePair<string, string>(type, flag);
            return this;
        }

        public ItemClassificationWrapper Flag(string type, string flag)
        {
            Success = false; // reset
            if (ConditionTable.Check(type, item, flag))
                SetFlag(type, flag);
            return this;
        }

        public ItemClassificationWrapper FlagIf(bool condition, string type, string flag)
        {
            Success = false; // reset
            if (condition) SetFlag(type, flag);
            return this;
        }

        public ItemClassificationWrapper FlagFirst(string type, params string[] flags)
        {
           foreach (var f in flags)
            {
                if (this.Flag(type, f).Success)
                    break;
            }
            return this;
        }

        public ItemClassificationWrapper FlagAny(string type, params string[] flags)
        {
            bool res = false;
            foreach (var f in flags)
                res |= this.Flag(type, f).Success;

            // we want to know if any of the operations succeeded, not just
            // the most recent one, so we catch any True value in res
            Success = res;
            return this;
        }

        public bool TryFlag(string type, string flag)
        {
            return this.Flag(type, flag).Success;
        }
    }
}
