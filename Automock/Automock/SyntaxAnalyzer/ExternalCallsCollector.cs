using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Automock.SyntaxAnalyzer
{
    internal class ExternalCallsCollector : CSharpSyntaxWalker
    {
        public List<MethodData> ExternalCalls = new List<MethodData>();

        private ExternalCallsCollector()
        {
        }

        public ExternalCallsCollector(SemanticModel model)
        {
            SymanticModel = model;
        }

        public SemanticModel SymanticModel { get; }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // easy check if it's external call
            if (node.ChildNodes().Any(n => n is MemberAccessExpressionSyntax))
            {
                var symbolInfo = SymanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
                if (symbolInfo != null)
                {
                    ExternalCalls.Add(MethodData.Create(symbolInfo));
                }
            }

            base.VisitInvocationExpression(node);
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            //ExternalCalls.Add($"{node.Expression}.{node.Name}");
            base.VisitMemberAccessExpression(node);
        }
    }
}
