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

using System;
using System.Collections.Generic;

namespace Jolt.Net
{
    /**
     * This Comparator is used for determining the execution order of childSpecs.apply(...)
     *
     * Argument Map of Class: integer is used to determine precedence
     */
    public class ComputedKeysComparator : IComparer<IBaseSpec>
    {
        /**
         * Static factory method to get an Comparator instance for a given order map
         * @param orderMap of precedence
         * @return Comparator that uses the given order map to determine precedence
         */
        public static ComputedKeysComparator FromOrder(Dictionary<Type, int> orderMap)
        {
            return new ComputedKeysComparator(orderMap);
        }

        private readonly Dictionary<Type, int> _orderMap;

        private ComputedKeysComparator(Dictionary<Type, int> orderMap)
        {
            _orderMap = orderMap;
        }

        public int Compare(IBaseSpec a, IBaseSpec b)
        {
            IPathElement ape = a.GetPathElement();
            IPathElement bpe = b.GetPathElement();

            int aa = _orderMap[ape.GetType()];
            int bb = _orderMap[bpe.GetType()];

            int elementsEqual = aa < bb ? -1 : aa == bb ? 0 : 1;

            if (elementsEqual != 0)
            {
                return elementsEqual;
            }

            // At this point we have two PathElements of the same type.
            string acf = ape.GetCanonicalForm();
            string bcf = bpe.GetCanonicalForm();

            int alen = acf.Length;
            int blen = bcf.Length;

            // Sort them by.Length, with the longest (most specific) being first
            //  aka "rating-range-*" needs to be evaluated before "rating-*", or else "rating-*" will catch too much
            // If the.Lengths are equal, sort alphabetically as the last ditch deterministic behavior
            return alen > blen ? -1 : alen == blen ? acf.CompareTo(bcf) : 1;
        }
    }
}
