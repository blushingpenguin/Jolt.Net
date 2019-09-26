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
using System;

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class PathAndGroupReferenceTest
    {
        public static TestCaseData[] ValidReferenceTests = new TestCaseData[]
        {
            new TestCaseData(     "", 0, 0, "(0,0)"),
            new TestCaseData(    "3", 3, 0, "(3,0)"),
            new TestCaseData(  "(3)", 3, 0, "(3,0)"),
            new TestCaseData("(1,2)", 1, 2, "(1,2)")
        };

        [TestCaseSource(nameof(ValidReferenceTests))]
        public void ValidAmpReferencePatternTest(string key, int pathIndex, int keyGroup, string canonicalForm)
        {
            IPathAndGroupReference amp = new AmpReference("&" + key);
            amp.GetPathIndex().Should().Be(pathIndex);
            amp.GetKeyGroup().Should().Be(keyGroup);
            amp.GetCanonicalForm().Should().Be($"&{canonicalForm}");
        }

        [TestCaseSource(nameof(ValidReferenceTests))]
        public void ValidDollarReferencePatternTest(string key, int pathIndex, int keyGroup, string canonicalForm)
        {
            IPathAndGroupReference amp = new DollarReference("$" + key);
            amp.GetPathIndex().Should().Be(pathIndex);
            amp.GetKeyGroup().Should().Be(keyGroup);
            amp.GetCanonicalForm().Should().Be($"${canonicalForm}");
        }

        public static TestCaseData[] FailReferenceTests = new TestCaseData[]
        {
            new TestCaseData("pants"),
            new TestCaseData("-1"),
            new TestCaseData("(-1,2)"),
            new TestCaseData("(1,-2)")
        };

        [TestCaseSource(nameof(FailReferenceTests))]
        public void FailAmpReferencePatternTest(String key)
        {
            FluentActions
                .Invoking(() => new AmpReference("&" + key))
                .Should().Throw<SpecException>();
        }

        [TestCaseSource(nameof(FailReferenceTests))]
        public void FailDollarReferencePatternTest(String key) 
        {
            FluentActions
                .Invoking(() => new DollarReference("$" + key))
                .Should().Throw<SpecException>();
        }
    }
}
