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

namespace Jolt.Net
{
    /**
     * All "References" extend this class and support three level of syntactic sugar
     * Example with the AmpReference
     *  1   "&"
     *  2   "&0"
     *  3   "&(0,0)"
     *  all three mean the same thing.
     *
     *  References are used to look up values in a WalkedPath.
     *  In the CanonicalForm the first entry is how far up the WalkedPath to look for a LiteralPathElement,
     *   and the second entry is which part of that LiteralPathElement to ask for.
     */
    public abstract class BasePathAndGroupReference : IPathAndGroupReference
    {
        private readonly int _keyGroup;     // equals 0 for "&"  "&0"  and  "&(x,0)"
        private readonly int _pathIndex;    // equals 0 for "&"  "&0"  and  "&(0,x)"

        protected abstract char GetToken();

        public BasePathAndGroupReference(string refStr)
        {
            if (string.IsNullOrEmpty(refStr) || GetToken() != refStr[0])
            {
                throw new SpecException("Invalid reference key=" + refStr + " either blank or doesn't start with correct character=" + GetToken());
            }

            int pI = 0;
            int kG = 0;

            try
            {
                if (refStr.Length > 1)
                {
                    string meat = refStr.Substring(1);

                    if (meat.Length >= 3 && meat.StartsWith("(") && meat.EndsWith(")"))
                    {
                        // "&(1,2)" -> "1,2".split( "," ) -> string[] { "1", "2" }    OR
                        // "&(3)"   -> "3".split( "," ) -> string[] { "3" }

                        string parenMeat = meat.Substring(1, meat.Length - 2);
                        string[] intStrs = parenMeat.Split(',');
                        if (intStrs.Length > 2)
                        {
                            throw new SpecException("Invalid Reference=" + refStr);
                        }

                        pI = Int32.Parse(intStrs[0]);
                        if (intStrs.Length == 2)
                        {
                            kG = Int32.Parse(intStrs[1]);
                        }
                    }
                    else // &2
                    {
                        pI = Int32.Parse(meat);
                    }
                }
            }
            catch (FormatException nfe)
            {
                throw new SpecException("Unable to parse '" + GetToken() + "' reference key:" + refStr, nfe);
            }

            if (pI < 0 || kG < 0)
            {
                throw new SpecException("Reference:" + refStr + " can not have a negative value.");
            }

            _pathIndex = pI;
            _keyGroup = kG;
        }

        public int GetPathIndex() => _pathIndex;

        public int GetKeyGroup() => _keyGroup;

        /**
         * Builds the non-syntactic sugar / maximally expanded and unique form of this reference.
         * @return canonical form : aka "&" -> "&(0,0)
         */
        public string GetCanonicalForm() =>
            GetToken() + "(" + _pathIndex + "," + _keyGroup + ")";
    }
}
