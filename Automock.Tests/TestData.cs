using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automock.Tests
{
    public class TestData
    {
        public const string GenericMethodCode = @"
using System;
namespace MyNamespace
{
    class C
    {
        public DateTime GetDateTime(bool isNotNow)
        {    
            var useless = DateTime.Parse(""10102018"");
            if (p)
                return DateTime.Parse(""10102018"");
            else
                return DateTime.Now;
        }    
    }
}";

        public static void PrepareCodeToTest(string code, out CompilationUnitSyntax root, out SemanticModel model)
        {
            PrepareTestCodeInternal(out root, out model, code);
        }

        private static void PrepareTestCodeInternal(out CompilationUnitSyntax root, out SemanticModel model, 
            string initialCode)
        {
            var tree = CSharpSyntaxTree.ParseText(initialCode);
            root = (CompilationUnitSyntax)tree.GetRoot();
            var compilation = CSharpCompilation.Create("HelloWorld")
                    .AddReferences(MetadataReference.CreateFromFile(
                        typeof(string).Assembly.Location))
                    .AddSyntaxTrees(tree);

            model = compilation.GetSemanticModel(tree);
        }
    }
}
