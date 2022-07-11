using HtmlAgilityPack;
using iEMB.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var unreadMessages = doc.DocumentNode.SelectNodes("//tbody")[0].SelectNodes("tr");
            var readMessages = doc.DocumentNode.SelectNodes("//tbody")[1].SelectNodes("tr");
            var unreadAnnouncements = new List<Announcement>();
            var readAnnouncements = new List<Announcement>();

            await Task.Run(() =>
            {
                foreach (var message in unreadMessages)
                {
                    var data = message.SelectNodes("td");

                    var announcement = new Announcement
                    {
                        PostDate = Regex.Replace(data[0].InnerText, @"\s+", ""),
                        Sender = data[1].SelectSingleNode("a").Attributes["tooltip-data"].Value,
                        Username = data[1].SelectSingleNode("a").InnerText.Trim(),
                        Subject = data[2].SelectSingleNode("a").InnerText,
                        Url = data[2].SelectSingleNode("a").Attributes["href"].Value,
                        BoardID = "1024",
                        Pid = Regex.Match(data[2].SelectSingleNode("a").Attributes["href"].Value, @"/Board/content/(\d+)").Groups[1].Value,
                        Priority = data[3].SelectSingleNode("img").Attributes["alt"].Value,
                        Recepients = data[4].InnerText.Trim(),
                        ViewCount = int.Parse(Regex.Match(data[5].InnerText, @"Viewer:\s+(\d+)").Groups[1].Value),
                        ReplyCount = int.Parse(Regex.Match(data[5].InnerText, @"Response:\s+(\d+)").Groups[1].Value),
                        IsRead = false
                    };

                    unreadAnnouncements.Add(announcement);
                }

                foreach (var message in readMessages)
                {
                    var data = message.SelectNodes("td");

                    var announcement = new Announcement
                    {
                        PostDate = Regex.Replace(data[0].InnerText, @"\s+", ""),
                        Sender = data[1].SelectSingleNode("a").Attributes["tooltip-data"].Value,
                        Username = data[1].SelectSingleNode("a").InnerText.Trim(),
                        Subject = data[2].SelectSingleNode("a").InnerText,
                        Url = data[2].SelectSingleNode("a").Attributes["href"].Value,
                        BoardID = "1024",
                        Pid = Regex.Match(data[2].SelectSingleNode("a").Attributes["href"].Value, @"/Board/content/(\d+)").Groups[1].Value,
                        Priority = data[3].SelectSingleNode("img").Attributes["alt"].Value,
                        Recepients = data[4].InnerText.Trim(),
                        ViewCount = int.Parse(Regex.Match(data[5].InnerText, @"Viewer:\s+(\d+)").Groups[1].Value),
                        ReplyCount = int.Parse(Regex.Match(data[5].InnerText, @"Response:\s+(\d+)").Groups[1].Value),
                        IsRead = true
                    };

                    readAnnouncements.Add(announcement);
                }
            });


        }

        private void LogoutButton_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new LoginPage();
        }
    }
}