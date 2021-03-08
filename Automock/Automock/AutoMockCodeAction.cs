using Automock.Licinsing;
using Automock.SyntaxAnalyzer;
using Automock.Telemetry;
using Automock.Templates;
using Automock.UI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Task = System.Threading.Tasks.Task;

namespace Automock
{
    public class AutoMockCodeAction : CodeAction
    {
        private readonly Document _document;
        private readonly MethodDeclarationSyntax _methodDeclaration;
        private readonly TelemetryManager _telemetryManager;
        private readonly LicensingManager _licensingManager;
        private NotificarionBarManager _notificationManager = new NotificarionBarManager();
        private Document _addDocumentWithTest;
        

        public override string Title => "Generate Unit Test Template";

        public AutoMockCodeAction(
            Document document, 
            MethodDeclarationSyntax methodDeclaration, 
            TelemetryManager telemetryManager,
            LicensingManager licensingManager)
        {
            _document = document;
            _methodDeclaration = methodDeclaration;
            _telemetryManager = telemetryManager;
            _licensingManager = licensingManager;
        }

        protected async override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
        {
            _telemetryManager.LogExecution();

           if (!_licensingManager.IsActivated())
            {
                InvokeOnUiThread(() => _notificationManager.ShowWarning(_licensingManager.MessageWhenNotActive));
            }

            try
            {
                return await GetChangesSolutionAsyncInternal(cancellationToken);
            }
            catch (Exception ex)
            {
                TelemetryManager.SendCrashReport(ex.ToString());
                throw;
            }
        }

        private async Task<Solution> GetChangesSolutionAsyncInternal(CancellationToken cancellationToken)
        {
            var dataCollector = new MethodDataCollector();
            var semanticModel = await _document.GetSemanticModelAsync(cancellationToken);

            dataCollector.GatherData(_methodDeclaration, semanticModel);

            var methodName = _methodDeclaration.Identifier;
            var parentClass = _methodDeclaration.Parent as ClassDeclarationSyntax;
            if (parentClass == null)
            {
                throw new InvalidOperationException();
            }
            _addDocumentWithTest = null;

            try
            {
                var testDocumentProvider = new TestProjectProvider();
                   var newDocument = testDocumentProvider.GetDocumentForTest(
                    parentClass, 
                    _methodDeclaration.Identifier.ToString(),
                    _document,
                    out var isNewDocument);

                var unitTestGenerator = new TestClassGenerator(newDocument, isNewDocument);

                var testClassSourceText = unitTestGenerator.GenerateTest(                    
                        TestClass.Create(
                            newDocument,
                            TestClassNameBuilder.GetTestClassName(parentClass.Identifier.ToString()),
                            isNewDocument),                        
                        dataCollector.MethodUnderTest,
                        dataCollector.ExternalCalls);

                newDocument = newDocument.WithText(testClassSourceText);
                _addDocumentWithTest = newDocument;

                return newDocument.Project.Solution;
            }
            catch (NotSupportedException)
            {
                _telemetryManager.LogClassGenerationRequest();

                InvokeOnUiThread(() => _notificationManager.ShowWarning(
                    "No suitable test - project found.Automock - beta can't create projects. Create empty test project with name {ProjectName}.Test and try generation again"));


                return _document.Project.Solution;
            }
        }        

        protected override Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(
               CancellationToken cancellationToken)
        {
            return Task.FromResult(Enumerable.Empty<CodeActionOperation>());
        }

        protected override Task<Document> PostProcessChangesAsync(Document document, CancellationToken cancellationToken)
        {
            if (_addDocumentWithTest != null)
            {
                Application.Current.Dispatcher.InvokeAsync(
                    () => {                   
                        document.Project.Solution.Workspace.OpenDocument(_addDocumentWithTest.Id);
                    });
            }          

            return base.PostProcessChangesAsync(document, cancellationToken);
        }

        private async void InvokeOnUiThread(Action action)
        {
            await Application.Current.Dispatcher.InvokeAsync(
                  () =>
                  {
                      action();
                  });
        }
    }
}
