using System;
using FakerLib;

namespace DateTimeGenerator
{
    [GeneratorType(GeneratorType="System.DateTime")]
    public class DateTimeGenerator : IDtoGenerator
    {
        private Random _random;

        public DateTimeGenerator()
        {
            _random = new Random();
        }

        public object Generate()
        {
            int year = _random.Next(2001, 2021);
            int month = _random.Next(1, 13);
            int day = _random.Next(1, 25);
            int hour = _random.Next(1, 24);
            int minute = _random.Next(1, 60);
            int second = _random.Next(1, 60);
            return new DateTime(year, month, day, hour, minute, second);
        }
    }
}
