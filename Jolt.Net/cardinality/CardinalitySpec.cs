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
using System.Collections.Generic;

namespace Jolt.Net
{
    /**
     * A Spec object represents a single line from the JSON Cardinality Spec.
     *
     * At a minimum a single Spec has :
     *   Raw LHS spec value
     *   Some kind of PathElement (based off that raw LHS value)
     *
     * Additionally there are 2 distinct subclasses of the base Spec
     *  CardinalityLeafSpec : where the RHS is either "ONE" or "MANY"
     *  CardinalityCompositeSpec : where the RHS is a map of children Specs
     *
     * The tree structure of formed by the CompositeSpecs is what is used during the transform
     *  to do the parallel tree walk with the input data tree.
     *
     * During the parallel tree walk, a Path<Literal PathElements> is maintained, and used when
     *  a tree walk encounters a leaf spec.
     */
    public abstract class CardinalitySpec : IBaseSpec
    {

        private const string STAR = "*";
        private const string AT = "@";

        // The processed key from the JSON config
        private readonly IMatchablePathElement _pathElement;

        public CardinalitySpec(string rawJsonKey)
        {
            var pathElements = Parse(rawJsonKey);

            if (pathElements.Count != 1)
            {
                throw new SpecException("CardinalityTransform invalid LHS:" + rawJsonKey + " can not contain '.'");
            }

            IPathElement pe = pathElements[0];
            if (!(pe is IMatchablePathElement mpe)) 
            {
                throw new SpecException("Spec LHS key=" + rawJsonKey + " is not a valid LHS key.");
            }

            _pathElement = mpe;
        }

        // once all the cardinalitytransform specific logic is extracted.
        public static List<IPathElement> Parse(string key)
        {
            var result = new List<IPathElement>();

            if (key.Contains(AT))
            {
                result.Add(new AtPathElement(key));
            }
            else if (STAR == key)
            {
                result.Add(new StarAllPathElement(key));
            }
            else if (key.Contains(STAR))
            {
                if (StringTools.CountMatches(key, STAR) == 1)
                {
                    result.Add(new StarSinglePathElement(key));
                }
                else
                {
                    result.Add(new StarRegexPathElement(key));
                }
            }
            else
            {
                result.Add(new LiteralPathElement(key));
            }

            return result;
        }

        /**
         * This is the main recursive method of the CardinalityTransform parallel "spec" and "input" tree walk.
         *
         * It should return true if this Spec object was able to successfully apply itself given the
         *  inputKey and input object.
         *
         * In the context of the CardinalityTransform parallel treewalk, if this method returns a non-null object,
         * the assumption is that no other sibling Cardinality specs need to look at this particular input key.
         *
         * @return true if this this spec "handles" the inputkey such that no sibling specs need to see it
         */
        public abstract bool ApplyCardinality(string inputKey, JToken input, WalkedPath walkedPath, JToken parentContainer);

        public bool Apply(string inputKey, JToken inputOptional, WalkedPath walkedPath, JObject output, JObject context)
        {
            return ApplyCardinality(inputKey, inputOptional, walkedPath, output);
        }

        public IMatchablePathElement GetPathElement() => _pathElement;
    }
}
