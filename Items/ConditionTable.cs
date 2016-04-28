using System.Collections.Generic;
using System;
using Terraria;

namespace InvisibleHand.Items
{
    // using static ItemFlags;
    using static ClassificationRules;
    // using condition_table = Dictionary<int, Func<Item, bool>>;
    using string_table = Dictionary<string, Func<Item, bool>>;

    internal static class ConditionTable
    {
        private static bool REPLACE_ME(Item item) => true;
        // public static readonly Dictionary<ItemFlags.Type, condition_table> RuleMatrix;
        public static readonly Dictionary<string, string_table> StringMatrix = new Dictionary<string, string_table>()
        {
            {"General", new string_table
                {
                    {"quest_item",    (i) => i.questItem},
                    {"expert",        (i) => i.expert},
                    {"material",      (i) => i.material},
                    {"mech",          (i) => i.mech},
                    {"channeled",     (i) => i.channel},
                    {"bait",          (i) => i.bait > 0},
                    {"reach_boost",   (i) => i.tileBoost > 0},
                    {"reach_penalty", (i) => i.tileBoost < 0},
                    {"heal_life",     (i) => i.healLife > 0},
                    {"regen_life",    (i) => i.lifeRegen > 0},
                    {"heal_mana",     (i) => i.healMana > 0},
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
                }
            },
            {"Placeable", new string_table
                {
                    {"lighted",       Binary.givesLight},
                    //~ {"furniture",  REPLACE_ME},
                    {"seed",          ByTileID.ImmatureHerb},
                    //~ {"dye_plant",     ByTileID.DyePlant},
                    {"strange_plant", Sets.StrangePlant},
                    {"block",         Sets.Block},
                    // {"brick",      REPLACE_ME},
                    //~ {"ore",           Sets.Ore},
                    //~ {"bar",           ByTileID.MetalBar},
                    {"wood",          Binary.isWood},
                    {"wall",          (i) => i.createWall > 0},
                    {"wall_deco",     Sets.WallDeco},
                    //~ {"gem",           ByTileID.Gem},
                    {"musicbox",      ByTileID.MusicBox},
                    {"banner",        ByTileID.Banner},
                    {"track",         (i) => i.cartTrack},
                    {"rope",          Sets.Rope},
                    {"rope_coil",     Sets.RopeCoil},
                    {"metal_detector",     Binary.showsOnMetalDetector},
                }
            },
            {"Placeable.Block", new string_table
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

                    {"wood", Binary.isWood},
                }
            },
            {"Ammo", new string_table
                {
                    {"arrow",     (i) => i.ammo == Constants.AmmoID.Arrow},
                    {"bullet",    (i) => i.ammo == Constants.AmmoID.Bullet},
                    {"rocket",    (i) => i.ammo == Constants.AmmoID.Rocket},
                    {"dart",      (i) => i.ammo == Constants.AmmoID.Dart},
                    {"sand",      (i) => i.ammo == 42},
                    {"coin",      (i) => i.ammo == Constants.AmmoID.Coin},
                    {"solution",  (i) => i.ammo == Constants.AmmoID.Solution},
                    {"endless",   (i) => i.ammo > 0 && !i.consumable},
                }
            },
            {"Dye", new string_table
                {
                    {"basic",    Dyes.BasicDyes},
                    {"black",    Dyes.BlackDyes},
                    {"bright",   Dyes.BrightDyes},
                    {"silver",   Dyes.SilverDyes},
                    {"flame",    Dyes.FlameDyes},
                    {"gradient", Dyes.GradientDyes},
                    {"strange",  Dyes.StrangeDyes},
                    {"lunar",    Dyes.LunarDyes},
                }
            },
            {"Equip", new string_table
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
                }
            },
            {"Weapon", new string_table
                {
                    {"automatic",       (i) => i.autoReuse},
                    {"type_melee",      (i) => i.melee},
                    {"has_projectile",  (i) => i.shoot > 0},
                    {"type_ranged",     (i) => i.ranged},
                    {"type_magic",      (i) => i.magic},
                    {"type_summon",     (i) => i.summon},
                    {"type_throwing",   (i) => i.thrown},
                    // {"weapon_other",  REPLACE_ME},
                }
            },
            {"Weapon.Melee", new string_table
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
                }
            },
            {"Weapon.Ranged", new string_table
                {
                    {"bullet_consuming", Weapons.Ranged.BulletConsuming},
                    {"arrow_consuming",  Weapons.Ranged.ArrowConsuming},
                    {"rocket_consuming", Weapons.Ranged.RocketConsuming},
                    {"dart_consuming",   Weapons.Ranged.DartConsuming},
                    {"gel_consuming",    Weapons.Ranged.GelConsuming},
                    {"flamethrower",     Weapons.Ranged.FlameThrower},
                    {"no_ammo",          (i) => i.useAmmo < 0},
                }
            },
            {"Weapon.Magic", new string_table
                {
                    {"area",       Weapons.Magic.Area},
                    {"homing",     Weapons.Magic.Homing},
                    {"bouncing",   Weapons.Magic.Bouncing},
                    {"controlled", Weapons.Magic.Controllable},
                    {"stream",     Weapons.Magic.Stream},
                    {"piercing",   Weapons.Magic.Piercing},
                }
            },
            {"Weapon.Summon", new string_table
                {
                    {"minion", Weapons.Summon.Minion},
                    {"sentry", Weapons.Summon.Sentry},
                }
            },
            {"Tool", new string_table
                {
                    {"pick",         (i) => i.pick > 0},
                    {"axe",          (i) => i.axe > 0},
                    {"hammer",       (i) => i.hammer > 0},
                    {"fishing_pole", (i) => i.fishingPole > 0},
                    {"wand",         (i) => i.tileWand > 0},
                    {"wrench",       Binary.isWrench},
                    // {"recall",  REPLACE_ME},
                    // {"other",  REPLACE_ME},
                }
            },
            {"Consumable", new string_table
                {
                    {"buff",   Binary.timedBuff},
                    {"food",   Binary.isFood},
                    {"potion", Binary.isPotion},
                    {"flask",  Binary.isFlask},
                    // {"heal",  REPLACE_ME},
                    // {"regen",  REPLACE_ME},
                    // {"life",  REPLACE_ME},
                    // {"mana",  REPLACE_ME},
                }
            },
            {"Mech", new string_table
                {
                    {"timer",          ByTileID.Timer},
                    // {mech.Switch, ByTileID.L},
                    {"trap",           ByTileID.Trap},
                    {"track",          (i) => i.cartTrack},
                    {"firework",       ByTileID.Firework},
                    // {"lever",       REPLACE_ME},
                    {"pressure_plate", ByTileID.PressurePlate},
                }
            },
            {"Furniture", new string_table
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
                }
            },
            {"Furniture.Doors", new string_table
                {
                    {"door",  ByTileID.Door},
                }
            },
            {"Furniture.Lighting", new string_table
                {
                    {"torch",           ByTileID.Torch},
                    {"candle",          ByTileID.Candle},
                    {"chandelier",      ByTileID.Chandelier},
                    {"hanging_lantern", ByTileID.HangingLantern},
                    {"lamp",            ByTileID.Lamp},
                    {"holiday_light",   ByTileID.HolidayLight},
                    {"candelabra",      ByTileID.Candelabra}
                }
            },
            {"Furniture.Tables", new string_table
                {
                    {"table",     ByTileID.Table},
                    {"workbench", ByTileID.WorkBench},
                    {"dresser",   ByTileID.Dresser},
                    {"piano",     ByTileID.Piano},
                    {"bookcase",  ByTileID.Bookcase},
                    {"bathtub",   ByTileID.Bathtub},
                }
            },
            {"Furniture.Chairs", new string_table
                {
                    {"chair", ByTileID.Chair},
                    {"bed",   ByTileID.Bed},
                    {"bench", ByTileID.Bench}
                }
            },
            {"Furniture.Other", new string_table
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
                }
            },
            {"Material", new string_table
                {
                    {"ore",           Sets.Ore},
                    {"dye_plant",     ByTileID.DyePlant},
                    {"bar",           ByTileID.MetalBar},
                    {"gem",           ByTileID.Gem},
                    {"alchemy",       Sets.AlchemyIngredient},
                    {"soul",          Sets.Soul},
                    {"critter",       Sets.Critter},
                    {"butterfly",     Sets.Butterfly},
                }

            }

        };

        public static bool Check(String table, Item item, string flag)
        {
            // Console.WriteLine("{0}[{1}]", table, flag);
            return StringMatrix[table][flag](item);
        }
    }

}
