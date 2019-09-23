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

    /**
     * TraversalStep that expects to handle List objects.
     */
    public class ArrayTraversalStep : BaseTraversalStep
    {

        public ArrayTraversalStep(Traversr traversr, ITraversalStep child) :
            base(traversr, child)
        {
        }

        public override Type GetStepType() => typeof(List<object>);

        public override object NewContainer() => new List<object>();

        public override OptionalObject Get(object tree, string key)
        {
            var list = (List<object>)tree;
            int arrayIndex = Int32.Parse(key);
            if (arrayIndex < list.Count)
            {
                return new OptionalObject(list[arrayIndex]);
            }
            return new OptionalObject();
        }

        public override OptionalObject Remove(object tree, string key)
        {
            var list = (List<object>)tree;
            int arrayIndex = Int32.Parse(key);
            if (arrayIndex < list.Count)
            {
                var value = list[arrayIndex];
                list.RemoveAt(arrayIndex);
                return new OptionalObject(value);
            }
            return new OptionalObject();
        }

        public override OptionalObject OverwriteSet(object tree, string key, object data)
        {
            var list = (List<object>)tree;
            int arrayIndex = Int32.Parse(key);
            EnsureArraySize(list, arrayIndex);            // make sure it is big enough
            list[arrayIndex] = data;
            return new OptionalObject(data);
        }

        private static void EnsureArraySize(List<object> list, int upperIndex)
        {
            for (int sizing = list.Count; sizing <= upperIndex; sizing++)
            {
                list.Add(null);
            }
        }
    }
}

