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
     * Leaf level Spec object.
     *
     * If this Spec's PathElement matches the input (successful parallel tree walk)
     *  this Spec has the information needed to write the given data to the output object.
     */
    public class ShiftrLeafSpec : ShiftrSpec
    {
        // traversal builder that uses a ShifterWriter to create a PathEvaluatingTraversal
        class ShiftrTraversalBuilder : TraversalBuilder<ShiftrWriter>
        {
            public override ShiftrWriter BuildFromPath(string path)
            {
                return new ShiftrWriter(path);
            }
        }

        protected static readonly TraversalBuilder<ShiftrWriter> TRAVERSAL_BUILDER = new ShiftrTraversalBuilder();

        // List of the processed version of the "write specifications"
        private readonly IReadOnlyList<PathEvaluatingTraversal> _shiftrWriters;

        public ShiftrLeafSpec(string rawKey, JToken rhs) :
            base(rawKey)
        {
            List<PathEvaluatingTraversal> writers;
            if (rhs.Type == JTokenType.String)
            {
                // leaf level so spec is an dot notation write path
                writers = new List<PathEvaluatingTraversal>();
                writers.Add(TRAVERSAL_BUILDER.Build(rhs));
            }
            else if (rhs is JArray rhsList)
            {
                // leaf level list
                // Spec : "foo": ["a", "b"] : Shift the value of "foo" to both "a" and "b"
                writers = new List<PathEvaluatingTraversal>(rhsList.Count);
                foreach (var dotNotation in rhsList)
                {
                    writers.Add(TRAVERSAL_BUILDER.Build(dotNotation));
                }
            }
            else if (rhs.Type == JTokenType.Null)
            {
                // this means someone wanted to match something, but not send it anywhere.  Basically like a removal.
                writers = new List<PathEvaluatingTraversal>();
            }
            else
            {
                throw new SpecException("Invalid Shiftr spec RHS.  Should be map, string, or array of strings.  Spec in question : " + rhs);
            }

            _shiftrWriters = writers.AsReadOnly();
        }

        /**
         * If this Spec matches the inputkey, then do the work of outputting data and return true.
         *
         * @return true if this this spec "handles" the inputkey such that no sibling specs need to see it
         */
        public override bool Apply(string inputKey, JToken inputOptional, WalkedPath walkedPath, JObject output, JObject context)
        {
            JToken input = inputOptional;
            MatchedElement thisLevel = _pathElement.Match(inputKey, walkedPath);
            if (thisLevel == null)
            {
                return false;
            }

            JToken data;
            bool realChild = false;  // by default don't block further Shiftr matches

            if (_pathElement is DollarPathElement ||
                _pathElement is HashPathElement)
            {
                // The data is already encoded in the thisLevel object created by the pathElement.match called above
                data = thisLevel.GetCanonicalForm();
            }
            else if (_pathElement is AtPathElement)
            {
                // The data is our parent's data
                data = input;
            }
            else if (_pathElement is TransposePathElement tpe)
            {
                // We try to walk down the tree to find the value / data we want

                // Note the data found may not be a string, thus we have to call the special objectEvaluate
                var evaledData = tpe.ObjectEvaluate(walkedPath);
                if (evaledData != null)
                {
                    data = evaledData;
                }
                else
                {
                    // if we could not find the value we want looking down the tree, bail
                    return false;
                }
            }
            else
            {
                // the data is the input
                data = input;
                // tell our parent that we matched and no further processing for this inputKey should be done
                realChild = true;
            }

            // Add our the LiteralPathElement for this level, so that write path References can use it as &(0,0)
            walkedPath.Add(input, thisLevel);

            // Write out the data
            foreach (PathEvaluatingTraversal outputPath in _shiftrWriters)
            {
                outputPath.Write(data, output, walkedPath);
            }

            walkedPath.RemoveLast();

            if (realChild)
            {
                // we were a "real" child, so increment the matchCount of our parent
                walkedPath.LastElement().MatchedElement.IncrementHashCount();
            }

            return realChild;
        }
    }
}
