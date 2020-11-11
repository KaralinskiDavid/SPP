using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom;

namespace TestGeneratorLib
{
    public class Generator
    {
        private static SemanticModel model;

        private static SyntaxList<UsingDirectiveSyntax> GetTestUsingDirectives(string namespaceName)
        {
            List<UsingDirectiveSyntax> result = new List<UsingDirectiveSyntax>
            {
                UsingDirective(IdentifierName("System")),
                UsingDirective(QualifiedName(
                    QualifiedName(
                    IdentifierName("System"),
                    IdentifierName("Collections")),
                    IdentifierName("Generic")
                    )),
                UsingDirective(QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName("Microsoft"),
                            IdentifierName("VisualStudio")),
                        IdentifierName("TestTools")),
                    IdentifierName("UnitTesting"))),
                UsingDirective(IdentifierName("Moq")),
                UsingDirective(IdentifierName(namespaceName))
            };
            return List(result);
        }

        private static SyntaxList<MemberDeclarationSyntax> GetTestMethods(ClassDeclarationSyntax classDeclaration)
        {
            List<MemberDeclarationSyntax> testMethods = new List<MemberDeclarationSyntax>();
            StatementSyntax fail = ParseStatement("Assert.Fail();");
            StatementSyntax areEqual = ParseStatement("Assert.AreEqual(expected, actual);");


            var attr = Attribute(IdentifierName("TestMethod()"));
            var attributes = SingletonList(AttributeList(SingletonSeparatedList<AttributeSyntax>(attr)));

            var methods = classDeclaration.Members.Where(m => m.Kind() == SyntaxKind.MethodDeclaration && ((MethodDeclarationSyntax)m).Modifiers.Where(mod => mod.Kind() == SyntaxKind.PublicKeyword).Any());

            foreach (var method in methods)
            {
                var returnType = ((MethodDeclarationSyntax)method).ReturnType;
                var identifier = ((MethodDeclarationSyntax)method).Identifier.ValueText + "Test";

                var testMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), identifier)
                                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                    .WithAttributeLists(attributes);
                testMethod.NormalizeWhitespace();

                var declarations = GetArrangeActSections(method as MethodDeclarationSyntax);
                if(returnType.ToString()!="void")
                    declarations = declarations.Add(areEqual);
                declarations = declarations.Add(fail);

                var blockbody = Block(declarations);
                testMethod = testMethod.WithBody(blockbody);
                testMethods.Add(testMethod);
            }

            return List(testMethods);
        }

        public static Task<List<TestClass>> GetTestClasses(string code)
        {
            return Task.Run(() =>
            {
                List<TestClass> result = new List<TestClass>();

                var tree = CSharpSyntaxTree.ParseText(code);

                var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                var compilation = CSharpCompilation.Create("MyCompilation",
                    syntaxTrees: new[] { tree }, references: new[] { mscorlib });
                model = compilation.GetSemanticModel(tree);

                var root = tree.GetRoot();
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

                var attr = Attribute(IdentifierName("TestClass()"));
                var attributes = SingletonList(AttributeList(SingletonSeparatedList(attr)));

                foreach (ClassDeclarationSyntax classDeclaration in classes)
                {
                    List<MemberDeclarationSyntax> fields = new List<MemberDeclarationSyntax>();

                    var constructor = classDeclaration.Members.FirstOrDefault(m => m.Kind() == SyntaxKind.ConstructorDeclaration);
                    string className = classDeclaration.Identifier.ValueText;

                    NamespaceDeclarationSyntax testClassNamespace = NamespaceDeclaration(IdentifierName(className+"Test"));
                    string testClassName = className + "Tests";
                    var methods = GetTestMethods(classDeclaration);
                    if (methods.Count > 0)
                    {
                        if (!classDeclaration.Modifiers.Where(m=>m.Kind()==SyntaxKind.StaticKeyword).Any())
                            methods = methods.Insert(0, GetClassinitializeMethod(classDeclaration, constructor as ConstructorDeclarationSyntax, fields));

                        var usings = GetTestUsingDirectives(((NamespaceDeclarationSyntax)classDeclaration.Parent).Name.ToString());

                        var newTree = CompilationUnit()
                                                        .WithUsings(usings)
                                                        .WithMembers(SingletonList<MemberDeclarationSyntax>(testClassNamespace
                                                        .WithMembers(SingletonList<MemberDeclarationSyntax>(ClassDeclaration(testClassName)
                                                            .WithMembers(List(fields).AddRange(methods))
                                                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                                            .WithAttributeLists(attributes))))
                        );

                        result.Add(new TestClass(testClassName + ".cs", newTree.NormalizeWhitespace().ToFullString()));
                    }
                }
                return result;
            }
            );
        }

        private static bool IsInterface(string typeName)
        {
            if (typeName[0] == 'I' && char.IsUpper(typeName[1]))
                return true;
            return false;
        }

        private static SyntaxList<StatementSyntax> GetArrangeActSections(MethodDeclarationSyntax method)
        {
            List<StatementSyntax> result = new List<StatementSyntax>();
            List<ArgumentSyntax> variableNames = new List<ArgumentSyntax>();

            var parameters = method.ParameterList.Parameters;

            foreach (var parameter in parameters)
            {
                var variable = GetVariableDeclaration(parameter);
                variableNames.Add(Argument(IdentifierName(variable.Declaration.Variables.First().Identifier.ValueText)));
                result.Add(variable);
            }

            result.AddRange(GetActSection(method, variableNames));
            return List(result);

        }

        private static LocalDeclarationStatementSyntax GetVariableDeclaration(ParameterSyntax parameter)
        {
            var variableType = model.GetSymbolInfo(parameter.Type).Symbol.ToString();
            var variableName = parameter.Identifier.ValueText;
            var defaultValue = GetDeafultValue(variableType);

            LiteralExpressionSyntax variableDefault;

            if (defaultValue != null)
                variableDefault = LiteralExpression(SyntaxKind.DefaultLiteralExpression, ParseToken((defaultValue).ToString()));
            else
                variableDefault = LiteralExpression(SyntaxKind.NullLiteralExpression);

            var variable = LocalDeclarationStatement(
                            VariableDeclaration(ParseTypeName(variableType), SeparatedList(new[]
                            {
                                    VariableDeclarator(Identifier(variableName),null,EqualsValueClause(variableDefault))
                            }
                                )));

            return variable;
        }

        private static FieldDeclarationSyntax GetMockDeclaration(string typeName)
        {
            FieldDeclarationSyntax mock = FieldDeclaration(
                                            VariableDeclaration(
                                                ParseTypeName($"Mock<{typeName}>"),
                                                SeparatedList(new[]
                                                {
                                                    VariableDeclarator(Identifier("_"+char.ToLower(typeName[1])+typeName.Substring(1)))
                                                })))
                                            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                                            .AddModifiers(Token(SyntaxKind.StaticKeyword));

            return mock;
        }

        private static List<StatementSyntax> GetActSection(MethodDeclarationSyntax method, List<ArgumentSyntax> variableNames)
        {
            List<StatementSyntax> statements = new List<StatementSyntax>(2);

            string classIdentifier;

            if (method.Modifiers.Where(m => m.Kind() == SyntaxKind.StaticKeyword).Any())
                classIdentifier = ((ClassDeclarationSyntax)method.Parent).Identifier.ValueText;
            else
                classIdentifier = "_" + char.ToLower(((ClassDeclarationSyntax)method.Parent).Identifier.ValueText[0]) + ((ClassDeclarationSyntax)method.Parent).Identifier.ValueText.Substring(1);



            var methodInvocation = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(classIdentifier),
                    IdentifierName(method.Identifier.ValueText)
                    ))
                .WithArgumentList(ArgumentList(SeparatedList(variableNames)));

            if (method.ReturnType.ToString()!="void")
            {
                var actualVariable = LocalDeclarationStatement(
                               VariableDeclaration(method.ReturnType, SeparatedList(new[]
                               {
                                    VariableDeclarator(Identifier("actual"),null,EqualsValueClause(methodInvocation))
                               }
                                   )));
                statements.Add(actualVariable);

                var defaultValue = GetDeafultValue(method.ReturnType.GetText().ToString());

                LiteralExpressionSyntax variableDefault;

                if (defaultValue != null)
                    variableDefault = LiteralExpression(SyntaxKind.DefaultLiteralExpression, ParseToken((defaultValue).ToString()));
                else
                    variableDefault = LiteralExpression(SyntaxKind.NullLiteralExpression);

                var expectedVariable = LocalDeclarationStatement(
                               VariableDeclaration(method.ReturnType, SeparatedList(new[]
                               {
                                    VariableDeclarator(Identifier("expected"),null,EqualsValueClause(variableDefault))
                               }
                                   )));
                statements.Add(expectedVariable);
            }



            return statements;
        }

        private static object GetDeafultValue(string typeName)
        {
            try
            {
                Type type = Type.GetType(typeName.Trim()) ?? Type.GetType(GetTypeFromAlias(typeName.Trim()));
                if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
                {
                    return Activator.CreateInstance(type);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static string GetTypeFromAlias(string aliasName)
        {
            var mscorlib = Assembly.GetAssembly(typeof(int));

            using (var provider = new CSharpCodeProvider())
            {
                foreach (var type in mscorlib.DefinedTypes)
                {
                    if (string.Equals(type.Namespace, "System"))
                    {
                        var typeRef = new CodeTypeReference(type);
                        var csTypeName = provider.GetTypeOutput(typeRef);
                        if (aliasName == csTypeName)
                        {
                            return type.FullName;
                        }
                    }
                }
            }
            return null;
        }

        private static MethodDeclarationSyntax GetClassinitializeMethod(ClassDeclarationSyntax classDeclaration, ConstructorDeclarationSyntax constructorDeclaration, List<MemberDeclarationSyntax> fields)
        {

            string className = classDeclaration.Identifier.ValueText;
            FieldDeclarationSyntax classObject = FieldDeclaration(
                                            VariableDeclaration(
                                                ParseTypeName(className),
                                                SeparatedList(new[]
                                                {
                                                    VariableDeclarator(Identifier("_"+char.ToLower(className[0])+className.Substring(1)))
                                                })))
                                            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                                            .AddModifiers(Token(SyntaxKind.StaticKeyword));
            fields.Add(classObject);


            List<StatementSyntax> statements = new List<StatementSyntax>();
            List<ArgumentSyntax> arguments = new List<ArgumentSyntax>();

            var attr = Attribute(IdentifierName("ClassInitialize"));
            var attributes = SingletonList(AttributeList(SingletonSeparatedList<AttributeSyntax>(attr)));

            if (constructorDeclaration != null)
            {
                var parameters = constructorDeclaration.ParameterList.Parameters;
                foreach (var parameter in parameters)
                {
                    string typeName = parameter.Type.GetText().ToString().Trim();

                    if (IsInterface(typeName))
                    {
                        fields.Add(GetMockDeclaration(typeName));
                        var mockObjectCreation = ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName("_" + char.ToLower(typeName[1]) + typeName.Substring(1)),
                            ObjectCreationExpression(
                                GenericName(
                                    Identifier("Mock"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            IdentifierName(typeName)))))
                            .WithArgumentList(
                                ArgumentList())));

                        statements.Add(mockObjectCreation);
                        arguments.Add(Argument(IdentifierName("_" + char.ToLower(typeName[1]) + typeName.Substring(1))));
                    }

                    else
                    {
                        var variable = GetVariableDeclaration(parameter);
                        statements.Add(variable);
                        arguments.Add(Argument(IdentifierName(variable.Declaration.Variables.First().Identifier.ValueText)));
                    }
                }
            }

            var objectCreation = ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("_" + char.ToLower(classDeclaration.Identifier.ValueText[0]) + classDeclaration.Identifier.ValueText.Substring(1)),
                    ObjectCreationExpression(
                        IdentifierName(classDeclaration.Identifier.ValueText))
                    .WithArgumentList(ArgumentList(SeparatedList(arguments))
                    )));

            statements.Add(objectCreation);

  

            var method = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("Setup"))
                .WithAttributeLists(attributes)
                .AddParameterListParameters(
                    Parameter(
                        Identifier("context"))
                    .WithType(ParseTypeName("TestContext")))
                .WithBody(Block(statements))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddModifiers(Token(SyntaxKind.StaticKeyword));

            return method;
        }
    }
}
