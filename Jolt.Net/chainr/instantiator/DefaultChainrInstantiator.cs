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

namespace Jolt.Net
{
    /**
     * Loads classes via Java Reflection APIs.
     */
    public class DefaultChainrInstantiator : IChainrInstantiator
    {
        public IJoltTransform HydrateTransform(ChainrEntry entry)
        {
            object spec = entry.GetSpec();
            Type transformType = entry.GetJoltTransformType();

            try
            {
                // If the transform class is a SpecTransform, we try to construct it with the provided spec.
                if (entry.IsSpecDriven())
                {

                    // Lookup a Constructor with a Single "object" arg.
                    var constructor = transformType.GetConstructor(new[] { typeof(object) });
                    if (constructor == null)
                    {
                        // This means the transform class "violated" the SpecTransform marker interface
                        throw new SpecException("JOLT Chainr encountered an constructing SpecTransform className:" + transformType.Name +
                                ".  Specifically, no single arg constructor found" + entry.GetErrorMessageIndexSuffix());
                    }

                    return (IJoltTransform)constructor.Invoke(new object[] { spec });
                }
                else
                {
                    // The opClass is just a Transform, so just create a newInstance of it.
                    var constructor = transformType.GetConstructor(new Type[0]);
                    if (constructor == null)
                    {
                        throw new Exception("JOLT Chainr encountered an error constructing className:" + transformType.Name +
                            ".  Specifically, a no arg constructor was not found" + entry.GetErrorMessageIndexSuffix());
                    }
                    return (IJoltTransform)constructor.Invoke(new object[0]);
                }
            }
            catch (Exception e)
            {
                // FYI 3 exceptions are known to be thrown here
                // IllegalAccessException, InvocationTargetException, InstantiationException
                throw new SpecException("JOLT Chainr encountered an exception constructing Transform className:"
                        + transformType.Name + entry.GetErrorMessageIndexSuffix(), e);
            }
        }
    }
}

