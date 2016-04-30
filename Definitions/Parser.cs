using System;
using System.Linq;
// using System.Linq.Expressions;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Hjson;
// using Terraria;

using InvisibleHand.Utils;
using InvisibleHand.Items;

namespace InvisibleHand.Definitions
{
    /// read in the category specs from the hjson files and convert to something useable
    public static class CategoryParser
    {
        private static string TraitFilePath;
        private static string CategoryDefsPath;

        private static Assembly assembly;

        public static IDictionary<string, IList<string>> TraitDefinitions { get; private set; }
        public static IDictionary<string, IDictionary<string, int>> FlagCollection { get; private set; }

        public static IDictionary<string, ItemCategory> CategoryDefinitions { get; private set; }

        /// uses keys based on a category's ordinal (ordering rank).
        public static SortedAutoTree<int, ItemCategory> CategoryTree { get; private set; }


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
            CalculatePriorities();
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

            FlagCollection = new Dictionary<string, IDictionary<string, int>>();

            foreach (var tgroup in TraitDefinitions)
            {
                var flagGroup = new Dictionary<string, int>();

                // always initialize with a "none"
                flagGroup["none"] = 0;
                foreach (var trait in tgroup.Value)
                    // each new value added is a (binary) order of magnitude larger than the last
                    flagGroup[trait] = 1 << (flagGroup.Count - 1);

                // add flag group to collection
                FlagCollection[tgroup.Key] = flagGroup;
            }
        }

        /*
        ██       ██████   █████  ██████       ██████  █████  ████████ ███████  ██████   ██████  ██████  ██ ███████ ███████
        ██      ██    ██ ██   ██ ██   ██     ██      ██   ██    ██    ██      ██       ██    ██ ██   ██ ██ ██      ██
        ██      ██    ██ ███████ ██   ██     ██      ███████    ██    █████   ██   ███ ██    ██ ██████  ██ █████   ███████
        ██      ██    ██ ██   ██ ██   ██     ██      ██   ██    ██    ██      ██    ██ ██    ██ ██   ██ ██ ██           ██
        ███████  ██████  ██   ██ ██████       ██████ ██   ██    ██    ███████  ██████   ██████  ██   ██ ██ ███████ ███████
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

                            // whether to activate this category
                            enable = catobj.ContainsKey("enable") ? catobj["enable"]?.Qb() ?? true : true,

                            // priority is now just a 'weight', used to modify the order of categories with regards to
                            // their siblings. There's no more bit shifting or anything, and the -500..500 limit is now
                            // entirely arbitrary (though I might keep it just so people don't get...too ambitious)
                            priority = catobj.ContainsKey("priority") ? catobj["priority"]?.Qi().Clamp(-500, 500) : null,

                            // the required traits for this category
                            requires = catobj.ContainsKey("requires") ? catobj["requires"]?.Qo() : null,

                            // any merged categories
                            merge = catobj.ContainsKey("merge") ? catobj["merge"]?.Qa() : null,

                            // ordered list of Item fields on which to sort items in this category
                            sort_fields = catobj.ContainsKey("sort") ? catobj["sort"]?.Qa() : null,
                        };

                        // name field is always required
                        if (catdef.name == String.Empty)
                            throw new HjsonFieldNotFoundException("name", nameof(catobj));

                        // get parent, if any
                        // ItemCategory parent = getParent(catdef.name, catdef.parent_name);
                        int parentID = getParentID(catdef.name, catdef.parent_name);
                        // ItemCategory parent = parentID > 0 ? ItemCategory.Registry[parentID] : null;


                        // and priority, either specific to this category or inherited from parent (or default 0)
                        // int priority = getPriority(parent, catdef.priority);
                        int priority = catdef.priority ?? 0;

                        // A union category
                        // ------------------
                        if (catdef.merge != null)
                        {
                            // bool is_enabled = catobj.ContainsKey("enabled") ? catobj["enabled"].Qb() : true;

                            var union = new UnionCategory(catdef.name, ++count, parentID, priority: priority);

                            // TODO: allow enable/disable at runtime
                            if (catdef.enable)
                                mergeUnionMembers(union, catdef.merge);

                            // XXX: should we add the unions to the search tree?
                            CategoryDefinitions[union.Name] = union;
                        }

                        // a 'Regular' category
                        // ------------------
                        else if (catdef.requires != null)
                        {
                            // parse requirements
                            // ------------------
                            var reqs = parseRequirements(catdef.name, catdef.requires);

                            // if, somehow, there are no requirements, don't bother adding to list
                            if (reqs.Count > 0)
                            {
                                // otherwise, create the new category object
                                var newcategory = new RegularCategory(catdef.name, ++count, reqs, parentID, priority);

                                // create/get the Sorting Rules for the category
                                // ---------------------------------------------
                                assignSortingRules(newcategory, catdef.sort_fields);

                                // store the new category in the collections
                                CategoryDefinitions[newcategory.Name] = newcategory;
                            }
                        }
                    } // end of category-object list
                }
            }
        }

        private static void assignSortingRules(RegularCategory category, JsonArray property_names)
        {
            if (property_names != null)
                category.SortRules = ItemRuleBuilder.BuildSortRules(property_names.Select(jv => jv.Qs()));

            else if (category.Parent != null) // inherit from parent
                category.CopySortRules(category.Parent);
                // newcategory.ruleExpressions = p?.ruleExpressions;

            // if the rules are somehow *still* null, add a default single-rule list of just sorting by type
            if (category.SortRules == null)
                // (should just pull the rule from its cache in the vast majority of cases)
                category.SortRules = new[] { ItemRuleBuilder.GetRule("type") }.ToList();
        }

        private static void mergeUnionMembers(UnionCategory union, JsonArray member_names)
        {
            foreach (var member in member_names)
            {
                // let each category listed under "merge"
                // know which wrapper it has been assigned to
                try
                {
                    CategoryDefinitions[member].Merge(union);
                }
                catch (KeyNotFoundException knfe)
                {
                    throw new UsefulKeyNotFoundException(
                        member,
                        nameof(CategoryDefinitions),
                        knfe,
                        "UnionCategory '" +union.Name + "': member category '{0}' could not be found in '{1}' for inclusion."
                    );
                    //FIXME: use ErrorLogger
                    // Console.WriteLine("{0}: {1}", member, e.Message);
                }
            }
        }

        private static Dictionary<string, int> parseRequirements(string category_name, JsonObject requires_obj)
        {
            // parse requirements
            var reqs = new Dictionary<string, int>();

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
                    flagvalues = FlagCollection[traitCategory];
                }
                catch (KeyNotFoundException knfe)
                {
                    throw new UsefulKeyNotFoundException(
                        traitCategory,
                        nameof(FlagCollection),
                        knfe,
                        "Category '" +category_name + "': the requested Trait Category '{0}' is not present in '{1}'."
                    );
                }

                if (!reqs.ContainsKey(traitCategory))
                    reqs[traitCategory] = 0;

                // go through the array of traits, add the appropriate flag value
                foreach (string trait_name in newreqs.Value.Qa())
                {
                    try
                    {
                        reqs[traitCategory] |= flagvalues[trait_name];
                    }
                    catch (KeyNotFoundException knfe)
                    {
                        throw new UsefulKeyNotFoundException(
                            trait_name,
                            nameof(FlagCollection)+"["+traitCategory+"]",
                            knfe,
                            "Category '" +category_name + "': the specified required trait '{0}' is not present in '{1}'."
                        );
                    }
                }
            }
            return reqs;
        }

        /// <summary>
        /// Get the id of the pre-existing parent of the given category
        /// </summary>
        /// <param name="category_name">Name of current category (only used in error messages if the parent name is not found) </param>
        /// <param name="parent_name"> name pulled from the JsonObject, or String.Empty if the 'parent' field was missing or null</param>
        /// <returns> The parent with the given name or `null` if String.Empty is passed for parent_name</returns>
        private static int getParentID(string category_name, string parent_name)
        {
            try
            {
                return parent_name == "" ? 0 : CategoryDefinitions[parent_name].ID;
            }
            catch (KeyNotFoundException knfe)
            {
                throw new UsefulKeyNotFoundException(parent_name, nameof(CategoryDefinitions), knfe,
                    "Category '" + category_name + "': The specified parent category '{0}' was not found in '{1}'."
                );
            }
        }

        /// <summary>
        /// Because it doesn't seem possible to assign proper priority values before all categories are loaded and
        /// parent-child relationships are known, we do that afterwards when we know all the context.
        /// </summary>
        private static void CalculatePriorities()
        {
            // creates a lookup of: ParentID => [collection of child categories]
            var lookup_byparentID = CategoryDefinitions.Select(kvp => kvp.Value).ToLookup(c=>c.ParentID, c=> c);
            // assign addresses to all the top level categories, and recursively to their children
            assignAddresses(0, int.MaxValue, 0, lookup_byparentID);

            // TESTING
            ConsoleHelper.PrintList(CategoryDefinitions.Select(kvp=>kvp.Value).OrderBy(c=>c.Priority).Select(c=> new {name=c.QualifiedName, order=c.Priority}), "Categories in order", true);
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
            var cattree = new SortedAutoTree<int, ItemCategory>() { Label = 0 };

            foreach (var kvp in CategoryDefinitions)
            {
                var category = kvp.Value;

                var parent = category.Parent;

                // get the ancestors for the category as a stack,
                // with the top-level category on top.
                var catstack = new Stack<ItemCategory>();
                while (parent != null)
                {
                    catstack.Push(parent);
                    parent = parent.Parent;
                }
                var subtree = cattree;

                // descend from the root down the parent-stack to the
                // proper depth of the child, auto-creating(vivifying!)
                // any non-existent nodes.
                while (catstack.Count > 0)
                {
                    subtree = subtree[catstack.Pop().Ordinal];
                }

                // now create/access the child and set its data
                subtree[category.Ordinal].Data = category;
            }

            CategoryTree = cattree;
        }
    }
}
