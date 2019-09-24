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
using System.Linq;

namespace Jolt.Net
{

    /**
     * Utility class for use in custom Transforms.
     *
     * Allows a programmer to just provide a single "human readable path"
     *  that they will want to be able to execute against multiple trees of data.
     *
     * Internally, parses the "human readable path" into a Traversr and a set of keys,
     *  so that the user only needs to call get/set with their input tree.
     *
     * Because the path is static, it is assumed that you will always be reading and writing
     *  objects of the same type to the tree, therefore this class can take a generic
     *  parameter "K" to reduce casting.
     */
    public class SimpleTraversal
    {
        private readonly SimpleTraversr _traversr;
        private readonly List<string> _keys;

        /**
         * Google Maps.newHashMap() trick to fill in generic type
         */
        public static SimpleTraversal NewTraversal(string humanReadablePath)
        {
            return new SimpleTraversal(humanReadablePath);
        }

        public SimpleTraversal(string humanReadablePath)
        {
            _traversr = new SimpleTraversr(humanReadablePath);

            string[] keysArray = humanReadablePath.Split('.');

            // extract the 3 from "[3]", but don't mess with "[]"
            for (int index = 0; index < keysArray.Length; index++)
            {
                string key = keysArray[index];
                if (key.Length > 2 && key[0] == '[' && key[key.Length - 1] == ']')
                {
                    keysArray[index] = key.Substring(1, key.Length - 2);
                }
            }

            _keys = keysArray.ToList();
        }

        /**
         * @param tree tree of Map and List JSON structure to navigate
         * @return the object you wanted, or null if the object or any step along the path to it were not there
         */
        public JToken Get(JToken tree)
        {
            return _traversr.Get(tree, _keys);
        }

        /**
         * @param tree tree of Map and List JSON structure to navigate
         * @param data JSON style data object you want to set
         * @return returns the data object if successfully set, otherwise null if there was a problem walking the path
         */
        public JToken Set(JToken tree, JToken data)
        {
            return _traversr.Set(tree, _keys, data);
        }

        /**
         * @param tree tree of Map and List JSON structure to navigate
         * @return removes and returns the data object if it was able to successfully navigate to it and remove it.
         */
        public JToken Remove(JToken tree)
        {
            return _traversr.Remove(tree, _keys);
        }
    }
}
