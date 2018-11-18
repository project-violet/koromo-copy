/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
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
    /// SearchSimpleElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SearchSimpleElements : UserControl
    {
        public SearchSimpleElements()
        {
            InitializeComponent();
        }

        private void SearchSimpleElements_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_Select)
            {
                PinkRectangle.Fill = Brushes.Transparent;
                Border.BorderBrush = Brushes.Transparent;
            }
        }

        private void SearchSimpleElements_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!_Select)
            {
                PinkRectangle.Fill = new SolidColorBrush(Color.FromArgb(100, 234, 202, 233));
                Border.BorderThickness = new Thickness(1);
                Border.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 174, 201));
                BorderCollapsed.BorderThickness = new Thickness(1);
            }
        }

        private bool _Select = false;
        public bool Select
        {
            get { return _Select; }
            set
            {
                _Select = value;
                if (value)
                {
                    PinkRectangle.Fill = new SolidColorBrush(Color.FromArgb(200, 234, 202, 233));
                    Border.BorderThickness = new Thickness(2);
                    Border.BorderBrush = Brushes.LightPink;
                    BorderCollapsed.BorderThickness = new Thickness(0);
                }
                else
                {
                    PinkRectangle.Fill = new SolidColorBrush(Color.FromArgb(100, 234, 202, 233));
                    Border.BorderThickness = new Thickness(1);
                    Border.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 174, 201));
                    BorderCollapsed.BorderThickness = new Thickness(1);
                }
            }
        }

        public void Transparent()
        {
            PinkRectangle.Fill = Brushes.Transparent;
            Border.BorderBrush = Brushes.Transparent;
        }

        private void SearchSimpleElements_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Select = !Select;
        }

        public IArticle Article;

        public SearchSimpleElements(IArticle article)
        {
            InitializeComponent();

            Article = article;
            
            Loaded += SearchSimpleElements_Loaded;

            if (article is HitomiArticle ha)
                if (HitomiLog.Instance.Contains(ha.Magic))
                    DownloadMark.Visibility = Visibility.Visible;
        }

        bool init = false;
        private void SearchSimpleElements_Loaded(object sender, EventArgs ex)
        {
            if (init) return;
            init = true;
            Task.Run(() =>
            {
                HitomiArticle ha = Article as HitomiArticle;
                ha.Thumbnail = HitomiCommon.HitomiThumbnail + HitomiParser.ParseGalleryBlock(Koromo_Copy.Net.NetCommon.DownloadString(
                    $"{HitomiCommon.HitomiGalleryBlock}{ha.Magic}.html")).Thumbnail;
                ha.ImagesLink = HitomiParser.GetImageLink(Koromo_Copy.Net.NetCommon.DownloadString(HitomiCommon.GetImagesLinkAddress(ha.Magic)));

                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    BitmapImage.BeginInit();
                    BitmapImage.UriSource = new Uri(ha.Thumbnail);
                    BitmapImage.EndInit();
                    Title.Text = ha.Title;
                    Image.Source = BitmapImage;
                }));
            });
        }
        
        public BitmapImage BitmapImage = new BitmapImage();

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            ImageToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            ImageToolTip.HorizontalOffset = e.GetPosition((IInputElement)sender).X + 10;
            ImageToolTip.VerticalOffset = e.GetPosition((IInputElement)sender).Y;
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        { 
            Application.Current.Dispatcher.BeginInvoke(new System.Action(
            delegate
            {
                if (Article is HitomiArticle ha)
                    (new ArticleInfoWindow(ha)).Show();
            }));
        }
    }
}
