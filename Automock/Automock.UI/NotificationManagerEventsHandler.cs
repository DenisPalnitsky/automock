using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automock.UI
{
    internal class NotificationManagerEventsHandler : IVsInfoBarUIEvents
    {
        private uint _coockie;

        public void OnClosed(IVsInfoBarUIElement infoBarUIElement)
        {
            infoBarUIElement.Unadvise(_coockie);
        }

        public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
        {
          
        }

        internal void RegisterCoockie(uint coockie)
        {
            _coockie = coockie;
        }
    }
}
