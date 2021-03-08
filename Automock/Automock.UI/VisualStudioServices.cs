using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Automock.UI
{
    public class VisualStudioServices
    {
        private static readonly Dictionary<Type, object> _serviceCache = new Dictionary<Type, object>();
        private static System.IServiceProvider _serviceProvider;
        public static string VisualStudioDevEnvPath
        {
            get
            {
                return VisualStudioServices.Dte.FullName;
            }
        }

        public static DTE Dte
        {
            get
            {
                return VisualStudioServices.GetService<SDTE, DTE>();
            }
        }

        public static IVsUIShell VsUiShell
        {
            get
            {
                return VisualStudioServices.GetService<SVsUIShell, IVsUIShell>();
            }
        }

        public static IVsShell VsShell
        {
            get
            {
                return VisualStudioServices.GetService<SVsShell, IVsShell>();
            }
        }

        public static IVsMonitorSelection MonitorSelection
        {
            get
            {
                return VisualStudioServices.GetService<SVsShellMonitorSelection, IVsMonitorSelection>();
            }
        }


        public static IVsSolution VsSolution
        {
            get
            {
                return VisualStudioServices.GetService<SVsSolution, IVsSolution>();
            }
        }

        public static void Initialize(System.IServiceProvider package)
        {
            if (package == null)
                throw new ArgumentNullException("");
            VisualStudioServices._serviceProvider = package;
        }

        public static TReturnType GetService<TRequestType, TReturnType>() where TReturnType : class
        {
            if (VisualStudioServices._serviceProvider == null)
                throw new InvalidOperationException();
            var index = typeof(TRequestType);
            if (!VisualStudioServices._serviceCache.ContainsKey(index))
            {
                object service = VisualStudioServices._serviceProvider.GetService(index);
                if (service == null)
                    throw new ExternalException("Can't get service " + index);
                VisualStudioServices._serviceCache[index] = service;
            }
            TReturnType returnType = VisualStudioServices._serviceCache[index] as TReturnType;
            if ((object)returnType == null)
                throw new InvalidOperationException("");
            else
                return returnType;
        }

        public static IVsInfoBarUIFactory InfoBarUIFactory
        {
            get
            {
                return GetService<SVsInfoBarUIFactory, IVsInfoBarUIFactory>();
            }
        }

        //public static void AddWatch(string watchText)
        //{
        //    watchText = watchText.Replace(Environment.NewLine, "\n");
        //    // ISSUE: reference to a compiler-generated method
        //    VisualStudioServices.Dte.ExecuteCommand("AddWatch", watchText);
        //}

        //public static IVsHierarchy GetHierarchy(Project project)
        //{
        //    IVsHierarchy ppHierarchy;
        //    ErrorHandler.ThrowOnFailure(VisualStudioServices.GetService<SVsSolution, IVsSolution>().GetProjectOfUniqueName(project.UniqueName, out ppHierarchy));
        //    return ppHierarchy;
        //}

        //public static ProjectItem ProjectItemFromWindowSafely(Window window)
        //{
        //    if (window == null)
        //        throw new ArgumentNullException();
        //    try
        //    {
        //        return window.ProjectItem;
        //    }
        //    catch (InvalidCastException)
        //    {
        //        return (ProjectItem)null;
        //    }
        //}

        //public static void ExecuteCommand(VsCommandIdentifier cmd)
        //{
        //    object obj = (object)null;
        //    VisualStudioServices.Dte.Commands.Raise(cmd.GUID, cmd.Index, ref obj, ref obj);
        //}
    }
}
