using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace iEMB.Models
{
    public static class VersionChecker
    {
        private static bool _isInitializedPrior = false;

        public static async void InitializeVersionChecker()
        {
            VersionTracking.Track();

            if (_isInitializedPrior) return;

            LatestVersion = await GetLatestVersion();
            IsAutoUpdatePreference = await GetAutoUpdatePreference();
            IsInitializedPrior = true;
        }

        private static async Task<string> GetLatestVersion()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://api.github.com/repos/unidentifiedx/iemb-mobile/releases/latest");

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            var latestVersion = (string)json["name"];

            return latestVersion;
        }

        private static async Task<bool> GetAutoUpdatePreference()
        {
            var autoUpdate = (await SecureStorage.GetAsync("autoupdate")) == "true";

            return autoUpdate;
        }

        public static string LatestVersion { get; private set; }
        public static bool IsLatestVersion
        {
            get
            {
                return LatestVersion == VersionTracking.CurrentVersion;
            }
        }
        public static bool IsAutoUpdatePreference { get; private set; }
        public static bool IsInitializedPrior
        {
            get
            {
                return _isInitializedPrior;
            }
            
            private set
            {
                _isInitializedPrior = value;
            }
        }
    }
}