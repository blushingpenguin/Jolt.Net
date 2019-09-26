using System;
using System.Collections.Generic;
using System.Text;

namespace Jolt.Net.Test
{
    public class TestTransforms
    {
        public static Dictionary<string, Type> _transforms = new Dictionary<string, Type>();
        public static IReadOnlyDictionary<string, Type> Transforms => _transforms;

        static void Add(Type type)
        {
            _transforms.Add(type.Name, type);
        }

        static TestTransforms()
        {
            Add(typeof(BadSpecTransform));
            Add(typeof(ExplodingTestTransform));
            Add(typeof(GoodContextDrivenTransform));
            Add(typeof(GoodSpecAndContextDrivenTransform));
            Add(typeof(GoodTestTransform));
        }
    }
}
