using System;
using System.Linq;
using System.Collections.Generic;


namespace InvisibleHand.Utils
{
    public static class ConsoleHelper
    {
        public static void PrintList<T>(IEnumerable<T> objlist, string label = "", bool splitlines = false)
        {
            // Console.WriteLine("{0}, {1}", catmatcher.Count, string.Join(",\n", catmatcher.Select(kv => kv.Key).ToArray()));

            string listsep = splitlines ? ",\n" : ", ";

            if (label != String.Empty)
            {
                Console.WriteLine("{0} ({1} entries): {2}", label, objlist.Count(), string.Join(listsep, objlist.ToArray()));
            }
            else
                Console.WriteLine("{0} entries: {1}", objlist.Count(), string.Join(listsep, objlist.ToArray()));
        }

        public static void PrintDictKeys<K, V>(IDictionary<K, V> mapping, string label = "", bool splitlines = false)
        {
            string listsep = splitlines ? ",\n" : ", ";
            if (label != String.Empty)
            {
                Console.WriteLine("{0} ({1} entries): {2}", label, mapping.Count(), string.Join(listsep, mapping.Select(v=>v.Key).ToArray()));
            }
            else
                Console.WriteLine("{0} entries: {1}", mapping.Count, string.Join(listsep, mapping.Select(v=>v.Key).ToArray()));
        }

    }
}
