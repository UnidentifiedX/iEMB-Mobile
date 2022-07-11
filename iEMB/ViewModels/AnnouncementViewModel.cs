using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace iEMB.ViewModels
{
    public class AnnouncementViewModel : BaseViewModel
    {
        public AnnouncementViewModel()
        {
            Title = "Announcements";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
        }

        public ICommand OpenWebCommand { get; }
    }
}