/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hitomi.Translate;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using MaterialDesignThemes.Wpf;
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
    /// SearchMaterialElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SearchMaterialElements : UserControl
    {
        public SearchMaterialElements()
        {
            InitializeComponent();
        }

        public IArticle Article;

        public SearchMaterialElements(IArticle article)
        {
            InitializeComponent();

            article.Title = HttpUtility.HtmlDecode(article.Title);
            Article = article;
            var ha = (HitomiArticle)(article);
            if (HitomiLog.Instance.Contains(ha.Magic))
                Downloaded.Visibility = Visibility.Visible;
            Title.Text = ha.Title;
            Artist.Text = ha.Artists != null ? ha.Artists[0] : "";

            Loaded += SearchMaterialElements_Loaded;
            MouseLeftButtonUp += SearchMaterialElements_MouseLeftButtonUp;
        }

        private void SearchMaterialElements_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Select = !Select;
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
                    Card.Background = new SolidColorBrush(Colors.Pink);
                    Card.Background.Opacity = 0.5;
                }
                else
                {
                    Card.Background = Brushes.White;
                }
            }
        }

        bool init = false;
        private void SearchMaterialElements_Loaded(object sender, RoutedEventArgs e)
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
                    try
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
                            BitmapImage.DownloadCompleted += B_DownloadCompleted;
                        }
                    }
                    catch (Exception ex)
                    {
                        Monitor.Instance.Push("[SME Error] " + ex.Message + "\r\n" + ex.StackTrace);
                    }

                    ImageCount.Text = ha.ImagesLink.Count + " Pages";
                    Image.Source = BitmapImage;
                    
                    if (ha.Artists != null && ha.Artists[0].ToUpper() != "N/A")
                    {
                        var stack = new StackPanel { Orientation = Orientation.Horizontal };
                        stack.Children.Add(new PackIcon { Kind = PackIconKind.Artist, Opacity = .56 });
                        stack.Children.Add(new TextBlock { Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Text = "작가 목록" });
                        var menu_item = new MenuItem { Header = stack };
                        foreach (var artist in ha.Artists)
                        {
                            var item = new MenuItem { Header = new TextBlock { Text = artist }, Tag = $"artist:{artist.Replace(' ', '_')}" };
                            item.Click += MenuItem_Click;
                            menu_item.Items.Add(item);
                        }
                        Menu.Items.Add(menu_item);
                    }
                    if (ha.Groups != null && ha.Groups[0].ToUpper() != "N/A")
                    {
                        var stack = new StackPanel { Orientation = Orientation.Horizontal };
                        stack.Children.Add(new PackIcon { Kind = PackIconKind.UserGroup, Opacity = .56 });
                        stack.Children.Add(new TextBlock { Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Text = "그룹 목록" });
                        var menu_item = new MenuItem { Header = stack };
                        foreach (var group in ha.Groups)
                        {
                            var item = new MenuItem { Header = new TextBlock { Text = group }, Tag = $"group:{group.Replace(' ', '_')}" };
                            item.Click += MenuItem_Click;
                            menu_item.Items.Add(item);
                        }
                        Menu.Items.Add(menu_item);
                    }
                    if (ha.Series != null)
                    {
                        var stack = new StackPanel { Orientation = Orientation.Horizontal };
                        stack.Children.Add(new PackIcon { Kind = PackIconKind.Book, Opacity = .56 });
                        stack.Children.Add(new TextBlock { Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Text = "시리즈 목록" });
                        var menu_item = new MenuItem { Header = stack };
                        foreach (var series in ha.Series)
                        {
                            MenuItem item = null;
                            if (KoreanSeries.SeriesMap(series) == series)
                                item = new MenuItem { Header = new TextBlock { Text = series }, Tag = $"series:{series.Replace(' ', '_')}" };
                            else
                                item = new MenuItem { Header = new TextBlock { Text = $"{series} ({KoreanSeries.SeriesMap(series)})" }, Tag = $"series:{series.Replace(' ', '_')}" };
                            item.Click += MenuItem_Click;
                            menu_item.Items.Add(item);
                        }
                        Menu.Items.Add(menu_item);
                    }
                    if (ha.Tags != null)
                    {
                        var stack = new StackPanel { Orientation = Orientation.Horizontal };
                        stack.Children.Add(new PackIcon { Kind = PackIconKind.Tag, Opacity = .56 });
                        stack.Children.Add(new TextBlock { Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Text = "태그 목록" });
                        var menu_item = new MenuItem { Header = stack };
                        foreach (var tag in ha.Tags)
                        {
                            MenuItem item = null;
                            if (KoreanTag.TagMap(tag) == tag)
                                item = new MenuItem { Header = new TextBlock { Text = tag }, Tag = $"{(tag.StartsWith("female:") || tag.StartsWith("male:") ? tag.Replace(' ', '_') : $"tag:{tag.Replace(' ', '_')}")}" };
                            else if (KoreanTag.TagMap(tag).Contains(':'))
                                item = new MenuItem { Header = new TextBlock { Text = $"{tag} ({KoreanTag.TagMap(tag).Split(':')[1]})" }, Tag = $"{(tag.StartsWith("female:") || tag.StartsWith("male:") ? tag.Replace(' ', '_') : $"tag:{tag.Replace(' ', '_')}")}" };
                            else
                                item = new MenuItem { Header = new TextBlock { Text = $"{tag} ({KoreanTag.TagMap(tag)})" }, Tag = $"{(tag.StartsWith("female:") || tag.StartsWith("male:") ? tag.Replace(' ', '_') : $"tag:{tag.Replace(' ', '_')}")}" };
                            item.Click += MenuItem_Click;
                            menu_item.Items.Add(item);
                        }
                        Menu.Items.Add(menu_item);
                    }


                }));
            });
        }

        private void B_DownloadCompleted(object sender, EventArgs e)
        {
        }

        public BitmapImage BitmapImage = new BitmapImage();

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new System.Action(
            delegate
            {
                if (Article is HitomiArticle ha)
                    (new ArticleInfoWindow(ha)).Show();
            }));
        }
        
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((MenuItem)(sender)).Tag.ToString();

            if (tag.StartsWith("artist:"))
                (new ArtistViewerWindow(tag.Substring("artist:".Length).Replace('_', ' '))).Show();
            else if (tag.StartsWith("group:"))
                (new GroupViewerWindow(tag.Substring("group:".Length).Replace('_', ' '))).Show();
            else
                (new FinderWindow(tag)).Show();
        }
    }
}
