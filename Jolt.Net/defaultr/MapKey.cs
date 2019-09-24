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
    public class MapKey : Key
    {
        public MapKey(string jsonKey, JToken spec) :
            base(jsonKey, spec)
        {
        }

        protected override int GetLiteralIntKey() =>
            throw new InvalidOperationException("Shouldn't be be asking a MapKey for int getLiteralIntKey().");

        protected override void ApplyChild(JToken container)
        {
            if (container is JObject defaulteeMap)
            {
                // Find all defaultee keys that match the childKey spec.  Simple for Literal keys, more work for * and |.
                foreach (string literalKey in DetermineMatchingContainerKeys(defaulteeMap))
                {
                    ApplyLiteralKeyToContainer(literalKey, defaulteeMap);
                }
            }
            // Else there is disagreement (with respect to Array vs Map) between the data in
            //  the Container vs the Defaultr Spec type for this key.  Container wins, so do nothing.
        }

        private void ApplyLiteralKeyToContainer(string literalKey, JObject container)
        {
            container.TryGetValue(literalKey, out var defaulteeValue);

            if (_children == null)
            {
                if (defaulteeValue == null ||
                    defaulteeValue.Type == JTokenType.Null)
                {
                    container[literalKey] = _literalValue; // apply a copy of the default value into a map
                }
            }
            else
            {
                if (defaulteeValue == null ||
                    defaulteeValue.Type == JTokenType.Null)
                {
                    defaulteeValue = CreateOutputContainerObject();
                    container[literalKey] = defaulteeValue;  // push a new sub-container into this map
                }

                // recurse by applying my children to this known valid container
                ApplyChildren(defaulteeValue);
            }
        }

        private IReadOnlyCollection<string> DetermineMatchingContainerKeys(JObject container)
        {
            switch (GetOp())
            {
                case OPS.LITERAL:
                    // the container should get these literal values added to it
                    return _keyStrings;
                case OPS.STAR:
                    // Identify all its keys
                    return container.Properties().Select(x => x.Name).ToList();
                case OPS.OR:
                    // Identify the intersection between its keys and the OR values
                    var intersection = new HashSet<string>();
                    foreach (var keyString in _keyStrings)
                    {
                        if (container.ContainsKey(keyString))
                        {
                            intersection.Add(keyString);
                        }
                    }
                    return intersection;
                default:
                    throw new InvalidOperationException("Someone has added an op type without changing this method.");
            }
        }
    }
}
