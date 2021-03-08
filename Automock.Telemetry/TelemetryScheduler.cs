using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automock.Telemetry
{
    public class TelemetryScheduler 
    {
        private static SolutionEvents _solutionEvents;
        private static Action _onVSClose;

        public static void RegisterOnCloseEvent(Action onVSClose)
        {
            var dte = (DTE)Package.GetGlobalService(typeof(DTE));


            _onVSClose = onVSClose;

            _solutionEvents = dte.Events.SolutionEvents;
            _solutionEvents.AfterClosing += _solutionEvents_AfterClosing;
        }

        private static void _solutionEvents_AfterClosing()
        {
            System.Diagnostics.Debug.WriteLine("AfterClosingSolution event fired");
            _onVSClose();
        }
    }
}
