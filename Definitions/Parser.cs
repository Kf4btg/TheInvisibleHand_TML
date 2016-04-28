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

        /// I hope 65,000 categories will be enough...
        public static IDictionary<ushort, ItemCategory> CategoryIDs { get; private set; }
        // public static SortedAutoTree<string, ItemCategory> CategoryTree { get; private set; }

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
        private static void LoadGroup(KeyValuePair<string, JsonValue> group_object, string name_prefix="")
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
                    flagGroup[trait] = 1 << (flagGroup.Count-1);

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
            CategoryIDs = new Dictionary<ushort, ItemCategory>();
            ushort count=0; // track absolute order of added categories (this will be the unique ID for each category)

            foreach(var res in assembly.GetManifestResourceNames().Where(n=>n.StartsWith(category_resources_path)).OrderBy(n=>n))
            {
                using (Stream s = assembly.GetManifestResourceStream(res))
                {
                    // read in the file stream and turn into an array of JsonObjects where each is a Category Definition
                    var CatObjList = HjsonValue.Load(s).Qa();

                    foreach (var catobj in CatObjList)
                    {
                        // get object parts
                        var catdef = new
                        {
                            name = catobj.ContainsKey("name") ? catobj["name"].Qs() : "",
                            parent_name = catobj.ContainsKey("parent") ? catobj["parent"]?.Qs() : null,

                            // whether to activate this category
                            enable = catobj.ContainsKey("enable") ? catobj["enable"].Qb() : true,

                            // restrict assignable priority value to [-500..500] and shift left by 6 bits
                            // to give us some guaranteed tweaking room between the priorities
                            priority = catobj.ContainsKey("priority") ? (short?)(catobj["priority"].Qi().Clamp(-500, 500) << 6) : null,

                            // the required traits for this category
                            requires = catobj.ContainsKey("requires") ? catobj["requires"].Qo() : null,

                            // any merged categories
                            merge = catobj.ContainsKey("merge") ? catobj["merge"].Qa() : null,

                            // ordered list of Item fields on which to sort items in this category
                            sort_fields = catobj.ContainsKey("sort") ? catobj["sort"].Qa() : null,
                        };

                        // and placeholder values
                        ItemCategory parent = null;
                        short priority;// = catdef.priority ?? 0;

                        // /////////////////////////////////
                        // name field is always required //
                        // /////////////////////////////////

                        if (catdef.name == String.Empty)
                            throw new HjsonFieldNotFoundException("name", nameof(catobj));

                        // //////////////////////
                        // get parent, if any //
                        // //////////////////////
                        if (catdef.parent_name != null)
                        {
                            try
                            {
                                parent = CategoryDefinitions[catdef.parent_name];

                                // child categories inherit the priority of their parent.
                                // each child level decreases the initial priority by 1;
                                // we SUBTRACT the depth from the priority to make sure that more specific categories are
                                // sorted first; for example, if "Weapon" has a priority of 500:
                                //      "Weapon.Melee.Broadsword" {P=498} < "Weapon.Melee" {P=499} < "Weapon.Throwing" {P=499} < "Weapon" {P=500}
                                // Note: weapon.melee is sorted before weapon.throwing (even though they have the same priority)
                                // because the weapon.melee category is loaded before (has a lower ID than) weapon.throwing.
                                //
                                // FIXME: under this scheme, sorting can go:
                                //      Weapon.Melee.Broadsword > Weapon.Magic.Homing > Weapon.Melee > Weapon.Magic
                                // which is neither ideal nor intuitive; seems we need some sort of depth-first approach?

                                // an explicitly-set priority overrides default or inherited value
                                priority = catdef.priority ?? (short)(parent.Priority - 1);
                            }
                            catch (KeyNotFoundException knfe)
                            {
                                throw new UsefulKeyNotFoundException(catdef.parent_name, nameof(CategoryDefinitions), knfe,
                                    "Category '" +catdef.name + "': The specified parent category '{0}' was not found in '{1}'."
                                );
                            }
                        }
                        else
                        {
                            priority = catdef.priority ?? 0;
                        }

                        // A union category
                        if (catdef.merge != null)
                        {
                            // bool is_enabled = catobj.ContainsKey("enabled") ? catobj["enabled"].Qb() : true;

                            var union = new UnionCategory(catdef.name, count++, parent, priority: priority);

                            // TODO: allow enable/disable at runtime
                            if (catdef.enable)
                            {
                                foreach (var member in catdef.merge)
                                {
                                    // let each category listed under "merge"
                                    // know which wrapper it has been assigned to
                                    try
                                    {
                                        CategoryDefinitions[member].Merge(union);
                                    }
                                    catch (KeyNotFoundException e)
                                    {
                                        //FIXME: use ErrorLogger
                                        Console.WriteLine("{0}: {1}", member, e.Message);
                                    }
                                }
                            }
                            CategoryDefinitions[catdef.name] = union;
                        }

                        // a 'Regular' category
                        else if (catdef.requires != null)
                        {
                            // var reqs = new Dictionary<string, int>();

                            // //////////////////////
                            // parse requirements //
                            // //////////////////////
                            var reqs = parseRequirements(catdef.name, catdef.requires);

                            // if, somehow, there are no requirements, don't bother adding to list
                            if (reqs.Count > 0)
                            {
                                var newcategory = new RegularCategory(catdef.name, count++, reqs, parent, priority);

                                // //////////////////////////////////////////////////////
                                // Now, create/get the Sorting Rules for the category //
                                // //////////////////////////////////////////////////////

                                if (catdef.sort_fields != null)
                                    newcategory.BuildSortRules(catdef.sort_fields.Select(jv => jv.Qs()));

                                else if (parent != null) // inherit from parent
                                    newcategory.CopySortRules(parent);
                                // newcategory.ruleExpressions = p?.ruleExpressions;

                                // if the rules are still null, add a default rule of just sorting by type
                                if (newcategory.SortRules == null)
                                    newcategory.BuildSortRules(new[] {"type"}); // default

                                // if (newcategory.ruleExpressions == null)
                                // {
                                //     Console.WriteLine($"{category_name}: ruleExpressions is null");
                                //     if (newcategory.SortRules == null)
                                //         Console.WriteLine($"{category_name}: SortRules is null");
                                //
                                // }
                                // else
                                    // ConsoleHelper.PrintList(newcategory.ruleExpressions.Select(ex=>ex.ToString()), category_name, true);

                                // store the new category in the collections
                                CategoryDefinitions[newcategory.Name] = CategoryIDs[newcategory.ID] = newcategory;
                            }
                        }
                    } // end of category-object list
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
            // var cattree = new SortedAutoTree<string, ItemCategory>() { Label = "root" };
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
                    // subtree = subtree[catstack.Pop().Name];
                }

                // now [create the child]/[set its data if it
                // has already been created by a previous operation]
                // subtree[category.Name].Data = category;
                subtree[category.Ordinal].Data = category;
            }

            CategoryTree = cattree;
        }
    }
}
