using iEMB.Models;
using iEMB.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace iEMB.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AnnouncementSearchPage : ContentPage
    {
        public ObservableCollection<Announcement> CombinedAnnouncements;

        public AnnouncementSearchPage(ObservableCollection<Announcement> unreadAnnouncements, ObservableCollection<Announcement> readAnnouncements)
        {
            InitializeComponent();
            BindingContext = new AnnouncementSearchViewModel();

            CombinedAnnouncements = new ObservableCollection<Announcement>(unreadAnnouncements.Union(readAnnouncements));
            BindableLayout.SetItemsSource(announcementStackLayout, CombinedAnnouncements);
        }

        private async void Announcement_Tapped(object sender, EventArgs e)
        {
            var announcementSender = (StackLayout)sender;
            var pid = ((Announcement)announcementSender.BindingContext).Pid;
            var announcement = CombinedAnnouncements.Where(a => a.Pid == pid).FirstOrDefault();

            await Navigation.PushAsync(new AnnouncementDetailPage(announcement));
        }

        private void SearchBar_QueryChanged(object sender, TextChangedEventArgs e)
        {
            var queryText = ((SearchBar)sender).Text;
            var possibleAnnouncements = CombinedAnnouncements.Where(a => a.Subject.ToLower().Contains(queryText.ToLower()));

            noQueryText.IsVisible = possibleAnnouncements.Count() == 0;
            BindableLayout.SetItemsSource(announcementStackLayout, possibleAnnouncements);
        }
    }
}