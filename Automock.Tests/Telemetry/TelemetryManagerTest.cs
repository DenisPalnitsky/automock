using Automock.Telemetry;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automock.Tests.Telemetry
{
    [TestFixture]
    public class TelemetryManagerTest
    {
        [Test]
        [Category("Integration")]
        public void SendStats_SendStsats()
        {
            var t = TelemetryManager.Create();
            t.LogExecution();
            t.LogClassGenerationRequest();
            t.LogClassGenerationRequest();
            t.SendStats();
            System.Threading.Thread.Sleep(1000);
        }

        [Test]
        [Category("Integration")]
        public void SendStats_SendCrashReport()
        {
            var t = TelemetryManager.Create();
            TelemetryManager.SendCrashReport(new NotImplementedException().ToString());
            System.Threading.Thread.Sleep(1000);
        }
    }
}
