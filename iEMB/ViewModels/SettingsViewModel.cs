using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace iEMB.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public string CopyrightText { get; } = $"Version {VersionTracking.CurrentVersion}. © 2023 UnidentifiedX.";

        public SettingsViewModel()
        {
            //CopyrightText = $"iEMB Mobile {VersionTracking.CurrentVersion}. Copyright 2023 UnidentifiedX.";
        }
    }
}
