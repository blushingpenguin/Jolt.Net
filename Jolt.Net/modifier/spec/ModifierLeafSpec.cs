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
using System;
using System.Collections.Generic;

namespace Jolt.Net
{
    public class ModifierLeafSpec : ModifierSpec
    {

        private readonly List<FunctionEvaluator> _functionEvaluatorList = new List<FunctionEvaluator>();

        public ModifierLeafSpec(string rawJsonKey, JToken rhsObj, OpMode opMode, IReadOnlyDictionary<string, IFunction> functionsMap) :
            base(rawJsonKey, opMode)
        {
            FunctionEvaluator functionEvaluator;

            // "key": "expression1"
            if (rhsObj.Type == JTokenType.String)
            {
                string s = rhsObj.ToString();
                functionEvaluator = BuildFunctionEvaluator(s, functionsMap);
                _functionEvaluatorList.Add(functionEvaluator);
            }
            // "key": ["expression1", "expression2", "expression3"]
            else if (rhsObj is JArray rhsList && rhsList.Count > 0)
            {
                foreach (var rhs in rhsList)
                {
                    if (rhs.Type == JTokenType.String)
                    {
                        functionEvaluator = BuildFunctionEvaluator(rhs.ToString(), functionsMap);
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

        protected override void ApplyElement(string inputKey, JToken inputOptional, MatchedElement thisLevel, WalkedPath walkedPath, JObject context)
        {
            JToken parent = walkedPath.LastElement().TreeRef;

            walkedPath.Add(inputOptional, thisLevel);

            JToken valueOptional = GetFirstAvailable(_functionEvaluatorList, inputOptional, walkedPath, context);

            if (valueOptional != null)
            {
                SetData(parent, thisLevel, valueOptional, _opMode);
            }

            walkedPath.RemoveLast();
        }

        private static FunctionEvaluator BuildFunctionEvaluator(string rhs, IReadOnlyDictionary<string, IFunction> functionsMap)
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

        private static JToken GetFirstAvailable(List<FunctionEvaluator> functionEvaluatorList, JToken inputOptional, WalkedPath walkedPath, JObject context)
        {
            JToken valueOptional = null;
            foreach (FunctionEvaluator functionEvaluator in functionEvaluatorList)
            {
                try
                {
                    valueOptional = functionEvaluator.Evaluate(inputOptional, walkedPath, context);
                    if (valueOptional != null)
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
