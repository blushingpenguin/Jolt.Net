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
    public class KeyOrderingTest
    {
        public static IEnumerable<TestCaseData> GetTestCases()
        {
            var tests = new object[][] {
                new object[]
                {
                    "Simple * and &",
                    JObject.Parse( "{ \"*\" : { \"a\" : \"b\" }, \"&\" : { \"a\" : \"b\" } }" ),
                    new string[] { "&(0,0)", "*" }
                },
                new object[]
                {
                    "2* and 2&",
                    JObject.Parse( "{ \"rating-*\" : { \"a\" : \"b\" }, \"rating-range-*\" : { \"a\" : \"b\" }, \"&\" : { \"a\" : \"b\" }, \"tuna-&(0)\" : { \"a\" : \"b\" } }" ),
                    new string[] { "tuna-&(0,0)", "&(0,0)", "rating-range-*", "rating-*" }
                },
                new object[]
                {
                    "2& alpha-number based fallback",
                    JObject.Parse( "{ \"&\" : { \"a\" : \"b\" }, \"&(0,1)\" : { \"a\" : \"b\" } }" ),
                    new string[] { "&(0,0)", "&(0,1)" }
                },
                new object[]
                {
                    "2* and 2& alpha fallback",
                    JObject.Parse( "{ \"aaaa-*\" : { \"a\" : \"b\" }, \"bbbb-*\" : { \"a\" : \"b\" }, \"aaaa-&\" : { \"a\" : \"b\" }, \"bbbb-&(0)\" : { \"a\" : \"b\" } }" ),
                    new string[] { "aaaa-&(0,0)", "bbbb-&(0,0)", "aaaa-*", "bbbb-*" }
                }
            };
            foreach (var test in tests)
            {
                yield return new TestCaseData(test.Skip(1).ToArray()) { TestName = $"RunTest({test[0]})" };
            }
        }

        [TestCaseSource(nameof(GetTestCases))]
        public void RunTest(JObject spec, string[] expectedOrder)
        {
            ShiftrCompositeSpec root = new ShiftrCompositeSpec( SpecDriven.ROOT_KEY, spec );

            for ( int index = 0; index < expectedOrder.Length; index++) 
            {
                var expected = expectedOrder[index];
                var actual = root.GetComputedChildren()[index].GetPathElement().GetCanonicalForm();
                actual.Should().Be(expected);
            }
        }
    }
}
