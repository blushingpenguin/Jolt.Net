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
    public class FiltrUnitTest : JsonTest
    {
        public static IEnumerable<TestCaseData> GetFiltrUnitTestCases()
        {
            return new TestCaseData[]
            {
                new TestCaseData(
                    JObject.Parse("{ \"users.*\" : { \"accountType\" : \"user\" } }"),
                    JObject.Parse("{ \"users\": [{" +
                                    "\"accountId\": 1001," +
                                    "\"accountType\": \"builtIn\"" +
                                  "}, {" +
                                    "\"accountId\": 1002," +
                                    "\"accountType\": \"user\"" +
                                  "}] }"),
                    JObject.Parse("{ \"users\": [{" +
                                    "\"accountId\": 1002," +
                                    "\"accountType\": \"user\"" +
                                  "}] }")
                )
                {
                    TestName = "FiltrUnitTests(Simple constant match)"
                },
                new TestCaseData(
                    JObject.Parse("{ \"users.*\" : { \"accountType\" : \"user|(built.*)\" } }"),
                    JObject.Parse("{ \"users\": [{" +
                                    "\"accountId\": 1001," +
                                    "\"accountType\": \"built-in\"" +
                                  "}, {" +
                                    "\"accountId\": 1002," +
                                    "\"accountType\": \"user\"" +
                                  "}] }"),
                    JObject.Parse("{ \"users\": [{" +
                                    "\"accountId\": 1001," +
                                    "\"accountType\": \"built-in\"" +
                                  "}, {" +
                                    "\"accountId\": 1002," +
                                    "\"accountType\": \"user\"" +
                                  "}] }")
                )
                {
                    TestName = "FiltrUnitTests(Regex match)"
                }
            };
        }

        [TestCaseSource(nameof(GetFiltrUnitTestCases))]
        public void FiltrUnitTests(JObject spec, JObject data, JObject expected)
        {
            Filtr shiftr = new Filtr(spec);
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
                    JObject.Parse( "{ 'tuna': '[' }" )
                )
                {
                    TestName = "FailureUnitTest(Bad regexp)"
                }
            };
        }

        [TestCaseSource(nameof(GetBadSpecsTestCases))]
        public void FailureUnitTest(JToken spec)
        {
            FluentActions
                .Invoking(() => new Filtr(spec))
                .Should().Throw<SpecException>();
        }

        [TestCase("array")]
        [TestCase("arrayIndex")]
        [TestCase("nested")]
        [TestCase("re")]
        public void RunTest(string testCaseName)
        {
            var testCase = GetTestCase($"filtr/{testCaseName}");
            Filtr filtr = new Filtr(testCase.Spec);
            var actual = filtr.Transform(testCase.Input);

            actual.Should().BeEquivalentTo(testCase.Expected);
        }
    }
}
