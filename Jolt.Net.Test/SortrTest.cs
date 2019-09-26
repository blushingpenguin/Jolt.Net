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

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class SortrTest : JsonTest
    {
        [TestCase("simple")]
        public void RunTestCases(string testCaseName)
        {
            var testPath = $"sortr/{testCaseName}";
            var input = GetJson($"{testPath}/input");
            var expected = GetJson($"{testPath}/output");

            var sortr = new Sortr();
            var actual = sortr.Transform(input);

            actual.Should().BeEquivalentTo(expected, "it should be the same object");

            // Make sure the sort actually worked.
            var orderErrorMessage = VerifyOrder(actual, expected);
            orderErrorMessage.Should().BeNull(orderErrorMessage);
        }

        public static string VerifyOrder(JToken actual, JToken expected)
        {
            if (actual.Type == JTokenType.Object && expected.Type == JTokenType.Object)
            {
                return VerifyMapOrder((JObject)actual, (JObject)expected);
            }
            else if (actual.Type == JTokenType.Array && expected.Type == JTokenType.Array)
            {
                return VerifyListOrder((JArray)actual, (JArray)expected);
            }
            return null;
        }

        private static string VerifyMapOrder(JObject actualMap, JObject expectedMap)
        {
            var actualIter = actualMap.GetEnumerator();
            var expectedIter = expectedMap.GetEnumerator();
            for (; ; )
            {
                bool actualHasValue = actualIter.MoveNext();
                bool expectedHasValue = expectedIter.MoveNext();
                if (actualHasValue != expectedHasValue)
                {
                    return "actual and expected objects differ in length";
                }
                if (!expectedHasValue)
                {
                    return null;
                }

                var actual = actualIter.Current;
                var expected = expectedIter.Current;

                if (actual.Key != expected.Key)
                {
                    return "Found out of order keys '" + actual.Key + "' and '" + expected.Key + "'";
                }

                string result = VerifyOrder(actual.Value, expected.Value);
                if (result != null)
                {
                    return result;
                }
            }
        }

        private static string VerifyListOrder(JArray actual, JArray expected)
        {
            if (actual.Count != expected.Count)
            {
                return "actual and expected arrays have different sizes";
            }

            for (int index = 0; index < actual.Count; index++)
            {
                string result = VerifyOrder(actual[index], expected[index]);
                if (result != null)
                {
                    return result;
                }
            }

            return null; // success
        }
    }
}
