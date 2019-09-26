using Jolt.Net;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Sample
{
    class Program
    {
        static JToken GetJson(string path)
        {
            path = Path.Combine($"../../../{path}".Split('/'));
            return JToken.Parse(File.ReadAllText(path));
        }

        static void Main(string[] args)
        {
            var spec = GetJson("spec.json");
            var input = GetJson("input.json");
            
            Chainr chainr = Chainr.FromSpec(spec);
            var transformedOutput = chainr.Transform(input);

            Console.WriteLine(transformedOutput.ToString());
        }
    }
}
