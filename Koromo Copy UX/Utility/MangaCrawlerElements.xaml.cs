/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// MangaCrawlerElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MangaCrawlerElements : UserControl
    {
        string image;
        string url;
        public MangaCrawlerElements(string image, string url, string title)
        {
            InitializeComponent();

            this.image = image;
            this.url = url;
            Loaded += MangaCrawlerElements_Loaded;
            Title.Text = title;
        }

        private void MangaCrawlerElements_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(image);
                    bitmap.EndInit();

                    Image.Source = bitmap;
                }));
            });
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start($"https://mangashow.me{url}");
        }
    }
}
