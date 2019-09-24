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
     * From the spec we need to guess the DataType of the incoming input
     *
     * This is useful for,
     * a) in cases where the spec suggested a list but input was map
     *    and vice versa, where we can just skip processing instead of
     *    throwing random array/map errors
     * b) in case where the input is actually null and we need to create
     *    appropriate data structure and then apply spec logic
     *
     * Note: By design jolt does not stop processing on bad input data
     */
    public abstract class DataType
    {

        private static readonly RUNTIME _runtimeInstance = new RUNTIME();
        private static readonly MAP _mapInstance = new MAP();

        public static DataType DetermineDataType(int confirmedArrayAtIndex, int confirmedMapAtIndex, int maxExplicitIndex)
        {
            // based on provided flags, set appropriate dataType
            if (confirmedArrayAtIndex > -1)
            {
                return new LIST(maxExplicitIndex);
            }
            else if (confirmedMapAtIndex > -1)
            {
                return _mapInstance;
            }
            // only a single "*" key was defined in spec. We need to get dataType at runtime from input
            else
            {
                return _runtimeInstance;
            }
        }

        /**
         * Determines if an input is compatible with current DataType
         */
        public abstract bool IsCompatible(JToken input);

        /**
         * MAP and LIST types overrides this method to return appropriate new map or list
         */
        protected abstract JToken CreateValue();

        /**
         * LIST overrides this method to expand the source (list) such that in can support
         * an index specified in spec that is outside the range input list, returns original size
         * of the input
         */
        public virtual int? Expand(JToken source)
        {
            throw new InvalidOperationException("Expand not supported in " + GetType().Name + " Type");
        }

        /**
         * Creates an empty map/list, as required by spec, in the parent map/list at given key/index
         *
         * @param keyOrIndex of the parent object to create
         * @param walkedPath containing the parent object
         * @param opMode     to determine if this write operation is allowed
         * @return newly created object
         */
        public JToken Create(string keyOrIndex, WalkedPath walkedPath, OpMode opMode)
        {
            object parent = walkedPath.LastElement().TreeRef;
            int? origSizeOptional = walkedPath.LastElement().OrigSize;
            if (!Int32.TryParse(keyOrIndex, out int index))
            {
                index = -1;
            }
            JToken value = null;
            if (parent is JObject map && opMode.IsApplicable(map, keyOrIndex))
            {
                map[keyOrIndex] = CreateValue();
            }
            else if (parent is JArray list && opMode.IsApplicable(list, index, origSizeOptional.Value))
            {
                list[index] = CreateValue();
            }
            return value;
        }
    }

    /**
     * List type that records maxIndex from spec, and uses that to expand a source (list) properly
     */
    public class LIST : DataType
    {
        private readonly int _maxIndexFromSpec;

        public LIST(int maxIndexFromSpec)
        {
            _maxIndexFromSpec = maxIndexFromSpec;
        }

        protected override JToken CreateValue() => new JArray();

        public override int? Expand(JToken input)
        {
            var source = (JArray)input;
            int reqIndex = _maxIndexFromSpec;
            int currLastIndex = source.Count - 1;
            int origSize = currLastIndex + 1;
            if (reqIndex >= source.Count)
            {
                while (currLastIndex++ < reqIndex)
                {
                    source.Add(null);
                }
            }
            return origSize;
        }

        public override bool IsCompatible(JToken input)
        {
            return input == null || input is JArray;
        }
    }

    /**
     * MAP type class
     */
    public class MAP : DataType
    {
        protected override JToken CreateValue() => new JObject();

        public override bool IsCompatible(JToken input) =>
            input == null || input is JObject;
    }

    /**
     * Runtime type
     */
    public class RUNTIME : DataType
    {
        public override bool IsCompatible(JToken input) =>
            input != null;
        protected override JToken CreateValue() =>
            throw new InvalidOperationException("Cannot create for RUNTIME Type");
    }
}
