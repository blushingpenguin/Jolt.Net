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
     * Static utility methods for handling specStrings such that we can process them into
     * usable formats for further processing into PathElement objects
     */
    public static class SpecStringParser
    {
        /**
         * Method that recursively parses a dotNotation string based on an iterator.
         *
         * This method will call out to parseAtPathElement
         *
         * @param pathStrings List to store parsed Strings that each represent a PathElement
         * @param iter the iterator to pull characters from
         * @param dotNotationRef the original dotNotation string used for error messages
         * @return evaluated List<string> from dot notation string spec
         */
        public static List<string> ParseDotNotation(
            List<string> pathStrings, IEnumerator<char> iter, string dotNotationRef)
        {
            if (!iter.MoveNext())
            {
                return pathStrings;
            }

            // Leave the forward slashes, unless it precedes a "."
            // The way this works is always suppress the forward slashes, but add them back in if the next char is not a "."
            bool prevIsEscape = false;
            bool currIsEscape = false;
            bool isPartOfArray = false;
            StringBuilder sb = new StringBuilder();

            char c;
            do
            {
                c = iter.Current;

                currIsEscape = false;
                if (c == '\\' && !prevIsEscape)
                {
                    // current is Escape only if the char is escape, or
                    //  it is an Escape and the prior char was, then don't consider this one an escape
                    currIsEscape = true;
                }

                if (prevIsEscape && c != '.' && c != '\\')
                {
                    sb.Append('\\');
                    sb.Append(c);
                }
                else if (c == '@')
                {
                    sb.Append('@');
                    sb.Append(ParseAtPathElement(iter, dotNotationRef));

                    //                      there was a "[" seen       but no "]"
                    // bool isPartOfArray = sb.IndexOf("[") != -1 && sb.IndexOf("]") == -1;
                    if (!isPartOfArray)
                    {
                        pathStrings.Add(sb.ToString());
                        sb = new StringBuilder();
                    }
                }
                else if (c == '.')
                {
                    if (prevIsEscape)
                    {
                        sb.Append('.');
                    }
                    else
                    {
                        if (sb.Length != 0)
                        {
                            pathStrings.Add(sb.ToString());
                        }
                        return ParseDotNotation(pathStrings, iter, dotNotationRef);
                    }
                }
                else if (!currIsEscape)
                {
                    if (c == '[')
                    {
                        isPartOfArray = true;
                    }
                    else if (c == ']')
                    {
                        isPartOfArray = false;
                    }
                    sb.Append(c);
                }

                prevIsEscape = currIsEscape;
            }
            while (iter.MoveNext());

            if (sb.Length != 0)
            {
                pathStrings.Add(sb.ToString());
            }
            return pathStrings;
        }

        /**
         * Helper method to turn a string into an Iterator<Character>
         */
        public static IEnumerator<char> StringIterator(string s)
        {
            // Ensure the error is found as soon as possible.
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }
            return s.GetEnumerator();
        }

        /**
         * Given a dotNotation style outputPath like "data[2].&(1,1)", this method fixes the syntactic sugar
         * of "data[2]" --> "data.[2]"
         *
         * This makes all the rest of the string processing easier once we know that we can always
         * split on the '.' character.
         *
         * @param dotNotaton Output path dot notation
         * @return
         */
        // TODO Unit Test this
        public static string FixLeadingBracketSugar(string dotNotation)
        {
            if (String.IsNullOrEmpty(dotNotation))
            {
                return "";
            }

            char prev = dotNotation[0];
            var sb = new StringBuilder();
            sb.Append(prev);

            for (int index = 1; index < dotNotation.Length; index++)
            {
                char curr = dotNotation[index];

                if (curr == '[' && prev != '\\')
                {
                    if (prev == '@' || prev == '.')
                    {
                        // no need to add an extra '.'
                    }
                    else
                    {
                        sb.Append('.');
                    }
                }

                sb.Append(curr);
                prev = curr;
            }

            return sb.ToString();
        }

        /**
         * Parse RHS Transpose @ logic.
         * "@(a.b)"  --> pulls "(a.b)" off the iterator
         * "@a.b"    --> pulls just "a" off the iterator
         *
         * This method expects that the the '@' character has already been seen.
         *
         * @param iter iterator to pull data from
         * @param dotNotationRef the original dotNotation string used for error messages
         */
        // TODO Unit Test this
        public static string ParseAtPathElement(IEnumerator<char> iter, string dotNotationRef)
        {
            if (!iter.MoveNext())
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();

            // Strategy here is to walk thru the string looking for matching parenthesis.
            // '(' increments the count, while ')' decrements it
            // If we ever get negative there is a problem.
            bool isParensAt = false;
            int atParensCount = 0;

            char c = iter.Current;
            if (c == '(')
            {
                isParensAt = true;
                atParensCount++;
            }
            else if (c == '.')
            {
                throw new SpecException("Unable to parse dotNotation, invalid TransposePathElement : " + dotNotationRef);
            }

            sb.Append(c);

            while (iter.MoveNext())
            {
                c = iter.Current;
                sb.Append(c);

                // Parsing "@(a.b.[&2])"
                if (isParensAt)
                {
                    if (c == '(')
                    {
                        throw new SpecException("Unable to parse dotNotation, too many open parens '(' : " + dotNotationRef);
                    }
                    else if (c == ')')
                    {
                        atParensCount--;
                    }

                    if (atParensCount == 0)
                    {
                        return sb.ToString();
                    }
                    else if (atParensCount < 0)
                    {
                        throw new SpecException("Unable to parse dotNotation, specifically the '@()' part : " + dotNotationRef);
                    }
                }
                // Parsing "@abc.def, return a canonical form of "@(abc)" and leave the "def" in the iterator
                else if (c == '.')
                {
                    return "(" + sb.ToString(0, sb.Length - 1) + ")";
                }
            }

            // if we got to the end of the string and we have mismatched parenthesis throw an exception.
            if (isParensAt && atParensCount != 0)
            {
                throw new SpecException("Invalid @() pathElement from : " + dotNotationRef);
            }
            // Parsing "@abc"
            return sb.ToString();
        }

        // Visible for Testing
        // given "\@pants" -> "pants"                 starts with escape
        // given "rating-\&pants" -> "rating-pants"   escape in the middle
        // given "rating\\pants" -> "ratingpants"     escape the escape char
        public static string RemoveEscapedValues(string origKey)
        {
            var sb = new StringBuilder();

            bool prevWasEscape = false;
            foreach (char c in origKey)
            {
                if ('\\' == c)
                {
                    if (prevWasEscape)
                    {
                        prevWasEscape = false;
                    }
                    else
                    {
                        prevWasEscape = true;
                    }
                }
                else
                {
                    if (!prevWasEscape)
                    {
                        sb.Append(c);
                    }
                    prevWasEscape = false;
                }
            }

            return sb.ToString();
        }

        // Visible for Testing
        // given "\@pants" -> "@pants"                 starts with escape
        // given "rating-\&pants" -> "rating-&pants"   escape in the middle
        // given "rating\\pants" -> "rating\pants"     escape the escape char
        public static string RemoveEscapeChars(string origKey)
        {
            var sb = new StringBuilder();

            bool prevWasEscape = false;
            foreach (char c in origKey)
            {
                if ('\\' == c)
                {
                    if (prevWasEscape)
                    {
                        sb.Append(c);
                        prevWasEscape = false;
                    }
                    else
                    {
                        prevWasEscape = true;
                    }
                }
                else
                {
                    sb.Append(c);
                    prevWasEscape = false;
                }
            }

            return sb.ToString();
        }

        public static List<string> ParseFunctionArgs(string argString)
        {
            var argsList = new List<string>();
            int firstBracket = argString.IndexOf('(');

            string className = argString.Substring(0, firstBracket);
            argsList.Add(className);

            // drop the first and last ( )
            argString = argString.Substring(firstBracket + 1, argString.Length - 2 - firstBracket);

            var sb = new StringBuilder();
            bool inBetweenBrackets = false;
            bool inBetweenQuotes = false;
            for (int i = 0; i < argString.Length; i++)
            {
                char c = argString[i];
                switch (c)
                {
                    case '(':
                        if (!inBetweenQuotes)
                        {
                            inBetweenBrackets = true;
                        }
                        sb.Append(c);
                        break;
                    case ')':
                        if (!inBetweenQuotes)
                        {
                            inBetweenBrackets = false;
                        }
                        sb.Append(c);
                        break;
                    case '\'':
                        inBetweenQuotes = !inBetweenQuotes;
                        sb.Append(c);
                        break;
                    case ',':
                        if (!inBetweenBrackets && !inBetweenQuotes)
                        {
                            argsList.Add(sb.ToString().Trim());
                            sb = new StringBuilder();
                            break;
                        }
                        sb.Append(',');
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            argsList.Add(sb.ToString().Trim());
            return argsList;
        }
    }
}
