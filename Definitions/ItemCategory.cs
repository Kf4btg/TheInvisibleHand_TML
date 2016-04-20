using System.Collections.Generic;

namespace InvisibleHand.Definitions
{
    public class ItemCategory
    {
        public readonly int Priority;
        public string Name { get; private set; }

        public IDictionary<string, int> Requirements;

        /// this may be unnecessary; intended to hold the list of
        /// category names that have been merged to this one if this is a
        /// merge-container; only the names are stored because it should
        /// not be necessary to directly access the merged categories from here.
        public ISet<string> Merged;

        /// the parent's requirements will have already been merged into
        /// this Category's requirements, so this property is really only
        /// here to help with determining sort-order.
        public ItemCategory Parent { get; private set; }

        /// indicates whether this Category is simply a container
        /// for the categories that were merged into it. If so, it
        /// cannot have Requirements, and Matches() will always return null.
        /// When a category that has been merged into this one has its
        /// Matches() method called, it will a reference to this instance
        /// rather than itself.
        public readonly bool MergeContainer;

        /// if not null, then this is the Merge-Container-Category that this
        ///
        private ItemCategory MergeTo;


        /// this is what should be used to get "this" category; will return the proper
        /// instance depending on whether this category has been merged or not.
        public ItemCategory Category => MergeTo ?? this;


        public ItemCategory(string name, ItemCategory parent = null, bool is_merge_container = false, int priority = 0)
        {
            Name = name;
            Parent = parent;
            Priority = priority;

            MergeContainer = is_merge_container;

            if (MergeContainer)
                Merged = new HashSet<string>();
            else
                Requirements = new Dictionary<string, int>();
        }

        /// initialize the category with a pre-created requirements dict
        public ItemCategory(string name, IDictionary<string, int> requirements, ItemCategory parent = null, int priority = 0)
        {
            Name = name;
            Parent = parent;
            Priority = priority;

            MergeContainer = false;

            Requirements = requirements;
        }

        public void AddTraitRequirement(string trait_family, int flag_value)
        {
            // TODO: throw a proper exception when attempting to add req to merge containers
            if (MergeContainer)
                return;

            if (Requirements.ContainsKey(trait_family))
                Requirements[trait_family] |= flag_value;
            else
                Requirements[trait_family] = flag_value;
        }

        public ItemCategory Matches(IDictionary<string, int> item_flags)
        {
            foreach (var kvp in Requirements)
            {
                var reqval = kvp.Value;
                int flagval;

                // if ever the item does not have a required trait or it does
                // have the correct flag value for the trait, return null to indicate no match
                if (!item_flags.TryGetValue(kvp.Key, out flagval) || ((flagval & reqval) != reqval))
                    return null;
            }

            // if we made it here, than the item passed all the required checks.
            // Now we return the matched category:
            // If this category has been marked as part of a merged category,
            // return that category; otherwise, return this instance.
            return this.Category;
        }

        public void Merge(ItemCategory merged)
        {
            this.MergeTo = merged;
            merged.Merged.Add(this.Name);
        }
        public void Unmerge()
        {
            this.MergeTo = null;
        }
    }
}
