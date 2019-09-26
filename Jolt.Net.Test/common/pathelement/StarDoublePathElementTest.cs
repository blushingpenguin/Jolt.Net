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
    public class StarDoublePathElementTest
    {
        [Test]
        public void TestStarInFirstAndMiddle()
        {
            IStarPathElement star = new StarDoublePathElement("*a*");

            star.StringMatch("bbbaaccccc").Should().BeTrue();
            star.StringMatch("abbbbbbbbcc").Should().BeFalse();
            star.StringMatch("bbba").Should().BeFalse();

            MatchedElement lpe = star.Match("bbbaccc", null);
            // * -> bbb
            // a -> a
            // * -> ccc
            lpe.GetSubKeyRef(0).Should().Be("bbbaccc");
            lpe.GetSubKeyRef(1).Should().Be("bbb");
            lpe.GetSubKeyRef(2).Should().Be("ccc");
            lpe.GetSubKeyCount().Should().Be(3);

        }

        [Test]
        public void TestStarAtFrontAndEnd()
        {
            IStarPathElement star = new StarDoublePathElement("*a*c");

            star.StringMatch("bbbbadddc").Should().BeTrue();
            star.StringMatch("bacc").Should().BeTrue();
            star.StringMatch("bac").Should().BeFalse();
            star.StringMatch("baa").Should().BeFalse();

            MatchedElement lpe = star.Match("abcadefc", null);
            // * -> abc
            // a -> a index 4
            // * -> def
            // c -> c
            lpe.GetSubKeyRef(0).Should().Be("abcadefc");
            lpe.GetSubKeyRef(1).Should().Be("abc");
            lpe.GetSubKeyRef(2).Should().Be("def");
            lpe.GetSubKeyCount().Should().Be(3);
        }

        [Test]
        public void TestStarAtMiddleAndEnd()
        {
            IStarPathElement star = new StarDoublePathElement("a*b*");

            star.StringMatch("adbc").Should().BeTrue();
            star.StringMatch("abbc").Should().BeTrue();
            star.StringMatch("adddddd").Should().BeFalse();
            star.StringMatch("addb").Should().BeFalse();
            star.StringMatch("abc").Should().BeFalse();

            MatchedElement lpe = star.Match("abcbbac", null);
            // a -> a
            // * -> bc index 1
            // b -> b   index 3
            // * -> bac index 4
            // c -> c
            lpe.GetSubKeyRef(0).Should().Be("abcbbac");
            lpe.GetSubKeyRef(1).Should().Be("bc");
            lpe.GetSubKeyRef(2).Should().Be("bac");
            lpe.GetSubKeyCount().Should().Be(3);
        }


        [Test]
        public void TestStarsInMiddle()
        {
            IStarPathElement star = new StarDoublePathElement("a*b*c");

            star.StringMatch("a123b456c").Should().BeTrue();
            star.StringMatch("abccbcc").Should().BeTrue();

            MatchedElement lpe = star.Match("abccbcc", null);
            // a -> a
            // * -> bcc index 1
            // b -> b
            // * -> c index 2
            // c -> c
            lpe.GetSubKeyRef(0).Should().Be("abccbcc");
            lpe.GetSubKeyRef(1).Should().Be("bcc");
            lpe.GetSubKeyRef(2).Should().Be("c");
            lpe.GetSubKeyCount().Should().Be(3);
        }


        [Test]
        public void TestStarsInMiddleNonGreedy()
        {
            IStarPathElement star = new StarDoublePathElement("a*b*c");

            MatchedElement lpe = star.Match("abbccbccc", null);
            // a -> a
            // * -> b index 1
            // b -> b
            // * -> ccbcc index 2
            // c -> c
            lpe.GetSubKeyRef(0).Should().Be("abbccbccc");
            lpe.GetSubKeyRef(1).Should().Be("b");
            lpe.GetSubKeyRef(2).Should().Be("ccbcc");
            lpe.GetSubKeyCount().Should().Be(3);
        }
    }
}
