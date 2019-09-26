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
using System.Collections.Generic;
using System.Linq;

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class ShiftrTraversrTest
    {
        public static IEnumerable<TestCaseData> GetTestCases(string testName)
        {
            var tests = new object[][]
            {
                new object[]
                {
                    "simple place",
                    new string[] { "tuna" },
                    new JValue("tuna"),
                    "a.b",
                    new string[] { "a", "b" },
                    JObject.Parse( "{ \"a\" : { \"b\" : \"tuna\" } }" )
                },
                new object[]
                {
                    "simple explicit array place",
                    new string[] { "tuna" },
                    null,
                    "a.b[]",
                    new string[] { "a", "b", "[]" },
                    JObject.Parse("{ \"a\" : { \"b\" : [ \"tuna\" ] } }")
                },
                new object[]
                {
                    "simple explicit array place with sub",
                    new string[] { "tuna" },
                    null,
                    "a.b[].c",
                    new string[] { "a", "b", "[]", "c" },
                    JObject.Parse("{ \"a\" : { \"b\" : [ { \"c\" : \"tuna\" } ] } }")
                },
                new object[]
                {
                    "simple array place",
                    new string[] { "tuna" },
                    new JValue("tuna"),
                    "a.b.[1]",
                    new string[] { "a", "b", "1" },
                    JObject.Parse("{ \"a\" : { \"b\" : [ null, \"tuna\" ] } }")
                },
                new object[]
                {
                    "nested array place",
                    new string[] { "tuna" },
                    new JValue("tuna"),
                    "a.b[1].c",
                    new string[] { "a", "b", "1", "c" },
                    JObject.Parse("{ \"a\" : { \"b\" : [ null, { \"c\" : \"tuna\" } ] } }")
                },
                new object[]
                {
                    "simple place into write array",
                    new string[] { "tuna", "marlin" },
                    new JArray("tuna", "marlin"),
                    "a.b",
                    new string[] { "a", "b" },
                    JObject.Parse("{ \"a\" : { \"b\" : [ \"tuna\", \"marlin\" ] } }")
                },
                new object[]
                {
                    "simple array place with nested write array",
                    new string[] { "tuna", "marlin" },
                    new JArray("tuna", "marlin"),
                    "a.b.[1]",
                    new string[] { "a", "b", "1" },
                    JObject.Parse("{ \"a\" : { \"b\" : [ null, [ \"tuna\", \"marlin\" ] ] } }")
                },
                new object[]
                {
                    "nested array place with nested output array",
                    new string[] { "tuna", "marlin" },
                    new JArray("tuna", "marlin"),
                    "a.b.[1].c",
                    new string[] { "a", "b", "1", "c" },
                    JObject.Parse("{ \"a\" : { \"b\" : [ null, { \"c\" : [ \"tuna\", \"marlin\"] } ] } }")
                }
            };
            foreach (var test in tests)
            {
                yield return new TestCaseData(test.Skip(1).ToArray()) { TestName = $"{testName}({test[0]})" };
            }
        }

        public static IEnumerable<TestCaseData> SetTests() =>
            GetTestCases("SetTest");

        public static IEnumerable<TestCaseData> GetTests() =>
            GetTestCases("GetTest");

        [TestCaseSource(nameof(SetTests))]
        public void SetTest(string[] outputs, JToken notUsedInThisTest, string traversrPath, string[] keys, JObject expected)
        {
            var actual = new JObject();

            Traversr traversr = new ShiftrTraversr(traversrPath);
            foreach (var output in outputs)
            {
                traversr.Set(actual, keys, output);
            }

            actual.Should().BeEquivalentTo(expected);
        }

        [TestCaseSource(nameof(GetTests))]
        public void GetTest(string[] notUsedInThisTest, JToken expected, string traversrPath, string[] keys, JObject tree)
        {
            var traversr = new ShiftrTraversr(traversrPath );
            var actual = traversr.Get(tree, keys);
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
