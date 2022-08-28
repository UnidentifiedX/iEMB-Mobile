using iEMB.Models;
using iEMB.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace iEMB.ViewModels
{
    public class AnnouncementViewModel : BaseViewModel
    {
        private ObservableCollection<Announcement> _unreadAnnouncements;
        private ObservableCollection<Announcement> _readAnnouncements;

        public AnnouncementViewModel()
        {
            Title = "Announcements";
            UnreadAnnouncements = AnnouncementPage.UnreadAnnouncements;
            ReadAnnouncements = AnnouncementPage.ReadAnnouncements;
        }

        public ObservableCollection<Announcement> UnreadAnnouncements
        {
            get { return _unreadAnnouncements; }
            set
            {
                _unreadAnnouncements = value;
            }
        }        

        public ObservableCollection<Announcement> ReadAnnouncements
        {
            get { return _readAnnouncements; }
            set
            {
                _readAnnouncements = value;
            }
        }
    }
}