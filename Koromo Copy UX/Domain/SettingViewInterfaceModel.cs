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
    public class SettingViewInterfaceModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool UsingThumbnailSearchElements
        {
            get { return Settings.Instance.UXSetting.UsingThumbnailSearchElements; }
            set
            {
                if (Settings.Instance.UXSetting.UsingThumbnailSearchElements == value) return;
                Settings.Instance.UXSetting.UsingThumbnailSearchElements = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
    }
}
