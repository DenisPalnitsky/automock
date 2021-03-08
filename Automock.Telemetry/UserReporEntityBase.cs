using System;
using System.Reflection;

namespace Automock.Telemetry
{
    public abstract class UserReporEntityBase
    {
        public UserReporEntityBase(string instanceId)
        {
            this.InstanceId = instanceId;
        }

        public string InstanceId { get; private set; }

        public string SessinonId() => Guid.NewGuid().ToString();

        public string Timestemp => DateTime.Now.ToString("yyyyMMdd-HH:mm");

        public string CurrentVersion => typeof(UserReporEntityBase).Assembly.GetName().Version.ToString();
        
    }
}