/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy_UX2.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX2
{
    /// <summary>
    /// Setting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Setting : UserControl
    {
        public Setting()
        {
            InitializeComponent();
            
            Loaded += Setting_Loaded;
        }

        private void Setting_Loaded(object sender, RoutedEventArgs e)
        {
            AddLanguages();
            Update();
        }

        public void AddLanguages()
        {
            var langs = new string[] {
                "모든 언어",
                "한국어",
                "日本語",
                "English",
                "Español",
                "ไทย",
                "Deutsch",
                "中文",
                "Português",
                "Français",
                "Tagalog",
                "Русский",
                "Italiano",
                "polski",
                "tiếng việt",
                "magyar",
                "Čeština",
                "Bahasa Indonesia",
                "العربية"
            };
            langs.ToList().ForEach(lang => ((ComboBox)FindName("Language")).Items.Add(new ComboBoxItem { Content = lang }));
        }

        public void Update()
        {
            ((TextBox)FindName("Path")).Text = Koromo_Copy.Settings.Instance.Hitomi.Path;
            ((Slider)FindName("Thread")).Value = Koromo_Copy.Settings.Instance.Model.Thread;
            ((ComboBox)FindName("Language")).Text = "한국어";//Koromo_Copy.Settings.Instance.Hitomi.Language;
        }
    }
}
