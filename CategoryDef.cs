using System.Collections.Generic;
using System;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace InvisibleHand
{
    // Most everything here is ridiculously hacky and doesn't extrapolate well to mod-added items
    // Unfortunately, I expect it's probably impossible to sufficiently generalize the item-matching logic,
    // simply due to the way Terraria itself is coded (i.e. items/item abilities are pretty much hacked-in individually, anyway)
    public static class CategoryDef
    {
        // pass initial capacity as ItemCat.OTHER -- this trick should work so long as OTHER remains the last member of the Enum
        public static readonly Dictionary<ItemCat, List<string>> ItemSortRules  =
                new Dictionary<ItemCat, List<string>>((int)ItemCat.OTHER+1);

        public static readonly Dictionary<ItemCat, Func<Item, bool>> Categories =
                new Dictionary<ItemCat, Func<Item, bool>>((int)ItemCat.OTHER+1);

        public static void Initialize()
        {
            SetupCategories();
            SetupSortingRules();
        }

        /** ***********************************************************************
        *  Define the categories and their matching rules
        */
        public static void SetupCategories()
        {
            //FIXME: this is horrible...
                        
            // Original item matching functions unceremoniously stolen from Shockah's Fancy Cheat Menu mod,
            // who graciously did all the hard work figuring these out so I didn't have to!
            // Since then I've added a boat load of new categories and adjusted some of the
            // matching logic pretty heavily.
            Categories.Add(ItemCat.COIN,
                item => Constants.TileGroupCoin.Contains(item.createTile));

            Categories.Add(ItemCat.PICK,
                item => item.pick > 0);

            Categories.Add(ItemCat.AXE,
                item => item.axe > 0);

            Categories.Add(ItemCat.HAMMER,
                item => item.hammer > 0);

            Categories.Add(ItemCat.TOOL,
                item => item.IsTool());

            Categories.Add(ItemCat.MECH,
                item => item.mech || item.cartTrack);

            Categories.Add(ItemCat.MELEE,
                item => item.damage > 0 && item.melee);

            Categories.Add(ItemCat.RANGED,
                item => item.damage > 0 && item.ranged && (item.ammo == 0));

            Categories.Add(ItemCat.BOMB,
                item => item.IsBomb());

            Categories.Add(ItemCat.AMMO,
                item => item.damage > 0 && item.ranged && item.ammo != 0 && !item.notAmmo);

            Categories.Add(ItemCat.MAGIC,
                item => item.damage > 0 && item.magic);

            Categories.Add(ItemCat.SUMMON,
                item => item.damage > 0 && item.summon);

            Categories.Add(ItemCat.PET,
                item => item.damage <= 0
                && ((item.shoot > 0 && Main.projPet[item.shoot])
                    || (item.buffType > 0 && (Main.vanityPet[item.buffType]
                                                || Main.lightPet[item.buffType]))));

            Categories.Add(ItemCat.HEAD,
                item => item.headSlot != -1 && item.defense > 0);

            Categories.Add(ItemCat.BODY,
                item => item.bodySlot != -1 && item.defense > 0);

            Categories.Add(ItemCat.LEGS,
                item => item.legSlot != -1 && item.defense > 0);

            Categories.Add(ItemCat.ACCESSORY,
                item => item.accessory && !item.vanity);

            Categories.Add(ItemCat.VANITY,
                item => (item.vanity || item.headSlot != -1 || item.bodySlot != -1 || item.legSlot != -1) && item.defense == 0); //catch the non-armor

            // for some reason, all vanilla potions have w=14, h=24. Food, ale, etc. are all different.
            Categories.Add(ItemCat.POTION,
                item => item.Matches(ItemCat.CONSUME) && item.width == 14 && item.height == 24);

            Categories.Add(ItemCat.CONSUME,
                item => item.consumable
                && item.bait == 0
                && item.damage <= 0
                && item.createTile == -1
                && item.tileWand == -1
                && item.createWall == -1
                && item.ammo == 0
                && item.name != "Xmas decorations");

            Categories.Add(ItemCat.BAIT,
                item => item.bait > 0 && item.consumable);

            Categories.Add(ItemCat.DYE,
                item => item.dye != 0);

            Categories.Add(ItemCat.PAINT,
                item => item.paint != 0);

            // Categories.Add(ItemCat.ORE, 		item   	=> item.createTile != -1 && (item.name.EndsWith("Ore") || Constants.TileGroupOre.Contains(item.createTile)) );
            Categories.Add(ItemCat.ORE,
                item => TileID.Sets.Ore[item.createTile]);

            Categories.Add(ItemCat.BAR,
                item => item.createTile == TileID.MetalBars);

            Categories.Add(ItemCat.GEM,
                item => item.createTile == TileID.ExposedGems);

            Categories.Add(ItemCat.SEED,
                item => Constants.TileGroupSeed.Contains(item.createTile)
                || (item.createTile != -1 && item.name.EndsWith("Seeds")));

            Categories.Add(ItemCat.LIGHT,
                item => Constants.TileGroupLighting.Contains(item.createTile));

            Categories.Add(ItemCat.CRAFT,
                item => Constants.TileGroupCrafting.Contains(item.createTile));

            Categories.Add(ItemCat.FURNITURE,
                item => Constants.TileGroupFurniture.Contains(item.createTile));

            Categories.Add(ItemCat.STATUE,
                item => Constants.TileGroupStatue.Contains(item.createTile));

            Categories.Add(ItemCat.WALLDECO,
                item => Constants.TileGroupWallDeco.Contains(item.createTile));

            Categories.Add(ItemCat.BANNER,
                item => item.createTile == TileID.Banners);

            Categories.Add(ItemCat.CLUTTER,
                item => Constants.TileGroupClutter.Contains(item.createTile));

            Categories.Add(ItemCat.WOOD,
                item => CraftGroup.Wood.Items.Contains(item.type));

            Categories.Add(ItemCat.BLOCK,
                item => item.createTile != -1 && item.width == 12 && item.height == 12 && item.value == 0);

            Categories.Add(ItemCat.BRICK,
                item => item.Matches(ItemCat.BLOCK)
                && (item.name.EndsWith("Brick")
                || item.name.EndsWith("Slab")
                || item.name.EndsWith("Plating")));

            Categories.Add(ItemCat.TILE,
                item => (item.createTile != -1 && item.createTile != TileID.DyePlants) || item.tileWand != -1 || item.name == "Xmas decorations");

            Categories.Add(ItemCat.WALL,
                item => item.createWall != -1);

            Categories.Add(ItemCat.MISC_MAT,
                item => item.material);

            //Boss summon, hearts, mana crystals
            Categories.Add(ItemCat.SPECIAL,
                item => item.useStyle == 4);

            Categories.Add(ItemCat.OTHER,
                item => true);


    		// TODO: see if possible to read some NPCdef file to figure out if
    		//		 an item is an NPC or an environment (entity or tile) drop.

        } //end setupCategories()

        /** ***********************************************************************
        *    This is where we define how the items within each category will be
        *    sorted re: each other. Each category will be associated with a list
        *    of strings corresponding to properties of an Item instance. This list,
        *    through the magic of Dynamic LINQ, will be transformed into the
        *    OrderBy() arguments for that category.
        *
        *    Order of the items in the list matters (first item is primary sort field,
        *    second is secondary, and so on), and each property word may be followed by
        *    a space and the letters "desc", representing that that field should be
        *    sorted in descending order rather than the default ascending.
        */
        public static void SetupSortingRules()
        {
            ItemSortRules.Add(ItemCat.COIN,     new List<string> {
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.PICK,     new List<string> {
                "rare",
                "pick",
                "type",
                "value"
                });
            ItemSortRules.Add(ItemCat.AXE,      new List<string> {
                "rare",
                "axe",
                "type",
                "value"
                });
            ItemSortRules.Add(ItemCat.HAMMER,   new List<string> {
                "rare",
                "hammer",
                "type",
                "value"
                });
            // stack to sort the stackable boomerangs separately
            ItemSortRules.Add(ItemCat.MELEE,    new List<string> {
                "maxStack",
                "damage",
                "type",
                "rare",
                "value",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.TOOL,     new List<string> {
                "consumable",
                "fishingPole",
                "shoot",
                "type",
                "stack desc"
                });
            // consumable to sort throwing weapons separately
            ItemSortRules.Add(ItemCat.MECH,     new List<string> {
                "cartTrack",
                "tileBoost",
                "createTile desc",
                "value",
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.RANGED,   new List<string> {
                "consumable",
                "damage",
                "type",
                "rare",
                "value",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.BOMB,     new List<string> {
                "damage>0 desc",
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.AMMO,     new List<string> {
                "rare",
                "damage",
                "type",
                "value",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.MAGIC,    new List<string> {
                "damage",
                "rare",
                "type",
                "value"
                });
            ItemSortRules.Add(ItemCat.SUMMON,   new List<string> {
                "damage",
                "rare",
                "type",
                "value"
                });
            ItemSortRules.Add(ItemCat.PET,      new List<string> {
                "buffType",
                "type"
                });
            ItemSortRules.Add(ItemCat.HEAD,     new List<string> {
                "rare",
                "defense",
                "value",
                "type"
                });
            ItemSortRules.Add(ItemCat.BODY,     new List<string> {
                "rare",
                "defense",
                "value",
                "type"
                });
            ItemSortRules.Add(ItemCat.LEGS,     new List<string> {
                "rare",
                "defense",
                "value",
                "type"
                });
            ItemSortRules.Add(ItemCat.ACCESSORY,new List<string> {
                "handOffSlot",
                "handOnSlot",
                "backSlot",
                "frontSlot",
                "shoeSlot",
                "waistSlot",
                "wingSlot",
                "balloonSlot",
                "faceSlot",
                "neckSlot",
                "shieldSlot",
                "rare",
                "value",
                "type",
                "prefix.id"
                });
            // stack because of those fishbowls...
            ItemSortRules.Add(ItemCat.VANITY,   new List<string> {
                "headSlot!=-1 desc",
                "bodySlot!=-1 desc",
                "legSlot!=-1 desc",
                "name",
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.POTION,   new List<string> {
                "healLife desc",
                "healMana desc",
                "buffType!=0 desc",
                "buffType",
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.CONSUME,  new List<string> {
                "potion desc",
                "buffType",
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.BAIT,     new List<string> {
                "bait",
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.DYE,      new List<string> {
                "dye",
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.PAINT,    new List<string> {
                "paint",
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.ORE,      new List<string> {
                "rare",
                "value",
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.BAR,      new List<string> {
                "rare",
                "value",
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.GEM,      new List<string> {
                "rare",
                "value",
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.SEED,     new List<string> {
                "name",
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.LIGHT,    new List<string> {
                "createTile",
                "type",
                "name",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.CRAFT,    new List<string> {
                "createTile",
                "type",
                "name",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.FURNITURE,new List<string> {
                "createTile",
                "type",
                "name",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.STATUE,   new List<string> {
                "createTile",
                "type",
                "name",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.WALLDECO, new List<string> {
                "createTile",
                "type",
                "name",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.BANNER,   new List<string> {
                "type",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.CLUTTER,  new List<string> {
                "createTile",
                "type",
                "name",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.WOOD,     new List<string> {
                "createTile",
                "type",
                "name",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.BLOCK,    new List<string> {
                "createTile",
                "type",
                "name",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.BRICK,    new List<string> {
                "createTile",
                "type",
                "name",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.TILE,     new List<string> {
                "tileWand",
                "createTile",
                "type",
                "name",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.WALL,     new List<string> {
                "createWall",
                "type",
                "stack desc"
                });
            // generic stuff
            ItemSortRules.Add(ItemCat.MISC_MAT, new List<string> {
                "type",
                "netID",
                "stack desc"
                });
            ItemSortRules.Add(ItemCat.SPECIAL,  new List<string> {
                "maxStack",
                "type",
                "stack desc"
                });
            // quest fish: uniquestack=true, rare=-11
            ItemSortRules.Add(ItemCat.OTHER,    new List<string> {
                "uniqueStack",
                "rare",
                "type",
                "netID",
                "stack desc"
                });
        }//end setup sorting rules
    }
}
