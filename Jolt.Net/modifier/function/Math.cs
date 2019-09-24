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

using System;
using Newtonsoft.Json.Linq;

namespace Jolt.Net.Functions.Math
{
    /**
     * Given a list of objects, returns the max value in its appropriate type
     * also, interprets string as Number and returns appropriately
     *
     * max(1,2l,3d) == Optional.of(3d)
     * max(1,2l,"3.0") == Optional.of(3.0)
     * max("a", "b", "c") == Optional.empty()
     * max([]) == Optional.empty()
     */
    public class NumberListCompare : ListFunction
    {
        private readonly Func<long?, long, long?> _longCompareFn;
        private readonly Func<double?, double, double?> _doubleCompareFn;
        private readonly Func<double, long, bool> _doubleLongCompareFn;

        public NumberListCompare(
            Func<long?, long, long?> longCompareFn,
            Func<double?, double, double?> doubleCompareFn,
            Func<double, long, bool> doubleLongCompareFn
        )
        {
            _doubleCompareFn = doubleCompareFn;
            _doubleLongCompareFn = doubleLongCompareFn;
            _longCompareFn = longCompareFn;
        }

        protected override JToken ApplyList(JArray input)
        {
            if (input == null || input.Count == 0)
            {
                return null;
            }

            long? curLong = null;
            double? curDouble = null;

            foreach (var arg in input)
            {
                if (arg.Type == JTokenType.Integer)
                {
                    curLong = _longCompareFn(curLong, arg.Value<long>());
                }
                else if (arg.Type == JTokenType.Float)
                {
                    curDouble = _doubleCompareFn(curDouble, arg.Value<double>());
                }
                else if (arg.Type == JTokenType.String)
                {
                    string s = arg.Value<string>();
                    if (Int64.TryParse(s, out var longVal))
                    {
                        curLong = _longCompareFn(curLong, longVal);
                    }
                    else if (Double.TryParse(s, out var doubleVal))
                    {
                        curDouble = _doubleCompareFn(curDouble, doubleVal);
                    }
                }
            }

            if (curLong.HasValue)
            {
                if (curDouble.HasValue && _doubleLongCompareFn(curDouble.Value, curLong.Value))
                {
                    return curDouble.Value;
                }
                return curLong.Value;
            }
            if (curDouble.HasValue)
            {
                return curDouble.Value;
            }
            return null;
        }
    }

    public class Max : NumberListCompare
    {
        public Max() : base(
            (long? max, long val) => max.HasValue ? System.Math.Max(max.Value, val) : val,
            (double? max, double val) => max.HasValue ? System.Math.Max(max.Value, val) : val,
            (double a, long b) => a > b)
        {
        }
    }

    public class Min : NumberListCompare
    {
        public Min() : base(
            (long? min, long val) => min.HasValue ? System.Math.Min(min.Value, val) : val,
            (double? min, double val) => min.HasValue ? System.Math.Min(min.Value, val) : val,
            (double a, long b) => a > b)
        {
        }
    }

    /**
    * Given any object, returns, if possible. its absolute value wrapped in Optional
    * Interprets string as Number
    *
    * abs("-123") == Optional.of(123)
    * abs("123") == Optional.of(123)
    * abs("12.3") == Optional.of(12.3)
    *
    * abs("abc") == Optional.empty()
    * abs(null) == Optional.empty()
    *
    */
    public class Abs : SingleFunction
    {
        protected override JToken ApplySingle(JToken arg)
        {
            if (arg.Type == JTokenType.Integer)
            {
                return System.Math.Abs(arg.Value<long>());
            }
            if (arg.Type == JTokenType.Float)
            {
                return System.Math.Abs(arg.Value<double>());
            }
            if (arg.Type == JTokenType.String)
            {
                string s = arg.Value<string>();
                if (Int64.TryParse(s, out var longVal))
                {
                    return System.Math.Abs(longVal);
                }
                if (Int64.TryParse(s, out var doubleVal))
                {
                    return System.Math.Abs(doubleVal);
                }
            }
            return null;
        }
    }

    /**
     * Given a list of numbers, returns their avg as double
     * any value in the list that is not a valid number is ignored
     *
     * avg(2,"2","abc") == Optional.of(2.0)
     */
    public class Avg : ListFunction
    {
        protected override JToken ApplyList(JArray args)
        {
            double sum = 0.0;
            int count = 0;
            foreach (var arg in args)
            {

                if (arg.Type == JTokenType.Integer || arg.Type == JTokenType.Float)
                {
                    sum += arg.Value<double>();
                    ++count;
                }
                else if (arg.Type == JTokenType.String &&
                         Double.TryParse(arg.Value<string>(), out var doubleVal))
                {
                    sum += doubleVal;
                    ++count;
                }
            }
            if (count == 0)
            {
                return null;
            }
            return sum / count;
        }
    }

    public class IntSum : ListFunction
    {
        protected override JToken ApplyList(JArray args)
        {
            int sum = 0;
            foreach (var arg in args)
            {
                if (arg.Type == JTokenType.Integer)
                {
                    sum += arg.Value<int>();
                }
                else if (arg.Type == JTokenType.String &&
                         Int32.TryParse(arg.Value<string>(), out var intVal))
                {
                    sum += intVal;
                }
            }
            return sum;
        }
    }

    public class LongSum : ListFunction
    {
        protected override JToken ApplyList(JArray args)
        {
            long sum = 0;
            foreach (var arg in args)
            {
                if (arg.Type == JTokenType.Integer)
                {
                    sum += arg.Value<long>();
                }
                else if (arg.Type == JTokenType.String &&
                         Int64.TryParse(arg.Value<string>(), out var intVal))
                {
                    sum += intVal;
                }
            }
            return sum;
        }
    }

    public class DoubleSum : ListFunction
    {
        protected override JToken ApplyList(JArray args)
        {
            double sum = 0;
            foreach (var arg in args)
            {
                if (arg.Type == JTokenType.Integer)
                {
                    sum += arg.Value<double>();
                }
                else if (arg.Type == JTokenType.String &&
                         Double.TryParse(arg.Value<string>(), out var intVal))
                {
                    sum += intVal;
                }
            }
            return sum;
        }
    }

    public class IntSubtract : ListFunction
    {
        protected override JToken ApplyList(JArray args)
        {
            if (args == null || args.Count != 2 ||
                args[0].Type != JTokenType.Integer ||
                args[1].Type != JTokenType.Integer)
            {
                return null;
            }
            return args[0].Value<int>() - args[1].Value<int>();
        }
    }

    public class LongSubtract : ListFunction
    {
        protected override JToken ApplyList(JArray args)
        {
            if (args == null || args.Count != 2 ||
                args[0].Type != JTokenType.Integer ||
                args[1].Type != JTokenType.Integer)
            {
                return null;
            }
            return args[0].Value<long>() - args[1].Value<long>();
        }
    }

    public class DoubleSubtract : ListFunction
    {
        protected override JToken ApplyList(JArray args)
        {
            if (args == null || args.Count != 2 ||
                (args[0].Type != JTokenType.Integer && args[0].Type != JTokenType.Float) ||
                (args[1].Type != JTokenType.Integer && args[1].Type != JTokenType.Float))
            {
                return null;
            }
            return args[0].Value<double>() - args[1].Value<double>();
        }
    }

    public class Divide : ListFunction
    {
        public static JToken DividePair(JArray args)
        {
            if (args == null || args.Count != 2 ||
                (args[0].Type != JTokenType.Integer && args[0].Type != JTokenType.Float) ||
                (args[1].Type != JTokenType.Integer && args[1].Type != JTokenType.Float))
            {
                return null;
            }

            double denominator = args[1].Value<double>();
            if (denominator == 0)
            {
                return null;
            }
            double numerator = args[0].Value<double>();
            return numerator / denominator;
        }

        protected override JToken ApplyList(JArray args) =>
            Divide.DividePair(args);
    }

    public class DivideAndRound : ArgDrivenIntListFunction
    {
        protected override JToken ApplyList(int specialArg, JArray args)
        {
            JToken result = Divide.DividePair(args);
            if (result != null)
            {
                return System.Math.Round(result.Value<double>(), specialArg);
            }
            return result;
        }
    }
}
