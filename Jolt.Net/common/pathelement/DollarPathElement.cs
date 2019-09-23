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
    public class DollarPathElement : BasePathElement, IMatchablePathElement, IEvaluatablePathElement {

        private readonly DollarReference _dRef;

        public DollarPathElement(string key) :
            base(key)
        {
            _dRef = new DollarReference(key);
        }

        public override string GetCanonicalForm() =>
            _dRef.GetCanonicalForm();

        public virtual string Evaluate(WalkedPath walkedPath)
        {
            MatchedElement pe = walkedPath.ElementFromEnd(_dRef.GetPathIndex()).MatchedElement;
            return pe.GetSubKeyRef(_dRef.GetKeyGroup());
        }

        public virtual MatchedElement Match(string dataKey, WalkedPath walkedPath)
        {
            string evaled = Evaluate(walkedPath);
            return new MatchedElement(evaled);
        }
    }

}
