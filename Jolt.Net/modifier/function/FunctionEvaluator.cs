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
    public class FunctionEvaluator
    {
        public static FunctionEvaluator ForFunctionEvaluation(IFunction function, params FunctionArg[] functionArgs)
        {
            return new FunctionEvaluator(function, functionArgs);
        }

        public static FunctionEvaluator ForArgEvaluation(FunctionArg functionArgs)
        {
            return new FunctionEvaluator(null, functionArgs);
        }

        // function that is evaluated and applied as output
        private readonly IFunction _function;

        // arguments of the function, not evaluated and can be a jolt path expression that
        // either point to a context or self, or a value present at the matching level
        private readonly FunctionArg[] _functionArgs;

        private FunctionEvaluator(IFunction function, params FunctionArg[] functionArgs)
        {
            _function = function;
            _functionArgs = functionArgs;
        }

        public OptionalObject Evaluate(OptionalObject inputOptional, WalkedPath walkedPath, Dictionary<string, object> context)
        {
            OptionalObject valueOptional = new OptionalObject();
            try
            {
                // "key": "@0", "key": literal
                if (_function == null)
                {
                    valueOptional = _functionArgs[0].EvaluateArg(walkedPath, context);
                }
                // "key": "=abs(@(1,&0))"
                // this is most usual case, a single argument is passed and we need to evaluate and
                // pass the value, if present, to the spec function
                else if (_functionArgs.Length == 1)
                {
                    var evaluatedArgValue = _functionArgs[0].EvaluateArg(walkedPath, context);
                    valueOptional = evaluatedArgValue.HasValue ? _function.Apply(evaluatedArgValue.Value) : _function.Apply();
                }
                // "key": "=abs(@(1,&0),-1,-3)"
                // this is more complicated case! if args is an array, after evaluation we cannot pass a missing value wrapped in
                // object[] into function. In such case null will be passed however, in json null is also a valid value, so it is
                // upto the implementer to interpret the value. Ideally we can almost always pass a list straight from input.
                else if (_functionArgs.Length > 1)
                {
                    object[] evaluatedArgs = EvaluateArgsValue(_functionArgs, context, walkedPath);
                    valueOptional = _function.Apply(evaluatedArgs);
                }
                //
                // FYI this is where the "magic" happens that allows functions that take a single method
                //  default to the current "match" rather than an explicit "reference".
                // Note, this does not work for functions that take more than a single input.
                //
                // "key": "=abs"
                else
                {
                    // pass current value as arg if present
                    valueOptional = inputOptional.HasValue ? _function.Apply(inputOptional.Value) : _function.Apply();
                }
            }
            catch (Exception)
            {
            }

            return valueOptional;

        }

        private static object[] EvaluateArgsValue(FunctionArg[] functionArgs, Dictionary<string, object> context, WalkedPath walkedPath)
        {
            object[] evaluatedArgs = new object[functionArgs.Length];
            for (int i = 0; i < functionArgs.Length; i++)
            {
                FunctionArg arg = functionArgs[i];
                OptionalObject evaluatedValue = arg.EvaluateArg(walkedPath, context);
                evaluatedArgs[i] = evaluatedValue.Value;
            }
            return evaluatedArgs;
        }
    }
}
