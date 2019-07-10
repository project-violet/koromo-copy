/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
    /// SearchSimpleElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SearchSimpleElements : UserControl
    {
        public SearchSimpleElements()
        {
            InitializeComponent();
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);
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
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);

            article.Title = HttpUtility.HtmlDecode(article.Title);
            Article = article;
            
            Loaded += SearchSimpleElements_Loaded;

            if (article is HitomiArticle ha)
                if (HitomiLog.Instance.Contains(ha.Magic))
                {
                    DownloadMark.Visibility = Visibility.Visible;

                    if (!Settings.Instance.Hitomi.DisableArtistLastestDownloadDate)
                    {
                        DateBorder.Visibility = Visibility.Visible;
                        Date.Text = HitomiLog.Instance.GetLatestDownload(ha.Magic).ToString();
                    }
                }
        }

        bool init = false;
        private void SearchSimpleElements_Loaded(object sender, EventArgs ex)
        {
            if (init) return;
            init = true;
            Task.Run(() =>
            {
                HitomiArticle ha = Article as HitomiArticle;
                try
                {
                    ha.Thumbnail = HitomiCommon.HitomiThumbnail + HitomiParser.ParseGalleryBlock(Koromo_Copy.Net.NetCommon.DownloadString(
                        $"{HitomiCommon.HitomiGalleryBlock}{ha.Magic}.html")).Thumbnail;
                    ha.ImagesLink = HitomiParser.GetImageLink(Koromo_Copy.Net.NetCommon.DownloadString(HitomiCommon.GetImagesLinkAddress(ha.Magic)));
                }
                catch
                {
                    ha.IsUnstable = true;
                    var har = HCommander.GetArticleData(Convert.ToInt32(ha.Magic));
                    if (!har.HasValue)
                    {
                        MessageBox.Show($"{ha.Magic}를 찾을 수 없습니다. 이 항목은 히요비, 이헨, 익헨 어디에도 없었습니다. 프로그램 제작자에게 문의하세요.", "Koromo copy", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    ha.UnstableModel = har.Value;
                    ha.Thumbnail = ha.UnstableModel.Thumbnail;
                    ha.ImagesLink = new List<string>();
                }
                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    if (ha.IsUnstable && ha.UnstableModel.ArticleType == HArticleType.EXHentai)
                    {
                        var image = NetCommon.GetExHentaiClient().DownloadData(new Uri(ha.UnstableModel.Thumbnail));
                        using (var ms = new System.IO.MemoryStream(image))
                        {
                            BitmapImage.BeginInit();
                            if (Settings.Instance.Model.LowQualityImage)
                                BitmapImage.DecodePixelWidth = 100;
                            BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            BitmapImage.StreamSource = ms;
                            BitmapImage.EndInit();
                        }
                    }
                    else
                    {
                        BitmapImage.BeginInit();
                        BitmapImage.UriSource = new Uri(ha.Thumbnail);
                        if (Settings.Instance.Model.LowQualityImage)
                            BitmapImage.DecodePixelWidth = 100;
                        BitmapImage.EndInit();
                    }
                    Title.Text = ha.Title;
                    Image.Source = BitmapImage;
                }));
            });
        }
        
        public BitmapImage BitmapImage = new BitmapImage();

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            ImageToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            ImageToolTip.HorizontalOffset = e.GetPosition((IInputElement)sender).X - 160;
            ImageToolTip.VerticalOffset = e.GetPosition((IInputElement)sender).Y;
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Select = false;
            Transparent();
            Application.Current.Dispatcher.BeginInvoke(new System.Action(
            delegate
            {
                if (Article is HitomiArticle ha)
                    (new ArticleInfoWindow(ha)).Show();
            }));
        }
    }
}
