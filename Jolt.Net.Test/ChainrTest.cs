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

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class ChainrTest : JsonTest
    {
        private static JArray NewChainrSpec() => new JArray();

        private static JObject NewActivity(string opname)
        {
            var activity = new JObject();
            activity.Add(ChainrEntry.OPERATION_KEY, opname);
            return activity;
        }

        private static JObject NewActivity(string operation, JToken spec)
        {
            var activity = new JObject();
            activity.Add(ChainrEntry.OPERATION_KEY, operation);
            if (spec != null)
            {
                activity.Add(ChainrEntry.SPEC_KEY, spec);
            }
            return activity;
        }

#if FALSE
        private JObject NewCustomJavaActivity( Class cls, Object spec ) {
            JObject activity = new HashMap<>();
            activity.put( ChainrEntry.OPERATION_KEY, cls.getName() );
            if ( spec != null ) {
                activity.put( ChainrEntry.SPEC_KEY, spec );
            }

            return activity;
        }

        private List<Map<String,Object>> newCustomJavaChainrSpec( Class cls, Object delegateSpec )
        {
            List<Map<String,Object>> retvalue = newChainrSpec();
            retvalue.add( newCustomJavaActivity( cls, delegateSpec ) );
            return retvalue;
        }
#endif

        private static JArray NewShiftrChainrSpec(JToken shiftrSpec)
        {
            var retvalue = NewChainrSpec();
            retvalue.Add(NewActivity("shift", shiftrSpec));
            return retvalue;
        }

        private static JArray NewShiftrDefaultrSpec(JToken defaultrSpec)
        {
            var retvalue = NewChainrSpec();
            retvalue.Add(NewActivity("default", defaultrSpec));
            return retvalue;
        }

        private static JArray NewShiftrRemovrSpec(JToken removrSpec)
        {
            var retvalue = NewChainrSpec();
            retvalue.Add(NewActivity("remove", removrSpec));
            return retvalue;
        }

        private static JArray NewShiftrSortrSpec(JToken sortrSpec)
        {
            var retvalue = NewChainrSpec();
            retvalue.Add(NewActivity("sort", sortrSpec));
            return retvalue;
        }

        [Test]
        public void process_itCallsShiftr()
        {
            var testCase = GetTestCase(Path.Combine("json", "shiftr", "queryMappingXform"));

            var chainrSpec = NewShiftrChainrSpec(testCase.Spec);

            Chainr unit = Chainr.FromSpec(chainrSpec);
            var actual = unit.Transform(testCase.Input, null);

            actual.Should().BeEquivalentTo(testCase.Expected);
        }

        [Test]
        public void process_itCallsDefaultr()
        {
            var testCase = GetTestCase(Path.Combine("json", "defaultr", "firstSample"));

            var chainrSpec = NewShiftrDefaultrSpec(testCase.Spec);

            Chainr unit = Chainr.FromSpec(chainrSpec);
            var actual = unit.Transform(testCase.Input, null);

            actual.Should().BeEquivalentTo(testCase.Expected);
        }

        [Test]
        public void process_itCallsRemover()
        {
            var testCase = GetTestCase(Path.Combine("json", "removr", "firstSample"));

            var chainrSpec = NewShiftrRemovrSpec(testCase.Spec);

            var unit = Chainr.FromSpec(chainrSpec);
            var actual = unit.Transform(testCase.Input, null);

            actual.Should().BeEquivalentTo(testCase.Expected);
        }

        [Test]
        public void process_itCallsSortr()
        {
            var input = GetJson(Path.Combine("json", "sortr", "simple", "input"));
            var expected = GetJson(Path.Combine("json", "sortr", "simple", "output"));
            var chainrSpec = NewShiftrSortrSpec(null);

            var unit = Chainr.FromSpec(chainrSpec);
            var actual = unit.Transform(input, null);

            actual.Should().BeEquivalentTo(expected);

            // TODO:
            // String orderErrorMessage = SortrTest.verifyOrder( actual, expected );
            // Assert.assertNull( orderErrorMessage, orderErrorMessage );
        }

#if FALSE
        [Test]
        public void process_itCallsCustomJavaTransform() 
        {
            var spec = newChainrSpec();
            var delegateSpec = new JObject();
            spec.add( newCustomJavaActivity( GoodTestTransform.class, delegateSpec ) );
            Object input = new Object();

            Chainr unit = Chainr.fromSpec( spec );
            TransformTestResult actual = (TransformTestResult) unit.transform( input, null );

            Assert.assertEquals( input, actual.input );
            Assert.assertEquals( delegateSpec, actual.spec );
        }
#endif

        static readonly JToken[] SpecExceptionTestCases = new JToken[]
        {
            null,
            new JValue("foo"),
            NewActivity( null ),
            NewActivity( "pants" )
        };

        [TestCaseSource(nameof(SpecExceptionTestCases))]
        public void process_itBlowsUp_fromSpec(JToken spec)
        {
            Action a = () => Chainr.FromSpec(spec);
            a.Should().Throw<SpecException>();
        }

#if FALSE
        static readonly JToken[] FailureTransformTestCases = new JToken[]
        {
            NewCustomJavaChainrSpec( ExplodingTestTransform.class, null )
        }
        public Object[][] failureTransformCases() {
            return new Object[][] {
                { newCustomJavaChainrSpec( ExplodingTestTransform.class, null ) }
            };
        }

        @Test(dataProvider = "failureTransformCases", expectedExceptions = TransformException.class)
        public void process_itBlowsUp_fromTransform(Object spec) {
            Chainr unit = Chainr.fromSpec( spec );
            unit.transform( new HashMap(), null );
            Assert.fail("Should have failed during transform.");
        }
#endif


        [TestCase("andrewkcarter1", false)]
        [TestCase("andrewkcarter2", false)]
        [TestCase("firstSample", true)]
        [TestCase("ismith", false)]
        [TestCase("ritwickgupta", false)]
        [TestCase("wolfermann1", false)]
        [TestCase("wolfermann2", false)]
        [TestCase("wolfermann2", false)]
        public void RunTestCases(string testCaseName, bool sorted)
        {
            var testPath = Path.Combine("json", "chainr", "integration", testCaseName);
            var testCase = GetTestCase(testPath);

            var unit = Chainr.FromSpec(testCase.Spec);

            unit.HasContextualTransforms().Should().BeFalse();
            unit.GetContextualTransforms().Count.Should().Be(0);

            var actual = unit.Transform(testCase.Input, null);

            actual.Should().BeEquivalentTo(testCase.Expected);

#if FALSE
            if ( sorted ) {
                // Make sure the sort actually worked.
                String orderErrorMessage = SortrTest.verifyOrder( actual, expected );
                Assert.assertNull( orderErrorMessage, orderErrorMessage );
            }
#endif
        }

        [Test]
        public void TestReuseChainr()
        {
            // Spec which moves "attributeMap"'s keys to a root "attributes" list.
            var specShift = JObject.Parse(
                    "{" +
                            "'operation':'shift'," +
                            "'spec' : { 'attributeMap' : { '*' : { '$' : 'attributes[#2]' } } }" +
                    "}"
            );

            // Create a single Chainr from the spec
            Chainr chainr = Chainr.FromSpec(new JArray(specShift));

            // Test input with three attributes
            var content = JObject.Parse(
                    "{ 'attributeMap' : { " +
                            "'attribute1' : 1, 'attribute2' : 2, 'attribute3' : 3 }" +
                    "}"
            );

            var transformed = chainr.Transform(content);

            // First time everything checks out
            transformed.Should().NotBeNull();
            transformed["attributes"].Should().BeEquivalentTo(new JArray("attribute1", "attribute2", "attribute3"));

            // Create a new identical input
            content = JObject.Parse(
                    "{ 'attributeMap' : { " +
                            "'attribute1' : 1, 'attribute2' : 2, 'attribute3' : 3 }" +
                            "}"
            );

            // Create a new transform from the same Chainr
            transformed = chainr.Transform(content);
            transformed.Should().NotBeNull();

            // The following assert fails because attributes will have three leading null values:
            // transformedMap["attributes"] == [null, null, null, "attribute1", "attribute2", "attribute3"]
            transformed["attributes"].Should().BeEquivalentTo(new JArray("attribute1", "attribute2", "attribute3"));
        }
    }
}
