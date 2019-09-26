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
using Jolt.Net.Functions.Objects;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class JoltUtilsSquashTest 
    {
        [Test]
        public void SquashNullsInAListTest() 
        {
            JToken actual = new JArray("a", null, 1, null, "b", 2);
            var expectedList = new JArray("a", 1, "b", 2);

            actual = SquashNulls.Squash(actual);
            actual.Should().BeEquivalentTo(expectedList);
        }

        [Test]
        public void SquashNullsInAMapTest() 
        {
            JToken actual = new JObject(
                new JProperty("a", 1),
                new JProperty("b", null),
                new JProperty("c", "C")
            );

            var expectedMap = new JObject(
                new JProperty("a", 1),
                new JProperty("c", "C")
            );

            actual = SquashNulls.Squash(actual);
            actual.Should().BeEquivalentTo(expectedMap);
        }

        [Test]
        public void RecursivelySquashNullsTest()
        {
            JToken actual = JObject.Parse("{ 'a' : 1, 'b' : null, 'c' : [ null, 4, null, 5, { 'x' : 'X', 'y' : null } ] }");
            JObject expected = JObject.Parse("{ 'a' : 1,             'c' : [       4,       5, { 'x' : 'X'             } ] }");

            actual = RecursivelySquashNulls.Squash(actual);
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
