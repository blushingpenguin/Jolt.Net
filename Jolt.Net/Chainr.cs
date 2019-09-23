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
using System.Linq;

namespace Jolt.Net
{

    /**
     * Chainr is the JOLT mechanism for chaining {@link JoltTransform}s together. Any of the built-in JOLT
     * transform types can be called directly from Chainr. Any custom-written Java transforms
     * can be adapted in by implementing the {@link Transform} or {@link SpecDriven} interfaces.
     *
     * A Chainr spec should be an array of objects in order that look like this:
     *
     * [
     *     {
     *         "operation": "[operation-name]",
     *         // stuff that the specific transform needs go here
     *     },
     *     ...
     * ]
     *
     * Each operation is called in the order that it is specified within the array. The original
     * input to Chainr is passed into the first operation, with its output passed into the next,
     * and so on. The output of the final operation is returned from Chainr.
     *
     * Currently, [operation-name] can be any of the following:
     *
     * - shift: ({@link Shiftr}) a tool for moving parts of an input JSON document to a new output document
     * - default: ({@link Defaultr}) a tool for applying default values to the provided JSON document
     * - remove: ({@link Removr}) a tool for removing specific values from the provided JSON document
     * - sort: ({@link Sortr}) sort the JSON document
     * - java: passes control to whatever Java class you specify as long as it implements the {@link Transform} interface
     *
     * Shift, default, and remove operation all require a "spec", while sort does not.
     *
     * [
     *     {
     *         "operation": "shift",
     *         "spec" : { // shiftr spec }
     *     },
     *     {
     *         "operation": "sort"  // sort does not need a spec
     *     },
     *     ...
     * ]
     *
     * Custom Java classes that implement {@link Transform} and/or {@link SpecDriven} can be loaded by specifying the full
     *  className to load. Additionally, if upon reflection of the class we see that it is an instance of a
     *  {@link SpecDriven}, then we will construct it with a the supplied "spec" object.
     *
     * [
     *     {
     *         "operation": "com.bazaarvoice.tuna.CustomTransform",
     *
     *         "spec" : { ... } // optional spec to use to construct a custom {@link Transform} if it has the {@link SpecDriven} marker interface.
     *     },
     *     ...
     * ]
     */
    public class Chainr : ITransform, IContextualTransform
    {

        // The list of Transforms we will march through on every call to chainr.
        // Note this will contain actual ContextualTransforms and adapted Transforms.
        private readonly List<IContextualTransform> _transformsList;

        // The list of actual ContextualTransforms, for clients that specifically care.
        private readonly IReadOnlyList<IContextualTransform> _actualContextualTransforms;

        public static Chainr FromSpec(JToken input)
        {
            return new ChainrBuilder(input).Build();
        }

        public static Chainr FromSpec(JToken input, IChainrInstantiator instantiator)
        {
            return new ChainrBuilder(input).Loader(instantiator).Build();
        }

        /**
         * Adapt "normal" Transforms to look like ContextualTransforms, so that
         *  Chainr can just maintain a single list of "JoltTransforms" to run.
         */
        private class ContextualTransformAdapter : IContextualTransform
        {

            private readonly ITransform _transform;

            public ContextualTransformAdapter(ITransform transform)
            {
                _transform = transform;
            }

            public JObject Transform(JObject input, Dictionary<string, object> context)
            {
                return _transform.Transform(input);
            }
        }

        public Chainr(List<IJoltTransform> joltTransforms)
        {
            if (joltTransforms == null)
            {
                throw new ArgumentNullException(nameof(joltTransforms), "Chainr requires a list of JoltTransforms.");
            }

            _transformsList = new List<IContextualTransform>(joltTransforms.Count);
            var realContextualTransforms = new List<IContextualTransform>();

            foreach (var joltTransform in joltTransforms)
            {
                // Do one pass of "instanceof" checks at construction time, rather than repeatedly at "runtime".
                bool isTransform = joltTransform is ITransform;
                bool isContextual = joltTransform is IContextualTransform;

                if (isContextual && isTransform)
                {
                    throw new SpecException("JOLT Chainr - JoltTransform className:" + joltTransform.GetType().Name +
                            " implements both Transform and ContextualTransform, should only implement one of those interfaces.");
                }
                if (!isContextual && !isTransform)
                {
                    throw new SpecException("JOLT Chainr - Transform className:" + joltTransform.GetType().Name +
                            " should implement Transform or ContextualTransform.");
                }

                // We are optimizing given the assumption that Chainr objects will be built and then reused many times.
                // We want to have a single list of "transforms" that we can just blindly march through.
                // In order to accomplish this, we adapt Transforms to look like ContextualTransforms and just maintain
                //  a list of type ContextualTransform.
                if (isContextual)
                {
                    _transformsList.Add((IContextualTransform)joltTransform);
                    realContextualTransforms.Add((IContextualTransform)joltTransform);
                }
                else
                {
                    _transformsList.Add(new ContextualTransformAdapter((ITransform)joltTransform));
                }
            }

            _actualContextualTransforms = realContextualTransforms.AsReadOnly();
        }

        /**
         * Runs a series of Transforms on the input, piping the inputs and outputs of the Transforms together.
         *
         * Chainr instances are meant to be immutable once they are created so that they can be
         * used many times.
         *
         * The notion of passing "context" to the transforms allows chainr instances to be
         * reused, even in situations were you need to slightly vary.
         *
         * @param input a JSON (Jackson-parsed) maps-of-maps object to transform
         * @param context optional tweaks that the consumer of the transform would like
         * @return an object representing the JSON resulting from the transform
         * @throws com.bazaarvoice.jolt.exception.TransformException if the specification is malformed, an operation is not
         *                       found, or if one of the specified transforms throws an exception.
         */
        public JObject Transform(JObject input, Dictionary<string, object> context)
        {
            return DoTransform(_transformsList, input, context);
        }

        public JObject Transform(JObject input)
        {
            return DoTransform(_transformsList, input, null);
        }

        /**
         * Have Chainr run a subset of the transforms in it's spec.
         *
         * Useful for testing and debugging.
         *
         * @param input the input data to transform
         * @param to transform from the chainrSpec to end with: 0 based index exclusive
         */
        public JObject Transform(int to, JObject input)
        {
            return Transform(0, to, input, null);
        }

        /**
         * Useful for testing and debugging.
         *
         * @param input the input data to transform
         * @param to transform from the chainrSpec to end with: 0 based index exclusive
         * @param context optional tweaks that the consumer of the transform would like
         */
        public JObject Transform(int to, JObject input, Dictionary<string, object> context)
        {
            return Transform(0, to, input, context);
        }

        /**
         * Useful for testing and debugging.
         *
         * @param input the input data to transform
         * @param from transform from the chainrSpec to start with: 0 based index
         * @param to transform from the chainrSpec to end with: 0 based index exclusive
         */
        public JObject Transform(int from, int to, JObject input)
        {
            return Transform(from, to, input, null);
        }

        /**
         * Have Chainr run a subset of the transforms in it's spec.
         *
         * Useful for testing and debugging.
         *
         * @param input the input data to transform
         * @param from transform from the chainrSpec to start with: 0 based index
         * @param to transform from the chainrSpec to end with: 0 based index exclusive
         * @param context optional tweaks that the consumer of the transform would like
         */
        public JObject Transform(int from, int to, JObject input, Dictionary<string, object> context)
        {
            if (from < 0 || to > _transformsList.Count || to <= from)
            {
                throw new TransformException("JOLT Chainr : invalid from and to parameters : from=" + from + " to=" + to);
            }

            return DoTransform(_transformsList.Skip(from).Take(to - from).ToList(), input, context);
        }

        private static JObject DoTransform(List<IContextualTransform> transforms, JObject input, Dictionary<string, object> context)
        {
            JObject intermediate = input;
            foreach (IContextualTransform transform in transforms)
            {
                intermediate = transform.Transform(intermediate, context);
            }

            return intermediate;
        }

        /**
         * @return true if this Chainr instance has any ContextualTransforms
         */
        public bool HasContextualTransforms()
        {
            return _actualContextualTransforms.Count > 0;
        }

        /**
         * This method allows Chainr clients to examine the ContextualTransforms
         * in this Chainr instance.  This may be helpful when building the "context".
         *
         * @return List of ContextualTransforms used by this Chainr instance
         */
        public IReadOnlyList<IContextualTransform> GetContextualTransforms() {
            return _actualContextualTransforms;
        }
    }
}
