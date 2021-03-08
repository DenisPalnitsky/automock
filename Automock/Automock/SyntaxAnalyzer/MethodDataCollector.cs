using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Automock.SyntaxAnalyzer
{
    class MethodDataCollector
    {        
        public IEnumerable<MethodData> ExternalCalls { get; private set; }
        public MethodData MethodUnderTest { get; private set; }

        public void GatherData(MethodDeclarationSyntax methodDeclaration, SemanticModel model)
        {
            var externalCallsCollector = new ExternalCallsCollector(model);
            MethodUnderTest = MethodData.Create(methodDeclaration, model);

            externalCallsCollector.Visit(methodDeclaration);
            ExternalCalls = externalCallsCollector.ExternalCalls;            
        }
    }
}
