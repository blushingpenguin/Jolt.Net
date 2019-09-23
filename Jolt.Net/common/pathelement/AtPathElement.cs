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
    public class AtPathElement : BasePathElement, IMatchablePathElement
    {
        public AtPathElement(string key) :
            base(key)
        {
            if ("@" != key)
            {
                throw new SpecException("'References Input' key '@', can only be a single '@'.  Offending key : " + key);
            }
        }

        public MatchedElement Match(string dataKey, WalkedPath walkedPath)
        {
            return walkedPath.LastElement().MatchedElement;  // copy what our parent was so that write keys of &0 and &1 both work.
        }

        public override string GetCanonicalForm() => "@";
    }
}
