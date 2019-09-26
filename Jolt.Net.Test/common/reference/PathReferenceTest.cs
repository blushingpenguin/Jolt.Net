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
using System.Collections.Generic;

namespace Jolt.Net.Test
{
    public class PathReferenceTest 
    {
        [TestCase(   "", 0, "0" )]
        [TestCase(  "3", 3, "3" )]
        [TestCase("12", 12, "12")]
        public void ValidAmpReferencePatternTest(string key, int pathIndex, string canonicalForm)
        {
            var ref_ = new HashReference("#" + key);
            ref_.GetPathIndex().Should().Be(pathIndex);
            ref_.GetCanonicalForm().Should().Be("#" + canonicalForm);
        }


        [TestCase("pants")]
        [TestCase("-1")]
        [TestCase("(1)")]
        public void FailAmpReferencePatternTest(string key)
        {
            FluentActions
                .Invoking(() => new HashReference("#" + key))
                .Should().Throw<SpecException>();
        }
    }
}
