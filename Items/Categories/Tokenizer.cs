// using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// using InvisibleHand.Utils;

namespace InvisibleHand.Items.Categories
{

    public class RequirementEntry
    {
        // public ReqType Type;

        public string TraitGroup;
        public IList<string> includes;
        public IList<string> excludes;

        public RequirementEntry(string trait_group, IEnumerable<string> exclude = null, IEnumerable<string> include = null)
        {
            TraitGroup = trait_group;
            this.includes = include?.ToList() ?? new List<string>();// ?? new string[5];
            this.excludes = exclude?.ToList() ?? new List<string>();// ?? new string[5];
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

    // FIXME: The "|" OR operator is no longer allowed
    public static class Tokenizer
    {
        private const string PARENT_NAME = @"@?(\w+\s?)+$";
        // trait grp names begin with capital letters, may contain underscores and numbers
        private const string TRAIT_GROUP_NAME = @"[A-Z][\w\d]+";

        // a fully-specified trait group may contain a hierarchy of names separated by ".";
        // e.g. "Placeable.Furniture.Tables"
        // This checks that any period is followed by an uppercase letter.
        // --Captured in group named 'TGroup'
        private const string TRAIT_GROUP = @"(?<TraitGroup>((" + TRAIT_GROUP_NAME + @")(\.(?=[A-Z]))?)+)";

        // trait names are always lowercase, may contain underscores and numbers
        // eg:"yellow"
        private const string TRAIT_SIMPLE_NAME = @"[a-z\d_]+"; // eg:"yellow"
        // private const string TRAIT_SIMPLE_NAME = @"(?<TraitName>[a-z\d_]+)"; // eg:"yellow"
        // private const string TRAIT_SIMPLE_NAME_GROUP = @"(?<TraitName>" + TRAIT_SIMPLE_NAME + @")"; // eg:"yellow"

        // private const string TRAIT_NEGATED_NAME = @"(?<NegTraitName>!" + TRAIT_SIMPLE_NAME + @")"; // eg:"yellow"
        // eg:"yellow" or "!trophy_2"
        // private const string TRAIT_OPTION = @"(?(!)(?<TraitOption>!" + TRAIT_SIMPLE_NAME + @")|" + TRAIT_SIMPLE_NAME + @")";
        private const string TRAIT_OPTION = @"(?<TraitName>!?" + TRAIT_SIMPLE_NAME + @")";
        // private const string TRAIT_OPTION = @"(?(!)" + TRAIT_NEGATED_NAME + @"|" + TRAIT_SIMPLE_NAME_GROUP + @")";

        // "And" traits from same group together by separating trait names w/ spaces
        // e.g. "purple green spiked"
        // private const string SIMPLE_TRAIT_LIST = @"(" + TRAIT_SIMPLE_NAME_GROUP + @"\s)*" + TRAIT_SIMPLE_NAME_GROUP;

        // as above, but optionally negated
        // e.g. "purple !green !blue spiked"
        private const string TRAIT_LIST = @"(" + TRAIT_OPTION + @"\s)*" + TRAIT_OPTION;

        // optionally include the trait group for full specification
        // e.g. "Property.Ident !wand" or "create_tile"
        // private const string TRAIT_SPEC = @"(?<TraitSpec>" + TRAIT_GROUP + @"\s+" + TRAIT_OPTION + @")";

        // put it all together
        private const string REQUIREMENTS_LINE = @"^" + TRAIT_GROUP + @"\s+" + TRAIT_LIST + @"$";

        public static RequirementEntry ParseRequirementLine(string line)
        {

            var match = Regex.Match(line, REQUIREMENTS_LINE, RegexOptions.ExplicitCapture);

            if (match.Success)
            {

                // Console.WriteLine();
                // Console.WriteLine(line);

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

                // Console.WriteLine("Trait Group: {0}", req.TraitGroup);
                // ConsoleHelper.PrintList(req.includes, "   includes");
                // ConsoleHelper.PrintList(req.excludes, "   excludes");
                return req;

            }
            else
            {
                throw new TokenizerException(line, "Error while parsing line.");
                // Console.WriteLine("Tokenizer Exception: {0}", line);
                // return new RequirementEntry("");
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
