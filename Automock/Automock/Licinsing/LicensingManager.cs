using Automock.UI;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Automock.Licinsing
{
    public class LicensingManager
    {
        private const string PublicRsaKey = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA3yAxKSaS919VTFxw2x1/
uWYO4syHQIF1n2oLobP2di4oVK0Vec3sAjtVD5h/Ju+vjRqIYzRJbNR0KQWkx+zI
YLHh9sxEkT9cKV28/4C3EcxYS+3BdAkHorUeq2WAv9fZ0wP+7VgrFhJm30Ef7wdf
jVV7oyk9NoNAV796qWRYd0Vt496EsncJlajIZJ0eVQ3fwP1BOYDO+d7BKiHMTwq6
SXfQQizL1n98I9gn8qiUjZWSLEupU7MpMuCl8bfZz+K8vSErKjcie9DK0SQi31Tn
Gyagf18JLK4YfBiO7/fRGo6UOHzc/JSFu2XCmYkqht8fYcxVuyCgQSXl0ClpaAbl
vwIDAQAB
-----END PUBLIC KEY-----";
        
        private bool? _isActivated;

        public string MessageWhenNotActive { get; private set; }

        static LicensingManager()
        {
        }


        public bool IsFirstTimeLicenseVerificationRequired()
        {
            return !_isActivated.HasValue;
        }
       
        public bool IsActivated()
        {
            if (!_isActivated.HasValue)
            {
                var isActivated = CanWeRunApplication();

                if (isActivated)
                {
                    _isActivated = true;
                }
                else
                {
                    _isActivated = false;
                }
            }

           
            return _isActivated.Value;
        }

        private bool CanWeRunApplication()
        {
            var httpClient = new HttpClient();
            var task = httpClient.GetStringAsync("https://automockstorage.blob.core.windows.net/$web/betaversionstate.json");
            System.Threading.Tasks.Task.WaitAll(task);
            var result= JObject.Parse(task.Result);
            MessageWhenNotActive = result.GetValue("MessageWhenNotActive").Value<string>();
            return result.GetValue("isBetaActive").Value<bool>();

        }
    }
}
