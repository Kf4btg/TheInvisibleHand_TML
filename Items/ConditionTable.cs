using System.Collections.Generic;
using System;
using Terraria;

namespace InvisibleHand.Items
{
    using static ClassificationRules;
    using condition_table = Dictionary<string, Func<Item, bool>>;

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
            {"bucket",       NameMatch.Bucket},
            {"recall",       MiscTools.Recall},
            {"bug_net",       MiscTools.BugNet},
            {"overhead",       MiscTools.OverheadThings},
            {"hand_light",       MiscTools.HandLights},
            {"painting",     MiscTools.PaintingTools},
            // {"recall",  REPLACE_ME},
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
        public static readonly condition_table Supplies = new condition_table
        {
            {"rope",          Sets.Rope},
            {"rope_coil",     Sets.RopeCoil},
            {"overhead",       MiscTools.OverheadThings},
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
                case "Consumable": return Consumable;
                case "Mech": return Mech;
                case "Furniture": return Furniture;
                case "Furniture.Doors": return Furniture_Doors;
                case "Furniture.Lighting": return Furniture_Lighting;
                case "Furniture.Tables": return Furniture_Tables;
                case "Furniture.Chairs": return Furniture_Chairs;
                case "Furniture.Other": return Furniture_Other;
                case "Material": return Material;
                case "Supplies": return Supplies;
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
    }

}
