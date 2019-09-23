using Newtonsoft.Json.Linq;
using System;

namespace Jolt.Net.Test
{
    class Program
    {
        static string _spec = @"
[
  {
    ""operation"": ""shift"",
    ""spec"": {
      ""ratings"": {
        ""*"": {
          // #2 means go three levels up the tree (count from 0),
          //  and ask the ""ratings"" node, how many of it's
          //  children have been matched.
          //
          // This allows us to put the Name and the Value into
          //  the same object in the Ratings array.
          ""$"": ""Ratings[#2].Name"",
          ""@"": ""Ratings[#2].Value""
        }
      }
    }
  }
]
";

        static string _input = @"
{
    ""ratings"": {
    ""primary"": 5,
    ""quality"": 4,
    ""design"": 5
  }
}";

        static void Main(string[] args)
        {
            var spec = JToken.Parse(_spec.Trim());
            var chainr = Chainr.FromSpec(spec);
            var input = JObject.Parse(_input.Trim());
            chainr.Transform(input);
        }
    }
}
