using System;
using System.Collections.Generic;
using System.Text;

namespace TestGeneratorLib
{
    public class TestClass
    {
        public string Name { get; }
        public string Code { get; }
        public TestClass(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }
}
