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
using System.Text;
using System.Text.RegularExpressions;

namespace Jolt.Net
{

    /**
     * Non-greedy * based Path Element.
     */
    public class StarRegexPathElement : BasePathElement, IStarPathElement {

        private readonly Regex _pattern;

    public StarRegexPathElement(string key) :
            base(key)
        {
            _pattern = MakePattern(key);
        }


        private static Regex MakePattern(string key)
        {
            // "rating-*-*"  ->  "^rating-(.+?)-(.+?)$"   aka the '*' must match something in a non-greedy way
            key = EscapeMetacharsIfAny(key);
            string regex = "^" + key.Replace("*", "(.+?)") + "$";

            /*
                wtf does "(.+?)" mean
                .  : match any character
                +  : match one or more of the previous thing
                ?  : match zero of one of the previous thing
                +? : reluctantly match

                See http://docs.oracle.com/javase/tutorial/essential/regex/quant.html
                  Differences Among Greedy, Reluctant, and Possessive Quantifiers section
            */

            return new Regex(regex, RegexOptions.Compiled);
        }

        // Metachars to escape .^$|*+?()[{\ in a regex

        /** +
         *
         * @param key : string key that needs to be escaped before compiling into regex.
         * @return : Metachar escaped key.
         *
         * Regex has some special meaning for the metachars [ .^$|*+?()[{\ ].If any of these metachars is present in the pattern key that was passed, it needs to be escaped so that
         * it can be matched against literal.
         */
        private static string EscapeMetacharsIfAny(string key)
        {
            var sb = new StringBuilder((key.Length * 3) / 2);
            foreach (var keychar in key)
            {
                switch (keychar)
                {
                    case '(':
                    case '[':
                    case '{':
                    case '\\':
                    case '^':
                    case '$':
                    case '|':
                    case ')':
                    case '?':
                    case '+':
                    case '.':
                        sb.Append('\\');
                        break;
                }
                sb.Append(keychar);
            }
            return sb.ToString();
        }

        /**
         * @param literal test to see if the provided string will match this Element's regex
         * @return true if the provided literal will match this Element's regex
         */
        public bool StringMatch(string literal)
        {
            return _pattern.IsMatch(literal);
        }

        public MatchedElement Match(string dataKey, WalkedPath walkedPath)
        {
            var result = _pattern.Match(dataKey);
            if (!result.Success)
            {
                return null;
            }

            int groupCount = result.Groups.Count;
            var subKeys = new List<string>();
            for (int index = 1; index <= result.Groups.Count; index++) {
                subKeys.Add(result.Groups[index].Value);
            }

            return new MatchedElement(dataKey, subKeys);
        }

        public override string GetCanonicalForm() => RawKey;
    }
}
