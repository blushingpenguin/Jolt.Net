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
    public class ChainrBuilder
    {
        private readonly JToken _chainrSpecObj;
        protected IChainrInstantiator _chainrInstantiator = new DefaultChainrInstantiator();

        /**
         * Initialize a Chainr to run a list of Transforms.
         * This is the constructor most "production" usages of Chainr should use.
         *
         * @param chainrSpecObj List of transforms to run
         */
        public ChainrBuilder(JToken chainrSpecObj)
        {
            _chainrSpecObj = chainrSpecObj;
        }

        /**
         * Set a ChainrInstantiator to use when instantiating Transform Objects.
         * If one is not set, defaults to DefaultChainrInstantiator;
         *
         * @param loader ChainrInstantiator to use load Transforms
         */
        public ChainrBuilder Loader(IChainrInstantiator loader)
        {
            _chainrInstantiator = loader ?? throw new ArgumentNullException(nameof(loader), "ChainrBuilder requires a non-null loader.");
            return this;
        }

        // public ChainrBuilder WithClassLoader(ClassLoader classLoader) {
        //     if (classLoader == null) {
        //         throw new IllegalArgumentException("ChainrBuilder requires a non-null classLoader.");
        //     }
        //     this.classLoader = classLoader;
        //     return this;
        // }

        public Chainr Build()
        {
            ChainrSpec chainrSpec = new ChainrSpec(_chainrSpecObj);
            var transforms = new List<IJoltTransform>(chainrSpec.GetChainrEntries().Count);
            foreach (ChainrEntry entry in chainrSpec.GetChainrEntries()) 
            {
                IJoltTransform transform = _chainrInstantiator.HydrateTransform(entry);
                transforms.Add(transform);
            }

            return new Chainr(transforms);
        }
    }
}
