// using Microsoft.Xna.Framework;
// using System.Collections.Generic;
// using System;
// using Terraria;
// using Terraria.ID;

namespace InvisibleHand.Items
{
    internal static class ItemFlags
    {
        public enum Type
        {
            General,
            Placeable,
            Ammo,
            Dye,
            Equip,
            Weapon,
            Tool,
            Consumable,
            Housing,
            Furniture,
            Mech
        }

        public static class general
        {
            public const int none          = 0;
            public const int expert        = 1;
            public const int quest_item    = 1 << 1;
            public const int material      = 1 << 2;
            public const int mech          = 1 << 3;
            public const int bait          = 1 << 4;
            public const int explosive     = 1 << 5;
            public const int auto          = 1 << 6;
            public const int channeled     = 1 << 7;
            public const int weapon        = 1 << 8;
            public const int equipable     = 1 << 9;
            public const int vanity        = 1 << 10;
            public const int defense       = 1 << 11;
            public const int reach_boost   = 1 << 12;
            public const int reach_penalty = 1 << 13;
            public const int heal_life     = 1 << 14;
            public const int regen_life    = 1 << 15;
            public const int heal_mana     = 1 << 16;
            public const int boost_mana    = 1 << 17;
            public const int use_mana      = 1 << 18;
            public const int tool          = 1 << 19;
            public const int placeable     = 1 << 20;
            public const int consumable    = 1 << 21;
            public const int dye           = 1 << 22;
            public const int ammo          = 1 << 23;
            public const int soul          = 1 << 24;
            public const int coin          = 1 << 25;
            public const int paint         = 1 << 26;
            public const int hair_dye      = 1 << 27;
        }

        public static class placeable
        {
            public const int none          = 0;
            public const int furniture     = 1;
            public const int seed          = 1 << 1;
            public const int block         = 1 << 2;
            public const int brick         = 1 << 3;
            public const int ore           = 1 << 4;
            public const int bar           = 1 << 5;
            public const int wood          = 1 << 6;
            public const int wall          = 1 << 7;
            public const int wall_deco     = 1 << 8;
            public const int dye_plant     = 1 << 9;
            public const int strange_plant = 1 << 10;
            public const int gem           = 1 << 11;
            public const int musicbox      = 1 << 12;
            public const int painting      = 1 << 13;
            public const int rack          = 1 << 14;
            public const int trophy        = 1 << 15;
            public const int banner        = 1 << 16;
            public const int wallpaper     = 1 << 17;
        }

        public static class mech
        {
            public const int none = 0;
            public const int trap = 1;
            public const int timer = 1 << 1;
            public const int Switch = 1 << 2;
            public const int lever = 1 << 3;
            public const int pressure_plate = 1 << 4;
            public const int track = 1 << 5;
            public const int firework = 1 << 6;

            // TODO: wire? teleporter?

            // public const int wire = 1 << 5;
        }

        public static class ammo
        {
            public const int none     = 0;
            public const int arrow    = 1;
            public const int bullet   = 1 << 1;
            public const int rocket   = 1 << 2;
            public const int dart     = 1 << 3;
            public const int sand     = 1 << 4;
            public const int coin     = 1 << 5;
            public const int solution = 1 << 6;

            public const int endless  = 1 << 7;

            public const int bomb     = 1 << 8;
            // gel,
        }

        public static class dye
        {
            public const int none     = 0;
            public const int basic    = 1;
            public const int black    = 1 << 1;
            public const int bright   = 1 << 2;
            public const int silver   = 1 << 3;
            public const int flame    = 1 << 4;
            public const int gradient = 1 << 5;
            public const int strange  = 1 << 6;
            public const int lunar    = 1 << 7;
        }

        public static class equip
        {
            public const int none           = 0;
            public const int armor          = 1;
            public const int accessory      = 1 << 2;
            public const int vanity         = 1 << 3;
            public const int pet            = 1 << 4;
            public const int mount          = 1 << 5;
            public const int grapple        = 1 << 6;

            // armor slots
            public const int slot_head      = 1 << 7;
            public const int slot_body      = 1 << 8;
            public const int slot_leg       = 1 << 9;

            // accy by slot

            public const int slot_face      = 1 << 10;
            public const int slot_neck      = 1 << 11;
            public const int slot_back      = 1 << 12;
            public const int wings          = 1 << 13;
            public const int slot_shoe      = 1 << 14;
            public const int slot_handon    = 1 << 15;
            public const int slot_handoff   = 1 << 16;
            public const int slot_shield    = 1 << 17;
            public const int slot_waist     = 1 << 18;
            public const int balloon        = 1 << 19;
            public const int slot_front     = 1 << 20;

            public const int pet_light      = 1 << 21;
            public const int pet_vanity     = 1 << 22;
            public const int grapple_single = 1 << 23;
            public const int grapple_multi  = 1 << 24;
            public const int mount_cart     = 1 << 1;
        }

        public static class weapon
        {
            public const long none           = 0;
            public const long automatic      = 1;
            public const long has_projectile = 1 << 12;

            public const long melee = 1 << 1;

                public const long style_swing       = 1 << 2;
                public const long style_jab         = 1 << 3;
                public const long style_directional = 1 << 4;
                public const long style_thrown      = 1 << 5;
                public const long broadsword        = 1 << 7;
                public const long boomerang         = 1 << 8;
                public const long spear             = 1 << 9;
                public const long flail             = 1 << 10;
                public const long yoyo              = 1 << 11;

                public const long shortsword = melee | style_jab;

            public const long ranged               = 1 << 13;
                public const long bullet_consuming = 1 << 14;
                public const long arrow_consuming  = 1 << 15;
                public const long rocket_consuming = 1 << 16;
                public const long dart_consuming   = 1 << 18;
                public const long gel_consuming    = 1 << 19;
                public const long no_ammo          = 1 << 20;

                public const long gun           = ranged | bullet_consuming;
                public const long automatic_gun = gun | automatic;
                public const long bow           = ranged | arrow_consuming;
                public const long repeater      = bow | automatic;
                public const long launcher      = ranged | rocket_consuming;

            public const long magic          = 1 << 26;
                public const long area       = 1 << 27;
                public const long homing     = 1 << 28;
                public const long bouncing   = 1 << 29;
                public const long controlled = 1 << 30;
                public const long stream     = 1L << 31;
                public const long piercing   = 1 << 23;

            public const long summon         = 1L << 32;
                public const long minion     = 1 << 24;
                public const long sentry     = 1 << 22;

            public const long throwing       = 1 << 21;
            public const long weapon_other   = 1 << 25;
        }

        public static class tool
        {
            public const int none         = 0;
            public const int pick         = 1;
            public const int axe          = 1 << 1;
            public const int hammer       = 1 << 2;
            public const int fishing_pole = 1 << 3;
            public const int wrench       = 1 << 4;
            public const int wand         = 1 << 5;
            public const int recall       = 1 << 6;
            public const int other        = 1 << 7;
        }

        public static class consumable
        {
            public const int none   = 0;
            public const int buff   = 1;
            public const int food   = 1 << 1;
            public const int potion = 1 << 2;
            public const int heal   = 1 << 3;
            public const int regen  = 1 << 4;
            public const int life   = 1 << 5;
            public const int mana   = 1 << 6;
        }

        public static class housing
        {
            public const int none  = 0;
            public const int door  = 1;
            public const int light = 1 << 1;
            public const int chair = 1 << 2;
            public const int table = 1 << 3;
        }

        public static class furniture
        {
            public const long none             = 0;
            public const long valid_housing    = 1;
            public const long clutter          = 1 << 1;
            public const long crafting_station = 1 << 2;
            public const long container        = 1 << 3;
            public const long useable          = 1 << 4;
            public const long decorative       = 1 << 5;

            // housing_furniture,
            // housing_door,
            public const long door             = 1 << 6;

            // housing_light,
            public const long torch            = 1 << 7;
            public const long candle           = 1 << 8;
            public const long chandelier       = 1 << 9;
            public const long hanging_lantern  = 1 << 10;
            public const long lamp             = 1 << 11;
            public const long holiday_light    = 1 << 12;
            public const long candelabra       = 1 << 13;

            // housing_chair,
            public const long chair = 1 << 15;
            public const long bed   = 1 << 16;
            public const long bench = 1 << 17;

            // housing_table,
            public const long table     = 1 << 18;
            public const long workbench = 1 << 19;
            public const long dresser   = 1 << 20;
            public const long piano     = 1 << 21;
            public const long bookcase  = 1 << 22;
            public const long bathtub   = 1 << 23;

            // other
            public const long sink            = 1 << 24;
            public const long clock           = 1 << 25;
            public const long bottle          = 1 << 26;
            public const long bowl            = 1 << 27;
            public const long beachstuff      = 1 << 28;
            public const long tombstone       = 1 << 29;
            public const long campfire        = 1 << 30;
            public const long statue          = 1L << 31;
            public const long statue_alphabet = 1L << 32;
            public const long crate           = 1L << 33;
            public const long monolith        = 1L << 34;
            public const long cooking_pot     = 1L << 35;
            public const long anvil           = 1L << 36;
            public const long cannon          = 1L << 37;
            public const long fountain        = 1L << 38;
            public const long planter         = 1 << 14;
        }
    }

    //
    public enum WallDecoType
    {
        Painting,
        Trophy,
        Rack,
        Other
    }

    //
    // [Flags]
    // public enum ItemFamilies
    // {
    //     None = 0x0,
    //     Tool = 0x1,
    //     Weapon = 0x2,
    //     Armor = 0x4,
    //     Accessory = 0x8,
    //     Consumable = 0x10,
    //     Material = 0x20,
    // }
    //
    //
    // [Flags]
    // public enum ToolTypes
    // {
    //     None = 0,
    //     Pick = 1,
    //     Axe = 2,
    //     Hammer = 4,
    //     Auto = 8,
    //
    //     Drill = Pick | Auto,
    //     Chainsaw = Axe | Auto,
    //     Jackhammer = Hammer | Auto,
    //
    //     Picksaw = Pick | Axe,
    //     Hamaxe = Axe | Hammer,
    //
    //     Drax = Drill | Axe,
    //     Hamdrill = Drill | Hammer,
    //     Hamdramaxe = Pick | Axe | Hammer | Auto,
    //
    // }
    //
    // [Flags]
    // public enum WeaponType
    // {
    //     None = 0,
    //
    //     Melee = 0x1,
    //     Ranged = 0x2,
    //     Thrown = 0x4,
    //     Magic = 0x8,
    //     Summon = 0x10,
    //     Other = 0x20
    // }
    //
    // [Flags]
    // public enum MeleeSpecialties
    // {
    //     None = 0,
    //
    //     Stab = 0x1,
    //     Swing = 0x2, // useStyle==1
    //     Throw = 0x4,
    //     Spin = 0x8,
    //
    //     Directional = 0x10,
    //     Follow = 0x20, // attempt to seek out/point at cursor; e.g. chainsaws, yoyos
    //
    //
    //     // Thrust = 0x10,
    //     // Yoyo = 0x20,
    //
    //     // Directional: (useStyle==5) includes Spears, flails, yoyos, Arkhalis, chainsaws, golem fist, & many similar things.
    //
    //     // Other = 0x40, // e.g. Solar Eruption, (Arkhalis?)
    //     // // can include solar eruption & arkhalis as Directional | Follow
    //
    //     Auto = 0x40,
    //
    //     Shortsword = Stab, // useStyle == 3
    //
    //     Sword = Swing,
    //     AutoSwing = Swing | Auto,
    //     Tool = AutoSwing, // pickaxe, axe, hammer
    //     MechTool = Directional | Auto | Follow, //  chainsaws, drills, etc.
    //
    //
    //     Spear = Directional | Stab,
    //     Flail = Throw | Spin, // (most flails, anyway)
    //     Boomerang = Throw,
    //     Yoyo = Throw | Follow,
    //
    // }
    //
    // [Flags]
    // public enum RangedSpecialties
    // {
    //     None = 0,
    //
    //     Gun = 0x1,
    //     Bow = 0x2,
    //     Repeater = 0x4,
    //     Launcher = 0x8,
    //     Dart = 0x10,
    //     Flame = 0x20,
    //     NoAmmo = 0x40,
    //
    //     Other = 0x80, // flare gun, star cannon, sandgun, etc.
    //
    //     Arrows = Bow | Repeater,
    //     Bullets = Gun,
    //     Rockets = Launcher,
    // }
    //
    // [Flags]
    // public enum MagicSpecialties
    // {
    //     None = 0,
    //     Direct = 0x1,
    //     Area = 0x2,
    //     Homing = 0x4,
    //     Bouncing = 0x8,
    //     Controlled = 0x10,
    // }
    //
    // [Flags]
    // public enum SummonSpecialties
    // {
    //     None = 0,
    //     Minion = 0x1,
    //     Sentry = 0x2,
    // }
    //
    // [Flags]
    // public enum ThrownSpecialties
    // {
    //     None = 0,
    //     Basic = 0x1,
    //     Explosive = 0x2,
    // }
    //
    // [Flags]
    // public enum OtherWeaponSpecialties
    // {
    //     None = 0,
    //
    //     Explosive = 0x1,
    //     Placeable = 0x2,
    // }

    //
    // public enum EquipType
    // {
    //     HeadSlot,
    //     BodySlot,
    //     LegSlot,
    //     Accessory,
    //     Hook,
    //     Mount,
    //     LightPet,
    //     VanityPet
    // }
    //
    //
    //
    //
    //
    //
    // public enum ValuedAttribute
    // {
    //     //generic
    //     value,
    //     useTime,
    //     rare,
    //     maxStack,
    //
    //     // identifying
    //     createTile,
    //     createWall,
    //     makeNPC, // e.g. the 'squirrel' item when you catch one with a bug net
    //     tileWand, // tile created by wands like the living wood wand or bone wand
    //     paint,
    //
    //     headSlot, // armor
    //     bodySlot, // armor
    //     legSlot,  // armor
    //
    //     faceSlot, // accys
    //     neckSlot, //  |
    //     backSlot, //  V
    //     wingSlot,
    //     shoeSlot,
    //     handOnSlot,
    //     handOffSlot,
    //     shieldSlot,
    //     waistSlot,
    //     balloonSlot,
    //     frontSlot, // cloaks and capes; goes in 'front' of other items
    //
    //     buffType,
    //     mountType, // which mount the item summons
    //
    //     // consumables
    //     healLife, // amount
    //
    //     ammo, // set of ammo this item belongs to
    //
    //     // weapons
    //     damage,
    //     knockback, // float
    //     shoot,
    //     shootSpeed,
    //     crit,
    //     useAmmo, // which set of ammo types it uses
    //     mana,
    //     reuseDelay, // last prism, medusa head, clockwork assault rifle have this
    //
    //     axe, // axepower/5
    //     pick, // pickpower
    //     hammer,
    //
    //     //stuff
    //     buffTime,
    //     tileBoost, // increased reach
    //     bait, // +fishing power
    //     fishingPole, // +fishing power
    //     lifeRegen, // only two items have this, and both have it == 1
    //
    //
    //     // hairDye, // seems like only the Hair Dye Remover has this? and it has a value of 0??
    //     // Ah, it's determined dynamically in game...
    //
    //
    //
    //     // only useful for placeables
    //     width,
    //     height,
    //     placeStyle,
    //     // scale,
    //
    //     // aesthetic
    //     glowmask, // things that show up in the dark
    //     alpha,
    //     stringColor, // this would be the color...of the string. (yoyo strings)
    //     useSound,
    //
    //     // color, // looks like, unlike everything else, this is a Color struct; applies to very few items,
    //     // mainly glowsticks, but a few other unrelated items; not likely to be useful for sorting.
    //
    //
    //     //meta
    //     useStyle,
    //     useAnimation,
    //     holdStyle, // 1:swing/throw; 2:Eat/use; 3:Stab; 4:Hold Up; 5:Guns/Staffs
    //
    // }
    //
    // [Flags]
    // public enum BooleanAttributes
    // {
    //     consumable,
    //     potion,
    //     cartTrack, // mine, pressure, booster
    //     useTurn,
    //     autoReuse, // autoswing
    //     noWet, //for things like torches that go out in water
    //     mech,
    //     expert,
    //     flame, // only (some) torches and candles have this in Vanilla; not the ones that don't flicker
    //
    //     questItem, // just the fish in Vanilla
    //     uniqueStack, // can only have one of these in your inventory; also just the quest fish
    //
    //     material, // only pressure plates have this attribute for some reason
    //
    //     accessory,
    //     vanity,
    //
    //
    //     // noUseGraphic, //?? don't really know what this is for...
    //
    //     // weapons
    //     noMelee, // "weapon sprite does no damage"
    //              // like pet-summon items or some projectile-launching weapons
    //     melee,
    //     magic,
    //     ranged, //(includes ammo)
    //     thrown,
    //     summon,
    //
    //     notAmmo, // used for coins & wire to prevent them going in the ammo slots.
    //
    //     // equipment/armor
    //     defense,
    //
    //
    //     // i THINK channel is used for things that either a) seem like they would take mana but don't, or b) maintain an 'active' state without requiring any resources or effort from the player (e.g. the drill/chainsaw VRRRRR noise, or the endlessly-flying yoyos)
    //     channel
    // }
    //
    //
    // /// There's too many possible traits (& trait combinations) to use
    // /// a [Flags] enum for them (at least, not without making some compromises
    // /// and sacrificing expandability), but we CAN use a BitArray to represent
    // /// an item's flagged traits in much the same manner. To make things clearer
    // /// when referencing a position within the bitarray, it will still behoove
    // /// us to define a typical consecutive enum for indexing rather than using
    // /// bare ints.
    // public enum Trait
    // {
    //     quest_item,
    //     expert,
    //     material,
    //     mech,
    //     bait,
    //     explosive,
    //
    //     auto,
    //     channeled,
    //
    //     weapon,
    //         melee,
    //             melee_style_swing,
    //             melee_style_jab,
    //             melee_style_directional,
    //             melee_style_thrown,
    //             shortsword,
    //             broadsword,
    //             boomerang,
    //             spear,
    //             flail,
    //             yoyo,
    //             has_projectile,
    //         ranged,
    //             bullet_consuming,
    //             arrow_consuming,
    //             rocket_consuming,
    //             dart_consuming,
    //             gel_consuming,
    //             no_ammo,
    //
    //             gun,
    //             automatic_gun,
    //             bow,
    //             repeater,
    //             launcher,
    //
    //         magic,
    //             //~ direct,
    //             area,
    //             homing,
    //             bouncing,
    //             controlled,
    //             stream,
    //         summon,
    //             minion,
    //             sentry,
    //         thrown,
    //         weapon_other,
    //
    //     defense,
    //     reach_boost,
    //     reach_penalty,
    //
    //     heal_life,
    //     regen_life,
    //     heal_mana,
    //     boost_mana,
    //
    //     use_mana,
    //
    //     tool,
    //     pick,
    //     axe,
    //     hammer,
    //
    //     wand,
    //     fishing_pole,
    //     wrench,
    //
    //     accessory,
    //     vanity,
    //
    //      // armor slots
    //     slot_head,
    //     slot_body,
    //     slot_leg,
    //
    //      // accy by slot
    //     slot_face,
    //     slot_neck,
    //     slot_back,
    //     wings,
    //     slot_shoe,
    //     slot_handon,
    //     slot_handoff,
    //     slot_shield,
    //     slot_waist,
    //     balloon,
    //     slot_front,
    //
    //     placeable,
    //     equipable,
    //     armor,
    //
    //     consumable,
    //     buff,
    //     food,
    //     potion,
    //
    //     pet_light,
    //     pet_vanity,
    //     grapple,
    //         grapple_single,
    //         grapple_multi,
    //     mount,
    //     mount_cart,
    //
    //     crafting_station,
    //
    //     housing_furniture,
    //         housing_door,
    //             door,
    //
    //         housing_light,
    //             torch,
    //             candle,
    //             chandelier,
    //             hanging_lantern,
    //             lamp,
    //             holiday_light,
    //             candelabra,
    //
    //         housing_chair,
    //             chair,
    //             bed,
    //             bench,
    //
    //         housing_table,
    //             table,
    //             workbench,
    //             dresser,
    //             piano,
    //             bookcase,
    //             bathtub,
    //
    //      // other furniture
    //     container,
    //     sink,
    //     clock,
    //     statue,
    //     statue_alphabet,
    //     planter,
    //     crate,
    //     monolith,
    //
    //     cannon,
    //     campfire,
    //     fountain,
    //     tombstone,
    //
    //      // house clutter
    //     bottle,
    //     bowl,
    //     beachstuff,
    //
    //      // mech
    //     track,
    //     trap,
    //     timer,
    //     pressure_plate,
    //
    //     cooking_pot,
    //     anvil,
    //
    //     wall_deco,
    //     trophy,
    //     painting,
    //     rack,
    //
    //     firework,
    //     plant_dye,
    //     plant_seed,
    //     plant_herb,
    //
    //     ore,
    //     bar,
    //     gem,
    //     musicbox,
    //
    //     ammo,
    //     arrow,
    //     bullet,
    //     rocket,
    //     dart,
    //     // gel,
    //     ammo_sand,
    //     ammo_coin,
    //     ammo_solution,
    //     endless,
    //
    //     coin,
    //     bomb,
    //
    //     dye,
    //     dye_basic,
    //     dye_black,
    //     dye_bright,
    //     dye_silver,
    //     dye_flame,
    //     dye_gradient,
    //     //~ dye_combined,
    //     dye_strange,
    //     dye_lunar,
    //
    //     hair_dye,
    //     paint,
    //     craft,
    //     // furniture,
    //     banner,
    //     clutter,
    //     wood,
    //     block,
    //     brick,
    //     tile,
    //     wall,
    //     special,    // boss summoning items, heart containers, mana crystals
    //     soul,
    //     other,
    //     COUNT
    // }
    //

}
