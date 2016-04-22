using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using System.Linq;


namespace InvisibleHand.Items
{

    public class Set
    {
        public string name { get; private set; }

        public readonly ISet<int> IDs;

        public Set(string name, int[] ids)
        {
            this.name = name;
            this.IDs = new HashSet<int>(ids);
        }

        /// create an empty set
        public Set(string name)
        {
            this.name = name;
            this.IDs = new HashSet<int>();
        }

        /// Create the set from a union of int lists
        public Set(string name, params IEnumerable<int>[] ids)
        {
            this.name = name;
            var idset = new HashSet<int>();
            foreach (var idlist in ids)
            {
                idset.UnionWith(idlist);
            }
            this.IDs = idset;
        }

        public void Add(int id)
        {
            this.IDs.Add(id);
        }

        public void Extend(IEnumerable<int> ids)
        {
            this.IDs.UnionWith(ids);
        }

        public bool Contains(int id)
        {
            return this.IDs.Contains(id);
        }

        public void Trim()
        {
            ((HashSet<int>)this.IDs).TrimExcess();
        }

        // a static, baseline, "no-such-set" set
        public static readonly Set Empty = new Set(string.Empty);

    }

    public static class TileSets
    {

        public static readonly Set Furniture;

        public static readonly Set CraftingStations;

        public static readonly Set AlchemyIngredients;

        static TileSets()
        {
            Furniture = new Set("Furniture", TileID.Sets.RoomNeeds.CountsAsDoor,
            TileID.Sets.RoomNeeds.CountsAsChair, TileID.Sets.RoomNeeds.CountsAsTable, TileID.Sets.RoomNeeds.CountsAsTorch);

            Furniture.Trim();

            // this might be a bad idea...let's see!
            CraftingStations = new Set("CraftingStations");
            AlchemyIngredients = new Set("AlchemyIngredients");

            int tileID;
            foreach (var r in Main.recipe)
            {
                // foreach (var tileID in r.requiredTile)

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
                    AlchemyIngredients.Extend(r.requiredItem.TakeWhile(i => i.type != 0).Select(i => i.type));
                }
            }
            CraftingStations.Trim();
            AlchemyIngredients.Trim();
        }

        /// return a set by name
        public static Set Get(string set_name)
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
                default:
                    ErrorLogger.Log($"No such set: '{set_name}'");
                    break;
            }
            return Set.Empty;
        }

    }

    public static class ItemSets
    {
        public static readonly Set Wood;

        static ItemSets()
        {
            // cache this here so we don't need to do a lookup each time we check
            Wood = new Set("Wood", CraftGroup.Wood.Items);
            Wood.Trim();
        }

        /// return a set by name
        public static Set Get(string set_name)
        {
            // Note: if someday a large amount of sets get implemented,
            // should use an actual dictionary for this...
            switch (set_name)
            {
                case "Wood":
                    return Wood;
                default:
                    ErrorLogger.Log($"No such set: '{set_name}'");
                    break;

            }
            return Set.Empty;
        }
    }
}
