using Automock.SyntaxAnalyzer;
using Automock.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using Shouldly;
using System.Linq;

namespace Automock.Tests.Utils
{
    [TestFixture]
    public class ExtensionsTest
    {
      
        [Test]
        public void ToHumanTypeString_Condition_Result()
        {
            // Arrange
            var sourceTest = @"
using System;
using System.Collections.Generic;

namespace MyNamespace
{
    class C
    {
        public static DateTime GetDateTimeStatic(byte[] isNotNow, List<int> l, Dictionary<string, DateTime> d)
        {    
            var useless = DateTime.Parse(""10102018"");
            if (p)
                return DateTime.Parse(""10102018"");
            else
                return DateTime.Now;
        }    
    }
}";

            var shimBuilder = new ShimBuilder();
            TestData.PrepareCodeToTest(sourceTest, out var root, out var model);

            var methodDeclaration = root.DescendantNodes(d => true)
              .First(s => s is MethodDeclarationSyntax)
              as MethodDeclarationSyntax;

            var methodDataCollector = new MethodDataCollector();
            methodDataCollector.GatherData(methodDeclaration, model);

            // Act

            var result = methodDataCollector.MethodUnderTest.Parameters.Select(p=>p.TypeSymbol.ToHumanTypeString()).ToArray();

            // Assert
            result[0].ShouldBe("byte[]");
            result[1].ShouldBe("List<int>");
            result[2].ShouldBe("Dictionary<string, DateTime>");
        }
    }
}