/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    /// PreviewImageElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PreviewImageElements : UserControl
    {
        public PreviewImageElements()
        {
            InitializeComponent();
        }


        public PreviewImageElements(string page, string image_url)
        {
            InitializeComponent();

            Page.Text = page;
            Loaded += PreviewImageElements_Loaded;
            ImageUrl = image_url;
        }

        string ImageUrl = "";
        public BitmapImage BitmapImage = new BitmapImage();
        
        Stream image_stream;
        private void PreviewImageElements_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    var req = (HttpWebRequest)WebRequest.Create(ImageUrl);
                    if (ImageUrl.Contains("hitomi.la"))
                    {
                        req.Referer = $"https://hitomi.la/galleries/{ImageUrl.Split('/')[4]}.html";
                    }
                    image_stream = req.GetResponse().GetResponseStream();
                    Application.Current.Dispatcher.BeginInvoke(new Action(
                    delegate
                    {
                        BitmapImage.BeginInit();
                        BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        BitmapImage.StreamSource = image_stream;
                        BitmapImage.DownloadCompleted += BitmapImage_DownloadCompleted;
                        BitmapImage.EndInit();
                        Image.Source = BitmapImage;
                    }));
                }
                catch (Exception ex) { Monitor.Instance.Push($"[Preview Image] Load Error {ex.Message} {ex.StackTrace}"); }
            });
        }

        private void BitmapImage_DownloadCompleted(object sender, EventArgs e)
        {
            Image.Stretch = Stretch.Uniform;
            Image.Height = BitmapImage.Height * 600 / BitmapImage.Width;
            image_stream.Close();
            image_stream.Dispose();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            ImageToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            ImageToolTip.HorizontalOffset = e.GetPosition((IInputElement)sender).X + 10;
            ImageToolTip.VerticalOffset = e.GetPosition((IInputElement)sender).Y;
        }
    }
}
