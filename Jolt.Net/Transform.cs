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

namespace Jolt.Net
{
    public interface ITransform : IJoltTransform
    {

        /**
         * Execute a transform on some input JSON with optionally provided "context" and return the result.
         *
         * @param input the JSON object to transform in plain vanilla Jackson Map<string, object> style
         * @return the results of the transformation
         * @throws com.bazaarvoice.jolt.exception.TransformException if there are issues with the transform
         */
        JObject Transform(JObject input);
    }
}
