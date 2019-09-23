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
    public class ModifierLeafSpec : ModifierSpec
    {

        private readonly List<FunctionEvaluator> _functionEvaluatorList = new List<FunctionEvaluator>();

        public ModifierLeafSpec(string rawJsonKey, object rhsObj, OpMode opMode, Dictionary<string, IFunction> functionsMap) :
            base(rawJsonKey, opMode)
        {
            FunctionEvaluator functionEvaluator;

            // "key": "expression1"
            if (rhsObj is string s)
            {
                functionEvaluator = BuildFunctionEvaluator(s, functionsMap);
                _functionEvaluatorList.Add(functionEvaluator);
            }
            // "key": ["expression1", "expression2", "expression3"]
            else if (rhsObj is List<object> rhsList && rhsList.Count > 0)
            {
                foreach (object rhs in rhsList)
                {
                    if (rhs is string rhsString)
                    {
                        functionEvaluator = BuildFunctionEvaluator(rhsString, functionsMap);
                        _functionEvaluatorList.Add(functionEvaluator);
                    }
                    else
                    {
                        functionEvaluator = FunctionEvaluator.ForArgEvaluation(FunctionArg.ForLiteral(rhs, false));
                        _functionEvaluatorList.Add(functionEvaluator);
                    }
                }
            }
            // "key": anyObjectOrLiteral --- just set as-is
            else
            {
                functionEvaluator = FunctionEvaluator.ForArgEvaluation(FunctionArg.ForLiteral(rhsObj, false));
                _functionEvaluatorList.Add(functionEvaluator);
            }
        }

        protected override void ApplyElement(string inputKey, OptionalObject inputOptional, MatchedElement thisLevel, WalkedPath walkedPath, Dictionary<string, object> context)
        {

            object parent = walkedPath.LastElement().TreeRef;

            walkedPath.Add(inputOptional.Value, thisLevel);

            OptionalObject valueOptional = GetFirstAvailable(_functionEvaluatorList, inputOptional, walkedPath, context);

            if (valueOptional.HasValue)
            {
                SetData(parent, thisLevel, valueOptional.Value, _opMode);
            }

            walkedPath.RemoveLast();
        }

        private static FunctionEvaluator BuildFunctionEvaluator(string rhs, Dictionary<string, IFunction> functionsMap)
        {
            // "key": "@0" --- evaluate expression then set
            if (!rhs.StartsWith(TemplatrSpecBuilder.FUNCTION))
            {
                return FunctionEvaluator.ForArgEvaluation(ConstructSingleArg(rhs, false));
            }
            else
            {
                string functionName;
                // "key": "=abs" --- call function with current value then set output if present
                if (!rhs.Contains("(") && !rhs.EndsWith(")"))
                {
                    functionName = rhs.Substring(TemplatrSpecBuilder.FUNCTION.Length);
                    functionsMap.TryGetValue(functionName, out var function);
                    return FunctionEvaluator.ForFunctionEvaluation(function);
                }
                // "key": "=abs(@(1,&0))" --- evaluate expression then call function with
                //                            expression-output, then set output if present
                else
                {
                    string fnString = rhs.Substring(TemplatrSpecBuilder.FUNCTION.Length);
                    List<string> fnArgs = SpecStringParser.ParseFunctionArgs(fnString);
                    functionName = fnArgs[0];
                    fnArgs.RemoveAt(0);
                    functionsMap.TryGetValue(functionName, out var function);
                    return FunctionEvaluator.ForFunctionEvaluation(function, ConstructArgs(fnArgs));
                }
            }
        }

        private static OptionalObject GetFirstAvailable(List<FunctionEvaluator> functionEvaluatorList, OptionalObject inputOptional, WalkedPath walkedPath, Dictionary<string, object> context)
        {
            var valueOptional = new OptionalObject();
            foreach (FunctionEvaluator functionEvaluator in functionEvaluatorList)
            {
                try
                {
                    valueOptional = functionEvaluator.Evaluate(inputOptional, walkedPath, context);
                    if (valueOptional.HasValue)
                    {
                        return valueOptional;
                    }
                }
                catch (Exception)
                {
                }
            }
            return valueOptional;
        }

        private static FunctionArg[] ConstructArgs(List<string> argsList)
        {
            FunctionArg[] argsArray = new FunctionArg[argsList.Count];
            for (int i = 0; i < argsList.Count; i++)
            {
                string arg = argsList[i];
                argsArray[i] = ConstructSingleArg(arg, true);
            }
            return argsArray;
        }

        private static FunctionArg ConstructSingleArg(string arg, bool forFunction)
        {
            if (arg.StartsWith(TemplatrSpecBuilder.CARET))
            {
                return FunctionArg.ForContext(TRAVERSAL_BUILDER.Build(arg.Substring(1)));
            }
            else if (arg.StartsWith(TemplatrSpecBuilder.AT))
            {
                return FunctionArg.ForSelf(TRAVERSAL_BUILDER.Build(arg));
            }
            else
            {
                return FunctionArg.ForLiteral(arg, forFunction);
            }
        }
    }
}
