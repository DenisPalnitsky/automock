using Automock.Templates;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis;

namespace Automock.Tests
{
    [TestFixture]
    class TestClassTest
    {
        [Test]
        public void CreateNew_CreatesNewFormTemplate()
        {
            var testClass = TestClass.Create(CreateNewDocument(), "SomeTestClass", true);
            testClass.TestClassSorceText.ShouldNotBeNull();
        }

        private static Document CreateNewDocument()
        {
            var workspace = new AdhocWorkspace();
            var proj = workspace.AddProject("TesProj", "C#");
            var doc = proj.AddDocument("UnitTest", "");
            return doc;
        }

        [Test]
        public void UpdateInitialize_CreatesNewSyntaxTreee()
        {
            var testClass = TestClass.Create(CreateNewDocument(), "SomeTestClass", true);

            const string newStatement = "var s = DateTime.Now;";
            testClass.AddStatementsToInitializeMethod(
                ParseStatement(newStatement)
                );

            testClass.TestClassSorceText.ToString().ShouldContain(newStatement);
        }

        [Test]
        public void UpdateName_CreatesNewSyntaxTreee()
        {
            var testClass = TestClass.Create(CreateNewDocument(), "SomeTestClass", true);

            var newTestClassName = "NewTestClassName";
            testClass.UpdateClassName("NewTestClassName");

            testClass.TestClassSorceText.ToString().ShouldContain(newTestClassName);
        }
    }
}
