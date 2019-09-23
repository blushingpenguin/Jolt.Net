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
    public abstract class BaseTraversalStep : ITraversalStep
    {

        protected readonly ITraversalStep child;
        protected readonly Traversr traversr;

        public BaseTraversalStep(Traversr traversr, ITraversalStep child)
        {
            this.traversr = traversr;
            this.child = child;
        }

        public abstract OptionalObject Get(object tree, string key);
        public abstract Type GetStepType();
        public abstract object NewContainer();
        public abstract OptionalObject OverwriteSet(object tree, string key, object data);
        public abstract OptionalObject Remove(object tree, string key);

        public ITraversalStep GetChild()
        {
            return child;
        }

        public OptionalObject Traverse(object tree, TraversalStepOperation op, IEnumerator<string> keys, object data)
        {
            if (tree == null)
            {
                return new OptionalObject();
            }

            if (GetStepType().IsAssignableFrom(tree.GetType()))
            {
                keys.MoveNext();
                string key = keys.Current;

                if (child == null)
                {
                    // End of the Traversal so do the set or get
                    switch (op)
                    {
                        case TraversalStepOperation.GET:
                            return Get(tree, key);
                        case TraversalStepOperation.SET:
                            return traversr.HandleFinalSet(this, tree, key, data);
                        case TraversalStepOperation.REMOVE:
                            return Remove(tree, key);
                        default:
                            throw new InvalidOperationException("Invalid op:" + op.ToString());
                    }
                }
                else
                {

                    // We just an intermediate step, so traverse and then hand over control to our child
                    OptionalObject optSub = traversr.HandleIntermediateGet(this, tree, key, op);

                    if (optSub.HasValue)
                    {
                        return child.Traverse(optSub.Value, op, keys, data);
                    }
                }
            }

            return new OptionalObject();
        }
    }
}
