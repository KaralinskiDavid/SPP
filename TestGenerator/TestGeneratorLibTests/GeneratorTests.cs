using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestGeneratorLib;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.IO;

namespace TestGeneratorLib.Tests
{
    [TestClass()]
    public class GeneratorTests
    {
        
        private static List<TestClass> generatedClasses;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            string source = "..\\..\\..\\";
            string testedClassespath = "TestedClasses.cs";
            string path = Path.Combine(source, testedClassespath);
            string code = File.ReadAllText(path);

            generatedClasses = Generator.GetTestClasses(code).Result;
        }


        [TestMethod()]
        public void GeneratedClassesCount()
        {
            int actualCount = generatedClasses.Count;

            int expectedCount = 3;

            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestMethod()]
        public void AttributesCheck()
        {
            foreach (var testClass in generatedClasses)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(testClass.Code);

                int actualClassAttrCount = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().
                    Where(cd => cd.AttributeLists.
                        Where(al => al.Attributes.
                            Where(a => a.Name.ToString() == "TestClass").Any())
                        .Any())
                    .Count();
                int actualMethodAttrCount = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().
                    Where(cd => cd.AttributeLists.
                        Where(al => al.Attributes.
                            Where(a => a.Name.ToString() == "TestMethod").Any())
                        .Any())
                    .Count();

                int expectedClassAttrCount = 1;
                int expectedMethodAttrCount = !syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().
                    Where(cd => cd.AttributeLists.
                        Where(al => al.Attributes.
                            Where(a => a.Name.ToString() == "ClassInitialize").Any())
                        .Any()).Any()
                     ?
                        syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Count() : 
                        syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Count() -1;

                Assert.AreEqual(expectedClassAttrCount, actualClassAttrCount);
                Assert.AreEqual(expectedMethodAttrCount, actualMethodAttrCount);
            }
        }

        [TestMethod()]
        public void MethodsCountCheck()
        {
            int actualMethodsCount = (CSharpSyntaxTree.ParseText(generatedClasses[0].Code).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()).Count();

            int expectedMethodsCount = 5;

            Assert.AreEqual(expectedMethodsCount, actualMethodsCount);
        }

        [TestMethod()]
        public void FailAssertsCheck()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(generatedClasses[0].Code);

            var invocations = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();
            int actualAssertsCount = invocations.Where(i => i.GetText().ToString().Contains("Assert.Fail()")).Count();

            int expectedAssertsAttrCount = !syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().
                    Where(cd => cd.AttributeLists.
                        Where(al => al.Attributes.
                            Where(a => a.Name.ToString() == "ClassInitialize").Any())
                        .Any()).Any()
                     ?
                        syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Count() :
                        syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Count() - 1;

            Assert.AreEqual(expectedAssertsAttrCount, actualAssertsCount);
        }

        [TestMethod()]
        public void ClassInitializeCheck()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(generatedClasses[0].Code);
            var classInitialize = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First(
                                    m => m.AttributeLists.
                                            Where(al => al.Attributes.
                                                Where(a => a.Name.ToString() == "ClassInitialize").Any()).Any());
            var variables = classInitialize.DescendantNodes().OfType<VariableDeclarationSyntax>();
            var mock = classInitialize.DescendantNodes().OfType<AssignmentExpressionSyntax>();

            Assert.IsTrue(variables.Where(v => v.ToString().Contains("int a = 0")).Any());
            Assert.IsTrue(variables.Where(v => v.ToString().Contains("string s = null")).Any());
            Assert.IsTrue(variables.Where(v => v.ToString().Contains("SecondTestedClass st = null")).Any());
            Assert.IsTrue(mock.Where(m => m.ToString().Contains("new Mock<IAsyncDisposable>()")).Any());
        }

        [TestMethod()]
        public void StaticCheck()
        {
            var syntaxTree3 = CSharpSyntaxTree.ParseText(generatedClasses[2].Code);
            
            var syntaxTree1 = CSharpSyntaxTree.ParseText(generatedClasses[0].Code);
            var staticMethod = syntaxTree1.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First(m => m.Identifier.ValueText == "StaticCheckTest");
            var staticInvocation = staticMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
            var staticMethodAsserts = staticMethod.DescendantNodes().OfType<InvocationExpressionSyntax>();

            Assert.IsFalse(syntaxTree3.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().
                    Where(cd => cd.AttributeLists.
                        Where(al => al.Attributes.
                            Where(a => a.Name.ToString() == "ClassInitialize").Any())
                        .Any()).Any());
            Assert.IsFalse(staticMethodAsserts.Where(a => a.GetText().ToString() == "Assert.AreEqual(expected, actual)").Any());
            Assert.IsTrue(staticInvocation.GetText().ToString().Contains("FirstTestedClass.StaticCheck(b)") &&
                !staticInvocation.GetText().ToString().Contains("_firstTestedClass.StaticCheck(b)"));
        }

        [TestMethod()]
        public void UsingsCheck()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(generatedClasses[0].Code);
            var actualUsings = syntaxTree.GetCompilationUnitRoot().Usings.Select(u => u.Name.ToString()).ToArray();

            var expectedUsings = new string[] { "Microsoft.VisualStudio.TestTools.UnitTesting", "Moq", "System", "System.Collections.Generic", "TestGeneratorLibTests"};

            CollectionAssert.AreEquivalent(expectedUsings, actualUsings);
        }

    }
}