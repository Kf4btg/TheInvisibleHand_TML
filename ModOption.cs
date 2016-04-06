using Terraria.ModLoader;
using Microsoft.Xna.Framework.Input;
using System;

namespace InvisibleHand
{
    /// T is the type of the option's value, usually a simple type such as int, bool, string, etc.
    public class ModOption<T>
    {
        public readonly Mod mod;

        public bool notifyChanged = false;

        // TODO: perhaps the callback should include the associated mod as a parameter, in case this is one day used for checking cross-mod options
        protected Action<T> callback;

        protected T _value;
        public virtual T Value
        {
            get { return this._value; }
            set
            {
                this._value = value;
                if (notifyChanged) callback?.Invoke(value);
            }
        }

        protected T defaultValue;
        public virtual T Default
        {
            get { return this.defaultValue; }
        }

        public ModOption(Mod mod, T value)
        {
            this.mod = mod;
            this.Value = value;
            this.defaultValue = value;
        }

        public ModOption(Mod mod, T value, Action<T> on_change, bool notify = true)
        {
            this.mod = mod;
            this.Value = value;
            this.defaultValue = value;

            this.notifyChanged = notify;
            this.callback = on_change;
        }

        public void RegisterCallback(Action<T> callback, bool notify = true)
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
            this.callback?.Invoke(this.Value);
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
        public StringOption(Mod mod, string value = "") : base(mod, value) { }

        public static implicit operator string(StringOption o)
        {
            return o.Value;
        }
    }

    public class BoolOption : ModOption<bool>
    {
        public BoolOption(Mod mod, bool value = false) : base(mod, value) { }

        public static implicit operator bool(BoolOption o)
        {
            return o.Value;
        }
    }

    public class IntOption : ModOption<int>
    {
        public IntOption(Mod mod, int value = 0) : base(mod, value) { }

        public static implicit operator int(IntOption o)
        {
            return o.Value;
        }
    }

    public class KeyOption : ModOption<Keys>
    {
        public KeyOption(Mod mod, Keys value = Keys.None) : base(mod, value) {}

        public static implicit operator Keys(KeyOption o)
        {
            return o.Value;
        }
    }
}
