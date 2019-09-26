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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;

namespace Jolt.Net.Test
{
    public class JsonTestCase
    {
        public JToken Input { get; set; }
        public JToken Spec { get; set; }
        public JToken Expected { get; set; }
    }

    public class JsonTest
    {
        public static JToken GetJson(string name)
        {
            name = $"../../../json/{name}.json";
            name = Path.Combine(name.Split('/'));

            // https://github.com/nunit/nunit/issues/3148
            // sigh. why make it hard?
            var testDirectory = Path.GetDirectoryName(new Uri(typeof(JsonTest).Assembly.CodeBase).LocalPath);
            name = Path.Combine(testDirectory, name);

            using (var fs = File.Open(name, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sr = new StreamReader(fs))
            using (var jr = new JsonTextReader(sr))
            {
                return JToken.Load(jr);
            }
        }

        public JsonTestCase GetTestCase(string name)
        {
            var testUnit = GetJson(name);
            return new JsonTestCase
            {
                Input = testUnit["input"],
                Spec = testUnit["spec"],
                Expected = testUnit["expected"]
            };
        }
    }
}
