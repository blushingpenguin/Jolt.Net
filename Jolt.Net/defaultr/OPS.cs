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
    public enum OPS
    {
        STAR,
        OR,
        LITERAL
    }

    class OPSUtils
    {
        public static OPS Parse(string key)
        {
            if (key.Contains(Defaultr.WildCards.STAR))
            {
                if (Defaultr.WildCards.STAR != key)
                {
                    throw new SpecException("Defaultr key " + key + " is invalid.  * keys can only contain *, and no other characters.");
                }

                return OPS.STAR;
            }
            if (key.Contains(Defaultr.WildCards.OR))
            {
                return OPS.OR;
            }
            return OPS.LITERAL;
        }
    }

    public class OpsPrecedenceComparator : IComparer<OPS>
    {
        /**
         * The order we want to apply Defaultr logic is Literals, Or, and then Star.
         * Since we walk the sorted data from 0 to n, that means Literals need to low, and Star should be high.
         */
        public int Compare(OPS ops, OPS ops1)
        {
            // a negative integer, zero, or a positive integer as the first argument is less than, equal to, or greater than the second.
            // s < s1 -> -1
            // s = s1 -> 0
            // s > s1 -> 1

            if (ops == ops1)
            {
                return 0;
            }

            if (OPS.STAR == ops)
            {
                return 1;
            }
            if (OPS.LITERAL == ops)
            {
                return -1;
            }

            // if we get here, "ops" has to equal OR
            if (OPS.STAR == ops1)
            {
                return -1;
            }
            if (OPS.LITERAL == ops1)
            {
                return 1;
            }

            // both are ORs, should never get here
            throw new InvalidOperationException("Someone has added an op type without changing this method.");
        }
    }
}
