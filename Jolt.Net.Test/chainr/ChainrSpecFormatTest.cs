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

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class ChainrSpecFormatTest : JsonTest
    {
        public static readonly string[] TestCases = new[]
        {
            "bad_spec_arrayClassName",
            "bad_spec_ClassName",
            "bad_spec_NonTransformClass",
            "bad_spec_empty"
        };

        [TestCaseSource(nameof(TestCases))]
        public void TestBadSpecs(string testCaseName)
        {
            var spec = GetJson($"chainr/specformat/{testCaseName}");
            FluentActions
                .Invoking(() => new ChainrSpec(spec, TestTransforms.Transforms))
                .Should().Throw<SpecException>();
        }

        [TestCaseSource(nameof(TestCases))]
        public void StaticChainrMethodNoArgs(string testCaseName)
        {
            var spec = GetJson($"chainr/specformat/{testCaseName}");
            FluentActions
                .Invoking(() => Chainr.FromSpec(spec)) // should fail when parsing spec
                .Should().Throw<SpecException>();
        }

        [TestCaseSource(nameof(TestCases))]
        public void StaticChainrMethodTransforms(string testCaseName)
        {
            var spec = GetJson($"chainr/specformat/{testCaseName}");
            FluentActions
                .Invoking(() => Chainr.FromSpec(spec, TestTransforms.Transforms)) // should fail when parsing spec
                .Should().Throw<SpecException>();
        }

        [TestCaseSource(nameof(TestCases))]
        public void StaticChainrMethodInstantiator(string testCaseName)
        {
            var spec = GetJson($"chainr/specformat/{testCaseName}");
            FluentActions
                .Invoking(() => Chainr.FromSpec(spec, new DefaultChainrInstantiator())) // should fail when parsing spec
                .Should().Throw<SpecException>();
        }

        [TestCaseSource(nameof(TestCases))]
        public void StaticChainrMethodInstantiatorAndTransforms(string testCaseName)
        {
            var spec = GetJson($"chainr/specformat/{testCaseName}");
            FluentActions
                .Invoking(() => Chainr.FromSpec(
                    spec, 
                    TestTransforms.Transforms,
                    new DefaultChainrInstantiator()
                )) // should fail when parsing spec
                .Should().Throw<SpecException>();
        }
    }
}
