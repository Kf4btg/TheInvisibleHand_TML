using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Hjson;

// using Terraria.ModLoader;

using InvisibleHand.Utils;

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

        // public static IDictionary<string, IDictionary<string, int>> CategoryDefinitions { get; private set; }
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
                    var CatObjList = HjsonValue.Load(s).Qa();

                    foreach (var catobj in CatObjList)
                    {
                        // name field is always required
                        string category_name = catobj["name"].Qs();

                        ItemCategory parent = null;
                        short priority = 0;

                        // get parent, if any
                        if (catobj.ContainsKey("parent"))
                        {
                            string parent_name = catobj["parent"]?.Qs();
                            if (parent_name != null)
                            {
                                // TODO: don't fail on malformed categories/missing parents,
                                // but log the error somewhere
                                parent = CategoryDefinitions[parent_name];

                                // child categories inherit the priority of their parent.
                                // each child level decreases the initial priority by 1;
                                // we SUBTRACT the depth from the priority to make sure that more specific categories are
                                // sorted first; for example, if "Weapon" has a priority of 500:
                                //      "Weapon.Melee.Broadsword" {P=498} < "Weapon.Melee" {P=499} < "Weapon.Throwing" {P=499} < "Weapon" {P=500}
                                // Note: weapon.melee is sorted before weapon.throwing (even though they have the same priority)
                                // because the weapon.melee category is loaded before (has a lower ID than) weapon.throwing.

                                priority = (short)(parent.Priority - 1);
                            }
                        }

                        var reqs = new Dictionary<string, int>();


                        // an explicitly-set priority overrides default or inherited value
                        if (catobj.ContainsKey("priority"))
                        {
                            // restrict assignable value to [-325..325] and multiply by 100
                            // to give us some guaranteed tweaking room between the priorities
                            priority = (short)(catobj["priority"].Qi().Clamp(-325, 325)*100);
                        }

                        if (catobj.ContainsKey("requires"))
                        {
                            foreach (var newreqs in catobj["requires"].Qo())
                            {
                                var traitCategory = newreqs.Key;
                                // FlagCollection[TraitCategory][TraitName]
                                var flagvalues = FlagCollection[traitCategory];

                                if (!reqs.ContainsKey(traitCategory))
                                    reqs[traitCategory] = 0;

                                // go through the array of traits, add the appropriate flag value
                                foreach (string trait_name in newreqs.Value.Qa())
                                    reqs[traitCategory] |= flagvalues[trait_name];
                            }

                            // if, somehow, there are no requirements, don't bother adding to list
                            if (reqs.Count > 0)
                            {
                                var newcategory = new ItemCategory(category_name, count++, reqs, parent, priority);

                                CategoryDefinitions[newcategory.Name] = CategoryIDs[newcategory.ID] = newcategory;
                            }
                        }
                        else if (catobj.ContainsKey("merge"))
                        {
                            var merge_wrapper = new ItemCategory(category_name, count++, parent, is_merge_wrapper: true, priority: priority);

                            foreach (var wrapped in catobj["merge"].Qa())
                            {
                                // let each category listed under "merge"
                                // know which wrapper it has been assigned to
                                try
                                {
                                    CategoryDefinitions[wrapped].Merge(merge_wrapper);
                                }
                                catch (KeyNotFoundException e)// e)
                                {
                                    //FIXME: use ErrorLogger
                                    Console.WriteLine(e.Message);
                                }
                            }
                            CategoryDefinitions[category_name] = merge_wrapper;
                        }
                    } // end of category-object list
                }
            }

            // Console.WriteLine("{0}, {1}", CategoryDefinitions.Count, string.Join(",\n", CategoryDefinitions.Select(kv=>kv.Key).ToArray()));
        }

        /// After reading in all of the category definitions, build a tree structure based on the parent-child relationships
        /// between the categories. Traversing the tree structure when testing an item's traits will be far more efficient than
        /// checking each category individually.
        /// NOTE: if the ordering of the categories changes (i.e. if changing priorities is allowed at runtime),
        /// the tree will need to be rebuilt each time to reflect the changes.
        private static void BuildCategoryTree()
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
