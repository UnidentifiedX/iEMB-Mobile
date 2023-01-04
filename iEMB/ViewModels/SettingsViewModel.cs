using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace iEMB.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public string CopyrightText { get; } = $"Version {VersionTracking.CurrentVersion}. © {DateTime.Now.Year} UnidentifiedX.";

        public SettingsViewModel()
        {
            
        }
    }
}
