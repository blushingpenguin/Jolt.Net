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
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jolt.Net.Functions.Objects
{
    class ToInteger : SingleFunction
    {
        protected override JToken ApplySingle(JToken arg)
        {
            if (arg.Type == JTokenType.Integer ||
                arg.Type == JTokenType.Float)
            {
                return arg.Value<int>();
            }
            if (arg.Type == JTokenType.String &&
                Int32.TryParse(arg.Value<string>(), out var intVal))
            {
                return intVal;
            }
            return null;
        }
    }

    class ToLong : SingleFunction
    {
        protected override JToken ApplySingle(JToken arg)
        {
            if (arg.Type == JTokenType.Integer ||
                arg.Type == JTokenType.Float)
            {
                return arg.Value<long>();
            }
            if (arg.Type == JTokenType.String &&
                Int64.TryParse(arg.Value<string>(), out var longVal))
            {
                return longVal;
            }
            return null;
        }
    }

    class ToDouble : SingleFunction
    {
        protected override JToken ApplySingle(JToken arg)
        {
            if (arg.Type == JTokenType.Integer ||
                arg.Type == JTokenType.Float)
            {
                return arg.Value<double>();
            }
            if (arg.Type == JTokenType.String &&
                Double.TryParse(arg.Value<string>(), out var doubleVal))
            {
                return doubleVal;
            }
            return null;
        }
    }

    class ToBoolean : SingleFunction
    {
        protected override JToken ApplySingle(JToken arg)
        {
            if (arg.Type == JTokenType.Boolean)
            {
                return arg;
            }
            if (arg.Type == JTokenType.String)
            {
                string s = arg.Value<string>();
                if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (s.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return null;
        }
    }

    class ToString : SingleFunction
    {
        private string TokenToString(JToken arg)
        {
            if (arg.Type == JTokenType.String)
            {
                return arg.Value<string>();
            }
            if (arg.Type == JTokenType.Array)
            {
                var sb = new StringBuilder();
                sb.Append("[");
                foreach (var elt in (JArray)arg)
                {
                    sb.Append(TokenToString(elt));
                }
                sb.Append("]");
                return sb.ToString();
            }
            return arg.ToString();
        }

        protected override JToken ApplySingle(JToken arg)
        {
            if (arg.Type == JTokenType.String)
            {
                return arg;
            }
            return TokenToString(arg);
        }
    }

    class SquashNulls : SingleFunction
    {
        public static JToken Squash(JToken arg)
        {
            if (arg.Type == JTokenType.Array)
            {
                var arr = (JArray)arg;
                for (int i = 0; i < arr.Count;)
                {
                    if (arr[i].Type == JTokenType.Null)
                        arr.RemoveAt(i);
                    else
                        ++i;
                }
            }
            else if (arg.Type == JTokenType.Object)
            {
                var obj = (JObject)arg;
                var newObj = new JObject();
                foreach (var kv in obj)
                {
                    if (kv.Value.Type != JTokenType.Null)
                    {
                        newObj.Add(kv.Key, kv.Value);
                    }
                }
                return newObj;
            }
            return arg;
        }

        protected override JToken ApplySingle(JToken arg) =>
            Squash(arg);
    }

    public class RecursivelySquashNulls : SingleFunction
    {
        protected override JToken ApplySingle(JToken arg)
        {
            // Makes two passes thru the data.
            arg = SquashNulls.Squash(arg);

            if (arg.Type == JTokenType.Array)
            {
                var arr = (JArray)arg;
                for (int i = 0; i < arr.Count; ++i)
                {
                    arr[i] = ApplySingle(arr[i]);
                }
            }
            else if (arg.Type == JTokenType.Object)
            {
                var obj = (JObject)arg;
                foreach (var kv in obj)
                {
                    obj[kv.Key] = ApplySingle(kv.Value);
                }
            }
            return arg;
        }
    }

    /**
     * Size is a special snowflake and needs specific care
     */
    public class Size : IFunction
    {
        public JToken Apply(params JToken[] args)
        {
            if (args == null || args.Length == 0)
            {
                return null;
            }

            if (args.Length == 1)
            {
                if (args[0] == null)
                {
                    return null;
                }
                if (args[0].Type == JTokenType.Array)
                {
                    return ((JArray)args[0]).Count;
                }
                if (args[0].Type == JTokenType.String)
                {
                    return args[0].ToString().Length;
                }
                if (args[0].Type == JTokenType.Object)
                {
                    return ((JObject)args[0]).Count;
                }
                return null;
            }

            return args.Length;
        }
    }
}
