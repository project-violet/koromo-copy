/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using Koromo_Copy_UX3.Domain;
using MaterialDesignThemes.Wpf;
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
using System.Windows.Shapes;

namespace Koromo_Copy_UX3
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
                    BitmapImage.DownloadCompleted += B_DownloadCompleted;
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
    }
}
