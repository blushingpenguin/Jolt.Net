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

namespace Jolt.Net.Test
{
    public class GoodSpecAndContextDrivenTransform : SpecDriven, IContextualTransform 
    {
        public const string CONTEXT_KEY = "test_context_key_2";

        private const string SPEC_DRIVEN_KEY = "KEY_TO_ADD";

        private readonly string _specKeyValue;

        public GoodSpecAndContextDrivenTransform(JToken spec)
        {
            _specKeyValue = spec[SPEC_DRIVEN_KEY].ToString();
        }

        public JToken Transform(JToken input, JObject context)
        {
            input[_specKeyValue] = context[CONTEXT_KEY];
            return input;
        }
    }
}
