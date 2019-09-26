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
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class StringsTest : AbstractTester
    {
        public static IEnumerable<TestCaseData> GetTestCases()
        {
            var SPLIT = new Jolt.Net.Functions.Strings.Split();

            var jnull = JValue.CreateNull();
            object[][] tests = new object[][] {
                new object[] { "split-invalid-null", SPLIT, null, null },
                new object[] { "split-invalid-string", SPLIT, new JArray(""), null },

                new object[] { "split-null-string", SPLIT, new JArray(",", jnull), null },
                new object[] { "split-null-separator", SPLIT, new JArray(jnull, "test"), null },

                new object[] { "split-empty-string", SPLIT, new JArray(",", ""), new JArray("") },
                new object[] { "split-single-token-string", SPLIT, new JArray(",", "test"), new JArray("test") },

                new object[] { "split-double-token-string", SPLIT, new JArray(",", "test,TEST"), new JArray("test", "TEST") },
                new object[] { "split-multi-token-string", SPLIT, new JArray(",", "test,TEST,Test,TeSt"), new JArray("test", "TEST", "Test", "TeSt") },
                new object[] { "split-spaced-token-string", SPLIT, new JArray(",", "test, TEST"), new JArray("test", " TEST") },
                new object[] { "split-long-separator-spaced-token-string", SPLIT, new JArray(", ", "test, TEST"), new JArray("test", "TEST") },

                new object[] { "split-regex-token-string", SPLIT, new JArray("[eE]", "test,TEST"), new JArray("t", "st,T", "ST") },
                new object[] { "split-regex2-token-string", SPLIT, new JArray("\\s+", "test TEST  Test    TeSt"), new JArray("test", "TEST", "Test", "TeSt") }
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
