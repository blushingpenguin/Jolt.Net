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

using Jolt.Net.Functions;
using Jolt.Net.Functions.Lists;
using Jolt.Net.Functions.Objects;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class ListsTest : AbstractTester 
    {
        public static IEnumerable<TestCaseData> GetTestCases()
        {
            var FIRST_ELEMENT = new FirstElement();
            var LAST_ELEMENT = new LastElement();
            var ELEMENT_AT = new ElementAt();

            var SIZE = new Size();

            object[][] tests = new object[][] {
                new object[] { "first-empty-array", FIRST_ELEMENT, new JArray(), null },

                new object[] { "first-null", FIRST_ELEMENT, null, null },
                new object[] { "first-array", FIRST_ELEMENT, new JArray(1, 2, 3), new JValue(1) },

                new object[] { "last-empty-array", LAST_ELEMENT, new JArray(), null },

                new object[] { "last-null", LAST_ELEMENT, null, null },
                new object[] { "last-array", LAST_ELEMENT, new JArray(1, 2, 3), new JValue(3) },

                new object[] { "at-empty-array", ELEMENT_AT, new JArray(5), null },
                new object[] { "at-empty-null", ELEMENT_AT, new JArray(null, 1), null },
                new object[] { "at-empty-invalid", ELEMENT_AT, new JObject(), null },

                new object[] { "at-array", ELEMENT_AT, new JArray(1, 2, 3, 1), new JValue(3) },

                new object[] { "at-array-missing", ELEMENT_AT, new JArray(5, 1, 2, 3), null },

                new object[] { "size-list", SIZE, new JArray(5, 1, 2, 3), new JValue(4) }
            };

            foreach (var test in tests)
            {
                yield return new TestCaseData(test.Skip(1).ToArray()) { TestName = $"RunTest({test[0]})" };
            }
        }

        [TestCaseSource(nameof(GetTestCases))]
        public void RunTest(IFunction function, JToken args, JToken expected)
        {
            TestFunction(function, args, expected);
        }
    }
}
