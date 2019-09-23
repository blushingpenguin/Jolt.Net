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
     * Spec for handling the leaf level of the Removr Transform.
     */
    public class RemovrLeafSpec : RemovrSpec
    {
        public RemovrLeafSpec(string rawKey) :
            base(rawKey)
        {
        }

        /**
         * Build a list of keys to remove from the input map, using the pathElement
         *  from the Spec.
         *
         * @param inputMap : Input map from which the spec key needs to be removed.
         */
        public override List<string> ApplyToMap(Dictionary<string, object> inputMap)
        {
            if (inputMap == null)
            {
                return null;
            }

            var keysToBeRemoved = new List<string>();

            if (_pathElement is LiteralPathElement)
            {

                // if we are a literal, check to see if we match
                if (inputMap.ContainsKey(_pathElement.RawKey))
                {
                    keysToBeRemoved.Add(_pathElement.RawKey);
                }
            }
            else if (_pathElement is IStarPathElement star)
            {

                // if we are a wildcard, check each input key to see if it matches us
                foreach (string key in inputMap.Keys)
                {

                    if (star.StringMatch(key))
                    {
                        keysToBeRemoved.Add(key);
                    }
                }
            }

            return keysToBeRemoved;
        }

        /**
         * @param inputList : Input List from which the spec key needs to be removed.
         */
        public override List<int?> ApplyToList(List<object> inputList)
        {
            if (inputList == null)
            {
                return null;
            }

            if (_pathElement is LiteralPathElement)
            {

                int? pathElementInt = GetNonNegativeIntegerFromLiteralPathElement();

                if (pathElementInt.HasValue && pathElementInt.Value < inputList.Count)
                {
                    var result = new List<int?>();
                    result.Add(pathElementInt);
                    return result;
                }
            }
            else if (_pathElement is StarAllPathElement)
            {
                // To be clear, this is kinda silly.
                // If you just wanted to remove the whole list, you could have just
                //  directly removed it, instead of stepping into it and using the "*".
                var toReturn = new List<int?>(inputList.Count);
                for (int index = 0; index < inputList.Count; index++)
                {
                    toReturn.Add(index);
                }

                return toReturn;
            }

            // else the pathElement is some other kind which is not supported when running
            //  against arrays, aka "tuna*" makes no sense against a list.
            return new List<int?>();
        }
    }
}
