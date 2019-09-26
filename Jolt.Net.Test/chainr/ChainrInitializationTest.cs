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
    public class ChainrInitializationTest : JsonTest
    {
        [TestCase("bad_transform_loadsExplodingTransform")]
        public void TestBadTransforms(string testCaseName)
        {
            var spec = GetJson($"chainr/transforms/{testCaseName}");
            var unit = Chainr.FromSpec(spec, TestTransforms.Transforms);
            Action a = () => unit.Transform((JToken)new JObject(), null);// should fail here
            a.Should().Throw<TransformException>();
        }

        [TestCase("loadsGoodTransform")]
        public void TestPassing(string testCaseName)
        {
            var spec = GetJson($"chainr/transforms/{testCaseName}");
            var unit = Chainr.FromSpec(spec, TestTransforms.Transforms);
            JToken input = new JObject();
            var result = unit.Transform(input, null);
            result["input"].Should().BeEquivalentTo(input);
            result["spec"].Should().NotBeNull();
        }

        [Test]
        public void ChainrBuilderFailsOnNullLoader()
        {
            var validSpec = GetJson("chainr/transforms/loadsGoodTransform");
            Action a = () => new ChainrBuilder( validSpec ).Loader( null );
            a.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ChainrBuilderFailsOnNullTransforms()
        {
            var validSpec = GetJson("chainr/transforms/loadsGoodTransform");
            Action a = () => new ChainrBuilder(validSpec).Transforms(null);
            a.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void FailsOnNullListOfJoltTransforms()
        {
            Action a = () => new Chainr( null );
            a.Should().Throw<ArgumentNullException>();
        }

        private class StupidTransform : IJoltTransform
        {
        }

        [Test]
        public void FailsOnStupidTransform()
        {
            var badSpec = new List<IJoltTransform>();

            // Stupid JoltTransform that implements the base interface, and not one of the useful ones
            badSpec.Add(new StupidTransform());

            FluentActions
                .Invoking(() => new Chainr( badSpec ))
                .Should().Throw<SpecException>();
        }

        private class OverEagerTransform : ITransform, IContextualTransform
        {
            public JToken Transform(JToken input, JObject context)
            {
                return null;
            }

            public JToken Transform(JToken input)
            {
                return null;
            }
        }

        [Test]
        public void FailsOnOverEagerTransform()
        {
            var badSpec = new List<IJoltTransform>();

            // Stupid JoltTransform that implements both "real" interfaces
            badSpec.Add(new OverEagerTransform());

            FluentActions
                .Invoking(() => new Chainr(badSpec))
                .Should().Throw<SpecException>();
        }
    }
}
