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
     * Helper class that encapsulates the Chainr spec's list.
     *
     * For reference : a Chainr spec should be an array of objects in order that look like this:
     *
     * <pre>
     * [
     *     {
     *         "operation": "[operation-name]",
     *         // stuff that the specific transform needs go here
     *     },
     *     ...
     * ]
     * </pre>
     *
     * This class represents the Array, while the ChainrEntry class encompass the individual elements
     * of the array.
     */
    public class ChainrSpec
    {

        private readonly IReadOnlyList<ChainrEntry> _chainrEntries;

        /**
         * @param chainrSpec Plain vanilla hydrated JSON representation of a Chainr spec .json file.
         */
        public ChainrSpec(JToken chainrSpec, IReadOnlyDictionary<string, Type> transforms)
        {
            if (!(chainrSpec is JArray operations))
            {
                throw new SpecException("JOLT Chainr expects a JSON array of objects - Malformed spec.");
            }

            if (operations.Count == 0)
            {
                throw new SpecException("JOLT Chainr passed an empty JSON array.");
            }

            var entries = new List<ChainrEntry>(operations.Count);

            for (int index = 0; index < operations.Count; index++)
            {
                var chainrEntryObj = operations[index];

                ChainrEntry entry = new ChainrEntry(index, chainrEntryObj, transforms);

                entries.Add(entry);
            }

            _chainrEntries = entries.AsReadOnly();
        }

        /**
         * @return the list of ChainrEntries from the initialize file
         */
        public IReadOnlyList<ChainrEntry> GetChainrEntries() => _chainrEntries;
    }
}
