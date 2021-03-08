using System;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AUTOMOCK_TEMPLATE_NamespaceName
{
    [TestClass]
    public class AUTOMOCK_TEMPLATE_TestClassName
    {
        private IDisposable _shimContext;

        [TestInitialize]
        public void Initialize()
        {
            _shimContext = ShimsContext.Create();
        }

        [TestCleanup]
        public void CleanUp()
        {
            _shimContext.Dispose();
        }


    }
}