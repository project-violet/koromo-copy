/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy_UX.Domain
{
    public class SettingDownloaderViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Thread
        {
            get { return Settings.Instance.Model.Thread; }
            set
            {
                if (Settings.Instance.Model.Thread == value) return;
                Settings.Instance.Model.Thread = value;
                DownloadGroup.Instance.Queue.Capacity = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }

        public bool AutoZip
        {
            get { return Settings.Instance.Model.AutoZip; }
            set
            {
                if (Settings.Instance.Model.AutoZip == value) return;
                Settings.Instance.Model.AutoZip = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
    }
}
