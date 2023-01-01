using Acr.UserDialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace iEMB.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private bool AutoUpdate;

        public SettingsPage()
        {
            InitializeComponent();

            RetrieveAutoUpdatePreference();
        }

        private async void RetrieveAutoUpdatePreference()
        {
            var autoUpdate = (await SecureStorage.GetAsync("autoupdate")) == "true";

            autoUpdateSwitch.IsToggled = autoUpdate;
            AutoUpdate = autoUpdate;
        }

        private async void ReportBug_Tapped(object sender, EventArgs e)
        {
            try
            {
                await Browser.OpenAsync("https://github.com/UnidentifiedX/iEMB-Mobile/issues", BrowserLaunchMode.SystemPreferred);
            }
            catch
            {

            }
        }

        private async void EmailOwner_Tapped(object sender, EventArgs e)
        {
            try
            {
                var message = new EmailMessage
                {
                    To = new List<string>()
                    {
                        "211530w@student.hci.edu.sg"
                    }
                };

                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException ex)
            {
                UserDialogs.Instance.Toast("Your default email app could not be opened. The email address has been copied to your clipboard");

                await Clipboard.SetTextAsync("211530w@student.hci.edu.sg");
            }
            catch
            {
                UserDialogs.Instance.Toast("Something went wrong. Please try again.");
            }
        }

        private async void AutoUpdateSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            AutoUpdate = !AutoUpdate;

            await SecureStorage.SetAsync("autoupdate", AutoUpdate.ToString().ToLower());
        }

        private async void CheckForUpdates_Clicked(object sender, EventArgs e)
        {
            var appVersion = VersionTracking.CurrentVersion;
            
            var client = new HttpClient();
            var response = await client.GetAsync("https://api.github.com/repos/unidentifiedx/iemb-mobile/releases/latest");

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            var latestVersion = (string)json["name"];

            if(latestVersion != appVersion)
            {
                var redirect = await DisplayAlert("New Version Found!", "A new version of the app has been found. Open link to download?", "Yes", "No");

                if(redirect)
                {
                    await Browser.OpenAsync("https://github.com/UnidentifiedX/iEMB-Mobile/releases/latest");
                }
            }
            else
            {
                await DisplayAlert("No New Version Found", "There is no new version found for the app. You're up to date!", "Nice");
            }
        }
    }
}