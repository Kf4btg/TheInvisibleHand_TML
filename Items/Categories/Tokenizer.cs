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
        public string Name;
        public string[] Options;
        public string[] Requires;
        public string[] SortFields;
    }

    public static class Tokenizer
    {
        // trait grp names begin with capital letters, may contain underscores and numbers
        private const string TRAIT_GROUP_NAME = @"[A-Z][\w\d]+";

        // a fully-specified trait group may contain a hierarchy of names separated by ".";
        // e.g. "Placeable.Furniture.Tables"
        // This checks that any period is followed by an uppercase letter.
        // --Captured in group named 'TraitGroup'
        private const string TRAIT_GROUP = @"(?<TraitGroup>((" + TRAIT_GROUP_NAME + @")(\.(?=[A-Z]))?)+)";

        // trait names are always lowercase, may contain underscores and numbers
        // eg:"yellow_2"
        private const string TRAIT_NAME = @"[a-z\d_]+";

        // as above, but optionally negated
        // --Captured in group named 'TraitName'
        private const string TRAIT_OPTION = @"(?<TraitName>!?" + TRAIT_NAME + @")";

        // "And" traits from same group together by separating trait names w/ spaces
        // e.g. "purple !green !blue spiked"
        private const string TRAIT_LIST = @"(" + TRAIT_OPTION + @"\s)*" + TRAIT_OPTION;

        // put it all together:
        // A requirement line is a Group Name followed by one or more traits (which can optionally begin with !)
        public const string REQUIREMENTS_LINE = @"^" + TRAIT_GROUP + @"\s+" + TRAIT_LIST + @"$";

        // same as above, but the group is optional (will be provided separately)
        private const string REQUIREMENTS_LINE_NO_GROUP = @"^(" + TRAIT_GROUP + @"\s+)?" + TRAIT_LIST + @"$";

        /// Given a line from the requirements array, parse out the group and in-/ex-cluded traits
        public static RequirementEntry ParseRequirementLine(string line)
        {
            // only capture the named groups to keep things simpler
            var match = Regex.Match(line, REQUIREMENTS_LINE, RegexOptions.ExplicitCapture);

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

        public static RequirementEntry ParseRequirementLine(string line, string using_group)
        {
            // if called with a null or empty group, just return the normal method
            if (using_group == null || using_group == String.Empty)
                return ParseRequirementLine(line);

            // otherwise, use the group provided in 'using_group' UNLESS a group is encountered on the line
            var match = Regex.Match(line, REQUIREMENTS_LINE_NO_GROUP, RegexOptions.ExplicitCapture);

            if (match.Success)
            {
                // otherwise, use the group provided in 'using_group'...
                var trait_group = using_group;

                // ...UNLESS a group name is encountered on the line
                if (match.Groups["TraitGroup"].Captures.Count > 0)
                    trait_group = match.Groups["TraitGroup"].Captures[0].Value;

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

        // testing
        // static void Main()
        // {
        //     Console.WriteLine(REQUIREMENTS_LINE);
        //
        //     // foreach (var str in new[]{
        //     //     "Property", "Property.Ident", "property", "property.ident", "Property_Ident"
        //     //     })
        //     //     {
        //     //         var m = Regex.Match(str, TRAIT_GROUP);
        //     //         Console.WriteLine("'{0}': {1}", str, m.Success ? "VALID trait group" : "INVALID trait group");
        //     //     }
        //
        //     // foreach (var str in new[] {
        //     //     "Property.Ident !(pick | hammer | axe)",
        //     //     "Property !(pick | hammer | axe)",
        //     //     "Property.Ident hammer",
        //     //     "PropertyIdent hammer",
        //     //     "PropertyIdent hammer axe2",
        //     //     "Prop_erty pick_3",
        //     //     "Property !pick__4_1",
        //     //     "Property2 pick hammer",
        //     //     "Property pick !hammer",
        //     //     "Property pick | Property.Ident hammer",
        //     //     "Property pick | Property.Ident !hammer",
        //     //
        //     //     // these should fail
        //     //     "PropertyIdent hammer axe | pick",
        //     //     "Property pick | Property.Ident hammer fish",
        //     //     "Property.Ident hammer | Weapon !(ranged | magic)",
        //     //     "PropertyIdent hammer | axe pick",
        //     //     "Property Pick", "Property Ident pick", "Property.ident hammer",
        //     //     "Property !(all | none) | Weapon ranged",
        //     //     })
        //     //     {
        //     //         var m = Regex.Match(str, REQUIREMENTS_LINE);
        //     //         Console.WriteLine("'{0}': {1}", str, m.Success ? "YES" : "NO");
        //     // }
        //
        //     // var newm = Regex.Match("Property pick | Property.Ident !hammer", REQUIREMENTS_LINE, RegexOptions.ExplicitCapture);
        //
        //     foreach (var str in new[] {
        //         "Property.Ident !(pick | hammer | axe)",
        //         "Property !(pick | hammer | axe)",
        //         "Property pick | hammer | axe",
        //         "Property.Ident hammer",
        //         "PropertyIdent hammer",
        //         "PropertyIdent hammer axe2",
        //         "Prop_erty pick_3",
        //         "Property !pick__4_1",
        //         "Property2 pick hammer",
        //         "Property pick !hammer",
        //         "Property !pick !hammer axe",
        //         "Property pick | Property.Ident hammer",
        //         "Property pick | Property.Ident hammer | Weapon !melee",
        //         "Property pick | Property.Ident !hammer"})
        //     {
        //         ParseRequirementLine(str);
        //         // var newm = Regex.Match(str, REQUIREMENTS_LINE, RegexOptions.ExplicitCapture);
        //         //
        //         //
        //         // if (newm.Success)
        //         // {
        //         //     Console.WriteLine("Matched '{0}'", newm.Value);
        //         //     // for (int ctr = 1; ctr < newm.Groups.Count; ctr++)
        //         //     foreach (var groupname in new[] {"MultiGroupOr","SingleGroupEntry", "TraitSpec", "TGroup", /*"NegTraitName",*/ "TraitName", "NotOrList","OrList"})
        //         //     {
        //         //         // Console.WriteLine("   Group {0}:  {1}", ctr, newm.Groups[ctr].Value);
        //         //         Console.WriteLine("   Group {0}:  {1}", groupname, newm.Groups[groupname].Value);
        //         //         int captureCtr = 0;
        //         //         // var captures = newm.Groups[ctr].Captures;
        //         //         // foreach (Capture capture in newm.Groups[ctr].Captures)
        //         //         foreach (Capture capture in newm.Groups[groupname].Captures)
        //         //         {
        //         //             Console.WriteLine("      Capture {0}: {1}", captureCtr, capture.Value);
        //         //             // Console.WriteLine("      Capture {0}: {1}", captureCtr, captures[capname].Value);
        //         //             captureCtr++;
        //         //         }
        //         //     }
        //         //     Console.WriteLine();
        //         // }
        //         // else Console.WriteLine("Match Failed");
        //     }
        // }
    }


    /// Tokenize the 1-line category definition.
    /// A def will consist of the following sections,
    /// where each section other than TYPE and NAME may be empty:
    ///
    /// [TYPE]:[NAME];[OPTIONS];[REQUIRES];[SORTBY]
    ///
    /// TYPE: either 'c' for category or 'u' for union
    /// NAME: the name of the Category; may be preceded
    /// 	by a number of periods indicating child depth
    /// OPTIONS:a list of "option_name=[true|false]", separated
    /// 	by commas
    /// REQUIRES: a list of Requirement lines, described elsewhere
    /// SORTBY: a list of Item properties by which to sort items
    /// 	in the category
    ///
    /// Example:
    /// 	c:Consumable;;Types consumable;type stack
    /// 	c:Equipable;;Types equipable;
    /// 	c:.Armor;match=false;Property !vanity;defense, rare, type, value
    /// 	u:..Gauntlets;merge=true;Main-Hand Gauntlet, Off-Hand Gauntlet;
    ///
    public class Tokenizer2
    {
        // private static char[] types = new[] { 'c', 'u' };

        // a word that start with a capital letter followed by any other letters or numbers
        private const string CAPWORD = @"[A-Z][\w\d]+";


        private const string BRACKETS_OPENING = @"[\(\[\{]";
        // private const string BRACKETS_OPENING = @"";
        private const string BRACKETS_CLOSING = @"[\)\]\}]";
        // private const string BRACKETS_CLOSING = @"";

        // a category name can be a phrase where each word begins with a capital letter.
        // Surrounding parentheses or brackets are allowed as well
        private const string CATEGORY_NAME = @"
            (" + BRACKETS_OPENING + @"?
            [A-Z][\w\d-]+
            " + BRACKETS_CLOSING + @"?\s+)*

            " + BRACKETS_OPENING + @"?
            [A-Z][\w\d-]+
            " + BRACKETS_CLOSING + @"?";


        // private const string TRAIT_GROUP = @"((" + CAPWORD + @")(\.(?=[A-Z]))?)+";
        // private const string TRAIT_NAME = @"[a-z\d_]+";
        // private const string TRAIT_OPTION = @"[!]?" + TRAIT_NAME;
        // private const string TRAIT_LIST = @"(" + TRAIT_OPTION + @"\s)*" + TRAIT_OPTION;
        // private const string REQUIREMENTS_LINE = TRAIT_GROUP + @"\s+" + TRAIT_LIST;
        private const string TRAIT_GROUP = @"(?<TraitGroup>((" + CAPWORD + @")(\.(?=[A-Z]))?)+)";
        private const string TRAIT_NAME = @"[a-z\d_]+";
        private const string TRAIT_OPTION = @"(?<TraitName>!?" + TRAIT_NAME + @")";
        private const string TRAIT_LIST = @"(" + TRAIT_OPTION + @"\s)*" + TRAIT_OPTION;
        private const string REQUIREMENTS_LINE = TRAIT_GROUP + @"\s+" + TRAIT_LIST;
        // private const string REQUIREMENTS_LINE = @"\b" + TRAIT_GROUP + @"\s+" + TRAIT_LIST + @"\b";

        // the fourth section can be a list of requirements or category names, depending
        // on whether this is a regular or union category
        private const string FOURTH_SECTION_ITEM = @"(" + REQUIREMENTS_LINE + @"|" + CATEGORY_NAME + @")";

        // option_name = True|False|true|false|T|F|0|1
        private const string OPTION = @"\w+\s*=\s*([TF01]|[Tt]rue|[Ff]alse)";

        // single word optionally surrounded by quotes (single or double).
        // the quotation marks were giving me troubles, that's why they're separated out like that
        private const string SORT_PROPERTY = @"
                        [" + "\"" + @"']?
                        [A-Za-z0-9_-]+
                        [" + "\"" + @"']?
                        ";

        /// Combine everything into one super regex that should be able to identify
        /// a valid definition line
        private const string DEFINITION_LINE = @"(?x)       # ignore whitespace
                        ^[CcUu]:                # the type code, followed by a colon.

                        \.*                     # any number of periods.
                        @?                      # optional @ sign
                        " + CATEGORY_NAME + @"  # The Category name

                        \s*;\s*                 # the first semicolon.

                        # the options list is a comma-separated list of items
                        # with the form 'word=True|true|False|False|T|F|0|1'
                        (
                        (" + OPTION + @",\s*)*  # any number of options followed by a comma,
                        "  + OPTION + @"        # then a final/single option.
                        )?                      # the entire section is optional

                        \s*;\s*                 # the second semicolon

                        (
                        (" + FOURTH_SECTION_ITEM + @"\s*,\s*)*  # list of requirements/categories
                        "  + FOURTH_SECTION_ITEM + @"           # final/single requirement/category
                        )?                      # the entire section is optional.

                        \s*;\s*                 # the third semicolon

                        # optional list of words, optionally separated by commas
                        (
                            (" + SORT_PROPERTY + @" # property name.
                                (,\s*|\s+)              # comma or whitespace separation.
                            )*
                        " + SORT_PROPERTY + @"
                        )?        # again, section optional
                        $"; // the end.


        private string current_path;

        // allow instantiation so that multiple tokenizers can be running in parallel
        public Tokenizer2()
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


            // Trim all extra whitespace
            var sections = def_line.Split(':', ';').Select(s=>s.Trim()).ToArray();

            // struct to return.
            return new ParsedCategory
            {
                TypeCode = sections[0].ToLower()[0],
                Name = sections[1],
                Options = sections[2] == String.Empty ? new string[0] : sections[2].Split(',').Select(s => s.Trim()).Where(s => s != String.Empty).ToArray(),
                Requires = sections[3] == String.Empty ? new string[0] : sections[3].Split(',').Select(s => s.Trim()).Where(s => s != String.Empty).ToArray(),
                SortFields = sections[4] == String.Empty ? new string[0] : sections[4].Split(',', ' ').Where(s => s != String.Empty).ToArray()
            };

            // bool union = def.type == "u";
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

        // static void Main()
        // {
        //     // var t = new Tokenizer2();
        //
        //     var tests = new[] {
        //         "c:Dye;;Property.Ident dye;value name type stack",
        //         "C:Quest Item;;Property quest_item; uniqueStack, name, type, stack",
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
