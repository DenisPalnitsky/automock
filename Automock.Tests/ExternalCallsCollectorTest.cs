using Automock;
using Automock.SyntaxAnalyzer;
using Automock.Tests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using Shouldly;
using System;
using System.Linq;

namespace Automock.Tests
{
    [TestFixture]
    public class ExternalCallsCollectorTest
    {        
        [Test]
        public void Test()
        {
            // Arragne 
            TestData.PrepareCodeToTest(TestData.GenericMethodCode, out var root, out var model);
            var externalCallsCollector = new ExternalCallsCollector(model);

            // Act
            externalCallsCollector.Visit(root);

            // Assert
            externalCallsCollector.ExternalCalls.Count.ShouldBe(2);
        }          
    }
}
