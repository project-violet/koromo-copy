/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy_UX.Domain;
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

namespace Koromo_Copy_UX
{
    /// <summary>
    /// SettingSpace.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingSpace : UserControl
    {
        public SettingSpace()
        {
            InitializeComponent();
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);

            SettingsTree.Items.Add(new TreeViewItem
            {
                Header=FindResource("information"),
                DataContext = new SettingViewInformation()
            });

            var downloader = new TreeViewItem
            {
                Header = FindResource("downloader"),
                DataContext = new SettingViewDownloader { DataContext = new SettingDownloaderViewModel() }
            };
            downloader.Items.Add(new TreeViewItem
            {
                Header = FindResource("hitomi"),
                DataContext = new SettingViewHitomi { DataContext = new SettingHitomiViewModel() }
            });
            //downloader.Items.Add(new TreeViewItem
            //{
            //    Header = "익헨"
            //});
            downloader.Items.Add(new TreeViewItem
            {
                Header = FindResource("pixiv"),
                DataContext = new SettingViewPixiv {  }
            });
            SettingsTree.Items.Add(downloader);

            //SettingsTree.Items.Add(new TreeViewItem
            //{
            //    Header = "유저 인터페이스",
            //    DataContext = new SettingViewUI()
            //});
            
            SettingsTree.Items.Add(new TreeViewItem
            {
                Header = FindResource("interface"),
                DataContext = new SettingViewInterface { DataContext = new SettingViewInterfaceModel() }
            });

            SettingsTree.Items.Add(new TreeViewItem
            {
                Header = FindResource("script"),
                DataContext = new SettingViewScript { }
            });

            SettingsTree.Items.Add(new TreeViewItem
            {
                Header = FindResource("update"),
                DataContext = new SettingViewUpdate()
            });

            SettingsTree.Items.Add(new TreeViewItem
            {
                Header = FindResource("advanced_setting"),
                DataContext = new SettingViewAdvanced()
            });

            ContentControl.Content = (SettingsTree.Items[0] as TreeViewItem).DataContext;
        }

        private void SettingsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = SettingsTree.SelectedItem;
            if (item is TreeViewItem tvi)
            {
                if (tvi.DataContext != null)
                {
                    ContentControl.Content = tvi.DataContext;
                }
            }
        }
    }
}
