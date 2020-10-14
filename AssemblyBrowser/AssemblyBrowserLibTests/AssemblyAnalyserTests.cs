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
            string methodSignature = "(Int32,char,ref double,in char,out int)";

            Assert.IsNotNull(testType);
            Assert.IsTrue(testType.methods.FirstOrDefault(m => m.methodName == methodname) != null);
            Assert.IsTrue(testType.methods.FirstOrDefault(m => m.methodName == "SomeMethod").methodSignature==methodSignature);
        }
    }


    public class ExtendedClass
    {
        int intField;
        Dictionary<IList<string>, double> dictionary;
        string stringProperty { get; set; }

        public void SomeMethod(int IntParameter, char CharParameter, ref double DoubleRef, in char InChar , out int OutInt)
        {
            OutInt = 3;
            return;
        }
    }
}