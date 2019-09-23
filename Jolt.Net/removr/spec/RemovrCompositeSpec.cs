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
using System.Collections.Generic;

namespace Jolt.Net
{

    /*
        Sample Spec
        "spec": {
            "ineedtoberemoved":"" //literal leaf element
            "TAG-*$*": "",       //Leaf Computed element
            "TAG-*#*": "",

            "*pants*" : "",

             "buckets": {     //composite literal Path element
                "a$*": ""    //Computed Leaf element
             },
             "rating*":{    //composite computed path element
                "*":{       //composite computed path element
                    "a":""  //literal leaf element
                }
            }
        }
    */

    /**
     *  Removr Spec that has children. In a removr spec, whenever the RHS is a Map, we build a RemovrCompositeSpec
     */
    public class RemovrCompositeSpec : RemovrSpec
    {
        private readonly IReadOnlyList<RemovrSpec> _allChildNodes;

        public RemovrCompositeSpec(string rawKey, Dictionary<string, object> spec) :
            base(rawKey)
        {
            var all = new List<RemovrSpec>();

            foreach (var kv in spec)
            {
                string[] keyStrings = kv.Key.Split(new[] { "\\|" }, StringSplitOptions.None);
                foreach (string keyString in keyStrings)
                {
                    RemovrSpec childSpec;
                    if (kv.Value is Dictionary<string, object> dic)
                    {
                        childSpec = new RemovrCompositeSpec(keyString, dic);
                    }
                    else if (kv.Value is string s && String.IsNullOrWhiteSpace(s))
                    {
                        childSpec = new RemovrLeafSpec(keyString);
                    }
                    else
                    {
                        throw new SpecException("Invalid Removr spec RHS. Should be an empty string or Map");
                    }
                    all.Add(childSpec);
                }
            }
            _allChildNodes = all.AsReadOnly();
        }

        public override List<string> ApplyToMap(Dictionary<string, object> inputMap)
        {
            if (_pathElement is LiteralPathElement)
            {
                inputMap.TryGetValue(_pathElement.RawKey, out object subInput);
                ProcessChildren(_allChildNodes, subInput);
            }
            else if (_pathElement is IStarPathElement star)
            {
                // Compare my pathElement with each key from the input.
                // If it matches, recursively call process the child nodes.
                foreach (var entry in inputMap)
                {
                    if (star.StringMatch(entry.Key))
                    {
                        ProcessChildren(_allChildNodes, entry.Value);
                    }
                }
            }

            // Composite Nodes always return an empty list, as they dont actually remove anything.
            return new List<string>();
        }

        public override List<int?> ApplyToList(List<object> inputList)
        {

            // IF the input is a List, the only thing that will match is a Literal or a "*"
            if (_pathElement is LiteralPathElement)
            {

                int? pathElementInt = GetNonNegativeIntegerFromLiteralPathElement();

                if (pathElementInt.HasValue && pathElementInt.Value < inputList.Count)
                {
                    object subObj = inputList[pathElementInt.Value];
                    ProcessChildren(_allChildNodes, subObj);
                }
            }
            else if (_pathElement is StarAllPathElement)
            {
                foreach (object entry in inputList)
                {
                    ProcessChildren(_allChildNodes, entry);
                }
            }

            // Composite Nodes always return an empty list, as they dont actually remove anything.
            return new List<int?>();
        }

        /**
         * Call our child nodes, build up the set of keys or indices to actually remove, and then
         *  remove them.
         */
        private void ProcessChildren(IReadOnlyList<RemovrSpec> children, object subInput)
        {
            if (subInput != null)
            {
                if (subInput is List<object> subList)
                {
                    var indiciesToRemove = new List<int?>();

                    // build a list of all indicies to remove
                    foreach (RemovrSpec childSpec in children)
                    {
                        indiciesToRemove.AddRange(childSpec.ApplyToList(subList));
                    }

                    var uniqueIndiciesToRemove = new List<int?>();
                    // Sort the list from Biggest to Smallest, so that when we remove items from the input
                    //  list we don't muck up the order.
                    // Aka removing 0 _then_ 3 would be bad, because we would have actually removed
                    //  0 and 4 from the "original" list.
                    uniqueIndiciesToRemove.Sort();

                    foreach (var index in uniqueIndiciesToRemove)
                    {
                        if (index.HasValue)
                        {
                            subList.Remove(index);
                        }
                    }
                }
                else if (subInput is Dictionary<string, object> subInputMap)
                {
                    var keysToRemove = new List<string>();

                    foreach (RemovrSpec childSpec in children)
                    {
                        keysToRemove.AddRange(childSpec.ApplyToMap(subInputMap));
                    }

                    foreach (string keyToRemove in keysToRemove)
                    {
                        subInputMap.Remove(keyToRemove);
                    }
                }
            }
        }
    }
}
