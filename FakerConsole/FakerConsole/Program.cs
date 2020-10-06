using System;
using FakerLib;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace FakerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            FakerConfig config = new FakerConfig();
            config.Add<Test,string,HelloGenerator>(x=>x.stringField);
            Faker faker = new Faker(config);
            Test test = faker.Create<Test>();
            TestStruct s = faker.Create<TestStruct>();
            List<Test> lt = faker.Create<List<Test>>();
            UriBuilder builder = new UriBuilder();
            Uri uri = faker.Create<Uri>();
            DateTime dt = faker.Create<DateTime>();
            Console.WriteLine(Serialize(test));
        }

        public static object Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            XmlSerializer serializer = new XmlSerializer(typeof(Test));
            serializer.Serialize(stream, obj);
            return Encoding.Default.GetString(stream.ToArray());
        }
    }

    public class Test
    {
        public Test()
        {

        }
        private Test(int A, char C)
        {
            intField = 851004;
            charField = 'U';
        }

        //public Uri uriProperty { get; set; }
        public DateTime dateTimeField;
        public List<TestStruct> teststructs;
        public int intField;
        public char charField;
        public string stringField;
        public Test2 test2Property { get; set; }

        public string txt { get; }
    }

    public class Test2
    {
        public int f;
        public bool t;
        public string s2;
        public Test tr;
    }

    public class HelloGenerator : IDtoGenerator
    {
        public object Generate()
        {
            return "Hello";
        }
    }

    public struct TestStruct
    {
        public int intField;
        public string stringField;
    }
}
