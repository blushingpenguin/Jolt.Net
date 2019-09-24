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
using System.Collections.Generic;

namespace Jolt.Net
{

    /**
     * Subclass of ArrayTraversalStep that does not care about array index numbers.
     * Instead it will just do an array add on any set.
     *
     * Consequently, get and remove are rather meaningless.
     *
     * This exists, because we need a way in the human readable path, so say that we
     *  always want a list value.
     *
     * Example : "tuna.marlin.[]"
     *   We want the value of marlin to always be a list, and anytime we set data
     *   to marlin, it should just be added to the list.
     */
    public class AutoExpandArrayTraversalStep : ArrayTraversalStep
    {
        public AutoExpandArrayTraversalStep(Traversr traversr, ITraversalStep child) :
                base(traversr, child)
        {
        }

        public override JToken Get(JToken list, string key)
        {
            if ("[]" != key)
            {
                throw new TraversrException("AutoExpandArrayTraversal expects a '[]' key. Was: " + key);
            }

            return null;
        }

        public override JToken Remove(JToken tree, string key)
        {
            if ("[]" != key)
            {
                throw new TraversrException("AutoExpandArrayTraversal expects a '[]' key. Was: " + key);
            }

            return null;
        }

        public override JToken OverwriteSet(JToken tree, string key, JToken data)
        {
            if ("[]" != key)
            {
                throw new TraversrException("AutoExpandArrayTraversal expects a '[]' key. Was: " + key);
            }
            var list = (JArray)tree;
            list.Add(data);
            return data;
        }
    }
}
