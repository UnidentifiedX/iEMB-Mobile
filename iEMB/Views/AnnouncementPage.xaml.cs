using HtmlAgilityPack;
using iEMB.Models;
using iEMB.ViewModels;
using Newtonsoft.Json;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace iEMB.Views
{
    public partial class AnnouncementPage : ContentPage
    {

        public AnnouncementPage()
        {
            BindingContext = new AnnouncementViewModel();
            GetAnnouncements(LoginPage.VerificationToken, LoginPage.SessionID, LoginPage.AuthenticationToken);
            InitializeComponent();
        }

        private async void GetAnnouncements(string verificationToken, string sessionID, string authenticationToken)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://iemb.hci.edu.sg/Board/Detail/1048");
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("https://iemb.hci.edu.sg/Board/Detail/1048"), new Cookie("__RequestVerificationToken", verificationToken));
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

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var unreadMessages = doc.DocumentNode.SelectNodes("//tbody")[0]?.SelectNodes("tr");
            var hasUnreadMessages = !(unreadMessages.FirstOrDefault().InnerText.Trim() == "No Record Found!");

            if(!hasUnreadMessages)
            {
                noUnreadAnnouncements.IsVisible = true;
            }

            var readMessages = doc.DocumentNode.SelectNodes("//tbody")[1].SelectNodes("tr");
            var unreadAnnouncements = new List<Announcement>();
            var readAnnouncements = new List<Announcement>();

            await Task.Run(() =>
            {
                if(hasUnreadMessages)
                {
                    foreach (var message in unreadMessages)
                    {
                        var data = message.SelectNodes("td");

                        var announcement = new Announcement
                        {
                            PostDate = Regex.Replace(data[0].InnerText, @"\s+", ""),
                            Sender = data[1].SelectSingleNode("a").Attributes["tooltip-data"].Value,
                            Username = data[1].SelectSingleNode("a").InnerText.Trim(),
                            Subject = WebUtility.HtmlDecode(data[2].SelectSingleNode("a").InnerText),
                            Url = data[2].SelectSingleNode("a").Attributes["href"].Value,
                            BoardID = "1024",
                            Pid = Regex.Match(data[2].SelectSingleNode("a").Attributes["href"].Value, @"/Board/content/(\d+)").Groups[1].Value,
                            Priority = data[3].SelectSingleNode("img").Attributes["alt"].Value,
                            Recepients = Regex.Replace(data[4].InnerText.Trim(), @"\s", " ").Trim(),
                            ViewCount = int.Parse(Regex.Match(data[5].InnerText, @"Viewer:\s+(\d+)").Groups[1].Value),
                            ReplyCount = int.Parse(Regex.Match(data[5].InnerText, @"Response:\s+(\d+)").Groups[1].Value),
                            IsRead = false
                        };

                        unreadAnnouncements.Add(announcement);
                    }
                }

                foreach (var message in readMessages)
                {
                    var data = message.SelectNodes("td");

                    var announcement = new Announcement
                    {
                        PostDate = Regex.Replace(data[0].InnerText, @"\s+", ""),
                        Sender = data[1].SelectSingleNode("a").Attributes["tooltip-data"].Value,
                        Username = data[1].SelectSingleNode("a").InnerText.Trim(),
                        Subject = WebUtility.HtmlDecode(data[2].SelectSingleNode("a").InnerText),
                        Url = data[2].SelectSingleNode("a").Attributes["href"].Value,
                        BoardID = "1024",
                        Pid = Regex.Match(data[2].SelectSingleNode("a").Attributes["href"].Value, @"/Board/content/(\d+)").Groups[1].Value,
                        Priority = data[3].SelectSingleNode("img").Attributes["alt"].Value,
                        Recepients = Regex.Replace(data[4].InnerText.Trim(), @"\s", " ").Trim(),
                        ViewCount = int.Parse(Regex.Match(data[5].InnerText, @"Viewer:\s+(\d+)").Groups[1].Value),
                        ReplyCount = int.Parse(Regex.Match(data[5].InnerText, @"Response:\s+(\d+)").Groups[1].Value),
                        IsRead = true
                    };

                    readAnnouncements.Add(announcement);
                }
            });

            LoadAnnouncements(readAnnouncements, unreadAnnouncements);
        }

        public static ObservableCollection<Announcement> UnreadAnnouncements = new ObservableCollection<Announcement>();
        public static ObservableCollection<Announcement> ReadAnnouncements = new ObservableCollection<Announcement>();

        private void LoadAnnouncements(List<Announcement> readAnnouncements, List<Announcement> unreadAnnouncements)
        {
            UnreadAnnouncements.Clear();
            ReadAnnouncements.Clear();

            foreach (var announcement in unreadAnnouncements)
            {
                UnreadAnnouncements.Add(announcement);
            }            
            
            foreach (var announcement in readAnnouncements)
            {
                ReadAnnouncements.Add(announcement);
            }
        }

        private async void LogoutButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await SecureStorage.SetAsync("userid", "");
                await SecureStorage.SetAsync("password", "");
            }
            catch
            {
                
            }

            Application.Current.MainPage = new LoginPage();
        }

        private async void Announcement_Tapped(object sender, EventArgs e)
        {
            var announcementSender = (StackLayout)sender;
            var pid = ((Announcement)announcementSender.BindingContext).Pid;
            Announcement announcement;

            if(((Announcement)announcementSender.BindingContext).IsRead)
            {   
                announcement = ReadAnnouncements.Where(a => a.Pid != pid).FirstOrDefault();          
            }
            else
            {
                announcement = UnreadAnnouncements.Where(a => a.Pid == pid).FirstOrDefault();
            }

            await Navigation.PushAsync(new AnnouncementDetailPage(announcement));
        }
    }
}