using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Automock.Templates
{
    class TestClass
    {
        Workspace _workspace;
        CompilationUnitSyntax _syntaxRoot;
        private string _testClassName;

        MethodDeclarationSyntax InitializeTestMethod
        {
            get
            {
                return _syntaxRoot.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Single(c => c.Identifier.Text.Equals("Initialize"));
            }
        }

        public NamespaceDeclarationSyntax NamespaceDeclaration { get; set; }
        
        public SourceText TestClassSorceText
        {
            get
            {               
                var formattedResult = Formatter.Format(_syntaxRoot, _workspace);
                return formattedResult.GetText();
            }
        }        

        public TestClass(Workspace workspace, CompilationUnitSyntax syntaxRoot, string testClassName)
        {
            _workspace = workspace;
            _syntaxRoot = syntaxRoot;
            _testClassName = testClassName;
        }

        public static TestClass Create(Document documentToUpdate, string testClassName, bool isNewClass)
        {
            if (isNewClass)
            {
                var code = ReadTemplateFromResources();
                var testClass = CreateTestClassInternal(
                    documentToUpdate.Project.Solution.Workspace, 
                    code, 
                    testClassName);
                testClass.UpdateClassName(testClassName);
                return testClass;
            }
            else
            {
                if (documentToUpdate.TryGetText(out var text))
                {
                    return CreateTestClassInternal(
                        documentToUpdate.Project.Solution.Workspace,
                        text.ToString(),
                        testClassName);
                }

                throw new InvalidOperationException("Failed to get text of test document");
            }
        }

        private static TestClass CreateTestClassInternal(Workspace workspace, string code, string testClassName)
        {
            var parsedClass = CSharpSyntaxTree.ParseText(code);           

            return new TestClass(workspace, parsedClass.GetCompilationUnitRoot(), testClassName);
        }      

        internal void AddMethod(MethodDeclarationSyntax testMethoDeclaration)
        {
            var classDeclaration = GetClassDeclaration(_testClassName);
            var newClass = classDeclaration.AddMembers(testMethoDeclaration);
            _syntaxRoot = _syntaxRoot.ReplaceNode(classDeclaration, newClass);
        }

        internal void AddUsingDerectives(IEnumerable<UsingDirectiveSyntax> usingDirectives)
        {
            var oldUsings = _syntaxRoot.DescendantNodes().OfType<UsingDirectiveSyntax>();
            var newUsings =  usingDirectives
                .Distinct(new UsingDirectiveSyntaxIEqualityComparere())
                .Except(oldUsings, new UsingDirectiveSyntaxIEqualityComparere())                
                .OrderBy(t => t.Name.ToString());
           
            _syntaxRoot = _syntaxRoot.AddUsings(newUsings.ToArray());
        }

        public class UsingDirectiveSyntaxIEqualityComparere : IEqualityComparer<UsingDirectiveSyntax>
        {
            public bool Equals(UsingDirectiveSyntax x, UsingDirectiveSyntax y)
            {
                return string.Equals(x.ToString(), y.ToString());
            }

            public int GetHashCode(UsingDirectiveSyntax obj)
            {
                return obj.ToString().GetHashCode();
            }
        }

        internal void UpdateNemaspace(string newNamespace)
        {
           var namespaceSyntax = _syntaxRoot.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single();
            _syntaxRoot = _syntaxRoot.ReplaceNode (namespaceSyntax.Name, SyntaxFactory.ParseName(newNamespace));
        }

        public void UpdateClassName(string newClassName)
        {
            var classDeclaration = GetClassDeclaration(newClassName);
            _syntaxRoot = _syntaxRoot.ReplaceToken(classDeclaration.Identifier, SyntaxFactory.Identifier(newClassName));
        }

        private ClassDeclarationSyntax GetClassDeclaration(string testClassName)
        {
            var testClassCandidate = _syntaxRoot
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.ValueText.Contains(testClassName));

            if (testClassCandidate == null)
            {
                return (ClassDeclarationSyntax)_syntaxRoot
                .DescendantNodes()
                .First(s=> s is ClassDeclarationSyntax);
            }

            return testClassCandidate;
        }

        public void AddStatementsToInitializeMethod(params StatementSyntax[] statements)
        {
            var old = InitializeTestMethod;
            var newNode = old.AddBodyStatements(statements);

            _syntaxRoot = _syntaxRoot.ReplaceNode(old, newNode);
        }        
     
        private static string ReadTemplateFromResources()
        {
            var currentAssembly = typeof(TestClass).Assembly;
            using (var stream = currentAssembly.GetManifestResourceStream("Automock.Templates.NewTestClassTemplate.cs"))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
