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

using FluentAssertions;
using FluentAssertions.Json;
using NUnit.Framework;

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class ShiftrTest : JsonTest
    {
        // TODO: test arrays better (wildcards test array could be in reverse order)
        [TestCase("arrayExample")]
        [TestCase("arrayMismatch")]
        [TestCase("bucketToPrefixSoup")]
        [TestCase("declaredOutputArray")]
        [TestCase("escapeAllTheThings")]
        [TestCase("escapeAllTheThings2")]
        [TestCase("explicitArrayKey")]
        [TestCase("filterParallelArrays")]
        [TestCase("filterParents1")]
        [TestCase("filterParents2")]
        [TestCase("filterParents3")]
        [TestCase("firstSample")]
        [TestCase("hashDefault")]
        [TestCase("identity")]
        [TestCase("inputArrayToPrefix")]
        [TestCase("invertMap")]
        [TestCase("json-ld-escaping")]
        [TestCase("keyref")]
        [TestCase("lhsAmpMatch")]
        [TestCase("listKeys")]
        [TestCase("mapToList")]
        [TestCase("mapToList2")]
        [TestCase("mergeParallelArrays1_and-transpose")]
        [TestCase("mergeParallelArrays2_and-do-not-transpose")]
        [TestCase("mergeParallelArrays3_and-filter")]
        [TestCase("multiPlacement")]
        [TestCase("objectToArray")]
        [TestCase("passNullThru")]
        [TestCase("passThru")]
        [TestCase("pollaxman_218_duplicate_speclines_bug")]
        [TestCase("prefixDataToArray")]
        [TestCase("prefixedData")]
        [TestCase("prefixSoupToBuckets")]
        [TestCase("queryMappingXform")]
        [TestCase("shiftToTrash")]
        [TestCase("simpleLHSEscape")]
        [TestCase("simpleRHSEscape")]
        [TestCase("singlePlacement")]
        [TestCase("specialKeys")]
        [TestCase("transposeArrayContents1")]
        [TestCase("transposeArrayContents2")]
        [TestCase("transposeComplex1")]
        [TestCase("transposeComplex2")]
        [TestCase("transposeComplex3_both-sides-multipart")]
        [TestCase("transposeComplex4_lhs-multipart-rhs-sugar")]
        [TestCase("transposeComplex5_at-logic-with-embedded-array-lookups")]
        [TestCase("transposeComplex6_rhs-complex-at")]
        [TestCase("transposeComplex7_coerce-int-string-conversion")]
        [TestCase("transposeComplex8_coerce-boolean-string-conversion")]
        [TestCase("transposeComplex9_lookup_an_array_index")]
        [TestCase("transposeInverseMap1")]
        [TestCase("transposeInverseMap2")]
        [TestCase("transposeLHS1")]
        [TestCase("transposeLHS2")]
        [TestCase("transposeLHS3")]
        [TestCase("transposeNestedLookup")]
        [TestCase("transposeSimple1")]
        [TestCase("transposeSimple2")]
        [TestCase("transposeSimple3")]
        [TestCase("wildcards")]
        [TestCase("wildcardSelfAndRef")]
        [TestCase("wildcardsWithOr")]
        // TODO: test arrays better (wildcards test array could be in reverse order)
        public void RunTest(string testCaseName)
        {
            var testCase = GetTestCase($"shiftr/{testCaseName}");
            Shiftr shiftr = new Shiftr(testCase.Spec);
            var actual = shiftr.Transform(testCase.Input);

            actual.Should().BeEquivalentTo(testCase.Expected);
        }
    }
}
