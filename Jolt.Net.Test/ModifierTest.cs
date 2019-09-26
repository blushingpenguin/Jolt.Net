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
using Jolt.Net.Functions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jolt.Net.Test
{
    public class MinMaxLabelComputation : IFunction
    {
        private bool _isMax;

        protected MinMaxLabelComputation(bool isMax)
        {
            _isMax = isMax;
        }

        public JToken Apply(params JToken[] args)
        {
            if (args.Length == 0)
            {
                return null;
            }
            int? minmax = null;
            var valueLabels = (JObject)args[0];
            foreach (var labelKey in valueLabels.Properties().Select(p => p.Name))
            {
                if (Int32.TryParse(labelKey, out var val))
                {
                    minmax = minmax.HasValue ?
                        _isMax ? Math.Max(val, minmax.Value) :
                                 Math.Min(val, minmax.Value) :
                        val;
                }
            }
            return minmax.HasValue ? valueLabels[minmax.Value.ToString()] : null;
        }
    }

    class MinLabelComputation : MinMaxLabelComputation
    {
        public MinLabelComputation() :
            base(false)
        {
        }
    }

    class MaxLabelComputation : MinMaxLabelComputation
    {
        public MaxLabelComputation() :
            base(true)
        {
        }
    }

    public abstract class TemplatrTestCase
    {
        protected static IReadOnlyDictionary<string, IFunction> Functions { get; } = 
            new Dictionary<string, IFunction>(Modifier.STOCK_FUNCTIONS)
            {
                { "minLabelComputation", new MinLabelComputation() },
                { "maxLabelComputation", new MaxLabelComputation() }
            };

        class OverwritrTestCase : TemplatrTestCase
        {
            public override string Name => "OVERWRITR";
            public override Modifier GetTemplatr(JObject spec) =>
                new Modifier.Overwritr(spec, Functions);
        }

        class DefaultrTestCase : TemplatrTestCase
        {
            public override string Name => "DEFAULTR";
            public override Modifier GetTemplatr(JObject spec) =>
                new Modifier.Defaultr(spec, Functions);
        }

        class DefinrTestCase : TemplatrTestCase
        {
            public override string Name => "DEFINR";
            public override Modifier GetTemplatr(JObject spec) =>
                new Modifier.Definr(spec, Functions);
        }

        public static readonly TemplatrTestCase OVERWRITR = new OverwritrTestCase();
        public static readonly TemplatrTestCase DEFAULTR = new DefaultrTestCase();
        public static readonly TemplatrTestCase DEFINR = new DefinrTestCase();
        
        public static readonly TemplatrTestCase[] Cases = new TemplatrTestCase[] 
        {
            OVERWRITR, DEFAULTR, DEFAULTR
        };

        public abstract Modifier GetTemplatr(JObject spec);
        public abstract string Name { get; }

        public override string ToString() => Name;
    }

    [Parallelizable(ParallelScope.All)]
    public class ModifierTest : JsonTest
    {
        public static TestCaseData[] ModifierTestCases = new TestCaseData[]
        {
            new TestCaseData("modifier/mapLiteral"),
            new TestCaseData("modifier/mapLiteralWithNullInput"),
            new TestCaseData("modifier/mapLiteralWithMissingInput"),
            new TestCaseData("modifier/mapLiteralWithEmptyInput"),

            new TestCaseData("modifier/arrayElementAt"),

            new TestCaseData("modifier/arrayLiteral"),
            new TestCaseData("modifier/arrayLiteralWithNullInput"),
            new TestCaseData("modifier/arrayLiteralWithEmptyInput"),
            new TestCaseData("modifier/arrayLiteralWithMissingInput"),

            new TestCaseData("modifier/simple"),
            new TestCaseData("modifier/simpleArray"),
            new TestCaseData("modifier/arrayObject"),

            new TestCaseData("modifier/simpleMapNullToArray"),
            new TestCaseData("modifier/simpleMapRuntimeNull"),

            new TestCaseData("modifier/simpleLookup"),
            new TestCaseData("modifier/complexLookup"),

            new TestCaseData("modifier/simpleArrayLookup"),
            new TestCaseData("modifier/complexArrayLookup"),

            new TestCaseData("modifier/valueCheckSimpleArray"),
            new TestCaseData("modifier/valueCheckSimpleArrayNullInput"),
            new TestCaseData("modifier/valueCheckSimpleArrayEmptyInput"),

            new TestCaseData("modifier/valueCheckSimpleMap"),
            new TestCaseData("modifier/valueCheckSimpleMapNullInput"),
            new TestCaseData("modifier/valueCheckSimpleMapEmptyInput"),

            new TestCaseData("modifier/simpleMapOpOverride"),
            new TestCaseData("modifier/simpleArrayOpOverride"),

            new TestCaseData("modifier/testListOfFunction")
        };

        [TestCaseSource(nameof(ModifierTestCases))]
        public void TestOverwritrTransform(string testFile)
        {
            DoTest(testFile, TemplatrTestCase.OVERWRITR);
        }

        [TestCaseSource(nameof(ModifierTestCases))]
        public void TestDefaultrTransform(string testFile)
        {
            DoTest(testFile, TemplatrTestCase.DEFAULTR);
        }

        [TestCaseSource(nameof(ModifierTestCases))]
        public void TestDefinrTransform(string testFile)
        {
            DoTest(testFile, TemplatrTestCase.DEFINR);
        }

        private void DoTest(string testFile, TemplatrTestCase testCase)
        {
            var testUnit = GetJson(testFile);
            var input = testUnit["input"];
            var spec = (JObject)testUnit["spec"];
            var context = (JObject)testUnit["context"];
            var expected = testUnit[testCase.Name];
            if (expected != null) 
            {
                var modifier = testCase.GetTemplatr(spec);
                var actual = modifier.Transform(input, context);
                actual.Should().BeEquivalentTo(expected);
            }
        }

        public static IEnumerable<TestCaseData> GetSpecValidationTestCases()
        {
            var testObjects = (JArray)GetJson("modifier/validation/specThatShouldFail");
            
            foreach (var testCase in TemplatrTestCase.Cases)
            {
                for (int i = 0; i < testObjects.Count; ++i)
                {
                    var spec = (JObject)testObjects[i];
                    yield return new TestCaseData(testCase, spec)
                    {
                        TestName = $"TestInvalidSpecs({testCase.Name}.{i})"
                    };
                }
            }
        }

        [TestCaseSource(nameof(GetSpecValidationTestCases))]
        public void TestInvalidSpecs(TemplatrTestCase testCase, JObject spec) 
        {
            FluentActions
                .Invoking(() => testCase.GetTemplatr(spec))
                .Should().Throw<SpecException>();
        }

        public static readonly TestCaseData[] TestFunctionsCases = new TestCaseData[]
        {
            new TestCaseData("modifier/functions/stringsSplitTest", TemplatrTestCase.OVERWRITR),
            new TestCaseData("modifier/functions/padStringsTest", TemplatrTestCase.OVERWRITR),
            new TestCaseData("modifier/functions/stringsTests", TemplatrTestCase.OVERWRITR),
            new TestCaseData("modifier/functions/mathTests", TemplatrTestCase.OVERWRITR),
            new TestCaseData("modifier/functions/arrayTests", TemplatrTestCase.OVERWRITR),
            new TestCaseData("modifier/functions/sizeTests", TemplatrTestCase.OVERWRITR),
            new TestCaseData("modifier/functions/labelsLookupTest", TemplatrTestCase.DEFAULTR),
            new TestCaseData("modifier/functions/valueTests", TemplatrTestCase.OVERWRITR),
            new TestCaseData("modifier/functions/squashNullsTests", TemplatrTestCase.OVERWRITR)
        };

        [TestCaseSource(nameof(TestFunctionsCases))]
        public void TestFunctions(string testFile, TemplatrTestCase testCase)
        {
            DoTest( testFile, testCase);
        }

        public static readonly TestCaseData[] TestFunctionArgParseCases = new TestCaseData[]
        {
            new TestCaseData("fn(abc,efg,pqr)", new String[] {"fn", "abc", "efg", "pqr"}),
            new TestCaseData("fn(abc,@(1,2),pqr)", new String[] {"fn", "abc", "@(1,2)", "pqr"}),
            new TestCaseData("fn(abc,efg,pqr,)", new String[] {"fn", "abc", "efg", "pqr", ""}),
            new TestCaseData("fn(abc,,@(1,,2),,pqr,,)", new String[] {"fn", "abc", "","@(1,,2)","", "pqr", "", ""}),
            new TestCaseData("fn(abc,'e,f,g',pqr)", new String[] {"fn", "abc", "'e,f,g'", "pqr"}),
            new TestCaseData("fn(abc,'e(,f,)g',pqr)", new String[] {"fn", "abc", "'e(,f,)g'", "pqr"})
        };

        [TestCaseSource(nameof(TestFunctionArgParseCases))]
        public void TestFunctionArgParse(string argString, string[] expected)
        {
            var actual = SpecStringParser.ParseFunctionArgs(argString);
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void TestModifierFirstElementArray()
        {
            var input = new JObject(
                new JProperty("input", new JArray(5, 4))
            );

            var spec = new JObject(
                new JProperty("first", "=firstElement(@(1,input))")
            );

            var expected = new JObject(
                new JProperty("input", new JArray(5, 4)),
                new JProperty("first", 5)
            );

            var modifier = new Modifier.Overwritr(spec);
            var actual = modifier.Transform(input, null);
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
