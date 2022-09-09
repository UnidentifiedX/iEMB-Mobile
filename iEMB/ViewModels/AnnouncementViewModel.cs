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
        public AnnouncementViewModel()
        {
            Title = "Announcements";
        }
    }
}