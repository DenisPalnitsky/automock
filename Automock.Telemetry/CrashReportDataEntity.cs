namespace Automock.Telemetry
{
    class CrashReportDataEntity : UserReporEntityBase
    {
        public string CrashReportContent { get; private set; }
        public CrashReportDataEntity(string instanceId, string crashReportContent) : base(instanceId)
        {
            CrashReportContent = crashReportContent;
        }
    }
}
