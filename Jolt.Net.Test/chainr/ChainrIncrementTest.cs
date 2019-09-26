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

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class ChainrIncrementTest : JsonTest
    {
        [TestCase(0, 1)]
        [TestCase(0, 3)]
        [TestCase(1, 3)]
        [TestCase(1, 4)]
        public void TestChainrIncrementsFromTo(int start, int end)
        {
            var spec = GetJson("chainr/increments/spec");

            var chainr = Chainr.FromSpec(spec, TestTransforms.Transforms);

            var expected = GetJson($"chainr/increments/{start}-{end}");

            var actual = chainr.Transform(start, end, (JToken)new JObject());

            actual.Should().BeEquivalentTo(expected);
        }


        [TestCase(1)]
        [TestCase(3)]
        public void TestChainrIncrementsTo(int end)
        {
            var spec = GetJson("chainr/increments/spec");

            var chainr = Chainr.FromSpec(spec, TestTransforms.Transforms);

            var expected = GetJson($"chainr/increments/0-{end}");

            var actual = chainr.Transform(end, (JToken)new JObject());

            actual.Should().BeEquivalentTo(expected);
        }

        [TestCase(0, 0)]
        [TestCase(-2, 2)]
        [TestCase(0, -2)]
        [TestCase(1, 10000)]
        public void TestFails(int start, int end)
        {
            var spec = GetJson("chainr/increments/spec");
            var chainr = Chainr.FromSpec(spec, TestTransforms.Transforms);
            Action a = () => chainr.Transform(start, end, (JToken)new JObject());
            a.Should().Throw<TransformException>();
        }
    }
}
