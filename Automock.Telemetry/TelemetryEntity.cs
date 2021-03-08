namespace Automock.Telemetry
{
    public class TelemetryEntity : UserReporEntityBase
    {        
        public TelemetryEntity(string instanceId) : base(instanceId)
        {
        }    

        public int ExecutionsCounter { get; set; } = 0;

        public int ClassGenerationRequest { get; set; } = 0;         
    }
}
