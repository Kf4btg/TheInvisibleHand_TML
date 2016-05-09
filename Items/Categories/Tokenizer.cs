using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace InvisibleHand.Items.Categories
{
    public enum ReqType
    {
        None,   // all traits in this requirements are to be excluded from the category
        All,    // all traits in this requirement must be present
        Any,    // at least one of the traits in this req. must be present
        One     // Only one of the traits in this req. may be present
    }

    public struct RequirementEntry
    {
        // public ReqType Type;

        public IList<string> includes;
        public IList<string> excludes;

        public RequirementEntry(IList<string> exclude = null, IList<string> include=null)
        {
            // this.Type = type;
            // this.Traits = traits ?? new string[5];
            this.includes = include;// ?? new string[5];
            this.excludes = exclude;// ?? new string[5];
        }

        // public void Add(string trait)
        // {
        //     this.Traits.Add(trait);
        // }

        public void Include(string trait)
        {
            if (this.includes == null)
                this.includes = new[] { trait };
            else
                this.includes.Add(trait);
        }

        public void Include(IEnumerable<string> traits)
        {
            if (this.includes == null)
                this.includes = traits.ToArray();
            else
                foreach (var t in traits)
                    this.includes.Add(t);
        }

        public void Exclude(string trait)
        {
            if (this.excludes == null)
                this.excludes = new[] { trait };
            else
                this.excludes.Add(trait);
        }

        public void Exclude(IEnumerable<string> traits)
        {
            if (this.excludes == null)
                this.excludes = traits.ToArray();
            else
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
        private const string TRAIT_GROUP = @"(?<TGroup>((" + TRAIT_GROUP_NAME + @")(\.(?=[A-Z]))?)+)";

        // trait names are always lowercase, may contain underscores and numbers
        // eg:"yellow"
        private const string TRAIT_SIMPLE_NAME = @"[a-z\d_]+"; // eg:"yellow"
        // private const string TRAIT_SIMPLE_NAME = @"(?<TraitName>[a-z\d_]+)"; // eg:"yellow"
        private const string TRAIT_SIMPLE_NAME_GROUP = @"(?<TraitName>" + TRAIT_SIMPLE_NAME + @")"; // eg:"yellow"

        // private const string TRAIT_NEGATED_NAME = @"(?<NegTraitName>!" + TRAIT_SIMPLE_NAME + @")"; // eg:"yellow"
        // eg:"yellow" or "!trophy_2"
        // --Captured in group named 'TName'
        // private const string TRAIT_OPTION = @"(?(!)(?<TraitOption>!" + TRAIT_SIMPLE_NAME + @")|" + TRAIT_SIMPLE_NAME + @")";
        private const string TRAIT_OPTION = @"(?<TraitName>!?" + TRAIT_SIMPLE_NAME + @")";
        // private const string TRAIT_OPTION = @"(?(!)" + TRAIT_NEGATED_NAME + @"|" + TRAIT_SIMPLE_NAME_GROUP + @")";

        // "And" traits from same group together by separating trait names w/ spaces
        // e.g. "purple green spiked"
        private const string SIMPLE_TRAIT_LIST = @"(" + TRAIT_SIMPLE_NAME_GROUP + @"\s)*" + TRAIT_SIMPLE_NAME_GROUP;

        // as above, but optionally negated
        // e.g. "purple !green !blue spiked"
        private const string TRAIT_LIST = @"(" + TRAIT_OPTION + @"\s)*" + TRAIT_OPTION;

        // Or traits together by separating with "|"
        // e.g. "bird | rock", "blue|yellow|now"
        private const string SIMPLE_TRAIT_OR_LIST = @"(?<OrList>" + TRAIT_SIMPLE_NAME_GROUP + @"(\s*\|\s*" + TRAIT_SIMPLE_NAME_GROUP + @")+)";

        // exclude several traits at once by negating an or'd list;
        // the internal traits must be simple (not negated, no full spec name)
        // e,g, "!(happy | sad | mellow)"
        private const string NEGATE_OR_LIST = @"(?<NotOrList>!\(" + SIMPLE_TRAIT_OR_LIST + @"\))";

        // private const string OR_LIST = @"(?<OrList>" + NEGATE_OR_LIST + @"|" + SIMPLE_TRAIT_OR_LIST + @")";
        private const string OR_LIST = NEGATE_OR_LIST + @"|" + SIMPLE_TRAIT_OR_LIST;

        // optionally include the trait group for full specification
        // e.g. "Property.Ident !wand" or "create_tile"
        private const string TRAIT_SPEC = @"(?<TraitSpec>" + TRAIT_GROUP + @"\s+" + TRAIT_OPTION + @")";
        // private const string TRAIT_SPEC_LIST = @"(?<TraitSpec>" + TRAIT_GROUP + @"\s+" + TRAIT_LIST + @")";


        // defines an entry that begins with the trait group then uses only simple name-constructs for the rest of the entry.
        // e.g. "Property create_tile | create_wall", "Property.Ident !(pick | axe | hammer)",
        // "Property heal_life heal_mana", "UseStyle use_style_2"
        private const string GROUP_TRAIT_ENTRY = @"(?<SingleGroupEntry>" + TRAIT_GROUP + @"\s+(" + TRAIT_LIST + @"|" + OR_LIST + @"))";

        // entry that starts with  a trait group name and may include a "|" with a trait from an entirely different group
        // e.g. "Property !not_ammo | UseStyle use_style_any"
        private const string MULTIGROUP_OR_ENTRY = @"(?<MultiGroupOr>"+TRAIT_SPEC + @"(\s*\|\s*" + TRAIT_SPEC + @")+)";

        // private const string TRAIT_OR_LIST = TRAIT_SIMPLE_NAME + @"(\s*\|\s*"+TRAIT_SIMPLE_NAME + @")+";
        // private const string TRAIT_SPEC = @"((" + TRAIT_GROUP + @")\s+)?" + TRAIT_NAME_LIST;

        // put it all together
        private const string REQUIREMENTS_LINE = @"^(" + GROUP_TRAIT_ENTRY + @"|" + MULTIGROUP_OR_ENTRY + @")$";

        // So far not allowed:
        //  Group trait1 !trait2  # 1-line and-ing w/ a negation
        //  Group !trait3 | trait4 # 1-line or-ing w/ a negation within the same group
        //  Group1 trait1 | Group2 trait2 | trait3 # not specifying the Group name each time when or-ing traits from different groups

        public static IDictionary<string, int> ParseRequirementLine(string line)
        {

            var match = Regex.Match(line, REQUIREMENTS_LINE, RegexOptions.ExplicitCapture);

            // list of all the requirements (there may be multiple entries if some of the traits are or'd)
            var reqs = new List<RequirementEntry>();

            // "MultiGroupOr", "SingleGroupEntry", "TraitSpec", "TGroup", /*"NegTraitName",*/ "TraitName", "NotOrList","OrList"
            if (match.Success)
            {

                var state = new
                {
                    single = match.Groups["SingleGroupEntry"].Success,
                    multi = match.Groups["MultiGroupOr"].Success,
                    tspecs = match.Groups["TraitSpec"].Captures,
                    tgroups = match.Groups["TGroup"].Captures,
                    tnames = match.Groups["TraitName"].Captures,
                    norlist = match.Groups["NotOrList"].Success,
                    orlist = match.Groups["OrList"].Success,
                };

                Console.WriteLine(line);
                Console.WriteLine(state);
                Console.WriteLine();

                if (state.single)
                {
                    var trait_group = state.tgroups[0].Value;

                    if (state.orlist)
                    {
                        if (state.norlist) // all are excluded
                        {
                            var req = new RequirementEntry();

                            for (int i = 0; i < state.tnames.Count; i++)
                                req.Exclude(state.tnames[i].Value);

                            reqs.Add(req);
                        }
                        // FIXME: this is NOT what we want. As requirements can span multiple
                        // lines, and we don't want a nrew requirment entry for each line (which
                        // would essentially OR all the lines, reather than AND them as intedended)
                        // we need to find a way to mark multiple-choice traits within a single requirement
                        else // need an entirely different requirement for each item
                        {
                            for (int i = 0; i < state.tnames.Count; i++)
                            {
                                var req = new RequirementEntry();
                                string trait = state.tnames[i].Value;

                                if (trait[0] == '!')
                                    req.Exclude(trait.Substring(1));
                                else
                                    req.Include(trait);

                                reqs.Add(req);

                            }
                        }
                        // req.Type = state.norlist ? ReqType.None : ReqType.Any;
                        // var target = state.norlist ? req.excludes : req.includes;

                    }
                    else
                    {

                    }
                    // Console.WriteLine("SingleGroupEntry: {0}", line);
                }
                else if (state.multi)
                {

                    // Console.WriteLine("MultiGroupOr: {0}", line);
                }
                else
                {

                    Console.WriteLine("Something else?");
                }

            }
            else
            {
                // throw new TokenizerException(line, "Error while parsing line.");
                Console.WriteLine("Tokenizer Exception");
            }

            return new Dictionary<string, int>();
        }

        /// testing
        static void Main()
        {
            Console.WriteLine(REQUIREMENTS_LINE);

            // foreach (var str in new[]{
            //     "Property", "Property.Ident", "property", "property.ident", "Property_Ident"
            //     })
            //     {
            //         var m = Regex.Match(str, TRAIT_GROUP);
            //         Console.WriteLine("'{0}': {1}", str, m.Success ? "VALID trait group" : "INVALID trait group");
            //     }

            // foreach (var str in new[] {
            //     "Property.Ident !(pick | hammer | axe)",
            //     "Property !(pick | hammer | axe)",
            //     "Property.Ident hammer",
            //     "PropertyIdent hammer",
            //     "PropertyIdent hammer axe2",
            //     "Prop_erty pick_3",
            //     "Property !pick__4_1",
            //     "Property2 pick hammer",
            //     "Property pick !hammer",
            //     "Property pick | Property.Ident hammer",
            //     "Property pick | Property.Ident !hammer",
            //
            //     // these should fail
            //     "PropertyIdent hammer axe | pick",
            //     "Property pick | Property.Ident hammer fish",
            //     "Property.Ident hammer | Weapon !(ranged | magic)",
            //     "PropertyIdent hammer | axe pick",
            //     "Property Pick", "Property Ident pick", "Property.ident hammer",
            //     "Property !(all | none) | Weapon ranged",
            //     })
            //     {
            //         var m = Regex.Match(str, REQUIREMENTS_LINE);
            //         Console.WriteLine("'{0}': {1}", str, m.Success ? "YES" : "NO");
            // }

            // var newm = Regex.Match("Property pick | Property.Ident !hammer", REQUIREMENTS_LINE, RegexOptions.ExplicitCapture);

            foreach (var str in new[] {
                "Property.Ident !(pick | hammer | axe)",
                "Property !(pick | hammer | axe)",
                "Property pick | hammer | axe",
                "Property.Ident hammer",
                "PropertyIdent hammer",
                "PropertyIdent hammer axe2",
                "Prop_erty pick_3",
                "Property !pick__4_1",
                "Property2 pick hammer",
                "Property pick !hammer",
                "Property !pick !hammer axe",
                "Property pick | Property.Ident hammer",
                "Property pick | Property.Ident hammer | Weapon !melee",
                "Property pick | Property.Ident !hammer"})
            {
                ParseRequirementLine(str);
                // var newm = Regex.Match(str, REQUIREMENTS_LINE, RegexOptions.ExplicitCapture);
                //
                //
                // if (newm.Success)
                // {
                //     Console.WriteLine("Matched '{0}'", newm.Value);
                //     // for (int ctr = 1; ctr < newm.Groups.Count; ctr++)
                //     foreach (var groupname in new[] {"MultiGroupOr","SingleGroupEntry", "TraitSpec", "TGroup", /*"NegTraitName",*/ "TraitName", "NotOrList","OrList"})
                //     {
                //         // Console.WriteLine("   Group {0}:  {1}", ctr, newm.Groups[ctr].Value);
                //         Console.WriteLine("   Group {0}:  {1}", groupname, newm.Groups[groupname].Value);
                //         int captureCtr = 0;
                //         // var captures = newm.Groups[ctr].Captures;
                //         // foreach (Capture capture in newm.Groups[ctr].Captures)
                //         foreach (Capture capture in newm.Groups[groupname].Captures)
                //         {
                //             Console.WriteLine("      Capture {0}: {1}", captureCtr, capture.Value);
                //             // Console.WriteLine("      Capture {0}: {1}", captureCtr, captures[capname].Value);
                //             captureCtr++;
                //         }
                //     }
                //     Console.WriteLine();
                // }
                // else Console.WriteLine("Match Failed");
            }
        }
    }
}
