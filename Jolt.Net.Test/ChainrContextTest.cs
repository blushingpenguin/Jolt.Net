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

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class ChainrContextTest : JsonTest
    {
        public static IEnumerable<TestCaseData> Tests
        {
            get
            {
                var testSuite = GetJson("chainr/context/spec_with_context");
                
                foreach (var testCase in testSuite["tests"])
                {
                    var name = testCase["testCaseName"].ToString();
                    var tcd = new TestCaseData(
                        name,
                        testSuite["spec"],
                        testCase["input"],
                        (JObject)testCase["context"],
                        testCase["expected"]
                    );
                    tcd.SetName($"RunTest({name})");
                    yield return tcd;
                }
            }
        }

        [TestCaseSource(nameof(Tests))]
        public void RunTest(string testCaseName, JToken spec, JToken input, JObject context, JToken expected)
        {
            Chainr unit = Chainr.FromSpec( spec, TestTransforms.Transforms );

            unit.HasContextualTransforms().Should().BeTrue();
            unit.GetContextualTransforms().Count.Should().Be(2);

            var actual = unit.Transform( input, context );

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
