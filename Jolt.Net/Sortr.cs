/*
 * Copyright 2013 Bazaarvoice, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jolt.Net
{
    /**
     * Standard alphabetical sort, with a special case for keys beginning with "~".
     */
    public class JsonKeyComparator : IComparer<string> 
    {
        public int Compare(string a, string b)
        {
            bool aTilde = a.Length > 0 && a[0] == '~';
            bool bTilde = b.Length > 0 && b[0] == '~';

            if (aTilde && !bTilde)
            {
                return -1;
            }
            if (!aTilde && bTilde)
            {
                return 1;
            }

            return a.CompareTo(b);
        }
    }

    /**
     * Recursively sorts all maps within a JSON object into new sorted LinkedHashMaps so that serialized
     * representations are deterministic.  Useful for debugging and making test fixtures.
     *
     * Note this will make a copy of the input Map and List objects.
     *
     * The sort order is standard alphabetical ascending, with a special case for "~" prefixed keys to be bumped to the top.
     */
    public class Sortr : ITransform
    {
        private readonly static JsonKeyComparator _jsonKeyComparator = new JsonKeyComparator();

        /**
         * Makes a "sorted" copy of the input JSON for human readability.
         *
         * @param input the JSON object to transform, in plain vanilla Jackson Map<string, object> style
         */
        public JToken Transform(JToken input)
        {
            return SortJson(input);
        }

        public static JToken SortJson(JToken value)
        {
            if (value is JObject obj)
            {
                return SortMap(obj);
            }
            if (value is JArray arr)
            {
                return Ordered(arr);
            }
            return value;
        }

        private static JObject SortMap(JObject map)
        {
            var orderedMap = new JObject();
            foreach (var prop in map.Properties().OrderBy(p => p.Name, _jsonKeyComparator))
            {
                orderedMap.Add(prop.Name, SortJson(prop.Value));
            }
            return orderedMap;
        }

        private static JArray Ordered(JArray list)
        {
            // Don't sort the list because that would change intent, but sort its components
            // Additionally, make a copy of the List in-case the provided list is Immutable / Unmodifiable
            var newList = new JArray();
            foreach (var entry in list)
            {
                newList.Add(SortJson(entry));
            }
            return newList;
        }
    }
}
