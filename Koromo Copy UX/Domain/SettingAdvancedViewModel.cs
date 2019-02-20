/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy_UX.Domain
{
    public class SettingAdvancedViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool DisableSeleniumHeadless
        {
            get { return Settings.Instance.Net.DisableSeleniumHeadless; }
            set
            {
                Settings.Instance.Net.DisableSeleniumHeadless = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }

        public bool UseFirefoxDriver
        {
            get { return Settings.Instance.Net.UsingFirefoxWebDriver; }
            set
            {
                Settings.Instance.Net.UsingFirefoxWebDriver = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }

        public bool WaitInfinity
        {
            get { return Settings.Instance.Net.TimeoutInfinite; }
            set
            {
                Settings.Instance.Net.TimeoutInfinite = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
        
        public bool LowQualityImage
        {
            get { return Settings.Instance.Model.LowQualityImage; }
            set
            {
                Settings.Instance.Model.LowQualityImage = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
    }
}
