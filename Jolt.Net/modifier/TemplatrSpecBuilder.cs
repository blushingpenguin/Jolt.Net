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

using Jolt.Net.Functions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Jolt.Net
{

    public class TemplatrSpecBuilder : SpecBuilder<ModifierSpec>
    {
        public const string CARET = "^";
        public const string AT = "@";
        public const string FUNCTION = "=";

        private readonly OpMode _opMode;
        private readonly IReadOnlyDictionary<string, IFunction> _functionsMap;

        public TemplatrSpecBuilder(OpMode opMode, IReadOnlyDictionary<string, IFunction> functionsMap)
        {
            _opMode = opMode;
            _functionsMap = functionsMap;
        }

        public override ModifierSpec CreateSpec(string lhs, JToken rhs)
        {
            if (rhs is JObject dic && dic.Count > 0)
            {
                return new ModifierCompositeSpec(lhs, dic, _opMode, this);
            }
            else
            {
                return new ModifierLeafSpec(lhs, rhs, _opMode, _functionsMap);
            }
        }
    }
}
