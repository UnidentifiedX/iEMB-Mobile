using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace iEMB.Views
{
    public partial class AnnouncementPage : ContentPage
    {
        public AnnouncementPage()
        {
            InitializeComponent();
        }

        public AnnouncementPage(string sessionID, string authenticationToken)
        {
            InitializeComponent();
        }
    }
}