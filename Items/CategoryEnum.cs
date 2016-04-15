// using Microsoft.Xna.Framework;
// using System.Collections.Generic;
using System;
// using Terraria;
// using Terraria.ID;

namespace InvisibleHand.Items
{
    [Flags]
    public enum ItemFamilies
    {
        None = 0x0,
        Tool = 0x1,
        Weapon = 0x2,
        Armor = 0x4,
        Accessory = 0x8,
        Consumable = 0x10,
        Material = 0x20,
    }


    [Flags]
    public enum ToolTypes
    {
        None = 0,
        Pick = 1,
        Axe = 2,
        Hammer = 4,
        Auto = 8,

        Drill = Pick | Auto,
        Chainsaw = Axe | Auto,
        Jackhammer = Hammer | Auto,

        Picksaw = Pick | Axe,
        Hamaxe = Axe | Hammer,

        Drax = Drill | Axe,
        Hamdrill = Drill | Hammer,
        Hamdramaxe = Pick | Axe | Hammer | Auto,

    }

    [Flags]
    public enum WeaponType
    {
        None = 0,

        Melee = 0x1,
        Ranged = 0x2,
        Thrown = 0x4,
        Magic = 0x8,
        Summon = 0x10,
        Other = 0x20
    }

    [Flags]
    public enum MeleeSpecialties
    {
        None = 0,

        Stab = 0x1,
        Swing = 0x2, // useStyle==1
        Throw = 0x4,
        Spin = 0x8,

        Directional = 0x10,
        Follow = 0x20, // attempt to seek out/point at cursor; e.g. chainsaws, yoyos


        // Thrust = 0x10,
        // Yoyo = 0x20,

        // Directional: (useStyle==5) includes Spears, flails, yoyos, Arkhalis, chainsaws, golem fist, & many similar things.

        // Other = 0x40, // e.g. Solar Eruption, (Arkhalis?)
        // // can include solar eruption & arkhalis as Directional | Follow

        Auto = 0x40,

        Shortsword = Stab, // useStyle == 3

        Sword = Swing,
        AutoSwing = Swing | Auto,
        Tool = AutoSwing, // pickaxe, axe, hammer
        MechTool = Directional | Auto | Follow, //  chainsaws, drills, etc.


        Spear = Directional | Stab,
        Flail = Throw | Spin, // (most flails, anyway)
        Boomerang = Throw,
        Yoyo = Throw | Follow,

    }

    [Flags]
    public enum RangedSpecialties
    {
        None = 0,

        Gun = 0x1,
        Bow = 0x2,
        Repeater = 0x4,
        Launcher = 0x8,
        Dart = 0x10,
        Flame = 0x20,
        NoAmmo = 0x40,

        Other = 0x80, // flare gun, star cannon, sandgun, etc.

        Arrows = Bow | Repeater,
        Bullets = Gun,
        Rockets = Launcher,
    }

    [Flags]
    public enum MagicSpecialties
    {
        None = 0,
        Direct = 0x1,
        Area = 0x2,
        Homing = 0x4,
        Bouncing = 0x8,
        Controlled = 0x10,
    }

    [Flags]
    public enum SummonSpecialties
    {
        None = 0,
        Minion = 0x1,
        Sentry = 0x2,
    }

    [Flags]
    public enum ThrownSpecialties
    {
        None = 0,
        Basic = 0x1,
        Explosive = 0x2,
    }

    [Flags]
    public enum OtherWeaponSpecialties
    {
        None = 0,

        Explosive = 0x1,
        Placeable = 0x2,
    }

    public enum WallDecoType
    {
        Painting = 0,
        Trophy = 1,
        Rack = 2,
        Other = 3
    }

    public enum EquipType
    {
        HeadSlot,
        BodySlot,
        LegSlot,
        Accessory,
        Hook,
        Mount,
        LightPet,
        VanityPet
    }






    public enum ValuedAttribute
    {
        //generic
        value,
        useTime,
        rare,
        maxStack,

        // identifying
        createTile,
        createWall,
        makeNPC, // e.g. the 'squirrel' item when you catch one with a bug net
        tileWand, // tile created by wands like the living wood wand or bone wand
        paint,

        headSlot, // armor
        bodySlot, // armor
        legSlot,  // armor

        faceSlot, // accys
        neckSlot, //  |
        backSlot, //  V
        wingSlot,
        shoeSlot,
        handOnSlot,
        handOffSlot,
        shieldSlot,
        waistSlot,
        balloonSlot,
        frontSlot, // cloaks and capes; goes in 'front' of other items

        buffType,
        mountType, // which mount the item summons

        // consumables
        healLife, // amount

        ammo, // set of ammo this item belongs to

        // weapons
        damage,
        knockback, // float
        shoot,
        shootSpeed,
        crit,
        useAmmo, // which set of ammo types it uses
        mana,
        reuseDelay, // last prism, medusa head, clockwork assault rifle have this

        axe, // axepower/5
        pick, // pickpower
        hammer,

        //stuff
        buffTime,
        tileBoost, // increased reach
        bait, // +fishing power
        fishingPole, // +fishing power
        lifeRegen, // only two items have this, and both have it == 1


        // hairDye, // seems like only the Hair Dye Remover has this? and it has a value of 0??
        // Ah, it's determined dynamically in game...



        // only useful for placeables
        width,
        height,
        placeStyle,
        // scale,

        // aesthetic
        glowmask, // things that show up in the dark
        alpha,
        stringColor, // this would be the color...of the string. (yoyo strings)
        useSound,

        // color, // looks like, unlike everything else, this is a Color struct; applies to very few items,
        // mainly glowsticks, but a few other unrelated items; not likely to be useful for sorting.


        //meta
        useStyle,
        useAnimation,
        holdStyle, // 1:swing/throw; 2:Eat/use; 3:Stab; 4:Hold Up; 5:Guns/Staffs

    }

    [Flags]
    public enum BooleanAttributes
    {
        consumable,
        potion,
        cartTrack, // mine, pressure, booster
        useTurn,
        autoReuse, // autoswing
        noWet, //for things like torches that go out in water
        mech,
        expert,
        flame, // only (some) torches and candles have this in Vanilla; not the ones that don't flicker

        questItem, // just the fish in Vanilla
        uniqueStack, // can only have one of these in your inventory; also just the quest fish

        material, // only pressure plates have this attribute for some reason

        accessory,
        vanity,


        // noUseGraphic, //?? don't really know what this is for...

        // weapons
        noMelee, // "weapon sprite does no damage"
                 // like pet-summon items or some projectile-launching weapons
        melee,
        magic,
        ranged, //(includes ammo)
        thrown,
        summon,

        notAmmo, // used for coins & wire to prevent them going in the ammo slots.

        // equipment/armor
        defense,


        // i THINK channel is used for things that either a) seem like they would take mana but don't, or b) maintain an 'active' state without requiring any resources or effort from the player (e.g. the drill/chainsaw VRRRRR noise, or the endlessly-flying yoyos)
        channel
    }


    /// There's too many possible traits (& trait combinations) to use
    /// a [Flags] enum for them (at least, not without making some compromises
    /// and sacrificing expandability), but we CAN use a BitArray to represent
    /// an item's flagged traits in much the same manner. To make things clearer
    /// when referencing a position within the bitarray, it will still behoove
    /// us to define a typical consecutive enum for indexing rather than using
    /// bare ints.
    public enum Trait
    {
        quest_item,
        expert,
        material,
        mech,
        bait,
        explosive,

        auto,
        channeled,

        weapon,
            melee,
                melee_style_swing,
                melee_style_jab,
                melee_style_directional,
                melee_style_thrown,
                shortsword,
                broadsword,
                boomerang,
                spear,
                flail,
                yoyo,
                has_projectile,
            ranged,
                bullet_consuming,
                arrow_consuming,
                rocket_consuming,
                dart_consuming,
                gel_consuming,
                no_ammo,

                gun,
                automatic_gun,
                bow,
                repeater,
                launcher,

            magic,
                //~ direct,
                area,
                homing,
                bouncing,
                controlled,
                stream,
            summon,
                minion,
                sentry,
            thrown,
            weapon_other,

        defense,
        reach_boost,
        reach_penalty,

        heal_life,
        regen_life,
        heal_mana,
        boost_mana,

        use_mana,

        tool,
        pick,
        axe,
        hammer,

        wand,
        fishing_pole,
        wrench,

        accessory,
        vanity,

         // armor slots
        slot_head,
        slot_body,
        slot_leg,

         // accy by slot
        slot_face,
        slot_neck,
        slot_back,
        wings,
        slot_shoe,
        slot_handon,
        slot_handoff,
        slot_shield,
        slot_waist,
        balloon,
        slot_front,

        placeable,
        equipable,
        armor,

        consumable,
        buff,
        food,
        potion,

        pet_light,
        pet_vanity,
        grapple,
            grapple_single,
            grapple_multi,
        mount,
        mount_cart,

        crafting_station,

        housing_furniture,
            housing_door,
                door,

            housing_light,
                torch,
                candle,
                chandelier,
                hanging_lantern,
                lamp,
                holiday_light,
                candelabra,

            housing_chair,
                chair,
                bed,
                bench,

            housing_table,
                table,
                workbench,
                dresser,
                piano,
                bookcase,
                bathtub,

         // other furniture
        container,
        sink,
        clock,
        statue,
        statue_alphabet,
        planter,
        crate,
        monolith,

        cannon,
        campfire,
        fountain,
        tombstone,

         // house clutter
        bottle,
        bowl,
        beachstuff,

         // mech
        track,
        trap,
        timer,
        pressure_plate,

        cooking_pot,
        anvil,

        wall_deco,
        trophy,
        painting,
        rack,

        firework,
        plant_dye,
        plant_seed,
        plant_herb,

        ore,
        bar,
        gem,
        musicbox,

        ammo,
        arrow,
        bullet,
        rocket,
        dart,
        // gel,
        ammo_sand,
        ammo_coin,
        ammo_solution,
        endless,

        coin,
        bomb,

        dye,
        dye_basic,
        dye_black,
        dye_bright,
        dye_silver,
        dye_flame,
        dye_gradient,
        //~ dye_combined,
        dye_strange,
        dye_lunar,

        hair_dye,
        paint,
        craft,
        // furniture,
        banner,
        clutter,
        wood,
        block,
        brick,
        tile,
        wall,
        special,    // boss summoning items, heart containers, mana crystals
        soul,
        other,
        COUNT
    }

    [Flags]
    public enum generalTraits
    {
        none                    = 0,
        expert                  = 1,
        material                = 1 << 2,
        mech                    = 1 << 3,
        bait                    = 1 << 4,
        explosive               = 1 << 5,
        auto                    = 1 << 6,
        channeled               = 1 << 7,
        weapon                  = 1 << 8,
        equipable               = 1 << 9,
        vanity                  = 1 << 10,
        defense                 = 1 << 11,
        reach_boost             = 1 << 12,
        reach_penalty           = 1 << 13,
        heal_life               = 1 << 14,
        regen_life              = 1 << 15,
        heal_mana               = 1 << 16,
        boost_mana              = 1 << 17,
        use_mana                = 1 << 18,
        tool                    = 1 << 19,
        placeable               = 1 << 20,
        consumable              = 1 << 21,
        quest_item              = 1 << 1,
    }

    [Flags]
    public enum placeabletype
    {
        none     = 0,
        furniture = 1,
        seed = 1 << 1,
        block = 1 << 2,
        brick = 1 << 3,
        ore = 1 << 4,
        bar = 1 << 5,
        wood = 1 << 6,
        wall = 1 << 7,
        wall_deco = 1 << 8,
    }

    [Flags]
    public enum ammotype
    {
        none     = 0,
        arrow    = 1,
        bullet   = 1 << 1,
        rocket   = 1 << 2,
        dart     = 1 << 3,
        sand     = 1 << 4,
        coin     = 1 << 5,
        solution = 1 << 6,

        endless  = 1 << 7,

        bomb     = 1 << 8,
        // gel,
    }

    [Flags]
    public enum dyetype
    {
        none         = 0,
        dye_basic    = 1,
        dye_black    = 1 << 1,
        dye_bright   = 1 << 2,
        dye_silver   = 1 << 3,
        dye_flame    = 1 << 4,
        dye_gradient = 1 << 5,
        dye_strange  = 1 << 6,
        dye_lunar    = 1 << 7,
    }

    [Flags]
    public enum equiptype
    {
        none           = 0,
        armor          = 1,
        accessory      = 1 << 2,
        vanity         = 1 << 3,
        pet            = 1 << 4,
        mount          = 1 << 5,
        grapple        = 1 << 6,

        // armor slots
        slot_head      = 1 << 7,
        slot_body      = 1 << 8,
        slot_leg       = 1 << 9,

        // accy by slot

        slot_face      = 1 << 10,
        slot_neck      = 1 << 11,
        slot_back      = 1 << 12,
        wings          = 1 << 13,
        slot_shoe      = 1 << 14,
        slot_handon    = 1 << 15,
        slot_handoff   = 1 << 16,
        slot_shield    = 1 << 17,
        slot_waist     = 1 << 18,
        balloon        = 1 << 19,
        slot_front     = 1 << 20,

        pet_light      = 1 << 21,
        pet_vanity     = 1 << 22,
        grapple_single = 1 << 23,
        grapple_multi  = 1 << 24,
        mount_cart     = 1 << 1,
    }

    [Flags]
    public enum weaponflags
    {
        none = 0,
        automatic = 1,

        melee                 = 1 << 1,
            style_swing       = 1 << 2,
            style_jab         = 1 << 3,
            style_directional = 1 << 4,
            style_thrown      = 1 << 5,
            broadsword        = 1 << 7,
            boomerang         = 1 << 8,
            spear             = 1 << 9,
            flail             = 1 << 10,
            yoyo              = 1 << 11,
            has_projectile    = 1 << 12,

            shortsword = melee | style_jab,

        ranged               = 1 << 13,
            bullet_consuming = 1 << 14,
            arrow_consuming  = 1 << 15,
            rocket_consuming = 1 << 16,
            dart_consuming   = 1 << 18,
            gel_consuming    = 1 << 19,
            no_ammo          = 1 << 20,

            gun           = ranged | bullet_consuming,
            automatic_gun = gun | automatic,
            bow           = ranged | arrow_consuming,
            repeater      = bow | automatic,
            launcher      = ranged | rocket_consuming,

        magic          = 1 << 26,
            area       = 1 << 27,
            homing     = 1 << 28,
            bouncing   = 1 << 29,
            controlled = 1 << 30,
            stream     = 1 << 31,
            piercing   = 1 << 23,

        summon         = 1 << 32,
            minion     = 1 << 24,
            sentry     = 1 << 22,

        throwing       = 1 << 21,
        weapon_other   = 1 << 25,
    }

    [Flags]
    public enum toolflags
    {
        none         = 0,
        pick         = 1,
        axe          = 1 << 1,
        hammer       = 1 << 2,
        fishing_pole = 1 << 3,
        wrench       = 1 << 4,
        wand         = 1 << 5,
        recall       = 1 << 6,
        other        = 1 << 7,
    }

    [Flags]
    public enum consumabletype
    {
        none   = 0,
        buff   = 1,
        food   = 1 << 1,
        potion = 1 << 2,
        heal   = 1 << 3,
        regen  = 1 << 4,
        life   = 1 << 5,
        mana   = 1 << 6,
    }

    public enum housing
    {
        door,
        light,
        chair,
        table,
    }

    [Flags]
    public enum furniture
    {
        none                      = 0,
        valid_housing,            = 1,
        clutter,                  = 1 << 1,
        crafting_station,         = 1 << 2,
        container,                = 1 << 3,
        useable,                  = 1 << 4,
        decorative,               = 1 << 5,

        // housing_furniture,
            // housing_door,
                door,             = 1 << 6,


            // housing_light,
                torch,            = 1 << 7,
                candle,           = 1 << 8,
                chandelier,       = 1 << 9,
                hanging_lantern,  = 1 << 10,
                lamp,             = 1 << 11,
                holiday_light,    = 1 << 12,
                candelabra,       = 1 << 13,

            // housing_chair,
                chair,            = 1 << 15,
                bed,              = 1 << 16,
                bench,            = 1 << 17,

            // housing_table,
                table,            = 1 << 18,
                workbench,        = 1 << 19,
                dresser,          = 1 << 20,
                piano,            = 1 << 21,
                bookcase,         = 1 << 22,
                bathtub,          = 1 << 23,

        // other
        sink,                     = 1 << 24,
        clock,                    = 1 << 25,
        bottle,                   = 1 << 26,
        bowl,                     = 1 << 27,
        beachstuff,               = 1 << 28,
        tombstone,                = 1 << 29,
        campfire,                 = 1 << 30,
        statue,                   = 1 << 31,
        statue_alphabet,          = 1 << 32,
        crate,                    = 1 << 33,
        monolith,                 = 1 << 34,
        cooking_pot,              = 1 << 35,
        anvil,                    = 1 << 36,
        cannon,                   = 1 << 37,
        planter,                  = 1 << 14,
    }


}
