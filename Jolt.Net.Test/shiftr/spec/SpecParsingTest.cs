/*
 * Copyright 2015 Bazaarvoice, Inc.
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

using FluentAssertions;
using FluentAssertions.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class SpecParsingTest
    {
        public static IEnumerable<TestCaseData> RHSParsingTestsRemoveEscapes()
        {
            var tests = new object[][] {
                new object[]
                {
                    "simple, no escape",
                    "a.b.c",
                    new string[] { "a", "b", "c" },
                },
                new object[]
                {
                    "ref and array, no escape",
                    "a.&(1,2).[]",
                    new string[] { "a", "&(1,2)", "[]" }
                },
                new object[]
                {
                    "single transpose, no escape",
                    "a.@(l.m.n).c",
                    new string[] { "a", "@(l.m.n)", "c" }
                },
                new object[]
                {
                    "non-special char escape passes thru",
                    "a\\\\bc.def",
                    new string[] { "a\\bc", "def" }
                },
                new object[]
                {
                    "single escape",
                    "a\\.b.c",
                    new string[] { "a.b", "c" }
                },
                new object[]
                {
                    "escaping rhs",
                    "data.\\\\$rating-&1",
                    new string[] { "data", "\\$rating-&1" }
                },
                new object[]
                {
                    "@Class example",
                    "a.@Class.c",
                    new string[] { "a", "@(Class)", "c" }
                }
            };
            foreach (var test in tests)
            {
                yield return new TestCaseData(test.Skip(1).ToArray()) { TestName = $"TestRHSParsingRemoveEscapes({test[0]})" };
            }
        }

        [TestCaseSource(nameof(RHSParsingTestsRemoveEscapes))]
        public void TestRHSParsingRemoveEscapes(string unSweetendDotNotation, string[] expected)
        {
            List<String> actual = SpecStringParser.ParseDotNotation(new List<string>(),
                unSweetendDotNotation.GetEnumerator(), unSweetendDotNotation );
            actual.Should().BeEquivalentTo(expected);
        }
        
        [TestCase("\\@pants", "@pants", TestName = "TestRemoveEscapeChars(starts with escape)")]
        [TestCase("rating-\\&pants", "rating-&pants", TestName = "TestRemoveEscapeChars(escape in the middle)")]
        [TestCase("rating\\\\pants", "rating\\pants", TestName = "TestRemoveEscapeChars(escape the escape char)")]
        public void TestRemoveEscapeChars(string input, string expected)
        {
            var actual = SpecStringParser.RemoveEscapeChars(input);
            actual.Should().Be(expected);
        }

        [TestCase("\\@pants", "pants", TestName = "TestEscapeParsing(starts with escape)")]
        [TestCase("rating-\\&pants", "rating-pants", TestName = "TestEscapeParsing(escape in the middle)")]
        [TestCase("rating\\\\pants", "ratingpants", TestName = "TestEscapeParsing(escape the escape char)")]
        [TestCase("\\[\\]pants", "pants", TestName = "TestEscapeParsing(escape the array)")]
        public void TestEscapeParsing(string input, string expected)
        {
            var actual = SpecStringParser.RemoveEscapedValues(input);
            actual.Should().Be(expected);
        }
    }
}
