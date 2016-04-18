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

        public CategoryParser(string category_dir = "Categories", string trait_file = "Traits.hjson")
        {
            this.CategoryDefsPath = category_dir;
            this.TraitFilePath = trait_file;
        }

        public void ReadConfigFiles(string path)
        {
            // this returns an enumerable of <Filename: List-of-category-objects> pairs
            var category_list = from file in Directory.GetFiles(path, "*.hjson", SearchOption.TopDirectoryOnly)
                         orderby file ascending
                         from catlist in HjsonValue.Load(file).Qa()
                         select new
                         {
                             File = file,
                             CatObjList = catlist
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
                foreach (JsonValue catobj in pair.CatObjList)
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
                        // var traitType = newreqs.Key; //string

                        // FlagCollection[TraitCategory][TraitName]
                        var traitCategory = newreqs.Key;
                        var flagvalues = ItemFlags.FlagCollection[traitCategory];

                        if (!reqs.ContainsKey(traitCategory))
                            reqs[traitCategory] = 0;

                        foreach (string trait_name in newreqs.Value.Qa())
                            reqs[traitCategory] |= flagvalues[trait_name];
                    }

                    catmatcher[category_name] = reqs;
                }
            }
        }
    }
}
