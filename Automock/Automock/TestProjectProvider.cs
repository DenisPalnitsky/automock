using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Automock
{
    class TestProjectProvider
    {
        public bool UseClassNamsAsTestFileName { get; private set; } = true;

        private Project FindProject(Solution currentSolution, string currentProjectName)
        {

            var project = currentSolution.Projects
                .FirstOrDefault(
                    p => p.Name.IndexOf(TestProjectNameBuilder.GetProjectName(currentProjectName), StringComparison.OrdinalIgnoreCase) >= 0);

            if (project != null)
            {
                return project;
            }


            var topLevelName = currentProjectName.Split('.').FirstOrDefault();
            if (!string.IsNullOrEmpty(topLevelName))
            {
                // second best option
                project = currentSolution.Projects
                    .FirstOrDefault(
                        p => p.Name.IndexOf(TestProjectNameBuilder.GetProjectName(currentProjectName), StringComparison.OrdinalIgnoreCase) >= 0);
            }

            //return currentSolution.Projects.First(p => p.Name.Equals(currentProjectName));
            throw new NotSupportedException("Adding new project is not yet supported");

            //var newProject = currentSolution.AddProject(NamesBuilder.GetProjectName(currentProjectName),
            //    NamesBuilder.GetProjectName(currentProjectName), LanguageNames.CSharp);
                        
            //return newProject;
        }

        public Document GetDocumentForTest(
            ClassDeclarationSyntax parentClass,
            string methodUnderTestName,
            Document currentDocument,
            out bool isItNewDocument)
        {
            var projectFinder = new TestProjectProvider();

            var testFileName = UseClassNamsAsTestFileName 
                ? TestClassNameBuilder.GetTestClassFileName(parentClass.Identifier.ToString())
                : TestClassNameBuilder.GetTestClassFileName(
                         parentClass.Identifier.ToString(),
                         methodUnderTestName);

            var newProject = projectFinder.FindProject(currentDocument.Project.Solution, currentDocument.Project.Name);
            
            var newDocument = newProject.Documents
                .FirstOrDefault(d => d.Name.Equals(testFileName, StringComparison.InvariantCultureIgnoreCase));

            if (newDocument != null)
            {
                isItNewDocument = false;
            }
            else
            {
                isItNewDocument = true;
                newDocument = newProject.AddDocument(
                                                testFileName,
                                                "// TODO Automock: Automock failed to finish generating the tests. Try again or report this to developer",
                                                currentDocument.Folders);
            }

            return newDocument;
        }
    }
}
