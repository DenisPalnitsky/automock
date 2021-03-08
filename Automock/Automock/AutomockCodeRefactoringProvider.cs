using Automock.Licinsing;
using Automock.Telemetry;
using Automock.UI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;
using System.Threading.Tasks;
using System.Windows;

namespace Automock
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AutomockCodeRefactoringProvider)), Shared]
    internal class AutomockCodeRefactoringProvider : CodeRefactoringProvider
    {
        readonly TelemetryManager _telemetryManager = TelemetryManager.Create();

        public AutomockCodeRefactoringProvider()
        {
            TelemetryScheduler.RegisterOnCloseEvent(_telemetryManager.SendStats);
        }

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);           

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a type declaration node.
            var methodDeclaration = node as MethodDeclarationSyntax;
            if (methodDeclaration == null)
            {
                return;
            }

            // For any type declaration node, create a code action to reverse the identifier text.
            var action = new AutoMockCodeAction(
                context.Document, 
                methodDeclaration, 
                _telemetryManager,
                new LicensingManager());

            // Register this code action.
            context.RegisterRefactoring(action);
        }
    }
}
