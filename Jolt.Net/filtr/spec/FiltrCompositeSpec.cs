using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jolt.Net
{
    class FiltrCompositeSpec
    {
        public Dictionary<string, FiltrCompositeSpec> Children { get; } =
            new Dictionary<string, FiltrCompositeSpec>();
        public List<FiltrLeafSpec> Filters { get; } = 
            new List<FiltrLeafSpec>();

        public IMatchablePathElement PathElement { get; }

        public FiltrCompositeSpec(IMatchablePathElement pathElement)
        {
            PathElement = pathElement;
        }

        private bool IsFiltered(FiltrCompositeSpec spec, JToken input)
        {
            if (!spec.Filters.Any())
            {
                return false;
            }
            // apply any filters to the object
            foreach (var filter in spec.Filters)
            {
                if (filter.Matches(input))
                {
                    return false;
                }
            }
            return true;
        }

        public void Apply(JToken input, WalkedPath walkedPath)
        {
            if (input is JArray arr)
            {
                HashSet<int> indexesToRemove = null;

                for (int i = 0; i < arr.Count; ++i)
                {
                    var elt = arr[i];
                    var key = i.ToString();
                    foreach (var child in Children.Values)
                    {
                        var match = child.PathElement.Match(key, walkedPath);
                        if (match == null)
                        {
                            continue;
                        }
                        if (IsFiltered(child, elt))
                        {
                            if (indexesToRemove == null)
                            {
                                indexesToRemove = new HashSet<int>();
                            }
                            indexesToRemove.Add(i);
                        }
                        else
                        {
                            walkedPath.Add(key, match);
                            child.Apply(elt, walkedPath);
                            walkedPath.RemoveLast();
                        }
                    }
                }

                if (indexesToRemove != null)
                {
                    foreach (var index in indexesToRemove.OrderByDescending(x => x))
                    {
                        arr.RemoveAt(index);
                    }
                }
            }
            else if (input is JObject obj)
            {
                HashSet<string> keysToRemove = null;
                foreach (var kv in obj)
                {
                    foreach (var child in Children.Values)
                    {
                        var match = child.PathElement.Match(kv.Key, walkedPath);
                        if (match == null)
                        {
                            continue;
                        }
                        if (IsFiltered(child, kv.Value))
                        {
                            if (keysToRemove == null)
                            {
                                keysToRemove = new HashSet<string>();
                            }
                            keysToRemove.Add(kv.Key);
                        }
                        else
                        {
                            walkedPath.Add(kv.Key, match);
                            child.Apply(kv.Value, walkedPath);
                            walkedPath.RemoveLast();
                        }
                    }
                }
                if (keysToRemove != null)
                {
                    foreach (string key in keysToRemove)
                    {
                        obj.Remove(key);
                    }
                }
            }
        }
    }
}
