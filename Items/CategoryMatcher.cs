// using System;
using System.Collections.Generic;


namespace InvisibleHand.Items
{
    using IF = ItemFlags;

    // using CatMatcher = Dictionary<string, Func<FlagInfo, bool>>;
    using FlagMatcher = Dictionary<FlagInfo, string>;
    using LabelMatcher = Dictionary<string, FlagInfo>;

    // Create and populate a decision table of flag combinations and what ItemCategory an item
    // matching those conditions should resolve to.  Ultimate goal is for this to be flexible
    // and customizable; perhaps even implement a learning algorithm that will guess at the
    // most appropriate categorization for unrecognized items. Defining these rules externally
    // (e.g. in a json file that is read in on load) would be best, but doing it here will
    // have to do for now.
    public static class CategoryMatcher_Alpha
    {
        public static CategoryCollection categories = new CategoryCollection();
        public static FlagMatcher flagegories = new FlagMatcher();
        public static LabelMatcher cateflags = new LabelMatcher();

        // public static Dictionary<string, FlagInfo> categories = new Dictionary<string, FlagInfo>();

        /// Use this as a template for creating new categories below
        private static FlagInfo template = new FlagInfo
        {
            general = IF.general.none,
            placeable = IF.placeable.none,
            weapon = IF.weapon.none,
            tool = IF.tool.none,
            furniture = IF.furniture.none,
            equip = IF.equip.none,
            ammo = IF.ammo.none,
            consumable = IF.consumable.none,
            dye = IF.dye.none,
            mech = IF.mech.none,
            w_melee = IF.weapon.melee.none,
            w_ranged = IF.weapon.ranged.none,
            w_magic = IF.weapon.magic.none,
            w_summon = IF.weapon.summon.none,
            f_door = IF.furniture.doors.none,
            f_light = IF.furniture.lighting.none,
            f_table = IF.furniture.tables.none,
            f_chair = IF.furniture.chairs.none,
            f_other = IF.furniture.other.none
        };

        private static class cat
        {
            private static bool tool(FlagInfo info) => (info.tool & IF.general.tool) != 0;

            private static bool weapon(FlagInfo info) => (info.general & IF.general.weapon) != 0;

            private static bool melee(FlagInfo info) => (info.weapon & IF.weapon.type_melee) != 0;
            private static bool ranged(FlagInfo info) => (info.weapon & IF.weapon.type_ranged) != 0;
            private static bool magic(FlagInfo info) => (info.weapon & IF.weapon.type_magic) != 0;
            private static bool summon(FlagInfo info) => (info.weapon & IF.weapon.type_summon) != 0;
            private static bool thrown(FlagInfo info) => (info.weapon & IF.weapon.type_throwing) != 0;
        }

        static CategoryMatcher_Alpha()
        {
            // var what = new[]
            // {
            //     // {"tool", }
            // }

            // var F = flagegories;
            // var C = cateflags;
            var C = categories;

            // F[new FlagInfo { general = IF.general.tool }] = "tool";
            // F[new FlagInfo { general = IF.general.weapon }] = "weapon";

            C["tool"] = new FlagInfo { general = IF.general.tool };
            C["weapon"] = new FlagInfo { general = IF.general.weapon };


            // foreach (var kvp in F)
            // {
            //     C[kvp.Value] = kvp.Key;
            // }


            // categories["pick"] = new FlagInfo
            // { general = IF.general.tool, tool = IF.tool.pick };
            //
            // categories["axe"] = new FlagInfo
            // { general = IF.general.tool, tool = IF.tool.axe };
            //
            // categories["hammer"] = new FlagInfo
            // { general = IF.general.tool, tool = IF.tool.axe };
        }
    }

    public class CategoryCollection : Dictionary<string, FlagInfo>
    {

        /// override the ContainsValue() method to check the bitwise-&
        /// of the given value against that of the values held in the dictionary.
        new public bool ContainsValue(FlagInfo value)
        {
            foreach (var kvp in this)
            {
                if ((kvp.Value & value) == value)
                    return true;
            }
            return false;
        }

        public IList<string> MatchingCategories(FlagInfo value)
        {
            var matches = new List<string>();
            foreach (var kvp in this)
            {
                if ((kvp.Value & value) == value)
                    matches.Add(kvp.Key);
            }
            return matches;

        }

        // private Dictionary<string, FlagInfo> _collection;
        // public int Count { get { return _collection.Count; } }
        // public bool IsReadOnly { get { return false; } }

        // public FlagInfo this[string key]
        // {
        //     get { return _collection[key]; }
        //     set { _collection[key] = value; }
        // }

        // public ICollection<string> Keys => _collection.Keys;
        // public ICollection<FlagInfo> Values => _collection.Values;

        // public CategoryCollection(int count = -1)
        // {
        //     if (count > 0)
        //         _collection = new Dictionary<string, FlagInfo>(count);
        //     else
        //         _collection = new Dictionary<string, FlagInfo>();
        // }

        // public void Add(KeyValuePair<string, FlagInfo> item)
        // {
        //     _collection.Add(item);
        // }

        // public void Add(string key, FlagInfo value)
        // {
        //     _collection.Add(key, value);
        // }
        //
        // public void ContainsKey(string key, FlagInfo value)



    }
}
