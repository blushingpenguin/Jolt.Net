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

using Jolt.Net.Functions;
using Jolt.Net.Functions.Math;
using Jolt.Net.Functions.Objects;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class MathTest : AbstractTester 
    {
        public static IEnumerable<TestCaseData> GetTestCases()
        {
            var MAX_OF = new Max();
            var MIN_OF = new Min();
            var ABS_OF = new Abs();
            var TO_INTEGER = new ToInteger();
            var TO_DOUBLE = new ToDouble();
            var TO_LONG = new ToLong();

            var INT_SUM_OF = new IntSum();
            var DOUBLE_SUM_OF = new DoubleSum();
            var LONG_SUM_OF = new LongSum();

            var INT_SUBTRACT_OF = new IntSubtract();
            var DOUBLE_SUBTRACT_OF = new DoubleSubtract();
            var LONG_SUBTRACT_OF = new LongSubtract();

            var DIV_OF = new Divide();
            var DIV_AND_ROUND_OF = new DivideAndRound();

            object[][] tests = new object[][] {
                new object[] { "max-empty-array", MAX_OF, new JArray(), null },
                new object[] { "max-null", MAX_OF, null, null },
                new object[] { "max-object", MAX_OF, new JArray(new JObject()), null },

                new object[] { "max-single-int-array", MAX_OF, new JArray(1), new JValue(1) },
                new object[] { "max-single-long-array", MAX_OF, new JArray(1L), new JValue(1L) },
                new object[] { "max-single-double-array", MAX_OF, new JArray(1.0), new JValue(1.0)  },

                new object[] { "max-single-int-list", MAX_OF, new JArray(1), new JValue(1)  },
                new object[] { "max-single-long-list", MAX_OF, new JArray(1L), new JValue(1L) },
                new object[] { "max-single-double-list", MAX_OF, new JArray(1.0), new JValue(1.0)  },

                new object[] { "max-single-int-array-extra-arg", MAX_OF, new JArray(1, "a"), new JValue(1) },
                new object[] { "max-single-long-array-extra-arg", MAX_OF, new JArray(1L, "a"), new JValue(1L) },
                new object[] { "max-single-double-array-extra-arg", MAX_OF, new JArray(1.0, "a"), new JValue(1.0) },

                new object[] { "max-single-int-list-extra-arg", MAX_OF, new JArray(1, "a"), new JValue(1) },
                new object[] { "max-single-long-list-extra-arg", MAX_OF, new JArray(1L, "a"), new JValue(1L) },
                new object[] { "max-single-double-list-extra-arg", MAX_OF, new JArray(1.0, "a"), new JValue(1.0) },

                new object[] { "max-multi-int-array", MAX_OF, new JArray(1, 3, 2, 5), new JValue(5) },
                new object[] { "max-multi-long-array", MAX_OF, new JArray(1L, 3L, 2L, 5L), new JValue(5L) },
                new object[] { "max-multi-double-array", MAX_OF, new JArray(1.0, 3.0, 2.0, 5.0), new JValue(5.0) },

                new object[] { "max-multi-int-list", MAX_OF, new JArray(1, 3, 2, 5), new JValue(5) },
                new object[] { "max-multi-long-list", MAX_OF, new JArray(1L, 3L, 2L, 5L), new JValue(5L) },
                new object[] { "max-multi-double-list", MAX_OF, new JArray(1.0, 3.0, 2.0, 5.0), new JValue(5.0) },

                new object[] { "max-combo-int-array", MAX_OF, new JArray(1.0, 3L, null, 5), new JValue(5) },
                new object[] { "max-combo-long-array", MAX_OF, new JArray(1.0, 3L, null, 5L), new JValue(5L) },
                new object[] { "max-combo-double-array", MAX_OF, new JArray(1.0, 3L, null, 5.0), new JValue(5.0) },

                new object[] { "max-combo-int-list", MAX_OF, new JArray(1.0, 3L, null, 5), new JValue(5) },
                new object[] { "max-combo-long-list", MAX_OF, new JArray(1.0, 3L, null, 5L), new JValue(5L) },
                new object[] { "max-combo-double-list", MAX_OF, new JArray(1.0, 3L, null, 5.0), new JValue(5.0) },

                new object[] { "max-NaN", MAX_OF, new JArray(1.0, Double.NaN), new JValue(Double.NaN) },
                new object[] { "max-positive-infinity", MAX_OF, new JArray(1.0, Double.PositiveInfinity), new JValue(Double.PositiveInfinity) },
                new object[] { "max-NaN-positive-infinity", MAX_OF, new JArray(1.0, Double.NaN, Double.PositiveInfinity), new JValue(Double.NaN) },

                new object[] { "min-empty-array", MIN_OF, new JArray(), null },
                new object[] { "min-null", MIN_OF, null, null },
                new object[] { "min-object", MIN_OF, new JObject(), null },

                new object[] { "min-single-int-array", MIN_OF, new JArray(1), new JValue(1) },
                new object[] { "min-single-long-array", MIN_OF, new JArray(1L), new JValue(1L) },
                new object[] { "min-single-double-array", MIN_OF, new JArray(1.0), new JValue(1.0) },

                new object[] { "min-single-int-list", MIN_OF, new JArray(1), new JValue(1) },
                new object[] { "min-single-long-list", MIN_OF, new JArray(1L), new JValue(1L) },
                new object[] { "min-single-double-list", MIN_OF, new JArray(1.0), new JValue(1.0) },

                new object[] { "min-single-int-array-extra-arg", MIN_OF, new JArray(1, "a"), new JValue(1) },
                new object[] { "min-single-long-array-extra-arg", MIN_OF, new JArray(1L, "a"), new JValue(1L) },
                new object[] { "min-single-double-array-extra-arg", MIN_OF, new JArray(1.0, "a"), new JValue(1.0) },

                new object[] { "min-single-int-list-extra-arg", MIN_OF, new JArray(1, "a"), new JValue(1) },
                new object[] { "min-single-long-list-extra-arg", MIN_OF, new JArray(1L, "a"), new JValue(1L) },
                new object[] { "min-single-double-list-extra-arg", MIN_OF, new JArray(1.0, "a"), new JValue(1.0) },

                new object[] { "min-multi-int-array", MIN_OF, new JArray(1, 3, 2, 5), new JValue(1) },
                new object[] { "min-multi-long-array", MIN_OF, new JArray(1L, 3L, 2L, 5L), new JValue(1L) },
                new object[] { "min-multi-double-array", MIN_OF, new JArray(1.0, 3.0, 2.0, 5.0), new JValue(1.0) },

                new object[] { "min-multi-int-list", MIN_OF, new JArray(1, 3, 2, 5), new JValue(1) },
                new object[] { "min-multi-long-list", MIN_OF, new JArray(1L, 3L, 2L, 5L), new JValue(1L) },
                new object[] { "min-multi-double-list", MIN_OF, new JArray(1.0, 3.0, 2.0, 5.0), new JValue(1.0) },

                new object[] { "min-combo-int-array", MIN_OF, new JArray(1, 3L, null, 5.0), new JValue(1) },
                new object[] { "min-combo-long-array", MIN_OF, new JArray(1L, 3, null, 5.0), new JValue(1L) },
                new object[] { "min-combo-double-array", MIN_OF, new JArray(1.0, 3L, null, 5), new JValue(1.0) },

                new object[] { "min-combo-int-list", MIN_OF, new JArray(1, 3L, null, 5.0), new JValue(1) },
                new object[] { "min-combo-long-list", MIN_OF, new JArray(1L, 3, null, 5.0), new JValue(1L) },
                new object[] { "min-combo-double-list", MIN_OF, new JArray(1.0, 3L, null, 5), new JValue(1.0) },

                new object[] { "min-NaN", MIN_OF, new JArray(-1.0, Double.NaN), new JValue(Double.NaN) },
                new object[] { "min-negative-Infinity", MIN_OF, new JArray(-1.0, Double.NegativeInfinity), new JValue(Double.NegativeInfinity) },
                new object[] { "min-NaN-positive-infinity", MIN_OF, new JArray(-1.0, Double.NaN, Double.NegativeInfinity), new JValue(Double.NaN) },


                new object[] { "abs-null", ABS_OF, null, null },
                new object[] { "abs-invalid", ABS_OF, new JObject(), null },
                new object[] { "abs-empty-array", ABS_OF, new JArray(), null },

                new object[] { "abs-single-negative-int", ABS_OF, new JValue(-1), new JValue(1) },
                new object[] { "abs-single-negative-long", ABS_OF, new JValue(-1L), new JValue(1L) },
                new object[] { "abs-single-negative-double", ABS_OF, new JValue(-1.0), new JValue(1.0) },
                new object[] { "abs-single-positive-int", ABS_OF, new JValue(1), new JValue(1) },
                new object[] { "abs-single-positive-long", ABS_OF, new JValue(1L), new JValue(1L) },
                new object[] { "abs-single-positive-double", ABS_OF, new JValue(1.0), new JValue(1.0) },

                new object[] { "abs-list", ABS_OF, new JArray(-1, -1L, -1.0), new JArray(1, 1L, 1.0) },
                new object[] { "abs-array", ABS_OF, new JArray(-1, -1L, -1.0), new JArray(1, 1L, 1.0) },

                new object[] { "abs-Nan", ABS_OF, new JValue(Double.NaN), new JValue(Double.NaN) },
                new object[] { "abs-PosInfinity", ABS_OF, new JValue(Double.PositiveInfinity), new JValue(Double.PositiveInfinity) },
                new object[] { "abs-NefInfinity", ABS_OF, new JValue(Double.NegativeInfinity), new JValue(Double.PositiveInfinity) },


                new object[] { "toInt-null", TO_INTEGER, null, null },
                new object[] { "toInt-invalid", TO_INTEGER, new JObject(), null },
                new object[] { "toInt-empty-array", TO_INTEGER, new JArray(), null },

                new object[] { "toInt-single-positive-string", TO_INTEGER, new JValue("1"), new JValue(1) },
                new object[] { "toInt-single-negative-string", TO_INTEGER, new JValue("-1"), new JValue(-1) },
                new object[] { "toInt-single-positive-int", TO_INTEGER, new JValue(1), new JValue(1) },
                new object[] { "toInt-single-negative-int", TO_INTEGER, new JValue(-1), new JValue(-1) },
                new object[] { "toInt-single-positive-long", TO_INTEGER, new JValue(1L), new JValue(1) },
                new object[] { "toInt-single-negative-long", TO_INTEGER, new JValue(-1L), new JValue(-1) },
                new object[] { "toInt-single-positive-double", TO_INTEGER, new JValue(1.0), new JValue(1) },
                new object[] { "toInt-single-negative-double", TO_INTEGER, new JValue(-1.0), new JValue(-1) },

                new object[] { "toInt-single-positive-string-list", TO_INTEGER, new JArray("1", "2"), new JArray(1, 2) },
                new object[] { "toInt-single-negative-string-array", TO_INTEGER, new JArray("-1", "-2"), new JArray(-1, -2) },
                new object[] { "toInt-single-positive-int-list", TO_INTEGER, new JArray(1, 2), new JArray(1, 2) },
                new object[] { "toInt-single-negative-int-array", TO_INTEGER, new JArray(-1, -2), new JArray(-1, -2) },
                new object[] { "toInt-single-positive-long-list", TO_INTEGER, new JArray(1L, 2L), new JArray(1, 2) },
                new object[] { "toInt-single-negative-long-array", TO_INTEGER, new JArray(-1L, -2L), new JArray(-1, -2) },
                new object[] { "toInt-single-positive-double-list", TO_INTEGER, new JArray(1.0, 2.0), new JArray(1, 2) },
                new object[] { "toInt-single-negative-double-array", TO_INTEGER, new JArray(-1.0, -2.0), new JArray(-1, -2) },

                new object[] { "toDouble-null", TO_DOUBLE, null, null },
                new object[] { "toDouble-invalid", TO_DOUBLE, new JObject(), null },
                new object[] { "toDouble-empty-array", TO_DOUBLE, new JArray(), null },

                new object[] { "toDouble-single-positive-string", TO_DOUBLE, new JValue("1"), new JValue(1.0) },
                new object[] { "toDouble-single-negative-string", TO_DOUBLE, new JValue("-1"), new JValue(-1.0) },
                new object[] { "toDouble-single-positive-int", TO_DOUBLE, new JValue(1), new JValue(1.0) },
                new object[] { "toDouble-single-negative-int", TO_DOUBLE, new JValue(-1), new JValue(-1.0) },
                new object[] { "toDouble-single-positive-long", TO_DOUBLE, new JValue(1L), new JValue(1.0) },
                new object[] { "toDouble-single-negative-long", TO_DOUBLE, new JValue(-1L), new JValue(-1.0) },
                new object[] { "toDouble-single-positive-double", TO_DOUBLE, new JValue(1.0), new JValue(1.0) },
                new object[] { "toDouble-single-negative-double", TO_DOUBLE, new JValue(-1.0), new JValue(-1.0) },

                new object[] { "toDouble-single-positive-string-list", TO_DOUBLE, new JArray("1", "2"), new JArray(1.0, 2.0) },
                new object[] { "toDouble-single-negative-string-array", TO_DOUBLE, new JArray("-1", "-2"), new JArray(-1.0, -2.0) },
                new object[] { "toDouble-single-positive-int-list", TO_DOUBLE, new JArray(1, 2), new JArray(1.0, 2.0) },
                new object[] { "toDouble-single-negative-int-array", TO_DOUBLE, new JArray(-1, -2), new JArray(-1.0, -2.0) },
                new object[] { "toDouble-single-positive-long-list", TO_DOUBLE, new JArray(1L, 2L), new JArray(1.0, 2.0) },
                new object[] { "toDouble-single-negative-long-array", TO_DOUBLE, new JArray(-1L, -2L), new JArray(-1.0, -2.0) },
                new object[] { "toDouble-single-positive-double-list", TO_DOUBLE, new JArray(1.0, 2.0), new JArray(1.0, 2.0) },
                new object[] { "toDouble-single-negative-double-array", TO_DOUBLE, new JArray(-1.0, -2.0), new JArray(-1.0, -2.0) },

                new object[] { "toLong-null", TO_LONG, null, null },
                new object[] { "toLong-invalid", TO_LONG, new JObject(), null },
                new object[] { "toLong-empty-array", TO_LONG, new JArray(), null },

                new object[] { "toLong-single-positive-string", TO_LONG, new JValue("1"), new JValue(1L) },
                new object[] { "toLong-single-negative-string", TO_LONG, new JValue("-1"), new JValue(-1L) },
                new object[] { "toLong-single-positive-int", TO_LONG, new JValue(1), new JValue(1L) },
                new object[] { "toLong-single-negative-int", TO_LONG, new JValue(-1), new JValue(-1L) },
                new object[] { "toLong-single-positive-long", TO_LONG, new JValue(1L), new JValue(1L) },
                new object[] { "toLong-single-negative-long", TO_LONG, new JValue(-1L), new JValue(-1L) },
                new object[] { "toLong-single-positive-double", TO_LONG, new JValue(1L), new JValue(1L) },
                new object[] { "toLong-single-negative-double", TO_LONG, new JValue(-1L), new JValue(-1L) },

                new object[] { "toLong-single-positive-string-list", TO_LONG, new JArray("1", "2"), new JArray(1L, 2L) },
                new object[] { "toLong-single-negative-string-array", TO_LONG, new JArray("-1", "-2"), new JArray(-1L, -2L) },
                new object[] { "toLong-single-positive-int-list", TO_LONG, new JArray(1, 2), new JArray(1L, 2L) },
                new object[] { "toLong-single-negative-int-array", TO_LONG, new JArray(-1, -2), new JArray(-1L, -2L) },
                new object[] { "toLong-single-positive-long-list", TO_LONG, new JArray(1L, 2L), new JArray(1L, 2L) },
                new object[] { "toLong-single-negative-long-array", TO_LONG, new JArray(-1L, -2L), new JArray(-1L, -2L) },
                new object[] { "toLong-single-positive-double-list", TO_LONG, new JArray(1L, 2L), new JArray(1L, 2L) },
                new object[] { "toLong-single-negative-double-array", TO_LONG, new JArray(-1L, -2L), new JArray(-1L, -2L) },

                new object[] { "toInteger-combo-string-array", TO_INTEGER, new JArray("-1", 2, -3L, 4.0), new JArray(-1, 2, -3, 4) },
                new object[] { "toLong-combo-int-array",       TO_LONG,    new JArray("-1", 2, -3L, 4.0), new JArray(-1L, 2L, -3L, 4L) },
                new object[] { "toDouble-combo-long-array",    TO_DOUBLE,  new JArray("-1", 2, -3L, 4.0), new JArray(-1.0, 2.0, -3.0, 4.0) },

                new object[] { "intsum-combo-string-array",    INT_SUM_OF,  new JArray(1, 2.0, "random", 0), new JValue(3) },
                new object[] { "intsum-single-value",          INT_SUM_OF,  new JValue(2),                   null },
                new object[] { "intsum-combo-intstring-array", INT_SUM_OF,  new JArray(1L, 2, "-3.0", 0),    new JValue(0) },

                new object[] { "doublesum-combo-string-array",    DOUBLE_SUM_OF, new JArray(1, 2.0, "random", 0), new JValue(3.0) },
                new object[] { "doublesum-single-value",          DOUBLE_SUM_OF, new JValue(2),                   null },
                new object[] { "doublesum-combo-intstring-array", DOUBLE_SUM_OF, new JArray(1L, 2, "-3.0", 0),    new JValue(0.0) },

                new object[] { "longsum-combo-string-array",      LONG_SUM_OF,   new JArray(1, 2.0, "random", 0), new JValue(3L) },
                new object[] { "longsum-single-value",            LONG_SUM_OF,   new JValue(2),                   null },
                new object[] { "longsum-combo-intstring-array",   LONG_SUM_OF,   new JArray(1L, 2, "-3.0", 0),    new JValue(0L) },

                new object[] { "intsubtract-happy-path",      INT_SUBTRACT_OF,  new JArray(4, 1),  new JValue(3) },
                new object[] { "intsubtract-single-value",    INT_SUBTRACT_OF,  new JValue(2),     null },
                new object[] { "intsubtract-wrong-type",      INT_SUBTRACT_OF,  new JArray(4.0, 1), null },

                new object[] { "doublesubtract-happy-path",   DOUBLE_SUBTRACT_OF,  new JArray(4.0, 1.0),  new JValue(3.0) },
                new object[] { "doublesubtract-single-value", DOUBLE_SUBTRACT_OF,  new JValue(2.0),       null },
                new object[] { "doublesubtract-wrong-type",   DOUBLE_SUBTRACT_OF,  new JArray(4L, 1),     null },

                new object[] { "longsubtract-happy-path",     LONG_SUBTRACT_OF,  new JArray(4L, 1L),  new JValue(3L) },
                new object[] { "longsubtract-single-value",   LONG_SUBTRACT_OF,  new JValue(2L),      null },
                new object[] { "longsubtract-wrong-type",     LONG_SUBTRACT_OF,  new JArray(4.0, 1),  null },

                // Test to make sure "div" only uses the first and second element in the array and ignores the rest.
                new object[] { "div-combo-array",          DIV_OF, new JArray(10L, 5.0, 2), null },
                new object[] { "div-combo-string-array",   DIV_OF, new JArray(10L, "5", 2), null },
                new object[] { "div-single-element-array", DIV_OF, new JArray("5"),         null },
                new object[] { "div-single-element",       DIV_OF, new JValue("10"),        null },

                // Dividing by 0 returns an empty result.
                new object[] { "div-combo-invalid-array",  DIV_OF, new JArray(10L, 0, 2),   null },

                // Dividing 0 by any number returns 0.0(double)
                new object[] { "div-combo-valid-array",    DIV_OF, new JArray(0.0,  10), new JValue(0.0) },

                new object[] { "divAndRound-single-precision-array",      DIV_AND_ROUND_OF, new JArray(1, 5.0, 2), new JValue(2.5) },
                new object[] { "divAndRound-double-precision-array",      DIV_AND_ROUND_OF, new JArray(2, 5.0, 2), new JValue(2.50) },
                new object[] { "divAndRound-trailing-precision-array",    DIV_AND_ROUND_OF, new JArray(3, 5.0, 2), new JValue(2.500) },
                new object[] { "divAndRound-no-precision-array",          DIV_AND_ROUND_OF, new JArray(0, 5.0, 2), new JValue(3.0) }, // Round up as >= 0.5
                new object[] { "divAndRound-no-precision-array",          DIV_AND_ROUND_OF, new JArray(0, 4.8, 2), new JValue(2.0) }, // Round down as < 0.5
            };

            foreach (var test in tests)
            {
                yield return new TestCaseData(test.Skip(1).ToArray()) { TestName = $"RunTest({test[0]})" };
            }
        }

        [TestCaseSource(nameof(GetTestCases))]
        public void RunTest(IFunction function, JToken args, JToken expected)
        {
            TestFunction(function, args, expected);
        }
    }
}
