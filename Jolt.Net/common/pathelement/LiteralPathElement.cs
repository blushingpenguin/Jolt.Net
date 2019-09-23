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
namespace Jolt.Net
{

    /**
     * Meant to be an immutable PathElement from a Spec, and therefore shareable across
     *  threads running multiple transforms using the same spec.
     */
    public class LiteralPathElement : BasePathElement, IMatchablePathElement, IEvaluatablePathElement {

        private readonly string _canonicalForm;

        public LiteralPathElement(string key) :
            base(key)
        {
            _canonicalForm = key.Replace(".", "\\.");
        }

        public string Evaluate(WalkedPath walkedPath) =>
            RawKey;

        public MatchedElement Match(string dataKey, WalkedPath walkedPath)
        {
            if (RawKey == dataKey)
            {
                return new MatchedElement(RawKey);
            }
            return null;
        }

        public override string GetCanonicalForm() => _canonicalForm;
    }
}
