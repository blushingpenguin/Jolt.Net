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
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

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

        private static JObject NewCustomActivity(Type type, JObject spec)
        {
            var activity = new JObject(
                new JProperty(ChainrEntry.OPERATION_KEY, type.Name)
            );
            if (spec != null && spec.Type != JTokenType.Null) 
            {
                activity[ChainrEntry.SPEC_KEY] = spec;
            }
            return activity;
        }

        private static JArray NewCustomChainrSpec(Type type, JObject delegateSpec)
        {
            var retvalue = new JArray();
            retvalue.Add(NewCustomActivity(type, delegateSpec));
            return retvalue;
        }

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
            var testCase = GetTestCase("shiftr/queryMappingXform");

            var chainrSpec = NewShiftrChainrSpec(testCase.Spec);

            Chainr unit = Chainr.FromSpec(chainrSpec);
            var actual = unit.Transform(testCase.Input, null);

            actual.Should().BeEquivalentTo(testCase.Expected);
        }

        [Test]
        public void process_itCallsDefaultr()
        {
            var testCase = GetTestCase("defaultr/firstSample");

            var chainrSpec = NewShiftrDefaultrSpec(testCase.Spec);

            Chainr unit = Chainr.FromSpec(chainrSpec);
            var actual = unit.Transform(testCase.Input, null);

            actual.Should().BeEquivalentTo(testCase.Expected);
        }

        [Test]
        public void process_itCallsRemover()
        {
            var testCase = GetTestCase("removr/firstSample");

            var chainrSpec = NewShiftrRemovrSpec(testCase.Spec);

            var unit = Chainr.FromSpec(chainrSpec);
            var actual = unit.Transform(testCase.Input, null);

            actual.Should().BeEquivalentTo(testCase.Expected);
        }

        [Test]
        public void process_itCallsSortr()
        {
            var input = GetJson("sortr/simple/input");
            var expected = GetJson("sortr/simple/output");
            var chainrSpec = NewShiftrSortrSpec(null);

            var unit = Chainr.FromSpec(chainrSpec);
            var actual = unit.Transform(input, null);

            actual.Should().BeEquivalentTo(expected);

            String orderErrorMessage = SortrTest.VerifyOrder(actual, expected);
            orderErrorMessage.Should().BeNull(orderErrorMessage);
        }

        [Test]
        public void process_itCallsCustomTransform() 
        {
            var spec = NewChainrSpec();
            var delegateSpec = new JObject();
            var transformType = typeof(GoodTestTransform);
            spec.Add(NewCustomActivity(transformType, delegateSpec));

            var transforms = new Dictionary<string, Type>(ChainrEntry.STOCK_TRANSFORMS)
            {
                { transformType.Name, transformType }
            };
            Chainr unit = Chainr.FromSpec(spec, transforms);

            var input = new JObject();
            var actual = unit.Transform(input, null);

            input.Should().BeEquivalentTo(actual["input"]);
            delegateSpec.Should().BeEquivalentTo(actual["spec"]);
        }

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

        [TestCase(typeof(ExplodingTestTransform))]
        public void process_itBlowsUp_fromTransform(Type transformType) 
        {
            var spec = NewCustomChainrSpec(transformType, null);
            var transforms = new Dictionary<string, Type>(ChainrEntry.STOCK_TRANSFORMS)
            {
                { transformType.Name, transformType }
            };
            Chainr unit = Chainr.FromSpec(spec, transforms);
            FluentActions
                .Invoking(() => unit.Transform(new JObject(), null))
                .Should().Throw<TransformException>();
        }

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
            var testCase = GetTestCase($"chainr/integration/{testCaseName}");

            var unit = Chainr.FromSpec(testCase.Spec);

            unit.HasContextualTransforms().Should().BeFalse();
            unit.GetContextualTransforms().Count.Should().Be(0);

            var actual = unit.Transform(testCase.Input, null);

            actual.Should().BeEquivalentTo(testCase.Expected);

            if (sorted) 
            {
                // Make sure the sort actually worked.
                var orderErrorMessage = SortrTest.VerifyOrder(actual, testCase.Expected);
                orderErrorMessage.Should().BeNull(orderErrorMessage);
            }
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
