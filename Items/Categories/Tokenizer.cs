using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace InvisibleHand.Items.Categories
{
    /// Represent a single line in the requirements array for a category definition
    public class RequirementEntry
    {
        public string TraitGroup;
        public IList<string> includes;
        public IList<string> excludes;

        public RequirementEntry(string trait_group, IEnumerable<string> exclude = null, IEnumerable<string> include = null)
        {
            TraitGroup = trait_group;
            this.includes = include?.ToList() ?? new List<string>();
            this.excludes = exclude?.ToList() ?? new List<string>();
        }

        public void AddInclude(string trait)
        {
            this.includes.Add(trait);
        }

        public void AddInclude(IEnumerable<string> traits)
        {
            foreach (var t in traits)
                this.includes.Add(t);
        }

        public void AddExclude(string trait)
        {
            this.excludes.Add(trait);
        }

        public void AddExclude(IEnumerable<string> traits)
        {
            foreach (var t in traits)
                this.excludes.Add(t);
        }
    }

    public struct ParsedCategory
    {
        public char TypeCode;
        public int Depth;
        public string Name;
        public string[] Options;
        public string[] Requires;
        public string[] SortFields;
    }


    /// Tokenize the 1-line category definition.
    /// A def will consist of the following sections,
    /// where each section other than TYPE and NAME may be empty:
    ///
    /// [TYPE]:[NAME];[OPTIONS];[REQUIRES];[SORTBY]
    ///
    /// TYPE: either 'c' for category or 'u' for union
    /// NAME: the name of the Category; must be quoted; may be preceded
    /// 	by a number of periods indicating child depth
    /// OPTIONS:a list of "option_name=[true|false]", separated
    /// 	by commas
    /// REQUIRES: a list of Requirement lines, described elsewhere
    /// SORTBY: a list of Item properties by which to sort items
    /// 	in the category
    ///
    /// Note: separators can be either colons or semicolons; consequently,
    ///     colons and semicolons are not valid characters in Category names
    ///
    /// Example:
    /// 	c:"Consumable";;Types consumable;type stack
    /// 	c:"Equipable";priority=-1;Types equipable;
    /// 	c:."Armor";match=false;Property !vanity;defense, rare, type, value
    /// 	u:.."Gauntlets";merge=true;"Main-Hand Gauntlet", "Off-Hand Gauntlet";
    ///
    public class Tokenizer
    {
        /// a word that start with a capital letter followed by any other letters or numbers
        private const string CAPWORD = @"[A-Z][\w\d]+";

        /// a category name can be a phrase where each word begins with a capital letter.
        /// Surrounding parentheses or brackets are allowed as well.
        /// the entire phrase may be surrounded by quotes
        // private const string BRACKETS_OPENING = @"[\(\[\{]";
        // private const string BRACKETS_CLOSING = @"[\)\]\}]";
        // private const string CATEGORY_NAME = "[\"']?"+@"
        //     (" + BRACKETS_OPENING + @"?
        //     [A-Z][\w\d-]+
        //     " + BRACKETS_CLOSING + @"?\s+)*
        //
        //     " + BRACKETS_OPENING + @"?
        //     [A-Z][\w\d-]+
        //     " + BRACKETS_CLOSING + @"?" + "[\"']?";

        /// This regex matches the category name/phrase between balanced quotes
        private const string CATEGORY_LABEL = @"(?<OpenQuote>" + "\"" +
            @")[^:;," + "\"" + @"]+" +      // stuff that's not [:;,"]
            @"(?<CloseQuote-OpenQuote>" +
            "\"" + @")";

        /// option_name = Yes|No|True|False|true|false|T|F|0|1|-42
        /// actually, all that matters is that the first non-space character
        /// after the = is a 'T', 'F', 'Y', or 'N' or that the entire value after the = is
        /// a number.
        private const string OPTION = @"\w+\s*=\s*([TtFfYyNn][\w]*|-?[\d]+)";

        #region requirement entry syntax
        /// a fully-specified trait group may contain a hierarchy of names separated by ".";
        /// e.g. "Placeable.Furniture.Tables"
        /// This checks that any period is followed by an uppercase letter.
        /// --Captured in group named 'TraitGroup'
        private const string TRAIT_GROUP = @"(?<TraitGroup>((" + CAPWORD + @")(\.(?=[A-Z]))?)+)";
        /// trait names are always lowercase, may contain underscores and numbers
        /// eg:"yellow_2"
        private const string TRAIT_NAME = @"[a-z\d_]+";
        /// as above, but optionally negated
        /// --Captured in group named 'TraitName'
        private const string TRAIT_OPTION = @"(?<TraitName>!?" + TRAIT_NAME + @")";
        /// "And" traits from same group together by separating trait names w/ spaces
        /// e.g. "purple !green !blue spiked"
        private const string TRAIT_LIST = @"(" + TRAIT_OPTION + @"\s)*" + TRAIT_OPTION;
        // put it all together:
        // A requirement line is a Group Name followed by one or more traits (which can optionally begin with !)
        private const string REQUIREMENTS_LINE = TRAIT_GROUP + @"\s+" + TRAIT_LIST;

        #endregion

        /// the fourth section can be a list of requirements or category names, depending
        /// on whether this is a regular or union category
        private const string FOURTH_SECTION_ITEM = @"(" + REQUIREMENTS_LINE + @"|" + CATEGORY_LABEL + @")";



        /// single word optionally surrounded by quotes (single or double).
        /// the quotation marks were giving me troubles, that's why they're separated out like that
        private const string SORT_PROPERTY = @"
                        [" + "\"" + @"']?
                        [A-Za-z0-9_-]+
                        [" + "\"" + @"']?
                        ";

        /// Combine everything into one super regex that should be able to
        /// identify a valid definition line
        private const string DEFINITION_LINE = @"(?x)       # ignore whitespace
                        ^[CcUuSs][:;]                # the type code, followed by a colon.

                        \.*                     # any number of periods.
                        " + CATEGORY_LABEL + @"  # The Category name

                        \s*[:;]\s*                 # the second colon.

                        # the options list is a comma-separated list of items
                        # with the form 'word=True|true|False|False|T|F|0|1|<some-integer>'
                        (
                        (" + OPTION + @",\s*)*  # any number of options followed by a comma,
                        "  + OPTION + @"        # then a final/single option.
                        )?                      # the entire section is optional

                        \s*[:;]\s*                 # the third colon

                        (
                        (" + FOURTH_SECTION_ITEM + @"\s*,\s*)*  # list of requirements/categories
                        "  + FOURTH_SECTION_ITEM + @"           # final/single requirement/category
                        )?                      # the entire section is optional.

                        \s*[:;]\s*                 # the fourth colon

                        # optional list of words, optionally separated by commas
                        (
                            (" + SORT_PROPERTY + @" # property name.
                                (,\s*|\s+)              # comma or whitespace separation.
                            )*
                        " + SORT_PROPERTY + @"
                        )?        # again, section optional
                        $"; // the end.


        // allow instantiation so that multiple tokenizers can run in parallel
        public Tokenizer()
        {

        }

        /// given a definition line, extract the various sections
        public ParsedCategory TokenizeCategory(string definition)
        {
            // this could be be done using regular expressions, but for just
            // getting the sections it's probably easier to use normal
            // string processing functions

            // strip ws
            var def_line = definition.Trim();

            // validate line structure
            if (!Regex.Match(def_line, DEFINITION_LINE).Success)
                throw new TokenizerException(def_line, "Malformed Definition");

            // Split on [semi]colons, trim all extra whitespace from sections
            var sections = def_line.Split(':', ';').Select(s=>s.Trim()).ToArray();

            // struct to return.
            return new ParsedCategory
            {
                TypeCode   = sections[0].ToLower()[0], // lowercase for easier comparisons
                // number of periods in name section
                Depth      = sections[1].Substring(0, sections[1].LastIndexOf('.')+1).Length,
                // name section - periods, remove quotes
                Name       = sections[1].Substring(sections[1].LastIndexOf('.')+1).Trim(' ', '"'),
                // split on commas, strip extra spaces, remove empty entries and convert to string[]
                Options    = sections[2] == String.Empty ? new string[0] : sections[2].Split(',').Select(s => s.Trim()).Where(s => s != String.Empty).ToArray(),
                // for the 'requires' section, also trim quotes as this section could contain
                // category names if we're dealing with a union category
                Requires   = sections[3] == String.Empty ? new string[0] : sections[3].Split(',').Select(s => s.Trim('"', ' ', '\t')).Where(s => s != String.Empty).ToArray(),
                // split on commas and spaces, remove empty entries and convert to string[]
                SortFields = sections[4] == String.Empty ? new string[0] : sections[4].Split(',', ' ').Where(s => s != String.Empty).ToArray()
            };
        }

        public RequirementEntry ParseRequirementLine(string line)
        {
            // only capture the named groups to keep things simpler
            var match = Regex.Match(line, @"^"+REQUIREMENTS_LINE+@"$", RegexOptions.ExplicitCapture);

            if (match.Success)
            {
                var trait_group = match.Groups["TraitGroup"].Captures[0].Value;
                var tnames = match.Groups["TraitName"].Captures;
                var req = new RequirementEntry(trait_group);
                for (int i = 0; i < tnames.Count; i++)
                {
                    string trait = tnames[i].Value;

                    if (trait[0] == '!')
                        req.AddExclude(trait.Substring(1));
                    else
                        req.AddInclude(trait);
                }
                return req;
            }
            else
            {
                throw new TokenizerException(line, "Error while parsing line.");
            }

        }

        /// extract the name and value from an option-spec of
        /// form "option_name = true|false|T|F|0|1|some-number-like-1234"
        public KeyValuePair<string, int> ParseOption(string option)
        {
            // split on '='
            // only deal w/ lower case names and values
            var split = option.Split('=').Select(s => s.Trim().ToLower()).ToArray();

            string opt_name = split[0];
            int opt_value = 0;

            var val = split[1];

            // any value starting with "t" or "y" will be considered "true"
            if ("ty".Contains(val[0]))
                opt_value = 1;
            // likewise, any value starting with "f" or "n" will be considered "false"
            else if ("fn".Contains(val[0]))
                opt_value = 0;
            // any other value should be an integer
            else
            {
                try
                {
                    opt_value = Int32.Parse(val);
                }
                catch //(FormatException)
                {
                    // TODO: log error
                    // on error, (probably format error) default to false/0
                    opt_value = 0;
                }
            }

            return new KeyValuePair<string, int>(split[0], opt_value);
        }

        /// ini-style section header (e.g. "[GroupName]").
        /// might have periods before the name indicating child depth
        private const string TRAIT_SECTION = @"^\s*\[(?<Depth>\.*)(?<GroupName>[A-Z][\w\d]+)\]$";

        // allows whole-line and inline comments starting with '#' or ";"
        // also allows the trait names to be quoted. For some reason.
        // public const string TRAIT_ENTRY = @"^\s*(["+"\""+@"']?(?<TraitName>[a-z0-9_]+)["+"\""+@"']?\s*)?([#;].*)?$";
        // don't worry about checking for comments; they should have been removed before
        // getting here.
        private const string TRAIT_ENTRY = @"^\s*["+"\""+@"']?(?<TraitName>[a-z0-9_]+)["+"\""+@"']?$";

        /// tokenize an ini-style section header (e.g. "[..GroupName]").
        /// which might have periods before the name indicating child depth.
        /// Return a tuple where Item1 is the name given in the header
        /// and Item2 is the depth (number of periods)
        public Tuple<string, int> GetSectionName(string line)
        {
            var match = Regex.Match(line, TRAIT_SECTION, RegexOptions.ExplicitCapture);

            if (match.Success)
            {
                var trait_group = match.Groups["GroupName"].Captures[0].Value;

                // depth is the number of periods preceding the name
                var depth_cap = match.Groups["Depth"].Captures;
                if (depth_cap.Count > 0)
                    return Tuple.Create(trait_group, depth_cap[0].Value.Length);

                return Tuple.Create(trait_group, 0);
            }
            throw new TokenizerException(line, "Invalid section header");
        }

        /// returns the non-comment content from a line
        public string TokenizeTraitEntry(string line)
        {
            var match = Regex.Match(line, TRAIT_ENTRY, RegexOptions.ExplicitCapture);

            if (match.Success)
            {
                var trait_cap = match.Groups["TraitName"].Captures;

                if (trait_cap.Count > 0)
                    return trait_cap[0].Value;
            }

            return String.Empty;
        }


        // ///testing
        // static void Main()
        // {
        //     // var t = new Tokenizer();
        //
        //     var tests = new[] {
        //         "c:Dye;;Property.Ident dye;value name type stack",
        //         "C:Quest Item;priority=1000;Property quest_item; uniqueStack, name, type, stack",
        //         "C:..Broadsword;;Weapon.Melee swing, Property.Ident !pick !axe !hammer, Property !no_use_graphic;",
        //         "c:..Restoration;;Property heal_life heal_mana;\"healLife\" \"healMana\" \"type\" \"stack\"",
        //         "c:..Buff;;Property buff_time;\"buffType\" \"buffTime\" \"type\" \"stack\"",
        //         "C:..Bounce (Heavy);;Weapon.Magic bounce_heavy;",
        //         "U:..Bouncing;merge = true, display=F;Bounce (Heavy), Bounce (Hyper);",
        //         "u:....Gloves;merge=true;Main-Hand Glove, Off-Hand Glove;",
        //
        //         // these should fail:
        //         "C:..Bounce (Heavy);;Weapon.Magic bounce_heavy",
        //         "C:Quest Item;Property quest_item; uniqueStack, name, type, stack",
        //         "u....Gloves;merge=true;Main-Hand Glove, Off-Hand Glove;"
        //     };
        //
        //     foreach (var test in tests)
        //     {
        //         Console.WriteLine(test);
        //         if (Regex.Match(test, DEFINITION_LINE).Success)
        //             Console.WriteLine("    Success");
        //         else
        //             Console.WriteLine("    Failure");
        //
        //     }
        // }

    }

}
