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
using NUnit.Framework;
using NSubstitute;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class DefaultrTest : JsonTest
    {

        [TestCase("arrayMismatch1")]
        [TestCase("arrayMismatch2")]
        [TestCase("defaultNulls")]
        [TestCase("expansionOnly")]
        [TestCase("firstSample")]
        [TestCase("identity")]
        [TestCase("nestedArrays1")]
        [TestCase("nestedArrays2")]
        [TestCase("orOrdering")]
        [TestCase("photosArray")]
        [TestCase("starsOfStars")]
        [TestCase("topLevelIsArray")]
        public void RunTest(string testCaseName)
        {
            var testCase = GetTestCase(Path.Combine("json", "defaultr", testCaseName));
            
            var defaultr = new Defaultr(testCase.Spec);
            var actual = defaultr.Transform(testCase.Input);

            actual.Should().BeEquivalentTo(testCase.Expected);
        }

        [Test]
        public void DeepCopyTest()
        {
            var testCase = GetTestCase(Path.Combine("json", "defaultr", "__deepCopyTest"));

            Defaultr defaultr = new Defaultr(testCase.Spec);
            {
                var fiddle = defaultr.Transform(testCase.Input);

                var array = (JArray)fiddle["array"];
                array.Add("a");

                var subMap = (JObject)fiddle["map"];
                subMap["c"] = "c";
            }
            {
                var testCase2 = GetTestCase(Path.Combine("json", "defaultr", "__deepCopyTest"));

                var actual = defaultr.Transform(testCase2.Input);
                actual.Should().BeEquivalentTo(testCase2.Expected);
            }
        }

        [Test]
        public void ThrowExceptionOnBadSpec()
        {
            var spec = JObject.Parse("{ \"tuna*\": \"marlin\" }");
            Action a = () => new Defaultr(spec);
            a.Should().Throw<SpecException>();
        }
    }
}
