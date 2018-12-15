/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component;
using Koromo_Copy.Component.Mangashow;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
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

namespace Koromo_Copy_UX3.Utility
{
    /// <summary>
    /// SeriesManagerElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SeriesManagerElements : UserControl
    {
        string url;
        SeriesInfo series_info;
        IManager manager;

        public SeriesManagerElements(string url)
        {
            InitializeComponent();

            this.url = url;
            Loaded += SeriesManagerElements_Loaded;
        }

        private void SeriesManagerElements_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                series_info = new SeriesInfo(url);
                manager = MangashowmeManager.Instance;

                ISeries article = manager.ParseSeries(NetCommon.DownloadString(url));

                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(article.Thumbnail);
                    //bitmap.DecodePixelWidth = 250;
                    //bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    Image.Source = bitmap;

                    Title.Text = article.Title;
                }));
            });
        }
        
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            ImageToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            ImageToolTip.HorizontalOffset = e.GetPosition((IInputElement)sender).X + 10;
            ImageToolTip.VerticalOffset = e.GetPosition((IInputElement)sender).Y;
        }
    }
}
