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

namespace Jolt.Net
{

    /**
     *  PathElement for the a double "*" wildcard such as tag-*-*.   In this case we can avoid doing any
     *  regex work by doing string begins, ends and mid element exists.
     */
    public class StarDoublePathElement : BasePathElement, IStarPathElement
    {
        private readonly string _prefix;
        private readonly string _suffix;
        private readonly string _mid;

        /**+
         *
         * @param key : should be a string with two "*" elements.
         */
        public StarDoublePathElement(string key) :
            base(key)
        {
            if (StringTools.CountMatches(key, "*") != 2) {
                throw new ArgumentException(nameof(key), "StarDoublePathElement should have two '*' in its key. Was: " + key);
            }

            string[] split = key.Split(new string[] { "\\*" }, StringSplitOptions.None);
            bool startsWithStar = key.StartsWith("*");
            bool endsWithStar = key.EndsWith("*");
            if (startsWithStar && endsWithStar)
            {
                _prefix = "";
                _mid = split[1];
                _suffix = "";
            }
            else if (endsWithStar)
            {
                _prefix = split[0];
                _mid = split[1];
                _suffix = "";
            }
            else if (startsWithStar)
            {
                _prefix = "";
                _mid = split[1];
                _suffix = split[2];
            }
            else
            {
                _prefix = split[0];
                _mid = split[1];
                _suffix = split[2];
            }
        }

        /**
         * @param literal test to see if the provided string will match this Element's regex
         * @return true if the provided literal will match this Element's regex
         */
        public bool StringMatch(string literal)
        {
            bool isMatch = false;
            if (literal.StartsWith(_prefix) && literal.EndsWith(_suffix))
            {
                isMatch = FinMidIndex(literal) > 0;
            }
            return isMatch;
        }

        /**
         * The assumption here is: * means 1 or more characters. So, if we can find the mid 1 char after the prefix ends and 1 char before the suffix
         * starts, we have found a mid match. Also, it will be the first occurrence of the mid in the literal, so we are not 'greedy' to capture as much as
         * in the '*'
         */
        private int FinMidIndex(string literal)
        {
            int startOffset = _prefix.Length + 1;
            int endOffset = literal.Length - _suffix.Length - 1;

            /**
             * Found a bug when there is only character after the prefix ends. For eg: if the spec is abc-*$* and the key
             * we got is abc-1
             *      prefix -> abc-
             *      suffix -> ""
             *      mid    -> $
             *      startoffset -> 5
             *      endoffset -> 5 - 0 - 1 = 4
             *  We are left with no substring to search for the mid. Bail out!
             */
            if (startOffset >= endOffset)
            {

                return -1;

            }
            int midIndex = literal.Substring(startOffset, endOffset - startOffset + 1).IndexOf(_mid);

            if (midIndex >= 0)
            {

                return midIndex + startOffset;
            }
            return -1;
        }

        public MatchedElement Match(string dataKey, WalkedPath walkedPath)
        {
            if (StringMatch(dataKey))
            {
                var subKeys = new List<string>(2);

                int midStart = FinMidIndex(dataKey);
                int midEnd = midStart + _mid.Length;

                string firstStarPart = dataKey.Substring(_prefix.Length, midStart - _prefix.Length);
                subKeys.Add(firstStarPart);

                string secondStarPart = dataKey.Substring(midEnd, dataKey.Length - _suffix.Length);
                subKeys.Add(secondStarPart);

                return new MatchedElement(dataKey, subKeys);
            }
            return null;
        }

        public override string GetCanonicalForm() => RawKey; 
    }
}
