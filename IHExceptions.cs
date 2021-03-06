using System;
// using System.Collections.Generic;

namespace InvisibleHand
{
    public class UsefulKeyNotFoundException : Exception
    {
        public UsefulKeyNotFoundException(string key, string dict_name,
                                            string msg_format = "The given key '{0}' was not found in the dictionary '{1}'.") :
                                            base (string.Format(msg_format, key, dict_name))
        {
            this.Data["Key"] = key;
            this.Data["Dict"] = dict_name;
        }

        /// <summary>
        /// A More usefule KeyNotFound Exception
        /// </summary>
        /// <param name="key">the key (or representation of it) that could not be located in the dictionary </param>
        /// <param name="dict_name"> variable name of the dictionary </param>
        /// <param name="inner"> The standard exception that was caught</param>
        /// <param name="msg_format"> A format string where {0} is the key and {1} is the dictionary name</param>
        /// <returns>  </returns>
        public UsefulKeyNotFoundException(string key, string dict_name, Exception inner,
            string msg_format = "The given key '{0}' was not found in the dictionary '{1}'.")
            : base (string.Format(msg_format, key, dict_name), inner)
        {
           this.Data["Key"] = key;
           this.Data["Dict"] = dict_name;
        }
    }

    public class HjsonFieldNotFoundException : UsefulKeyNotFoundException
    {
        // public HjsonFieldNotFoundException (string message) : base(message) {}

        public HjsonFieldNotFoundException(string key, string object_name)
            : base (key, object_name, "The field '{0}' was not found in the JsonValue object '{1}'.")
        { }

        // public HjsonFieldNotFoundException (string message, Exception inner) : base(message,inner) {}
        public HjsonFieldNotFoundException(string key, string object_name, Exception inner)
            : base (key, object_name, inner, "The field '{0}' was not found in the JsonValue object '{1}'.")
        { }
    }

    public class NoDualUnionsException : Exception
    {
        public NoDualUnionsException(string message) : base(message) {}
    }

    public class TokenizerException : Exception
    {
        /// The line that failed to tokenize correctly
        public readonly string Line;
        public TokenizerException(string line, string msg) : base(msg)
        {
            Line = line;
        }
    }

    public class ParserException : Exception
    {
        public ParserException(string msg) : base(msg) {}
    }

    public class MalformedFieldError : Exception
    {
        public MalformedFieldError(string msg="Hjson field wrong type or bad format") : base(msg) {}
    }
}
