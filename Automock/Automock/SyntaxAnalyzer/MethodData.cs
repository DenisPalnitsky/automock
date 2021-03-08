using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Automock.SyntaxAnalyzer
{
    class MethodData
    {                        
        public string ClassName => ContainingClass.Name; 

        public string AssemblyName { get; private set; }

        public ITypeSymbol ContainingClass { get; private set; }

        public string MethodName { get; private set; }

        public List<ParameterData> Parameters { get; private set; }

        public string Namespace { get; private set; }

        public bool IsStatic { get; private set; }

        public ITypeSymbol ReturnType { get; private set; }

        private MethodData() { }


        public static MethodData Create(IMethodSymbol symbol)
        {
            var dependency = new MethodData()
            {
                Parameters = symbol.Parameters.Select(p => new ParameterData(p.Type, p.Name)).ToList()
            };

            dependency.ContainingClass = symbol.ContainingType;
            dependency.MethodName = symbol.Name;
            dependency.AssemblyName = symbol.ContainingAssembly.Name;
            dependency.Namespace = symbol.ContainingNamespace.Name;
            dependency.IsStatic = symbol.IsStatic;
            dependency.ReturnType = symbol.ReturnType;

            return dependency;
            
        }

        public static MethodData Create(MethodDeclarationSyntax declarationSyntax, SemanticModel model)
        {
            var methodData = new MethodData()
            {
                Parameters = new List<ParameterData>()
            };

            methodData.MethodName = declarationSyntax.Identifier.ToString();
            methodData.AssemblyName = "TODO: Define";
            methodData.Namespace = (declarationSyntax.Parent.Parent as NamespaceDeclarationSyntax)?.Name.ToString();

            var declarationTypeSyntax = declarationSyntax.Parent as TypeDeclarationSyntax;
            var declaredTypeInfo = model.GetDeclaredSymbol(declarationTypeSyntax) as INamedTypeSymbol;
            methodData.ContainingClass = declaredTypeInfo;
            methodData.ReturnType = null;      
            methodData.IsStatic = declarationSyntax.Modifiers.Any(m => m.ToFullString().Contains("static"));
            
            foreach (var parameterSyntax in declarationSyntax.ParameterList.Parameters)
            {
                var typeInfo = model.GetTypeInfo(parameterSyntax.Type);                
                methodData.Parameters.Add(new ParameterData(typeInfo.Type, parameterSyntax.Identifier.ToString() ));
            }

            return methodData;
        }

        public IEnumerable<string> GetAllRelatedNamespaces()
        {
            var result = new HashSet<string>()
            {
               Namespace
            };

            // containing namesapace can be null for basic types
            if (ReturnType != null && ReturnType.ContainingNamespace != null)
            {
                result.Add(ReturnType.ContainingNamespace.Name);
            }

            foreach (var paramNamespace in Parameters
                .Where(p=>p.TypeSymbol.ContainingNamespace != null)
                .Select(p => p.TypeSymbol.ContainingNamespace.Name))           
            {
                result.Add (paramNamespace);
            }

            return result;
            
        }
    }
}
