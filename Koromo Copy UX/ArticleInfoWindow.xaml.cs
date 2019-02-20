/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component;
using Koromo_Copy.Component.EH;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Net;
using Koromo_Copy_UX.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Koromo_Copy_UX
{
    /// <summary>
    /// ArticleInfoWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ArticleInfoWindow : Window
    {
        public ArticleInfoWindow()
        {
            InitializeComponent();
        }

        public HitomiArticle Article;
        public ArticleInfoWindow(HitomiArticle article)
        {
            InitializeComponent();

            Article = article;
            Title.Text = $"제목 : {article.Title}";
            if (article.Artists != null)
                InfoPanel.Children.Add(new TextBlock
                {
                    Text = $"작가 : " + string.Join(", ", article.Artists),
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10, 0, 0, 0)
                });
            if (article.Groups != null)
                InfoPanel.Children.Add(new TextBlock
                {
                    Text = $"그룹 : " + string.Join(", ", article.Groups),
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10, 0, 0, 0)
                });
            if (article.Series != null)
                InfoPanel.Children.Add(new TextBlock
                {
                    Text = $"시리즈 : " + string.Join(", ", article.Series),
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10, 0, 0, 0)
                });
            if (article.Characters != null)
                InfoPanel.Children.Add(new TextBlock
                {
                    Text = $"캐릭터 : " + string.Join(", ", article.Characters),
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10, 0, 0, 0)
                });
            if (article.Tags != null)
            {
                foreach (var tag in article.Tags)
                {
                    var e = new Button();

                    string text = tag;
                    var stack_panel = new StackPanel { Orientation = Orientation.Horizontal };

                    if (tag.StartsWith("female:"))
                    {
                        e.Background = new SolidColorBrush(Color.FromRgb(0xE8, 0x60, 0xA0));
                        e.BorderBrush = e.Background;
                        text = tag.Substring("female:".Length);
                        stack_panel.Children.Add(new PackIcon { Kind = PackIconKind.GenderFemale, Opacity = 0.8, VerticalAlignment = VerticalAlignment.Center });
                    }
                    else if (tag.StartsWith("male:"))
                    {
                        e.Background = new SolidColorBrush(Color.FromRgb(0x00, 0xB1, 0xCF));
                        e.BorderBrush = e.Background;
                        text = tag.Substring("male:".Length);
                        stack_panel.Children.Add(new PackIcon { Kind = PackIconKind.GenderMale, Opacity = 0.8, VerticalAlignment = VerticalAlignment.Center });
                    }
                    else
                    {
                        e.Background = Brushes.LightGray;
                        e.BorderBrush = e.Background;
                    }

                    stack_panel.Children.Add(new TextBlock { FontSize = 11, Margin = new Thickness(2, 0, 3, 0), VerticalAlignment = VerticalAlignment.Center, Text = text });
                    e.Content = stack_panel;
                    e.Height = 20;
                    e.FontSize = 15;
                    e.Margin = new Thickness(1, 1, 1, 1);
                    e.Click += E_Click;
                    e.Tag = tag;

                    Tags.Children.Add(e);
                }
            }
            Date.Text = HitomiDate.estimate_datetime(Convert.ToInt32(article.Magic)).ToString();

            Loaded += SearchElements_Loaded;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }

        bool init = false;
        private void SearchElements_Loaded(object sender, EventArgs ex)
        {
            if (init) return;
            init = true;
            Task.Run(() =>
            {
                HitomiArticle ha = Article;
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
                        BitmapImage.DownloadCompleted += B_DownloadCompleted;
                    }
                    Page.Text = ha.ImagesLink.Count + " Pages";
                    Image.Source = BitmapImage;
                    Image.Stretch = Stretch.Uniform;
                    Image.Width = BitmapImage.Width * 500 / BitmapImage.Height;
                }));
            });
        }

        private void B_DownloadCompleted(object sender, EventArgs e)
        {
            Image.Stretch = Stretch.Uniform;
            Image.Width = BitmapImage.Width * 500 / BitmapImage.Height;
            UpdateLayout();
        }

        public BitmapImage BitmapImage = new BitmapImage();

        private void E_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString().Replace(' ', '_');
            (new FinderWindow(tag)).Show();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
                Close();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            DropShadow.BlurRadius = 8;
            DropShadow.Color = Colors.Gray;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            DropShadow.BlurRadius = 10;
            DropShadow.Color = SettingWrap.Instance.ThemeColor;
        }

        private void ThreeButton_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();
            if (tag == "Close")
            {
                Close();
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            ImageToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            ImageToolTip.HorizontalOffset = e.GetPosition((IInputElement)sender).X + 10;
            ImageToolTip.VerticalOffset = e.GetPosition((IInputElement)sender).Y;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Tag.ToString())
            {
                case "Preview":
                    (new PreviewWindow(Article)).Show();
                    break;

                case "Artist":
                    {
                        if (Article is HitomiArticle ha)
                        {
                            if (ha.Artists != null)
                                (new ArtistViewerWindow(ha.Artists[0])).Show();
                        }
                    }
                    break;

                case "Group":
                    {
                        if (Article is HitomiArticle ha)
                        {
                            if (ha.Groups != null)
                                (new GroupViewerWindow(ha.Groups[0])).Show();
                        }
                    }
                    break;

                case "":
                case "Hitomi":
                    {
                        if (Article is HitomiArticle ha)
                        {
                            System.Diagnostics.Process.Start($"{HitomiCommon.HitomiAddress}galleries/{ha.Magic}.html");
                        }
                    }
                    break;

                case "Exhentai":
                    {
                        if (Article is HitomiArticle ha)
                        {
                            string result = ExHentaiTool.GetAddressFromMagicTitle(ha.Magic, ha.Title);
                            if (result != "")
                                System.Diagnostics.Process.Start(result);
                            else
                                MainWindow.Instance.FadeOut_MiddlePopup("익헨 주소를 찾지 못했습니다 ㅠㅠ", false);
                        }
                    }
                    break;

                case "Series":
                    {
                        if (Article is HitomiArticle ha)
                        {
                            if (ha.Series != null)
                                (new FinderWindow($"series:{ha.Series[0].Replace(' ', '_')}")).Show();
                        }
                    }
                    break;

                case "Character":
                    {
                        if (Article is HitomiArticle ha)
                        {
                            if (ha.Characters != null)
                                (new FinderWindow($"character:{ha.Characters[0].Replace(' ', '_')}")).Show();
                        }
                    }
                    break;

                case "Comment":
                    {
                        if (Article is HitomiArticle ha)
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
                    break;

                case "Download":
                    {
                        if (Article is HitomiArticle ha)
                        {
                            var prefix = HitomiCommon.MakeDownloadDirectory(ha);
                            Directory.CreateDirectory(prefix);

                            if (!ha.IsUnstable)
                            {
                                DownloadSpace.Instance.RequestDownload(ha.Title,
                                    ha.ImagesLink.Select(y => HitomiCommon.GetDownloadImageAddress(ha.Magic, y)).ToArray(),
                                    ha.ImagesLink.Select(y => Path.Combine(prefix, y)).ToArray(),
                                    Koromo_Copy.Interface.SemaphoreExtends.Default, prefix, ha);
                            }
                            else
                            {
                                DownloaderHelper.ProcessUnstable(ha.UnstableModel);
                            }
                            MainWindow.Instance.FadeOut_MiddlePopup($"1개 항목 다운로드 시작...");
                            MainWindow.Instance.Activate();
                            MainWindow.Instance.FocusDownload();
                            Close();
                        }
                    }
                    break;
            }
        }
    }
}
