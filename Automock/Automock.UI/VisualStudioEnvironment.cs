using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automock.UI
{
    public class VisualStudioEnvironment
    {
        public static DTE DTE
        {
            get
            {
                return (DTE)Package.GetGlobalService(typeof(DTE));
            }   
        }
    }
}
