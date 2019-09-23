/*
 * Copyright 2014 Bazaarvoice, Inc.
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
using System.Text;

namespace Jolt.Net
{

    /**
     * Combines a Traversr with the ability to evaluate References against a WalkedPath.
     *
     * Convenience class for path based off a single dot notation string,
     *  like "rating.&1(2).&.value".
     *
     * This processes the dot notation path into internal data structures, so
     *  that the string processing only happens once.
     */
    public abstract class PathEvaluatingTraversal
    {
        private readonly IReadOnlyList<IEvaluatablePathElement> _elements;
        private readonly Traversr _traversr;

        public PathEvaluatingTraversal(string dotNotation)
        {
            if ((dotNotation.Contains("*") && !dotNotation.Contains("\\*")) ||
                 (dotNotation.Contains("$") && !dotNotation.Contains("\\$"))) {
                throw new SpecException("DotNotation (write key) can not contain '*' or '$' : write key: " + dotNotation);
            }

            List<IPathElement> paths;
            Traversr trav;

            if (!String.IsNullOrWhiteSpace(dotNotation))
            {
                // Compute the path elements.
                paths = PathElementBuilder.ParseDotNotationRHS(dotNotation);

                // Use the canonical versions of the path elements to create the Traversr
                var traversrPaths = new List<string>(paths.Count);
                foreach (IPathElement pe in paths)
                {
                    traversrPaths.Add(pe.GetCanonicalForm());
                }
                trav = CreateTraversr(traversrPaths);
            }
            else
            {
                paths = new List<IPathElement>();
                trav = CreateTraversr(new List<string>(new[] { "" }));
            }

            var evalPaths = new List<IEvaluatablePathElement>(paths.Count);
            foreach (IPathElement pe in paths)
            {
                if (!(pe is IEvaluatablePathElement epe))
                {
                    throw new SpecException("RHS key=" + pe.RawKey + " is not a valid RHS key.");
                }

                evalPaths.Add(epe);
            }

            _elements = evalPaths.AsReadOnly();
            _traversr = trav;
        }

        protected abstract Traversr CreateTraversr(List<string> paths);

        /**
         * Use the supplied WalkedPath, in the evaluation of each of our PathElements to
         *  build a concrete output path.  Then use that output path to write the given
         *  data to the output.
         *
         * @param data data to write
         * @param output data structure we are going to write the data to
         * @param walkedPath reference used to lookup reference values like "&1(2)"
         */
        public void Write(object data, Dictionary<string, object> output, WalkedPath walkedPath)
        {
            var evaledPaths = Evaluate(walkedPath);
            if (evaledPaths != null)
            {
                _traversr.Set(output, evaledPaths, data);
            }
        }

        public OptionalObject Read(object data, WalkedPath walkedPath)
        {
            var evaledPaths = Evaluate(walkedPath);
            if (evaledPaths == null)
            {
                return new OptionalObject();
            }

            return _traversr.Get(data, evaledPaths);
        }

        /**
         * Use the supplied WalkedPath, in the evaluation of each of our PathElements.
         *
         * If our PathElements contained a TransposePathElement, we may return null.
         *
         * @param walkedPath used to lookup/evaluate PathElement references values like "&1(2)"
         * @return null or fully evaluated Strings, possibly with concrete array references like "photos.[3]"
         */
        // Visible for testing
        public List<string> Evaluate(WalkedPath walkedPath)
        {
            var strings = new List<string>(_elements.Count);
            foreach (IEvaluatablePathElement pathElement in _elements)
            {
                string evaledLeafOutput = pathElement.Evaluate(walkedPath);
                if (evaledLeafOutput == null)
                {
                    // If this output path contains a TransposePathElement, and when evaluated,
                    //  return null, then bail
                    return null;
                }
                strings.Add(evaledLeafOutput);
            }

            return strings;
        }

        public int Size() => _elements.Count;

        public IPathElement Get(int index) =>
            _elements[index];

        /**
         * Testing method.
         */
        public string GetCanonicalForm()
        {
            var buf = new StringBuilder();

            foreach (IPathElement pe in _elements)
            {
                buf.Append(".").Append(pe.GetCanonicalForm());
            }

            return buf.ToString(1, buf.Length - 1); // strip the leading "."
        }
    }
}
