using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Hjson;

using InvisibleHand.Utils;

namespace InvisibleHand.Definitions
{
    /// read in the category specs from the hjson files and convert to something useable
    public static class CategoryParser
    {
        private static string TraitFilePath;
        private static string CategoryDefsPath;

        public static IDictionary<string, IList<string>> TraitDefinitions { get; private set; }
        public static IDictionary<string, IDictionary<string, int>> FlagCollection { get; private set; }

        // public static IDictionary<string, IDictionary<string, int>> CategoryDefinitions { get; private set; }
        public static IDictionary<string, ItemCategory> CategoryDefinitions { get; private set; }
        public static SortedAutoTree<string, ItemCategory> CategoryTree { get; private set; }


        /// Call this method to run all the other class methods
        public static void Parse(string category_dir = "Definitions/Categories", string trait_file = "Definitions/Traits/0-All.hjson")
        {
            CategoryDefsPath = category_dir;
            TraitFilePath = trait_file;

            // the order is important here
            LoadTraitDefinitions();
            AssignFlagValues();
            LoadCategoryDefinitions();
            BuildCategoryTree();
        }

        /// Read in Traits.hjson and organize the Traits by family name;
        /// (a trait-family is something like "General", "Consumable", "Weapon", "Weapon.Melee")
        public static void LoadTraitDefinitions()
        {
            var traitGroups = HjsonValue.Load(TraitFilePath).Qo();

            TraitDefinitions = new Dictionary<string, IList<string>>();

            // Trait Group object:
            //  object name = object.Key
            //  "traits": list of strings defining the names of traits in this group
            //              Note: there should not be more than 32 traits in any single group
            //  optional Subobjects: in same form
            foreach (var tgroup in traitGroups)
            {
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
        public static void AssignFlagValues()
        {
            // make sure trait-defs were loaded
            if (TraitDefinitions==null)
                LoadTraitDefinitions();

            FlagCollection = new Dictionary<string, IDictionary<string, int>>();

            foreach (var tgroup in TraitDefinitions)
            {
                var flagGroup = new Dictionary<string, int>();

                // always initialize with a "none"
                flagGroup["none"] = 0;
                foreach (var trait in tgroup.Value)
                {
                    // each new value added is a (binary) order of magnitude larger than the last
                    flagGroup[trait] = 1 << (flagGroup.Count-1);
                }

                // add flag group to collection
                FlagCollection[tgroup.Key] = flagGroup;
            }
        }

        /// After all the trait-definitions have been loaded, read in all the Category definition
        /// files and assign each Category a List of {Trait-Family: combined_flag_value} pairs
        /// using the bit-flag-values assigned in AssignFlagValues(). These family::flags maps
        /// define the full set of flags required for an individual item to match the given
        /// category. Note that it is possible for an item to match multiple categories. Conflict
        /// resolution will be weighted by [currently by order of appearance in the cat-def files;
        /// later on a "priority" field may be implemented to further define which categories
        /// override others].
        public static void LoadCategoryDefinitions()
        {
            // this returns an enumerable of <Filename: List-of-category-objects> pairs
            var category_list =
                from file in Directory.GetFiles(CategoryDefsPath, "*.hjson", SearchOption.TopDirectoryOnly)
                orderby file
                select new
                {
                    File = file,
                    CatObjList = HjsonValue.Load(file).Qa()
                };

            var category_defs = new Dictionary<string, ItemCategory>();

            // Structure of a category object:
            // 'name': string
            // 'parent': string (must be a name of a previously-encountered category) || null (for top level categories)
            // 'requires': a mapping (dict) of Trait-families to a list of required traits of that type;
            // this list will be combined with the 'required' list from the parent (if any) to define
            // the full requirements for items matching this category.

            int count=0; // track order of added categories
            foreach (var pair in category_list)
            {
                var fname = pair.File;
                foreach (var catobj in pair.CatObjList)
                {
                    string category_name = catobj["name"].Qs();
                    string parent_name = catobj["parent"]?.Qs();

                    var reqs = new Dictionary<string, int>();

                    ItemCategory parent = null;
                    // get the parent requirements first
                    if (parent_name != null)
                    {
                        // TODO: don't fail on malformed categories, but log the error somewhere
                        parent = category_defs[parent_name];
                        foreach (var kvp in parent.Requirements)
                            reqs[kvp.Key] = kvp.Value;


                        // try {
                        // }
                        // catch (KeyNotFoundException e)
                        // {
                        //     Console.WriteLine("{0}, {1}", catmatcher.Count, string.Join(",\n", catmatcher.Select(kv=>kv.Key).ToArray()));
                        //     Console.WriteLine("{0}", parent);
                        //     throw e;
                        // }
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

                            foreach (string trait_name in newreqs.Value.Qa())
                            reqs[traitCategory] |= flagvalues[trait_name];
                        }

                        /// if, somehow, there are no requirements, don't bother adding to list
                        if (reqs.Count > 0)
                            category_defs[category_name] = new ItemCategory(category_name, reqs, parent, count++);
                    }
                    else if (catobj.ContainsKey("merge"))
                    {
                        var merge_container = new ItemCategory(category_name, parent, true, count++);
                        foreach (var mergedcat in catobj["merge"].Qa())
                        {
                            try
                            {
                                category_defs[mergedcat].Merge(merge_container);
                            }
                            catch (KeyNotFoundException)// e)
                            {
                                //FIXME: use ErrorLogger
                                // Console.WriteLine(e.Message);
                            }
                        }
                        category_defs[category_name] = merge_container;
                    }
                } // end of category-object list
            }
            CategoryDefinitions = category_defs;

            // foreach (var kvp in CategoryDefinitions)
            // {
            //     Console.WriteLine("{0}:", kvp.Key);
            //     if (kvp.Value.MergeContainer)
            //         Console.WriteLine("    {0}", string.Join(",\n    ", kvp.Value.Merged.ToArray()));
            //     else
            //     {
            //         foreach (var flv in kvp.Value.Requirements)
            //         {
            //             Console.WriteLine("    {0}: {1}", flv.Key, flv.Value);
            //         }
            //     }
            // }

            // Console.WriteLine("{0}, {1}", CategoryDefinitions.Count, string.Join(",\n", CategoryDefinitions.Select(kv=>kv.Key).ToArray()));
        }

        /// After reading in all of the category definitions, build a tree structure based on the parent-child relationships
        /// between the categories. Traversing the tree structure when testing an item's traits will be far more efficient than
        /// checking each category individually.
        public static void BuildCategoryTree()
        {
            // TODO: make sure these are sorted based on priority
            // create the root of the tree; the label here is unimportant. All other labels
            // will be created automatically during autovivification
            var cattree = new SortedAutoTree<string, ItemCategory>() { Label = "root" };

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
                    subtree = subtree[catstack.Pop().Name];
                }

                // now create the child/set its data if it
                // has already been created by a previous operation
                subtree[category.Name].Data = category;
            }

            CategoryTree = cattree;
        }
    }
}