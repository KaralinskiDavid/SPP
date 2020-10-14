using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssemblyBrowserLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace AssemblyBrowserLib.Tests
{
    [TestClass()]
    public class AssemblyAnalyserTests
    {
        public AssemblyAnalyser analyser;
        public AssemblyAnalysisResult analysysResult;

        [TestInitialize]
        public void Initialize()
        {
            analyser = new AssemblyAnalyser();
            analysysResult = analyser.GetAnalysysResult(Directory.GetCurrentDirectory() + "/AssemblyBrowserLibTests.dll");
        }

        [TestMethod()]
        public void NamespaceNameTest()
        {
            var namespaces = analysysResult.namespaces;

            Assert.IsTrue(namespaces.Where(n => n.NamespaceName == "AssemblyBrowserLib.Tests").Any());
        }

        [TestMethod()]
        public void TypeCheck()
        {
            var namespaces = analysysResult.namespaces;
            var testType = namespaces.First(n => n.NamespaceName == "AssemblyBrowserLib.Tests").types.FirstOrDefault(t => t.typeName == "ExtendedClass");
            string methodname = "SomeMethod";
            string methodSignature = "(Int32,Char,ref Double,in Char,out Int32)";

            Assert.IsNotNull(testType);
            Assert.IsTrue(testType.methods.Where(m => m.methodName == methodname).Any());
            Assert.IsTrue(testType.methods.FirstOrDefault(m => m.methodName == methodname).methodSignature==methodSignature);
        }

        [TestMethod]
        public void ExtensionTypeCheck()
        {
            var namespaces = analysysResult.namespaces;
            var testType = namespaces.First(n => n.NamespaceName == "AssemblyBrowserLib.Tests").types.FirstOrDefault(t => t.typeName == "ExtendedClass");
            string extensionMethodName = "GetIntFieldPlus";

            Assert.IsTrue(testType.methods.Where(m => m.methodName == extensionMethodName).Any());
        }

        [TestMethod]
        public void TypesCountCheck()
        {
            var namespaces = analysysResult.namespaces;
            var types = namespaces.First(n => n.NamespaceName == "AssemblyBrowserLib.Tests").types;
            int typesCountExpected = 3;

            int typesCountActual = types.Count;

            Assert.AreEqual(typesCountExpected, typesCountActual);
        }

        public void GenericTypeNameCheck()
        {
            var namespaces = analysysResult.namespaces;
            var testType = namespaces.First(n => n.NamespaceName == "AssemblyBrowserLib.Tests").types.FirstOrDefault(t => t.typeName == "ExtendedClass");
            string genericTypeName = "Dictionary<IList<string>,double>";
            string genericTypeNameActual = testType.fields.Where(f => f.fieldName == "dictionary").First().typeName;

            Assert.AreEqual(genericTypeName, genericTypeNameActual);

        }

    }


    public class ExtendedClass
    {
        public int intField;
        Dictionary<IList<string>, double> dictionary;
        string stringProperty { get; set; }

        public void SomeMethod(int IntParameter, char CharParameter, ref double DoubleRef, in char InChar , out int OutInt)
        {
            OutInt = 3;
            return;
        }
    }

    public static class ClassExtension
    {
        public static int GetIntFieldPlus(this ExtendedClass extendedClass, int plus)
        {
            return extendedClass.intField + plus;
        }
    }

}