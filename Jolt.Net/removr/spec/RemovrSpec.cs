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

namespace Jolt.Net
{
    public abstract class RemovrSpec
    {
        protected readonly IMatchablePathElement _pathElement;

        public RemovrSpec(string rawJsonKey)
        {
            var pathElement = Parse(rawJsonKey);

            if (!(pathElement is IMatchablePathElement mpe))
            {
                throw new SpecException("Spec LHS key=" + rawJsonKey + " is not a valid LHS key.");
            }

            _pathElement = mpe;
        }

        // Ex Keys :  *, cdv-*, *-$de
        public static IPathElement Parse(string key)
        {
            if ("*" == key)
            {
                return new StarAllPathElement(key);
            }

            int numOfStars = StringTools.CountMatches(key, "*");
            if (numOfStars == 1)
            {
                return new StarSinglePathElement(key);
            }
            else if (numOfStars == 2)
            {
                return new StarDoublePathElement(key);
            }
            else if (numOfStars > 2)
            {
                return new StarRegexPathElement(key);
            }
            else
            {
                return new LiteralPathElement(key);
            }
        }

        /**
         * Try to "interpret" the spec string value as a non-negative integer.
         *
         * @return non-negative integer, otherwise null
         */
        protected int? GetNonNegativeIntegerFromLiteralPathElement()
        {
            if (Int32.TryParse(_pathElement.RawKey, out int pathElementInt) &&
                pathElementInt >= 0)
            {
                return pathElementInt;
            }
            return null;
        }

        /**
         * Build a list of indices to remove from the input list, using the pathElement
         *  from the Spec.
         *
         * @return the indicies to remove, otherwise empty List.
         */
        public abstract IEnumerable<int> ApplyToList(JArray inputList);

        /**
         * Build a list of keys to remove from the input map, using the pathElement
         *  from the Spec.
         *
         * @return the keys to remove, otherwise empty List.
         */
        public abstract List<string> ApplyToMap(JObject inputMap);
    }
}
