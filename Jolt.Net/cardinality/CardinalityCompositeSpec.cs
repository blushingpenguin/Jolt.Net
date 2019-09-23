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
using System.Linq;

namespace Jolt.Net
{

    /**
     * CardinalitySpec that has children, which it builds and then manages during Transforms.
     */
    public class CardinalityCompositeSpec : CardinalitySpec
    {
        private static readonly Dictionary<Type, int> _orderMap = new Dictionary<Type, int>
        {
            { typeof(AmpPathElement), 1 },
            { typeof(IStarPathElement), 2 },
        };
        private static readonly ComputedKeysComparator computedKeysComparator =
            ComputedKeysComparator.FromOrder(_orderMap);

        // Three different buckets for the children of this CardinalityCompositeSpec
        private CardinalityLeafSpec _specialChild;                    // children that aren't actually triggered off the input data
        private readonly IReadOnlyDictionary<string, CardinalitySpec> _literalChildren;  // children that are simple exact matches against the input data
        private readonly IReadOnlyList<CardinalitySpec> _computedChildren;        // children that are regex matches against the input data

        public CardinalityCompositeSpec(string rawKey, Dictionary<string, object> spec) :
            base(rawKey)
        {
            var literals = new Dictionary<string, CardinalitySpec>();
            var computed = new List<CardinalitySpec>();

            _specialChild = null;

            // self check
            if (GetPathElement().GetType() == typeof(AtPathElement))
            {
                throw new SpecException("@ CardinalityTransform key, can not have children.");
            }

            List<CardinalitySpec> children = CreateChildren(spec);

            if (children.Count == 0)
            {
                throw new SpecException("Shift CardinalitySpec format error : CardinalitySpec line with empty {} as value is not valid.");
            }

            foreach (CardinalitySpec child in children)
            {
                var childPe = child.GetPathElement();
                literals[childPe.RawKey] = child;

                if (childPe is LiteralPathElement)
                {
                    literals[childPe.RawKey] = child;
                }
                // special is it is "@"
                else if (childPe is AtPathElement)
                {
                    if (child is CardinalityLeafSpec cls)
                    {
                        _specialChild = cls;
                    }
                    else
                    {
                        throw new SpecException("@ CardinalityTransform key, can not have children.");
                    }
                }
                // star
                else
                {
                    computed.Add(child);
                }
            }

            // Only the computed children need to be sorted
            computed.Sort(computedKeysComparator);
            computed.TrimExcess();
            _literalChildren = literals;
            _computedChildren = computed.AsReadOnly();
        }


        /**
         * Recursively walk the spec input tree.
         */
        private static List<CardinalitySpec> CreateChildren(Dictionary<string, object> rawSpec)
        {
            var children = new List<CardinalitySpec>();
            var actualKeys = new HashSet<string>();

            foreach (var kv in rawSpec)
            {
                CardinalitySpec childSpec;
                if (kv.Value is Dictionary<string, object> dic)
                {
                    childSpec = new CardinalityCompositeSpec(kv.Key, dic);
                }
                else
                {
                    childSpec = new CardinalityLeafSpec(kv.Key, kv.Value);
                }

                string childCanonicalString = childSpec.GetPathElement().GetCanonicalForm();

                if (actualKeys.Contains(childCanonicalString))
                {
                    throw new ArgumentException(nameof(rawSpec),
                        "Duplicate canonical CardinalityTransform key found : " + childCanonicalString);
                }

                actualKeys.Add(childCanonicalString);

                children.Add(childSpec);
            }

            return children;
        }

        /**
         * If this Spec matches the inputkey, then perform one step in the parallel treewalk.
         * <p/>
         * Step one level down the input "tree" by carefully handling the List/Map nature the input to
         * get the "one level down" data.
         * <p/>
         * Step one level down the Spec tree by carefully and efficiently applying our children to the
         * "one level down" data.
         *
         * @return true if this this spec "handles" the inputkey such that no sibling specs need to see it
         */
        public override bool ApplyCardinality(string inputKey, object input, WalkedPath walkedPath, object parentContainer)
        {
            MatchedElement thisLevel = GetPathElement().Match(inputKey, walkedPath);
            if (thisLevel == null)
            {
                return false;
            }

            walkedPath.Add(input, thisLevel);

            // The specialChild can change the data object that I point to.
            // Aka, my key had a value that was a List, and that gets changed so that my key points to a ONE value
            if (_specialChild != null)
            {
                input = _specialChild.ApplyToParentContainer(inputKey, input, walkedPath, parentContainer);
            }

            // Handle the rest of the children
            Process(input, walkedPath);

            walkedPath.RemoveLast();
            return true;
        }

        private void Process(object input, WalkedPath walkedPath)
        {
            if (input is Dictionary<string, object> dic)
            {
                // XXX: not sure what this means
                // Iterate over the whole entrySet rather than the keyset with follow on gets of the values
                foreach (var kv in dic.ToList())
                {
                    ApplyKeyToLiteralAndComputed(this, kv.Key, kv.Value, walkedPath, input);
                }
                // Set<Map.Entry<string, object>> entrySet = new HashSet<>(((Map<string, object>)input).entrySet());
                // for (Map.Entry<string, object> inputEntry : entrySet) {
                //     ApplyKeyToLiteralAndComputed(this, inputEntry.getKey(), inputEntry.getValue(), walkedPath, input);
                // }
            }
            else if (input is List<object> list)
            {

                for (int index = 0; index < list.Count; index++)
                {
                    object subInput = list[index];
                    string subKeyStr = index.ToString();

                    ApplyKeyToLiteralAndComputed(this, subKeyStr, subInput, walkedPath, input);
                }
            }
            else if (input != null)
            {

                // if not a map or list, must be a scalar
                string scalarInput = input.ToString();
                ApplyKeyToLiteralAndComputed(this, scalarInput, null, walkedPath, scalarInput);
            }
        }

        /**
         * This method implements the Cardinality matching behavior
         *  when we have both literal and computed children.
         * <p/>
         * For each input key, we see if it matches a literal, and it not, try to match the key with every computed child.
         */
        private static void ApplyKeyToLiteralAndComputed(CardinalityCompositeSpec spec, string subKeyStr, object subInput, WalkedPath walkedPath, object input)
        {
            // if the subKeyStr found a literalChild, then we do not have to try to match any of the computed ones
            if (spec._literalChildren.TryGetValue(subKeyStr, out var literalChild))
            {
                literalChild.ApplyCardinality(subKeyStr, subInput, walkedPath, input);
            }
            else
            {
                // If no literal spec key matched, iterate through all the computedChildren

                // Iterate through all the computedChildren until we find a match
                // This relies upon the computedChildren having already been sorted in priority order
                foreach (CardinalitySpec computedChild in spec._computedChildren)
                {
                    // if the computed key does not match it will quickly return false
                    if (computedChild.ApplyCardinality(subKeyStr, subInput, walkedPath, input))
                    {
                        break;
                    }
                }
            }
        }
    }
}
