using iEMB.Services;
using iEMB.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace iEMB
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new LoginPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
