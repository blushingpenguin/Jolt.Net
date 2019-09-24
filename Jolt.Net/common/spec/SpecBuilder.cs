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
     * Factory class that provides a factory method create(...) that takes itself
     * as argument to specify how to handle child specs
     *
     * @param <T>
     */
    public abstract class SpecBuilder<T> where T : IBaseSpec
    {
        /**
         * Recursively walk the spec input tree.
         */
        public List<T> CreateSpec(JObject rawSpec)
        {
            var result = new List<T>();
            var actualKeys = new HashSet<string>();

            foreach (var rawKv in rawSpec)
            {
                var keyStrings = rawKv.Key.Split('|'); // unwrap the syntactic sugar of the OR
                foreach (string keyString in keyStrings)
                {
                    T childSpec = CreateSpec(keyString, rawKv.Value);

                    string childCanonicalString = childSpec.GetPathElement().GetCanonicalForm();

                    if (actualKeys.Contains(childCanonicalString))
                    {
                        throw new InvalidOperationException("Duplicate canonical key found : " + childCanonicalString);
                    }

                    actualKeys.Add(childCanonicalString);

                    result.Add(childSpec);
                }
            }

            return result;
        }

        /**
         * Given a lhs key and rhs spec object, determine, create and return appropriate spec
         * @param lhsKey lhs key
         * @param rhsSpec rhs Spec
         * @return Spec object
         */
        public abstract T CreateSpec(string lhsKey, JToken rhsSpec);
    }
}
