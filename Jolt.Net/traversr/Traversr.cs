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
using System.Collections.Generic;

namespace Jolt.Net
{

    /**
     * Traversr allows you to walk JSON tree structures of data, and to GET and SET operations.
     *
     * Corner cases that arise during tree walk, are handled by subclasses.
     * Ex: If no data exists mid tree walk quit or insert a new container?
     *     Or if there is data but it is the wrong type : overwrite or skip?
     *
     * Traversr analyzes the path path to be traversed and creates a "linked list" of Traversal objects.
     *
     * Then that list of Traversals can be used many times to write data into different JSON tree structures
     *  with different key values.
     *
     * For example given a Shiftr output path of :  "tuna[&1].bob.&3[]" some of the keys are known,
     *  "tuna" and "bob", but other keys will only be known later.
     *
     * However, the structure of the output path will not change, which means we can do some work before
     *  the keys are known.
     *
     * First the output path is turned into its canonical form : "tuna.[4].[&1].bob.&3.[]".
     * Then, a series of Traversals is created.
     *  tuna -> MapTraversal
     *  [&1] -> ArrayTraversal
     *  bob  -> MapTraversal
     *  &3   -> MapTraversal
     *  []   -> AutoExpandArrayTraversal
     *
     * Later, a list of keys can then be provided, such as
     *  [ "tuna", "2", "bob", "smith", "[]" ], and they can be quickly used without having to build or
     *  parse any more objects.
     *
     * The list of keys are all Strings, which ArrayTraversals will convert to Integers as needed.
     */
    public abstract class Traversr // <DataType>
    {
        private readonly ITraversalStep _root;
        private readonly int _traversaLength;

        public Traversr(string humanPath)
        {
            string intermediatePath = humanPath.Replace("[", ".[");
            // given this replace and split strategy, we can end up with double dots, "..", which will generate an empty path element.
            // so remove any ".."  ;)
            intermediatePath = intermediatePath.Replace("..", ".");

            if (intermediatePath[0] == '.')
            {
                // if the path started with an array, aka "[0].tuna", remove the leading .
                intermediatePath = intermediatePath.Substring(1);
            }

            string[] paths = intermediatePath.Split('.');

            ITraversalStep rooty = null;
            for (int index = paths.Length - 1; index >= 0; index--)
            {
                rooty = MakePathElement(paths[index], rooty);
            }
            _traversaLength = paths.Length;
            _root = rooty;
        }

        /**
         * Constructor where we provide a known good set of pathElement Strings in a list.
         * Aka, no need to extract it from a "Human Readable" form.
         */
        public Traversr(List<string> paths)
        {
            ITraversalStep rooty = null;
            for (int index = paths.Count - 1; index >= 0; index--)
            {
                rooty = MakePathElement(paths[index], rooty);
            }
            _traversaLength = paths.Count;
            _root = rooty;
        }

        private ITraversalStep MakePathElement(string path, ITraversalStep child)
        {
            if ("[]" == path)
            {
                return new AutoExpandArrayTraversalStep(this, child);
            }
            else if (path.StartsWith("[") && path.EndsWith("]"))
            {
                return new ArrayTraversalStep(this, child);
            }
            else
            {
                return new MapTraversalStep(this, child);
            }
        }

        /**
         * Note : Calling this method MAY modify the tree object by adding new Maps and Lists as needed
         *  for the traversal.  This is determined by the behavior of the implementations of the
         *  abstract methods of this class.
         */
        public JToken Get(JToken tree, List<string> keys)
        {
            if (keys.Count != _traversaLength)
            {
                throw new TraversrException("Traversal Path and number of keys mismatch, _traversaLength:" + _traversaLength + " numKeys:" + keys.Count);
            }

            return _root.Traverse(tree, TraversalStepOperation.GET, keys.GetEnumerator(), null);
        }

        /**
         * @param tree tree of Map and List JSON structure to navigate
         * @param data JSON style data object you want to set
         * @return returns the data object if successfully set, otherwise null if there was a problem walking the path
         */
        public JToken Set(JToken tree, List<string> keys, JToken data)
        {
            if (keys.Count != _traversaLength)
            {
                throw new TraversrException("Traversal Path and number of keys mismatch, _traversaLength:" + _traversaLength + " numKeys:" + keys.Count);
            }

            /*
               This may seem counterintuitive.
               Aka, 'set' is going to create maps and lists as it walks, so why not take null input,
                and make the top level map or list as needed'
               The problem is that, we have no way to return our newly created top level container.
               All we return is a reference to the data, if we were successful in our set.
            */
            if (tree == null)
            {
                return null;
            }

            return _root.Traverse(tree, TraversalStepOperation.SET, keys.GetEnumerator(), data);
        }

        /**
         * Note : Calling this method MAY modify the tree object by adding new Maps and Lists as needed
         *  for the traversal.  This is determined by the behavior of the implementations of the
         *  abstract methods of this class.
         */
        public JToken Remove(JToken tree, List<string> keys)
        {
            if (keys.Count != _traversaLength)
            {
                throw new TraversrException("Traversal Path and number of keys mismatch, _traversaLength:" + _traversaLength + " numKeys:" + keys.Count);
            }

            if (tree == null)
            {
                return null;
            }

            return _root.Traverse(tree, TraversalStepOperation.REMOVE, keys.GetEnumerator(), null);
        }

        // TODO extract these methods to an interface, and then sublasses of Traverser like ShiftrTraversr can do the
        //  Swing style "I implement the interface and pass myself down" trick.
        //  Means we can still can have a ShiftrTraversr, but less of a an explicit dependency inversion going
        //   on between the Traversr and its Traversals.
        /**
         * Allow subclasses to control how "sets" are done, if/once the traversal has made it to the the last element.
         *
         * Overwrite existing data?   List-ize existing data with new data?
         *
         * @return the data object if the set was successful, or null if not
         */
        public abstract JToken HandleFinalSet(ITraversalStep traversalStep, JToken tree, string key, JToken data);

        /**
         * Allow subclasses to control how gets are handled for intermediate traversals.
         *
         * Example: we are a MapTraversal and out key is "foo".
         *   We simply  do a 'tree.get( "foo" )'.  However, if we get a null back, or we get back
         *   a data type incompatible with our child Traversal, what do we do?
         *
         * Overwrite or just return?
         */
        public abstract JToken HandleIntermediateGet(ITraversalStep traversalStep, JToken tree, string key, TraversalStepOperation op);
    }
}
