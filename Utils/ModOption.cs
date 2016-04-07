using Terraria.ModLoader;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace InvisibleHand.Utils
{
    public enum OptionType
    {
        STRING,
        BOOL,
        INT,
        FLOAT,
        KEY
    }

    public static class OptionManager
    {
        /// maps mod name to Mod
        // private static IDictionary<string, Mod> trackedMods;
        /// maps

        /// maps modname to registered options (of any ModOption type)
        private static IDictionary<string, IDictionary<string, dynamic>> OptionsByMod = new Dictionary<string, IDictionary<string, dynamic>>();

        /// Called as extension method on the Mod class; the type of the option is inferred dynamically from the type of `default_value`. If this type is one of string, bool, int, float, or Keys, an appropriate subclass ModOption will be instantiated. Otherwise, a new generic ModOption type will be generated and used.
        public static void RegisterOption(this Mod mod, string option_name, dynamic default_value)
        {

            // if (!trackedMods.ContainsKey(mod.Name))
            //     trackedMods.Add(mod.Name, mod);

            // special cases
            dynamic option;
            if (default_value is string)
                option = new StringOption(mod, option_name, default_value);
            else if (default_value is bool)
                option = new BoolOption(mod, option_name, default_value);
            else if (default_value is int)
                option = new IntOption(mod, option_name, default_value);
            else if (default_value is float)
                option = new FloatOption(mod, option_name, default_value);
            else if (default_value is Keys)
                option = new KeyOption(mod, option_name, default_value);
            else
            {
                // create a ModOption type based on the default value
                Type t = default_value.GetType();
                Type constructedOptType = typeof(ModOption<>).MakeGenericType(new Type[] { t });

                option = constructedOptType.GetConstructor(new Type[]{
                    typeof(Mod),
                    typeof(string),
                    t}).Invoke(new object[]{mod, option_name, default_value});
            }

            _addOption(mod.Name, option);
        }

        /// register a pre-constructed mod option
        private static void _addOption<U, T>(string mod_name, U mod_option) where U : ModOption<T>
        {
            if (!OptionsByMod.ContainsKey(mod_name))
                OptionsByMod.Add(mod_name, new Dictionary<string, dynamic>());

            try
            {
                OptionsByMod[mod_name].Add(mod_option.name, mod_option);
            }
            catch (ArgumentException)
            {
                // don't crash on duplicate keys; but do write an error to the log
                ErrorLogger.Log($"An Option with the name {mod_option.name} is already registered for mod {mod_name}.");
            }
        }

        /// register a pre-constructed mod option for this mod
        public static void AddOption<U, T>(this Mod mod, U mod_option) where U : ModOption<T>
        {
            _addOption<U, T>(mod.Name, mod_option);
        }

        /// request the option value of the type T for the option named `option_name` in this mod
        /// E.g., if a bool option was registered with name "enableFeatureA", you could retrieve its
        /// value with
        ///     ```bool featA_enabled = mod.GetOptionValue<bool>("enableFeatureA");```
        /// If either the mod or the option has not yet been registered with the OptionManager, an exception will be thrown.
        public static T GetOptionValue<T>(this Mod mod, string option_name)
        {
            return OptionsByMod[mod.Name][option_name].Value;
        }

        public static void UpdateOption<T>(this Mod mod, string option_name, T new_value)
        {
            OptionsByMod[mod.Name][option_name].Value = new_value;
        }

        // public static void RegisterCallback(this Mod mod, string option_name, Action<string)
        // {
        //
        // }
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

    public class FloatOption : ModOption<float>
    {
        public FloatOption(Mod mod, string name, float value = default(float)) : base(mod, name, value) { }

        public static implicit operator float(FloatOption o)
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
