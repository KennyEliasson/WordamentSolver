using System;
using System.Collections.Generic;
using System.Linq;

namespace WordamentSolver
{
    public class Trie<TItem>
    {
        public Trie(IEnumerable<TItem> items, Func<TItem, string> keySelector, IComparer<TItem> comparer)
        {
            KeySelector = keySelector;
            Comparer = comparer;
            Items = (from item in items
                    from i in Enumerable.Range(1, KeySelector(item).Length)
                    let key = KeySelector(item).Substring(0, i)
                    group item by key)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        protected Dictionary<string, List<TItem>> Items { get; set; }

        protected Func<TItem, string> KeySelector { get; set; }

        protected IComparer<TItem> Comparer { get; set; }

        public List<TItem> Retrieve(string prefix)
        {
            return Items.ContainsKey(prefix)
                ? Items[prefix]
                : new List<TItem>();
        }
    }
}