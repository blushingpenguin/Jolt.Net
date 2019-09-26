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

using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class ShiftrUnitTest 
    {
        public static IEnumerable<TestCaseData> GetShiftrUnitTestCases()
        {
            return new TestCaseData[]
            {
                new TestCaseData(
                    JObject.Parse("{ \"tuna-*-marlin-*\" : { \"rating-*\" : \"&(1,2).&.value\" } }"),
                    JObject.Parse("{ \"tuna-A-marlin-AAA\" : { \"rating-BBB\" : \"bar\" } }"),
                    JObject.Parse("{ \"AAA\" : { \"rating-BBB\" : { \"value\" : \"bar\" } } }")
                )
                {
                    TestName = "ShiftrUnitTests(Simple * and Reference)"
                },
                new TestCaseData(
                    JObject.Parse("{ \"tuna-*-marlin-*\" : { \"rating-*\" : [ \"&(1,2).&.value\", \"foo\"] } }"),
                    JObject.Parse("{ \"tuna-A-marlin-AAA\" : { \"rating-BBB\" : \"bar\" } }"),
                    JObject.Parse("{ \"foo\" : \"bar\", \"AAA\" : { \"rating-BBB\" : { \"value\" : \"bar\" } } }")
                )
                {
                    TestName = "ShiftrUnitTests(Shift to two places)"
                },
                new TestCaseData(
                    JObject.Parse("{ \"tuna|marlin\" : \"&-write\" }"),
                    JObject.Parse("{ \"tuna\" : \"snapper\" }"),
                    JObject.Parse("{ \"tuna-write\" : \"snapper\" }")
                )
                {
                    TestName = "ShiftrUnitTests(Or)"
                },
                new TestCaseData(
                    JObject.Parse("{ \"rating-*\" : { \"&(0,1)\" : { \"match\" : \"&\" } } }"),
                    JObject.Parse("{ \"rating-a\" : { \"a\" : { \"match\": \"a-match\" }, \"random\" : { \"match\" : \"noise\" } }," +
                            "              \"rating-c\" : { \"c\" : { \"match\": \"c-match\" }, \"random\" : { \"match\" : \"noise\" } } }"),
                    JObject.Parse("{ \"match\" : [ \"a-match\", \"c-match\" ] }")
                )
                {
                    TestName = "ShiftrUnitTests(KeyRef)"
                },
                new TestCaseData(
                    JObject.Parse("{ \"tuna-*-marlin-*\" : { \"rating-*\" : \"tuna[&(1,1)].marlin[&(1,2)].&(0,1)\" } }"),
                    JObject.Parse("{ \"tuna-2-marlin-3\" : { \"rating-BBB\" : \"bar\" }," +
                                            "\"tuna-1-marlin-0\" : { \"rating-AAA\" : \"mahi\" } }"),
                    JObject.Parse("{ \"tuna\" : [ null, " +
                            "                           { \"marlin\" : [ { \"AAA\" : \"mahi\" } ] }, " +
                            "                           { \"marlin\" : [ null, null, null, { \"BBB\" : \"bar\" } ] } " +
                            "                         ] " +
                            "            }")
                )
                {
                    TestName = "ShiftrUnitTests(Complex array write)"
                }
            };
        }

        [TestCaseSource(nameof(GetShiftrUnitTestCases))]
        public void ShiftrUnitTests(JObject spec, JObject data, JObject expected)
        {
            Shiftr shiftr = new Shiftr(spec);
            var actual = shiftr.Transform(data);
            actual.Should().BeEquivalentTo(expected);
        }

        public static IEnumerable<TestCaseData> GetBadSpecsTestCases()
        {
            return new TestCaseData[]
            {
                new TestCaseData(null)
                {
                    TestName = "FailureUnitTest(Null Spec)"
                },
                new TestCaseData(new JArray())
                {
                    TestName = "FailureUnitTest(List Spec)"
                },
                new TestCaseData(
                    JObject.Parse( "{ }" )
                )
                {
                    TestName = "FailureUnitTest(Empty spec)",
                },
                new TestCaseData(
                    JObject.Parse( "{ 'tuna' : {} }" )
                )
                {
                    TestName = "FailureUnitTest(Empty sub-spec)"
                },
                new TestCaseData(
                    JObject.Parse( "{ 'tuna-*-marlin-*' : { 'rating-@' : '&(1,2).&.value' } }" )
                )
                {
                    TestName = "FailureUnitTest(Bad @)"
                },
                new TestCaseData(
                    JObject.Parse( "{ 'tuna-*-marlin-*' : { 'rating-*' : '&(1,2).@.value' } }" )
                )
                {
                    TestName = "FailureUnitTest(RHS @ by itself)"
                },
                new TestCaseData(
                    JObject.Parse( "{ 'tuna-*-marlin-*' : { 'rating-*' : '&(1,2).@(data.&(1,1).value' } }" )
                )
                {
                    TestName = "FailureUnitTest(RHS @ with bad Parens)"
                },
                new TestCaseData(
                    JObject.Parse( "{ 'tuna-*-marlin-*' : { 'rating-*' : '&(1,2).*.value' } }")
                )
                {
                    TestName = "FailureUnitTest(RHS *)"
                },
                new TestCaseData(
                    JObject.Parse( "{ 'tuna-*-marlin-*' : { 'rating-*' : '&(1,2).$.value' } }")
                )
                {
                    TestName = "FailureUnitTest(RHS $)"
                },
                new TestCaseData(
                    JObject.Parse("{ 'tuna-*-marlin-*' : { 'rating-*' : [ '&(1,2).photos[&(0,1)]-subArray[&(1,2)].value', 'foo'] } }")
                )
                {
                    TestName = "FailureUnitTest(Two Arrays)",
                },
                new TestCaseData(
                    JObject.Parse("{ 'tuna-*-marlin-*' : { 'rating-&(1,2)-*' : [ '&(1,2).value', 'foo'] } }")
                )
                {
                    TestName = "FailureUnitTest(Can't mix * and & in the same key)",
                },
                new TestCaseData(
                    JObject.Parse("{ 'tuna' : 'marlin[-1]' }")
                )
                {
                    TestName = "FailureUnitTest(Don't put negative numbers in array references)",
                }
            };
        }

        [TestCaseSource(nameof(GetBadSpecsTestCases))]
        public void FailureUnitTest(JToken spec)
        {
            FluentActions
                .Invoking(() => new Shiftr(spec))
                .Should().Throw<SpecException>();
        }

        /**
         * @return canonical dotNotation String built from the given paths
         */
        public string BuildCanonicalString(List<IPathElement> paths) =>
            String.Join('.', paths.Select(x => x.GetCanonicalForm()));

        [TestCase("@a", "@(0,a)", TestName = "ValidRHSTests(#1)")]
        [TestCase("@abc", "@(0,abc)", TestName = "ValidRHSTests(#2)")]
        [TestCase("@a.b.c", "@(0,a).b.c", TestName = "ValidRHSTests(#3)")]
        [TestCase("@(a.b\\.c)", "@(0,a.b\\.c)", TestName = "ValidRHSTests(#4)")]
        [TestCase("@a.b.c", "@(0,a).b.c", TestName = "ValidRHSTests(#5)")]
        [TestCase("@a.b.@c", "@(0,a).b.@(0,c)", TestName = "ValidRHSTests(#6)")]
        [TestCase("@(a[2].&).b.@c", "@(0,a.[2].&(0,0)).b.@(0,c)", TestName = "ValidRHSTests(#7)")]
        [TestCase("a[&2].@b[1].c", "a.[&(2,0)].@(0,b).[1].c", TestName = "ValidRHSTests(#8)")]
        public void ValidRHSTests(string dotNotation, string expected)
        {
            var paths = PathElementBuilder.ParseDotNotationRHS(dotNotation);
            string actualCanonicalForm = BuildCanonicalString(paths);
            actualCanonicalForm.Should().Be(expected);
        }

        [Test]
        public void TestTransposePathParsing() 
        {
            var paths = PathElementBuilder.ParseDotNotationRHS( "test.@(2,foo\\.bar)" );
            paths.Count.Should().Be(2);
            var actualApe = (TransposePathElement)paths[1];
            actualApe.GetCanonicalForm().Should().Be("@(2,foo\\.bar)");
        }

        [TestCase("@", TestName = "FailureRHSTests(naked at)")]
        [TestCase("a@", TestName = "FailureRHSTests(missing suffix)")]
        [TestCase("@a@b", TestName = "FailureRHSTests(missing prefix)")]
        [TestCase("@(a.b.&(2,2)", TestName = "FailureRHSTests(missing trailing bracket #1)")]
        [TestCase("@(a.b.&(2,2).d", TestName = "FailureRHSTests(missing trailing bracket #2)")]
        [TestCase("@(a.b.@c).d", TestName = "FailureRHSTests(missing prefix in brackets)")]
        [TestCase("@(a.*.c)", TestName = "FailureRHSTests(@ can not contain a *)")]
        [TestCase("@(a.$2.c)", TestName = "FailureRHSTests(@ can not contain a $)")]
        public void FailureRHSTests(string dotNotation)
        {
            FluentActions
                .Invoking(() => PathElementBuilder.ParseDotNotationRHS(dotNotation))
                .Should().Throw<SpecException>();
        }
    }
}
