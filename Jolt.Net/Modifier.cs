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

using System.Collections.Generic;
using Jolt.Net.Functions;
using Jolt.Net.Functions.Lists;
using Jolt.Net.Functions.Math;
using Jolt.Net.Functions.Objects;
using Jolt.Net.Functions.Strings;
using Newtonsoft.Json.Linq;

namespace Jolt.Net
{
    /**
     * Base Templatr transform that to behave differently based on provided opMode
     */
    public abstract class Modifier : SpecDriven, IContextualTransform
    {
        public static readonly IReadOnlyDictionary<string, IFunction> STOCK_FUNCTIONS = new Dictionary<string, IFunction>
        {
            { "toLower", new ToLowerCase() },
            { "toUpper", new ToUpperCase() },
            { "concat", new Concat() },
            { "join", new Join() },
            { "split", new Split() },
            { "substring", new Substring() },
            { "trim", new Trim() },
            { "leftPad", new LeftPad() },
            { "rightPad", new RightPad() },

            { "min", new Min() },
            { "max", new Max() },
            { "abs", new Abs() },
            { "avg", new Avg() },
            { "intSum", new IntSum() },
            { "doubleSum", new DoubleSum() },
            { "longSum", new LongSum() },
            { "intSubtract", new IntSubtract() },
            { "doubleSubtract", new DoubleSubtract() },
            { "longSubtract", new LongSubtract() },
            { "divide", new Divide() },
            { "divideAndRound", new DivideAndRound() },

            { "toInteger", new ToInteger() },
            { "toDouble", new ToDouble() },
            { "toLong", new ToLong() },
            { "toBoolean", new ToBoolean() },
            { "toString", new ToString() },
            { "size", new Size() },
            
            { "squashNulls", new SquashNulls() },
            { "recursivelySquashNulls", new RecursivelySquashNulls() },

            { "noop", new Noop() },
            { "isPresent", new IsPresent() },
            { "notNull", new NotNull() },
            { "isNull", new IsNull() },

            { "firstElement", new FirstElement() },
            { "lastElement", new LastElement() },
            { "elementAt", new ElementAt() },
            { "toList", new ToList() },
            { "sort", new Sort() }
        };

        private readonly ModifierCompositeSpec _rootSpec;

        private Modifier(JObject spec, OpMode opMode, IReadOnlyDictionary<string, IFunction> functionsMap)
        {
            if (spec == null)
            {
                throw new SpecException(opMode.GetName() + " expected a spec of Map type, got 'null'.");
            }

            if (functionsMap == null || functionsMap.Count == 0)
            {
                throw new SpecException(opMode.GetName() + " expected a populated functions' map type, got " + (functionsMap == null ? "null" : "empty"));
            }

            TemplatrSpecBuilder templatrSpecBuilder = new TemplatrSpecBuilder(opMode, functionsMap);
            _rootSpec = new ModifierCompositeSpec(ROOT_KEY, spec, opMode, templatrSpecBuilder);
        }

        public JToken Transform(JToken input, JObject context)
        {
            var contextWrapper = new JObject();
            contextWrapper.Add(ROOT_KEY, context);

            MatchedElement rootLpe = new MatchedElement(ROOT_KEY);
            WalkedPath walkedPath = new WalkedPath();
            walkedPath.Add(input, rootLpe);

            _rootSpec.Apply(ROOT_KEY, input, walkedPath, null, contextWrapper);
            return contextWrapper[ROOT_KEY];
        }

        /**
         * This variant of modifier creates the key/index is missing,
         * and overwrites the value if present
         */
        public class Overwritr : Modifier
        {
            public Overwritr(JObject spec) :
                this(spec, Modifier.STOCK_FUNCTIONS)
            {

            }

            public Overwritr(JObject spec, IReadOnlyDictionary<string, IFunction> functionsMap) :
                base(spec, OpMode.OVERWRITR, functionsMap)
            {
            }
        }

        /**
         * This variant of modifier only writes when the key/index is missing
         */
        public class Definr : Modifier
        {
            public Definr(JObject spec) :
                this(spec, Modifier.STOCK_FUNCTIONS)
            {

            }

            public Definr(JObject spec, IReadOnlyDictionary<string, IFunction> functionsMap) :
                base(spec, OpMode.DEFINER, functionsMap)
            {
            }
        }

        /**
         * This variant of modifier only writes when the key/index is missing or the value is null
         */
        public class Defaultr : Modifier
        {
            public Defaultr(JObject spec) :
                this(spec, Modifier.STOCK_FUNCTIONS)
            {

            }

            public Defaultr(JObject spec, IReadOnlyDictionary<string, IFunction> functionsMap) :
                base(spec, OpMode.DEFAULTR, functionsMap)
            {
            }
        }
    }
}
