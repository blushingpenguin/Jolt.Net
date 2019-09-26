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
using System.Collections;
using System.Collections.Generic;

namespace Jolt.Net
{

    /**
     * DataStructure used by a SpecTransform during it's parallel tree walk.
     *
     * Basically this is Stack that records the steps down the tree that have been taken.
     * For each level, there is a PathStep, which contains a pointer the data of that level,
     *  and a pointer to the LiteralPathElement matched at that level.
     *
     * At any given point in time, it represents where in the tree walk a Spec is operating.
     * It is primarily used to by the ShiftrLeafSpec and CardinalityLeafSpec as a reference
     * to lookup real values for output "&(1,1)" references.
     *
     * It is expected that as the SpecTransform navigates down the tree, MatchedElements will be added and then
     *  removed when that subtree has been walked.
     */
    public class WalkedPath : IList<PathStep>
    {
        private List<PathStep> _list;

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public PathStep this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        public WalkedPath()
        {
            _list = new List<PathStep>();
        }

        public WalkedPath(IEnumerable<PathStep> c)
        {
            _list = new List<PathStep>(c);
        }

        public WalkedPath(JToken treeRef, MatchedElement matchedElement) :
            this()
        {
            Add(new PathStep(treeRef, matchedElement));
        }

        /**
         * Convenience method
         */
        public void Add(JToken treeRef, MatchedElement matchedElement)
        {
            Add(new PathStep(treeRef, matchedElement));
        }

        public void RemoveLast()
        {
            _list.RemoveAt(_list.Count - 1);
        }

        /**
         * Method useful to "&", "&1", "&2", etc evaluation.
         */
        public PathStep ElementFromEnd(int idxFromEnd)
        {
            if (_list.Count == 0)
            {
                return null;
            }
            return _list[_list.Count - 1 - idxFromEnd];
        }

        public PathStep LastElement()
        {
            return _list[_list.Count - 1];
        }

        public int IndexOf(PathStep item) =>
            _list.IndexOf(item);

        public void Insert(int index, PathStep item) =>
            _list.Insert(index, item);

        public void RemoveAt(int index) =>
            _list.RemoveAt(index);

        public void Add(PathStep item) =>
            _list.Add(item);

        public void Clear() =>
            _list.Clear();

        public bool Contains(PathStep item) =>
            _list.Contains(item);

        public void CopyTo(PathStep[] array, int arrayIndex) =>
            _list.CopyTo(array, arrayIndex);

        public bool Remove(PathStep item) =>
            _list.Remove(item);

        public IEnumerator<PathStep> GetEnumerator() =>
            _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)_list).GetEnumerator();
    }
}
