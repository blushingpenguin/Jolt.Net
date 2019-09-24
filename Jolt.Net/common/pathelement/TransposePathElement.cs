/*
 * Copyright 2014 Bazaarvoice, Inc.
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
using System.Text;

namespace Jolt.Net
{

    /**
     * This PathElement is used by Shiftr to Transpose data.
     *
     * It can be used on the Left and Right hand sides of the spec.
     *
     * Input
     * {
     *   "author" : "Stephen Hawking",
     *   "book" : "A Brief History of Time"
     * }
     *
     * Wanted
     * {
     *   "Stephen Hawking" : "A Brief History of Time"
     * }
     *
     * The first part of the process is to allow a CompositeShiftr node to look down the input JSON tree.
     *
     * Spec
     * {
     *     "@author" : "@book"
     * }
     *
     *
     * Secondly, we can look up the tree, and come down a different path to locate data.
     *
     * For example of this see the following ShiftrUnit tests :
     *   LHS Lookup : json/shiftr/filterParents.json
     *   RHS Lookup : json/shiftr/transposeComplex6_rhs-complex-at.json
     *
     *
     * CanonicalForm Expansion
     *  Sugar
     *    "@2         -> "@(2,)
     *    "@(2)       -> "@(2,)
     *    "@author"   -> "@(0,author)"
     *    "@(author)" -> "@(0,author)"
     *
     *  Splenda
     *    "@(a.b)"    -> "@(0,a.b)"
     *    "@(a.&2.c)" -> "@(0,a.&(2,0).c)"
     */
    public class TransposePathElement : BasePathElement, IMatchablePathElement, IEvaluatablePathElement
    {
        private readonly int _upLevel;
        private readonly TransposeReader _subPathReader;
        private readonly string _canonicalForm;

        /**
         * Parse a text value from a Spec, into a TransposePathElement.
         *
         * @param key rawKey from a Jolt Spec file
         * @return a TransposePathElement
         */
        public static TransposePathElement Parse(string key)
        {
            if (key == null || key.Length < 2)
            {
                throw new SpecException("'Transpose Input' key '@', can not be null or of.Length 1.  Offending key : " + key);
            }
            if ('@' != key[0])
            {
                throw new SpecException("'Transpose Input' key must start with an '@'.  Offending key : " + key);
            }

            // Strip off the leading '@' as we don't need it anymore.
            string meat = key.Substring(1);

            if (meat.Contains("@"))
            {
                throw new SpecException("@ pathElement can not contain a nested @. Was: " + meat);
            }
            if (meat.Contains("*") || meat.Contains("[]"))
            {
                throw new SpecException("'Transpose Input' can not contain expansion wildcards (* and []).  Offending key : " + key);
            }

            // Check to see if the key is wrapped by parens
            if (meat.StartsWith("("))
            {
                if (meat.EndsWith(")"))
                {
                    meat = meat.Substring(1, meat.Length - 2);
                }
                else
                {
                    throw new SpecException("@ path element that starts with '(' must have a matching ')'.  Offending key : " + key);
                }
            }

            return InnerParse(key, meat);
        }

        /**
         * Parse the core of the TransposePathElement key, once basic errors have been checked and
         *  syntax has been handled.
         *
         * @param originalKey The original text for reference.
         * @param meat The string to actually parse into a TransposePathElement
         * @return TransposePathElement
         */
        private static TransposePathElement InnerParse(string originalKey, string meat)
        {
            char first = meat[0];
            if (Char.IsDigit(first))
            {
                // loop until we find a comma or end of string
                StringBuilder sb = new StringBuilder().Append(first);
                for (int index = 1; index < meat.Length; index++) {
                    char c = meat[index];

                    // when we find a / the first comma, stop looking for integers, and just assume the rest is a string path
                    if (',' == c)
                    {
                        if (!Int32.TryParse(sb.ToString(), out int upLevel))
                        {
                            // I don't know how this exception would get thrown, as all the chars were checked by isDigit, but oh well
                            throw new SpecException("@ path element with non/mixed numeric key is not valid, key=" + originalKey);
                        }

                        return new TransposePathElement(originalKey, upLevel, meat.Substring(index + 1));
                    }
                    else if (Char.IsDigit(c))
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        throw new SpecException("@ path element with non/mixed numeric key is not valid, key=" + originalKey);
                    }
                }

                // if we got out of the for loop, then the whole thing was a number.
                return new TransposePathElement(originalKey, Int32.Parse(sb.ToString()), null);
            }
            else
            {
                return new TransposePathElement(originalKey, 0, meat);
            }
        }

        /**
         * Private constructor used after parsing is done.
         *
         * @param originalKey for reference
         * @param upLevel How far up the tree to go
         * @param subPath Where to go down the tree
         */
        private TransposePathElement(string originalKey, int upLevel, string subPath) :
            base(originalKey)
        {
            _upLevel = upLevel;
            if (String.IsNullOrEmpty(subPath))
            {
                _subPathReader = null;
                _canonicalForm = "@(" + upLevel + ",)";
            }
            else
            {
                _subPathReader = new TransposeReader(subPath);
                _canonicalForm = "@(" + upLevel + "," + _subPathReader.GetCanonicalForm() + ")";
            }
        }

        /**
         * This method is used when the TransposePathElement is used on the LFH as data.
         *
         * Aka, normal "evaluate" returns either a Number or a string.
         *
         * @param walkedPath WalkedPath to evaluate against
         * @return The data specified by this TransposePathElement.
         */
        public JToken ObjectEvaluate(WalkedPath walkedPath)
        {
            // Grap the data we need from however far up the tree we are supposed to go
            PathStep pathStep = walkedPath.ElementFromEnd(_upLevel);

            if (pathStep == null)
            {
                return null;
            }

            var treeRef = pathStep.TreeRef;

            // Now walk down from that level using the subPathReader
            if (_subPathReader == null)
            {
                return treeRef;
            }
            else
            {
                return _subPathReader.Read(treeRef, walkedPath);
            }
        }

        public string Evaluate(WalkedPath walkedPath)
        {
            var data = ObjectEvaluate(walkedPath);

            if (data != null)
            {
                // Coerce a number into a string
                if (data.Type == JTokenType.Integer)
                {
                    // the idea here being we are looking for an array index value
                    return data.ToString();
                }

                // Coerce a boolean into a string
                if (data.Type == JTokenType.Boolean)
                {
                    return data.Value<bool>() ? "true" : "false";
                }

                if (data.Type == JTokenType.String)
                {
                    return data.ToString();
                }

                // If this output path has a TransposePathElement, and when we evaluate it
                //  it does not resolve to a string, then return null
            }
            return null;
        }

        public MatchedElement Match(string dataKey, WalkedPath walkedPath)
        {
            return walkedPath.LastElement().MatchedElement;  // copy what our parent was so that write keys of &0 and &1 both work.
        }

        public override string GetCanonicalForm() =>
            _canonicalForm;
    }
}
