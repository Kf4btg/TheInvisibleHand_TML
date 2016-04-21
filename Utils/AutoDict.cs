using System.Collections.Generic;
using System.Text;

namespace InvisibleHand.Utils
{

    #region dictionary-based

    /// when attempting to access a key that is not present in the dictionary,
    /// rather than throwing an exception, the DefaultDict will create and return
    /// a new instance of the TValue type (for this reason, TValue has the new()
    /// constraint()).
    public class DefaultDict<K, V> : Dictionary<K, V> where V : new()
    {

        public bool HasChildren => this.Count > 0;

        public new V this[K key]
        {
            get
            {
                V ret;
                // according to MSDN, this is more efficient than catching the exception
                if (!base.TryGetValue(key, out ret))
                {
                    this.Add(key, new V());
                    ret = base[key];
                }
                return ret;
                // try{
                //     return base[key];
                // }
                // catch (KeyNotFoundException)
                // {
                //     this.Add(key, new V());
                // }
                // return base[key];
            }
            set {
                base[key] = value;
            }
        }

        public new virtual void Add(K key, V val)
        {
            base.Add(key, val);
        }
    }


    /// This is an implementation of an "Autovivifying Dictionary"--effectively a tree where
    /// each branch is a dictionary of child dictionaries; the "auto-vivification" comes in
    /// when attempting to access a node in the tree that does not exist: rather than throwing
    /// an exception, a new tree node (dictionary) will be created, inserted into the parent
    /// dictionary at the specified key location, and returned as the result of the access
    /// operation.
    /// Since the Value-Type of this Data structure is the type of the structure itself,
    /// data cannot be stored and accessed like a normal dictionary using the indexer[] notation.
    /// To store data in a node, use the public Data field, which is the type of the second
    /// generic parameter for the `AutoTree<TKey, TData>`
    /// Note: in python, the entire implementation of this object would be: `def Tree(): defaultdict(Tree)`
    public class AutoTree<K, V> : DefaultDict<K, AutoTree<K, V>>
    {
        public K Label { get; set; }

        public V Data { get; set; }

        public bool HasData => this.Data != null;


        private static int depth = 0;
        private static string indent => new string(' ', depth);
        public override string ToString()
        {
            string s = $"{indent}{Label}:\n";
            StringBuilder sb = new StringBuilder(s);

            if (this.Count > 0)
            {
                depth++;
                foreach (var kvp in this)
                {
                    sb.Append(kvp.Value.ToString());
                }
                depth--;
            }
            return sb.ToString();
        }

        /// Automatically assigns the Label based on the key
        public override void Add(K key, AutoTree<K, V> val)
        {
            val.Label = key;
            base.Add(key, val);
        }
    }

    #endregion

    #region sortedlist-based
    /// Same as the DefaultDict, but based on a SortedList rather than a Dictionary
    public class SortedDefaultDict<K, V> : SortedList<K, V> where V : new()
    {

        public bool HasChildren => this.Count > 0;

        public new V this[K key]
        {
            get
            {
                V ret;
                if (!base.TryGetValue(key, out ret))
                {
                    this.Add(key, new V());
                    ret = base[key];
                }
                return ret;
            }
            set {
                base[key] = value;
            }
        }

        public new virtual void Add(K key, V val)
        {
            base.Add(key, val);
        }
    }

    /// same as the regular AutoTree, but uses a SortedList as the underlying
    /// implementation rather than a normal dictionary
    public class SortedAutoTree<K, V> : SortedDefaultDict<K, SortedAutoTree<K, V>>
    {
        public K Label { get; set; }

        public V Data { get; set; }

        public bool HasData => this.Data != null;



        // for debugging, mainly
        private static int depth = 0;
        private static string indent => new string(' ', depth);
        public override string ToString()
        {
            string s = $"{indent}{Label}:\n";
            StringBuilder sb = new StringBuilder(s);

            if (this.Count > 0)
            {
                depth++;
                foreach (var kvp in this)
                {
                    sb.Append(kvp.Value.ToString());
                }
                depth--;
            }
            return sb.ToString();
        }
        
        /// Automatically assigns the Label based on the key
        public override void Add(K key, SortedAutoTree<K, V> val)
        {
            val.Label = key;
            base.Add(key, val);
        }
    }
    #endregion
}
