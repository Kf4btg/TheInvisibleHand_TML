// using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
// using TAPI;
// using Terraria.ID;

namespace InvisibleHand.Items
{
    [Flags]
    public enum ItemTypes
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
    public enum ToolSpecialties
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
    public enum WeaponSpecialties
    {
        None = 0,

        Melee = 0x1,
        Ranged = 0x2,
        Thrown = 0x4,
        Magic = 0x8,
        Summon = 0x10,
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


    public enum Trait
    {
        placeable,
        auto,
        weapon,
        tool,
        //WEAPONS
            //MELEE
                //TOOLS,
                PICK,
                AXE,
                HAMMER,
            MELEE,
            BOOMERANG,
            // THROWN
            THROWN,

            // RANGED
        RANGED,

        // MAGIC
        MAGIC,

            //SUMMON
            SUMMON,

        COIN,
        TOOL,
        MECH,
        BOMB,
        AMMO,
        PET,
        HEAD,
        BODY,
        LEGS,
        ACCESSORY,
        VANITY,
        POTION,
        CONSUME,
        BAIT,
        DYE,
        PAINT,
        ORE,
        BAR,
        GEM,
        SEED,
        LIGHT,
        CRAFT,
        FURNITURE,
        STATUE,
        WALLDECO,
        BANNER,
        CLUTTER,
        WOOD,
        BLOCK,
        BRICK,
        TILE,
        WALL,
        MISC_MAT,
        SPECIAL,    // Boss summoning items, heart containers, mana crystals
        OTHER
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

        headSlot,
        bodySlot,
        legSlot,
        faceSlot,
        neckSlot,
        backSlot,
        wingslot,
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

    public static class ItemSpecialty
    {
        public static readonly Dictionary<ItemTypes, int> Specialties;

        static ItemSpecialty()
        {
            foreach (ItemTypes itype in Enum.GetValues(typeof(ItemTypes)))
            {

            }
        }

        private static IList<int> get_specs(ItemTypes type)
        {
            switch (type)
            {
                case ItemTypes.Armor:
                case ItemTypes.Accessory:
                case ItemTypes.Consumable:
                case ItemTypes.Material:
                case ItemTypes.Tool:
                case ItemTypes.Weapon:

                    break;

            }

            return new int[0];
        }
    }

}
