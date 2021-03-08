using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automock.UI
{
    internal class NotificationInfoBarFactory
    {
        IVsInfoBarUIElement _warningInfoBar;
        IVsInfoBarUIElement _buyLicenseInfoBar;
        object _lock = new object();


        public IVsInfoBarUIElement GetWarningInfoBar(string text)
        {         
            lock (_lock)
            {
                if (_warningInfoBar == null)
                {
                    _warningInfoBar =
                        CreateWarningInfoBar(
                        text);
                }
            }

            return _warningInfoBar;
        }

        public IVsInfoBarUIElement GetBuyLicenseInfoBar()
        {
            lock (_lock)
            {
                if (_buyLicenseInfoBar == null)
                {
                    _buyLicenseInfoBar =
                       CreateBuyLicenseInfoBar();
                }
            }
            return _buyLicenseInfoBar;
        }

        private static IVsInfoBarUIElement CreateBuyLicenseInfoBar()
        {
            var infoBar = new InfoBarModel(
                              textSpans: new[]
                              {
                            new InfoBarTextSpan("This instance is of Automock is not activated. Consider buying license" ),
                                  //new InfoBarHyperlink("hyperlink"),
                                  //new InfoBarTextSpan(" InfoBar.")
                              },
                              actionItems: new[]
                              {
                                     new InfoBarButton("Buy")
                              },
                              image: KnownMonikers.StatusInformation,
                              isCloseButtonVisible: true);



            if (!TryCreateInfoBarUI(infoBar, out var uiElement))
            {
                throw new InvalidOperationException("Can't create info bar");
            }

            var eventSink = new NotificationManagerEventsHandler();
            uiElement.Advise(eventSink, out var coockie);
            eventSink.RegisterCoockie(coockie);

            return uiElement;
        }


        private static IVsInfoBarUIElement CreateWarningInfoBar(string message)
        {

            var infoBar = new InfoBarModel(
                                 textSpans: new[]
                                 {
                            new InfoBarTextSpan(message),
                                     //new InfoBarHyperlink("hyperlink"),
                                     //new InfoBarTextSpan(" InfoBar.")
                                 },
                                 //actionItems: new[]
                                 //{
                                 //       new InfoBarButton("Click Me")
                                 //},
                                 image: KnownMonikers.StatusInformation,
                                 isCloseButtonVisible: true);

            if (!TryCreateInfoBarUI(infoBar, out var uiElement))
            {
                throw new InvalidOperationException("Can't create info bar");
            }

            return uiElement;
        }

        private static bool TryCreateInfoBarUI(IVsInfoBar infoBar, out IVsInfoBarUIElement uiElement)
        {
            var infoBarUIFactory = Package.GetGlobalService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
            if (infoBarUIFactory == null)
            {
                uiElement = null;
                return false;
            }

            uiElement = infoBarUIFactory.CreateInfoBar(infoBar);
            return uiElement != null;
        }
    }
}
