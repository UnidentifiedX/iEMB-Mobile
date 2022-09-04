using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iEMB.Models;
using iEMB.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace iEMB.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SavedAnnouncementPage : ContentPage
    {
        private AsyncLazy<AnnouncementDatabase> _database = AnnouncementDatabase.Instance;
        private List<Announcement> _announcements;

        public SavedAnnouncementPage()
        {
            InitializeComponent();
            BindingContext = new SavedAnnouncementViewModel();

            LoadSavedAnnouncements();
        }

        protected override void OnAppearing()
        {
            LoadSavedAnnouncements();
        }

        private async void LoadSavedAnnouncements()
        {
            var database = await _database;
            var savedAnnouncements = await database.GetAnnouncementsAsync();

            _announcements = savedAnnouncements;
            savedAnnouncmentSearchBar.IsVisible = savedAnnouncements.Any();
            noSavedAnnouncements.IsVisible = !savedAnnouncements.Any();

            BindableLayout.SetItemsSource(savedAnnouncementsStack, savedAnnouncements);
        }

        private async void Announcement_Tapped(object sender, EventArgs e)
        {
            var announcementSender = (StackLayout)sender;
            var pid = ((Announcement)announcementSender.BindingContext).Pid;
            var announcement = _announcements.Where(a => a.Pid == pid).FirstOrDefault();

            await Navigation.PushAsync(new AnnouncementDetailPage(announcement));
        }

        private void SearchBar_QueryChanged(object sender, TextChangedEventArgs e)
        {
            var queryText = ((SearchBar)sender).Text;
            BindableLayout.SetItemsSource(savedAnnouncementsStack, _announcements.Where(a => a.Subject.ToLower().Contains(queryText.ToLower())));
        }
    }
}