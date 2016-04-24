using System;
using System.Collections.Generic;

namespace InvisibleHand.Utils
{
    public class OptionManager<T>
    {
        public readonly IDictionary<string, ModOption<T>> Options = new Dictionary<string, ModOption<T>>();


        // THOUGHTS: perhaps, in the future, this could be a more generically-useful tool by taking a Mod object as a parameter
        // and managing options for various mods at once
        // public OptionManager(Mod mod)
        // {
        //
        // }

        public T this[string key]
        {
            get { return Options[key].Value; }
            set { Options[key].Value = value; }
        }

        /// for some dictionary-parity
        public bool HasOption(string option_name) => Options.ContainsKey(option_name);

        /// for some dictionary-parity
        public bool TryGetValue(string option_name, out T val)
        {
            ModOption<T> opt;
            if (Options.TryGetValue(option_name, out opt))
            {
                val = opt.Value;
                return true;
            }
            val = default(T);
            return false;
        }

        public ModOption<T> RegisterOption(string option_name, T default_value)
        {
            // create a ModOption type based on the default value
            ModOption<T> option = new ModOption<T>(option_name, default_value);
            AddOption(option);
            return option;
        }
        public ModOption<T> RegisterOption(string option_name, T default_value, Action<string, T> callback, bool notify = true)
        {
            var option = RegisterOption(option_name, default_value);
            option.RegisterCallback(callback, notify);
            return option;
        }

        /// register a pre-constructed, non-standard (i.e. subclassed) mod option for this mod
        public void AddOption(ModOption<T> mod_option)
        {
            this.Options[mod_option.name] = mod_option;
        }
        public void AddOption(ModOption<T> mod_option, Action<string, T> callback, bool notify = true)
        {
            AddOption(mod_option);
            mod_option.RegisterCallback(callback, notify);
        }

        /// request the value (of type T) for the option registered with the name `option_name`
        /// If the option has not yet been registered with the OptionManager, an exception will be thrown.
        public T GetOptionValue(string option_name) => Options[option_name].Value;

        /// return the actual ModOption object with the given option_name;
        public ModOption<T> GetOption(string option_name) => Options[option_name];

        /// Change the value of the stored option;
        /// If a callback has been registered with this option and notify==true, that callback will be invoked with the name of the option
        /// and the new_value.
        public void UpdateOption(string option_name, T new_value)
        {
            // Console.WriteLine($"UpdateOption: {mod.Name}, {option_name}, {new_value}");
            Options[option_name].UpdateValue(new_value);
        }

        public void RegisterCallback(string option_name, Action<string, T> callback, bool notify = true)
        {
            Options[option_name].RegisterCallback(callback, notify);
        }
    }
}
