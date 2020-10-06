using System;
using FakerLib;

namespace FloatGenerator
{
    [GeneratorType(GeneratorType = "System.Single")]
    public class FloatGenerator : IDtoGenerator
    {
        private Random _random;

        public FloatGenerator()
        {
            _random = new Random();
        }
        public object Generate()
        {
            return (float)_random.NextDouble();
        }
    }
}