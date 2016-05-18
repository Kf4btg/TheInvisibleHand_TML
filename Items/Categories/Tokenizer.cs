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
        private const string REQUIREMENTS_LINE = @"^" + TRAIT_GROUP + @"\s+" + TRAIT_LIST + @"$";

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
}
