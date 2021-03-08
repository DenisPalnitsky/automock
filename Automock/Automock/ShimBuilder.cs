using Automock.SyntaxAnalyzer;
using Automock.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Automock
{
    class ShimBuilder
    {
        public ShimBuilder ()
        {            
        }

        public StatementSyntax BuildMethodMock(MethodData methodData)
        {
            var allParameters = new List<ParameterData>(methodData.Parameters);
            if (!methodData.IsStatic)
            {
                allParameters.Insert(0,  new ParameterData( methodData.ContainingClass, "instance"));
            }

            var parametersList = BuildLambdaParametersList(
                allParameters);            

            var returnStatement =  methodData.ReturnType.SpecialType != SpecialType.System_Void
                ? DefaultExpression(ParseTypeName(methodData.ReturnType.ToHumanTypeString()))
                : ParseExpression(" {}");
           
            var assignment = AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    BuildShimAccessExpression(methodData),
                    ParenthesizedLambdaExpression(parametersList, returnStatement));

            return SyntaxFactory.ExpressionStatement(assignment);
        }

        private static ParameterListSyntax BuildLambdaParametersList(
            IEnumerable<ParameterData> parameterTypes)
        {
            var counter = 1;
            var parametersList = parameterTypes.Select (t=> Parameter(Identifier("p"+counter++)));

            return ParameterList(
                    SeparatedList(
                          parametersList));
        }

        private MemberAccessExpressionSyntax BuildShimAccessExpression(MethodData methodData)
        {
            if (methodData.IsStatic)
            {
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("Shim" + methodData.ClassName),
                    IdentifierName(BuildShimMethodName(methodData)));
            }
            else
            {
                return
                     MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("Shim" + methodData.ClassName),
                            IdentifierName("AllInstances")),
                        IdentifierName(BuildShimMethodName(methodData)));                   
            }
        }

        private static string BuildShimMethodName(MethodData methodData)
        {
            var builder = new StringBuilder();
            builder.Append(methodData.MethodName);

            foreach (var parameter in methodData.Parameters)
            {
                builder.Append(parameter.TypeSymbol.Name);
            }

            return builder.ToString();
        }
    }
}
