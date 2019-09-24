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
     * PathElement for the a single "*" wildcard such as tag-*.   In this case we can avoid doing any
     *  regex work by doing string begins and ends with comparisons.
     */
    public class StarSinglePathElement : BasePathElement, IStarPathElement
    {

        private readonly string _prefix;
        private readonly string _suffix;

        public StarSinglePathElement(string key) :
            base(key)
        {
            if (StringTools.CountMatches(key, "*") != 1)
            {
                throw new ArgumentException(nameof(key), "StarSinglePathElement should only have one '*' in its key. Was: " + key);
            }
            else if ("*" == key)
            {
                throw new ArgumentException(nameof(key), "StarSinglePathElement should have a key that is just '*'. Was: " + key);
            }

            if (key.StartsWith("*"))
            {
                _prefix = "";
                _suffix = key.Substring(1);
            }
            else if (key.EndsWith("*"))
            {
                _prefix = key.Substring(0, key.Length - 1);
                _suffix = "";
            }
            else
            {
                string[] split = key.Split('*');
                _prefix = split[0];
                _suffix = split[1];
            }
        }

        /**
         * @param literal test to see if the provided string will match this Element's regex
         * @return true if the provided literal will match this Element's regex
         */
        public bool StringMatch(string literal)
        {
            return literal.StartsWith(_prefix) && literal.EndsWith(_suffix)  // the ends match
                    && literal.Length > _prefix.Length + _suffix.Length;   // and the * captures something
        }

        public MatchedElement Match(string dataKey, WalkedPath walkedPath)
        {
            if (StringMatch(dataKey))
            {
                var subKeys = new List<string>();

                string starPart = dataKey.Substring(_prefix.Length, dataKey.Length - _suffix.Length - _prefix.Length);
                subKeys.Add(starPart);

                return new MatchedElement(dataKey, subKeys);
            }

            return null;
        }

        public override string GetCanonicalForm() => RawKey;
    }
}
