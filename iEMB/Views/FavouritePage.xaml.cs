using Acr.UserDialogs;
using iEMB.Models;
using iEMB.ViewModels;
using iEMB.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            var starredData = await GetPageData(verificationToken, sessionID, authenticationToken, boardID: 1048, pageNumber: 1);

            if(starredData == null)
            {
                UserDialogs.Instance.Toast("Unable to fetch starred announcements.");
                return;
            }

            var json = JObject.Parse(starredData);
            var announcementJson = json["data"]; 
            var announcementList = new List<Announcement>();
            
            foreach (var announcement in announcementJson)
            {
                announcementList.Add(new Announcement
                {
                    PostDate = announcement["posttime"].ToString(),
                    Sender = announcement["postby"].ToString(),
                    Username = null,
                    Subject = announcement["title"].ToString(),
                    Url = $"/Board/Content/{announcement["id"]}?board={announcement["boardId"]}&isArchived={announcement["isArchived"]}",
                    BoardID = announcement["boardId"].ToString(),
                    Pid = announcement["id"].ToString(),
                    Priority = null,
                    Recepients = announcement["groupName"].ToString(),
                    ViewCount = int.Parse(announcement["viewer"].ToString()),
                    ReplyCount = null,
                    IsRead = true,
                    IsArchived = announcement["isArchived"].ToString() == "True",
                });
            }

            Console.WriteLine(json["paging"]["TotalPage"]);
            LoadAnnouncements(announcementList);
        }

        public static ObservableCollection<Announcement> StarredAnnouncements = new ObservableCollection<Announcement>();

        private void LoadAnnouncements(List<Announcement> starredAnnouncements)
        {
            StarredAnnouncements.Clear();

            foreach (var announcement in starredAnnouncements)
            {
                StarredAnnouncements.Add(announcement);
            }

            loadingIcons.IsVisible = false;
            //pageIndex.IsVisible = true;
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
                var reader = new StreamReader(response.GetResponseStream());

                return WebUtility.HtmlDecode(reader.ReadToEnd());
            } 
            catch
            {
                return null;
            }
        }

        private async void Announcement_Tapped(object sender, EventArgs e)
        {
            var announcementSender = (StackLayout)sender;
            var pid = ((Announcement)announcementSender.BindingContext).Pid;
            var announcement = StarredAnnouncements.Where(a => a.Pid == pid).FirstOrDefault();

            await Navigation.PushAsync(new AnnouncementDetailPage(announcement));
        }
    }
}