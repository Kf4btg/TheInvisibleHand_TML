using System.Collections.Generic;
using Terraria;

using System;
// using System.Linq;

namespace InvisibleHand.Items
{
    // save some typing in the fluent interface;
    // StaticFlagFunction
    using SFlagFunc = Func<ClassificationWrapper, string, string, ClassificationWrapper>;
    using SFlagsFunc = Func<ClassificationWrapper, string, string[], ClassificationWrapper>;

    public class ClassificationWrapper
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
        public ClassificationWrapper(Item item, ItemFlagInfo info)
        {
            this.item = item;
            this.info = info;

            this.info.Flags = new Dictionary<string, int>();
        }

        /// tag this instance with the given trait;
        /// return the modified instance
        public ClassificationWrapper SetFlag(string type, string flag)
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
        public ClassificationWrapper Flag(string type, string flag)
        {
            Success = false; // reset
            if (ConditionTable.Check(type, item, flag))
                SetFlag(type, flag);
            return this;
        }

        /// goes through the list of traits in the params list, attempting to tag each one; when a tag is successful,
        /// return without checking the remaining. Should be used for mutually-exclusive traits of the same
        /// Trait type.
        public ClassificationWrapper FlagFirst(string type, params string[] flags)
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
        public ClassificationWrapper FlagAny(string type, params string[] flags)
        {
            bool res = false;
            foreach (var f in flags)
                res |= this.Flag(type, f).Success;

            // we want to know if any of the operations succeeded, not just
            // the most recent one, so we catch any True value in res
            Success = res;
            return this;
        }
    }

    /// An attempt at making an API for the ClassificationWrapper that is slightly less painful
    /// and confusing to use. See ItemClassifier for examples of its usage.
    public static class ICWFluent
    {
        // Static Method Wrappers (SFLag[s]Func)
        // ------------
        public static ClassificationWrapper SetFlag(
                ClassificationWrapper item, string type, string flag)
                => item.SetFlag(type, flag);

        public static ClassificationWrapper Flag(
                ClassificationWrapper item, string type, string flag)
                => item.Flag(type, flag);

        public static ClassificationWrapper FlagFirst(
                ClassificationWrapper item, string type, params string[] flags)
                => item.FlagFirst(type, flags);

        public static ClassificationWrapper FlagAny(
                ClassificationWrapper item, string type, params string[] flags)
                => item.FlagAny(type, flags);


        // ---------------------------------------
        // Conditional Returns, Conditional Checks
        // ---------------------------------------
        // These are intended to be called as extension methods on an ItemClassificationWrapper instance.
        // Combined with the static wrappers given above and the (new in C# 6)
        // 'using static ICWFluent' statement, this should allow syntax like:
        //      bool tool = item.Try(FlagAny, "Tool", "pick", "axe", "drill");
        //      ...
        //      item.If(tool, Flag, "Weapon", "type_melee");
        // Of course the methods that return a ClassificationWrapper can be chained, as well
        // as combined with the normal ClassificationWrapper instance methods:
        //      item.If(...).FlagAny(...);
        // ---------------------------------------

        public static ClassificationWrapper If(this ClassificationWrapper item, bool condition,
                SFlagsFunc method, string type, params string[] flags)
                => condition ? method(item, type, flags) : item;

        public static ClassificationWrapper If(this ClassificationWrapper item, bool condition,
                SFlagFunc method, string type, string flag)
                => condition ? method(item, type, flag) : item;

        public static bool Try(this ClassificationWrapper item,
                SFlagsFunc method, string type, params string[] flags)
                => method(item, type, flags).Success;

        public static bool Try(this ClassificationWrapper item,
                SFlagFunc method, string type, string flag)
                => method(item, type, flag).Success;

        public static bool TryIf(this ClassificationWrapper item,
                bool condition, SFlagsFunc method, string type, params string[] flags)
                => condition && method(item, type, flags).Success;
                // C# short-circuits its ands, right?

        public static bool TryIf(this ClassificationWrapper item,
                bool condition, SFlagFunc method, string type, string flag)
                => condition && method(item, type, flag).Success;
    }
}
