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
     * Helper class that encapsulates the information one of the individual transform entries in
     * the Chainr spec's list.
     */
    public class ChainrEntry
    {

        /**
         * Map transform "operation" names to the classes that handle them
         */
        public static Dictionary<string, Type> STOCK_TRANSFORMS = new Dictionary<string, Type>
        {
            { "shift", typeof(Shiftr) },
            { "default", typeof(Defaultr) },
            { "modify-overwrite-beta", typeof(Modifier.Overwritr) },
            { "modify-default-beta", typeof(Modifier.Defaultr) },
            { "modify-define-beta", typeof(Modifier.Definr) },
            { "remove", typeof(Removr) },
            { "sort", typeof(Sortr) },
            { "cardinality", typeof(CardinalityTransform) }
        };

        public const string OPERATION_KEY = "operation";
        public const string SPEC_KEY = "spec";

        private readonly int _index;
        private readonly JToken _spec;

        private readonly Type _joltTransformType;
        private readonly bool _isSpecDriven;

        public JToken ChainrEntryObj { get; }

        /**
         * Process an element from the Chainr Spec into a ChainrEntry class.
         * This method tries to validate the syntax of the Chainr spec, whereas
         * the ChainrInstantiator deals with loading the Transform classes.
         *
         * @param chainrEntryObj the unknown object from the Chainr list
         * @param index the index of the chainrEntryObj, used in reporting errors
         */
        public ChainrEntry(int index, JToken chainrEntryObj)
        {
            if (!(chainrEntryObj is JObject chainrEntryMap))
            {
                throw new SpecException("JOLT ChainrEntry expects a JSON map - Malformed spec" + GetErrorMessageIndexSuffix());
            }

            _index = index;
            ChainrEntryObj = chainrEntryObj;
            string opString = ExtractOperationString(chainrEntryMap);

            if (opString == null)
            {
                throw new SpecException("JOLT Chainr 'operation' must implement Transform or ContextualTransform" + GetErrorMessageIndexSuffix());
            }

            if (!STOCK_TRANSFORMS.TryGetValue(opString, out Type type))
            {
                // TODO:
                throw new SpecException($"JOLT Chainr cannot find a handler for {opString}");
            }

            _joltTransformType = type;
            _isSpecDriven = typeof(SpecDriven).IsAssignableFrom(type);

            if (!chainrEntryMap.TryGetValue(ChainrEntry.SPEC_KEY, out _spec) &&
                _isSpecDriven)
            {
                throw new SpecException("JOLT Chainr - Transform className:" + type.Name + " requires a spec" + GetErrorMessageIndexSuffix());
            }
        }

        private string ExtractOperationString(JObject chainrEntryMap)
        {
            if (!chainrEntryMap.TryGetValue(ChainrEntry.OPERATION_KEY, out var operationNameObj))
            {
                return null;
            }
            else if (operationNameObj.Type == JTokenType.String)
            {
                var s = operationNameObj.ToString();
                if (String.IsNullOrWhiteSpace(s))
                {
                    throw new SpecException("JOLT Chainr '" + ChainrEntry.OPERATION_KEY + "' should not be blank" + GetErrorMessageIndexSuffix());
                }
                return s;
            }
            else
            {
                throw new SpecException("JOLT Chainr needs a '" + ChainrEntry.OPERATION_KEY + "' of type string" + GetErrorMessageIndexSuffix());
            }
        }

        /**
         * Generate an error message suffix what lists the index of the ChainrEntry in the overall ChainrSpec.
         */
        public string GetErrorMessageIndexSuffix()
        {
            return " at index:" + _index + ".";
        }

        /**
         * @return Spec for the transform, can be null
         */
        public JToken GetSpec() => _spec;

        /**
         * @return Class instance specified by this ChainrEntry
         */
        public Type GetJoltTransformType() => _joltTransformType;

        /**
         * @return true if the Jolt Transform specified by this ChainrEntry implements the SpecTransform interface
         */
        public bool IsSpecDriven() => _isSpecDriven;
    }
}
