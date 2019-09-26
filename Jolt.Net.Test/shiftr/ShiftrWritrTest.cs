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
    // Todo Now that the PathElement classes have been split out (no longer inner classes)
    //  each class should get a test
    [Parallelizable(ParallelScope.All)]
    public class ShiftrWritrTest 
    {
        [Test]
        public void ReferenceTest() 
        {
            ShiftrWriter path = new ShiftrWriter("SecondaryRatings.tuna-&(0,1)-marlin.Value");

            path.Get(0).RawKey.Should().Be("SecondaryRatings");
            path.Get(0).ToString().Should().Be("SecondaryRatings");
            path.Get(2).RawKey.Should().Be("Value");
            path.Get(2).ToString().Should().Be("Value");
            path.Get(2).ToString().Should().Be("Value");

            var refElement = (AmpPathElement)path.Get(1);

            refElement.GetTokens().Count.Should().Be(3);
            refElement.GetTokens()[0].Should().Be("tuna-");
            refElement.GetTokens()[2].Should().Be("-marlin");

            refElement.GetTokens()[1].Should().BeOfType<AmpReference>();
            var ref_ = (AmpReference)refElement.GetTokens()[1];
            ref_.GetPathIndex().Should().Be(0);
            ref_.GetKeyGroup().Should().Be(1);
        }

        [Test]
        public void ArrayRefTest() 
        {
            ShiftrWriter path = new ShiftrWriter("ugc.photos-&1-bob[&2]");

            path.Size().Should().Be(3);
            {  // 0
                var pe = path.Get(0);
                pe.Should().BeOfType<LiteralPathElement>("First pathElement should be a literal one.");
            }

            { // 1
                var pe = path.Get(1);
                pe.Should().BeOfType< AmpPathElement>("Second pathElement should be a AmpPathElement.");

                var refElement = (AmpPathElement)pe;

                refElement.GetTokens().Count.Should().Be(3);

                {
                    refElement.GetTokens()[0].Should().BeOfType<string>();
                    refElement.GetTokens()[0].Should().Be("photos-");
                }
                {
                    refElement.GetTokens()[1].Should().BeOfType<AmpReference>();
                    var ref_ = (AmpReference)refElement.GetTokens()[1];
                    ref_.GetCanonicalForm().Should().Be("&(1,0)");
                    ref_.GetPathIndex().Should().Be(1);
                    ref_.GetKeyGroup().Should().Be(0);
                }
                {
                    refElement.GetTokens()[2].Should().BeOfType<string>();
                    refElement.GetTokens()[2].Should().Be("-bob");
                }
            }

            { // 2
                var pe = path.Get(2);
                pe.Should().BeOfType<ArrayPathElement>("Third pathElement should be a literal one.");

                var arrayElement = (ArrayPathElement)pe;
                arrayElement.GetCanonicalForm().Should().Be("[&(2,0)]");
            }
        }

        [Test]
        public void calculateOutputTest_refsOnly() 
        {
            var pe1 = (IMatchablePathElement)PathElementBuilder.ParseSingleKeyLHS("tuna-*-marlin-*");
            var pe2 = (IMatchablePathElement)PathElementBuilder.ParseSingleKeyLHS("rating-*");

            MatchedElement lpe = pe1.Match("tuna-marlin", new WalkedPath());
            lpe.Should().BeNull();

            lpe = pe1.Match("tuna-A-marlin-AAA", new WalkedPath());
            lpe.RawKey.Should().Be("tuna-A-marlin-AAA");
            lpe.GetSubKeyRef(0).Should().Be("tuna-A-marlin-AAA");
            lpe.GetSubKeyCount().Should().Be(3);
            lpe.GetSubKeyRef(1).Should().Be("A");
            lpe.GetSubKeyRef(2).Should().Be("AAA");

            MatchedElement lpe2 = pe2.Match("rating-BBB", new WalkedPath(null, lpe));
            lpe2.RawKey.Should().Be("rating-BBB");
            lpe2.GetSubKeyRef(0).Should().Be("rating-BBB");
            lpe2.GetSubKeyCount().Should().Be(2);
            lpe2.GetSubKeyRef(1).Should().Be("BBB");

            ShiftrWriter outputPath = new ShiftrWriter("&(1,2).&.value");
            WalkedPath twoSteps = new WalkedPath(null, lpe);
            twoSteps.Add(null, lpe2);
            {
                var outputElement = (IEvaluatablePathElement)outputPath.Get(0);
                var evaledLeafOutput = outputElement.Evaluate(twoSteps);
                evaledLeafOutput.Should().Be("AAA");
            }
            {
                var outputElement = (IEvaluatablePathElement)outputPath.Get(1);
                var evaledLeafOutput = outputElement.Evaluate(twoSteps);
                evaledLeafOutput.Should().Be("rating-BBB");
            }
            {
                var outputElement = (IEvaluatablePathElement)outputPath.Get(2);
                var evaledLeafOutput = outputElement.Evaluate(twoSteps);
                evaledLeafOutput.Should().Be("value");
            }
        }

        [Test]
        public void calculateOutputTest_arrayIndexes() 
        {
            // simulate Shiftr LHS specs
            var pe1 = (IMatchablePathElement)PathElementBuilder.ParseSingleKeyLHS("tuna-*-marlin-*");
            var pe2 = (IMatchablePathElement)PathElementBuilder.ParseSingleKeyLHS("rating-*");

            // match them against some data to get LiteralPathElements with captured values
            MatchedElement lpe = pe1.Match("tuna-2-marlin-3", new WalkedPath());
            lpe.GetSubKeyRef(1).Should().Be("2");
            lpe.GetSubKeyRef(2).Should().Be("3");

            MatchedElement lpe2 = pe2.Match("rating-BBB", new WalkedPath(null, lpe));
            lpe2.GetSubKeyCount().Should().Be(2);
            lpe2.GetSubKeyRef(1).Should().Be("BBB");

            // Build an write path path
            ShiftrWriter shiftrWriter = new ShiftrWriter("tuna[&(1,1)].marlin[&(1,2)].&(0,1)");

            shiftrWriter.Size().Should().Be(5);
            shiftrWriter.GetCanonicalForm().Should().Be("tuna.[&(1,1)].marlin.[&(1,2)].&(0,1)");

            // Evaluate the write path against the LiteralPath elements we build above ( like Shiftr does )
            WalkedPath twoSteps = new WalkedPath(null, lpe);
            twoSteps.Add(null, lpe2);
            var stringPath = shiftrWriter.Evaluate(twoSteps);
            stringPath[0].Should().Be("tuna");
            stringPath[1].Should().Be("2");
            stringPath[2].Should().Be("marlin");
            stringPath[3].Should().Be("3");
            stringPath[4].Should().Be("BBB");
        }
    }
}
