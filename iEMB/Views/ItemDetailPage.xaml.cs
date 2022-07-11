using iEMB.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace iEMB.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}