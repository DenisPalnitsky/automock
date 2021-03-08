using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio;
using EnvDTE;

namespace Automock.UI
{
    public class NotificarionBarManager 
    {
        private IVsInfoBarHost _host;
        private readonly NotificationInfoBarFactory _notificationInfoBarFactory = new NotificationInfoBarFactory();
        private readonly object _lock = new object();

        IVsInfoBarHost Host
        {
            get
            {
                lock (_lock)
                {
                    if (_host == null)
                    {
                        _host = GetInfoBarHost();
                    }
                }
                return _host;
            }
        }

        public void ShowWarning(string text)
        {
            var infoBar = _notificationInfoBarFactory.GetWarningInfoBar(text);

            Host.RemoveInfoBar(infoBar);
            Host.AddInfoBar(infoBar);
        }

        public void ShowBuyLicenseMessage()
        {
            var infoBar = _notificationInfoBarFactory.GetBuyLicenseInfoBar();

            Host.RemoveInfoBar(infoBar);
            Host.AddInfoBar(infoBar);
        }    

        private IVsInfoBarHost GetInfoBarHost()
        {
            var shell = Package.GetGlobalService(typeof(SVsShell)) as IVsShell;
            if (shell != null)
            {
                shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var obj);
                var host = (IVsInfoBarHost)obj;
                if (host == null)
                {
                    return null;
                }

                return host;
            }
            return null;
        }

    }
}
