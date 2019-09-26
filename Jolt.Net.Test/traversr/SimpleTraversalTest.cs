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
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jolt.Net.Test
{
    [Parallelizable(ParallelScope.All)]
    public class SimpleTraversalTest 
    {
        private static IEnumerable<TestCaseData> CreateTestCases(string testName)
        {
            var tests = new object[][]
            {
                new object[]
                {
                    "Simple Map Test",
                    SimpleTraversal.NewTraversal( "a.b" ),
                    JToken.Parse( "{ \"a\" : null }" ),
                    JToken.Parse( "{ \"a\" : { \"b\" : \"tuna\" } }" ),
                    new JValue("tuna")
                },
                new object[]
                {
                    "Simple explicit array test",
                    SimpleTraversal.NewTraversal( "a.[1].b" ),
                    JToken.Parse( "{ \"a\" : null }" ),
                    JToken.Parse( "{ \"a\" : [ null, { \"b\" : \"tuna\" } ] }" ),
                    new JValue("tuna")
                },
                new object[]
                {
                    "Leading Array test",
                    SimpleTraversal.NewTraversal( "[0].a" ),
                    JToken.Parse( "[ ]" ),
                    JToken.Parse( "[ { \"a\" : \"b\" } ]" ),
                    new JValue("b")
                },
                new object[]
                {
                    "Auto expand array test",
                    SimpleTraversal.NewTraversal( "a.[].b" ),
                    JToken.Parse( "{ \"a\" : null }" ),
                    JToken.Parse( "{ \"a\" : [ { \"b\" : null } ] }" ),
                    null
                }
            };
            foreach (var test in tests)
            {
                yield return new TestCaseData(test.Skip(1).ToArray()) { TestName = $"{testName}({test[0]})" };
            }
        }

        public static IEnumerable<TestCaseData> SetTestCases() =>
            CreateTestCases("SetTests");

        public static IEnumerable<TestCaseData> GetTestCases() =>
            CreateTestCases("GetTests");

        [TestCaseSource(nameof(GetTestCases))]
        public void GetTests(SimpleTraversal simpleTraversal, JToken ignoredForTest, JToken input, JToken expected)
        {
            var original = input.DeepClone();
            var tree = input.DeepClone();

            var actual = simpleTraversal.Get(tree);

            expected.Should().BeEquivalentTo(actual);
            original.Should().BeEquivalentTo(tree, "Get should not have modified the input");
        }

        [TestCaseSource(nameof(SetTestCases))]
        public void SetTests(SimpleTraversal simpleTraversal, JToken start, JToken expected, JToken toSet) 
        {
            var actual = start.DeepClone();

            simpleTraversal.Set(actual, toSet).Should().BeEquivalentTo(toSet); // set should be successful

            actual.Should().BeEquivalentTo(actual);
        }

        [Test]
        public void TestAutoArray()
        {
            var traversal = SimpleTraversal.NewTraversal( "a.[].b" );

            var expected = JToken.Parse( "{ \"a\" : [ { \"b\" : \"one\" }, { \"b\" : \"two\" } ] }" );

            var actual = new JObject();

            traversal.Get(actual).Should().BeNull();
            actual.Count.Should().Be(0); // get didn't add anything

            // Add two things and validate the Auto Expand array
            traversal.Set(actual, "one").Should().BeEquivalentTo(JValue.CreateString("one"));
            traversal.Set(actual, "two").Should().BeEquivalentTo(JValue.CreateString("two"));

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void TestOverwrite()
        {
            var traversal = SimpleTraversal.NewTraversal( "a.b" );

            var actual = JToken.Parse( "{ \"a\" : { \"b\" : \"tuna\" } }" );
            var expectedOne = JToken.Parse( "{ \"a\" : { \"b\" : \"one\" } }" );
            var expectedTwo = JToken.Parse( "{ \"a\" : { \"b\" : \"two\" } }" );

            traversal.Get(actual).Should().BeEquivalentTo(JValue.CreateString("tuna"));

            // Set twice and verify that the sets did in fact overwrite
            traversal.Set(actual, "one").Should().BeEquivalentTo(JValue.CreateString("one"));
            actual.Should().BeEquivalentTo(expectedOne);

            traversal.Set(actual, "two").Should().BeEquivalentTo(JValue.CreateString("two"));
            actual.Should().BeEquivalentTo(expectedTwo);
        }

        public static IEnumerable<TestCaseData> RemoveTestCases()
        {
            return new TestCaseData[]
            {
                new TestCaseData(
                    SimpleTraversal.NewTraversal( "__queryContext" ),
                    JObject.Parse("{ 'Id' : '1234', '__queryContext' : { 'catalogLin' : [ 'a', 'b' ] } }" ),
                    JObject.Parse("{ 'Id' : '1234' }" ),
                    JObject.Parse("{ 'catalogLin' : [ 'a', 'b' ] }" )
                )
                {
                    TestName = "RemoveTests(Inception Map Test)"
                },
                new TestCaseData(
                    SimpleTraversal.NewTraversal( "a.list.[1]" ),
                    JObject.Parse("{ 'a' : { 'list' : [ 'a', 'b', 'c' ] } }" ),
                    JObject.Parse("{ 'a' : { 'list' : [ 'a', 'c' ] } }" ),
                    new JValue("b")
                )
                {
                    TestName = "RemoveTests(List Test)"
                },
                new TestCaseData(
                    SimpleTraversal.NewTraversal( "a.list" ),
                    JObject.Parse("{ 'a' : { 'list' : [ 'a', 'b', 'c' ] } }" ),
                    JObject.Parse("{ 'a' : { } }" ),
                    new JArray( "a","b","c" )
                )
                {
                    TestName = "RemoveTests(Map leave empty Map)"
                },
                new TestCaseData(
                    SimpleTraversal.NewTraversal( "a.list.[0]" ),
                    JObject.Parse("{ 'a' : { 'list' : [ 'a' ] } }" ),
                    JObject.Parse("{ 'a' : { 'list' : [ ] } }" ),
                    new JValue("a")
                )
                {
                    TestName = "RemoveTestsMap leave empty List)"
                }
            };
        }

        [TestCaseSource(nameof(RemoveTestCases))]
        public void RemoveTests(SimpleTraversal simpleTraversal,
                                 JToken start, JToken expectedLeft, JToken expectedReturn)
        {
            var actualRemoveOpt = simpleTraversal.Remove(start);
            actualRemoveOpt.Should().BeEquivalentTo(expectedReturn);
            start.Should().BeEquivalentTo(expectedLeft);
        }

        [Test]
        public void ExceptionTestListIsMap()
        {
            var tree = JObject.Parse("{ 'Id' : '1234', '__queryContext' : { 'catalogLin' : [ 'a', 'b' ] } }" );
            var trav = SimpleTraversal.NewTraversal( "__queryContext" );
            // barfs here, needs the 'List list =' part to trigger it
            FluentActions
                .Invoking(() => (JArray)trav.Get(tree))
                .Should().Throw<InvalidCastException>();
        }

        [Test]
        public void ExceptionTestMapIsList()
        {
            var tree = JObject.Parse("{ 'Id' : '1234', '__queryContext' : { 'catalogLin' : [ 'a', 'b' ] } }" );

            var trav = SimpleTraversal.NewTraversal( "__queryContext.catalogLin" );
            // barfs here, needs the 'Map map =' part to trigger it
            FluentActions
                .Invoking(() => (JObject)trav.Get(tree))
                .Should().Throw<InvalidCastException>();
        }

        [Test]
        public void ExceptionTestListIsMapErasure()
        {
            var tree = JObject.Parse("{ 'Id' : '1234', '__queryContext' : { 'catalogLin' : [ 'a', 'b' ] } }" );

            var trav = SimpleTraversal.NewTraversal( "__queryContext" );
            
            // this works
            var queryContext = (JObject)trav.Get(tree);

            // this does not
            FluentActions
                .Invoking(() => (JObject)queryContext["catalogLin"])
                .Should().Throw<InvalidCastException>();
        }

        [Test]
        public void ExceptionTestLMapIsListErasure()
        {
            var tree = JObject.Parse("{ 'Id' : '1234', '__queryContext' : { 'catalogLin' : { 'a' : 'b' } } }" );

            var trav = SimpleTraversal.NewTraversal( "__queryContext" );

            // this works
            var queryContext = (JObject)trav.Get(tree);

            // this does not
            FluentActions
                .Invoking(() => (JArray)queryContext["catalogLin"])
                .Should().Throw<InvalidCastException>();
        }
    }
}
