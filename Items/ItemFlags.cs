// using System;
// using System.Linq;
// using System.Reflection;
// using System.Collections.Generic;

namespace InvisibleHand.Items
{

    /// Deprecated; Likely to be removed in favor of Loading from hjson definition files
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
            WeaponMelee,
            WeaponRanged,
            WeaponMagic,
            WeaponSummon,
            // Thrown,
            Tool,
            Consumable,
            // Housing,
            Furniture,
            FurnitureDoor,
            FurnitureLight,
            FurnitureTable,
            FurnitureChair,
            FurnitureOther,
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
            public const int none           = 0;
            public const int trap           = 1;
            public const int timer          = 1 << 1;
            public const int Switch         = 1 << 2;
            public const int lever          = 1 << 3;
            public const int pressure_plate = 1 << 4;
            public const int track          = 1 << 5;
            public const int firework       = 1 << 6;

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
            public const int none           = 0;
            public const int automatic      = 1;
            public const int has_projectile = 1 << 1;
            public const int type_melee     = 1 << 2;
            public const int type_ranged    = 1 << 3;
            public const int type_magic     = 1 << 4;

            public const int type_summon    = 1 << 5;

            public const int type_throwing    = 1 << 6;
            public const int type_other     = 1 << 7;

            public static class melee
            {
                public const int none = 0;
                public const int style_swing       = 1;
                public const int style_jab         = 1 << 1;
                public const int style_directional = 1 << 2;
                public const int style_thrown      = 1 << 3;
                public const int broadsword        = 1 << 4;
                public const int boomerang         = 1 << 5;
                public const int spear             = 1 << 6;
                public const int flail             = 1 << 7;
                public const int yoyo              = 1 << 8;

                // public const int shortsword = style_jab;
            }

            public static class ranged
            {
                public const int none = 0;
                public const int bullet_consuming = 1;
                public const int arrow_consuming  = 1 << 1;
                public const int rocket_consuming = 1 << 2;
                public const int dart_consuming   = 1 << 3;
                public const int gel_consuming    = 1 << 4;
                public const int no_ammo          = 1 << 5;
            }

            public static class magic
            {
                public const int none = 0;
                public const int area       = 1;
                public const int homing     = 1 << 1;
                public const int bouncing   = 1 << 2;
                public const int controlled = 1 << 3;
                public const int stream     = 1 << 4;
                public const int piercing   = 1 << 5;
            }

            public static class summon
            {
                public const int none = 0;
                public const int minion     = 1;
                public const int sentry     = 1 << 1;
            }
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

        public static class furniture
        {
            public const int none             = 0;
            public const int valid_housing    = 1;
            public const int clutter          = 1 << 1;
            public const int crafting_station = 1 << 2;
            public const int container        = 1 << 3;
            public const int useable          = 1 << 4;
            public const int decorative = 1 << 5;

            public const int housing_door  = 1 << 6;
            public const int housing_light = 1 << 7;
            public const int housing_chair = 1 << 8;
            public const int housing_table = 1 << 9;

            public static class lighting
            {
                public const int none = 0;
                public const int torch            = 1;
                public const int candle           = 1 << 1;
                public const int chandelier       = 1 << 2;
                public const int hanging_lantern  = 1 << 3;
                public const int lamp             = 1 << 4;
                public const int holiday_light    = 1 << 5;
                public const int candelabra       = 1 << 6;
            }

            public static class chairs
            {
                public const int none = 0;
                public const int chair = 1;
                public const int bed   = 1 << 1;
                public const int bench = 1 << 2;
            }

            public static class doors
            {
                public const int none = 0;
                public const int door = 1;
                public const int other = 1 << 1;
            }

            public static class tables
            {
                public const int none = 0;
                public const int table     = 1;
                public const int workbench = 1 << 1;
                public const int dresser   = 1 << 2;
                public const int piano     = 1 << 3;
                public const int bookcase  = 1 << 4;
                public const int bathtub   = 1 << 5;
            }

            // other
            public static class other
            {
                public const int none = 0;
                public const int sink            = 1;
                public const int clock           = 1 << 1;
                public const int bottle          = 1 << 2;
                public const int bowl            = 1 << 3;
                public const int beachstuff      = 1 << 4;
                public const int tombstone       = 1 << 5;
                public const int campfire        = 1 << 6;
                public const int statue          = 1 << 7;
                public const int statue_alphabet = 1 << 8;
                public const int crate           = 1 << 9;
                public const int monolith        = 1 << 10;
                public const int cooking_pot     = 1 << 11;
                public const int anvil           = 1 << 12;
                public const int cannon          = 1 << 13;
                public const int fountain        = 1 << 14;
                public const int planter         = 1 << 15;
            }
        }

        // public static readonly Dictionary<string, FlagGroup> FlagCollection;
    //     public static readonly Dictionary<string, Dictionary<string, int>> FlagCollection;
    //
    //
    //     /// This initializes the FlagCollection (i.e. a version of the consts above that can be retrieved with strings)
    //     /// using reflection on this class.  TODO: Perhaps it would be better to ditch the hard-coded consts altogether
    //     /// and parse the Traits.hjson file to generate the flag names and values.
    //     static ItemFlags()
    //     {
    //         System.Type thistype = typeof(ItemFlags);
    //         IEnumerable<System.Type> nested = thistype.GetNestedTypes(BindingFlags.Public)
	// 			.Where(type => type != typeof(ItemFlags.Type)); // don't want the enum
    //
    //
    //         // FlagCollection = new Dictionary<string, FlagGroup>();
    //         FlagCollection = new Dictionary<string, Dictionary<string, int>>();
    //         foreach (var nested_type in nested)
    //         {
    //             var flag_group_name = nested_type.Name;
    //             FlagCollection[flag_group_name] = createFlagGroup(nested_type);
    //
    //             // recurse once
    //             foreach (var subtype in nested_type.GetNestedTypes(BindingFlags.Public))
    //             {
    //                 // Flatten the nested types by making the dictionary key "ParentName.ChildName",
    //                 // e.g. "Weapon.Melee". this will greatly simplify referencing things later.
    //                 var combiname = $"{flag_group_name}.{subtype.Name}";
    //                 FlagCollection[combiname] = createFlagGroup(subtype);
    //             }
    //         }
    //     }
    //
    //
    //     private static Dictionary<string, int> createFlagGroup(System.Type nested_type)
    //     {
    //         // var trait_collection_name = nested_type.Name;
    //         // var fg = new FlagGroup(group_name);
    //         var fg = new Dictionary<string, int>();
    //
    //         // always add a none
    //         fg["none"] = 0;
    //         foreach (var flag in nested_type.GetFields().Where(f => f.Name != "none"))
    //         {
    //             // TODO: if I'm defining the consts above, this should probably
    //             // set the flag-values explicitly
    //             // fg.AddFlag(flag.Name);
    //
    //             // each new value added is a (binary) order of magnitude larger than the previous
    //             fg.Add(flag.Name, 1 << (fg.Count-1));
    //         }
    //         return fg;
    //     }
    }

    //
    public enum WallDecoType
    {
        Painting,
        Trophy,
        Rack,
        Other
    }

#region oldstuff
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
    #endregion
}
