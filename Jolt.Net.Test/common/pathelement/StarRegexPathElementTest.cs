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
using Jolt.Net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Jolt.Net.Test
{
    public class StarRegexPathElementTest
    {
        [TestCase("rating-*-*",                 "rating-tuna-marlin",                         "tuna",     "marlin",       TestName = "easy star test")]
        [TestCase("terms--config--*--*--cdv",   "terms--config--Expertise--12345--cdv",       "Expertise", "12345",       TestName = "easy facet usage")]
        [TestCase("terms--config--*--*--cdv",   "terms--config--Expertise--12345--6789--cdv", "Expertise", "12345--6789", TestName = "degenerate ProductId in facet")]
        [TestCase("rating.$.*.*",               "rating.$.marlin$.test.",                     "marlin$",   "test.",       TestName = "multi metachar test")]
        public void StarPatternTest(string spec, string dataKey, string expected1, string expected2)
        {
            IStarPathElement star = new StarRegexPathElement(spec);

            MatchedElement lpe = star.Match(dataKey, null);

            lpe.GetSubKeyCount().Should().Be(3);
            lpe.GetSubKeyRef(0).Should().Be(dataKey);
            lpe.GetSubKeyRef(1).Should().Be(expected1);
            lpe.GetSubKeyRef(2).Should().Be(expected2);
        }

        [Test]
        public void MustMatchSomethingTest() 
        {
            IStarPathElement star = new StarRegexPathElement("tuna-*-*");

            star.Match("tuna--", null).Should().BeNull();
            star.Match("tuna-bob-", null).Should().BeNull();
            star.Match("tuna--bob", null).Should().BeNull();

            IStarPathElement multiMetacharStarpathelement = new StarRegexPathElement("rating-$-*-*");

            multiMetacharStarpathelement.Match("rating-capGrp1-capGrp2", null).Should().BeNull();
            multiMetacharStarpathelement.Match("rating-$capGrp1-capGrp2", null).Should().BeNull();
            multiMetacharStarpathelement.Match("rating-$-capGrp1-capGrp2", null).Should().NotBeNull();
        }
    }
}
