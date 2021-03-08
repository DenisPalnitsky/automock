using Automock.SyntaxAnalyzer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automock.Tests
{
    [TestFixture]
    public class MethodDataCollectorTest
    {
        [Test]
        public void GatherData_ProvidesDataForMethodUnderTest()
        {
            // Arrange
            TestData.PrepareCodeToTest(TestData.GenericMethodCode, out var root, out var model);
            var dataCollector = new MethodDataCollector();

            // Act
            dataCollector.GatherData(root.DescendantNodes().OfType<MethodDeclarationSyntax>().First(), 
                model);

            // Assert
            dataCollector.MethodUnderTest.ClassName.ShouldBe("C");
            dataCollector.MethodUnderTest.MethodName.ShouldBe("GetDateTime");            
        }
    }
}
