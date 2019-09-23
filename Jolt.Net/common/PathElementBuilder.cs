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

using System.Collections.Generic;

namespace Jolt.Net
{

    /**
     * Static utility class that creates PathElement(s) given a string key from a json spec document
     */
    public class PathElementBuilder
    {

        private PathElementBuilder() { }

        /**
         * Create a path element and ensures it is a Matchable Path Element
         */
        public static IMatchablePathElement BuildMatchablePathElement(string rawJsonKey)
        {
            IPathElement pe = PathElementBuilder.ParseSingleKeyLHS(rawJsonKey);

            if (!(pe is IMatchablePathElement mpe))
            {
                throw new SpecException("Spec LHS key=" + rawJsonKey + " is not a valid LHS key.");
            }

            return (IMatchablePathElement)mpe;
        }

        /**
         * Visible for Testing.
         *
         * Inspects the key in a particular order to determine the correct sublass of
         *  PathElement to create.
         *
         * @param origKey string that should represent a single PathElement
         * @return a concrete implementation of PathElement
         */
        public static IPathElement ParseSingleKeyLHS(string origKey)
        {

            string elementKey;  // the string to use to actually make Elements
            string keyToInspect;  // the string to use to determine which kind of Element to create

            if (origKey.Contains("\\"))
            {
                // only do the extra work of processing for escaped chars, if there is one.
                keyToInspect = SpecStringParser.RemoveEscapedValues(origKey);
                elementKey = SpecStringParser.RemoveEscapeChars(origKey);
            }
            else
            {
                keyToInspect = origKey;
                elementKey = origKey;
            }

            //// LHS single values
            if ("@" == keyToInspect)
            {
                return new AtPathElement(elementKey);
            }
            else if ("*" == keyToInspect)
            {
                return new StarAllPathElement(elementKey);
            }
            else if (keyToInspect.StartsWith("["))
            {

                if (StringTools.CountMatches(keyToInspect, "[") != 1 || StringTools.CountMatches(keyToInspect, "]") != 1)
                {
                    throw new SpecException("Invalid key:" + origKey + " has too many [] references.");
                }

                return new ArrayPathElement(elementKey);
            }
            //// LHS multiple values
            else if (keyToInspect.StartsWith("@") || keyToInspect.Contains("@("))
            {
                // The traspose path element gets the origKey so that it has it's escapes.
                return TransposePathElement.Parse(origKey);
            }
            else if (keyToInspect.Contains("@"))
            {
                throw new SpecException("Invalid key:" + origKey + " can not have an @ other than at the front.");
            }
            else if (keyToInspect.Contains("$"))
            {
                return new DollarPathElement(elementKey);
            }
            else if (keyToInspect.Contains("["))
            {

                if (StringTools.CountMatches(keyToInspect, "[") != 1 || StringTools.CountMatches(keyToInspect, "]") != 1)
                {
                    throw new SpecException("Invalid key:" + origKey + " has too many [] references.");
                }

                return new ArrayPathElement(elementKey);
            }
            else if (keyToInspect.Contains("&"))
            {

                if (keyToInspect.Contains("*"))
                {
                    throw new SpecException("Invalid key:" + origKey + ", Can't mix * with & ) ");
                }
                return new AmpPathElement(elementKey);
            }
            else if (keyToInspect.Contains("*"))
            {

                int numOfStars = StringTools.CountMatches(keyToInspect, "*");

                if (numOfStars == 1)
                {
                    return new StarSinglePathElement(elementKey);
                }
                else if (numOfStars == 2)
                {
                    return new StarDoublePathElement(elementKey);
                }
                else
                {
                    return new StarRegexPathElement(elementKey);
                }
            }
            else if (keyToInspect.Contains("#"))
            {
                return new HashPathElement(elementKey);
            }
            else
            {
                return new LiteralPathElement(elementKey);
            }
        }

        /**
         * Parse the dotNotation of the RHS.
         */
        public static List<IPathElement> ParseDotNotationRHS(string dotNotation)
        {
            string fixedNotation = SpecStringParser.FixLeadingBracketSugar(dotNotation);
            List<string> pathStrs = SpecStringParser.ParseDotNotation(new List<string>(), fixedNotation.GetEnumerator(), dotNotation);

            return ParseList(pathStrs, dotNotation);
        }

        /**
         * @param refDotNotation the original dotNotation string used for error messages
         * @return List of PathElements based on the provided List<string> keys
         */
        public static List<IPathElement> ParseList(List<string> keys, string refDotNotation)
        {
            var paths = new List<IPathElement>();

            foreach (string key in keys)
            {
                IPathElement path = ParseSingleKeyLHS(key);
                if (path is AtPathElement)
                {
                    throw new SpecException("'.@.' is not valid on the RHS: " + refDotNotation);
                }
                paths.Add(path);
            }

            return paths;
        }
    }
}
