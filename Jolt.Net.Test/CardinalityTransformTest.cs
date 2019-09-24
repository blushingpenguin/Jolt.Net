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
    public class CardinalityTransformTest : JsonTest
    {
        [TestCase("oneLiteralTestData")]
        [TestCase("manyLiteralTestData")]
        [TestCase("starTestData")]
        [TestCase("atTestData")]
        public void RunTest(string testCaseName)
        {
            var testCase = GetTestCase(Path.Combine("json", "cardinality", testCaseName));
            CardinalityTransform cardinality = new CardinalityTransform(testCase.Spec);
            var actual = cardinality.Transform(testCase.Input);

            actual.Should().BeEquivalentTo(testCase.Expected);
        }

        [Test]
        public void TestSpecExceptions()
        {
            var testCase = GetJson(Path.Combine("json", "cardinality", "failCardinalityType"));
            var spec = testCase["spec"];

            // Should throw exception
            Action a = () => new CardinalityTransform( spec );
            a.Should().Throw<SpecException>();
        }

        [Test]
        public void TestArrayCardinalityOne()
        {
            // The above tests cover cardinality on elements that are Lists, this test covers elements that are arrays
            var input = new JObject(
                new JProperty("input", new JArray(5, 4))
            );

            var spec = new JObject(
                new JProperty("input", "ONE")
            );

            var expected = new JObject(
                new JProperty("input", 5)
            );

            var cardinalityTransform = new CardinalityTransform(spec);
            var actual = cardinalityTransform.Transform(input);
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void TestArrayCardinalityMany()
        {
            // The above tests cover cardinality on elements that are Lists, this test covers elements that are arrays
            var input = new JObject(
                new JProperty("input", new JArray(5, 4))
            );

            var spec = new JObject(
                new JProperty("input", "MANY")
            );

            var expected = new JObject(
                new JProperty("input", new JArray(5, 4))
            );

            var cardinalityTransform = new CardinalityTransform(spec);
            var actual = cardinalityTransform.Transform(input);
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
