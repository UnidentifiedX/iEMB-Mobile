using Acr.UserDialogs;
using iEMB.Models;
using iEMB.ViewModels;
using iEMB.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace iEMB.Views
{
    public partial class FavouritePage : ContentPage
    {
        protected override void OnAppearing()
        {
            GetStarredAnnouncements(LoginPage.VerificationToken, LoginPage.SessionID, LoginPage.AuthenticationToken);
        }

        public FavouritePage()
        {
            InitializeComponent();
            BindingContext = new FavouriteViewModel();
        }

        private async void GetStarredAnnouncements(string verificationToken, string sessionID, string authenticationToken)
        {
            var starredData = await GetPageData(verificationToken, sessionID, authenticationToken, 1048, 1);

            if(starredData == null)
            {
                UserDialogs.Instance.Toast("Unable to fetch starred announcements.");
            }

            Console.WriteLine(starredData);
        }

        private static async Task<string> GetPageData(string verificationToken, string sessionID, string authenticationToken, int boardID, int pageNumber)
        {
            try
            {
                var postData = $"id={boardID}&page={pageNumber}";
                var postDataByteArray = Encoding.ASCII.GetBytes(postData);

                var cookieContainer = new CookieContainer();
                var request = (HttpWebRequest)WebRequest.Create("https://iemb.hci.edu.sg/Board/FavouriteList");
                cookieContainer.Add(new Uri("https://iemb.hci.edu.sg/"), new Cookie("__RequestVerificationToken", verificationToken));
                cookieContainer.Add(new Uri("https://iemb.hci.edu.sg/"), new Cookie("ASP.NET_SessionId", sessionID));
                cookieContainer.Add(new Uri("https://iemb.hci.edu.sg/"), new Cookie("AuthenticationToken", authenticationToken));
                request.CookieContainer = cookieContainer;
                request.Method = "POST";
                request.Host = "iemb.hci.edu.sg";
                request.Referer = "https://iemb.hci.edu.sg/";
                request.Headers.Add("Origin", "https://iemb.hci.edu.sg");
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Mobile Safari/537.36";
                var bodyStream = request.GetRequestStream();
                bodyStream.Write(postDataByteArray, 0, postDataByteArray.Length);


                var response = (HttpWebResponse)await request.GetResponseAsync();
                var reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII);

                return reader.ReadToEnd();
            } 
            catch
            {
                return null;
            }
        }
    }
}