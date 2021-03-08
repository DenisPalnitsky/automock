using Automock.SyntaxAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Automock.Tests
{
    [TestFixture]
    public class ShimBuilderTest
    {
        [Test]
        public void BuildMethodMock_Condition_Result()
        {
            // Arrange
            var shimBuilder = new ShimBuilder();
            TestData.PrepareCodeToTest(@"
using System;
class C
    {
        public void DoBeep()
        {    
           System.Console.Beep();
        }    
    }
", out var root, out var model);

            var methodDeclaration = root.DescendantNodes(d => true)
              .First(s => s is MethodDeclarationSyntax) as MethodDeclarationSyntax;

            var methodDataCollector = new MethodDataCollector();
            methodDataCollector.GatherData(methodDeclaration, model);
            

            // Act
            var syntaxStatement = shimBuilder.BuildMethodMock(methodDataCollector.ExternalCalls.First());
            var code = syntaxStatement.ToFullString();

            // Assert
            code.ShouldBe("ShimConsole.Beep=()=> {};");
        }
    }
}