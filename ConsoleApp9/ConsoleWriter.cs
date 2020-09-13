using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp9
{
    class ConsoleWriter : Writer
    {
        public override void Write(string Serialized)
        {
            Console.WriteLine(Serialized);
        }
    }
}
