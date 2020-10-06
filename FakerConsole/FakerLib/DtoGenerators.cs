using System;
using System.Collections.Generic;
using System.Text;

namespace FakerLib
{
    public interface IDtoGenerator
    {
        object Generate();
    }

    public class GeneratorTypeAttribute : Attribute
    {
        public string GeneratorType;
    }

    public class IntGenerator : IDtoGenerator
    {
        private Random _random;

        public IntGenerator()
        {
            _random = new Random();
        }
        public object Generate()
        {
            return _random.Next(0,133);
        }
    }

    public class LongGenerator : IDtoGenerator
    {
        private Random _random;

        public LongGenerator()
        {
            _random = new Random();
        }
        public object Generate()
        {
            return (long)_random.Next();
        }
    }

    public class DoubleGenerator : IDtoGenerator
    {
        private Random _random;

        public DoubleGenerator()
        {
            _random = new Random();
        }
        public object Generate()
        {
            return _random.NextDouble();
        }
    }

    public class CharGenerator : IDtoGenerator
    {
        private Random _random;

        public CharGenerator()
        {
            _random = new Random();
        }

        public object Generate()
        {
            int reg = _random.Next();
            return (reg%2==0) ? (char)_random.Next(65,91) : (char)_random.Next(97,123);
        }
    }

    public class StringGenerator : IDtoGenerator
    {
        private Random _random;

        public StringGenerator()
        {
            _random = new Random();
        }

        public object Generate()
        {
            CharGenerator charGenerator = new CharGenerator();
            int length = _random.Next(1,33);
            StringBuilder result = new StringBuilder(length);
            for (var i=0;i<length;i++)
            {
                result.Append(charGenerator.Generate());
            }
            return result.ToString();
        }
    }

    public class UriGenerator : IDtoGenerator
    {
        public object Generate()
        {
            StringGenerator sg = new StringGenerator();
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Host = (string)sg.Generate() + "." + (string)sg.Generate();
            uriBuilder.Path = (string)sg.Generate();
            return uriBuilder.Uri;
        }
    }

    public class ListGenerator : IDtoGenerator
    {
        private Random _random;
        private Faker _faker;

        public ListGenerator(Faker faker)
        {
            _random = new Random();
            _faker = faker;
        }
        public object Generate()
        {
            throw new NotImplementedException();
        }

        public List<T> GenerateGeneric<T>()
        {
            int length = _random.Next(1, 21);
            List<T> result = new List<T>(length);
            for(var i=0;i<length;i++)
            {
                result.Add(_faker.Create<T>());
            }
            return result;
        }
    }

}

