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
    /**
    * StringTools Tester.
    */
    [Parallelizable(ParallelScope.All)]
    public class StringToolsTest
    {
        [TestCase(null, null, 0)]
        [TestCase(null, "", 0)]
        [TestCase("", null, 0)]
        [TestCase("", "", 0)]
        [TestCase("foofoofoo", "oo", 3)]
        [TestCase("foofoofoo", "o", 6)]
        [TestCase("foofoofoo", "f", 3)]
        [TestCase("barlocksbarlocksbarlocks", "bax", 0)]
        public void CountMatches(string str, string subStr, int result)
        {
            StringTools.CountMatches(str, subStr).Should().Be(result);
        }
    }
}
