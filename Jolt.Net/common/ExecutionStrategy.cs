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
using System.Collections.Generic;

namespace Jolt.Net
{
    public abstract class ExecutionStrategy
    {
        public static ExecutionStrategy Computed { get; } =
            new ComputedExecutionStrategy();

        public static ExecutionStrategy Conflict { get; } =
            new ConflictExecutionStrategy();

        public static ExecutionStrategy AvailableLiterals { get; } =
            new AvailableLiteralsExecutionStrategy();

        public static ExecutionStrategy AvailableLiteralsWithComputed { get; } =
            new AvailableLiteralsWithComputedExecutionStrategy();

        public static ExecutionStrategy AllLiterals { get; } =
            new AllLiteralsExecutionStategy();

        public static ExecutionStrategy AllLiteralsWithComputed { get; } =
            new AllLiteralsWithComputedExecutionStrategy();


        public abstract void ProcessMap(IOrderedCompositeSpec spec, Dictionary<string, object> inputMap, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context);
        public abstract void ProcessList(IOrderedCompositeSpec spec, List<object> inputList, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context);
        public abstract void ProcessScalar(IOrderedCompositeSpec spec, string scalarInput, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context);

        public void Process(IOrderedCompositeSpec spec, OptionalObject inputOptional, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context)
        {
            object input = inputOptional.Value;
            if (input is Dictionary<string, object> map)
            {
                ProcessMap(spec, map, walkedPath, output, context);
            }
            else if (input is List<object> list)
            {
                ProcessList(spec, list, walkedPath, output, context);
            }
            else if (input != null)
            {
                // if not a map or list, must be a scalar
                ProcessScalar(spec, input.ToString(), walkedPath, output, context);
            }
        }

        /**
         * This is the method we are trying to avoid calling.  It implements the matching behavior
         *  when we have both literal and computed children.
         *
         * For each input key, we see if it matches a literal, and it not, try to match the key with every computed child.
         *
         * Worse case : n + n * c, where
         *   n is number of input keys
         *   c is number of computed children
         */
        protected static void ApplyKeyToLiteralAndComputed<T>(T spec, string subKeyStr, OptionalObject subInputOptional, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context)
            where T : IOrderedCompositeSpec
        {
            // if the subKeyStr found a literalChild, then we do not have to try to match any of the computed ones
            if (spec.GetLiteralChildren().TryGetValue(subKeyStr, out var literalChild))
            {
                literalChild.Apply(subKeyStr, subInputOptional, walkedPath, output, context);
            }
            else
            {
                // If no literal spec key matched, iterate through all the getComputedChildren()
                ApplyKeyToComputed(spec.GetComputedChildren(), walkedPath, output, subKeyStr, subInputOptional, context);
            }
        }

        protected static void ApplyKeyToComputed<T>(IReadOnlyList<T> computedChildren, WalkedPath walkedPath, Dictionary<string, object> output, string subKeyStr, OptionalObject subInputOptional, Dictionary<string, object> context)
            where T : IBaseSpec
        {
            // Iterate through all the getComputedChildren() until we find a match
            // This relies upon the getComputedChildren() having already been sorted in priority order
            foreach (IBaseSpec computedChild in computedChildren)
            {
                // if the computed key does not match it will quickly return false
                if (computedChild.Apply(subKeyStr, subInputOptional, walkedPath, output, context))
                {
                    break;
                }
            }
        }
    }

    public class AvailableLiteralsExecutionStrategy : ExecutionStrategy
    {
        /**
         * The performance assumption built into this code is that the literal values in the spec, are generally smaller
         *  than the number of potential keys to check in the input.
         *
         *  More specifically, the assumption here is that the set of literalChildren is smaller than the input "keyset".
         */
        public override void ProcessMap(IOrderedCompositeSpec spec, Dictionary<string, object> inputMap, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context)
        {
            foreach (var kv in spec.GetLiteralChildren())
            {
                // Do not work if the value is missing in the input map
                if (inputMap.TryGetValue(kv.Key, out var inputValue))
                {
                    var subInputOptional = new OptionalObject(kv.Value);
                    kv.Value.Apply(kv.Key, subInputOptional, walkedPath, output, context );
                }
            }
        }

        public override void ProcessList(IOrderedCompositeSpec spec, List<object> inputList, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context)
        {
            int? originalSize = walkedPath.LastElement().OrigSize;
            foreach (var kv in spec.GetLiteralChildren())
            {
                // If the data is an Array, but the spec keys are Non-Integer Strings,
                //  we are annoyed, but we don't stop the whole transform.
                // Just this part of the Transform won't work.
                if (Int32.TryParse(kv.Key, out int keyInt) &&
                    // Do not work if the index is outside of the input list
                    keyInt < inputList.Count)
                {
                    object subInput = inputList[keyInt];
                    OptionalObject subInputOptional;
                    if (subInput == null && originalSize.HasValue && keyInt >= originalSize.Value) {
                        subInputOptional = new OptionalObject();
                    }
                    else {
                        subInputOptional = new OptionalObject();
                    }

                    kv.Value.Apply(kv.Key, subInputOptional, walkedPath, output, context);
                }
            }
        }

        public override void ProcessScalar(IOrderedCompositeSpec spec, string scalarInput, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context )
        {
            if (spec.GetLiteralChildren().TryGetValue(scalarInput, out var literalChild))
            {
                literalChild.Apply(scalarInput, new OptionalObject(), walkedPath, output, context);
            }
        }
    }

    /**
     * This is identical to AVAILABLE_LITERALS, except for the fact that it does not skip keys if its missing in the input, like literal does
     * Given this works like defaultr, a missing key is our point of entry to insert a default value, either from a passed context or a
     * hardcoded value.
     */
    public class AllLiteralsExecutionStategy : AvailableLiteralsExecutionStrategy
    {
        public override void ProcessMap(IOrderedCompositeSpec spec, Dictionary<string, object> inputMap, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context )
        {
            foreach (var kv in spec.GetLiteralChildren())
            {
                // if the input in not available in the map us null or else get value,
                // then lookup and place a defined value from spec there
                var subInputOptional = new OptionalObject();
                if (inputMap.TryGetValue(kv.Key, out var input))
                {
                    subInputOptional = new OptionalObject(input);
                }
                kv.Value.Apply(kv.Key, subInputOptional, walkedPath, output, context );
            }
        }

        public override void ProcessList(IOrderedCompositeSpec spec, List<object> inputList, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context )
        {
            int? originalSize = walkedPath.LastElement().OrigSize;
            foreach (var kv in spec.GetLiteralChildren())
            {
                var subInputOptional = new OptionalObject();
                // If the data is an Array, but the spec keys are Non-Integer Strings,
                //  we are annoyed, but we don't stop the whole transform.
                // Just this part of the Transform won't work.
                if (Int32.TryParse(kv.Key, out int keyInt) &&
                    keyInt < inputList.Count)
                {
                    // if the input in not available in the list use null or else get value,
                    // then lookup and place a default value as defined in spec there
                    object subInput = inputList[keyInt];
                    if ( subInput != null || !originalSize.HasValue || keyInt < originalSize.Value ) {
                        subInputOptional = new OptionalObject( subInput );
                    }
                }
                kv.Value.Apply(kv.Key, subInputOptional, walkedPath, output, context);
            }
        }
    }

    /**
     * If the CompositeSpec only has computed children, we can avoid checking the getLiteralChildren() altogether, and
     *  we can do a slightly better iteration (HashSet.entrySet) across the input.
     */
    public class ComputedExecutionStrategy : ExecutionStrategy
    {
        public override void ProcessMap(IOrderedCompositeSpec spec, Dictionary<string, object> inputMap, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context )
        {
            // Iterate over the whole entrySet rather than the keyset with follow on gets of the values
            foreach (var inputEntry in inputMap)
            {
                ApplyKeyToComputed(spec.GetComputedChildren(), walkedPath, output, inputEntry.Key, new OptionalObject(inputEntry.Value), context );
            }
        }

        public override void ProcessList(IOrderedCompositeSpec spec, List<object> inputList, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context )
        {
            int? originalSize = walkedPath.LastElement().OrigSize;
            for (int index = 0; index < inputList.Count; index++)
            {
                object subInput = inputList[index];
                string subKeyStr = index.ToString();
                OptionalObject subInputOptional;
                if ( subInput == null && originalSize.HasValue && index >= originalSize.Value ) {
                    subInputOptional = new OptionalObject();
                }
                else {
                    subInputOptional = new OptionalObject(subInput);
                }

                ApplyKeyToComputed( spec.GetComputedChildren(), walkedPath, output, subKeyStr, subInputOptional, context );
            }
        }

        public override void ProcessScalar(IOrderedCompositeSpec spec, string scalarInput, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context ) {
            ApplyKeyToComputed( spec.GetComputedChildren(), walkedPath, output, scalarInput, new OptionalObject(), context );
        }
    }

    /**
     * In order to implement the key precedence order, we have to process each input "key", first to
     *  see if it matches any literals, and if it does not, check against each of the computed
     */
    public class ConflictExecutionStrategy : ExecutionStrategy
    {
        public override void ProcessMap( IOrderedCompositeSpec spec, Dictionary<string, object> inputMap, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context ) {

            // Iterate over the whole entrySet rather than the keyset with follow on gets of the values
            foreach (var inputEntry in inputMap)
            {
                ApplyKeyToLiteralAndComputed( spec, inputEntry.Key, new OptionalObject(inputEntry.Value), walkedPath, output, context );
            }
        }

        public override void ProcessList(IOrderedCompositeSpec spec, List<object> inputList, WalkedPath walkedPath, 
            Dictionary<string, object> output, Dictionary<string, object> context )
        {
            int? originalSize = walkedPath.LastElement().OrigSize;
            for (int index = 0; index < inputList.Count; index++)
            {
                object subInput = inputList[index];
                string subKeyStr = index.ToString();
                OptionalObject subInputOptional;
                if ( subInput == null && originalSize.HasValue && index >= originalSize ) {
                    subInputOptional = new OptionalObject();
                }
                else {
                    subInputOptional = new OptionalObject(subInput);
                }

                ApplyKeyToLiteralAndComputed( spec, subKeyStr, subInputOptional, walkedPath, output, context );
            }
        }

        public override void ProcessScalar( IOrderedCompositeSpec spec, string scalarInput, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context )
        {
            ApplyKeyToLiteralAndComputed( spec, scalarInput, new OptionalObject(), walkedPath, output, context );
        }
    }

    /**
     * We have both literal and computed children, but we have determined that there is no way an input key
     *  could match one of our literal and computed children.  Hence we can safely run each one.
     */
    public class AvailableLiteralsWithComputedExecutionStrategy : ExecutionStrategy
    {        
        public override void ProcessMap( IOrderedCompositeSpec spec, Dictionary<string, object> inputMap, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context )
        {
            ExecutionStrategy.AvailableLiterals.ProcessMap( spec, inputMap, walkedPath, output, context );
            ExecutionStrategy.Computed.ProcessMap( spec, inputMap, walkedPath, output, context );
        }

        public override void ProcessList(IOrderedCompositeSpec spec, List<object> inputList, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context )
        {
            ExecutionStrategy.AvailableLiterals.ProcessList( spec, inputList, walkedPath, output, context );
            ExecutionStrategy.Computed.ProcessList( spec, inputList, walkedPath, output, context );
        }

        public override void ProcessScalar(IOrderedCompositeSpec spec, string scalarInput, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context )
        {
            ExecutionStrategy.AvailableLiterals.ProcessScalar( spec, scalarInput, walkedPath, output, context );
            ExecutionStrategy.Computed.ProcessScalar( spec, scalarInput, walkedPath, output, context );
        }
    }

    public class AllLiteralsWithComputedExecutionStrategy : ExecutionStrategy
    {
        public override void ProcessMap(IOrderedCompositeSpec spec, Dictionary<string, object> inputMap, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context )
        {
            ExecutionStrategy.AllLiterals.ProcessMap( spec, inputMap, walkedPath, output, context );
            ExecutionStrategy.Computed.ProcessMap( spec, inputMap, walkedPath, output, context );
        }

        public override void ProcessList(IOrderedCompositeSpec spec, List<object> inputList, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context )
        {
            ExecutionStrategy.AllLiterals.ProcessList( spec, inputList, walkedPath, output, context );
            ExecutionStrategy.Computed.ProcessList( spec, inputList, walkedPath, output, context );
        }

        public override void ProcessScalar(IOrderedCompositeSpec spec, string scalarInput, WalkedPath walkedPath, Dictionary<string, object> output, Dictionary<string, object> context)
        {
            ExecutionStrategy.AllLiterals.ProcessScalar( spec, scalarInput, walkedPath, output, context );
            ExecutionStrategy.Computed.ProcessScalar( spec, scalarInput, walkedPath, output, context );
        }
    };
}
