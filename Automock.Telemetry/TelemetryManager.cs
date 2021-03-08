using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Automock.Telemetry
{
    public class TelemetryManager
    {
        const string StatsCollection = "usage_stats";
        const string CrashReportCollection = "crash_reports";
        const string MongoUrlTemplate = 
            @"https://api.mlab.com/api/1/databases/automock_telemetry/collections/{0}?apiKey=c-2kRildK_6FPV6zJu5ZL7h3Imoh0xAA";


        private TelemetryEntity _telemetryEntity;

        public TelemetryManager(string instanceId)
        {
            _telemetryEntity = new TelemetryEntity(instanceId);
        }

        public static TelemetryManager Create()
        {
            return new TelemetryManager( GetInstanceId());
        }

        private static string GetInstanceId()
        {
            return
#if !DEBUG
            Environment.MachineName.GetHashCode().ToString()
#else
            $"DEBUG_ {Environment.MachineName.GetHashCode().ToString()}"
#endif
;
        }


        public static async void SendCrashReport(string exception)
        {
            var crashReportItem = new CrashReportDataEntity(GetInstanceId(), exception);
            var stats = Serialize(crashReportItem);
            await SendUserDataEntity(stats, string.Format(MongoUrlTemplate, CrashReportCollection));
        }

        public async void SendStats()
        {
            var stats = Serialize(_telemetryEntity);
            await SendUserDataEntity(stats, string.Format(MongoUrlTemplate, StatsCollection));
            _telemetryEntity = new TelemetryEntity(_telemetryEntity.InstanceId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializedContent">String in JSon format </param>
        /// <param name="url"></param>
        private static async Task SendUserDataEntity(string serializedContent, string url)
        {
            var client = new HttpClient();
            var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
            try
            {

#if !DEBUG
                await client.PostAsync(url, content);
                System.Diagnostics.Debug.WriteLine("SendStats executed");
#else
                var task = client.PostAsync(url, content);
                Task.WaitAll(task);
                var t = task.Result.Content.ReadAsStringAsync();
                Task.WaitAll(t);
                System.Diagnostics.Debug.Write(t.Result);
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to send crash report: " +ex.ToString());
            }
        }


        private static string Serialize(UserReporEntityBase telemetryEntity)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(telemetryEntity);
        }

        public void LogExecution()
        {
            _telemetryEntity.ExecutionsCounter++;
        }

        public void LogClassGenerationRequest()
        {
            _telemetryEntity.ClassGenerationRequest++;
        } 
    }
}