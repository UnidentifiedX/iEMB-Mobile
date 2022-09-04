using SQLite;
using Xamarin.Forms;
namespace iEMB.Models
{
    public class Announcement
    {
        public string PostDate { get; set; }
        public string Sender { get; set; }
        public string Username { get; set; }
        public string Subject { get; set; }
        public string Url { get; set; }
        public string BoardID { get; set; }
        [PrimaryKey]
        public string Pid { get; set; }
        public string Priority { get; set; }
        public string Recepients { get; set; }
        public int ViewCount { get; set; }
        public int? ReplyCount { get; set; }
        public bool IsRead { get; set; }
        public bool IsArchived { get; set; }
        public bool HasAttatchments { get; set; }
        public string HtmlString { get; set; }
        public string PriorityImageSource { get; set; }
    }
}
