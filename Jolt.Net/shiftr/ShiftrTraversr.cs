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
using System.Collections.Generic;

namespace Jolt.Net
{
    /**
     * Traverser that does not overwrite data.
     */
    public class ShiftrTraversr : SimpleTraversr
    {
        public ShiftrTraversr(string humanPath) :
            base(humanPath)
        {
        }

        public ShiftrTraversr(List<string> paths) :
            base(paths)
        {
        }

        /**
         * Do a Shift style insert :
         *  1) if there is no data "there", then just set it
         *  2) if there is already a list "there", just add the data to the list
         *  3) if there something other than a list there, grab it and stuff it and the data into a list
         *     and overwrite what is there with a list.
         */
        public override OptionalObject HandleFinalSet(ITraversalStep traversalStep, object tree, string key, object data)
        {
            OptionalObject optSub = traversalStep.Get(tree, key);

            if (!optSub.HasValue || optSub.Value == null)
            {
                // nothing is here so just set the data
                traversalStep.OverwriteSet(tree, key, data);
            }
            else if (optSub.Value is List<object> lo) 
            {
                // there is a list here, so we just add to it
                lo.Add(data);
            }
            else
            {
                // take whatever is there and make it the first element in an Array
                var temp = new List<object>();
                temp.Add(optSub.Value);
                temp.Add(data);

                traversalStep.OverwriteSet(tree, key, temp);
            }

            return new OptionalObject(data);
        }
    }
}

