using DevMateKit.DMActivation.UI;
using DevMateKit.DMFramework;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Automock.UI
{
    public class InfoBarService : IVsInfoBarUIEvents
    {
        private uint _cookie;

        public static InfoBarService Instance { get; private set; }


        public void OnClosed(IVsInfoBarUIElement infoBarUIElement)
        {
            infoBarUIElement.Unadvise(_cookie);
        }

        public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
        {
            string context = (string)actionItem.ActionContext;

            if (context == "yes")
            {
                DMFrameworkSettings.TrialType = new TrialType() { DMTrialDay = true };
                // move this to UI assembly
                var window = new DMActivationWindow(true);
                //window.BigIconSource = new BitmapImage();
                window.ShowDialog();
                var dialogResult = window.DialogResult;             
            }
            else
            {
                MessageBox.Show("You clicked No!");
            }
        }

        public void ShowInfoBar(string message)
        {            
            var shell = Package.GetGlobalService(typeof(SVsShell)) as IVsShell;
            if (shell != null)
            {
                shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var obj);
                var host = (IVsInfoBarHost)obj;

                if (host == null)
                {
                    return;
                }
                InfoBarTextSpan text = new InfoBarTextSpan(message);
                InfoBarHyperlink yes = new InfoBarHyperlink("Yes", "yes");
                InfoBarHyperlink no = new InfoBarHyperlink("No", "no");

                InfoBarTextSpan[] spans = new InfoBarTextSpan[] { text };
                InfoBarActionItem[] actions = new InfoBarActionItem[] { yes, no };
                InfoBarModel infoBarModel = new InfoBarModel(spans, actions, KnownMonikers.StatusInformation, isCloseButtonVisible: true);
                
                var factory = Package.GetGlobalService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
                IVsInfoBarUIElement element = factory.CreateInfoBar(infoBarModel);
                element.Advise(this, out _cookie);
                host.AddInfoBar(element);
            }
        }

        public static void Initialize()
        {
            Instance = new InfoBarService();
        }
    }
}
