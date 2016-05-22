using System;
using System.Linq;
// using System.Linq.Expressions;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using InvisibleHand.Items.Categories.Types;
using Terraria.ModLoader;


namespace InvisibleHand.Items.Categories
{
    public struct ReqEx
    {
        public IDictionary<string, int> Requirements;
        public IDictionary<string, int> Exclusions;
    }

    /// read in the trait and category specs from the definition files and convert to something useable
    public static class Parser
    {
        // private static string TraitFilePath;
        private static string TraitDefsPath;
        private static string CategoryDefsPath;

        private static Assembly assembly;

        /// track category currently being parsed
        private static string _currentCategory;
        /// and how many have been created so far
        private static int _currentCount = 0;

        // tokenizer instance that will handle our definition files
        private static Tokenizer tokenizer;
        // char arrays used during parsing
        private static char[] comment_chars = "#;".ToCharArray();
        private static char[] strip_chars = " \t\n\"'".ToCharArray();


        public static IDictionary<string, IList<string>> TraitDefinitions { get; private set; }
        public static IDictionary<string, ItemCategory> CategoryDefinitions { get; private set; }

        /// Call this method to run all the other class methods
        public static void Parse(string category_path = "Definitions.Categories.conf", string trait_path = "Definitions.Traits.ini")
        {
            assembly = Assembly.GetExecutingAssembly();

            CategoryDefsPath = "InvisibleHand." + category_path;
            TraitDefsPath = "InvisibleHand." + trait_path;

            tokenizer = new Tokenizer();


            // the order is important here
            LoadTraits(TraitDefsPath);
            AssignFlagValues();

            LoadCategories(CategoryDefsPath);
            CalculateOrdinals();
            BuildCategoryTree();
        }

        // haha this is like writing SOS on the beach with rocks so you can get rescued
        /*
        ██       ██████   █████  ██████      ████████ ██████   █████  ██ ████████ ███████
        ██      ██    ██ ██   ██ ██   ██        ██    ██   ██ ██   ██ ██    ██    ██
        ██      ██    ██ ███████ ██   ██        ██    ██████  ███████ ██    ██    ███████
        ██      ██    ██ ██   ██ ██   ██        ██    ██   ██ ██   ██ ██    ██         ██
        ███████  ██████  ██   ██ ██████         ██    ██   ██ ██   ██ ██    ██    ███████
        */

        /// Read in Traits.ini and organize the Traits by family name;
        /// (a trait-family is something like "General", "Consumable", "Weapon", "Weapon.Melee")
        private static void LoadTraits(string trait_defs_path)
        {
            TraitDefinitions = new Dictionary<string, IList<string>>();

            using (Stream s = assembly.GetManifestResourceStream(trait_defs_path))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    var current_group = "";
                    var full_group_spec = ""; // the fully-dereferenced name, like "CreateTile.Furniture.Door"
                    var depth = 0;
                    var comment_chars = "#;".ToCharArray();
                    var strip_chars = " \t\n\"'".ToCharArray();

                    var section = (Tuple<string, int>)null;

                    var parents = new LinkedList<string>();

                    while (sr.Peek() >= 0)
                    {
                        // read a single line, stripping comments/whitespace/other stuff
                        var line = _getLine(sr.ReadLine());

                        // skip blank lines or full-line comments
                        if (line == String.Empty)
                            continue;

                        // if we're not in a section, skip lines until we
                        // find a valid one
                        if (section == null)
                        {
                            if (line[0] == '[')
                                section = _getSectionHeader(line);
                        }
                        // we were in a section, but found start of a new one
                        else if (line[0] == '[')
                        {
                            // if there's an error during parsing, this will return
                            // null which will re-start the check above (skipping
                            // lines to the next valid section)
                            section = _getSectionHeader(line);
                        }
                        else // we're in the trait-list of a section
                        {
                            // we have a new section, but haven't done anything with it yet
                            if (current_group != section.Item1)
                            {
                                // just have it modify the parents list
                                _getParent(parents, current_group, section.Item2, depth);

                                // update current group name & depth
                                current_group = section.Item1;
                                depth = section.Item2;

                                // get the dot-qualified full name of the group if it has
                                // any ancestors
                                full_group_spec = parents.Count > 0 ? string.Join(".", parents) + "." + current_group : current_group;

                                // create a container for this new group
                                TraitDefinitions[full_group_spec] = new List<string>();
                            }

                            // now we can handle the line, which should just be the
                            // name of a trait
                            var trait_name = tokenizer.TokenizeTraitEntry(line);

                            // if it was a comment or blank line (should have been
                            // caught already, though...), ignore it
                            if (trait_name == String.Empty) continue;

                            // otherwise add it to the members for the current group
                            TraitDefinitions[current_group].Add(trait_name);
                        }
                    }
                }
            }
        }

        /// Remove comments, quotes, and trailing/leading whitespace,
        /// then return the modified line.
        private static string _getLine(string line)
        {
            var comment_start = line.IndexOfAny(comment_chars);

            //Remove comments, strip any leading/trailing whitespace and quotes
            return comment_start >= 0 ? line.Substring(0, comment_start).Trim(strip_chars) : line.Trim(strip_chars);
        }

        /// attempt to get the value for an ini-style section header ("[Section]");
        /// log any errors encountered during parsing and  return null
        private static Tuple<string, int> _getSectionHeader(string line)
        {
            try
            {
                // returns Tuple(group_name, depth)
                return tokenizer.GetSectionName(line);
            }
            catch (TokenizerException te)
            {
                // error in section header
                ErrorLogger.Log(te.Message + ": " + te.Line);
                return null;
            }
        }

        /// After reading in the Trait families in LoadTraitDefinitions(),
        /// assign each trait a flag-bit-value (power of 2) based on its location
        /// within the family. As we're using ints, there should not be more than
        /// 32 members in a single family
        private static void AssignFlagValues()
        {
            // make sure trait-defs were loaded
            if (TraitDefinitions == null)
                // LoadTraitDefinitions(TraitFilePath);
                LoadTraits(TraitDefsPath);

            foreach (var tgroup in TraitDefinitions)
            {
                var flagGroup = new Dictionary<string, int>();

                // always initialize with a "none"
                flagGroup["none"] = 0;
                foreach (var trait in tgroup.Value)
                    // each new value added is a (binary) order of magnitude larger than the last
                    flagGroup[trait] = 1 << (flagGroup.Count - 1);

                // add flag group to collection
                IHBase.FlagCollection[tgroup.Key] = flagGroup;
            }
        }

        /*
        ██       ██████   █████  ██████       ██████ ████████  ██████ ██    ██ ███████
        ██      ██    ██ ██   ██ ██   ██     ██         ██    ██       ██  ██  ██
        ██      ██    ██ ███████ ██   ██     ██         ██    ██   ███  ████   ███████
        ██      ██    ██ ██   ██ ██   ██     ██         ██    ██    ██   ██         ██
        ███████  ██████  ██   ██ ██████       ██████    ██     ██████    ██    ███████
        */

        /// After all the trait-definitions have been loaded, read in all the Category definitions
        /// files and assign each Category a List of {Trait-Family: combined_flag_value} pairs
        /// using the bit-flag-values assigned in AssignFlagValues(). These family::flags maps
        /// define the full set of flags required for an individual item to match the given
        /// category. Note that it is possible for an item to match multiple categories. Conflict
        /// resolution will be weighted by the category's 'Priority' value (or the value inherited from
        /// its parent), and secondarily by the order in which it was loaded from the definition file.
        private static void LoadCategories(string category_def_file)
        {

            _currentCount = 0; // track absolute order of added categories (this will be the unique ID for each category)
            using (Stream s = assembly.GetManifestResourceStream(category_def_file))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    var prev_name = "";
                    int prev_depth = 0;

                    var parent = "";
                    var parents = new LinkedList<string>();

                    Dictionary<string, int> options;

                    // read to end of file(stream)
                    while (sr.Peek() >= 0)
                    {
                        // read a single line, strip any leading/trailing whitespace
                        var line = sr.ReadLine().Trim();

                        // lines starting with # are comments.
                        // Skip comments and empty lines
                        if (line.StartsWith("#") || line == String.Empty)
                            continue;

                        // analyze line and extract various sections into
                        // a definition object
                        var category_def = tokenizer.TokenizeCategory(line);

                        // the "depth" of a category is determined by the number of periods preceding
                        // the category's name; e.g. "...Melee" has a depth of 3.
                        // This indicates how many ancestors the category has.
                        // Using the 'parents' stack, we can utilize this depth value to track
                        // the ancestor hierarchy of the current category. Thus, the order of the
                        // definitions in the definition file is very significant to the final result.
                        parent = _getParent(parents, prev_name, category_def.Depth, prev_depth);

                        // reset options to defaults
                        options = new Dictionary<string, int>()
                        {
                            ["enable"] = 1,
                            ["display"] = 1,
                            ["merge"] = 0,
                            ["priority"] = 0,
                        };

                        // parse the "name=value" strings into something useable
                        foreach (var opt in category_def.Options)
                        {
                            var parsed_opt = tokenizer.ParseOption(opt);
                            options[parsed_opt.Key] = parsed_opt.Value;
                        }

                        bool union = category_def.TypeCode == 'u';

                        // TODO: remove the 'priority' stuff; only load order matters for initial sorting;
                        // priority may become settable at run-time
                        buildCategory(category_def.TypeCode, category_def.Name, parent, Bool(options["enable"]), Bool(options["display"]), Bool(options["merge"]), options["priority"],
                                        // right now, I'm using the 'requires' property to hold either
                                        // the list of requirements (for regular categories)
                                        // OR the list of member categories (for unions).
                                        // TODO: find a better & clearer way to store/pass this information
                                        union ? null : category_def.Requires,
                                        union ? category_def.Requires : null,
                                        category_def.SortFields);

                        // update these values for next loop
                        prev_name = _currentCategory;
                        prev_depth = category_def.Depth;
                    }
                }
            }
        }

        /// helper for getting the true/false from a option (we store all options as ints)
        private static bool Bool(int v) => Convert.ToBoolean(v);

        /// For handling of tracking the current parent for nested traits/categories
        /// where the only information we have is the names seen so far and the current
        /// "child-depth" of the trait/category being considered
        /// None of the ints here should ever be passed as a negative number. That would
        /// be a bad time.
        private static string _getParent(LinkedList<string> ancestry, string prev_name, int curr_depth, int prev_depth)
        {
            // if we're at a new top-level element, just nuke the list
            if (curr_depth == 0)
                ancestry.Clear();

            // if we've gone deeper, append the previous element to the
            // ancestors list
            else if (curr_depth > prev_depth)
                ancestry.AddLast(prev_name);

            // if we've moved back up, pop elements off the list
            // until we reach the proper depth
            else if (curr_depth < prev_depth)
                while (ancestry.Count > curr_depth)
                    ancestry.RemoveLast();

            // otherwise depth hasn't changed

            // parent is last-added value (Last will be null if list empty)
            return ancestry.Last?.Value ?? String.Empty;
        }

        /*
        ██████  ██    ██ ██ ██      ██████       ██████ ████████  ██████  ██████  ██    ██
        ██   ██ ██    ██ ██ ██      ██   ██     ██         ██    ██       ██   ██  ██  ██
        ██████  ██    ██ ██ ██      ██   ██     ██         ██    ██   ███ ██████    ████
        ██   ██ ██    ██ ██ ██      ██   ██     ██         ██    ██    ██ ██   ██    ██
        ██████   ██████  ██ ███████ ██████       ██████    ██     ██████  ██   ██    ██
        */


        private static void buildCategory(char type, string name, string parent = "",
                                  bool enable = true, bool display = true,
                                  bool merge = true, int priority = 0,
                                  IEnumerable<string> requires = null,
                                  IEnumerable<string> union_members = null,
                                  IEnumerable<string> sort_fields = null)
        {

            if (name == String.Empty)
                throw new ParserException("Categories must have a unique, non-empty name");


            // track the  name of our in-construction category
            _currentCategory = name;

            var parentID = getParentID(parent);
            // var priority = prio;
            //
            switch (type)
            {
                case 'u': //Union
                    // A union category
                    var union = new UnionCategory(name, ++_currentCount, parentID, priority: priority);

                    // TODO: allow enable/disable at runtime

                    addUnionMembers(union, union_members);
                    union.MergeItems = merge;

                    union.Enabled = enable;
                    CategoryDefinitions[union.Name] = union;
                    break;

                case 's': //Sub
                    // generate sub-categories (matchers) for each child under parent
                    generateSubCategories(name, parentID, priority, enable, requires, sort_fields);
                    break;

                case 'c': // Category
                    // a 'Regular' category
                    createMatchCategory(name, parentID, priority, enable, requires, sort_fields);
                    break;

            }

            // A union category
            // ------------------
            // if (union_members != null && union_members.Count() > 0)
            // {
            //     var union = new UnionCategory(name, ++_currentCount, parentID, priority: priority);
            //
            //     // TODO: allow enable/disable at runtime
            //
            //     addUnionMembers(union, union_members);
            //     union.MergeItems = merge;
            //
            //     union.Enabled = enable;
            //     CategoryDefinitions[union.Name] = union;
            // }
            //
            // // generate sub-categories (matchers) for each child under parent
            // // ------------------------------
            // else if (name != _name) // check will succeed if we removed the '@'
            //     generateSubCategories(name, parentID, priority, enable, requires, sort_fields);
            //
            // // a 'Regular' category
            // // ---------------------
            // else
            //     createMatchCategory(name, parentID, priority, enable, requires, sort_fields);
        }

        /*
        ███    ███  █████  ████████  ██████ ██   ██ ███████ ██████
        ████  ████ ██   ██    ██    ██      ██   ██ ██      ██   ██
        ██ ████ ██ ███████    ██    ██      ███████ █████   ██████
        ██  ██  ██ ██   ██    ██    ██      ██   ██ ██      ██   ██
        ██      ██ ██   ██    ██     ██████ ██   ██ ███████ ██   ██
        */

        /// <summary>
            /// create a <see cref="MatchCategory"/> with the given parameters, parsing out the requirements and exclusions from the <paramref name="requires"/> list.
            /// </summary>
            /// <param name="name">category name </param>
            /// <param name="parent_id"> ID of the category's immediate parent, or 0 if this is a top-level category </param>
            /// <param name="priority"> sort-order offset</param>
            /// <param name="enable"> bool, is this category enabled</param>
            /// <param name="requires"> A list of strings (possibly empty) that should be in the Requirement-line format.</param>
            /// <param name="sort_fields"> a list of strings, each the name of a Terraria.Item property</param>
        private static void createMatchCategory(string name, int parent_id,
                                                int priority, bool enable,
                                                IEnumerable<string> requires,
                                                IEnumerable<string> sort_fields)
        {

            createMatchCategory(name, parent_id, priority, enable, getRequirements(requires), sort_fields);
        }

        /// uses a pre-constructed ReqExcTuple instead of list of strings for requirements
        private static void createMatchCategory(string name, int parent_id,
                                                int priority, bool enable,
                                                ReqEx reqex,
                                                IEnumerable<string> sort_fields)
        {
            var new_category = new MatchCategory(name, ++_currentCount, parent_id, priority);

            // add reqs/excls if any
            foreach (var kvp in reqex.Requirements)
                new_category.RequireTrait(kvp.Key, kvp.Value);
            foreach (var kvp in reqex.Exclusions)
                new_category.ExcludeTrait(kvp.Key, kvp.Value);

            // create/get the Sorting Rules for the category
            assignSortingRules(new_category, sort_fields);

            // enable (or disable) the category
            new_category.Enabled = enable;

            // add to by-name collection
            CategoryDefinitions[new_category.Name] = new_category;
        }

        /*
        ███████ ██    ██ ██████   ██████ ████████  ██████ ██    ██ ███████
        ██      ██    ██ ██   ██ ██         ██    ██       ██  ██  ██
        ███████ ██    ██ ██████  ██         ██    ██   ███  ████   ███████
             ██ ██    ██ ██   ██ ██         ██    ██    ██   ██         ██
        ███████  ██████  ██████   ██████    ██     ██████    ██    ███████
        */

        /// A category definition can contain an indication that it is meant to be
        /// added as a sub-category to each already-existing child of the specified parent.
        /// This method generates those sub-categories
        private static void generateSubCategories(
                            string sub_name, int parent_id,
                            int priority, bool enable,
                            IEnumerable<string> requires,
                            IEnumerable<string> sort_fields)
        {
            // the requirements will be the same for each of these, so pre-calculate them
            var reqex = getRequirements(requires);

            foreach (var child in ItemCategory.Registry.Values.Where(cat => cat.ParentID == parent_id))
                // set name to "ParentName - ThisName"
                // e.g. "Arrows - Endless"
                createMatchCategory($"{child.Name} - {sub_name}", child.ID, priority, enable, reqex, sort_fields);
        }


        /*
        ███████  ██████  ██████  ████████     ██████  ██    ██ ██      ███████ ███████
        ██      ██    ██ ██   ██    ██        ██   ██ ██    ██ ██      ██      ██
        ███████ ██    ██ ██████     ██        ██████  ██    ██ ██      █████   ███████
             ██ ██    ██ ██   ██    ██        ██   ██ ██    ██ ██      ██           ██
        ███████  ██████  ██   ██    ██        ██   ██  ██████  ███████ ███████ ███████
        */

        /// <summary>
        /// Generate sorting rules for the given category
        /// </summary>
        /// <param name="category"> The already-initialized category that is
        /// capable of accepting sort-rules</param>
        /// <param name="property_names"> List of names of <see cref="Terraria.Item"/>
        /// properties that will be used to sort items belonging to this category.
        /// If this value is null or empty, copy the rules from the category's parent.</param>
        private static void assignSortingRules(ItemSorter category, IEnumerable<string> property_names)
        {
            if (property_names != null && property_names.Count() > 0)
                category.SortRules = ItemRuleBuilder.BuildSortRules(property_names);
            else if (category.Parent != null) // inherit from parent
                category.CopyParentRules();

            // if the rules are somehow *still* null, add a default single-rule list of just sorting by type
            if (category.SortRules == null)
                // (should just pull the rule from its cache in the vast majority of cases)
                category.SortRules = new[] { ItemRuleBuilder.GetRule("type") }.ToList();
        }


        /*
        ███    ███ ███████ ██████   ██████  ███████
        ████  ████ ██      ██   ██ ██       ██
        ██ ████ ██ █████   ██████  ██   ███ █████
        ██  ██  ██ ██      ██   ██ ██    ██ ██
        ██      ██ ███████ ██   ██  ██████  ███████
        */

        /// <summary>
        /// add each category listed under "union" in the category definition to the UnionCategory object.
        /// When the union is enabled, these categories will be notified.
        /// </summary>
        /// <param name="union"> the UnionCategory</param>
        /// <param name="member_names"> the list of names from the category definition</param>
        private static void addUnionMembers(UnionCategory union, IEnumerable<string> member_names)
        {
            foreach (var member in member_names)
            {
                try
                {
                    union.AddMember(CategoryDefinitions[member]);
                }
                catch (KeyNotFoundException knfe)
                {
                    throw new UsefulKeyNotFoundException(
                        member,
                        nameof(CategoryDefinitions),
                        knfe,
                        "UnionCategory '" +union.Name + "': member category '{0}' could not be found in '{1}' for inclusion."
                    );
                }
            }
        }

        /*
        ██████  ███████  ██████  ██    ██ ██ ██████  ███████ ███████
        ██   ██ ██      ██    ██ ██    ██ ██ ██   ██ ██      ██
        ██████  █████   ██    ██ ██    ██ ██ ██████  █████   ███████
        ██   ██ ██      ██ ▄▄ ██ ██    ██ ██ ██   ██ ██           ██
        ██   ██ ███████  ██████   ██████  ██ ██   ██ ███████ ███████
                            ▀▀
        */

        /// <summary>
            /// Get the tokenizer to analyze each entry in the requires_list
            /// and return an object containing any found requirements or
            /// exclusions. The values for each line are accumulated and
            /// placed into the returned tuple as appropriate. If for some
            /// reason no requirements or exclusions were found, (e.g. the
            /// requires_list was empty), the items of the returned tuple
            /// will both have a Count of 0.
            /// </summary>
            /// <returns> ReqEx structure with the Requirements and Exclusions properties
            /// set to the mappings generated here.
            /// </returns>
            /// <exception cref="MalformedFieldError">A requirement line was not
            /// in an recognized format</exception>
        private static ReqEx getRequirements(IEnumerable<string> requires_list)
        {
            ReqEx reqex;

            reqex.Requirements = new Dictionary<string, int>();
            reqex.Exclusions = new Dictionary<string, int>();

            // if the "requires" entry on the category was null, empty, or missing, then "requires_list" will be an
            // empty string[] and this loop will not run
            foreach (var line in requires_list)
            {
                try
                {
                    var entry = tokenizer.ParseRequirementLine(line);

                    var trait_group = entry.TraitGroup;

                    // use Update to overwrite any inherited reqs w/ the same name
                    reqex.Requirements.Update(entry.includes.ToDictionary(t=>t, t=>getValueForFlag(trait_group, t)));
                    reqex.Exclusions.Update(entry.excludes.ToDictionary(t=>t, t=>getValueForFlag(trait_group, t)));
                }
                catch (TokenizerException e)
                {
                    throw new MalformedFieldError($"Could not parse requirement entry '{e.Line}' for category '{_currentCategory}'");
                }
            }
            // return new ReqExcTuple(requirements, excludements);
            return reqex;

        }


        //helpers
        //---------------------------------------------------------------------------------


        /// <summary>
        /// gets the collection that maps the names of the members of the given trait-group to their bit-flag values.
        /// </summary>
        /// <param name="trait_group"> name of the trait-family </param>
        /// <returns>  Dictionary of trait names to bit-flag values (int)</returns>
        private static IDictionary<string, int> getFlagValues(string trait_group)
        {
            // using try-catch instead of TryGetValue because I'm not *expecting*
            // the flag to be missing; therefore, under normal circumstances, the
            // try block should always succeed and we don't have to worry about the
            // performance difference between catch() && TryGetValue
            try
            {
                return IHBase.FlagCollection[trait_group];
            }
            catch (KeyNotFoundException knfe)
            {
                throw new UsefulKeyNotFoundException(
                    trait_group,
                    nameof(IHBase.FlagCollection),
                    knfe,
                    "Category '" + _currentCategory + "': the requested Trait Group '{0}' is not present in '{1}'."
                );
            }
        }

        /// <summary>
            /// return the bit-flag value for the given trait (flag_name) from the given trait family (trait_group)
            /// </summary>
            /// <param name="trait_group"> </param>
            /// <param name="flag_name"> </param>
            /// <returns>  </returns>
        private static int getValueForFlag(string trait_group, string flag_name)
        {
            try
            {
                return getFlagValues(trait_group)[flag_name];
            }
            catch (KeyNotFoundException knfe)
            {
                throw new UsefulKeyNotFoundException(
                    flag_name,
                    nameof(IHBase.FlagCollection)+"["+trait_group+"]",
                    knfe,
                    "Category '" + _currentCategory + "': the specified required trait '{0}' is not present in '{1}'."
                );
            }
        }

        /// <summary>
            /// Get the id of the pre-existing parent with the given name. If that parent is disabled, will walk up the parent
            /// hierarchy until an enabled parent (or the root) is found.
            /// <param name="parent_name"> name pulled from the JsonObject, or String.Empty if the 'parent' field was missing or null</param>
            /// <returns> The parent with the given name or `null` if String.Empty is passed for parent_name</returns>
        private static int getParentID(string parent_name)
        {
            int parID = getCategoryID(parent_name);

            while (parID > 0 && !ItemCategory.ActiveCategories.Contains(parID))
            {
                // the parent of this category has been deactivated, so we need to 'reparent':
                // keep moving the child up a level until it has an active parent or becomes
                // a toplevel category
                parID = ItemCategory.Registry[parID].ParentID;
            }
            return parID;
        }

        /// <summary>
        /// Get the id of the pre-existing category with the given name
        /// </summary>
        private static int getCategoryID(string category_name)
        {
            try
            {
                return category_name == String.Empty ? 0 : CategoryDefinitions[category_name].ID;
            }
            catch (KeyNotFoundException knfe)
            {
                throw new UsefulKeyNotFoundException(category_name, nameof(CategoryDefinitions), knfe,
                    "Could not get ID of category '{0}': category not found in '{1}'."
                );
            }
        }

        /*
         ██████  ██████  ██████  ██ ███    ██  █████  ██      ███████
        ██    ██ ██   ██ ██   ██ ██ ████   ██ ██   ██ ██      ██
        ██    ██ ██████  ██   ██ ██ ██ ██  ██ ███████ ██      ███████
        ██    ██ ██   ██ ██   ██ ██ ██  ██ ██ ██   ██ ██           ██
         ██████  ██   ██ ██████  ██ ██   ████ ██   ██ ███████ ███████
        */

        /// <summary>
        /// Because it doesn't seem possible to assign proper ordinal values before all categories are loaded and
        /// parent-child relationships are known, we do that afterwards when we know all the context.
        /// </summary>
        private static void CalculateOrdinals()
        {
            // creates a lookup of: ParentID => [collection of child categories]
            // var lookup_byparentID = CategoryDefinitions.Select(kvp => kvp.Value).ToLookup(c => c.ParentID, c => c);
            var lookup_byparentID = ItemCategory.Registry.Values.ToLookup(c => c.ParentID, c => c);
            // assign addresses to all the top level categories, and recursively to their children
            assignAddresses(0, int.MaxValue, 0, lookup_byparentID);

            // TESTING
            // ConsoleHelper.PrintList(CategoryDefinitions.Select(kvp=>kvp.Value).OrderBy(c=>c.Ordinal).Select(c=> new {name=c.QualifiedName, order=c.Ordinal}), "Categories in order", true);
        }

        /// <summary>
            /// According to their weighted loadorder, each category is assigned a range (or 'bucket')
            /// of priority numbers ('addresses') that they and their children can use. The size of
            /// these buckets is determined by dividing the maximum addressable space (0, int.MaxValue]
            /// by the number of items currently requiring an address,
            /// here the number of toplevel categories. Each top category is assigned an address
            /// within its respective range, and later will perform the same process for its children,
            /// dividing its addressable space among them, and then the same will happen for those
            /// children's children and so on and so forth. Those categories created first (with lower IDs)
            /// will receive the lower addresses (and thus be sorted first later on), unless the user has
            /// given preference to some categories and changed their initial priority values in the definition
            /// files, in which case that order will be considered before creation order.
            /// It's basic European primogeniture, but with abstract conceptualizations of directed
            /// electrical impulses rather than scheming dukes...unless of course you have a very 'modern' view
            /// of human conciousness; then it's exactly the same.
            /// </summary>
            /// <remarks>
            /// Each category receives an address that is just 1 below the minimum range of its next sibling.
            /// This ensures that the children of this category receive lower addresses than the category itself
            /// and are sorted before it while still making sure that the parent is sorted before its siblings.
            /// In this way, we encoding a post-order tree-traversal into the priority values.
            /// </remarks>
        private static void assignAddresses(int min_address, int max_address,
                                            int parent_id,
                                            ILookup<int, ItemCategory> child_category_lookup)
        {
            // get direct children of the specified parent_id, sort by preset priority, then ID
            var recipients = child_category_lookup[parent_id].OrderBy(c => c.Priority).ThenBy(c => c.ID);

            // divide the addressable space into equal-sized buckets
            var bucket_size = (max_address - min_address) / recipients.Count();

            // go through the children, assigning each an address within their range.
            foreach (var r in recipients.Select((c, i) => new { category = c, index = i }))
            {
                var range_start = min_address + (r.index * bucket_size);
                var range_end = range_start + bucket_size - 1;

                // each category will receive an address that is just 1 below the minimum range of it's sibling.
                // r.category.Ordinal = range_end;
                // store the range on the category so it can know if an item belongs to one of its children
                r.category.SetOrdinalRange(range_start, range_end);

                // If this child is itself a parent, recursively call this function for its children
                if (child_category_lookup.Contains(r.category.ID))
                    // subtract 1 more from the end of the range so we don't try to overwrite this category
                    assignAddresses(range_start, range_end - 1, r.category.ID, child_category_lookup);
            }
        }


        /*
        ██████  ██    ██ ██ ██      ██████      ████████ ██████  ███████ ███████
        ██   ██ ██    ██ ██ ██      ██   ██        ██    ██   ██ ██      ██
        ██████  ██    ██ ██ ██      ██   ██        ██    ██████  █████   █████
        ██   ██ ██    ██ ██ ██      ██   ██        ██    ██   ██ ██      ██
        ██████   ██████  ██ ███████ ██████         ██    ██   ██ ███████ ███████
        */

        /// After reading in all of the category definitions, build a tree structure based on the parent-child relationships
        /// between the categories. Traversing the tree structure when testing an item's traits will be far more efficient than
        /// checking each category individually.
        /// NOTE: if the ordering of the categories changes (i.e. if changing priorities is allowed at runtime),
        /// the tree will need to be rebuilt each time to reflect the changes.
        public static void BuildCategoryTree()
        {
            var registry = ItemCategory.Registry;

            // only add enabled categories to the tree
            foreach (var active_id in ItemCategory.ActiveCategories)
            {
                var category = registry[active_id];

                var parentID = category.ParentID;

                // get the ordinal values (tree-keys) of the category's
                // ancestors as a stack, with the ordinal of the top-level
                // category at the front of the stack
                var catstack = new Stack<int>();
                while (parentID > 0)
                {
                    var parent = registry[parentID];
                    catstack.Push(parent.Ordinal);
                    parentID = parent.ParentID;
                }
                var subtree = IHBase.CategoryTree;

                // descend from the root down the parent-stack to the
                // proper depth of the child, auto-creating(vivifying!)
                // any non-existent nodes.
                while (catstack.Count > 0)
                {
                    // the stack contains the ordinal values for the hierarchy,
                    // so we just need to pop them off
                    subtree = subtree[catstack.Pop()];
                }
                // now create/access the child and set its data
                subtree[category.Ordinal].Data = category;
            }

            // Console.WriteLine(HBase.CategoryTree.ToString());
        }
    }
}
