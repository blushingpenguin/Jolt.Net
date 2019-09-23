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
     * Simple Traversr that
     *
     * 1 Does overwrite sets at the leaf level
     * 2 Will create intermediate container objects only on SET operations
     */
    public class SimpleTraversr : Traversr
    {

        public SimpleTraversr(string humanPath) :
            base(humanPath)
        {
        }

        public SimpleTraversr(List<string> paths) :
            base(paths)
        {
        }

        public override OptionalObject HandleFinalSet(ITraversalStep traversalStep, object tree, string key, object data)
        {
            return traversalStep.OverwriteSet(tree, key, data);
        }

        /**
         * Only make a new instance of a container object for SET, if there is nothing "there".
         */
        public override OptionalObject HandleIntermediateGet(ITraversalStep traversalStep, object tree, string key, TraversalStepOperation op)
        {
            OptionalObject optSub = traversalStep.Get(tree, key);

            object sub = optSub.Value;

            if (sub == null && op == TraversalStepOperation.SET)
            {
                // get our child to make the container object, so it will be happy with it
                sub = traversalStep.GetChild().NewContainer();
                traversalStep.OverwriteSet(tree, key, sub);
            }

            return new OptionalObject(sub);
        }
    }
}
