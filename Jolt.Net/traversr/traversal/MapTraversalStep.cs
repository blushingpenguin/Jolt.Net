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
     * TraversalStep that expects to handle Map objects.
     */
    public class MapTraversalStep : BaseTraversalStep
    {

        public MapTraversalStep(Traversr traversr, ITraversalStep child) :
            base(traversr, child)
        {
        }

        public override Type GetStepType() => typeof(Dictionary<string, object>);

        public override object NewContainer() => new Dictionary<string, object>();

        public override OptionalObject Get(object tree, string key)
        {
            var map = (Dictionary<string, object>)tree;

            // This here was the whole point of adding the Optional stuff.
            // Aka, I need a way to distinguish between the key not existing in the map
            //  or the key existing but having a _valid_ null value.
            return map.TryGetValue(key, out var value) ?
                new OptionalObject(value) : new OptionalObject();
        }

        public override OptionalObject Remove(object tree, string key)
        {
            var map = (Dictionary<string, object>)tree;
            if (map.TryGetValue(key, out var value))
            {
                map.Remove(key);
                return new OptionalObject(value);
            }
            return new OptionalObject();
        }

        public override OptionalObject OverwriteSet(object tree, string key, object data)
        {
            var map = (Dictionary<string, object>)tree;
            map[key] = data;
            return new OptionalObject(data);
        }
    }
}
