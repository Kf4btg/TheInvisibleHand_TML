using System;
using System.Linq;
using System.Collections.Generic;
using InvisibleHand.Rules;
using Terraria;

namespace InvisibleHand.Items
{
    using ItemCompRule = Func<Item, Item, int>;

    public static class ItemRuleBuilder
    {
        private static IDictionary<string, ItemCompRule> RuleCache = new Dictionary<string, ItemCompRule>();

        /// exists only to feed to the generic RuleBuilder
        private static readonly List<Item> dummylist = new List<Item>(0);

        /// Given an enumerable of Terraria.Item property names, build and compile
        /// lambda expressions that will return the result of comparing those properties
        /// on two distinct Item objects; return the list of those compiled expressions.
        public static List<ItemCompRule> BuildSortRules(IEnumerable<string> properties)
        {
            var rules = new List<ItemCompRule>();
            // if the cache contains any of the requested properties, we'll build the rules 1-by-1
            if (properties.Any(p => RuleCache.ContainsKey(p)))
            {
                // add each cached/created rule to list
                foreach (var prop in properties)
                    rules.Add(GetRule(prop));
            }
            // if all are uncached, call the multi-rule builder (for efficiency)
            // TODO: Decide whether copying the original list of names, building the rules, then
            // creating and using a new iterable of anonymous types based on the newly-built
            // rule list to populate the cache using the index-referenced value from the
            // aforementioned name-list-copy...IS, in fact, more efficient than having a cache
            // miss and building each rule individually...
            else if (properties.Count() > 0) // make sure list isn't empty
            {
                // make a List (with a capital L) copy of the property names
                var plist = new List<string>(properties);

                rules = RuleBuilder.CompileVsRules(dummylist, plist);

                // because the rules SHOULD be in the same order as the properties given,
                // we can access the property-name-list by index to get the new cache key:
                foreach (var newrule in rules.Select((r, i) => new { rule = r, index = i }))
                    RuleCache[plist[newrule.index]] = newrule.rule;
            }

            return rules;
        }

        public static ItemCompRule GetRule(string property)
        {
            ItemCompRule rule;
            // check the cache first
            if (!RuleCache.TryGetValue(property, out rule))
            {
                // if the rule wasn't there, create it with the RuleBuilder
                rule = RuleBuilder.CompileVsRule(dummylist, property);
                // and cache it
                RuleCache[property] = rule;
            }
            return rule;
        }

    }
}
