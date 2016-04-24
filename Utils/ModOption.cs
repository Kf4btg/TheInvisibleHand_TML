using Terraria.ModLoader;
using System;
using System.Collections.Generic;

namespace InvisibleHand.Utils
{

    /// T is the type of the option's value, usually a simple type such as int, bool, string, etc.
    public class ModOption<T>
    {
        public bool notifyChanged = false;
        public readonly string name;

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

        public ModOption(string name, T value)
        {
            this.name = name;
            this.Value = value;
            this.defaultValue = value;
        }

        public ModOption(string name, T value, Action<string, T> on_change, bool notify = true)
        {
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

    // TODO: split to different files?

    public class StringOption : ModOption<String>
    {
        public StringOption(string name, string value = "") : base(name, value) { }

        public static implicit operator string(StringOption o)
        {
            return o.Value;
        }
    }

    public class BoolOption : ModOption<bool>
    {
        public BoolOption(string name, bool? value = false) : base(name, (bool)value) { }

        public static implicit operator bool(BoolOption o)
        {
            return o.Value;
        }
    }

    public class FloatOption : ModOption<float>
    {
        public FloatOption(string name, float? value = default(float)) : base(name, (float)value) { }

        public static implicit operator float(FloatOption o)
        {
            return o.Value;
        }
    }

    public class IntOption : ModOption<int>
    {
        public IntOption(string name, int? value = 0) : base(name, (int)value) { }

        public static implicit operator int(IntOption o)
        {
            return o.Value;
        }
    }

    // public class KeyOption : ModOption<Keys>
    // {
    //     public KeyOption(string name, Keys? value = Keys.None) : base(name, (Keys)value) {}
    //
    //     public static implicit operator Keys(KeyOption o)
    //     {
    //         return o.Value;
    //     }
    //
    //     /// return the integer value of the Keys enum member; intended for use in serialization.
    //     public static explicit operator int(KeyOption o)
    //     {
    //         return (int)o.Value;
    //     }
    // }
}
