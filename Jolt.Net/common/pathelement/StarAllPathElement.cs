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

namespace Jolt.Net
{
    /**
     * PathElement for the lone "*" wildcard.   In this case we can avoid doing any
     *  regex or string comparison work at all.
     */
    public class StarAllPathElement : IStarPathElement
    {
        public StarAllPathElement(string key)
        {
            if ("*" != key)
            {
                throw new ArgumentException("StarAllPathElement key should just be a single '*'.  Was: " + key);
            }
        }

        /**
         * @param literal test to see if the provided string will match this Element's regex
         * @return true if the provided literal will match this Element's regex
         */
        public bool StringMatch(string literal) => true;

        public MatchedElement Match(string dataKey, WalkedPath walkedPath)
        {
            int? origSizeOptional = walkedPath.LastElement().OrigSize;
            if (origSizeOptional.HasValue)
            {
                return new ArrayMatchedElement(dataKey, origSizeOptional.Value);
            }
            else
            {
                return new MatchedElement(dataKey);
            }
        }

        public string GetCanonicalForm() => "*";

        public string RawKey => "*";
    }
}
