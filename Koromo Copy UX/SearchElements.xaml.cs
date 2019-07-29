/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component;
using Koromo_Copy.Component.EH;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using Koromo_Copy_UX.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Koromo_Copy_UX
{
    /// <summary>
    /// SearchElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SearchElements : UserControl
    {
        public SearchElements()
        {
            InitializeComponent();
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);
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
                    Background = new SolidColorBrush(Colors.Pink);
                    Background.Opacity = 0.5;
                    
                }
                else
                {
                    Background = Brushes.Transparent;
                }
            }
        }
        
        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Select = !Select;
        }

        public IArticle Article;

        public SearchElements(IArticle article)
        {
            InitializeComponent();
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);

            article.Title = HttpUtility.HtmlDecode(article.Title);
            Article = article;

            HitomiArticle ha = article as HitomiArticle;
            if (HitomiLog.Instance.Contains(ha.Magic))
                Downloaded.Visibility = Visibility.Visible;
            Title.Text = $"{FindResource("title")} : {ha.Title}";
            if (ha.Artists != null)
                InfoPanel.Children.Add(new TextBlock
                {
                    Text = $"{FindResource("artist")} : " + string.Join(", ", ha.Artists),
                    FontSize = 17,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10, 0, 0, 0)
                });
            if (ha.Groups != null)
                InfoPanel.Children.Add(new TextBlock
                {
                    Text = $"{FindResource("group")} : " + string.Join(", ", ha.Groups),
                    FontSize = 17,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10, 0, 0, 0)
                });
            if (ha.Series != null)
                InfoPanel.Children.Add(new TextBlock
                {
                    Text = $"{FindResource("series")} : " + string.Join(", ", ha.Series),
                    FontSize = 17,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10, 0, 0, 0)
                });
            if (ha.Characters != null)
                InfoPanel.Children.Add(new TextBlock
                {
                    Text = $"{FindResource("character")} : " + string.Join(", ", ha.Characters),
                    FontSize = 17,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10, 0, 0, 0)
                });
            if (ha.Tags != null)
            {
                foreach (var tag in ha.Tags)
                {
                    var e = new Button();
                    
                    string text = tag;
                    var stack_panel = new StackPanel { Orientation = Orientation.Horizontal };

                    if (tag.StartsWith("female:"))
                    {
                        e.Background = new SolidColorBrush(Color.FromRgb(0xE8, 0x60, 0xA0));
                        e.BorderBrush = e.Background;
                        text = tag.Substring("female:".Length);
                        stack_panel.Children.Add(new PackIcon
                        {
                            Kind = PackIconKind.GenderFemale,
                            Opacity = 0.8,
                            VerticalAlignment = VerticalAlignment.Center,
                        });
                    }
                    else if (tag.StartsWith("male:"))
                    {
                        e.Background = new SolidColorBrush(Color.FromRgb(0x00, 0xB1, 0xCF));
                        e.BorderBrush = e.Background;
                        text = tag.Substring("male:".Length);
                        stack_panel.Children.Add(new PackIcon
                        {
                            Kind = PackIconKind.GenderMale,
                            Opacity = 0.8,
                            VerticalAlignment = VerticalAlignment.Center,
                        });
                    }
                    else
                    {
                        e.Background = Brushes.LightGray;
                        e.BorderBrush = e.Background;
                    }

                    stack_panel.Children.Add(new TextBlock
                    {
                        FontSize = 11,
                        Margin = new Thickness(2, 0, 3, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = text
                    });

                    e.Content = stack_panel;
                    e.Height = 20;
                    e.FontSize = 10;
                    e.Margin = new Thickness(1, 1, 1, 1);
                    e.Click += E_Click;
                    e.Tag = tag;

                    Tags.Children.Add(e);
                }
            }
            Date.Text = HitomiDate.estimate_datetime(Convert.ToInt32(ha.Magic)).ToString();

            Loaded += SearchElements_Loaded;
        }
        
        private void E_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString().Replace(' ', '_');
            (new FinderWindow(tag)).Show();
        }

        bool init = false;
        private void SearchElements_Loaded(object sender, EventArgs ex)
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
                        Koromo_Copy.Console.Console.Instance.WriteErrorLine($"Cannot find '{ha.Magic}'! This item was not found in Hiyobi, Ex-Hentai, and Hitomi. Contact to the developer.\r\n" +
                            "Run program as an administrator, or use a DPI program or VPN.");
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
                        if (!ha.IsUnstable)
                            Page.Text = ha.ImagesLink.Count + " Pages";
                        else
                            Page.Text = ha.UnstableModel.Length + " Pages";
                        Image.Source = BitmapImage;
                        Image.Stretch = Stretch.Uniform;
                        Image.Width = BitmapImage.Width * 200 / BitmapImage.Height;
                    }
                    catch (Exception e)
                    {
                        Monitor.Instance.Push($"[Search Elements] isunstable={ha.IsUnstable} article_type={ha.UnstableModel}" + 
                            $" thumbnail={(ha.UnstableModel.Thumbnail ?? ha.Thumbnail)}" +
                            $"\r\n{e.Message}\r\n{e.StackTrace}");
                    }
                }));
            });
        }

        private void B_DownloadCompleted(object sender, EventArgs e)
        {
            Image.Stretch = Stretch.Uniform;
            Image.Width = BitmapImage.Width * 200 / BitmapImage.Height;
        }

        public BitmapImage BitmapImage = new BitmapImage();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();

            HitomiArticle ha = Article as HitomiArticle;

            if (tag == "FindArtist")
            {
                if (ha.Artists != null && ha.Artists[0].ToUpper() != "N/A")
                {
                    ArtistViewerWindow avw = new ArtistViewerWindow(ha.Artists[0]);
                    avw.Show();
                }
            }
            else if (tag == "FindGroup")
            {
                if (ha.Groups != null && ha.Groups[0].ToUpper() != "N/A")
                {
                    GroupViewerWindow avw = new GroupViewerWindow(ha.Groups[0]);
                    avw.Show();
                }
            }
            else if (tag == "Preview")
            {
                PreviewWindow pw = new PreviewWindow(Article);
                pw.Show();
            }
            else if (tag == "ShowOnEX")
            {
                string result = ExHentaiTool.GetAddressFromMagicTitle(ha.Magic, ha.Title);
                if (result != "")
                    System.Diagnostics.Process.Start(result);
                else
                {
                    MainWindow.Instance.FadeOut_MiddlePopup("익헨 주소를 찾지 못했습니다 ㅠㅠ", false);
                }
            }
            else if (tag == "Comment")
            {
                string result = ExHentaiTool.GetAddressFromMagicTitle(ha.Magic, ha.Title);
                if (result != "")
                {
                    (new CommentWindow(result)).Show();
                }
                else
                    MainWindow.Instance.FadeOut_MiddlePopup("익헨 주소를 찾지 못했습니다 ㅠㅠ", false);
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            ImageToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            ImageToolTip.HorizontalOffset = e.GetPosition((IInputElement)sender).X + 10;
            ImageToolTip.VerticalOffset = e.GetPosition((IInputElement)sender).Y;
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Select = false;
            Application.Current.Dispatcher.BeginInvoke(new System.Action(
            delegate
            {
                if (Article is HitomiArticle ha)
                    (new ArticleInfoWindow(ha)).Show();
            }));
        }
    }
}
