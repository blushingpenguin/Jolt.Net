# Getting Started

## 1 Add the Jolt.Net nuget package to your c# project.

## 2 Code and Sample Data

1. Copy-paste this code and sample data.
2. Get it to run
3. Replace the input and spec file with your own

### JoltSample.cs

Available [here](https://github.com/blushingpenguin/Jolt.Net/Sample/Program.cs).

``` csharp
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
```

### /json/sample/input.json
Available [here](https://github.com/blushingpenguin/Jolt.Net/Sample/input.json).

``` json
{
    "rating": {
        "primary": {
            "value": 3
        },
        "quality": {
            "value": 3
        }
    }
}
```

### /json/sample/spec.json
Available [here](https://github.com/blushingpenguin/Jolt.Net/Sample/spec.json).

``` json
[
    {
        "operation": "shift",
        "spec": {
            "rating": {
                "primary": {
                    "value": "Rating"
                },
                "*": {
                    "value": "SecondaryRatings.&1.Value",
                    "$": "SecondaryRatings.&.Id"
                }
            }
        }
    },
    {
        "operation": "default",
        "spec": {
            "Range" : 5,
            "SecondaryRatings" : {
                "*" : {
                    "Range" : 5
                }
            }
        }
    }
]
```

### Output

With pretty formatting, looks like:

``` json
{
    "Rating": 3,
    "Range": 5,
    "SecondaryRatings": {
        "quality": {
            "Id": "quality",
            "Value": 3,
            "Range": 5
        }
    }
}
```
