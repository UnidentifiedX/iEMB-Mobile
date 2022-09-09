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
        private static readonly int maxPageButtonCount = 5;
        private static int currentSelectedPage = 1;

        protected override void OnAppearing()
        {
            GetStarredAnnouncements(LoginPage.VerificationToken, LoginPage.SessionID, LoginPage.AuthenticationToken, pageNumber: 1);
        }

        public FavouritePage()
        {
            InitializeComponent();
            BindingContext = new FavouriteViewModel();
        }

        private async void GetStarredAnnouncements(string verificationToken, string sessionID, string authenticationToken, int pageNumber)
        {
            var starredData = await GetPageData(verificationToken, sessionID, authenticationToken, boardID: 1048, pageNumber: pageNumber);

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

            LoadAnnouncements(announcementList);
            Console.WriteLine(json["paging"]["TotalPage"].ToString() + " balls");
            LoadPageIcons(int.Parse(json["paging"]["TotalPage"].ToString()));
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
        }

        private void LoadPageIcons(int totalPages)
        {
            pageIndex.Children.Clear();
            pageIndex.IsVisible = true;

            if (totalPages <= maxPageButtonCount)
            {
                for(int i = 1; i <= totalPages; i++)
                {
                    pageIndex.Children.Add(
                            CreatePageButton(text: i.ToString(), marginRight: totalPages % maxPageButtonCount != 0 ? 10 : 0, backgroundColor: i == currentSelectedPage ? Color.Red : Color.Transparent)
                        );
                }
            }
            else
            {
                for(int i = 1; i <= maxPageButtonCount; i++)
                {
                    pageIndex.Children.Add(
                            CreatePageButton(text: i.ToString(), marginRight: totalPages % maxPageButtonCount != 0 ? 10 : 0, backgroundColor: i == currentSelectedPage ? Color.Red : Color.Transparent)
                        );
                }

                pageIndex.Children.Add(
                        new Label
                        {
                            Text = "...",
                            TextColor = Color.White,
                            Padding = new Thickness(0, 10, 0, 0),
                        }
                    );

                pageIndex.Children.Add(
                    CreatePageButton(text: totalPages.ToString(), marginRight: totalPages % maxPageButtonCount != 0 ? 10 : 0, backgroundColor: currentSelectedPage == totalPages ? Color.Red : Color.Transparent)
                );
            }
        }

        private Button CreatePageButton(string text, int marginRight, Color backgroundColor)
        {
            var button = new Button
            {
                Text = text,
                WidthRequest = 40,
                HeightRequest = 40,
                CornerRadius = 100,
                Margin = new Thickness(0, 0, marginRight, 0),
                BackgroundColor = backgroundColor,
            };

            button.Clicked += (s, _) =>
            {
                StarredAnnouncements.Clear();

                var clickedPage = int.Parse(((Button)s).Text);
                currentSelectedPage = clickedPage;
                loadingIcons.IsVisible = true;
                pageIndex.IsVisible = false;

                GetStarredAnnouncements(LoginPage.VerificationToken, LoginPage.SessionID, LoginPage.AuthenticationToken, pageNumber: clickedPage);
            };

            return button;
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