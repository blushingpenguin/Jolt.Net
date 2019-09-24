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
    public abstract class BaseTraversalStep : ITraversalStep
    {
        protected readonly ITraversalStep _child;
        protected readonly Traversr _traversr;

        public BaseTraversalStep(Traversr traversr, ITraversalStep child)
        {
            _traversr = traversr;
            _child = child;
        }

        public abstract JToken Get(JToken tree, string key);
        public abstract Type GetStepType();
        public abstract JToken NewContainer();
        public abstract JToken OverwriteSet(JToken tree, string key, JToken data);
        public abstract JToken Remove(JToken tree, string key);

        public ITraversalStep GetChild()
        {
            return _child;
        }

        public JToken Traverse(JToken tree, TraversalStepOperation op, IEnumerator<string> keys, JToken data)
        {
            if (tree == null)
            {
                return null;
            }

            if (GetStepType().IsAssignableFrom(tree.GetType()))
            {
                keys.MoveNext();
                string key = keys.Current;

                if (_child == null)
                {
                    // End of the Traversal so do the set or get
                    switch (op)
                    {
                        case TraversalStepOperation.GET:
                            return Get(tree, key);
                        case TraversalStepOperation.SET:
                            return _traversr.HandleFinalSet(this, tree, key, data);
                        case TraversalStepOperation.REMOVE:
                            return Remove(tree, key);
                        default:
                            throw new InvalidOperationException("Invalid op:" + op.ToString());
                    }
                }
                else
                {

                    // We just an intermediate step, so traverse and then hand over control to our child
                    var optSub = _traversr.HandleIntermediateGet(this, tree, key, op);

                    if (optSub != null)
                    {
                        return _child.Traverse(optSub, op, keys, data);
                    }
                }
            }

            return null;
        }
    }
}
