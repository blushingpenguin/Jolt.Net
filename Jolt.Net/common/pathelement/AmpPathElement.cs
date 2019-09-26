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
using System.Text;

namespace Jolt.Net
{
    /**
     * PathElement class that handles keys with & values, like input: "photos-&(1,1)""
     * It breaks down the string into a series of string or Reference tokens, that can be used to
     * 1) match input like "photos-5" where "&(1,1)" evaluated to 5
     */
    public class AmpPathElement : BasePathElement, IMatchablePathElement, IEvaluatablePathElement
    {
        private readonly IReadOnlyList<object> _tokens;
        private readonly string _canonicalForm;

        public AmpPathElement(string key) :
            base(key)
        {
            var literal = new StringBuilder();
            var canonicalBuilder = new StringBuilder();

            var tok = new List<object>();
            int index = 0;
            while (index < key.Length)
            {
                char c = key[index];

                // beginning of reference
                if (c == '&')
                {

                    // store off any literal text captured thus far
                    if (literal.Length > 0)
                    {
                        tok.Add(literal.ToString());
                        canonicalBuilder.Append(literal);
                        literal.Clear();
                    }

                    int refEnd = FindEndOfReference(key.Substring(index + 1));
                    AmpReference ref_ = new AmpReference(key.Substring(index, refEnd + 1));
                    canonicalBuilder.Append(ref_.GetCanonicalForm());

                    tok.Add(ref_);
                    index += refEnd;
                }
                else
                {
                    literal.Append(c);
                }
                index++;
            }
            if (literal.Length > 0)
            {
                tok.Add(literal.ToString());
                canonicalBuilder.Append(literal.ToString());
            }
            tok.TrimExcess();

            _tokens = tok.AsReadOnly();
            _canonicalForm = canonicalBuilder.ToString();
        }

        private static int FindEndOfReference(string key)
        {
            if ("" == key)
            {
                return 0;
            }

            for (int index = 0; index < key.Length; index++)
            {
                char c = key[index];
                // keep going till we see something other than a digit, parens, or comma
                if (!Char.IsDigit(c) && c != '(' && c != ')' && c != ',')
                {
                    return index;
                }
            }
            return key.Length;
        }

        public override string GetCanonicalForm() =>
            _canonicalForm;

        // Visible for testing
        public IReadOnlyList<object> GetTokens() =>
            _tokens;

        public string Evaluate(WalkedPath walkedPath)
        {
            // Walk thru our tokens and build up a string
            // Use the supplied Path to fill in our token References
            StringBuilder output = new StringBuilder();

            foreach (object token in _tokens)
            {
                if (token is string stoken)
                {
                    output.Append(stoken);
                }
                else
                {
                    AmpReference ref_ = (AmpReference)token;
                    MatchedElement matchedElement = walkedPath.ElementFromEnd(ref_.GetPathIndex()).MatchedElement;
                    string value = matchedElement.GetSubKeyRef(ref_.GetKeyGroup());
                    output.Append(value);
                }
            }

            return output.ToString();
        }

        public MatchedElement Match(string dataKey, WalkedPath walkedPath)
        {
            string evaled = Evaluate(walkedPath);
            if (evaled == dataKey)
            {
                return new MatchedElement(evaled);
            }
            return null;
        }
    }
}
