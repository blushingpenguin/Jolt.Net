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
using Newtonsoft.Json;

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class RemovrTest : JsonTest
    {
        [TestCase("firstSample")]
        [TestCase("boundaryConditions")]
        [TestCase("removrWithWildcardSupport")]
        [TestCase("multiStarSupport")]
        [TestCase("starDoublePathElementBoundaryConditions")]
        // Array tests
        [TestCase("array_canPassThruNestedArrays")]
        [TestCase("array_canHandleTopLevelArray")]
        [TestCase("array_nonStarInArrayDoesNotDie")]
        [TestCase("array_removeAnArrayIndex")]
        [TestCase("array_removeJsonArrayFields")]
        public void RunTestCase(string testCaseName)
        {
            var testCase = GetTestCase(Path.Combine("json", "removr", testCaseName));
            Removr removr = new Removr(testCase.Spec);
            var actual = removr.Transform(testCase.Input);
            actual.Should().BeEquivalentTo(testCase.Expected);
        }

        [TestCase("negativeTestCases")]
        public void RunNegativeTestCases(string testCaseName)
        {
            var testCase = GetJson(Path.Combine("json", "removr", testCaseName));
            Action a = () => new Removr(testCase["spec"]);
            a.Should().Throw<SpecException>();
        }
    }
}
