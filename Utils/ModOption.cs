using Terraria.ModLoader;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace InvisibleHand.Utils
{

    public static class OptionManager
    {
        /// maps mod name to Mod
        private static Dictionary<string, Mod> trackedMods;
        /// maps

        /// maps modname to registered String options for that mod
        private static Dictionary<string, Dictionary<string, StringOption>> StringOptions;
        // private static Dictionary<string, StringOption> StringOptions;
        // private static Dictionary<string, StringOption> StringOptions;
        // private static Dictionary<string, StringOption> StringOptions;
        // private static Dictionary<string, StringOption> StringOptions;

        /// used as an extension method on the mod class
        public static void RegisterModOption<T>(this Mod mod, string option_name, T default_value)
        {
            if (!trackedMods.ContainsKey(mod.Name))
                trackedMods.Add(mod.Name, mod);

            // Type opt_type = option_type ?? default_value.GetType();

            // switch (opt_type.Name)
            // {
            //     case "String":
            //     case "StringOption":
            //         AddOption(new StringOption(mod, (string)default_value));
            //         break;
            //     case "Boolean":
            //     case "BoolOption":
            //         AddOption(new BoolOption(mod, (bool)default_value));
            //         break;
            //     case "Int32":
            //     case "IntOption":
            //         AddOption(new IntOption(mod, (int)default_value));
            //         break;
            //     case "Keys":
            //     case "KeyOption":
            //         AddOption(new KeyOption(mod, (Keys)default_value));
            //         break;
            //     default:
            //         // if option_type is a subclass of ModOption<>, then use that
            //         if (opt_type.Equals(typeof(ModOption<>)) || opt_type.IsSubclassOf(typeof(ModOption<>)))
            //             AddOption(new opt_type())
            //
            //         // create a generic type for the object
            //         Type genericOptType = typeof(ModOption<>);
            //         Type constructedOptType = genericOptType.MakeGenericType(new Type[] { opt_type });
            //
            //         AddOption(new ModOption<opt_type>);
            //         break;
            // }
        }

        public static void RegisterStringOption(this Mod mod, string option_name, string default_value)
        {
            if (StringOptions.ContainsKey(mod.Name))
                StringOptions[mod.Name].Add(option_name, new StringOption(mod, option_name, default_value));
        }

        public static void RegisterBoolOption(this Mod mod, string option_name, bool default_value)
        {

        }

        public static void RegisterIntOption(this Mod mod, string option_name, int default_value)
        {

        }

        public static void RegisterKeyOption(this Mod mod, string option_name, Keys default_value)
        {

        }

        public static void AddOption<T>(ModOption<T> newopt)
        {

        }
    }

    /// T is the type of the option's value, usually a simple type such as int, bool, string, etc.
    public class ModOption<T>
    {
        public readonly Mod mod;

        public bool notifyChanged = false;
        public readonly string name;

        // TODO: perhaps the callback should include the associated mod as a parameter, in case this is one day used for checking cross-mod options
        protected Action<string, T> callback;

        protected T _value;
        public virtual T Value
        {
            get { return this._value; }
            set
            {
                this._value = value;
                if (notifyChanged)
                    callback?.Invoke(this.name, value);
            }
        }

        protected T defaultValue;
        public virtual T Default
        {
            get { return this.defaultValue; }
        }

        public ModOption(Mod mod, string name, T value)
        {
            this.mod = mod;
            this.name = name;
            this.Value = value;
            this.defaultValue = value;
        }

        public ModOption(Mod mod, string name, T value, Action<string, T> on_change, bool notify = true)
        {
            this.mod = mod;
            this.name = name;
            this.Value = value;
            this.defaultValue = value;

            this.notifyChanged = notify;
            this.callback = on_change;
        }

        public void RegisterCallback(Action<string, T> callback, bool notify = true)
        {
            this.notifyChanged = notify;
            this.callback = callback;
        }

        public void RemoveCallback()
        {
            this.notifyChanged = false;
            this.callback = null;
        }

        /// Bypass the 'notifyChanged' check and immediately invoke the callback with the current option Value
        public virtual void Notify()
        {
            this.callback?.Invoke(this.name, this.Value);
        }

        public virtual void UpdateValue(T new_value)
        {
            this.Value = new_value;
        }

        public virtual void Reset()
        {
            this.Value = this.Default;
        }

        // probably a bad idea to have this here generically...
        // public static implicit operator T(ModOption<T> o)
        // {
        //     return o.Value;
        // }
    }

    // TODO: split to different classes?

    public class StringOption : ModOption<String>
    {
        public StringOption(Mod mod, string name, string value = "") : base(mod, name, value) { }

        public static implicit operator string(StringOption o)
        {
            return o.Value;
        }
    }

    public class BoolOption : ModOption<bool>
    {
        public BoolOption(Mod mod, string name, bool value = false) : base(mod, name, value) { }

        public static implicit operator bool(BoolOption o)
        {
            return o.Value;
        }
    }

    public class IntOption : ModOption<int>
    {
        public IntOption(Mod mod, string name, int value = 0) : base(mod, name, value) { }

        public static implicit operator int(IntOption o)
        {
            return o.Value;
        }
    }

    public class KeyOption : ModOption<Keys>
    {
        public KeyOption(Mod mod, string name, Keys value = Keys.None) : base(mod, name, value) {}

        public static implicit operator Keys(KeyOption o)
        {
            return o.Value;
        }

        /// return the integer value of the Keys enum member; intended for use in serialization.
        public static explicit operator int(KeyOption o)
        {
            return (int)o.Value;
        }
    }
}
