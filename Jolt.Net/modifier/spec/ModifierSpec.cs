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
     * Base Templatr spec
     */
    public abstract class ModifierSpec : IBaseSpec
    {
        class ModifierTraversalBuilder : TraversalBuilder<TransposeReader>
        {
            public override TransposeReader BuildFromPath(string path)
            {
                return new TransposeReader(path);
            }
        }

        // traversal builder that uses a TransposeReader to create a PathEvaluatingTraversal
        protected static readonly TraversalBuilder<TransposeReader> TRAVERSAL_BUILDER = new ModifierTraversalBuilder();

        protected readonly OpMode _opMode;
        protected readonly IMatchablePathElement _pathElement;
        protected readonly bool _checkValue;

        public bool GetCheckValue() => _checkValue;

        /**
         * Builds LHS pathElement and validates to specification
         */
        protected ModifierSpec(string rawJsonKey, OpMode opMode) 
        {
            string prefix = rawJsonKey.Substring(0, 1);
            string suffix = rawJsonKey.Length > 1 ? rawJsonKey.Substring(rawJsonKey.Length - 1) : null;

            if (OpMode.IsValid(prefix))
            {
                _opMode = OpMode.From(prefix);
                rawJsonKey = rawJsonKey.Substring(1);
            }
            else
            {
                _opMode = opMode;
            }

            if (suffix == "?" && !rawJsonKey.EndsWith("\\?"))
            {
                _checkValue = true;
                rawJsonKey = rawJsonKey.Substring(0, rawJsonKey.Length - 1);
            }
            else
            {
                _checkValue = false;
            }

            _pathElement = PathElementBuilder.BuildMatchablePathElement(rawJsonKey);
            if (!(_pathElement is IStarPathElement) && !(_pathElement is LiteralPathElement) && !(_pathElement is ArrayPathElement))
            {
                throw new SpecException(opMode.GetName() + " cannot have " + _pathElement.GetType().Name + " RHS");
            }
        }

        public IMatchablePathElement GetPathElement() => _pathElement;

        public bool Apply(string inputKey, JToken inputOptional, WalkedPath walkedPath, JObject output, JObject context)
        {
            if (output != null)
            {
                throw new TransformException("Expected a null output");
            }

            MatchedElement thisLevel = _pathElement.Match(inputKey, walkedPath);
            if (thisLevel == null)
            {
                return false;
            }

            if (!_checkValue) // there was no trailing "?" so no check is necessary
            {
                ApplyElement(inputKey, inputOptional, thisLevel, walkedPath, context);
            }
            else if (inputOptional != null)
            {
                ApplyElement(inputKey, inputOptional, thisLevel, walkedPath, context);
            }
            return true;
        }

        /**
         * Templatr specific override that is used in BaseSpec#apply(...)
         * The name is changed for easy identification during debugging
         */
        protected abstract void ApplyElement(string key, JToken inputOptional, MatchedElement thisLevel, WalkedPath walkedPath, JObject context);

        /**
         * Static utility method for facilitating writes on input object
         *
         * @param parent the source object
         * @param matchedElement the current spec (leaf) element that was matched with input
         * @param value to write
         * @param opMode to determine if write is applicable
         */
        protected static void SetData(JToken parent, MatchedElement matchedElement, JToken value, OpMode opMode)
        {
            if (parent is JObject source)
            {
                string key = matchedElement.RawKey;
                if (opMode.IsApplicable(source, key))
                {
                    source[key] = value;
                }
            }
            else if (parent is JArray list && matchedElement is ArrayMatchedElement ame)
            {
                int origSize = ame.GetOrigSize();
                int reqIndex = ame.GetRawIndex();
                if (opMode.IsApplicable(list, reqIndex, origSize))
                {
                    list[reqIndex] = value;
                }
            }
            else
            {
                throw new InvalidOperationException("Should not come here!");
            }
        }
    }
}
