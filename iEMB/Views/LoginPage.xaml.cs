using iEMB.ViewModels;
using System;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Acr.UserDialogs;

namespace iEMB.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel();

            LoadSavedData();
        }

        private async void LoadSavedData()
        {
            try
            {
                var id = await SecureStorage.GetAsync("userid");
                var password = await SecureStorage.GetAsync("password");

                if (id != "" && password != "")
                {
                    UserDialogs.Instance.Toast("Successfully loaded user data!");
                    CheckLoginCredentials(id, password);
                }
            }
            catch
            {
                UserDialogs.Instance.Toast("Something went wrong loading user data");
                loginButton.IsEnabled = true;
            }
        }

        private void Login(object sender, EventArgs e)
        {
            CheckLoginCredentials(idField.Text, passwordField.Text);
        }

        private async void CheckLoginCredentials(string id, string password)
        {
            ToastConfig.DefaultPosition = ToastPosition.Bottom;
            var rememberMe = rememberMeCheckbox.IsChecked;

            loginButton.IsEnabled = false;
            loadingBar.IsRunning = true;
            if (id == null || password == null)
            {
                errorMsg.IsVisible = true;
                loadingBar.IsRunning = false;
                loginButton.IsEnabled = true;
                return;
            }

            var request = (HttpWebRequest)WebRequest.Create("https://iemb.hci.edu.sg");
            request.CookieContainer = new CookieContainer();
            var response = (HttpWebResponse)await request.GetResponseAsync();

            var dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            var content = reader.ReadToEnd();

            var verificationCookie = response.Cookies[1].Value;
            var verificationToken = Regex.Match(content, @"(?<=<input name=""__RequestVerificationToken"" type="".+?"" value="").*").Value;
            verificationToken = verificationToken.Substring(0, verificationToken.Length - 5);

            var encodedUserId = Uri.EscapeDataString(id);
            var encodedPassword = Uri.EscapeDataString(password);
            var encodedToken = Uri.EscapeDataString(verificationToken);

            var postData = $"UserName={encodedUserId}&Password={encodedPassword}&__RequestVerificationToken={encodedToken}&submitbut=Submit";
            var postDataByteArray = Encoding.ASCII.GetBytes(postData);
            var cookieContainer = new CookieContainer();
            request = (HttpWebRequest)WebRequest.Create("https://iemb.hci.edu.sg/home/logincheck");
            cookieContainer.Add(new Uri("https://iemb.hci.edu.sg/home/logincheck"), new Cookie("__RequestVerificationToken", verificationCookie));
            request.CookieContainer = cookieContainer;
            request.Method = "POST";
            request.Host = "iemb.hci.edu.sg";
            request.Referer = "https://iemb.hci.edu.sg/";
            request.Headers.Add("Origin", "https://iemb.hci.edu.sg");
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Mobile Safari/537.36";
            Stream bodyStream = request.GetRequestStream();
            bodyStream.Write(postDataByteArray, 0, postDataByteArray.Length);
            dataStream.Close();
            request.AllowAutoRedirect = false;

            response = (HttpWebResponse)await request.GetResponseAsync();

            try
            {
                var sessionID = response.Cookies[1].Value;
                var authenticationToken = response.Cookies[2].Value;

                if (rememberMe)
                { 
                    try
                    {
                        await SecureStorage.SetAsync("userid", id);
                        await SecureStorage.SetAsync("password", password);
                        UserDialogs.Instance.Toast("Saved user data!");
                    }
                    catch
                    {
                        UserDialogs.Instance.Toast("Something went wrong. Please try again later.");
                    }
                }

                loadingBar.IsRunning = false;

                Application.Current.MainPage = new AppShell();
                await Navigation.PushAsync(new AnnouncementPage(verificationToken, sessionID, authenticationToken));

                loginButton.IsEnabled = true;
            }
            catch
            {
                loadingBar.IsRunning = false;
                loginButton.IsEnabled = true;
                errorMsg.IsVisible = true;
            }
        }
    }
}