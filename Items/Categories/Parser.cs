using System;
using System.Linq;
// using System.Linq.Expressions;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Hjson;

using InvisibleHand.Items.Categories.Types;

namespace InvisibleHand.Items.Categories
{
    using ReqExcTuple = Tuple<IDictionary<string, int>, IDictionary<string, int>>;
    /// read in the category specs from the hjson files and convert to something useable
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


        public static IDictionary<string, IList<string>> TraitDefinitions { get; private set; }

        public static IDictionary<string, ItemCategory> CategoryDefinitions { get; private set; }

        // track placeholders
        private static Dictionary<string, MatchCategory> placeholders = new Dictionary<string, MatchCategory>();

        // track any "&" variables
        private static Stack<string> use_variables = new Stack<string>();


        /// Call this method to run all the other class methods
        // public static void Parse(string category_dir = "Definitions/Categories", string trait_file = "Definitions/Traits/0-All.hjson")
        // public static void Parse(string category_path = "Definitions.Categories", string trait_path = "Definitions.Traits.0-All.hjson")
        public static void Parse(string category_path = "Definitions.Categories", string trait_path = "Definitions.Traits")
        {
            assembly = Assembly.GetExecutingAssembly();

            CategoryDefsPath = "InvisibleHand." + category_path;
            TraitDefsPath = "InvisibleHand." + trait_path;

            // the order is important here
            // LoadTraitDefinitions(TraitFilePath);
            LoadFromTraitDefinitions(TraitDefsPath);
            AssignFlagValues();
            LoadCategoryDefinitions(CategoryDefsPath);
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

        /// Read in Traits.hjson and organize the Traits by family name;
        /// (a trait-family is something like "General", "Consumable", "Weapon", "Weapon.Melee")
        private static void LoadTraitDefinitions(string trait_resource)
        {
            using (Stream s = assembly.GetManifestResourceStream(trait_resource))
            {
                // var traitGroups = HjsonValue.Load(TraitFilePath).Qo();
                var traitGroups = HjsonValue.Load(s).Qo();

                TraitDefinitions = new Dictionary<string, IList<string>>();

                // Trait Group object:
                //  object name = object.Key
                //  "traits": list of strings defining the names of traits in this group
                //              Note: there should not be more than 30 traits in any single group
                //  optional Subobjects: in same form
                foreach (var tgroup in traitGroups)
                    LoadGroup(tgroup);
            }
        }

        private static void LoadFromTraitDefinitions(string traits_dir)
        {
            TraitDefinitions = new Dictionary<string, IList<string>>();

            // get file list
            foreach (var res in assembly.GetManifestResourceNames().Where(n => n.StartsWith(traits_dir)).OrderBy(n => n))
            {
                // for each file
                using (Stream s = assembly.GetManifestResourceStream(res))
                {
                    var traitGroups = HjsonValue.Load(s).Qo();

                    // Trait Group object:
                    // {
                    //    "traits": [
                    //        list of strings defining the names of traits in this group
                    //                Note: there should not be more than 32 traits in any single group
                    //    ]
                    //    optional Subobjects: in same form
                    //
                    // }  (object name = object.Key)

                    foreach (var tgroup in traitGroups)
                        LoadGroup(tgroup);
                }
            }
        }

        /// Extract and store the defined Traits for each Group. Some groups may contain nested groups
        /// (e.g. Weapon > Melee); in this case, LoadGroup will be called recursively with the name
        /// of the parent group(s) supplied for the name_prefix. The name of the nested group will
        /// be appended to the parent name with a "." (e.g. "Weapon.Melee"), thereby flattening
        /// the nesting to a single-layer Mapping
        private static void LoadGroup(KeyValuePair<string, JsonValue> group_object, string name_prefix = "")
        {
            string name = group_object.Key;

            if (name_prefix != String.Empty)
                name = name_prefix + "." + name;

            foreach (var subitem in group_object.Value.Qo())
            {
                if (subitem.Key == "traits")
                {
                    // enumerable of the names of the traits in this group
                    var members = subitem.Value.Qa().Select(jv => jv.Qs());
                    TraitDefinitions[name] = new List<string>(members);
                }
                else // assume it's a nested Group object
                {
                    LoadGroup(subitem, name);
                }
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
                LoadFromTraitDefinitions(TraitDefsPath);

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

        /// After all the trait-definitions have been loaded, read in all the Category definition
        /// files and assign each Category a List of {Trait-Family: combined_flag_value} pairs
        /// using the bit-flag-values assigned in AssignFlagValues(). These family::flags maps
        /// define the full set of flags required for an individual item to match the given
        /// category. Note that it is possible for an item to match multiple categories. Conflict
        /// resolution will be weighted by the category's 'Priority' value (or the value inherited from
        /// its parent), and secondarily by the order in which it was loaded from the definition file.
        private static void LoadCategoryDefinitions(string category_resources_path)
        {
            CategoryDefinitions = new Dictionary<string, ItemCategory>();
            _currentCount = 0; // track absolute order of added categories (this will be the unique ID for each category)

            foreach (var res in assembly.GetManifestResourceNames().Where(n => n.StartsWith(category_resources_path)).OrderBy(n => n))
                using (Stream s = assembly.GetManifestResourceStream(res))
                {
                    // read in the file stream and turn into an array of JsonObjects where each is a Category Definition
                    foreach (var catobj in HjsonValue.Load(s).Qa())
                        // get object parts
                        buildCategory(
                            // each part contains a null? check to avoid breakage at this step
                            name         : catobj.ContainsKey("name")     ? catobj["name"]    ?.Qs() ?? ""    : "",
                            parent       : catobj.ContainsKey("parent")   ? catobj["parent"]  ?.Qs() ?? ""    : "",
                            // TODO: actually use this:
                            inherit      : catobj.ContainsKey("inherit")  ? catobj["inherit"] ?.Qs()          : "", // can be null or ""
                            enable       : catobj.ContainsKey("display")  ? catobj["display"] ?.Qb() ?? true  : true,
                            // TODO: actually use this:
                            display      : catobj.ContainsKey("display")  ? catobj["display"] ?.Qb() ?? true  : true,
                            prio         : catobj.ContainsKey("priority") ? catobj["priority"]?.Qi()          : null,
                            requires     : catobj.ContainsKey("requires") ? catobj["requires"]                : null,
                            union_members: catobj.ContainsKey("union")    ? catobj["union"]   ?.Qa()          : null,
                            // TODO: actually use this:
                            merge        : catobj.ContainsKey("merge")    ? catobj["merge"]   ?.Qb() ?? false : false,
                            include      : catobj.ContainsKey("include")  ? catobj["include"] ?.Qo()          : null,
                            sort_fields  : catobj.ContainsKey("sort")     ? catobj["sort"]    ?.Qa()          : null
                        );
                }

            // when we're done, make sure there are no unfulfilled placeholders
            if (placeholders.Count > 0)
            {
                throw new ParserException("The following place-holder(s) never received a full definition: "+ string.Join(", ", placeholders.Keys));
            }
        }

        /*
        ██████  ██    ██ ██ ██      ██████       ██████ ████████  ██████  ██████  ██    ██
        ██   ██ ██    ██ ██ ██      ██   ██     ██         ██    ██       ██   ██  ██  ██
        ██████  ██    ██ ██ ██      ██   ██     ██         ██    ██   ███ ██████    ████
        ██   ██ ██    ██ ██ ██      ██   ██     ██         ██    ██    ██ ██   ██    ██
        ██████   ██████  ██ ███████ ██████       ██████    ██     ██████  ██   ██    ██
        */

        /// we have to return an enumerable of categories because of the option that causes multiple subcategories to be created
        private static void buildCategory(string name, string parent, string inherit, bool enable, bool display, int? prio, JsonValue requires, JsonArray union_members, bool merge, JsonObject include, JsonArray sort_fields)
         {
            if (name == String.Empty)
                throw new HjsonFieldNotFoundException("name", "Category Object");
            _currentCategory = name;

            // get parent, if any

            // if inherit is different than parent
            // TODO: actually use this

            // and priority, either specific to this category (or default 0)

            var parent_name = parent != String.Empty && parent[0] == '@' ? parent.Substring(1) : parent;
            var parentID    = getParentID(parent_name);
            var priority    = prio ?? 0;

            // get inherited requirements, if any
            // ---------------------------------
            var inheritsID =
                // an explicitly null "inherit" value always means "Do not inherit from anything"
                inherit == null ? 0
                // an unspecified inherit value infers "inherit from parent" (which may be nothing[id of 0])
                : inherit == String.Empty ? parentID
                // for anything else, use the category ID for the name given
                : getCategoryID(inherit);


            // A union category
            // ------------------
            if (union_members != null && union_members.Count > 0)
            {
                var union = new UnionCategory(name, ++_currentCount, parentID, priority: priority);

                // TODO: allow enable/disable at runtime

                addUnionMembers(union, union_members);
                union.MergeItems = merge;

                union.Enabled = enable;
                CategoryDefinitions[union.Name] = union;
            }

            // generate sub-categories (matchers) for each child under parent
            // ------------------------------
            else if (parent_name != parent) // check will succeed if we removed the '@'
                generateSubCategories(name, parentID, inheritsID, priority, enable, sort_fields, requires, include);

            // a 'Regular' category
            // ---------------------
            else
                createMatchCategory(name, parentID, inheritsID, priority, enable, sort_fields, requires, include);
        }


        /*
        ██ ███    ██  ██████ ██      ██    ██ ██████  ███████ ███████
        ██ ████   ██ ██      ██      ██    ██ ██   ██ ██      ██
        ██ ██ ██  ██ ██      ██      ██    ██ ██   ██ █████   ███████
        ██ ██  ██ ██ ██      ██      ██    ██ ██   ██ ██           ██
        ██ ██   ████  ██████ ███████  ██████  ██████  ███████ ███████
        */

        /// <summary>
        /// for parsing the "mini" categories inside an include: block
        /// </summary>
        /// <param name="parent_id"> id number of the category containing the include: block</param>
        /// <param name="category_name"> name of the included category</param>
        /// <param name="value"> Either a string that is the single requirement line for the included category, or
        /// an object that can hold two optional keys: a "requires" array, and a nested "include" block</param>
        private static void parseMiniCategory(int parent_id, string category_name, JsonValue value)
        {



            // if the "value" of a include-entry is null, it shall be understood as a placeholder for
            // a category that will defined at a later time. It is being included here to reserve its
            // sort order, so we need to make sure it gets a proper ID
            if (value == null)
                createPlaceholder(category_name, parent_id);

            else if (value.JsonType == JsonType.String)
                // category requirements are held in the single string value for the key
                createMatchCategory(category_name, parent_id, 0, 0, true, null, value);

            // if it's an object, could contain "requires" list and/or nested "include"
            else if (value.JsonType == JsonType.Object)
            {
                var minicatobj = value.Qo();

                // requires string or array
                var requires = minicatobj.ContainsKey("requires") ? minicatobj["requires"] : null;

                // recursively perform this same process for any members of a nested "include" block
                var include = minicatobj.ContainsKey("include") ? minicatobj["include"]?.Qo() : null;

                // Params:
                //    name=category_name
                //    parID = parent_id
                //    inherits_id = 0 (=>inherit from parent, allow tree to ensure parent requirements met)
                //    priority = 0 (=>do not adjust sort order)
                //    sort_fields = null (=>copy from parent)
                //    requires (=>requires[] from definition or null if not there)
                //    include (=> include object from definition or null if not present)
                createMatchCategory(category_name, parent_id, 0, 0, true, null, requires, include);
            }
            else
                throw new MalformedFieldError($"Could not parse 'include' entry '{category_name}' for category '{_currentCategory}'");
        }

        /// <summary>
            /// create a <see cref="MatchCategory"/> with the given parameters, using the pre-calculated requirements and exclusions held in the tuple 'reqex'
            /// </summary>
            /// <param name="name">category name </param>
            /// <param name="parent_id"> ID of the category's immediate parent, or 0 if this is a top-level category </param>
            /// <param name="priority"> sort-order offset</param>
            /// <param name="enable"> bool, is this category enabled</param>
            /// <param name="sort_fields"> JsonArray of strings, each the name of a Terraria.Item property</param>
            /// <param name="reqex"> Tuple&lt;IDictionary&lt;string, int&gt;, IDictionary&lt;string, int&gt;&gt; where Item1 is the Requirements map for the category and Item2 is the Exclusions map.</param>
            /// <param name="include"> Optional object containing nested child-categories; provided as a convenience feature for defining multiple, simple categories. Entries in the object will be parsed and recursively turned into a hierarchy of child-categories for this category.</param>
        private static void createMatchCategory(string name, int parent_id, int priority, bool enable, JsonArray sort_fields, ReqExcTuple reqex, JsonObject include = null)
        {

            // if this is the definition of a previously-referenced placeholder, pull it from that collection;
            // otherwise create it anew
            var new_category = placeholders.ContainsKey(name)
                                ? placeholders[name]
                                : new MatchCategory(name, ++_currentCount, parent_id, priority);

            // add reqs/excls if any
            foreach (var kvp in reqex.Item1)
                new_category.RequireTrait(kvp.Key, kvp.Value);
            foreach (var kvp in reqex.Item2)
                new_category.ExcludeTrait(kvp.Key, kvp.Value);

            // create/get the Sorting Rules for the category
            assignSortingRules(new_category, sort_fields);

            // enable (or disable) the category
            new_category.Enabled = enable;

            // add to by-name collection
            addCategoryDefinition(new_category);

            // finally handle the "include" entry
            if (include != null)
            {
                var set_var = false;
                foreach (var minicat in include)
                {
                    // check for variable definition to use for the rest of the include entries
                    if (minicat.Key == "&")
                    {
                        if (use_variables.Count > 0)
                            // this allows "stacking" the variables:
                            // e.g.:
                            //  &: A   => A
                            //  &: &.B => A.B
                            use_variables.Push(minicat.Value.Qs().Replace("&", use_variables.Peek()));
                        else
                            use_variables.Push(minicat.Value.Qs());
                        set_var = true;
                    }
                    else
                    {
                        parseMiniCategory(new_category.ID, minicat.Key, minicat.Value);
                    }
                }
                // if we set a variable to use for these include entries, pop it off the stack now that we're done
                if (set_var) use_variables.Pop();
            }
        }

        /// <summary>
            /// create a <see cref="MatchCategory"/> with the given parameters, parsing out the requirements and exclusions from the <paramref name="requires"/> JsonValue.
            /// </summary>
            /// <param name="name">category name </param>
            /// <param name="parent_id"> ID of the category's immediate parent, or 0 if this is a top-level category </param>
            /// <param name="inherit_id"> should be non-zero only if the category inherits requirements from an existing category other than the parent </param>
            /// <param name="priority"> sort-order offset</param>
            /// <param name="enable"> bool, is this category enabled</param>
            /// <param name="sort_fields"> <see cref="JsonArray"/> of strings, each the name of a Terraria.Item property</param>
            /// <param name="requires"> A JsonValue that should be either 1) A string in the Requirement-line format, 2) an array of such strings, or 3) null. </param>
            /// <param name="include"> Optional object containing nested child-categories; provided as a convenience feature for defining multiple, simple categories. Entries in the object will be parsed and recursively turned into a hierarchy of child-categories for this category.</param>
        private static void createMatchCategory(string name, int parent_id, int inherit_id, int priority, bool enable, JsonArray sort_fields, JsonValue requires, JsonObject include = null)
        {
            createMatchCategory(
                name, parent_id, priority, enable, sort_fields,
                // we can call getRequirementLines with a null value and it will just return an empty list
                parseRequirements(getRequirementLines(requires), (inherit_id > 0 && inherit_id != parent_id) ? inherit_id : 0),
                include);
        }

        /// create an empty category simply to reserve the name and id.
        /// Track placeholders to ensure that all have been
        /// defined by the time we're done parsing.
        private static void createPlaceholder(string name, int parent_id)
        {
            var new_category = new MatchCategory(name, ++_currentCount, parent_id, 0);
            // copy sort fields from parent
            // assignSortingRules(new_category, null);

            placeholders[name] = new_category;
        }

        /// Note: using "include" in a generational-category definition can quickly lead to an explosion of categories!
        private static void generateSubCategories(string sub_name, int parent_id, int inherit_id, int priority, bool enable, JsonArray sort_fields, JsonValue requires, JsonObject include = null)
        {
            // the requirements will be the same for each of these, so pre-calculate them
            var reqex = parseRequirements(getRequirementLines(requires), inherit_id > 0 ? inherit_id : 0);

            foreach (var child in ItemCategory.Registry.Values.Where(cat => cat.ParentID == parent_id))
            {
                // set name to "ParentName - ThisName"
                // e.g. "Arrows - Endless"
                createMatchCategory($"{child.Name} - {sub_name}", child.ID, priority, enable, sort_fields, reqex, include);
            }
        }


        /*
        ███████  ██████  ██████  ████████     ██████  ██    ██ ██      ███████ ███████
        ██      ██    ██ ██   ██    ██        ██   ██ ██    ██ ██      ██      ██
        ███████ ██    ██ ██████     ██        ██████  ██    ██ ██      █████   ███████
             ██ ██    ██ ██   ██    ██        ██   ██ ██    ██ ██      ██           ██
        ███████  ██████  ██   ██    ██        ██   ██  ██████  ███████ ███████ ███████
        */

        private static void assignSortingRules(ItemSorter category, JsonArray property_names)
        {
            if (property_names != null)
                category.SortRules = ItemRuleBuilder.BuildSortRules(property_names.Select(jv => jv.Qs()));

            else if (category.Parent != null) // inherit from parent
                category.CopyParentRules();
                // newcategory.ruleExpressions = p?.ruleExpressions;

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
        /// <param name="member_names"> the array of names under the "union" hjson key</param>
        private static void addUnionMembers(UnionCategory union, JsonArray member_names)
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
            /// obtain a list of requirement-definition lines from the raw <see cref="JsonValue"/> in the definition file
            /// </summary>
            /// <param name="requires"> can be a single string, an array of strings (possibly empty), or <see langword="null"/> </param>
            /// <returns> if the requires list is null, return an empty list; if the requires "list" is just a single string,
            /// return a list containing that string; otherwise return the list of strings found in the array.
            /// </returns>
            /// <exception cref="MalformedFieldError">if the "requires" value is neither null, string, nor array</exception>
        private static IEnumerable<string> getRequirementLines(JsonValue requires)
        {
            if (requires == null)
                return new string[0];

            if (requires.JsonType == JsonType.String)
                return new[] { requires.Qs() };

            else if (requires.JsonType == JsonType.Array)
                return requires.Qa().Select(r => r.Qs());

            throw new MalformedFieldError();
        }

        /// <summary>
            /// Get the tokenizer to analyze each line in the requires_list and return an object containing any found
            /// requirements or exclusions. The values for each line are accumulated and placed into the returned tuple
            /// as appropriate. If for some reason no requirements or exclusions were found, (e.g. the requires_list was empty),
            /// the items of the returned tuple will both have a Count of 0.
            /// </summary>
            /// <returns>
            /// Tuple&lt;IDictionary&lt;string, int&gt;, IDictionary&lt;string, int&gt;&gt; where Item1 is the Requirements map
            /// for the category and Item2 is the Exclusions map.
            /// </returns>
        private static ReqExcTuple parseRequirements(IEnumerable<string> requires_list, int inherit_from_id)
        {
            // if the inherited requirements do not come from the parent, we cannot
            // rely on tree traversal to ensure that the prerequisites for this category are met;
            // therefore we must add them explicitly.
            var inherited = getInheritedRequirements(inherit_from_id);

            // since the tuple returned from getInherited... contains a pointer to the actual collections of the
            // inherited category, we want to copy them before making any changes
            var requirements = new Dictionary<string, int>(inherited.Item1);
            var excludements = new Dictionary<string, int>(inherited.Item2);

            string replace_amp = null;
            if (use_variables.Count > 0)
                // if there's something in the &variable stack, use the topmost entry
                replace_amp = use_variables.Peek();

            bool use_var = replace_amp != null;

            // if the "requires" entry on the category was null, empty, or missing, then "requires_list" will be an
            // empty string[] and this loop will not run
            foreach (var line in requires_list)
            {
                try
                {
                    var entry = Tokenizer.ParseRequirementLine(use_var ? line.Replace("&", replace_amp) : line);

                    var trait_group = entry.TraitGroup;

                    // use Update to overwrite any inherited reqs w/ the same name
                    requirements.Update(entry.includes.ToDictionary(t=>t, t=>getValueForFlag(trait_group, t)));
                    excludements.Update(entry.excludes.ToDictionary(t=>t, t=>getValueForFlag(trait_group, t)));
                }
                catch (TokenizerException e)
                {
                    throw new MalformedFieldError($"Could not parse requirement entry '{e.Line}' for category '{_currentCategory}'");
                }
            }
            return new ReqExcTuple(requirements, excludements);
        }

        private static ReqExcTuple getInheritedRequirements(int inherit_from_id)
        {
            var inherited = inherit_from_id > 0 ? ItemCategory.Registry[inherit_from_id] as MatchCategory : null;

            // if the inherited category or its collection(s) are null, return an empty dictionary for that slot
            return new ReqExcTuple(
                inherited?.Requirements ?? new Dictionary<string, int>(),
                inherited?.Exclusions ?? new Dictionary<string, int>()
            );
        }

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
            int pid = getCategoryID(parent_name);

            while (pid > 0 && !ItemCategory.ActiveCategories.Contains(pid))
            {
                // the parent of this category has been deactivated, so we need to 'reparent':
                // keep moving the child up a level until it has an active parent or becomes
                // a toplevel category
                pid = ItemCategory.Registry[pid].ParentID;
            }
            return pid;
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

        private static void addCategoryDefinition(ItemCategory category)
        {
            // check to see if it's the definition of a previous
            // placeholder and untrack it if so
            if (placeholders.ContainsKey(category.Name))
                placeholders.Remove(category.Name);

            // record in Category Defs
            CategoryDefinitions[category.Name] = category;
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
