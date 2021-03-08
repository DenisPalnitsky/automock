using Automock.SyntaxAnalyzer;
using Automock.Templates;
using Automock.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Automock
{
    public class TestClassGenerator
    {
        private const string ShimPlaceholderVariableName = "ShimPlaceholder";
        private const string ShimsPlaceholder = "var " + ShimPlaceholderVariableName + " = 1;";
        private const string ArrangeComment = "// Arrange";
        private const string ActComment = "// Act";
        private const string AssertComment = "// Assert";
        private const string AssertInconclusiveStatement = @"Assert.Inconclusive();";

        private const string TestMethodAttribute = "TestMethod";
        private const string TestFrameworkNamespace = "Microsoft.VisualStudio.TestTools.UnitTesting";

        private Document _targetDocument;        
        private readonly bool _isNewTestClass;

        public TestClassGenerator(Document newDocument, bool isNewTestClass)
        {
            _targetDocument = newDocument;
            _isNewTestClass = isNewTestClass;
        }

        internal SourceText GenerateTest(
            TestClass testClass,
            MethodData methodUnderTest, 
            IEnumerable<MethodData> externalCalls)
        {             
            var testMethoDeclaration = CreateTestMethod(methodUnderTest);

            testMethoDeclaration = AddShims(testClass, externalCalls, testMethoDeclaration);
            testClass.AddMethod(testMethoDeclaration);

            testClass.UpdateNemaspace(
                TestClassNameBuilder.GetNamespaceName(_targetDocument));

            var usingDirectives = GenerateUsingDirectives(methodUnderTest, externalCalls);
            testClass.AddUsingDerectives(usingDirectives);

            return testClass.TestClassSorceText;
        }

        private MethodDeclarationSyntax AddShims(TestClass testClass, IEnumerable<MethodData> externalCalls, MethodDeclarationSyntax testMethoDeclaration)
        {
            var nodeInArrangeSection = testMethoDeclaration
                            .DescendantNodes()
                            .OfType<LocalDeclarationStatementSyntax>()
                            .Single(v => v.ToString()
                                         .StartsWith(ShimsPlaceholder));
            var shimStatements = CreateShimsStatements(externalCalls);

            if (_isNewTestClass)
            {
                testClass.AddStatementsToInitializeMethod(shimStatements.ToArray());
                testMethoDeclaration = testMethoDeclaration.RemoveNode(nodeInArrangeSection, SyntaxRemoveOptions.KeepLeadingTrivia);
            }
            else
            {
                if (shimStatements.Any())
                {
                    // Arrange
                    shimStatements[0] = shimStatements[0].WithLeadingTrivia(Comment(ArrangeComment), EndOfLine(" "));

                    testMethoDeclaration = testMethoDeclaration
                      .ReplaceNode(nodeInArrangeSection, shimStatements);
                }
                else
                {
                    testMethoDeclaration = testMethoDeclaration.RemoveNode(nodeInArrangeSection, SyntaxRemoveOptions.KeepLeadingTrivia);
                }
            }
           
            return testMethoDeclaration;
        }

        private static IEnumerable<UsingDirectiveSyntax> GenerateUsingDirectives(
            MethodData methodUnderTest, 
            IEnumerable<MethodData> externalCalls)
        {
            var namespaces = new HashSet<string>
            {
                TestFrameworkNamespace,
                "Microsoft.QualityTools.Testing.Fakes",
                "Shouldly"
            };
            namespaces.UnionWith(methodUnderTest.GetAllRelatedNamespaces());            
            namespaces.UnionWith(externalCalls.SelectMany(s => s.GetAllRelatedNamespaces()));
            namespaces.RemoveWhere(s => string.IsNullOrEmpty(s));

            // Fake everything for now 
            namespaces.UnionWith(namespaces.ToArray().Select(s => s + ".Fakes"));
            

            return namespaces.Select(n => UsingDirective(ParseName(n)));
        }

        private static MethodDeclarationSyntax CreateTestMethod(MethodData methodUnderTest)
        {
            var block = (BlockSyntax)ParseStatement("{\r\n" + CreateMethodBody(methodUnderTest) + "}");

            var testMethoDeclaration = MethodDeclaration(
                    List(
                        new AttributeListSyntax[]{ AttributeList(SingletonSeparatedList<AttributeSyntax>(
                           Attribute(IdentifierName(TestMethodAttribute))))}
                        ),
                    TokenList(Token(SyntaxKind.PublicKeyword)),
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    null,
                    Identifier($"{methodUnderTest.MethodName}_Condition_Result"),
                    null,
                    ParameterList(),
                    List<TypeParameterConstraintClauseSyntax>(),
                    block,
                    Token(SyntaxKind.SemicolonToken)
                );

            testMethoDeclaration = testMethoDeclaration.WithLeadingTrivia(EndOfLine(Environment.NewLine));

            return testMethoDeclaration;
        }

        private static string CreateMethodBody(MethodData methodUnderTest)
        {
            const string classUnderTestVariableName = "classUnderTest";

            var testMethodBuilder = new StringBuilder();
            testMethodBuilder.AppendLine(
                BuildArrangeSection(methodUnderTest, classUnderTestVariableName));

            testMethodBuilder.AppendLine(
                BuildActSection(methodUnderTest, classUnderTestVariableName));

            testMethodBuilder.AppendLine();

            testMethodBuilder.AppendLine(AssertComment);
            testMethodBuilder.AppendLine(AssertInconclusiveStatement);
            return testMethodBuilder.ToString();
        }

        private static string BuildArrangeSection(MethodData methodUnderTest, string classUnderTestVariableName)
        {
            var testMethodBuilder = new StringBuilder();
            testMethodBuilder.AppendLine(ArrangeComment);

            testMethodBuilder.AppendLine(ShimsPlaceholder);

            if (!methodUnderTest.IsStatic)
            {
                testMethodBuilder.AppendLine();
                var createClassUnderTestMethod = ObjectCreationExpression(
                            IdentifierName(methodUnderTest.ClassName))
                            .WithArgumentList(
                                ArgumentList()).NormalizeWhitespace();

                testMethodBuilder.AppendLine($"var {classUnderTestVariableName} = {createClassUnderTestMethod.GetText().ToString()};");
            }

            return testMethodBuilder.ToString();
        }

        private static string BuildActSection(MethodData methodUnderTest, string classUnderTestVariableName)
        {
            var actSectionBuilder = new StringBuilder();
            actSectionBuilder.AppendLine(ActComment);

            if (!methodUnderTest.IsStatic)
            {
                actSectionBuilder.AppendLine($"{classUnderTestVariableName}.{methodUnderTest.MethodName}(");
            }
            else
            {
                actSectionBuilder.AppendLine($"{methodUnderTest.ClassName}.{methodUnderTest.MethodName}(");
            }

            foreach (var p in methodUnderTest.Parameters)
            {
                actSectionBuilder.AppendLine($"\t\t{p.Name.Trim()} : default({p.TypeSymbol.ToHumanTypeString()}),");
            }

            var methodCall = actSectionBuilder.ToString().Trim().TrimEnd(',') + ");";
            return methodCall;
        }

        private static ArgumentListSyntax BuildArgumentsList(MethodData methodUnderTest)
        {
            return ArgumentList(
                        SeparatedList(
                            methodUnderTest.Parameters
                                .Select(p=> Argument(DefaultExpression(IdentifierName(p.Name))))
                            ));
        }         

        private static List<StatementSyntax> CreateShimsStatements(IEnumerable<MethodData> externalCalls)
        {
            var shimStetements = new List<StatementSyntax>();
            var mockBuilder = new ShimBuilder();
                     
            var shimAsStrings = new HashSet<string>();
            foreach (var methodData in externalCalls)
            {
                var expression = mockBuilder.BuildMethodMock(methodData);
                var shimAsString = expression.ToString();
                if (!shimAsStrings.Contains(shimAsString))
                {
                    shimAsStrings.Add(shimAsString);
                    shimStetements.Add(expression);
                }                
            }

            return shimStetements;
        }
       
    }
}
