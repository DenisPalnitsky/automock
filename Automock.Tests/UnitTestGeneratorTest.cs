using Automock.SyntaxAnalyzer;
using Automock.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using Shouldly;
using System;
using System.Linq;

namespace Automock.Tests
{
    [TestFixture]
    public class UnitTestGeneratorTest
    {
        [Test]
        public void GenerateTest_AddsShimForDateTimeParse()
        {
            var testCode = GenerateTestCode(@"
using System;
namespace MyNamespace
{
    class C
    {
        public DateTime GetDateTime(bool isNotNow)
        {    
            var useless = DateTime.Parse(""10102018"");
            if (p)
                return DateTime.Parse(""10102018"");
            else
                return DateTime.Now;
        }    
    }
}");

            testCode.ShouldContain("ShimDateTime.ParseString = (p1) => default(DateTime);");
            testCode.ShouldContain("using System.Fakes");
            testCode.ShouldContain("using Microsoft.VisualStudio.TestTools.UnitTesting;");
            testCode.ShouldContain("[TestMethod]");

            testCode.ShouldContain("public class");
            testCode.ShouldContain("// Arrange");
            testCode.ShouldContain("// Act");
            testCode.ShouldContain("// Assert");
            testCode.ShouldContain("var classUnderTest = new C();");
            testCode.ShouldContain(@"classUnderTest.GetDateTime(
                    isNotNow: default(bool));");

            var shimForGetDateTime = "ShimDateTime.ParseString = (p1) => default(DateTime)";
            Assert.IsTrue(testCode.IndexOf(shimForGetDateTime) ==
                testCode.LastIndexOf(shimForGetDateTime));

            testCode.ShouldContain("private IDisposable _shimContext");
            testCode.ShouldContain("_shimContext = ShimsContext.Create();");
            testCode.ShouldContain("_shimContext.Dispose();");
            testCode.ShouldContain("public void CleanUp()");
        }

        [Test]
        public void GenerateTest_ForStaticMethod_GeneratesClass()
        {
            var sourceTest = GenerateTestCode(@"
using System;
namespace MyNamespace
{
    class C
    {
        public static DateTime GetDateTimeStatic(bool isNotNow)
        {    
            var useless = DateTime.Parse(""10102018"");
            if (p)
                return DateTime.Parse(""10102018"");
            else
                return DateTime.Now;
        }    
    }
}");

            sourceTest.ShouldContain(@"C.GetDateTimeStatic(
                    isNotNow: default(bool));");
        }

        [Test]
        public void GenerateTest_FileAlreadyExist_AddShimToTestMethod()
        {
            var sourceTest = GenerateTestCode(@"
using System;
namespace MyNamespace
{
    class C
    {
        public DateTime GetDateTime()
        {    
            return DateTime.Parse(""10102018"");
        }    
    }
}", false);

            sourceTest.ShouldContain(@"// Arrange
            ShimDateTime.ParseString = (p1) => default(DateTime);
            var classUnderTest = new C();");
        }

        [Test]
        public void GenerateTest_FileAlreadyExistForStaticMethod_AddShimToTestMethod()
        {
            var sourceTest = GenerateTestCode(@"
using System;
namespace MyNamespace
{
    class C
    {
        public static DateTime GetDateTime()
        {    
            return DateTime.Parse(""10102018"");
        }    
    }
}", false);
            // arrange 
            // assert

            sourceTest.ShouldContain(@"// Arrange
            ShimDateTime.ParseString = (p1) => default(DateTime);

            // Act");
        }

        [Test]
        public void GenerateTest_FileAlreadyExistButNoShimsNeeded_RemovePlaceholder()
        {
            var sourceTest = GenerateTestCode(@"
using System;
namespace MyNamespace
{
    class C
    {
        public static void GetDateTime()
        {                
        }    
    }
}", false);

            sourceTest.ShouldContain(@"// Arrange

            // Act");
        }

        [Test]
        public void GenerateTest_ShimWithoutReturnType_InsureOnelineShimFormatt()
        {
            var sourceTest = GenerateTestCode(@"
using System;
class C
    {
        public void DoBeep()
        {    
           System.Console.Beep();
        }    
    }   
", false);

            sourceTest.ShouldContain(@" ShimConsole.Beep = () => {};");
        }

        private string GenerateTestCode(string code, bool isNewDocument = true)
        {
            TestData.PrepareCodeToTest(code, out var root, out var model);

            var methodDeclaration = root.DescendantNodes(d => true)
                .First(s => s is MethodDeclarationSyntax)
                as MethodDeclarationSyntax;

            var methodDataCollector = new MethodDataCollector();
            methodDataCollector.GatherData(methodDeclaration, model);

            var targetDocument = CreateDocument();
            var unitTestGenerator = new TestClassGenerator(targetDocument, isNewDocument);

            var sourceTest = unitTestGenerator.GenerateTest(
                TestClass.Create(targetDocument, "A", isNewDocument),
                methodDataCollector.MethodUnderTest,
                methodDataCollector.ExternalCalls).ToString();
            return sourceTest;
        }



        private Document CreateDocument()
        {
            var workspace = new AdhocWorkspace();

            var proj = workspace.AddProject("NewProject", LanguageNames.CSharp);
            var sourceText = SourceText.From("namespace Ns { class A {} }");
            var newDocument = proj.AddDocument("NewFile.cs", sourceText);
            newDocument.Project.Solution.Workspace.TryApplyChanges(newDocument.Project.Solution);

            foreach (var project in workspace.CurrentSolution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    Console.WriteLine(project.Name + "\t\t\t" + document.Name);
                }
            }
            return newDocument;
        }

        private Document CreateDocument2()
        {
            var workspace = new AdhocWorkspace();

            string projectName = "HelloWorldProject";
            ProjectId projectId = ProjectId.CreateNewId();
            VersionStamp versionStamp = VersionStamp.Create();
            ProjectInfo helloWorldProject = ProjectInfo.Create(projectId, versionStamp, projectName, projectName, LanguageNames.CSharp);
            SourceText sourceText = SourceText.From("class Program { static void Main() { System.Console.WriteLine(\"HelloWorld\"); } }");

            Project newProject = workspace.AddProject(helloWorldProject);
            Document newDocument = workspace.AddDocument(newProject.Id, "Program.cs", sourceText);
            return newDocument;
        }
    
    }
}
