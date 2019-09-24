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
    public class ArrayKey : Key
    {
        private List<int> _keyInts;
        private int _keyInt = -1;

        public ArrayKey(string jsonKey, JToken spec) :
            base(jsonKey, spec)
        {
            // Handle ArrayKey specific stuff
            switch (GetOp())
            {
                case OPS.OR:
                    _keyInts = new List<int>();
                    foreach (string orLiteral in _keyStrings)
                    {
                        int orInt = Int32.Parse(orLiteral);
                        _keyInts.Add(orInt);
                    }
                    break;
                case OPS.LITERAL:
                    _keyInts = new List<int>();
                    _keyInt = Int32.Parse(_rawKey);
                    _keyInts.Add(_keyInt);
                    break;
                case OPS.STAR:
                    _keyInts = new List<int>();
                    break;
                default:
                    throw new InvalidOperationException("Someone has added an op type without changing this method.");
            }
        }

        protected override int GetLiteralIntKey() => _keyInt;

        protected override void ApplyChild(JToken container)
        {
            if (container is JArray defaultList)
            {
                // Find all defaultee keys that match the childKey spec.  Simple for Literal keys, more work for * and |.
                foreach (int literalKey in DetermineMatchingContainerKeys(defaultList))
                {
                    ApplyLiteralKeyToContainer(literalKey, defaultList);
                }
            }
            // Else there is disagreement (with respect to Array vs Map) between the data in
            //  the Container vs the Defaultr Spec type for this key.  Container wins, so do nothing.
        }

        private void ApplyLiteralKeyToContainer(int literalIndex, JArray container)
        {
            JToken defaulteeValue = container[literalIndex];

            if (_children == null)
            {
                if (defaulteeValue.Type == JTokenType.Null)
                {
                    container[literalIndex] = _literalValue.DeepClone();  // apply a copy of the default value into a List, assumes the list as already been expanded if needed.
                }
            }
            else
            {
                if (defaulteeValue.Type == JTokenType.Null)
                {
                    defaulteeValue = CreateOutputContainerObject();
                    container[literalIndex] = defaulteeValue; // push a new sub-container into this list
                }

                // recurse by applying my children to this known valid container
                ApplyChildren(defaulteeValue);
            }
        }

        private List<int> DetermineMatchingContainerKeys(JToken container)
        {
            switch (GetOp())
            {
                case OPS.LITERAL:
                    // Container it should get these literal values added to it
                    return _keyInts;
                case OPS.STAR:
                    // Identify all its keys
                    // this assumes the container list has already been expanded to the right size
                    var defaultList = (JArray)container;
                    var allIndexes = new List<int>(defaultList.Count);
                    for (int index = 0; index < defaultList.Count; index++)
                    {
                        allIndexes.Add(index);
                    }
                    return allIndexes;
                case OPS.OR:
                    // Identify the intersection between the container "keys" and the OR values
                    var indexesInRange = new List<int>();
                    int count = ((JArray)container).Count;

                    foreach (int orValue in _keyInts)
                    {
                        if (orValue < count)
                        {
                            indexesInRange.Add(orValue);
                        }
                    }
                    return indexesInRange;

                default:
                    throw new InvalidOperationException("Someone has added an op type without changing this method.");
            }
        }
    }
}
