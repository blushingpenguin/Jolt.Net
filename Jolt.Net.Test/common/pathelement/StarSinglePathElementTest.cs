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
    public class StarSinglePathElementTest
    {
        [Test]
        public void TestStarAtFront()
        {
            IStarPathElement star = new StarSinglePathElement("*-tuna");
            star.StringMatch("tuna-tuna").Should().BeTrue();
            star.StringMatch("bob-tuna").Should().BeTrue();
            star.StringMatch("-tuna").Should().BeFalse();   // * has to catch something
            star.StringMatch("tuna").Should().BeFalse();
            star.StringMatch("tuna-bob").Should().BeFalse();

            MatchedElement lpe = star.Match("bob-tuna", null);
            lpe.GetSubKeyRef(0).Should().Be("bob-tuna");
            lpe.GetSubKeyRef(1).Should().Be("bob");
            lpe.GetSubKeyCount().Should().Be(2);

            star.Match("-tuna", null).Should().BeNull();
        }

        [Test]
        public void TestStarAtEnd()
        {
            IStarPathElement star = new StarSinglePathElement("tuna-*");
            star.StringMatch("tuna-tuna").Should().BeTrue();
            star.StringMatch("tuna-bob").Should().BeTrue();
            star.StringMatch("tuna-").Should().BeFalse();
            star.StringMatch("tuna").Should().BeFalse();
            star.StringMatch("bob-tuna").Should().BeFalse();

            MatchedElement lpe = star.Match("tuna-bob", null);
            lpe.GetSubKeyRef(0).Should().Be("tuna-bob");
            lpe.GetSubKeyRef(1).Should().Be("bob");
            lpe.GetSubKeyCount().Should().Be(2);

            star.Match("tuna-", null).Should().BeNull();
        }

        [Test]
        public void TestStarInMiddle()
        {
            IStarPathElement star = new StarSinglePathElement("tuna-*-marlin");
            star.StringMatch("tuna-tuna-marlin").Should().BeTrue();
            star.StringMatch("tuna-bob-marlin").Should().BeTrue();
            star.StringMatch("tuna--marlin").Should().BeFalse();
            star.StringMatch("tunamarlin").Should().BeFalse();
            star.StringMatch("marlin-bob-tuna").Should().BeFalse();

            MatchedElement lpe = star.Match("tuna-bob-marlin", null);
            lpe.GetSubKeyRef(0).Should().Be("tuna-bob-marlin");
            lpe.GetSubKeyRef(1).Should().Be("bob");
            lpe.GetSubKeyCount().Should().Be(2);

            star.Match("bob", null).Should().BeNull();
        }
    }
}
