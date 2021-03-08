using Automock.Licinsing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automock.Tests
{
    [TestFixture]
    public class LicensingManagerTests
    {
        [Test]
        public void IsActivated_returnsTrue_whenServiceReturnsTrus()
        {
            var l = new LicensingManager();
            Assert.IsTrue(l.IsActivated());
        }
    }
}
