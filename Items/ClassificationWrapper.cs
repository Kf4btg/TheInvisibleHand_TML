using System.Collections.Generic;
using Terraria;

// using System;
// using System.Linq;

namespace InvisibleHand.Items
{
    internal class ItemClassificationWrapper
    {
        public readonly Item item;
        public ItemFlagInfo info;

        // set the flags directly on the ItemInfo instance
        public IDictionary<string, int> ItemFlags => info.Flags;

        // for the multi-tag operations, this can be checked to see if the operation was successful for that given scenario.
        /// Holds whether the most recent tagging attempt was successful
        /// (i.e. the trait's condition was satisfied)
        public bool Success { get; private set; }

        /// holds the Trait-group and flag-name of the most recently flagged trait
        public KeyValuePair<string, string> LastFlag { get; private set; }

        /// create a new wrapper around the given Item and ItemInfo
        public ItemClassificationWrapper(Item item, ItemFlagInfo info)
        {
            this.item = item;
            this.info = info;

            this.info.Flags = new Dictionary<string, int>();
        }

        /// tag this instance with the given trait;
        /// return the modified instance
        public ItemClassificationWrapper SetFlag(string type, string flag)
        {
            // try
            // {

                if (this.ItemFlags.ContainsKey(type))
                    this.ItemFlags[type] |= IHBase.FlagCollection[type][flag];
                else
                    this.ItemFlags[type] = IHBase.FlagCollection[type][flag];

                this.Success = true;
                this.LastFlag = new KeyValuePair<string, string>(type, flag);
            // }
            // catch (KeyNotFoundException)
            // {

                // Console.WriteLine("{0}[{1}]", type, flag);
                // Console.WriteLine(string.Join(", ", IHBase.FlagCollection.Select(kvp => kvp.Key).ToArray()));
                // Console.WriteLine(string.Join(", ", IHBase.FlagCollection[type].Select(kvp => kvp.Key).ToArray()));
                // throw;
            // }
            return this;
        }

        /// tag this instance with the given trait iff the condition
        /// for the trait (as found in the Condition Table) is true;
        ///return the instance, whether modified or not.
        public ItemClassificationWrapper Flag(string type, string flag)
        {
            Success = false; // reset
            if (ConditionTable.Check(type, item, flag))
                SetFlag(type, flag);
            return this;
        }



        /// goes through the list of traits in the params list, attempting to tag each one; when a tag is successful,
        /// return without checking the remaining. Should be used for mutually-exclusive traits of the same
        /// Trait type.
        public ItemClassificationWrapper FlagFirst(string type, params string[] flags)
        {
           foreach (var f in flags)
            {
                if (this.Flag(type, f).Success)
                    break;
            }
            return this;
        }

        /// attempts to tag each of the traits given in the params list
        /// without regard to the success of each tag operation.
        /// Can be used to try tagging related but not mutually-exclusive traits
        /// of the same Trait type.
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

        /// convenience function for checking the value of the .Success
        /// attribute after a Flag() operation
        public bool TryFlag(string type, string flag)
        {
            return Flag(type, flag).Success;
        }

        /// tag this instance with the given trait
        /// IFF condition is true; return the instance,
        /// modified or not.
        /// Mainly a shortcut for:
        /// 	if (condition) item.SetFlag(type, flag);
        public ItemClassificationWrapper SetFlagIf(bool condition, string type, string flag)
        {
            if (condition) return SetFlag(type, flag);

            Success = false; // reset
            return this;
        }

        /// shortcut for :
        /// 	if (condition) item.Flag(type, flag);
        public ItemClassificationWrapper FlagIf(bool condition, string type, string flag)
        {
            if (condition) return Flag(type, flag);

            Success = false; // reset
            return this;
        }

        /// shortcut for :
        /// 	if (condition) item.FlagFirst(type, flag1, flag2, ...)
        public ItemClassificationWrapper FlagFirstIf(bool condition, string type, params string[] flags)
        {
            if (condition) return FlagFirst(type, flags);

            Success = false; // reset
            return this;
        }

        /// shortcut for :
        /// 	if (condition) item.FlagAny(type, flag1, flag2, ...)
        public ItemClassificationWrapper FlagAnyIf(bool condition, string type, params string[] flags)
        {
            if (condition) FlagAny(type, flags);

            Success = false; // reset
            return this;
        }
    }
}
