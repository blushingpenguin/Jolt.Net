/*
 * Copyright 2014 Bazaarvoice, Inc.
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
using System.Collections.Generic;

namespace Jolt.Net
{
    /**
     * A tuple class that contains the data for one level of a
     *  tree walk, aka a reference to the input for that level, and
     *  the LiteralPathElement that was matched at that level.
     */
    public sealed class PathStep
    {
        public object TreeRef { get; }
        public MatchedElement MatchedElement { get; }
        public int? OrigSize { get; }

        public PathStep(object treeRef, MatchedElement matchedElement)
        {
            TreeRef = treeRef;
            MatchedElement = matchedElement;
            if (MatchedElement is ArrayMatchedElement ame)
            {
                OrigSize = ame.GetOrigSize();
            }
        }
    }
}
