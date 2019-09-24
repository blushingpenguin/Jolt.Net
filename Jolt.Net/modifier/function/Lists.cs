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

using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Jolt.Net.Functions.Lists
{
    /**
     * Given a list, return the first element
     */
    public class FirstElement : ListFunction
    {
        protected override JToken ApplyList(JArray input)
        {
            return input.Count > 0 ? input[0] : null;
        }
    }

    /**
     * Given a list, return the last element
     */
    public class LastElement : ListFunction
    {
        protected override JToken ApplyList(JArray input)
        {
            return input.Count > 0 ? input[input.Count - 1] : null;
        }
    }

    /**
     * Given an index at arg[0], and a list at arg[1] or args[1...N], return element at index of list or array
     */
    public class ElementAt : ArgDrivenIntListFunction
    {
        protected override JToken ApplyList(int specialArg, JArray args)
        {
            if (args != null && specialArg >= 0 && specialArg < args.Count)
            {
                return args[specialArg];
            }
            return null;
        }
    }

    /**
     * Given an arbitrary number of arguments, return them as list
     */
    public class ToList : BaseFunction
    {
        protected override JToken ApplyList(JArray input)
        {
            return input;
        }

        protected override JToken ApplySingle(JToken arg)
        {
            return new JArray(arg);
        }
    }

    /**
     * Given an arbitrary list of items, returns a new array of them in sorted state
     */
    public class Sort : BaseFunction
    {
        protected override JToken ApplyList(JArray input)
        {
            return new JArray(input.OrderBy(x => x.ToString()));
        }

        protected override JToken ApplySingle(JToken arg)
        {
            return arg;
        }
    }
}
