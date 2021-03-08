using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automock.Tests
{
    [TestFixture]
    public class TestProjectProviderTest
    {
        [Test]
        public void Test()
        {
            var workspace = new AdhocWorkspace();

            var projName = "NewProject";
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projName, projName, LanguageNames.CSharp);
            var newProject = workspace.AddProject(projectInfo);
            var sourceText = SourceText.From("class A {}");
            var newDocument = workspace.AddDocument(newProject.Id, "NewFile.cs", sourceText);

            foreach (var project in workspace.CurrentSolution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    Console.WriteLine(project.Name + "\t\t\t" + document.Name);
                }
            }
        }
    }
}
