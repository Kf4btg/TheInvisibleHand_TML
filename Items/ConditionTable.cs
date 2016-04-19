using System.Collections.Generic;
using System;
using Terraria;

namespace InvisibleHand.Items
{
    using static ItemFlags;
    using static ClassificationRules;
    using condition_table = Dictionary<int, Func<Item, bool>>;
    using string_table = Dictionary<string, Func<Item, bool>>;

    internal static class ConditionTable
    {
        private static bool REPLACE_ME(Item item) => true;
        public static readonly Dictionary<ItemFlags.Type, condition_table> RuleMatrix;
        public static readonly Dictionary<string, string_table> StringMatrix;

        static ConditionTable()
        {
            RuleMatrix = new Dictionary<ItemFlags.Type, condition_table>()
            {
                {Type.General, new condition_table
                    {
                        {general.quest_item, (i)    => i.questItem},
                        {general.expert, (i)        => i.expert},
                        {general.material, (i)      => i.material},
                        {general.mech, (i)          => i.mech},
                        {general.channeled, (i)     => i.channel},
                        {general.bait, (i)          => i.bait > 0},
                        {general.reach_boost, (i)   => i.tileBoost > 0},
                        {general.reach_penalty, (i) => i.tileBoost < 0},
                        {general.heal_life, (i)     => i.healLife > 0},
                        {general.regen_life, (i)    => i.lifeRegen > 0},
                        {general.heal_mana, (i)     => i.healMana > 0},
                        {general.boost_mana, (i)    => i.manaIncrease > 0},
                        {general.use_mana, (i)      => i.mana > 0},
                        {general.vanity, (i)        => i.vanity},
                        {general.dye, (i)           => i.dye > 0},
                        {general.equipable, Binary.isEquipable},
                        {general.placeable, Binary.CanBePlaced},
                        {general.consumable, Binary.isConsumable},
                        {general.weapon, Binary.isWeapon},
                        {general.ammo, Binary.isAmmo},
                        {general.soul, Groupings.Soul},
                        {general.hair_dye, (i) => i.hairDye > 0},
                        {general.coin, (i)     => i.ammo == 71},
                        {general.paint, (i)    => i.paint > 0},
                    }
                },
                {Type.Placeable, new condition_table
                    {
                        // {placeable.furniture, REPLACE_ME},
                        {placeable.seed, ByTileID.ImmatureHerb},
                        {placeable.dye_plant, ByTileID.DyePlant},
                        {placeable.strange_plant, Groupings.StrangePlant},
                        // {placeable.block, REPLACE_ME},
                        // {placeable.brick, REPLACE_ME},
                        {placeable.ore, Groupings.Ore},
                        {placeable.bar, ByTileID.MetalBar},
                        // {placeable.wood, REPLACE_ME},
                        {placeable.wall, (i) => i.createWall > 0},
                        {placeable.wall_deco, Groupings.WallDeco},
                        {placeable.gem, ByTileID.Gem},
                        {placeable.musicbox, ByTileID.MusicBox},
                        {placeable.banner, ByTileID.Banner},
                    }
                },
                {Type.Ammo, new condition_table
                    {
                        {ammo.arrow, (i)    => i.ammo == 1},
                        {ammo.bullet, (i)   => i.ammo == 14},
                        {ammo.rocket, (i)   => i.ammo == 771},
                        {ammo.dart, (i)     => i.ammo == 51},
                        {ammo.sand, (i)     => i.ammo == 42},
                        {ammo.coin, (i)     => i.ammo == 71},
                        {ammo.solution, (i) => i.ammo == 780},
                        {ammo.endless, (i)  => i.ammo > 0 && !i.consumable},
                    }
                },
                {Type.Dye, new condition_table
                    {
                        {dye.basic,    Dyes.BasicDyes},
                        {dye.black,    Dyes.BlackDyes},
                        {dye.bright,   Dyes.BrightDyes},
                        {dye.silver,   Dyes.SilverDyes},
                        {dye.flame,    Dyes.FlameDyes},
                        {dye.gradient, Dyes.GradientDyes},
                        {dye.strange,  Dyes.StrangeDyes},
                        {dye.lunar,    Dyes.LunarDyes},
                    }
                },
                {Type.Equip, new condition_table
                    {
                        // {equip.armor, REPLACE_ME},
                        {equip.accessory,    (i) => i.accessory},
                        // {equip.pet, REPLACE_ME},
                        {equip.vanity,       (i) => i.vanity},
                        {equip.mount, Binary.isMount},
                        {equip.grapple, Binary.isHook},
                        {equip.slot_head,    (i) => i.headSlot > 0},
                        {equip.slot_body,    (i) => i.bodySlot > 0},
                        {equip.slot_leg,     (i) => i.legSlot > 0},
                        {equip.slot_face,    (i) => i.faceSlot > 0},
                        {equip.slot_neck,    (i) => i.neckSlot > 0},
                        {equip.slot_back,    (i) => i.backSlot > 0},
                        {equip.wings,        (i) => i.wingSlot > 0},
                        {equip.slot_shoe,    (i) => i.shoeSlot > 0},
                        {equip.slot_handon,  (i) => i.handOnSlot > 0},
                        {equip.slot_handoff, (i) => i.handOffSlot > 0},
                        {equip.slot_shield,  (i) => i.shieldSlot > 0},
                        {equip.slot_waist,   (i) => i.waistSlot > 0},
                        {equip.balloon,      (i) => i.balloonSlot > 0},
                        {equip.slot_front,   (i) => i.frontSlot > 0},
                        {equip.pet_light, Binary.isLightPet},
                        {equip.pet_vanity, Binary.isVanityPet},
                        // {equip.grapple_single, REPLACE_ME},
                        // {equip.grapple_multi, REPLACE_ME},
                        // {equip.mount_cart, REPLACE_ME},
                    }
                },
                {Type.Weapon, new condition_table
                    {
                        {weapon.automatic,      (i) => i.autoReuse},
                        {weapon.type_melee,     (i) => i.melee},
                        {weapon.has_projectile, (i) => i.shoot > 0},
                        {weapon.type_ranged,    (i) => i.ranged},
                        {weapon.type_magic,     (i) => i.magic},
                        {weapon.type_summon,    (i) => i.summon},
                        {weapon.type_throwing,  (i) => i.thrown},
                        // {weapon.weapon_other, REPLACE_ME},
                    }
                },
                {Type.WeaponMelee, new condition_table
                    {
                        {weapon.melee.style_swing, Weapons.Melee.Swing},
                        {weapon.melee.style_jab, Weapons.Melee.Jab},
                        {weapon.melee.style_directional, Weapons.Melee.Directional},
                        {weapon.melee.style_thrown, Weapons.Melee.Thrown},
                        {weapon.melee.broadsword, Weapons.Melee.BroadSword},
                        {weapon.melee.boomerang, Weapons.Melee.Boomerang},
                        {weapon.melee.spear, Weapons.Melee.Spear},
                        {weapon.melee.flail, Weapons.Melee.Flail},
                        {weapon.melee.yoyo, Weapons.Melee.Yoyo},
                    }
                },
                {Type.WeaponRanged, new condition_table
                    {
                        {weapon.ranged.bullet_consuming, Weapons.Ranged.BulletConsuming},
                        {weapon.ranged.arrow_consuming, Weapons.Ranged.ArrowConsuming},
                        {weapon.ranged.rocket_consuming, Weapons.Ranged.RocketConsuming},
                        {weapon.ranged.dart_consuming, Weapons.Ranged.DartConsuming},
                        {weapon.ranged.gel_consuming, Weapons.Ranged.GelConsuming},
                        {weapon.ranged.no_ammo, (i) => i.useAmmo < 0},
                    }
                },
                {Type.WeaponMagic, new condition_table
                    {
                        {weapon.magic.area, Weapons.Magic.Area},
                        {weapon.magic.homing, Weapons.Magic.Homing},
                        {weapon.magic.bouncing, Weapons.Magic.Bouncing},
                        {weapon.magic.controlled,Weapons.Magic.Controllable},
                        {weapon.magic.stream, Weapons.Magic.Stream},
                        {weapon.magic.piercing, Weapons.Magic.Piercing},
                    }
                },
                {Type.WeaponSummon, new condition_table
                    {
                        {weapon.summon.minion, Weapons.Summon.Minion},
                        {weapon.summon.sentry, Weapons.Summon.Sentry},
                    }
                },
                {Type.Tool, new condition_table
                    {
                        {tool.pick,         (i) => i.pick > 0},
                        {tool.axe,          (i) => i.axe > 0},
                        {tool.hammer,       (i) => i.hammer > 0},
                        {tool.fishing_pole, (i) => i.fishingPole > 0},
                        {tool.wand,         (i) => i.tileWand > 0},
                        {tool.wrench, Binary.isWrench},
                        // {tool.recall, REPLACE_ME},
                        // {tool.other, REPLACE_ME},
                    }
                },
                {Type.Consumable, new condition_table
                    {
                        {consumable.buff, Binary.timedBuff},
                        {consumable.food, Binary.isFood},
                        {consumable.potion, Binary.isPotion},
                        // {consumable.heal, REPLACE_ME},
                        // {consumable.regen, REPLACE_ME},
                        // {consumable.life, REPLACE_ME},
                        // {consumable.mana, REPLACE_ME},
                    }
                },
                {Type.Mech, new condition_table
                    {
                        {mech.timer, ByTileID.Timer},
                        // {mech.Switch, ByTileID.L},
                        {mech.trap, ByTileID.Trap},
                        {mech.track, (i) => i.cartTrack},
                        {mech.firework, ByTileID.Firework},
                        // {mech.lever, REPLACE_ME},
                        {mech.pressure_plate, ByTileID.PressurePlate},
                    }
                },
                {Type.Furniture, new condition_table
                    {
                        {furniture.valid_housing, Groupings.Furniture},
                        {furniture.housing_door, Groupings.housingDoor},
                        {furniture.housing_light, Groupings.housingTorch},
                        {furniture.housing_chair, Groupings.housingChair},
                        {furniture.housing_table, Groupings.housingTable},
                        // {furniture.clutter, REPLACE_ME},
                        {furniture.crafting_station, (i) => TileSets.CraftingStations.Contains(i.createTile)},
                        {furniture.container, ByTileID.Container},
                        // {furniture.useable, REPLACE_ME},
                        // {furniture.decorative, REPLACE_ME},
                    }
                },
                {Type.FurnitureDoor, new condition_table
                    {
                        {furniture.doors.door, ByTileID.Door},
                    }
                },
                {Type.FurnitureLight, new condition_table
                    {
                        {furniture.lighting.torch, ByTileID.Torch},
                        {furniture.lighting.candle, ByTileID.Candle},
                        {furniture.lighting.chandelier, ByTileID.Chandelier},
                        {furniture.lighting.hanging_lantern, ByTileID.HangingLantern},
                        {furniture.lighting.lamp, ByTileID.Lamp},
                        {furniture.lighting.holiday_light, ByTileID.HolidayLight},
                        {furniture.lighting.candelabra, ByTileID.Candelabra}
                    }
                },
                {Type.FurnitureTable, new condition_table
                    {
                        {furniture.tables.table, ByTileID.Table},
                        {furniture.tables.workbench, ByTileID.WorkBench},
                        {furniture.tables.dresser, ByTileID.Dresser},
                        {furniture.tables.piano, ByTileID.Piano},
                        {furniture.tables.bookcase, ByTileID.Bookcase},
                        {furniture.tables.bathtub, ByTileID.Bathtub},
                    }
                },
                {Type.FurnitureChair, new condition_table
                    {
                        {furniture.chairs.chair, ByTileID.Chair},
                        {furniture.chairs.bed, ByTileID.Bed},
                        {furniture.chairs.bench, ByTileID.Bench}
                    }
                },
                {Type.FurnitureOther, new condition_table
                    {
                        {furniture.other.sink, ByTileID.Sink},
                        {furniture.other.clock, ByTileID.GrandfatherClock},
                        {furniture.other.bottle, ByTileID.Bottle},
                        {furniture.other.bowl, ByTileID.Bowl},
                        {furniture.other.beachstuff, ByTileID.BeachPile},
                        {furniture.other.tombstone, ByTileID.Tombstone},
                        {furniture.other.campfire, ByTileID.Campfire},
                        {furniture.other.statue, ByTileID.Statue},
                        {furniture.other.statue_alphabet, ByTileID.AlphabetStatue},
                        {furniture.other.crate, ByTileID.FishingCrate},
                        {furniture.other.monolith, ByTileID.LunarMonolith},
                        {furniture.other.cooking_pot, ByTileID.CookingPot},
                        {furniture.other.anvil, ByTileID.Anvil},
                        {furniture.other.cannon, ByTileID.Cannon},
                        {furniture.other.planter, ByTileID.PlanterBox},
                        {furniture.other.fountain, ByTileID.WaterFountain},
                    }
                }
            };

            StringMatrix = new Dictionary<string, string_table>()
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
                        {"soul",          Groupings.Soul},
                        {"hair_dye",      (i) => i.hairDye > 0},
                        {"coin",          (i) => i.ammo == 71},
                        {"paint",         (i) => i.paint > 0},
                    }
                },
                {"Placeable", new string_table
                    {
                        // {"furniture",  REPLACE_ME},
                        {"seed",          ByTileID.ImmatureHerb},
                        {"dye_plant",     ByTileID.DyePlant},
                        {"strange_plant", Groupings.StrangePlant},
                        // {"block",      REPLACE_ME},
                        // {"brick",      REPLACE_ME},
                        {"ore",           Groupings.Ore},
                        {"bar",           ByTileID.MetalBar},
                        // {"wood",       REPLACE_ME},
                        {"wall",          (i) => i.createWall > 0},
                        {"wall_deco",     Groupings.WallDeco},
                        {"gem",           ByTileID.Gem},
                        {"musicbox",      ByTileID.MusicBox},
                        {"banner",        ByTileID.Banner},
                    }
                },
                {"Ammo", new string_table
                    {
                        {"arrow",     (i) => i.ammo == 1},
                        {"bullet",    (i) => i.ammo == 14},
                        {"rocket",    (i) => i.ammo == 771},
                        {"dart",      (i) => i.ammo == 51},
                        {"sand",      (i) => i.ammo == 42},
                        {"coin",      (i) => i.ammo == 71},
                        {"solution",  (i) => i.ammo == 780},
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
                        {"valid_housing",    Groupings.Furniture},
                        {"housing_door",     Groupings.housingDoor},
                        {"housing_light",    Groupings.housingTorch},
                        {"housing_chair",    Groupings.housingChair},
                        {"housing_table",    Groupings.housingTable},
                        // {"clutter",       REPLACE_ME},
                        {"crafting_station", (i) => TileSets.CraftingStations.Contains(i.createTile)},
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
                }
            };
        }

        public static bool Check(ItemFlags.Type table, Item item, int flag)
        {
            return RuleMatrix[table][flag](item);
        }

        public static bool Check(String table, Item item, string flag)
        {
            return StringMatrix[table][flag](item);
        }
    }

}
