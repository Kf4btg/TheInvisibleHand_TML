using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Hjson;

namespace InvisibleHand.Items
{
    using Requirements = Dictionary<string, int>;
    using CategoryMatcher = Dictionary<string, Dictionary<string, int>>;

    /// read in the category specs from the hjson files and convert to something useable
    public class CategoryParser
    {
        private string TraitFilePath;
        private string CategoryDefsPath;

        public CategoryMatcher CatMatcher { get; private set; }

        public IDictionary<string, IList<string>> TraitDefinitions { get; private set; }

        public CategoryParser(string category_dir = "Definitions/Categories", string trait_file = "Definitions/Traits.hjson")
        {
            this.CategoryDefsPath = category_dir;
            this.TraitFilePath = trait_file;
        }

        public void LoadTraitDefinitions()
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
        private void LoadGroup(KeyValuePair<string, JsonValue> group_object, string name_prefix="")
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

        public void LoadCategoryDefinitions()
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
            var catmatcher = new CategoryMatcher();

            // Structure of a category object:
            // 'name': string
            // 'parent': string (must be a name of a previously-encountered category) || null (for top level categories)
            // 'requires': a mapping (dict) of Trait-groups to a list of required traits of that type;
            // this list will be combined with the 'required' list from the parent (if any) to define
            // the full requirements for items matching this category.

            foreach (var pair in category_list)
            {
                var fname = pair.File;
                foreach (var catobj in pair.CatObjList)
                {
                    string category_name = catobj["name"].Qs();
                    string parent = catobj["parent"]?.Qs();
                    var reqs = new Requirements();

                    // get the parent requirements first
                    if (parent != null)
                        foreach (var kvp in catmatcher[parent])
                            reqs[kvp.Key] = kvp.Value;

                    foreach (var newreqs in catobj["requires"].Qo())
                    {
                        var traitCategory = newreqs.Key.ToLower();
                        // FlagCollection[TraitCategory][TraitName]
                        var flagvalues = ItemFlags.FlagCollection[traitCategory];

                        if (!reqs.ContainsKey(traitCategory))
                            reqs[traitCategory] = 0;

                        foreach (string trait_name in newreqs.Value.Qa())
                            reqs[traitCategory] |= flagvalues[trait_name];
                    }
                    catmatcher[category_name] = reqs;
                }
            }
            CatMatcher = catmatcher;
        }
    }
}
