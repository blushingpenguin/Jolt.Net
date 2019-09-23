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

namespace Jolt.Net
{
    public abstract class BasePathReference : IPathReference
    {
        private readonly int _pathIndex;    // equals 0 for "&"  "&0"  and  "&(0,x)"

        protected abstract char GetToken();

        public BasePathReference(string refStr)
        {
            if (string.IsNullOrEmpty(refStr) || GetToken() != refStr[0])
            {
                throw new SpecException("Invalid reference key=" + refStr + " either blank or doesn't start with correct character=" + GetToken());
            }

            int pathIndex = 0;

            if (refStr.Length > 1)
            {
                string meat = refStr.Substring(1);
                if (!Int32.TryParse(meat, out pathIndex))
                {
                    throw new SpecException("Unable to parse '" + GetToken() + "' reference key:" + refStr);
                }
            }

            if (pathIndex < 0)
            {
                throw new SpecException("Reference:" + refStr + " can not have a negative value.");
            }

            _pathIndex = pathIndex;
        }

        public int GetPathIndex() => _pathIndex;

        /**
         * Builds the non-syntactic sugar / maximally expanded and unique form of this reference.
         * @return canonical form : aka "#" -> "#0
         */
        public string GetCanonicalForm() =>
            $"{GetToken()}{_pathIndex}";
    }
}
