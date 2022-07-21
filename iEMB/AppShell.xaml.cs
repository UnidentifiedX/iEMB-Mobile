using iEMB.ViewModels;
using iEMB.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace iEMB
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AnnouncementDetailPage), typeof(AnnouncementDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

    }
}
