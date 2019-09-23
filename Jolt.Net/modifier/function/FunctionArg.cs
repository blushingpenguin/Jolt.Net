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
    class SelfLookupArg : FunctionArg
    {
        public readonly TransposePathElement _pathElement;

        public SelfLookupArg(PathEvaluatingTraversal traversal)
        {
            IPathElement pathElement = traversal.Get(traversal.Size() - 1);
            if (pathElement is TransposePathElement tpe)
            {
                _pathElement = tpe;
            }
            else
            {
                throw new SpecException("Expected @ path element here");
            }
        }

        public override OptionalObject EvaluateArg(WalkedPath walkedPath, Dictionary<string, object> context)
        {
            return _pathElement.ObjectEvaluate(walkedPath);
        }
    }

    class ContextLookupArg : FunctionArg
    {
        private readonly PathEvaluatingTraversal _traversal;

        public ContextLookupArg(PathEvaluatingTraversal traversal)
        {
            _traversal = traversal;
        }

        public override OptionalObject EvaluateArg(WalkedPath walkedPath, Dictionary<string, object> context)
        {
            return _traversal.Read(context, walkedPath);
        }
    }

    class LiteralArg : FunctionArg
    {

        private OptionalObject _returnValue;

        public LiteralArg(object o)
        {
            _returnValue = new OptionalObject(o);
        }

        public override OptionalObject EvaluateArg(WalkedPath walkedPath, Dictionary<string, object> context)
        {
            return _returnValue;
        }
    }


    public abstract class FunctionArg
    {
        public static FunctionArg ForSelf(PathEvaluatingTraversal traversal)
        {
            return new SelfLookupArg(traversal);
        }

        public static FunctionArg ForContext(PathEvaluatingTraversal traversal)
        {
            return new ContextLookupArg(traversal);
        }

        public static FunctionArg ForLiteral(object obj, bool parseArg)
        {
            if (parseArg)
            {
                if (obj is string arg)
                {
                    if (arg.Length == 0)
                    {
                        return new LiteralArg(null);
                    }
                    else if (arg.StartsWith("'") && arg.EndsWith("'"))
                    {
                        return new LiteralArg(arg.Substring(1, arg.Length - 2));
                    }
                    else if (arg.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        return new LiteralArg(true);
                    }
                    else if (arg.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        return new LiteralArg(false);
                    }
                    else
                    {
                        if (Double.TryParse(arg, out var numVal))
                        {
                            return new LiteralArg(numVal);
                        }
                        return new LiteralArg(arg);
                    }
                }
                else
                {
                    return new LiteralArg(obj);
                }
            }
            else
            {
                return new LiteralArg(obj);
            }
        }

        public abstract OptionalObject EvaluateArg(WalkedPath walkedPath, Dictionary<string, object> context);
    }
}
