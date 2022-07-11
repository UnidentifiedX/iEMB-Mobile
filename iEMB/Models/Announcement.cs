using System;
using System.Collections.Generic;
using System.Text;

namespace iEMB.Models
{
    internal class Announcement
    {
        public string PostDate { get; set; }
        public string Sender { get; set; }
        public string Username { get; set; }
        public string Subject { get; set; }
        public string Url { get; set; }
        public string BoardID { get; set; }
        public string Pid { get; set; }
        public string Priority { get; set; }
        public string Recepients { get; set; }
        public int ViewCount { get; set; }
        public int ReplyCount { get; set; }
        public bool IsRead { get; set; }
    }
}
