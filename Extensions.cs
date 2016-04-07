using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

using InvisibleHand.Utils;

namespace InvisibleHand
{
    public static class IHExtensions
    {

        #region backports

        /// Backport the generic GetModPlayer from TModLoader-devel version
        // public static T GetModPlayer<T>(this Player player, Mod mod) where T : ModPlayer
        // {
        //     return (T)player.GetModPlayer(mod, typeof(T).Name);
        // }
        #endregion

        #region genericExtensions

        /// <summary>
        /// Get the array slice between the two indexes.
        /// ... Inclusive for start index, exclusive for end index.</summary>
        public static T[] slice<T>(this T[] source, int start, int end = -1)
        {
            // Handle negative ends
            if (end < 0)
                end = source.Length + end;
            int len = end - start;

            T[] sub = new T[len];
            Array.Copy(source, start, sub, 0, len);

            return sub;
        }

        /// query the Main (current) keystate about the given key
        public static bool Down(this Keys key)
        {
            return Main.keyState.IsKeyDown(key);
        }
        public static bool Up(this Keys key)
        {
            return Main.keyState.IsKeyUp(key);
        }

        /// query an arbitrary keystate about the given key
        public static bool Down(this Keys key, KeyboardState state)
        {
            return state.IsKeyDown(key);
        }
        public static bool Up(this Keys key, KeyboardState state)
        {
            return state.IsKeyUp(key);
        }


        /// returns true if the key is down now, but was up in the previous state
        public static bool Pressed(this Keys key, KeyboardState previous_state)
        {
            return key.Up(previous_state) && key.Down();
        }

        #endregion

        #region itemExensions


        public static bool IsBlank(this Item item)
        {

            return item.type == 0 || item.stack == 0;
        }

        public static ItemCat GetCategory(this Item item)
        {
            foreach (ItemCat catID in Constants.CheckOrder)
            {
                if (CategoryDef.Categories[catID].Invoke(item)) return catID;
            }
            return ItemCat.OTHER;
        }

        public static bool Matches(this Item item, ItemCat isCategory)
        {
            return CategoryDef.Categories[isCategory].Invoke(item);
        }

        public static bool IsHook(this Item item)
        {
            return Main.projHook[item.shoot];
            // return ProjectileLoader. .byType.ContainsKey(item.shoot) && (ProjDef.byType[item.shoot].hook || ProjDef.byType[item.shoot].aiStyle==7);
        }

        public static bool IsBomb(this Item item)
        {   //grenades, bombs, etc
            var proj = ProjectileLoader.GetProjectile(item.shoot);
            return proj != null && proj.projectile.aiStyle == 16;

            // return ProjDef.byType.ContainsKey(item.shoot) && ProjDef.byType[item.shoot].aiStyle==16;
        }

        // FIXME: glowstick and flaregun and...the whole thing, really
        public static bool IsTool(this Item item)
        {
            return item.createTile == TileID.Rope || item.createTile == TileID.Chain || item.name.EndsWith("Bucket") ||
            item.fishingPole > 1 || item.tileWand != -1 || item.IsHook() ||
            // ItemDef.autoSelect["Glowstick"].Contains(item.type) ||
            item.type == 1991 || item.type == 50 || item.type == 1326 ||
            // ItemDef.autoSelect["Flaregun"].Contains(item.type) ||
            item.name.Contains("Paintbrush") || item.name.Contains("Paint Roller") || item.name.Contains("Paint Scraper") ||
            (item.type >= 1543 && item.type <= 1545);
            //bucket, bug net, magic mirror, rod of discord, spectre paint tools
        }
    #endregion

    #region buttonExtensions

        // public static bool IsHovered(this Rectangle frame)
        // {
        //     return frame.Contains(Main.mouseX, Main.mouseY);
        // }
        //
        // /// Convert a byte to an rgba Color, using the
        // /// value of the byte for each field unless a value > 1
        // /// is provided for alpha. If a mult. factor is provided, each
        // /// color component will be multiplied by that factor.
        // public static Color toColor(this byte b, float mult = 1, float alpha = -1)
        // {
        //     byte a;
        //     if (mult != 1) {
        //         // casting to byte is basically just taking modulus 256:
        //         //  (b*mult)%256
        //         byte c = (byte)((float)b * mult);
        //         a = alpha > 0 ? (byte)(alpha * mult) : c;
        //         return new Color((int)c, (int)c, (int)c, (int)a);
        //     }
        //     a = alpha > 0 ? (byte)alpha : b ;
        //     return new Color((int)b, (int)b, (int)b, (int)a);
        // }
        //
        // /// This is a simplified version of toColor specifically tailored
        // /// for being called each frame when drawing text-only buttons.
        // /// This enables the pulse effect seen on the vanilla text.
        // public static Color toScaledColor(this byte b, float mult)
        // {
        //     var c = (int)((byte)((float)b * mult));
        //     return new Color(c, c, c, c);
        // }
        //
        // public static Color toScaledColor(this byte b, float mult, Color tint)
        // {
        //     var c = (int)((byte)((float)b * mult));
        //     return TAPI.Extensions.Multiply(new Color(c, c, c, c), tint);
        //     // Terraria.Utils.Multiply(textColor, tint),
        // }

    #endregion

    #region TIH action exts

        /// <summary>
        /// DefaultLabelForAction
        /// </summary>
        /// <param name="action"> </param>
        /// <param name="use_originals">If true, will return the value pulled
        /// from Terraria's code for that action. </param>
        /// <returns>Corresponding label for the action or the empty string ""
        /// if one could not be found.</returns>
        public static string DefaultLabelForAction(this TIH action, bool use_originals)
        {
            if (use_originals)
            {
                switch (action)
                {
                    case TIH.LootAll:
                    case TIH.DepositAll:
                    case TIH.QuickStack:
                    case TIH.Rename:
                    case TIH.SaveName:
                    case TIH.CancelEdit:
                        return IHBase.OriginalButtonLabels[action];
                }
            }

            string label;
            if (Constants.DefaultButtonLabels.TryGetValue(action, out label))
                return label;

            return "";
        }

        /// returns the key-bind (as a string) for the button with the given
        /// action. return value will be something like "(X)"
        // public static string GetKeyTip(this TIH action)
        // {
        //     string kbopt;
        //     if (Constants.ButtonActionToKeyBindOption.TryGetValue(action, out kbopt))
        //         return IHBase.ButtonKeyTips[kbopt];
        //
        //     return "";
        // }

    #endregion


    /// intended to use in a fluent-interface type of way; these are generic so
    /// that the proper subtype will be returned rather than a generic CoreButton
    #region ButtonService helpers

        // ///<summary>
        // /// Add a ButtonService to this button and subscribe to its hooks
        // ///</summary>
        // public static T AddNewService<T>(this T button, ButtonService service) where T : ICoreButton
        // {
        //     button.AddService(service);
        //     return button;
        // }
        //
        // /// Activates the button's default OnClick action; on_right_click = true
        // /// makes the action happen on a right click rather than a left.
        // public static T EnableDefault<T>(this T button, bool on_right_click = false) where T : ICoreButton
        // {
        //     return button.AddNewService(new DefaultClickService(button, on_right_click));
        // }
        //
        // /// Cause this button to lock/unlock its corresponding action when right-clicked
        // public static T MakeLocking<T>(this T button, Vector2? lock_offset = null, Color? lock_color = null, string locked_string = "") where T: ICoreButton
        // {
        //     return button.AddNewService(new LockingService(button, lock_offset, lock_color, locked_string));
        // }
        //
        // /// Allow this button to toggle to another button
        // public static T AddToggle<T>(this T button, T toggle_to_button, KState.Special toggle_key = KState.Special.Shift) where T: ICoreButton
        // {
        //     return button.AddNewService(new ToggleService(button, toggle_to_button, toggle_key));
        // }
        //
        // /// Specialized version of AddToggle for connecting sort/reverse-sort buttons
        // public static T AddSortToggle<T>(this T button, T reverse_button, KState.Special toggle_key = KState.Special.Shift) where T: ICoreButton
        // {
        //     return button.AddNewService(new SortingToggleService(button, reverse_button, toggle_key));
        // }
        //
        // /// Have the button toggle to a different button automatically based on defined condition.
        // /// Important!! Make sure to add this service to the other button, too, unless you want to get stuck there!
        // public static T AddDynamicToggle<T>(this T button, T button_when_false, Func<bool> check_game_state) where T: ICoreButton
        // {
        //     return button.AddNewService(new DynamicToggleService(button, button_when_false, check_game_state));
        // }
        //
        // /// <summary>
        // /// use this to help with creating buttons; e.g.:
        // /// </summary>
        // /// <example>
        // /// <code>
        // ///     TexturedButton cb = new TexturedButton(TIH.Sort).With( (b) => {
        // ///          b.Hooks.onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer);
        // ///          b.ToolTip = "Sort Me";
        // ///          // ... etc.
        // ///    })
        // /// </code>
        // ///</example>
        // public static T With<T>(this T button, Action<T> action) where T : ICoreButton
        // {
        //     if (button != null)
        //         action(button);
        //     return button;
        // }

    #endregion

    #region misc
        ///play the given sound effect;
        public static void Play(this Sound s)
        {
            SoundManager.Queue(s);
        }

        ///<param name="min">Minimum value boundary; default is 0</param>
        ///<param name="max">Maximum value boundary; default is 1</param>
        ///<returns>value bound by specified minumum and maximum
        /// values (inclusive)</returns>
        public static float Clamp(this float value, float min = 0, float max = 1)
        {
            return MathHelper.Clamp(value, min, max);
        }

    #endregion
    }
}
