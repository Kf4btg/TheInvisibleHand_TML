using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
// using System;
using Terraria;

using InvisibleHand.Utils;
using InvisibleHand.Items;
using InvisibleHand.Items.Categories.Types;

namespace InvisibleHand
{
    public static class IHExtensions
    {
        #region generic extensions

        /// Updates (creates new key-value pairs and overwrites existing pairs) a dictionary
        /// with the entries from a second dictionary
        public static void Update<K,V>(this IDictionary<K, V> self, IDictionary<K, V> other)
        {
            if (other != null)
                foreach (var kvp in other)
                    self[kvp.Key] = kvp.Value;
        }

        /// Extend (add all elements from other, which, for collections of
        /// key-value-pairs like IDictionary, will throw exceptions if a key in `other`
        /// is null or already present) this collection with the entries from the other enumerable
        public static void Extend<T>(this ICollection<T> self, IEnumerable<T> other)
        {
            if (other != null)
                foreach (var item in other)
                    self.Add(item);
        }

        /// <summary>
        /// Get the array slice between the two indexes.
        /// ... Inclusive for start index, exclusive for end index.</summary>
        // public static T[] slice<T>(this T[] source, int start, int end = -1)
        // {
        //     // Handle negative ends
        //     if (end < 0)
        //         end = source.Length + end;
        //     int len = end - start;
        //
        //     T[] sub = new T[len];
        //     Array.Copy(source, start, sub, 0, len);
        //
        //     return sub;
        // }

        #endregion

        /// query the Main (current) keystate about the given key
        public static bool Down(this Keys key)
        {
            return Main.keyState.IsKeyDown(key);
        }
        public static bool Up(this Keys key)
        {
            return Main.keyState.IsKeyUp(key);
        }

        /// query an arbitrary keystate about the given key
        // public static bool Down(this Keys key, KeyboardState state)
        // {
        //     return state.IsKeyDown(key);
        // }
        // public static bool Up(this Keys key, KeyboardState state)
        // {
        //     return state.IsKeyUp(key);
        // }

        /// returns true if the key is down now, but was up in the previous state
        // public static bool Pressed(this Keys key, KeyboardState previous_state)
        // {
        //     return key.Up(previous_state) && key.Down();
        // }


        #region itemExensions

        // category stuff:

        public static ItemFlagInfo GetFlagInfo(this Item item)
        {
            return item.GetModInfo<ItemFlagInfo>(IHBase.Instance);
        }

        public static ItemCategory GetCategory(this Item item)
        {
            // var iteminfo = (ItemFlagInfo)(item.GetModInfo(IHBase.Instance, "ItemFlagInfo"));
            var iteminfo = item.GetFlagInfo();
            if (iteminfo.Flags == null)
                ItemClassifier.ClassifyItem(item, iteminfo);

            return iteminfo.Category;
        }

        // sorting/movement stuff:

        public static bool CanStackWith(this Item src_item, Item dest_item)
        {
            return !dest_item.IsBlank() && dest_item.IsTheSameAs(src_item) && dest_item.stack < dest_item.maxStack;
        }

        public static bool IsBlank(this Item item)
        {

            return item.type == 0 || item.stack == 0;
        }

    #endregion

    #region misc
        ///play the given sound effect;
        public static void Play(this Sound s)
        {
            SoundManager.Queue(s);
        }

        ///<param name="min">Minimum value boundary; default is 0</param>
        ///<param name="max">Maximum value boundary; default is 1</param>
        ///<returns>value bound by specified minumum and maximum
        /// values (inclusive)</returns>
        public static float Clamp(this float value, float min = 0f, float max = 1.0f)
        {
            return MathHelper.Clamp(value, min, max);
        }

        public static int Clamp(this int value, int min = 0, int max = 1)
        {
            return (int)MathHelper.Clamp(value, min, max);
        }

    #endregion
    }
}
