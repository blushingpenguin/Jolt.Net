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

    /**
     * Spec that has children, which it builds and then manages during Transforms.
     */
    public class ShiftrCompositeSpec : ShiftrSpec, IOrderedCompositeSpec
    {

        /*
        Example of how a Spec gets parsed into Composite and LeafSpec objects :

        {                                                        //  "implicit" root CompositeSpec, with one specialChild ("@") and one literalChild ("rating")
            "@" : [ "payload.original", "payload.secondCopy" ]   //  LeafSpec with an AtPathElement and outputWriters [ "payload.original", "payload.secondCopy" ]

            "rating": {                                          //  CompositeSpec with 1 literalChild ("rating") and one computedChild ("*")
                "primary": {
                    "value": "Rating",
                    "max": "RatingRange"
                },
                "*": {
                    "value": "SecondaryRatings.&1.Value",        // LeafSpec with a LiteralPathElement and one outputWriter [ "SecondaryRatings.&1.Value" ]
                    "max": "SecondaryRatings.&1.Range",
                    "&": "SecondaryRatings.&1.Id"                // & with no children : specialKey : Means use the text value of the key as the input
                }
            }
        }
        */

        private static readonly Dictionary<Type, int> _orderMap = new Dictionary<Type, int>
        {
            { typeof(AmpPathElement), 1 },
            { typeof(StarRegexPathElement), 2 },
            { typeof(StarDoublePathElement), 3 },
            { typeof(StarSinglePathElement), 4 },
            { typeof(StarAllPathElement), 5 }
        };
        private static readonly ComputedKeysComparator _computedKeysComparator =
            ComputedKeysComparator.FromOrder(_orderMap);
        private static readonly SpecBuilder<ShiftrSpec> _specBuilder = new ShiftrSpecBuilder();

        // Three different buckets for the children of this CompositeSpec
        private readonly IReadOnlyList<ShiftrSpec> _specialChildren;         // children that aren't actually triggered off the input data
        private readonly IReadOnlyDictionary<string, IBaseSpec> _literalChildren;  // children that are simple exact matches against the input data
        private readonly IReadOnlyList<ShiftrSpec> _computedChildren;        // children that are regex matches against the input data
        private readonly ExecutionStrategy _executionStrategy;

        public ShiftrCompositeSpec(string rawKey, JObject spec) :
            base(rawKey)
        {
            var special = new List<ShiftrSpec>();
            var literals = new Dictionary<string, IBaseSpec>();
            var computed = new List<ShiftrSpec>();

            // self check
            var pathElement = GetPathElement();
            if (pathElement is AtPathElement)
            {
                throw new SpecException("@ Shiftr key, can not have children.");
            }
            if (pathElement is DollarPathElement)
            {
                throw new SpecException("$ Shiftr key, can not have children.");
            }

            List<ShiftrSpec> children = _specBuilder.CreateSpec(spec);

            if (children.Count == 0)
            {
                throw new SpecException("Shift ShiftrSpec format error : ShiftrSpec line with empty {} as value is not valid.");
            }

            foreach (ShiftrSpec child in children)
            {
                var childPe = child.GetPathElement();
                if (childPe is LiteralPathElement)
                {
                    literals[childPe.RawKey] = child;
                }
                // special is it is "@" or "$"
                else if (childPe is AtPathElement ||
                          childPe is HashPathElement ||
                          childPe is DollarPathElement ||
                          childPe is TransposePathElement)
                {
                    special.Add(child);
                }
                else
                {   // star || (& with children)
                    computed.Add(child);
                }
            }

            // Only the computed children need to be sorted
            computed.Sort(_computedKeysComparator);

            special.TrimExcess();
            computed.TrimExcess();

            _specialChildren = special.AsReadOnly();
            _literalChildren = literals;
            _computedChildren = computed.AsReadOnly();

            _executionStrategy = DetermineExecutionStrategy();
        }


        public IReadOnlyDictionary<string, IBaseSpec> GetLiteralChildren() => _literalChildren;

        public IReadOnlyList<IBaseSpec> GetComputedChildren() => _computedChildren;

        public ExecutionStrategy DetermineExecutionStrategy()
        {
            if (_computedChildren.Count == 0)
            {
                return ExecutionStrategy.AvailableLiterals;
            }
            else if (_literalChildren.Count == 0)
            {
                return ExecutionStrategy.Computed;
            }

            foreach (IBaseSpec computed in _computedChildren)
            {
                if (!(computed.GetPathElement() is IStarPathElement starPathElement))
                {
                    return ExecutionStrategy.Conflict;
                }

                foreach (string literal in _literalChildren.Keys)
                {
                    if (starPathElement.StringMatch(literal))
                    {
                        return ExecutionStrategy.Conflict;
                    }
                }
            }

            return ExecutionStrategy.AvailableLiteralsWithComputed;
        }

        /**
         * If this Spec matches the inputKey, then perform one step in the Shiftr parallel treewalk.
         *
         * Step one level down the input "tree" by carefully handling the List/Map nature the input to
         *  get the "one level down" data.
         *
         * Step one level down the Spec tree by carefully and efficiently applying our children to the
         *  "one level down" data.
         *
         * @return true if this this spec "handles" the inputKey such that no sibling specs need to see it
         */
        public override bool Apply(string inputKey, JToken inputOptional, WalkedPath walkedPath, JObject output, JObject context)
        {
            MatchedElement thisLevel = _pathElement.Match(inputKey, walkedPath);
            if (thisLevel == null)
            {
                return false;
            }

            // If we are a TransposePathElement, try to swap the "input" with what we lookup from the Transpose
            if (_pathElement is TransposePathElement tpe)
            {
                // Note the data found may not be a string, thus we have to call the special objectEvaluate
                // Optional, because the input data could have been a valid null.
                var optional = tpe.ObjectEvaluate(walkedPath);
                if (optional == null)
                {
                    return false;
                }
                inputOptional = optional;
            }

            // add ourselves to the path, so that our children can reference us
            walkedPath.Add(inputOptional, thisLevel);

            // Handle any special / key based children first, but don't have them block anything
            foreach (ShiftrSpec subSpec in _specialChildren)
            {
                subSpec.Apply(inputKey, inputOptional, walkedPath, output, context);
            }

            // Handle the rest of the children
            _executionStrategy.Process(this, inputOptional, walkedPath, output, context);

            // We are done, so remove ourselves from the walkedPath
            walkedPath.RemoveLast();

            // we matched so increment the matchCount of our parent
            walkedPath.LastElement().MatchedElement.IncrementHashCount();
            return true;
        }
    }
}
