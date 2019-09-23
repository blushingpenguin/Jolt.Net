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

    /**
     * Composite spec is non-leaf level spec that contains one or many child specs and processes
     * them based on a pre-determined execution strategy
     */
    public class ModifierCompositeSpec : ModifierSpec, IOrderedCompositeSpec
    {
        private static readonly Dictionary<Type, int> _orderMap = new Dictionary<Type, int>
        {
            { typeof(ArrayPathElement), 1 },
            { typeof(StarRegexPathElement), 2 },
            { typeof(StarDoublePathElement), 3 },
            { typeof(StarSinglePathElement), 4 },
            { typeof(StarAllPathElement), 5 }
        };
        private static readonly ComputedKeysComparator _computedKeysComparator =
            ComputedKeysComparator.FromOrder(_orderMap);

        private readonly IReadOnlyDictionary<string, IBaseSpec> _literalChildren;
        private readonly IReadOnlyList<ModifierSpec> _computedChildren;
        private readonly ExecutionStrategy _executionStrategy;
        private readonly DataType _specDataType;

        public ModifierCompositeSpec(string key, JObject spec, OpMode opMode, TemplatrSpecBuilder specBuilder) :
            base(key, opMode)
        {

            var literals = new Dictionary<string, IBaseSpec>();
            var computed = new List<ModifierSpec>();

            List<ModifierSpec> children = specBuilder.CreateSpec(spec);

            // remember max explicit index from spec to expand input array at runtime
            // need to validate spec such that it does not specify both array and literal path element
            int maxExplicitIndexFromSpec = -1, confirmedMapAtIndex = -1, confirmedArrayAtIndex = -1;

            for (int i = 0; i < children.Count; i++)
            {
                var childSpec = children[i];
                var childPathElement = childSpec.GetPathElement();

                // for every child,
                //  a) mark current index as either must be map or must be array
                //  b) mark it as literal or computed
                //  c) if arrayPathElement,
                //      - make sure its an explicit index type
                //      - save the max explicit index in spec
                if (childPathElement is LiteralPathElement)
                {
                    confirmedMapAtIndex = i;
                    literals[childPathElement.RawKey] = childSpec;
                }
                else if (childPathElement is ArrayPathElement childArrayPathElement)
                {
                    confirmedArrayAtIndex = i;

                    if (!childArrayPathElement.IsExplicitArrayIndex())
                    {
                        throw new SpecException(opMode.GetName() + " RHS only supports explicit Array path element");
                    }
                    int? explicitIndex = childArrayPathElement.GetExplicitArrayIndex();
                    // if explicit index from spec also enforces "[...]?" don't bother using that as max index
                    if (!childSpec.GetCheckValue())
                    {
                        maxExplicitIndexFromSpec = Math.Max(maxExplicitIndexFromSpec, explicitIndex ?? 0);
                    }

                    literals[explicitIndex.ToString()] = childSpec;
                }
                else
                {
                    // StarPathElements evaluates to string keys in a Map, EXCEPT StarAllPathElement
                    // which can be both all keys in a map or all indexes in a list
                    if (!(childPathElement is StarAllPathElement))
                    {
                        confirmedMapAtIndex = i;
                    }
                    computed.Add(childSpec);
                }

                // Bail as soon as both confirmedMapAtIndex & confirmedArrayAtIndex is set
                if (confirmedMapAtIndex > -1 && confirmedArrayAtIndex > -1)
                {
                    throw new SpecException(opMode.GetName() + " RHS cannot mix int array index and string map key, defined spec for " + key + " contains: " +
                        children[confirmedMapAtIndex].GetPathElement().GetCanonicalForm() + " conflicting " +
                        children[confirmedArrayAtIndex].GetPathElement().GetCanonicalForm());
                }
            }

            // set the dataType from calculated indexes
            _specDataType = DataType.DetermineDataType(confirmedArrayAtIndex, confirmedMapAtIndex, maxExplicitIndexFromSpec);

            // Only the computed children need to be sorted
            computed.Sort(_computedKeysComparator);
            computed.TrimExcess();

            _literalChildren = literals;
            _computedChildren = computed.AsReadOnly();

            // extract generic execution strategy
            _executionStrategy = DetermineExecutionStrategy();

        }

        protected override void ApplyElement(string inputKey, OptionalObject inputOptional, MatchedElement thisLevel, WalkedPath walkedPath, Dictionary<string, object> context)
        {
            object input = inputOptional.Value;
            // sanity checks, cannot work on a list spec with map input and vice versa, and runtime with null input
            if (!_specDataType.IsCompatible(input))
            {
                return;
            }

            // create input if it is null
            if (input == null)
            {
                input = _specDataType.Create(inputKey, walkedPath, _opMode);
                // if input has changed, wrap
                if (input != null)
                {
                    inputOptional = new OptionalObject(input);
                }
            }

            // if input is List, create special ArrayMatchedElement, which tracks the original size of the input array
            if (input is List<object> list)
            {
                // LIST means spec had array index explicitly specified, hence expand if needed
                if (_specDataType is LIST specList)
                {
                    int? origSize = specList.Expand(input);
                    thisLevel = new ArrayMatchedElement(thisLevel.RawKey, origSize ?? 0);
                }
                else
                {
                    // specDataType is RUNTIME, so spec had no array index explicitly specified, no need to expand
                    thisLevel = new ArrayMatchedElement(thisLevel.RawKey, list.Count);
                }
            }

            // add self to walked path
            walkedPath.Add(input, thisLevel);
            // Handle the rest of the children
            _executionStrategy.Process(this, inputOptional, walkedPath, null, context);
            // We are done, so remove ourselves from the walkedPath
            walkedPath.RemoveLast();
        }

        public IReadOnlyDictionary<string, IBaseSpec> GetLiteralChildren() =>
            _literalChildren;

        public IReadOnlyList<IBaseSpec> GetComputedChildren() =>
            _computedChildren;

        public ExecutionStrategy DetermineExecutionStrategy()
        {

            if (_computedChildren.Count == 0)
            {
                return ExecutionStrategy.AllLiterals;
            }
            else if (_literalChildren.Count == 0)
            {
                return ExecutionStrategy.Computed;
            }
            else if (_opMode == OpMode.DEFINER && _specDataType is LIST)
            {
                return ExecutionStrategy.Conflict;
            }
            else
            {
                return ExecutionStrategy.AllLiteralsWithComputed;
            }
        }
    }
}
