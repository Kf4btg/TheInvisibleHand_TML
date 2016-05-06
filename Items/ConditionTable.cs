using System.Collections.Generic;
using System;
using Terraria;
using Terraria.ID;
using System.Linq;

namespace InvisibleHand.Items
{
    using static ClassificationRules;
    using condition_table = Dictionary<string, Func<Item, bool>>;


    // NOTE TO SELF:
    // When adding new traits, categories, unions, etc. make sure that everything is properly updated:
    //      1) The Category Hjson files have definitions for the new categories/use the new traits
    //      2) the Traits hjson has any new traits used by new categories
    //      3) The Condition tables below are updated with the same traits added to the Traits hjson file
    //      4) A method is created in ClassificationRules, if need be, along with any new Sets
    //      5) The traits are referenced and assigned appropriately in the ItemClassifier
    //
    //      *) And, eventually, try to clean out any old, unused, or obsolete code left lying around by the changes

    internal static class ConditionTable
    {
        // private static bool REPLACE_ME(Item item) => true;
        // TODO: maybe use 'nameof()' through here?

        public static readonly condition_table General = new condition_table
        {
            {"quest_item",    (i) => i.questItem},
            {"expert",        (i) => i.expert},

            // special case (well, actually, 'material' is the special case...);
            // 'material_notcategory' is used to mark ANY item for which .material is true.
            // However, not every one of those items should fall in the 'Material' category,
            // which is why the "material" flag has a few extra conditions on it.
            // 'material_notcategory' may be used later on for some fancy filtering features.
            // Or it may not.
            {"material_notcategory",      (i) => i.material},
            {"material",      Binary.isMaterial},

            {"mech",          (i) => i.mech},
            {"channeled",     (i) => i.channel},
            {"bait",          (i) => i.bait > 0},
            {"reach_boost",   (i) => i.tileBoost > 0},
            {"reach_penalty", (i) => i.tileBoost < 0},
            {"regen_life",    (i) => i.lifeRegen > 0},
            {"boost_mana",    (i) => i.manaIncrease > 0},
            {"use_mana",      (i) => i.mana > 0},
            {"vanity",        (i) => i.vanity},
            {"dye",           (i) => i.dye > 0},
            {"equipable",     Binary.isEquipable},
            {"placeable",     Binary.CanBePlaced},
            {"consumable",    Binary.isConsumable},
            {"weapon",        Binary.isWeapon},
            {"ammo",          Binary.isAmmo},
            {"explosive",     Binary.Explosive},
            {"defense",       (i) => i.defense > 0},
            //~ {"soul",          Sets.Soul},
            {"hair_dye",      (i) => i.hairDye > 0},
            {"coin",          (i) => i.ammo == Constants.AmmoID.Coin},
            {"paint",         (i) => i.paint > 0},
        };

        public static readonly condition_table Placeable = new condition_table
        {
            {"lighted",       Binary.givesLight},
            //~ {"furniture",  REPLACE_ME},
            {"seed",          ByTileID.ImmatureHerb},
            {"strange_plant", Sets.StrangePlant},
            {"block",         Sets.Block},
            // {"brick",      REPLACE_ME},
            {"wood",          Sets.Wood},
            {"wall",          (i) => i.createWall > 0},
            {"wall_deco",     Sets.WallDeco},
            {"musicbox",      ByTileID.MusicBox},
            {"banner",        ByTileID.Banner},
            {"track",         (i) => i.cartTrack},
            {"rope",          Sets.Rope},
            {"rope_coil",     Sets.RopeCoil},
            {"metal_detector",     Binary.showsOnMetalDetector},
        };

        public static readonly condition_table Placeable_Block = new condition_table
        {
            // {"brick", },
            {"bouncy", Sets.BouncyBlock},
            {"dungeon_brick", Sets.DungeonBrick},

            {"hallow", Sets.HallowBlock},
            {"crimson", Sets.CrimsonBlock},
            {"corrupt", Sets.CorruptionBlock},

            {"sand", Sets.Sand},
            {"hardened_sand", Sets.HardenedSand},
            {"sandstone", Sets.Sandstone},

            {"ice", Sets.Ice},
            {"stone", Sets.Stone},

            {"wood", Sets.Wood},
        };

        public static readonly condition_table Ammo = new condition_table
        {
            {"arrow",     (i) => i.ammo == Constants.AmmoID.Arrow},
            {"bullet",    (i) => i.ammo == Constants.AmmoID.Bullet},
            {"rocket",    (i) => i.ammo == Constants.AmmoID.Rocket},
            {"dart",      (i) => i.ammo == Constants.AmmoID.Dart},
            {"sand",      (i) => i.ammo == 42},
            {"coin",      (i) => i.ammo == Constants.AmmoID.Coin},
            {"solution",  (i) => i.ammo == Constants.AmmoID.Solution},
            {"endless",   (i) => i.ammo > 0 && !i.consumable},
        };

        public static readonly condition_table Dye = new condition_table
        {
            {"basic",    Dyes.BasicDyes},
            {"black",    Dyes.BlackDyes},
            {"bright",   Dyes.BrightDyes},
            {"silver",   Dyes.SilverDyes},
            {"flame",    Dyes.FlameDyes},
            {"gradient", Dyes.GradientDyes},
            {"strange",  Dyes.StrangeDyes},
            {"lunar",    Dyes.LunarDyes},
        };

        public static readonly condition_table Equip = new condition_table
        {
            {"armor",        Binary.isArmor},
            {"accessory",    (i) => i.accessory},
            // {"pet",       REPLACE_ME},
            {"vanity",       (i) => i.vanity},
            {"mount",        Binary.isMount},
            {"grapple",      Binary.isHook},
            {"slot_head",    (i) => i.headSlot > 0},
            {"slot_body",    (i) => i.bodySlot > 0},
            {"slot_leg",     (i) => i.legSlot > 0},
            {"slot_face",    (i) => i.faceSlot > 0},
            {"slot_neck",    (i) => i.neckSlot > 0},
            {"slot_back",    (i) => i.backSlot > 0},
            {"slot_wings",   (i) => i.wingSlot > 0},
            {"slot_shoe",    (i) => i.shoeSlot > 0},
            {"slot_handon",  (i) => i.handOnSlot > 0},
            {"slot_handoff", (i) => i.handOffSlot > 0},
            {"slot_shield",  (i) => i.shieldSlot > 0},
            {"slot_waist",   (i) => i.waistSlot > 0},
            {"slot_balloon", (i) => i.balloonSlot > 0},
            {"slot_front",   (i) => i.frontSlot > 0},
            {"pet_light",    Binary.isLightPet},
            {"pet_vanity",   Binary.isVanityPet},
            // {"grapple_single",  REPLACE_ME},
            // {"grapple_multi",  REPLACE_ME},
            // {"mount_cart",  REPLACE_ME},
        };

        public static readonly condition_table Weapon = new condition_table
        {
            {"automatic",       (i) => i.autoReuse},
            {"type_melee",      (i) => i.melee},
            {"has_projectile",  (i) => i.shoot > 0},
            {"type_ranged",     (i) => i.ranged},
            {"type_magic",      (i) => i.magic},
            {"type_summon",     (i) => i.summon},
            {"type_throwing",   (i) => i.thrown},
            // {"weapon_other",  REPLACE_ME},
        };

        public static readonly condition_table Weapon_Melee = new condition_table
        {
            {"style_swing",       Weapons.Melee.Swing},
            {"style_jab",         Weapons.Melee.Jab},
            {"style_directional", Weapons.Melee.Directional},
            {"style_thrown",      Weapons.Melee.Thrown},
            {"broadsword",        Weapons.Melee.BroadSword},
            {"boomerang",         Weapons.Melee.Boomerang},
            {"chain",             Weapons.Melee.ChainWeapon},
            {"spear",             Weapons.Melee.Spear},
            {"flail",             Weapons.Melee.Flail},
            {"yoyo",              Weapons.Melee.Yoyo},
        };

        public static readonly condition_table Weapon_Ranged = new condition_table
        {
            {"bullet_consuming", Weapons.Ranged.BulletConsuming},
            {"arrow_consuming",  Weapons.Ranged.ArrowConsuming},
            {"rocket_consuming", Weapons.Ranged.RocketConsuming},
            {"dart_consuming",   Weapons.Ranged.DartConsuming},
            {"gel_consuming",    Weapons.Ranged.GelConsuming},
            {"flamethrower",     Weapons.Ranged.FlameThrower},
            {"no_ammo",          (i) => i.useAmmo < 0},
        };

        public static readonly condition_table Weapon_Magic = new condition_table
        {
            {"area",       Weapons.Magic.Area},
            {"homing",     Weapons.Magic.Homing},
            {"bouncing",   Weapons.Magic.Bouncing},
            {"controlled", Weapons.Magic.Controllable},
            {"stream",     Weapons.Magic.Stream},
            {"piercing",   Weapons.Magic.Piercing},
        };

        public static readonly condition_table Weapon_Summon = new condition_table
        {
            {"minion", Weapons.Summon.Minion},
            {"sentry", Weapons.Summon.Sentry},
        };

        public static readonly condition_table Tool = new condition_table
        {
            {"pick",         (i) => i.pick > 0},
            {"axe",          (i) => i.axe > 0},
            {"hammer",       (i) => i.hammer > 0},

            {"fishing_pole", (i) => i.fishingPole > 0},
            {"wand",         (i) => i.tileWand > 0},
            {"wrench",       Binary.isWrench},
            {"bucket",       MiscTools.Bucket},
            {"bug_net",       MiscTools.BugNet},
            {"painting",     MiscTools.PaintingTools},
            // {"recall",       MiscTools.Recall},
            // {"other",  REPLACE_ME},
        };

        public static readonly condition_table Consumable = new condition_table
        {
            {"buff",   Binary.timedBuff},
            {"food",   Binary.isFood},
            {"potion", Binary.isPotion},
            {"flask",  Binary.isFlask},
            {"heal_life",     (i) => i.healLife > 0},
            {"heal_mana",     (i) => i.healMana > 0},
        };

        public static readonly condition_table Mech = new condition_table
        {
            {"timer",          ByTileID.Timer},
            // {mech.Switch, ByTileID.L},
            {"trap",           ByTileID.Trap},
            {"track",          (i) => i.cartTrack},
            {"firework",       ByTileID.Firework},
            // {"lever",       REPLACE_ME},
            {"pressure_plate", ByTileID.PressurePlate},
        };

        public static readonly condition_table Furniture = new condition_table
        {
            {"valid_housing",    Sets.Furniture},
            {"housing_door",     Sets.housingDoor},
            {"housing_light",    Sets.housingTorch},
            {"housing_chair",    Sets.housingChair},
            {"housing_table",    Sets.housingTable},
            // {"clutter",       REPLACE_ME},
            {"crafting_station", Sets.CraftingStation},
            {"container",        ByTileID.Container},
            // {"useable",  REPLACE_ME},
            // {"decorative",  REPLACE_ME},
        };

        public static readonly condition_table Furniture_Doors = new condition_table
        {
            {"door",  ByTileID.Door},
        };

        public static readonly condition_table Furniture_Lighting = new condition_table
        {
            {"torch",           ByTileID.Torch},
            {"candle",          ByTileID.Candle},
            {"chandelier",      ByTileID.Chandelier},
            {"hanging_lantern", ByTileID.HangingLantern},
            {"lamp",            ByTileID.Lamp},
            {"holiday_light",   ByTileID.HolidayLight},
            {"candelabra",      ByTileID.Candelabra}
        };

        public static readonly condition_table Furniture_Tables = new condition_table
        {
            {"table",     ByTileID.Table},
            {"workbench", ByTileID.WorkBench},
            {"dresser",   ByTileID.Dresser},
            {"piano",     ByTileID.Piano},
            {"bookcase",  ByTileID.Bookcase},
            {"bathtub",   ByTileID.Bathtub},
        };

        public static readonly condition_table Furniture_Chairs = new condition_table
        {
            {"chair", ByTileID.Chair},
            {"bed",   ByTileID.Bed},
            {"bench", ByTileID.Bench}
        };

        public static readonly condition_table Furniture_Other = new condition_table
        {
            {"sink",            ByTileID.Sink},
            {"clock",           ByTileID.GrandfatherClock},
            {"bottle",          ByTileID.Bottle},
            {"bowl",            ByTileID.Bowl},
            {"beachstuff",      ByTileID.BeachPile},
            {"tombstone",       ByTileID.Tombstone},
            {"campfire",        ByTileID.Campfire},
            {"statue",          ByTileID.Statue},
            {"statue_alphabet", ByTileID.AlphabetStatue},
            {"crate",           ByTileID.FishingCrate},
            {"monolith",        ByTileID.LunarMonolith},
            {"cooking_pot",     ByTileID.CookingPot},
            {"anvil",           ByTileID.Anvil},
            {"cannon",          ByTileID.Cannon},
            {"planter",         ByTileID.PlanterBox},
            {"fountain",        ByTileID.WaterFountain},
        };

        public static readonly condition_table Material = new condition_table
        {
            {"ore",           Sets.Ore},
            {"bar",           ByTileID.MetalBar},
            {"dye_plant",     ByTileID.DyePlant},
            {"gem",           ByTileID.Gem},
            {"alchemy",       Sets.AlchemyIngredient},
            {"soul",          Sets.Soul},
            {"critter",       Sets.Critter},
            {"butterfly",     Sets.Butterfly},
        };

        /// IDEA: new category: supplies? Resources? Spelunking/Climbing/Camping Equipment?
        /// this would include Rope, torches, glowsticks, breathing reed, magic mirror, maybe bombs
        /// and even food?
        /// Actually all of those (other than the mirror and reed) are consumables, hmm...
        public static readonly condition_table Tool_Exploration = new condition_table
        {
            {"rope",          Sets.Rope},
            {"rope_coil",     Sets.RopeCoil},
            // {"exploration",       MiscTools.TerrainSurvival},
            {"survival",       MiscTools.TerrainSurvival},
            {"hand_light",       MiscTools.HandLights},
            {"recall",       MiscTools.Recall},
            {"demolitions",       MiscTools.Demolitions},
        };

        /// retrieve the appropriate table by name
        private static condition_table RuleMatrix(string rule_category)
        {
            // this was using a large, nested dictionary, but my IDE would freak out and send all
            // my processors to 100% when I attempted to edit it; hopefully using
            // a series of dictionaries and a switch statement will calm it down. I expect it's better
            // on memory at runtime, as well.
            switch (rule_category)
            {
                case "General": return General;
                case "Placeable": return Placeable;
                case "Placeable.Block": return Placeable_Block;
                case "Ammo": return Ammo;
                case "Dye": return Dye;
                case "Equip": return Equip;
                case "Weapon": return Weapon;
                case "Weapon.Melee": return Weapon_Melee;
                case "Weapon.Ranged": return Weapon_Ranged;
                case "Weapon.Magic": return Weapon_Magic;
                case "Weapon.Summon": return Weapon_Summon;
                case "Tool": return Tool;
                case "Tool.Exploration": return Tool_Exploration;
                case "Consumable": return Consumable;
                case "Mech": return Mech;
                case "Furniture": return Furniture;
                case "Furniture.Doors": return Furniture_Doors;
                case "Furniture.Lighting": return Furniture_Lighting;
                case "Furniture.Tables": return Furniture_Tables;
                case "Furniture.Chairs": return Furniture_Chairs;
                case "Furniture.Other": return Furniture_Other;
                case "Material": return Material;
            }
            throw new UsefulKeyNotFoundException(rule_category, nameof(RuleMatrix),
                "Invalid table name '{0}'; does not exist in {1}.");
        }

        public static bool Check(String table, Item item, string flag)
        {
            // Console.WriteLine("{0}[{1}]", table, flag);
            try
            {
                return RuleMatrix(table)[flag](item);
            }
            catch (KeyNotFoundException knfe)
            {
                throw new UsefulKeyNotFoundException(flag, table, knfe,
                    "The trait '{0}' does not exist in table '{1}'.");
            }
        }

        static bool contains(IList<bool> idlist, int id) => id > 0 && idlist[id];

        static bool TestProjectileAI(Item item, int style_id)
        {
            var projid = item.shoot;
            if (projid < 1) return false;

            var proj = Main.projectile[projid];

            proj.SetDefaults(projid);
            return proj.aiStyle == style_id;
        }

        // omnisharp really sucks sometimes...
        internal static readonly condition_table TraitTable = new condition_table();

        static ConditionTable()
        {
            // var this_is_stupid = TraitTable;
            var tt = TraitTable;
            //bool
            tt["ammunition"]     = (i) => i.ammo > 0;
            tt["auto_reuse"]     = (i) => i.autoReuse;
            tt["channel"]        = (i) => i.channel;
            tt["consumable"]     = (i) => i.consumable;
            tt["expert"]         = (i) => i.expert;
            tt["material"]       = (i) => i.material;
            tt["mech"]           = (i) => i.mech;
            tt["quest_item"]     = (i) => i.questItem;
            tt["unique_stack"]   = (i) => i.uniqueStack;
            tt["vanity"]         = (i) => i.vanity;
            tt["accessory"]      = (i) => i.accessory;
            tt["stackable"]      = (i) => i.maxStack > 1;
            tt["no_use_graphic"] = (i) => i.noUseGraphic;
            tt["no_melee"]       = (i) => i.noMelee;
            tt["not_ammo"]       = (i) => i.notAmmo;
            tt["cart_track"]     = (i) => i.cartTrack;

            //valued
            tt["damage"]        = (i) => i.damage > 0;
            tt["defense"]       = (i) => i.defense > 0;
            tt["knockback"]     = (i) => i.knockBack > 0;
            tt["buff"]          = (i) => i.buffType != -1;
            tt["buff_time"]     = (i) => i.buffTime > 0;
            tt["projectile"]    = (i) => i.shoot > 0;
            tt["reach_boost"]   = (i) => i.tileBoost > 0;
            tt["reach_penalty"] = (i) => i.tileBoost < 0;
            tt["create_tile"]   = (i) => i.createTile != -1;
            tt["create_wall"]   = (i) => i.createWall != -1;
            tt["create_npc"]    = (i) => i.makeNPC != 0;
            tt["value"]         = (i) => i.value > 0;
            tt["mana_cost"]     = (i) => i.mana > 0;
            tt["heal_life"]     = (i) => i.healLife > 0;
            tt["heal_mana"]     = (i) => i.healMana > 0;

            //ident
            tt["ammo"]         = (i) => i.ammo > 0 && !i.notAmmo;
            tt["bait"]         = (i) => i.bait > 0;
            tt["hair_dye"]     = (i) => i.hairDye > 0;
            tt["dye"]          = (i) => i.dye > 0;
            tt["paint"]        = (i) => i.paint > 0;
            tt["coin"]         = (i) => i.ammo == Constants.AmmoID.Coin;
            tt["fishing_pole"] = (i) => i.fishingPole > 0;
            tt["wand"]         = (i) => i.tileWand > 0;



            // ammo
            tt["arrow"]     = (i) => i.ammo == Constants.AmmoID.Arrow;
            tt["bullet"]    = (i) => i.ammo == Constants.AmmoID.Bullet;
            tt["rocket"]    = (i) => i.ammo == Constants.AmmoID.Rocket;
            tt["dart"]      = (i) => i.ammo == Constants.AmmoID.Dart;
            tt["ammo_sand"] = (i) => i.ammo == Constants.AmmoID.Sand;
            tt["solution"]  = (i) => i.ammo == Constants.AmmoID.Solution;
            // tt["coin"]     = (i) => i.ammo == Constants.AmmoID.Coin};
            // tt["endless"] =  (i) => i.ammo > 0 && !i.consumable;


            // use-styles
            tt["use_style_any"] = (i) => i.useStyle > 0;
            tt["use_style_1"]   = (i) => i.useStyle == 1;
            tt["use_style_2"]   = (i) => i.useStyle == 2;
            tt["use_style_3"]   = (i) => i.useStyle == 3;
            tt["use_style_4"]   = (i) => i.useStyle == 4;
            tt["use_style_5"]   = (i) => i.useStyle == 5;

            // use-time
            tt["insanely_fast"]  = (i) => i.useTime <= 8;
            tt["very_fast"]      = (i) => i.useTime > 8  && i.useTime <= 20;
            tt["fast"]           = (i) => i.useTime > 20 && i.useTime <= 25;
            tt["average"]        = (i) => i.useTime > 25 && i.useTime <= 30;
            tt["slow"]           = (i) => i.useTime > 30 && i.useTime <= 35;
            tt["very_slow"]      = (i) => i.useTime > 35 && i.useTime <= 45;
            tt["extremely_slow"] = (i) => i.useTime > 45 && i.useTime <= 55;
            tt["snail"]          = (i) => i.useTime > 55;

            tt["hold_style_any"] = (i) => i.holdStyle > 0;
            tt["hold_style_1"]   = (i) => i.holdStyle == 1; // torches, flaregun, glowsticks, and some other stuff
            tt["hold_style_2"]   = (i) => i.holdStyle == 2; // breathing-reed & umbrella
            tt["hold_style_3"]   = (i) => i.holdStyle == 3; // magical harp

            // rarity
            tt["tier-1"]       = (i) => i.rare == -1;
            tt["gray"]         = (i) => i.rare == -1;

            tt["tier0"]        = (i) => i.rare == 0;
            tt["white"]        = (i) => i.rare == 0;

            tt["tier1"]        = (i) => i.rare == 1;
            tt["blue"]         = (i) => i.rare == 1;

            tt["tier2"]        = (i) => i.rare == 2;
            tt["green"]        = (i) => i.rare == 2;

            tt["tier3"]        = (i) => i.rare == 3;
            tt["orange"]       = (i) => i.rare == 3;

            tt["tier4"]        = (i) => i.rare == 4;
            tt["light_red"]    = (i) => i.rare == 4;

            tt["tier5"]        = (i) => i.rare == 5;
            tt["pink"]         = (i) => i.rare == 5;

            tt["tier6"]        = (i) => i.rare == 6;
            tt["light_purple"] = (i) => i.rare == 6;

            tt["tier7"]        = (i) => i.rare == 7;
            tt["lime"]         = (i) => i.rare == 7;

            tt["tier8"]        = (i) => i.rare == 8;
            tt["yellow"]       = (i) => i.rare == 8;

            tt["tier9"]        = (i) => i.rare == 9;
            tt["cyan"]         = (i) => i.rare == 9;

            tt["tier10"]       = (i) => i.rare == 10;
            tt["red"]          = (i) => i.rare == 10;

            tt["tier11"]       = (i) => i.rare == 11;
            tt["purple"]       = (i) => i.rare == 11;

            tt["tier-11"]      = (i) => i.rare == -11;
            tt["amber"]        = (i) => i.rare == -11;

            tt["tier-12"]      = (i) => i.rare == -12;
            tt["rainbow"]      = (i) => i.rare == -12;

            // tileids
            tt["dirt"] = (i) => i.createTile == TileID.Dirt;
            tt["wood"] = ItemSets.Wood.Contains;

            tt["tile_brick"]    = (i) => contains(Main.tileBrick, i.createTile);
            tt["dungeon_brick"] = (i) => contains(Main.tileDungeon, i.createTile);
            tt["sand"]          = (i) => contains(Main.tileSand, i.createTile);
            tt["bouncy"]        = (i) => contains(Main.tileBouncy, i.createTile);
            tt["rope"]          = (i) => contains(Main.tileRope, i.createTile);

            tt["hardened_sand"] = (i) => contains(TileID.Sets.Conversion.HardenedSand, i.createTile);
            tt["sandstone"]     = (i) => contains(TileID.Sets.Conversion.Sandstone, i.createTile);
            tt["stone"]         = (i) => contains(TileID.Sets.Conversion.Stone, i.createTile);

            tt["ore"] = (i) => contains(TileID.Sets.Ore, i.createTile);
            tt["ice"] = (i) => contains(TileID.Sets.Ices, i.createTile);

            tt["hallow"]  = (i) => contains(TileID.Sets.Hallow, i.createTile);
            tt["corrupt"] = (i) => contains(TileID.Sets.Corrupt, i.createTile);
            tt["crimson"] = (i) => contains(TileID.Sets.Crimson, i.createTile);

            tt["gravity_works"] = (i) => contains(TileID.Sets.Falling, i.createTile);
            tt["lighted"]       = (i) => contains(Main.tileLighted, i.createTile);
            tt["wall_placeable"]       = (i) => contains(TileID.Sets.FramesOnKillWall, i.createTile);

            // furniture-ish

            tt["crafting_station"]  = ItemSets.CraftingStations.Contains;
            tt["housing_furniture"] = ItemSets.Furniture.Contains;
            tt["wall_decoration"]   = ItemSets.WallDecor.Contains;
            tt["container"]         = (i) => i.createTile == TileID.Containers;
            tt["banner"]            = (i) => i.createTile == TileID.Banners;

            // room-needs
            tt["room_needs_door"] = (i) => TileID.Sets.RoomNeeds.CountsAsDoor.Contains(i.createTile);
            tt["door"]            = (i) => i.createTile == TileID.ClosedDoor;
            tt["platform"]        = (i) => i.createTile == TileID.Platforms;

            tt["room_needs_chair"] = (i) => TileID.Sets.RoomNeeds.CountsAsChair.Contains(i.createTile);
            tt["chair"]            = (i) => i.createTile == TileID.Chairs;
            tt["bench"]            = (i) => i.createTile == TileID.Benches;

            tt["room_needs_table"] = (i) => TileID.Sets.RoomNeeds.CountsAsTable.Contains(i.createTile);
            tt["work_bench"]       = (i) => i.createTile == TileID.WorkBenches;
            tt["table"]            = (i) => i.createTile == TileID.Tables;
            tt["piano"]            = (i) => i.createTile == TileID.Pianos;
            tt["dresser"]          = (i) => i.createTile == TileID.Dressers;
            tt["bookcase"]         = (i) => i.createTile == TileID.Bookcases;
            tt["bathtub"]          = (i) => i.createTile == TileID.Bathtubs;

            tt["room_needs_torch"] = (i) => TileID.Sets.RoomNeeds.CountsAsTorch.Contains(i.createTile);
            tt["candle"]           = (i) => i.createTile == TileID.Candles;
            tt["chandelier"]       = (i) => i.createTile == TileID.Chandeliers;
            tt["hanging_lantern"]  = (i) => i.createTile == TileID.HangingLanterns;
            tt["torch"]            = (i) => i.createTile == TileID.Torches;
            tt["lamp"]             = (i) => i.createTile == TileID.Lamps;
            tt["candelabra"]       = (i) => i.createTile == TileID.Candelabras;
            tt["holiday_light"]    = (i) => i.createTile == TileID.HolidayLights;

            // misc furniture
            tt["anvil"]             = (i) => i.createTile == TileID.Anvils;
            tt["cooking_pot"]       = (i) => i.createTile == TileID.CookingPots;
            tt["sink"]              = (i) => i.createTile == TileID.Sinks;
            tt["grandfather_clock"] = (i) => i.createTile == TileID.GrandfatherClocks;
            tt["cannon"]            = (i) => i.createTile == TileID.Cannon;
            tt["campfire"]          = (i) => i.createTile == TileID.Campfire;
            tt["bowl"]              = (i) => i.createTile == TileID.Bowls;
            tt["bottle"]            = (i) => i.createTile == TileID.Bottles;
            tt["beach_pile"]        = (i) => i.createTile == TileID.BeachPiles;

            // mech
            tt["trap"]           = (i) => i.createTile == TileID.Traps;
            tt["pressure_plate"] = (i) => i.createTile == TileID.PressurePlates;
            tt["timer"]          = (i) => i.createTile == TileID.Timers;
            tt["firework"]       = (i) => i.createTile == TileID.Firework;
            tt["music_box"]      = (i) => i.createTile == TileID.MusicBoxes;

            // not-quite-so-furniture-ish
            tt["statue"]          = (i) => i.createTile == TileID.Statues;
            tt["tombstone"]       = (i) => i.createTile == TileID.Tombstones;
            tt["water_fountain"]  = (i) => i.createTile == TileID.WaterFountain;
            tt["alphabet_statue"] = (i) => i.createTile == TileID.AlphabetStatues;
            tt["crate"]           = (i) => i.createTile == TileID.FishingCrate;
            tt["planter"]         = (i) => i.createTile == TileID.PlanterBox;
            tt["monolith"]        = (i) => i.createTile == TileID.LunarMonolith;

            // random placeables
            tt["dye_plant"] = (i) => i.createTile == TileID.DyePlants;
            tt["herb"]      = (i) => i.createTile == TileID.ImmatureHerbs;
            tt["metal_bar"] = (i) => i.createTile == TileID.MetalBars;
            tt["gem"]       = (i) => i.createTile == TileID.ExposedGems;

            // make npc
            tt["critter"]   = (i) => i.makeNPC != 0; // this is redundant w/ create_npc
            tt["butterfly"] = (i) => i.makeNPC == 356;

            // item-type-dependent
            tt["extractinator_valid"]  = (i) => ItemID.Sets.ExtractinatorMode.Contains(i.type);
            tt["metal_detector_valid"] = (item) => item.createTile > 0 && Main.tileValue[item.createTile] > 0;

            // consumables
            tt["food"]             = (i) => i.buffType == BuffID.WellFed;
            tt["flask"]            = (i) => contains(Main.meleeBuff, i.buffType);
            tt["trigger_cooldown"] = (i) => i.potion;

            // equipable
            tt["light_pet"] = (i) => contains(Main.lightPet, i.buffType);
            tt["pet"]       = (i) => contains(Main.vanityPet, i.buffType);
            tt["mount"]     = (i) => i.mountType != -1 && !contains(MountID.Sets.Cart, i.mountType);
            tt["minecart"]  = (i) => contains(MountID.Sets.Cart, i.mountType);

            tt["grapple"]   = (i) => contains(Main.projHook, i.shoot);

            // slots
            tt["head"]      = (i) => i.headSlot > 0;
            tt["body"]      = (i) => i.bodySlot > 0;
            tt["leg"]       = (i) => i.legSlot > 0;
            tt["back"]      = (i) => i.backSlot > 0;
            tt["balloon"]   = (i) => i.balloonSlot > 0;
            tt["face"]      = (i) => i.faceSlot > 0;
            tt["front"]     = (i) => i.frontSlot > 0;
            tt["neck"]      = (i) => i.neckSlot > 0;
            tt["shield"]    = (i) => i.shieldSlot > 0;
            tt["shoe"]      = (i) => i.shoeSlot > 0;
            tt["waist"]     = (i) => i.waistSlot > 0;
            tt["wings"]     = (i) => i.wingSlot > 0;
            tt["main_hand"] = (i) => i.handOnSlot > 0;
            tt["off_hand"]  = (i) => i.handOffSlot > 0;

            // dyes
            tt["basic_dye"]    = ItemSets.BasicDyes.Contains;
            tt["bright_dye"]   = ItemSets.BlackDyes.Contains;
            tt["black_dye"]    = ItemSets.BrightDyes.Contains;
            tt["silver_dye"]   = ItemSets.SilverDyes.Contains;
            tt["flame_dye"]    = ItemSets.GradientDyes.Contains;
            tt["gradient_dye"] = ItemSets.FlameDyes.Contains;
            tt["strange_dye"]  = ItemSets.StrangeDyes.Contains;
            tt["lunar_dye"]    = ItemSets.LunarDyes.Contains;

            // weapon
            tt["crit"]     = (i) => i.crit > 0;
            tt["melee"]    = (i) => i.melee;
            tt["ranged"]   = (i) => i.ranged;
            tt["magic"]    = (i) => i.magic;
            tt["summon"]   = (i) => i.summon;
            tt["throwing"] = (i) => i.thrown;

            // projectile ai
            // ---------------

            // misc
            tt["rope_coile"] = (i) => TestProjectileAI(i, Constants.ProjectileAI.RopeCoil);

            // melee
            tt["boomerang"] = (i) => TestProjectileAI(i, Constants.ProjectileAI.Boomerang);
            tt["chained"]   = (i) => TestProjectileAI(i, Constants.ProjectileAI.Chained);
            tt["spear"]     = (i) => TestProjectileAI(i, Constants.ProjectileAI.Spear);
            tt["flail"]     = (i) => TestProjectileAI(i, Constants.ProjectileAI.Flail);
            tt["yoyo"]      = (i) => TestProjectileAI(i, Constants.ProjectileAI.Yoyo);

            //ranged
            tt["flame_thrower"] = (i) => TestProjectileAI(i, Constants.ProjectileAI.FlameThrower);

            //magic
            tt["area"]         = Weapons.Magic.Area;
            tt["pierce"]       = Weapons.Magic.Pierce; // can be generic
            tt["homing"]       = (i) => contains(ProjectileID.Sets.Homing, i.shoot);
            tt["controllable"] = (i) => TestProjectileAI(i, Constants.ProjectileAI.FollowCursor); // can be generic
            tt["bounce_hyper"] = (i) => TestProjectileAI(i, Constants.ProjectileAI.HyperBounce);
            tt["bounce_heavy"] = (i) => TestProjectileAI(i, Constants.ProjectileAI.HeavyBounce);
            tt["stream"]       = (i) => TestProjectileAI(i, Constants.ProjectileAI.Stream);
            tt["vilethorn_ai"] = (i) => TestProjectileAI(i, Constants.ProjectileAI.Vilethorn);

            // more weapon stuff
            // ------------------

            // melee
            tt["swing"]       = (i) => i.useStyle == 1 && !i.noMelee;
            tt["stab"]        = (i) => i.useStyle == 3;
            tt["directional"] = (i) => i.useStyle == 5;
            tt["throw"]       = (i) => i.useStyle == 1 && i.noMelee;

            //ranged
            tt["use_ammo_arrow"]    = (i) => i.useAmmo == Constants.AmmoID.Arrow;
            tt["use_ammo_bullet"]   = (i) => i.useAmmo == Constants.AmmoID.Bullet;
            tt["use_ammo_coin"]     = (i) => i.useAmmo == Constants.AmmoID.Coin;
            tt["use_ammo_dart"]     = (i) => i.useAmmo == Constants.AmmoID.Dart;
            tt["use_ammo_rocket"]   = (i) => i.useAmmo == Constants.AmmoID.Rocket;
            tt["use_ammo_sand"]     = (i) => i.useAmmo == Constants.AmmoID.Sand;
            tt["use_ammo_solution"] = (i) => i.useAmmo == Constants.AmmoID.Solution;
            tt["use_ammo_stars"]    = (i) => i.useAmmo == Constants.AmmoID.Star;
            tt["use_ammo_gel"]      = (i) => i.useAmmo == Constants.AmmoID.Gel;

            tt["use_ammo_none"]     = (i) => i.useAmmo < 1;

            // summon
            tt["minion"] = Weapons.Summon.Minion;
            // sentry will have to be "!minion"

            // tools
            tt["axe"]    = (i) => i.axe > 0;
            tt["pick"]   = (i) => i.pick > 0;
            tt["hammer"] = (i) => i.hammer > 0;

            // misc tools
            tt["bug_net"]    = MiscTools.BugNet;
            tt["paint_tool"] = MiscTools.PaintingTools;
            tt["terrain"]    = (i) => i.holdStyle == 2;
            tt["recall"]     = (i) => i.useAnimation == 90;
            tt["bucket"]     = MiscTools.Bucket;
            // also includes wire-cutter
            tt["wrench"]     = (i) => i.mech && i.tileBoost == 20;

            // item-ids
            tt["soul"]               = (i) => contains(ItemID.Sets.AnimatesAsSoul, i.type);
            tt["nebula_pickup"]      = (i) => contains(ItemID.Sets.NebulaPickup, i.type);
            tt["alchemy_ingredient"] = ItemSets.AlchemyIngredients.Contains;
            tt["alchemy_result"]     = ItemSets.AlchemyResults.Contains;


            // let's just see
            Console.WriteLine("TraitTable: {0} entries", tt.Count);
        }

        public static bool CheckTrait(Item item, string trait)
        {
            try
            {
                return TraitTable[trait](item);
            }
            catch (KeyNotFoundException knfe)
            {
                throw new UsefulKeyNotFoundException(trait, nameof(TraitTable), knfe,
                    "The trait '{0}' does not exist in '{1}'.");
            }
        }
    }
}
