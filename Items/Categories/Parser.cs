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
                //              Note: there should not be more than 32 traits in any single group
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
                            inherit      : catobj.ContainsKey("inherit")  ? catobj["inherit"] ?.Qs() ?? ""    : "",
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

            ItemCategory new_category = null;
            var parent_name = parent != String.Empty && parent[0] == '@' ? parent.Substring(1) : parent;
            var parentID    = getParentID(parent_name);
            var inheritsID  = getCategoryID(inherit);
            var priority    = prio ?? 0;

            // A union category
            // ------------------
            if (union_members != null && union_members.Count > 0)
            {
                var union = new UnionCategory(name, ++_currentCount, parentID, priority: priority);

                // TODO: allow enable/disable at runtime

                addUnionMembers(union, union_members);
                union.MergeItems = merge;

                union.Enabled = enable;
                new_category = union;
                CategoryDefinitions[union.Name] = union;
            }

            // a 'Regular' category
            // ------------------
            else if (requires != null)
            {
                IDictionary<string, int> reqs;
                IDictionary<string, int> excls;

                // if there are requirements, create new reqular category
                if (parseRequirements(getRequirementLines(requires), out reqs, out excls))
                {
                    // if the parent's name starts with '@', this is the special case where we
                    // generate subcategories for every child
                    if (parent_name != parent) // check will succeed if we removed the '@'
                    {
                        foreach (var child in ItemCategory.Registry.Values.Where(cat => cat.ParentID == parentID))
                        {
                            // set name to "ParentName - ThisName"
                            // e.g. "Arrows - Endless"
                            var subcat = new MatchCategory($"{child.Name} - {name}", ++_currentCount, child.ID, priority, reqs, excls);
                            subcat.Enabled = enable;

                            assignSortingRules(subcat, sort_fields);
                            CategoryDefinitions[subcat.Name] = subcat;
                        }
                        // no "include" allowed for this type
                        return;
                    }

                    var newcategory = new MatchCategory(name, ++_currentCount, parentID, priority, reqs, excls);
                    newcategory.Enabled = enable;

                    // create/get the Sorting Rules for the category
                    assignSortingRules(newcategory, sort_fields);

                    // store the new category in the collections
                    CategoryDefinitions[newcategory.Name] = newcategory;
                    new_category = newcategory;
                }
            }
            else
            {
                // if "requires" and "union" were null or empty, consider this
                // a "Container" category
                var new_container = new ContainerCategory(name, ++_currentCount, parentID, priority);
                new_container.Enabled = enable;
                assignSortingRules(new_container, sort_fields);

                CategoryDefinitions[new_container.Name] = new_container;

                new_category = new_container;
            }

            // finally handle the "include" entry
            if (new_category != null && include != null)
            {
                foreach (var minicat in include)
                    parseMiniCategory(new_category.ID, minicat.Key, minicat.Value);
            }
        }


        /*
        ██ ███    ██  ██████ ██      ██    ██ ██████  ███████ ███████
        ██ ████   ██ ██      ██      ██    ██ ██   ██ ██      ██
        ██ ██ ██  ██ ██      ██      ██    ██ ██   ██ █████   ███████
        ██ ██  ██ ██ ██      ██      ██    ██ ██   ██ ██           ██
        ██ ██   ████  ██████ ███████  ██████  ██████  ███████ ███████
        */

        private static void parseInclude(int parent_id, JsonObject include)
        {
            if (include == null) return;

            foreach (var minicat in include)
                parseMiniCategory(parent_id, minicat.Key, minicat.Value);
        }

        /// for parsing the "mini" categories inside an include: block
        /// parent_id is the id number of the category containing the include: block
        private static void parseMiniCategory(int parent_id, string category_name, JsonValue value)
        {
            IDictionary<string, int> requirements;
            IDictionary<string, int> exclusions;

            if (value.JsonType == JsonType.String)
            {
                // category requirements are held in the single string value for the key
                if (parseRequirements(new[] { value.Qs() }, out requirements, out exclusions))
                {
                    var newcategory = new MatchCategory(category_name, ++_currentCount, parent_id, 0, requirements, exclusions);
                    newcategory.Enabled = true;

                    // assign sorting rules from parent
                    newcategory.CopyParentRules();

                    CategoryDefinitions[newcategory.Name] = newcategory;
                }
            }

            // if it's an object, could contain "requires" list and/or nested "include"
            else if (value.JsonType == JsonType.Object)
            {
                var minicatobj = value.Qo();

                Sorter newcategory;
                if (minicatobj.ContainsKey("requires") && parseRequirements(getRequirementLines(minicatobj["requires"]), out requirements, out exclusions))
                {
                    newcategory = new MatchCategory(category_name, ++_currentCount, parent_id, 0, requirements, exclusions);
                    newcategory.Enabled = true;

                    // assign sorting rules from parent
                    newcategory.CopyParentRules();

                    CategoryDefinitions[newcategory.Name] = newcategory;
                }
                else
                {
                    newcategory = new ContainerCategory(category_name, ++_currentCount, parent_id);
                    newcategory.Enabled = true;
                    // assign sorting rules from parent
                    newcategory.CopyParentRules();
                    CategoryDefinitions[newcategory.Name] = newcategory;
                }
                // recursively perform this same process for any members of a nested "include" block
                if (minicatobj.ContainsKey("include"))
                    parseInclude(newcategory.ID, minicatobj["include"]?.Qo());
            }
            else
                throw new MalformedFieldError($"Could not parse 'include' entry '{category_name}' for category '{_currentCategory}'");
        }


        /*
        ███████  ██████  ██████  ████████     ██████  ██    ██ ██      ███████ ███████
        ██      ██    ██ ██   ██    ██        ██   ██ ██    ██ ██      ██      ██
        ███████ ██    ██ ██████     ██        ██████  ██    ██ ██      █████   ███████
             ██ ██    ██ ██   ██    ██        ██   ██ ██    ██ ██      ██           ██
        ███████  ██████  ██   ██    ██        ██   ██  ██████  ███████ ███████ ███████
        */

        private static void assignSortingRules(Sorter category, JsonArray property_names)
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

        private static void addUnionMembers(UnionCategory union, JsonArray member_names)
        {
            foreach (var member in member_names)
            {
                // add each category listed under "merge"
                // to the union. When the union is enabled,
                // these categories will be notified.
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

        /// if the requires list is null, return an empty list;
        /// if the requires "list" is just a single string, return a list containing that string.
        /// otherwise return the list of strings found in the array.
        /// If the "requires" value is neither null, string, nor array, throw MalformedFieldError
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

        /// Get the tokenizer to analyze each line in the requires_list and return an object containing any found
        /// requirements or exclusions. The values for each line are accumulated and placed into the out parameters
        /// as appropriate. Returns true if any requirements or exclusions were found, false if for some reason none
        /// were (e.g. the requires_list was empty)
        private static bool parseRequirements(IEnumerable<string> requires_list, out IDictionary<string, int> requirements, out IDictionary<string, int> exclusions)
        {
            var reqs = new Dictionary<string, int>();
            var excls = new Dictionary<string, int>();

            foreach (var line in requires_list)
            {
                try
                {
                    var entry = Tokenizer.ParseRequirementLine(line);

                    var trait_group = entry.TraitGroup;

                    foreach (var trait in entry.includes)
                        reqs.Add(trait, getValueForFlag(trait_group, trait));

                    foreach (var trait in entry.excludes)
                        excls.Add(trait, getValueForFlag(trait_group, trait));

                }
                catch (TokenizerException e)
                {
                    throw new MalformedFieldError($"Could not parse requirement entry '{e.Line}' for category '{_currentCategory}'");
                }
            }

            requirements = reqs.Count > 0 ? reqs : null;
            exclusions = excls.Count > 0 ? excls : null;

            return (reqs.Count + excls.Count) > 0;
        }

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
                r.category.Ordinal = range_end;

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
