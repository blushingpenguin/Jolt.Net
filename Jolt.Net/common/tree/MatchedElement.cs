/*
 * Copyright 2016 Bazaarvoice, Inc.
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
     * MatchedElement is the result of a "match" between a spec PathElement and some input data.
     *
     * MatchedElements are not thread safe, and should instead be stack / single Thread/Transform specific.
     *
     * This mutability was specifically added for the the HashCount functionality, which allows Shiftr
     *  to transform data form maps to lists.
     */
    public class MatchedElement : BasePathElement, IEvaluatablePathElement
    {
        private readonly IReadOnlyList<string> _subKeys;

        private int _hashCount = 0;

        public MatchedElement(string key) :
            base(key)
        {
            var subKeys = new List<string>();
            subKeys.Add(key); // always add the full key to index 0
            _subKeys = subKeys.AsReadOnly();
        }

        public MatchedElement(string key, List<string> subKeys) :
            base(key)
        {
            if (subKeys == null)
            {
                throw new ArgumentNullException(nameof(subKeys), 
                    $"MatchedElement for key:{key} got null list of subKeys");
            }

            var keys = new List<string>(1 + subKeys.Count);
            keys.Add(key); // always add the full key to index 0
            keys.AddRange(subKeys);
            _subKeys = keys.AsReadOnly();
        }

        public string Evaluate(WalkedPath walkedPath) =>
            RawKey;

        public override string GetCanonicalForm() =>
            RawKey;

        public string GetSubKeyRef(int index)
        {
            if ((index < 0) || (index >= _subKeys.Count))
            {
                throw new IndexOutOfRangeException("MatchedElement " + nameof(_subKeys) + " cannot be indexed with index " + index);
            }
            return _subKeys[index];
        }

        public int GetSubKeyCount()
        {
            return _subKeys.Count;
        }

        public int GetHashCount()
        {
            return _hashCount;
        }

        /**
         * Here be mutability...
         */
        public void IncrementHashCount()
        {
            _hashCount++;
        }
    }
}
