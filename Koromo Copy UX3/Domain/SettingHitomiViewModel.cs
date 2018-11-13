/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
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

namespace Koromo_Copy_UX3.Domain
{
    public class SettingHitomiViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public bool AutoSync
        {
            get { return Settings.Instance.Hitomi.AutoSync; }
            set
            {
                if (Settings.Instance.Hitomi.AutoSync == value) return;
                Settings.Instance.Hitomi.AutoSync = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
        
        public bool UsingOptimization
        {
            get { return Settings.Instance.Hitomi.UsingOptimization; }
            set
            {
                if (Settings.Instance.Hitomi.UsingOptimization == value) return;
                Settings.Instance.Hitomi.UsingOptimization = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
        
        public bool UsingFuzzy
        {
            get { return Settings.Instance.Hitomi.UsingFuzzy; }
            set
            {
                if (Settings.Instance.Hitomi.UsingFuzzy == value) return;
                Settings.Instance.Hitomi.UsingFuzzy = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
        
        public bool UsingAdvancedSearch
        {
            get { return Settings.Instance.Hitomi.UsingAdvancedSearch; }
            set
            {
                if (Settings.Instance.Hitomi.UsingAdvancedSearch == value) return;
                Settings.Instance.Hitomi.UsingAdvancedSearch = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
        
        public bool UsingOnlyFMTagsOnAnalysis
        {
            get { return Settings.Instance.HitomiAnalysis.UsingOnlyFMTagsOnAnalysis; }
            set
            {
                if (Settings.Instance.HitomiAnalysis.UsingOnlyFMTagsOnAnalysis == value) return;
                Settings.Instance.HitomiAnalysis.UsingOnlyFMTagsOnAnalysis = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
        
        public bool UsingXiAanlysis
        {
            get { return Settings.Instance.HitomiAnalysis.UsingXiAanlysis; }
            set
            {
                if (Settings.Instance.HitomiAnalysis.UsingXiAanlysis == value) return;
                Settings.Instance.HitomiAnalysis.UsingXiAanlysis = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
        
        public bool UsingRMSAanlysis
        {
            get { return Settings.Instance.HitomiAnalysis.UsingRMSAanlysis; }
            set
            {
                if (Settings.Instance.HitomiAnalysis.UsingRMSAanlysis == value) return;
                Settings.Instance.HitomiAnalysis.UsingRMSAanlysis = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
        
        public bool UsingCosineAnalysis
        {
            get { return Settings.Instance.HitomiAnalysis.UsingCosineAnalysis; }
            set
            {
                if (Settings.Instance.HitomiAnalysis.UsingCosineAnalysis == value) return;
                Settings.Instance.HitomiAnalysis.UsingCosineAnalysis = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
        
        public bool RecommendNMultipleWithLength
        {
            get { return Settings.Instance.HitomiAnalysis.RecommendNMultipleWithLength; }
            set
            {
                if (Settings.Instance.HitomiAnalysis.RecommendNMultipleWithLength == value) return;
                Settings.Instance.HitomiAnalysis.RecommendNMultipleWithLength = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
        
        public bool RecommendLanguageALL
        {
            get { return Settings.Instance.HitomiAnalysis.RecommendLanguageALL; }
            set
            {
                if (Settings.Instance.HitomiAnalysis.RecommendLanguageALL == value) return;
                Settings.Instance.HitomiAnalysis.RecommendLanguageALL = value;
                Settings.Instance.Save();
                OnPropertyChanged();
            }
        }
    }
}
