using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace iEMB.Views
{
    public partial class AnnouncementPage : ContentPage
    {
        public AnnouncementPage()
        {
            InitializeComponent();
        }

        public AnnouncementPage(string verificationToken, string sessionID, string authenticationToken)
        {
            InitializeComponent();
            LoadAnnouncements(verificationToken, sessionID, authenticationToken);
        }

        private async void LoadAnnouncements(string verifictionToken, string sessionID, string authenticationToken)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://iemb.hci.edu.sg/Board/Detail/1048");
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("https://iemb.hci.edu.sg/Board/Detail/1048"), new Cookie("__RequestVerificationToken", verifictionToken));
            cookieContainer.Add(new Uri("https://iemb.hci.edu.sg/Board/Detail/1048"), new Cookie("ASP.NET_SessionId", sessionID));
            cookieContainer.Add(new Uri("https://iemb.hci.edu.sg/Board/Detail/1048"), new Cookie("AuthenticationToken", authenticationToken));
            request.Method = "GET";
            request.CookieContainer = cookieContainer;
            request.Host = "iemb.hci.edu.sg";
            request.Referer = "https://iemb.hci.edu.sg/";
            request.Headers.Add("origin", "https://iemb.hci.edu.sg");
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Mobile Safari/537.36";

            var response = (HttpWebResponse)await request.GetResponseAsync();
            var dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            var content = reader.ReadToEnd();

            Console.WriteLine(content);
        }

        private void LogoutButton_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new LoginPage();
        }
    }
}