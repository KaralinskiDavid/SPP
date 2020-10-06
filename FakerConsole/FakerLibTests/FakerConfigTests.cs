using Microsoft.VisualStudio.TestTools.UnitTesting;
using FakerLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakerLibTests
{
    [TestClass()]
    public class FakerConfigTests
    {
        FakerConfig config = new FakerConfig();

        [TestMethod]
        public void ConfigTest()
        {
            config.Add<TestClass, int, IntPropertyGenerator>(obj => obj.intProperty);
            config.Add<TestClass, string, StringPropertyGenerator>(obj => obj.stringProperty);
            config.Add<TestClass, TestSubClass, TestSubClassGenerator>(obj => obj.testSubClass);
            Faker faker = new Faker(config);
            TestClass testClass = faker.Create<TestClass>();

            Assert.AreEqual(851004, testClass.intProperty);
            Assert.AreEqual("851004", testClass.stringProperty);
            Assert.AreEqual(13, testClass.testSubClass.A);
            Assert.AreEqual("пятница", testClass.testSubClass.B);
        }
    }

    public class TestClass
    {
        public int intProperty { get; set; }
        public string stringProperty { get; set; }
        public TestSubClass testSubClass { get; set; }

    }

    public class TestSubClass
    {
        public int A;
        public string B;
    }

    public class IntPropertyGenerator : IDtoGenerator
    {
        public object Generate()
        {
            return 851004;
        }
    }

    public class StringPropertyGenerator : IDtoGenerator
    {
        public object Generate()
        {
            return "851004";
        }
    }


    public class TestSubClassGenerator : IDtoGenerator
    {
        public object Generate()
        {
            return new TestSubClass { A=13, B="пятница"};
        }
    }
}
