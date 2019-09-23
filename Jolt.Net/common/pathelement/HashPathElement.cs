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
     * For use on the LHS, allows the user to specify an explicit string to write out.
     * Aka given a input that is boolean, would want to write something out other than "true" / "false".
     */
    public class HashPathElement : BasePathElement, IMatchablePathElement
    {
        private readonly string _keyValue;

        public HashPathElement(string key) :
            base(key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new SpecException("HashPathElement cannot have empty string as input.");
            }

            if (!key.StartsWith("#"))
            {
                throw new SpecException("LHS # should start with a # : " + key);
            }

            if (key.Length <= 1) 
            {
                throw new SpecException("HashPathElement input is too short : " + key);
            }


            if (key[0] == '(')
            {
                if (key[key.Length - 1] == ')')
                {
                    _keyValue = key.Substring(2, key.Length - 2);
                }
                else
                {
                    throw new SpecException("HashPathElement, mismatched parens : " + key);
                }
            }
            else
            {
                _keyValue = key.Substring(1);
            }
        }

        public override string GetCanonicalForm() =>
            "#(" + _keyValue + ")";

        public MatchedElement Match(string dataKey, WalkedPath walkedPath) =>
            new MatchedElement(_keyValue);
    }
}
