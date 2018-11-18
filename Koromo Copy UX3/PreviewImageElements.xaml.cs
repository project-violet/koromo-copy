/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

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

namespace Koromo_Copy_UX3
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

        private void B_DownloadCompleted(object sender, EventArgs e)
        {
            Image.Stretch = Stretch.Uniform;
            Image.Height = BitmapImage.Height * 600 / BitmapImage.Width;
        }

        private void PreviewImageElements_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    BitmapImage.BeginInit();
                    BitmapImage.UriSource = new Uri(ImageUrl);
                    BitmapImage.EndInit();
                    BitmapImage.DownloadCompleted += B_DownloadCompleted;
                    Image.Source = BitmapImage;
                    Image.Stretch = Stretch.Uniform;
                    Image.Height = BitmapImage.Height * 600 / BitmapImage.Width;
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
