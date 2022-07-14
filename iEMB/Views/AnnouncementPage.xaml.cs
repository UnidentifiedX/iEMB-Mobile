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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace iEMB.Views
{
    public partial class AnnouncementPage : ContentPage
    {
        private string _verificationToken;
        private string _sessionID; 
        private string _authenticationToken;

        public AnnouncementPage()
        {
            InitializeComponent();
            BindingContext = new AnnouncementViewModel();
        }

        public AnnouncementPage(string verificationToken, string sessionID, string authenticationToken)
        {
            InitializeComponent();
            _verificationToken = verificationToken;
            _sessionID = sessionID;
            _authenticationToken = authenticationToken;
            GetAnnouncements(verificationToken, sessionID, authenticationToken);
        }

        private async void GetAnnouncements(string verificationToken, string sessionID, string authenticationToken)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://iemb.hci.edu.sg/Board/Detail/1048");
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("https://iemb.hci.edu.sg/Board/Detail/1048"), new Cookie("__RequestVerificationToken", _verificationToken));
            cookieContainer.Add(new Uri("https://iemb.hci.edu.sg/Board/Detail/1048"), new Cookie("ASP.NET_SessionId", _sessionID));
            cookieContainer.Add(new Uri("https://iemb.hci.edu.sg/Board/Detail/1048"), new Cookie("AuthenticationToken", _authenticationToken));
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
                        Subject = WebUtility.HtmlDecode(data[2].SelectSingleNode("a").InnerText),
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

            LoadAnnouncements(readAnnouncements, unreadAnnouncements);
        }

        public static ObservableCollection<Announcement> ReadAnnouncements = new ObservableCollection<Announcement>();

        private void LoadAnnouncements(List<Announcement> readAnnouncements, List<Announcement> unreadAnnouncements)
        {
            ReadAnnouncements.Clear();
            foreach (var announcement in unreadAnnouncements)
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
            Console.WriteLine(_verificationToken + "amogus");
            //var announcementSender = (StackLayout)sender;
            //var pid = ((Announcement)announcementSender.BindingContext).Pid;
            //var boardID = 1024;
            //var announcement = ReadAnnouncements.Where(a => a.Pid == pid).FirstOrDefault();

            //Console.WriteLine(_verificationToken);
            //Console.WriteLine(_sessionID);
            //Console.WriteLine(_authenticationToken);

            //var request = (HttpWebRequest)WebRequest.Create($"https://iemb.hci.edu.sg/Board/content/{pid}?board={boardID}&isArchived=False");
            //var cookieContainer = new CookieContainer();
            //cookieContainer.Add(new Uri($"https://iemb.hci.edu.sg/Board/Detail/{boardID}"), new Cookie("__RequestVerificationToken", _verificationToken));
            //cookieContainer.Add(new Uri($"https://iemb.hci.edu.sg/Board/Detail/{boardID}"), new Cookie("ASP.NET_SessionId", _sessionID));
            //cookieContainer.Add(new Uri($"https://iemb.hci.edu.sg/Board/Detail/{boardID}"), new Cookie("AuthenticationToken", _authenticationToken));
            //request.Method = "GET";
            //request.CookieContainer = cookieContainer;
            //request.Host = "iemb.hci.edu.sg";
            //request.Referer = $"https://iemb.hci.edu.sg/Board/Detail/{boardID}";
            ////request.ContentType = "application/x-www-form-urlencoded";
            //request.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Mobile Safari/537.36";

            //var response = (HttpWebResponse)await request.GetResponseAsync();
            //var dataStream = response.GetResponseStream();
            //var reader = new StreamReader(dataStream);
            //var content = reader.ReadToEnd();

            //Console.WriteLine(content);
            //using (var browserFetcher = Puppeteer.CreateBrowserFetcher(new BrowserFetcherOptions()))
            //{
            //    var revisionInfo = await browserFetcher.DownloadAsync(533271);

            //    var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            //    {
            //        ExecutablePath = revisionInfo.ExecutablePath,
            //        Headless = true,
            //    });
            //    var page = await browser.NewPageAsync();

            //    await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
            //    {
            //        { "referer", $"https://iemb.hci.edu.sg/Board/Detail/{boardID}" },
            //        { "user-agent",  "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Mobile Safari/537.36" },
            //        { "cookie", $"__RequestVerificationToken=${_verificationToken};.Mozilla%2f4.0+(compatible%3b+MSIE+6.1%3b+Windows+XP);ASP.NET_SessionId=${_sessionID}; AuthenticationToken=${_authenticationToken};" }
            //    });

            //    await page.GoToAsync($"https://iemb.hci.edu.sg/Board/content/{pid}?board={boardID}&isArchived=False", new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });

            //    var doc = new HtmlDocument();
            //    var content = await page.GetContentAsync();
            //    doc.LoadHtml(content);
            //    Console.WriteLine(content);
            //}
        }
    }
}