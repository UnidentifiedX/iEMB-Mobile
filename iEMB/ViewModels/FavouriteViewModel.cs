using iEMB.Models;
using iEMB.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace iEMB.ViewModels
{
    public class FavouriteViewModel : BaseViewModel
    {
        public ObservableCollection<Announcement> _starredAnnouncements;

        public ObservableCollection<Announcement> StarredAnnouncements
        {
            get { return _starredAnnouncements; }
            set
            {
                _starredAnnouncements = value;
            }
        }

        public FavouriteViewModel()
        {
            Title = "Starred Messages";
            StarredAnnouncements = FavouritePage.StarredAnnouncements;
        }
    }
}