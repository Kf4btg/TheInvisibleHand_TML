using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Hjson;

namespace InvisibleHand.Items
{
    /// read in the category specs from the hjson files and convert to something useable
    public static class CategoryParser
    {
        private static string TraitFilePath;
        private static string CategoryDefsPath;

        public static IDictionary<string, IList<string>> TraitDefinitions { get; private set; }
        public static IDictionary<string, IDictionary<string, int>> FlagCollection {get; private set;}
        public static IDictionary<string, IDictionary<string, int>> CategoryDefinitions { get; private set; }

        public static void Parse(string category_dir = "Definitions/Categories", string trait_file = "Definitions/Traits.hjson")
        {
            CategoryDefsPath = category_dir;
            TraitFilePath = trait_file;

            // the order is important here
            LoadTraitDefinitions();
            AssignFlagValues();
            LoadCategoryDefinitions();
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

            // Maps: CategoryName to (TraitType1: combined_flag_value, TraitType2: ...)
            var catmatcher = new Dictionary<string, IDictionary<string, int>>();

            // Structure of a category object:
            // 'name': string
            // 'parent': string (must be a name of a previously-encountered category) || null (for top level categories)
            // 'requires': a mapping (dict) of Trait-families to a list of required traits of that type;
            // this list will be combined with the 'required' list from the parent (if any) to define
            // the full requirements for items matching this category.

            foreach (var pair in category_list)
            {
                var fname = pair.File;
                foreach (var catobj in pair.CatObjList)
                {
                    string category_name = catobj["name"].Qs();
                    string parent = catobj["parent"]?.Qs();
                    var reqs = new Dictionary<string, int>();

                    // get the parent requirements first
                    if (parent != null)
                        foreach (var kvp in catmatcher[parent])
                            reqs[kvp.Key] = kvp.Value;

                    foreach (var newreqs in catobj["requires"].Qo())
                    {
                        var traitCategory = newreqs.Key; //.ToLower();
                        // FlagCollection[TraitCategory][TraitName]
                        var flagvalues = FlagCollection[traitCategory];

                        if (!reqs.ContainsKey(traitCategory))
                            reqs[traitCategory] = 0;

                        foreach (string trait_name in newreqs.Value.Qa())
                            reqs[traitCategory] |= flagvalues[trait_name];
                    }
                    catmatcher[category_name] = reqs;
                }
            }
            CategoryDefinitions = catmatcher;
        }
    }
}
