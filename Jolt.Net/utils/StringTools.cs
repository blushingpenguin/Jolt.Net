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

    /**
     *
     * This class mimics the behavior of apache StringTools, except that it works on CharSequence instead of string
     *
     * Also, with this, jolt-core can finally be free of apache-common dependency
     */
    public class StringTools
    {
        /**
         * Count the num# of matches of subSequence in sourceSequence
         *
         * @param sourceSequence to find occurrence from
         * @param subSequence to find occurrence of
         * @return num of occurrences of subSequence in sourceSequence
         */
        public static int CountMatches(string sourceSequence, string subSequence) {
            if (String.IsNullOrEmpty(sourceSequence) || String.IsNullOrEmpty(subSequence) || 
                sourceSequence.Length < subSequence.Length)
            {
                return 0;
            }

            int count = 0;
            int sourceSequenceIndex = 0;
            int subSequenceIndex = 0;

            while (sourceSequenceIndex < sourceSequence.Length)
            {
                if (sourceSequence[sourceSequenceIndex] == subSequence[subSequenceIndex])
                {
                    sourceSequenceIndex++;
                    subSequenceIndex++;
                    while (sourceSequenceIndex < sourceSequence.Length && subSequenceIndex < subSequence.Length)
                    {
                        if (sourceSequence[sourceSequenceIndex] != subSequence[subSequenceIndex])
                        {
                            break;
                        }
                        sourceSequenceIndex++;
                        subSequenceIndex++;
                    }
                    if (subSequenceIndex == subSequence.Length)
                    {
                        count++;
                    }
                    subSequenceIndex = 0;
                    continue;
                }
                sourceSequenceIndex++;
            }

            return count;
        }
    }
}
