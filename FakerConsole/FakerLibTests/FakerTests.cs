using Microsoft.VisualStudio.TestTools.UnitTesting;
using FakerLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakerLib.Tests
{
    [TestClass()]
    public class FakerTests
    {
        private Faker _faker;

        [TestInitialize]
        public void Initialize()
        {
            _faker = new Faker();
        }

        [TestMethod]
        public void DtoTest()
        {
            int testInt = _faker.Create<int>();
            double testDouble = _faker.Create<double>();
            long testLong = _faker.Create<long>();
            float testFloat = _faker.Create<float>();
            string testString = _faker.Create<string>();
            DateTime testDateTime = _faker.Create<DateTime>();
            Uri testUri = _faker.Create<Uri>();

            Assert.AreNotEqual(default, testInt);
            Assert.AreNotEqual(default, testDouble);
            Assert.AreNotEqual(default, testLong);
            Assert.AreNotEqual(default, testFloat);
            Assert.AreNotEqual(default, testString);
            Assert.AreNotEqual(default, testDateTime);
            Assert.AreNotEqual(default, testUri);
        }

        [TestMethod]
        public void NotDtoTest()
        {
            byte ts = _faker.Create<byte>();
            ushort hc = _faker.Create<ushort>();

            Assert.AreEqual(default, ts);
            Assert.AreEqual(default, hc);
        }

        [TestMethod]
        public void SimpleClassGenerationTest()
        {
            TestClassWithoutConstructor testClass = _faker.Create<TestClassWithoutConstructor>();

            Assert.AreNotEqual(default, testClass);
            Assert.AreNotEqual(default, testClass.intField);
            Assert.AreNotEqual(default, testClass.intProperty);
            Assert.AreNotEqual(default, testClass.charField);
            Assert.AreNotEqual(default, testClass.charProperty);
            Assert.AreNotEqual(default, testClass.stringField);
            Assert.AreNotEqual(default, testClass.stringProperty);
        }

        [TestMethod]
        public void ListGenerationTest()
        {
            List<TestClassWithoutConstructor> testList = _faker.Create<List<TestClassWithoutConstructor>>();

            Assert.AreNotEqual(default, testList);
            Assert.AreNotEqual(default, testList.Count);
            CollectionAssert.AllItemsAreNotNull(testList);
        }

        [TestMethod]
        public void ConstructorCheckTest()
        {
            TestClassWithConstructors testClass = _faker.Create<TestClassWithConstructors>();

            Assert.AreEqual(13,testClass.intField);
            Assert.AreEqual("Succesesful", testClass.stringField);
        }

        [TestMethod]
        public void ReadonlyPropertiesTest()
        {
            TestClassWithReadonlyProperties testClass = _faker.Create<TestClassWithReadonlyProperties>();

            Assert.AreEqual(default, testClass.intProp);
            Assert.AreEqual(default, testClass.stringProp);
        }

        [TestMethod]
        public void LoopTest()
        {
            ClassLoop testClass = _faker.Create<ClassLoop>();

            Assert.IsNull(testClass.loopClass.classLoop.loopClass.classLoop.loopClass.classLoop);
        }

        [TestMethod]
        public void StructTest()
        {
            TestStruct testStruct = _faker.Create<TestStruct>();

            Assert.AreNotEqual(default, testStruct.intField);
            Assert.AreNotEqual(default, testStruct.stringProperty);
        }

        [TestMethod]
        public void NonWorkingConstructorCheck()
        {
            TestClassWithNoWorkingConstructor testClass = _faker.Create<TestClassWithNoWorkingConstructor>();

            Assert.IsNotNull(testClass);
        }
    }

    public class TestClassWithoutConstructor
    {
        public int intField;
        public char charField;
        public string stringField;
        public int intProperty { get; set; }
        public char charProperty { get; set; }
        public string stringProperty { get; set; }
    }

    public class TestClassWithConstructors
    {
        public TestClassWithConstructors(int intfield)
        {
            intField = 10;
        }

        private TestClassWithConstructors(int intfield, string stringfield)
        {
            intField = 13;
            stringField = "Succesesful";
        }

        public int intField;
        public string stringField;
    }

    public class TestClassWithReadonlyProperties
    {
        public int intProp { get; }
        public string stringProp { get; private set; }
    }

    public class LoopClass
    {
        public ClassLoop classLoop;
    }

    public class ClassLoop
    {
        public LoopClass loopClass;
    }

    public struct TestStruct
    {
        public int intField;
        public string stringProperty { get; set; }
    }

    public class TestClassWithNoWorkingConstructor
    {
        public TestClassWithNoWorkingConstructor(int A,int B)
        {
            throw new Exception();
        }

        public TestClassWithNoWorkingConstructor(int A)
        {

        }
    }
}