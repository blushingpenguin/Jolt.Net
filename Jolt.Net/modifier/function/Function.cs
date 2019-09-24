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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jolt.Net.Functions
{

    /**
     * Modifier supports a Function on RHS that accepts jolt path expressions as arguments and evaluates
     * them at runtime before calling it. Function always returns an Optional, and the value is written
     * only if the optional is not empty.
     *
     * function spec is defined by "key": "=functionName(args...)"
     *
     *
     * input:
     *      { "num": -1.0 }
     * spec:
     *      { "num": "=abs(@(1,&0))" }
     *      will call the stock function Math.abs() and will pass the matching value at "num"
     *
     * spec:
     *      { "num": "=abs" }
     *      an alternative shortcut will do the same thing
     *
     * output:
     *      { "num": 1.0 }
     *
     *
     *
     * input:
     *      { "value": -1.0 }
     *
     * spec:
     *      { "absValue": "=abs(@(1,value))" }
     *      will evaluate the jolt path expression @(1,value) and pass the output to stock function Math.abs()
     *
     * output:
     *      { "value": -1.0, "absValue": 1.0 }
     *
     *
     *
     * Currently defined stock functions are:
     *
     *      toLower     - returns toLower value of toString() value of first arg, rest is ignored
     *      toUpper     - returns toUpper value of toString() value of first arg, rest is ignored
     *      concat      - concatenate all given arguments' toString() values
     *
     *      min       - returns the min of all numbers provided in the arguments, non-numbers are ignored
     *      max       - returns the max of all numbers provided in the arguments, non-numbers are ignored
     *      abs         - returns the absolute value of first argument, rest is ignored
     *      toInteger   - returns the intValue() value of first argument if its numeric, rest is ignored
     *      toDouble    - returns the doubleValue() value of first argument if its numeric, rest is ignored
     *      toLong      - returns the longValue() value of first argument if its numeric, rest is ignored
     *
     * All of these functions returns Optional.EMPTY if unsuccessful, which results in a no-op when performing
     * the actual write in the json doc.
     *
     * i.e.
     * input:
     *      { "value1": "xyz" } --- note: string, not number
     *      { "value1": "1.0" } --- note: string, not number
     *
     * spec:
     *      { "value1": "=abs" } --- fails silently
     *      { "value2": "=abs" }
     *
     * output:
     *      { "value1": "xyz", "value2": "1" } --- note: "absValue": null is not inserted
     *
     *
     * This is work in progress, and probably will be changed in future releases. Hence it is marked for
     * removal as it'll eventually be moved to a different package as the Function feature is baked into
     * other transforms as well. In short this interface is not yet ready to be implemented outside jolt!
     *
     */

    public interface IFunction
    {
        JToken Apply(params JToken[] args);
    }

    public class Noop : IFunction
    {
        /**
         * Does nothing
         *
         * spec - "key": "=noop"
         *
         * will cause the key to remain unchanged
         */
        public JToken Apply(params JToken[] args)
        {
            return null;
        }
    }

    /**
     * Returns the first argument, null or otherwise
     *
     * spec - "key": [ "=isPresent", "otherValue" ]
     *
     * input - "key": null
     * output - "key": null
     *
     * input - "key": "value"
     * output - "key": "value"
     *
     * input - key is missing
     * output - "key": "otherValue"
     *
     */
    public class IsPresent : IFunction
    {
        public JToken Apply(params JToken[] args)
        {
            if (args.Length == 0)
            {
                return null;
            }
            return args[0];
        }
    }

    /**
     * Returns the first argument if in not null
     *
     * spec - "key": ["=notNull", "otherValue" ]
     *
     * input - "key": null
     * output - "key": "otherValue"
     *
     * input - "key": "value"
     * output - "key": "value"
     *
     */
    public class NotNull : IFunction
    {
        public JToken Apply(params JToken[] args)
        {
            if (args.Length == 0 || args[0] == null)
            {
                return null;
            }
            return args[0];
        }
    }

    /**
     * Returns the first argument if it is null
     *
     * spec - "key": ["=inNull", "otherValue" ]
     *
     * input - "key": null
     * output - "key": null
     *
     * input - "key": "value"
     * output - "key": "otherValue"
     *
     */
    public class IsNull : IFunction
    {
        public JToken Apply(params JToken[] args)
        {
            if (args.Length == 0 || args[0] != null)
            {
                return null;
            }
            return args[0];
        }
    }

    /**
     * Abstract class that processes var-args and calls two abstract methods
     *
     * If its single list arg, or many args, calls applyList()
     * else calls applySingle()
     *
     * @param <T> type of return value
     */
    public abstract class BaseFunction : IFunction
    {
        public JToken Apply(params JToken[] args)
        {
            if (args.Length == 0)
            {
                return null;
            }
            else if (args.Length == 1)
            {
                if (args[0] is JArray arr)
                {
                    if (arr.Count == 0)
                    {
                        return null;
                    }
                    return ApplyList(arr);
                }
                else if (args[0] == null)
                {
                    return null;
                }
                else
                {
                    return ApplySingle(args[0]);
                }
            }
            else
            {
                var arr = new JArray();
                foreach (var arg in args)
                {
                    arr.Add(arg);
                }
                return ApplyList(arr);
            }
        }

        protected abstract JToken ApplyList(JArray input);
        protected abstract JToken ApplySingle(JToken arg);
    }

    /**
     * Abstract class that provides rudimentary abstraction to quickly implement
     * a function that works on an single value input
     *
     * i.e. toUpperCase a string
     *
     * @param <T> type of return value
     */
    public abstract class SingleFunction : BaseFunction
    {
        protected override JToken ApplyList(JArray input)
        {
            var result = new JArray();
            foreach (var o in input)
            {
                var s = ApplySingle(o);
                result.Add(s != null ? s : o);
            }
            return result;
        }
    }

    /**
     * Abstract class that provides rudimentary abstraction to quickly implement
     * a function that works on an List of input
     *
     * i.e. find the max item from a list, etc.
     *
     */
    public abstract class ListFunction : BaseFunction
    {
        protected override JToken ApplySingle(JToken arg)
        {
            return null;
        }
    }

    /**
     * Abstract class that provides rudimentary abstraction to quickly implement
     * a function that classifies first arg as special input and rest as regular
     * input.
     *
     * @param <SOURCE> type of special argument
     * @param <RETTYPE> type of return value
     */
    public abstract class ArgDrivenFunction<T> : IFunction
    {
        public JToken Apply(params JToken[] args_)
        {
            IList<JToken> args = args_;
            if (args.Count == 1 && args[0] is JArray arr) 
            {
                args = arr;
            }

            if (TryGetSpecialArg(args, out T specialArg))
            {
                if (args.Count == 2)
                {
                    if (args[1] is JArray arr2)
                    {
                        return ApplyList(specialArg, arr2);
                    }
                    else
                    {
                        return ApplySingle(specialArg, args[1]);
                    }
                }
                else
                {
                    var input = new JArray(args.Skip(1));
                    return ApplyList(specialArg, input);
                }
            }
            else
            {
                return null;
            }
        }

        protected abstract bool TryGetSpecialArg(IList<JToken> args, out T value);
        protected abstract JToken ApplyList(T specialArg, JArray args);
        protected abstract JToken ApplySingle(T specialArg, JToken args);
    }

    public abstract class ArgDrivenListFunction<T> : ArgDrivenFunction<T>
    {
        protected override JToken ApplySingle(T specialArg, JToken arg)
        {
            return null;
        }
    }

    static class ArgDrivenFunctionHelper
    {
        public static bool TryGetSpecialArg(IList<JToken> args, out int value)
        {
            if (args.Count >= 2 && args[0].Type == JTokenType.Integer)
            {
                value = args[0].Value<int>();
                return true;
            }
            value = default;
            return false;
        }

        public static bool TryGetSpecialArg(IList<JToken> args, out string value)
        {
            if (args.Count >= 2 && args[0].Type == JTokenType.String)
            {
                value = args[0].ToString();
                return true;
            }
            value = null;
            return false;
        }
    }

    public abstract class ArgDrivenIntListFunction : ArgDrivenListFunction<int>
    {
        protected override bool TryGetSpecialArg(IList<JToken> args, out int value) =>
            ArgDrivenFunctionHelper.TryGetSpecialArg(args, out value);
    }

    public abstract class ArgDrivenStringListFunction : ArgDrivenListFunction<string>
    {
        protected override bool TryGetSpecialArg(IList<JToken> args, out string value) =>
            ArgDrivenFunctionHelper.TryGetSpecialArg(args, out value);
    }

    /**
     * Extends ArgDrivenConverter to provide rudimentary abstraction to quickly
     * implement a function that works on a single input
     *
     * i.e. increment(1, value)
     *
     * @param <S> type of special argument
     * @param <R> type of return value
     */
    public abstract class ArgDrivenSingleFunction<T> : ArgDrivenFunction<T>
    {
        protected override JToken ApplyList(T specialArg, JArray input)
        {
            var result = new JArray();
            foreach (var o in input)
            {
                var r = ApplySingle(specialArg, o);
                result.Add(r == null ? r : o);
            }
            return result;
        }
    }

    public abstract class ArgDrivenSingleStringFunction : ArgDrivenSingleFunction<string>
    {
        protected override bool TryGetSpecialArg(IList<JToken> args, out string value) =>
            ArgDrivenFunctionHelper.TryGetSpecialArg(args, out value);
    }
}
