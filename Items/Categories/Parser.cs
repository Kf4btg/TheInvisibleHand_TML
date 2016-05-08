using System;
using System.Linq;
// using System.Linq.Expressions;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Hjson;

namespace InvisibleHand.Items.Categories
{
    /// read in the category specs from the hjson files and convert to something useable
    public static class Parser
    {
        private static string TraitFilePath;
        private static string CategoryDefsPath;

        private static Assembly assembly;

        public static IDictionary<string, IList<string>> TraitDefinitions { get; private set; }
        // public static IDictionary<string, IDictionary<string, int>> FlagCollection { get; private set; }

        public static IDictionary<string, ItemCategory> CategoryDefinitions { get; private set; }

        /// stores the ids of enabled, non-union categories
        public static ISet<int> ActiveCategories { get; private set; }

        // public static SortedAutoTree<int, ItemCategory> CategoryTree { get; private set; }

        /// Call this method to run all the other class methods
        // public static void Parse(string category_dir = "Definitions/Categories", string trait_file = "Definitions/Traits/0-All.hjson")
        public static void Parse(string category_path = "Definitions.Categories", string trait_path = "Definitions.Traits.0-All.hjson")
        {
            assembly = Assembly.GetExecutingAssembly();

            CategoryDefsPath = "InvisibleHand." + category_path;
            TraitFilePath = "InvisibleHand." + trait_path;

            // the order is important here
            LoadTraitDefinitions(TraitFilePath);
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
                LoadTraitDefinitions(TraitFilePath);

            // FlagCollection = new Dictionary<string, IDictionary<string, int>>();

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
            ActiveCategories = new HashSet<int>();
            int count = 0; // track absolute order of added categories (this will be the unique ID for each category)

            foreach (var res in assembly.GetManifestResourceNames().Where(n => n.StartsWith(category_resources_path)).OrderBy(n => n))
            {
                using (Stream s = assembly.GetManifestResourceStream(res))
                {
                    // read in the file stream and turn into an array of JsonObjects where each is a Category Definition
                    var CatObjList = HjsonValue.Load(s).Qa();

                    foreach (var catobj in CatObjList)
                    {
                        // get object parts
                        // ------------------
                        var catdef = new
                        {
                            // each part contains a null? check to avoid breakage at this step

                            name = catobj.ContainsKey("name") ? catobj["name"]?.Qs() ?? "" : "",
                            parent_name = catobj.ContainsKey("parent") ? catobj["parent"]?.Qs() ?? "" : "",
                            inherits = catobj.ContainsKey("inherit") ? catobj["inherit"]?.Qs() ?? "" : "",

                            // whether to activate this category (DEFAULT: true)
                            enable = catobj.ContainsKey("enable") ? catobj["enable"]?.Qb() ?? true : true,

                            // priority is now just a 'weight', used to modify the order of categories with regards to
                            // their siblings. There's no more bit shifting or anything, and the -500..500 limit is now
                            // entirely arbitrary (though I might keep it just so people don't get...too ambitious)
                            priority = catobj.ContainsKey("priority") ? catobj["priority"]?.Qi().Clamp(-500, 500) : null,

                            // the required traits for this category
                            requires = catobj.ContainsKey("requires") ? catobj["requires"]?.Qo() : null,

                            // any merged categories
                            merge = catobj.ContainsKey("merge") ? catobj["merge"]?.Qa() : null,

                            // any include child categories
                            include = catobj.ContainsKey("include") ? catobj["include"]?.Qo() : null,

                            // ordered list of Item fields on which to sort items in this category
                            sort_fields = catobj.ContainsKey("sort") ? catobj["sort"]?.Qa() : null,
                        };

                        // name field is always required
                        if (catdef.name == String.Empty)
                            throw new HjsonFieldNotFoundException("name", nameof(catobj));

                        // get parent, if any
                        int parentID = getParentID(catdef.name, catdef.parent_name);

                        // and priority, either specific to this category (or default 0)
                        int priority = catdef.priority ?? 0;

                        // A union category
                        // ------------------
                        if (catdef.merge != null)
                        {
                            var union = new UnionCategory(catdef.name, ++count, parentID, priority: priority);

                            // TODO: allow enable/disable at runtime
                            // if (catdef.enable)
                            addUnionMembers(union, catdef.merge);

                            union.Enabled = catdef.enable;

                            CategoryDefinitions[union.Name] = union;
                            // XXX: should we add the unions to the search tree?
                            // ANSWER: No.
                            // if (catdef.enable) ActiveCategories.Add(union.ID);
                        }

                        // a 'Regular' category
                        // ------------------
                        else if (catdef.requires != null)
                        {
                            // parse requirements
                            // ------------------
                            IDictionary<string, int> reqs;
                            IDictionary<string, int> excls;

                            // if, somehow, there are no requirements, don't bother adding to list
                            if (parseRequirements(catdef.name, catdef.requires, out reqs, out excls))
                            {
                                // otherwise, create the new category object
                                var newcategory = new RegularCategory(catdef.name, ++count, parentID, priority, reqs, excls);

                                newcategory.Enabled = catdef.enable;


                                // create/get the Sorting Rules for the category
                                // ---------------------------------------------
                                assignSortingRules(newcategory, catdef.sort_fields);

                                // store the new category in the collections
                                CategoryDefinitions[newcategory.Name] = newcategory;
                                // if (catdef.enable) ActiveCategories.Add(newcategory.ID);

                            }
                        }
                    } // end of category-object list
                }
            }
        }

        /// for parsing the "mini" categories inside an include: block
        private static void parseMiniCategories(JsonObject include)
        {
            foreach (var minicat in include)
            {
                var cat_name = minicat.Key;

                Dictionary<string, int> requires;
                if (minicat.Value.JsonType == JsonType.String)
                {
                    // category requirements are held in the single string value for the key
                    requires = new Dictionary<string, int>();
                }
                else if (minicat.Value.JsonType == JsonType.Object)
                {

                }

                // requires = minicat.Value.JsonType == JsonType.String ? minicat.Value.Qs() : "";

                var catdef = new
                {
                    name = minicat.Key,
                    requires = minicat.Value.JsonType == JsonType.String ? minicat.Value.Qs() : "",
                };
            }
        }

        private static void parseRequirementString(string requirements)
        {
            
        }


        /*
        ███████  ██████  ██████  ████████     ██████  ██    ██ ██      ███████ ███████
        ██      ██    ██ ██   ██    ██        ██   ██ ██    ██ ██      ██      ██
        ███████ ██    ██ ██████     ██        ██████  ██    ██ ██      █████   ███████
             ██ ██    ██ ██   ██    ██        ██   ██ ██    ██ ██      ██           ██
        ███████  ██████  ██   ██    ██        ██   ██  ██████  ███████ ███████ ███████
        */

        private static void assignSortingRules(RegularCategory category, JsonArray property_names)
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

        private static void addUnionMembers(IUnion<ItemCategory> union, JsonArray member_names)
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

        private static bool parseRequirements(string category_name, JsonObject requires_obj, out IDictionary<string, int> requirements, out IDictionary<string, int> exclusions)
        {
            // temp containers
            var reqs = new Dictionary<string, int>();
            var excls = new Dictionary<string, int>();

            // keep track of whether any requires were even listed with the definition
            bool requirements_found = false;

            // iterate through listed traits
            foreach (var newreqs in requires_obj)
            {
                var traitCategory = newreqs.Key;
                // FlagCollection[TraitCategory][TraitName]

                IDictionary<string, int> flagvalues;

                // using try-catch instead of TryGetValue because I'm not *expecting*
                // the flag to be missing; therefore, under normal circumstances, the
                // try block should always succeed and we don't have to worry about the
                // performance difference between catch() && TryGetValue
                try
                {
                    flagvalues = IHBase.FlagCollection[traitCategory];
                }
                catch (KeyNotFoundException knfe)
                {
                    throw new UsefulKeyNotFoundException(
                        traitCategory,
                        nameof(IHBase.FlagCollection),
                        knfe,
                        "Category '" +category_name + "': the requested Trait Category '{0}' is not present in '{1}'."
                    );
                }

                // go through the array of traits, add the appropriate flag value
                foreach (string trait_name in newreqs.Value.Qa())
                {
                    // copy the trait name because we might be modifying it
                    var use_name = trait_name;
                    bool exclude = false;
                    // check if we're inverting a value
                    if (trait_name[0] == '!')
                    {
                        exclude = true;
                        // remove the exclamation mark and any additional whitespace from the trait name
                        use_name = trait_name.Trim(new[] { '!', ' ' });
                    }

                    int flagvalue;
                    try
                    {
                        flagvalue = flagvalues[use_name];
                    }
                    catch (KeyNotFoundException knfe)
                    {
                        throw new UsefulKeyNotFoundException(
                            use_name,
                            nameof(IHBase.FlagCollection)+"["+traitCategory+"]",
                            knfe,
                            "Category '" +category_name + "': the specified required trait '{0}' is not present in '{1}'."
                        );
                    }
                    // now OR in the required value to the appropriate container

                    if (exclude) // if this is a "!trait" requirement
                    {
                        requirements_found = true;

                        if (!excls.ContainsKey(traitCategory))
                            excls[traitCategory] = 0;

                        excls[traitCategory] |= flagvalue;
                    }
                    else
                    {
                        requirements_found = true;
                        // initialize the value for this trait type if it has not been seen before
                        if (!reqs.ContainsKey(traitCategory))
                            reqs[traitCategory] = 0;
                        reqs[traitCategory] |= flagvalue;
                    }
                }
            }
            requirements = reqs.Count > 0 ? reqs : null;
            exclusions = excls.Count > 0 ? excls : null;

            return requirements_found;
        }

        /// <summary>
        /// Get the id of the pre-existing parent of the given category
        /// </summary>
        /// <param name="category_name">Name of current category (only used in error messages if the parent name is not found) </param>
        /// <param name="parent_name"> name pulled from the JsonObject, or String.Empty if the 'parent' field was missing or null</param>
        /// <returns> The parent with the given name or `null` if String.Empty is passed for parent_name</returns>
        private static int getParentID(string category_name, string parent_name)
        {
            int pid = 0;
            try
            {
                pid = parent_name == String.Empty ? 0 : CategoryDefinitions[parent_name].ID;

            }
            catch (KeyNotFoundException knfe)
            {
                throw new UsefulKeyNotFoundException(parent_name, nameof(CategoryDefinitions), knfe,
                    "Category '" + category_name + "': The specified parent category '{0}' was not found in '{1}'."
                );
            }

            while (pid > 0 && !ItemCategory.ActiveCategories.Contains(pid))
            {
                // the parent of this category has been deactivated, so we need to 'reparent':
                // keep moving the child up a level until it has an active parent or becomes
                // a toplevel category
                pid = ItemCategory.Registry[pid].ParentID;
            }

            // return parent_name == "" ? 0 : CategoryDefinitions[parent_name].ID;

            return pid;
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
            var lookup_byparentID = CategoryDefinitions.Select(kvp => kvp.Value).ToLookup(c=>c.ParentID, c=> c);
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
            foreach (var r in recipients.Select((x, i) => new { category = x, index = i }))
            {
                var range_start = min_address + (r.index * bucket_size);
                var range_end = range_start + bucket_size - 1;

                // each category will receive an address that is just 1 below the minimum range of it's sibling.
                r.category.Ordinal = range_end;

                // If this child is itself a parent, recursively call this function for its children
                if (child_category_lookup.Contains(r.category.ID))
                    // subtract 1 more from the end of the range so we don't try to overwrite this category
                    assignAddresses(range_start, range_end-1, r.category.ID, child_category_lookup);
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
            // create the root of the tree; the label here is unimportant. All other labels
            // will be created automatically during autovivification, using the ordinal value of the category
            // var cattree = new SortedAutoTree<int, ItemCategory>() { Label = 0 };

            var registry = ItemCategory.Registry;

            // foreach (var kvp in registry)

            // only add enabled categories to the tree
            foreach (var active_id in ItemCategory.ActiveCategories)
            {
                // var category = kvp.Value;
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
