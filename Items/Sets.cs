using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using System.Collections;
using System.Linq;


namespace InvisibleHand.Items
{
    public class Set<T>: IEnumerable<T>
    {
        public string Name { get; protected set; }

        /// internal storage for the contents of this Set
        /// by default, this is a hashset
        protected readonly ISet<T> _items;

        /// create an empty set with the given name
        public Set(string name)
        {
            this.Name = name;
            this._items = new HashSet<T>();
        }

        /// create a new set with the given name and initialize
        /// it with the unique items in the given array
        public Set(string name, T[] items)
        {
            this.Name = name;
            this._items = new HashSet<T>(items);
        }

        /// Create a new set with the given name, initializing it
        /// from the union of the given collections
        public Set(string name, params IEnumerable<T>[] items)
        {
            this.Name = name;
            var itemset = new HashSet<T>();
            foreach (var itemlist in items)
            {
                itemset.UnionWith(itemlist);
            }
            this._items = itemset;
        }

        /// Add a single item to this Set. If it is already present
        /// in the set, it will be silently ignored.
        public void Add(T item)
        {
            this._items.Add(item);
        }

        /// Add the unique items from a number of collections of
        /// of items to this set; pre-existing items will not
        /// be duplicated and no error will be thrown.
        public void Extend(params IEnumerable<T>[] items)
        {
            foreach (var itemlist in items)
                this._items.UnionWith(itemlist);
        }

        /// Add the ids from the given collection to this set;
        /// if an ID is already present, it will not be duplicated
        /// and no error will be thrown.
        public void Union(IEnumerable<T> items)
        {
            this._items.UnionWith(items);
        }

        /// Remove all items in the given collection from this Set
        public void Remove(IEnumerable<T> items)
        {
            this._items.ExceptWith(items);
        }

        /// Return true if the given id exists in this set
        public bool Contains(T item)
        {
            return this._items.Contains(item);
        }

        /// Decrease the capacity of the Set's internal storage so that
        /// it is just large enough to hold its current contents. This
        /// should obviously only be done if all modification to the set
        /// have been completed and it is certain that no more changes to
        /// the set will occur during its lifetime.
        public void Trim()
        {
            ((HashSet<T>)this._items).TrimExcess();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }



    }

    /// A set for collections of TileIDs
    public class TileIDSet : Set<int>
    {
        public TileIDSet(string name) : base(name) { }

        public bool Contains(Item item) => this.Contains(item.createTile);

    }

    /// A set for collections of ItemIDs
    public class ItemIDSet : Set<int>
    {
        public ItemIDSet(string name) : base(name) { }

        public bool Contains(Item item) => this.Contains(item.type);
    }

    /// Various collections that will aid in identifying Items. Some of them may be sets of TileIDs
    /// (check item.createTile) and others may be sets of ItemIds (check item.type).
    /// Both types of set have a special Set&lt;int&gt; subclass that allows one to simply call
    /// mySet.Contains(item) where 'item' is a Terraria.Item instance and the Set will check the
    /// appropriate item property automagically. Should save some brainwork on when building rule methods.
    public static class ItemSets
    {
        /// a static, baseline, "no-such-set" set
        public static readonly Set<int> Empty = new Set<int>(string.Empty);

        public static readonly TileIDSet Furniture = new TileIDSet("Furniture");

        public static readonly TileIDSet CraftingStations = new TileIDSet("CraftingStations");

        public static readonly ItemIDSet AlchemyIngredients = new ItemIDSet("AlchemyIngredients");

        public static readonly ItemIDSet AlchemyResults = new ItemIDSet("AlchemyResults");

        public static readonly ItemIDSet Wood = new ItemIDSet("Wood");


        public static void Initialize()
        {
            Furniture.Extend(TileID.Sets.RoomNeeds.CountsAsDoor,
                             TileID.Sets.RoomNeeds.CountsAsChair,
                             TileID.Sets.RoomNeeds.CountsAsTable,
                             TileID.Sets.RoomNeeds.CountsAsTorch);

            Furniture.Trim();

            Wood.Extend(CraftGroup.Wood.Items);
            Wood.Trim();

            int tileID;
            Recipe r;

            // this might be a bad idea...let's see!
            for (int n = 0; n < Recipe.numRecipes; n++)
            {
                r = Main.recipe[n];

                // save a bit of gc-pressure
                for (int i = 0; i < Recipe.maxRequirements; i++)
                {
                    tileID = r.requiredTile[i];
                    // the requiredTile array is initialized from 0 on up;
                    // if we found -1 (the default value for the array entries),
                    // then there are no more required tiles for this recipe;
                    // since most recipes just have 0-1 (rarely even 2)
                    // required tiles, this could save us a good bit of time as the
                    // length of the required tile array is 15
                    if (tileID == -1) break;
                    CraftingStations.Add(tileID);
                }

                if (r.alchemy)
                {
                    // add the item-ids for the the ingredients;
                    // this isn't technically a 'tile-set', then,
                    // but I'd rather not go through the Recipes twice
                    AlchemyIngredients.Union(r.requiredItem.TakeWhile(item => item.type != 0).Select(item => item.type));

                    // also track the results
                    Console.WriteLine($"alch result: {r.createItem.name}");

                    // TODO: see about removing the Vicious/Vile Powder from this set
                    AlchemyResults.Add(r.createItem.type);
                }
            }
            AlchemyResults.Trim();
            // after all that, remove any items from the alchemy-ingredients set that are ALSO the result of
            // an alchemy recipe; this will prevent items such as lesser healing potions (which can be crafted
            // with alchemy, then used as an ingredient for an upgraded potion) from showing up under the
            // Ingredients category rather than the Potions category; will hopefully also stop mushrooms from showing
            // up in Potions.
            AlchemyIngredients.Remove(AlchemyResults);
            AlchemyIngredients.Trim();

            CraftingStations.Trim();
        }

        /// return a set by name
        public static Set<int> Get(string set_name)
        {
            // Note: if someday a large amount of sets get implemented,
            // should use an actual dictionary for this...
            switch (set_name)
            {
                case "Furniture":
                    return Furniture;
                case "Crafting":
                case "CraftingStations":
                    return CraftingStations;
                case "AlchemyIngredients":
                    return AlchemyIngredients;
                case "Wood":
                    return Wood;
                default:
                    ErrorLogger.Log($"No such set: '{set_name}'");
                    break;
            }
            return ItemSets.Empty;
        }

    }
}
