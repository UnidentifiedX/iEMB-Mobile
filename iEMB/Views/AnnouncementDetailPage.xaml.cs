using Acr.UserDialogs;
using HtmlAgilityPack;
using iEMB.Models;
using iEMB.ViewModels;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace iEMB.Views
{
    public partial class AnnouncementDetailPage : ContentPage
    {

        static FormattedString FormattedString = new FormattedString();

        public AnnouncementDetailPage()
        {
            InitializeComponent();
        }

        public AnnouncementDetailPage(Announcement announcement)
        {
            InitializeComponent();
            GetAnnouncement(announcement);
            BindingContext = new AnnouncementDetailViewModel();
        }

        private async void GetAnnouncement(Announcement announcement)
        {
            var boardID = "1048";

            var verificationToken = LoginPage.VerificationToken;
            var sessionID = LoginPage.SessionID;
            var authenticationToken = LoginPage.AuthenticationToken;

            using (var handler = new HttpClientHandler { UseCookies = false })
            using (var client = new HttpClient(handler) { BaseAddress = new Uri("https://iemb.hci.edu.sg") })
            {
                var message = new HttpRequestMessage(HttpMethod.Get, $"https://iemb.hci.edu.sg/Board/content/{announcement.Pid}?board={boardID}&isArchived=False");
                // chinese quiz thing
                //var message = new HttpRequestMessage(HttpMethod.Get, $"https://iemb.hci.edu.sg/Board/content/98724?board=1048&isArchived=False");
                // LSS test information
                //var message = new HttpRequestMessage(HttpMethod.Get, $"https://iemb.hci.edu.sg/Board/content/98745?board=1048&isArchived=False");
                // australian math 
                //var message = new HttpRequestMessage(HttpMethod.Get, $"https://iemb.hci.edu.sg/Board/content/97500?board=1048&isArchived=False");
                // Canteen pic
                //var message = new HttpRequestMessage(HttpMethod.Get, $"https://iemb.hci.edu.sg/Board/content/98715?board=1048&isArchived=False");
                // Chinese CT mesage
                //var message = new HttpRequestMessage(HttpMethod.Get, $"https://iemb.hci.edu.sg/Board/content/98074?board=1048&isArchived=False");
                // class tshirt
                //var message = new HttpRequestMessage(HttpMethod.Get, $"https://iemb.hci.edu.sg/Board/content/87413?board=1048&isArchived=False");


                //var message = new HttpRequestMessage(HttpMethod.Get, $"https://iemb.hci.edu.sg/Board/content/97500?board=1048&isArchived=False");

                //buggy
                // https://iemb.hci.edu.sg/Board/content/98685?board=1048&isArchived=False (a href and strong don't work with each other)
                // https://iemb.hci.edu.sg/Board/content/95252?board=1048&isArchived=False (when you have a ul in a ul (WHY????????))

                message.Headers.Add("host", "iemb.hci.edu.sg");
                message.Headers.Add("referer", $"https://iemb.hci.edu.sg/Board/Detail/{boardID}");
                message.Headers.Add("user-agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Mobile Safari/537.36");
                message.Headers.Add("cookie", $"__RequestVerificationToken={verificationToken};ASP.NET_SessionId={sessionID}; AuthenticationToken={authenticationToken};");

                var result = await client.SendAsync(message);
                var doc = new HtmlDocument();
                doc.LoadHtml(await result.Content.ReadAsStringAsync());

                var post = doc.DocumentNode.Descendants().Where(div => div.HasClass("box") && div.Id == "fontBox").First().Descendants().Where(div => div.Id == "hyplink-css-style").First().SelectSingleNode("div");

                var isStarred = doc.GetElementbyId("fav").Attributes["title"].Value == "starred message";
                if(isStarred)
                {
                    itemStar.IconImageSource = ImageSource.FromFile("icon_star_filled.png");
                }

                itemStar.Clicked += (s, e) =>
                {
                    StarAnnouncement(s, e, announcement.Pid, isStarred, verificationToken, sessionID, authenticationToken);
                    isStarred = !isStarred;
                };

                subject.Text = announcement.Subject;
                sender.Text = "From: " + announcement.Sender;
                recepients.Text = "Recepients: " + announcement.Recepients;
                postDate.Text = "Posted on: " + announcement.PostDate;

                loadingIndicator.IsRunning = false;
                loadingText.IsVisible = false;
                try
                {
                    foreach (var node in post.Descendants())
                    {
                        switch (node.Name)
                        {
                            case "table":
                                InsertFormattedString();

                                var frameBorderWidth = 2;

                                var grid = new Grid()
                                {
                                    ColumnSpacing = -frameBorderWidth,
                                    RowSpacing = -frameBorderWidth,
                                };

                                var rows = node.SelectNodes("tbody/tr").Count;
                                var columns = node.SelectNodes("tbody/tr").Max(tr => tr.SelectNodes("td").Count);

                                for (int i = 0; i < rows; i++)
                                {
                                    grid.RowDefinitions.Add(new RowDefinition
                                    {
                                        Height = new GridLength(1, GridUnitType.Auto),
                                    });
                                }

                                for (int i = 0; i < columns; i++)
                                {
                                    grid.ColumnDefinitions.Add(new ColumnDefinition
                                    {
                                        Width = new GridLength(1, GridUnitType.Auto),
                                    });
                                }

                                var rowCount = 0;
                                foreach (var row in node.SelectNodes("tbody/tr"))
                                {
                                    var tdNodes = row.SelectNodes("td");
                                    var cellCount = tdNodes.Count;

                                    for (int columnCount = 0; columnCount < cellCount; columnCount++)
                                    {
                                        var rowSpan = tdNodes[columnCount].Attributes["rowspan"]?.Value;
                                        var colSpan = tdNodes[columnCount].Attributes["colspan"]?.Value;
                                        var frame = new Frame
                                        {
                                            BorderColor = Color.White,
                                            BackgroundColor = Color.FromHex("#1a1a1a"),
                                            Padding = new Thickness(frameBorderWidth),
                                        };
                                        var label = new Label
                                        {
                                            TextColor = Color.White,
                                        };
                                        var formattedString = new FormattedString();

                                        foreach (var tdNode in tdNodes[columnCount].Descendants())
                                        {
                                            var span = ParseFormattedText(tdNode, NodeType.table);
                                            span.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(FormattedString));
                                            formattedString.Spans.Add(span);
                                        }

                                        label.FormattedText = formattedString;
                                        frame.Content = label;
                                        grid.Children.Add(frame,
                                            columnCount, colSpan == null ? columnCount + 1 : columnCount + int.Parse(colSpan), rowCount, rowSpan == null ? rowCount + 1 : rowCount + int.Parse(rowSpan));
                                    }
                                    rowCount++;
                                }

                                announcementContent.Children.Add(grid);
                                break;
                            case "ol":
                                var count = 1;
                                foreach (var point in node.SelectNodes("li"))
                                {
                                    AddSpan(new Span
                                    {
                                        Text = Environment.NewLine + $"{count}. ",
                                    });

                                    foreach (var descendant in point.Descendants())
                                    {
                                        AddSpan(ParseFormattedText(descendant, NodeType.ul));
                                    }

                                    count++;
                                }

                                break;
                            case "ul":
                                //InsertFormattedString();
                                foreach (var point in node.SelectNodes("li"))
                                {
                                    AddSpan(new Span
                                    {
                                        Text = Environment.NewLine + "• ",
                                    });

                                    foreach (var descendant in point.Descendants())
                                    {
                                        AddSpan(ParseFormattedText(descendant, NodeType.ul));
                                    }
                                }
                                break;
                            case "img":
                                try
                                {
                                    InsertFormattedString();
                                    var src = node.Attributes["src"].Value;

                                    Image image;
                                    if (src.StartsWith("http"))
                                    {
                                        image = new Image()
                                        {
                                            Source = new Uri(src),
                                            Margin = new Thickness(0, 15, 0, 0),
                                        };
                                    }
                                    else
                                    {
                                        var bytes = Convert.FromBase64String(Regex.Replace(src, @"data:image/.+?;base64,", ""));
                                        image = new Image()
                                        {
                                            Source = ImageSource.FromStream(() => new MemoryStream(bytes)),
                                            Margin = new Thickness(0, 15, 0, 0)
                                        };
                                    }

                                    announcementContent.Children.Add(image);
                                }
                                catch (Exception)
                                {
                                    //ToastConfig.DefaultPosition = ToastPosition.Bottom;
                                    //UserDialogs.Instance.Toast("Unable to load image");
                                }
                                break;
                            case "p":
                            case "a":
                            case "br":
                            case "i":
                            case "u":
                            case "strong":
                            case "#text":
                                AddSpan(ParseFormattedText(node, NodeType.none));
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ToastConfig.DefaultPosition = ToastPosition.Bottom;
                    UserDialogs.Instance.Toast("Something went wrong loading the announcement.");
                    Console.WriteLine(ex.ToString());
                }



                // Insert any remaining formatted strings, if any
                InsertFormattedString();
            }
        }

        private static Span ParseFormattedText(HtmlNode node, NodeType nodeType)
        {
            var text = WebUtility.HtmlDecode(node.InnerText);
            var previous = FormattedString.Spans.LastOrDefault();
            //if (previous != null) Console.WriteLine(previous.Text);
            //Console.WriteLine(node.);

            switch (nodeType)
            {
                case NodeType.none:
                    if (node.Ancestors("ul").Count() != 0 ||
                        node.Ancestors("ol").Count() != 0 ||
                        node.Ancestors("table").Count() != 0)
                    {
                        return new Span();
                    }
                    break;
                case NodeType.ul:
                case NodeType.ol:
                case NodeType.table:
                    break;
            }

            switch (node.Name)
            {
                case "a":
                    var link = node.Attributes["href"]?.Value;
                    var span = new Span();

                    if (link == null)
                    {
                        span.Text = text;
                        span.TextColor = Color.White;
                    }
                    else
                    {
                        span = new Span()
                        {
                            Text = text,
                            TextColor = Color.MediumPurple,
                            TextDecorations = TextDecorations.Underline,
                        };

                        var tapGestureRecognizer = new TapGestureRecognizer();
                        tapGestureRecognizer.Tapped += async (s, e) =>
                        {
                            try
                            {
                                await Browser.OpenAsync(link);
                            }
                            catch
                            {
                                ToastConfig.DefaultPosition = ToastPosition.Bottom;
                                UserDialogs.Instance.Toast("Something went wrong opening this link");
                            }
                        };

                        span.GestureRecognizers.Add(tapGestureRecognizer);
                    }

                    return span;
                case "b":
                case "strong":
                    TextDecorations previousTextDecorations = TextDecorations.None;
                    FontAttributes previousFontAttributes = FontAttributes.None;
                    if (previous != null && node.InnerText.StartsWith(previous.Text))
                    {
                        previousTextDecorations = previous.TextDecorations;
                        previousFontAttributes = previous.FontAttributes;

                        FormattedString.Spans.RemoveAt(FormattedString.Spans.Count - 1);
                    }

                    return new Span()
                    {
                        Text = text,
                        TextColor = Color.White,
                        TextDecorations = previousTextDecorations,
                        FontAttributes = previousFontAttributes | FontAttributes.Bold,
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Span)),
                    };
                case "u":
                    previousTextDecorations = TextDecorations.None;
                    previousFontAttributes = FontAttributes.None;
                    if (previous != null && node.InnerText.StartsWith(previous.Text))
                    {
                        previousTextDecorations = previous.TextDecorations;
                        previousFontAttributes = previous.FontAttributes;

                        FormattedString.Spans.RemoveAt(FormattedString.Spans.Count - 1);
                    }

                    return new Span()
                    {
                        Text = text,
                        TextColor = Color.White,
                        TextDecorations = previousTextDecorations | TextDecorations.Underline,
                        FontAttributes = previousFontAttributes,
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Span)),
                    };
                case "i":
                    previousTextDecorations = TextDecorations.None;
                    previousFontAttributes = FontAttributes.None;
                    if (previous != null && node.InnerText.StartsWith(previous.Text))
                    {
                        previousTextDecorations = previous.TextDecorations;
                        previousFontAttributes = previous.FontAttributes;

                        FormattedString.Spans.RemoveAt(FormattedString.Spans.Count - 1);
                    }

                    return new Span()
                    {
                        Text = text,
                        TextColor = Color.White,
                        TextDecorations = previousTextDecorations,
                        FontAttributes = previousFontAttributes | FontAttributes.Italic,
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Span)),
                    };
                case "br":
                    return new Span()
                    {
                        Text = Environment.NewLine,
                    };
                case "p":
                    //if(Regex.IsMatch(text, @"\s"))
                    //{
                    //    return new Span()
                    //    {
                    //        Text = Environment.NewLine,
                    //    };
                    //}
                    return new Span()
                    {
                        Text = Environment.NewLine,
                    };
                case "#text":
                    if (Regex.Replace(text, @"\s", "") == "") return new Span();

                    // If the current text starts with the previous span, the previous span will be removed from the current text
                    //if (previous?.Text != null && text.StartsWith(previous.Text))
                    //{
                    //    text = text.Remove(0, previous.Text.Length);
                    //}
                    if (node.Ancestors("strong").Count() != 0) return new Span();
                    if (node.Ancestors("b").Count() != 0) return new Span();
                    if (node.Ancestors("u").Count() != 0) return new Span();
                    if (node.Ancestors("i").Count() != 0) return new Span();
                    if (node.Ancestors("a").Count() != 0) return new Span();

                    return new Span()
                    {
                        Text = text,
                        TextColor = Color.White,
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Span)),
                    };
                default:
                    return new Span();
            }
        }

        private void AddSpan(Span span)
        {
            FormattedString.Spans.Add(span);
        }

        private void InsertFormattedString()
        {
            if (FormattedString.ToString() == "") return;

            var label = new Label()
            {
                FormattedText = FormattedString,
                TextColor = Color.White,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
            };

            announcementContent.Children.Add(label);
            FormattedString = new FormattedString();
        }

        private async void StarAnnouncement(object s, EventArgs e, string pid, bool isStarred, string verificationToken, string sessionID, string authenticationToken)
        {
            var boardID = 1048;

            var postData = $"status={(isStarred ? 0 : 1)}&boardId={boardID}&topicid={pid}";
            var postDataByteArray = Encoding.ASCII.GetBytes(postData);

            var cookieContainer = new CookieContainer();
            var request = (HttpWebRequest)WebRequest.Create("https://iemb.hci.edu.sg/Board/ProcessFav");
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
            var successful = Regex.Match(reader.ReadToEnd(), @"{""IsSuccess"":(.+?)}").Groups[1].Value == "true";

            if(successful)
            {
                itemStar.IconImageSource = ImageSource.FromFile(isStarred ? "icon_star.png" : "icon_star_filled.png");
            }
            else
            {
                UserDialogs.Instance.Toast("Unable to star message.");
            }
        }
    }
}