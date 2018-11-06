using Koromo_Copy.Component.EH;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using MaterialDesignThemes.Wpf;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// SearchElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SearchElements : UserControl
    {
        public SearchElements()
        {
            InitializeComponent();
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

        private void SearchElements_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Select = !Select;
        }

        public IArticle Article;

        public SearchElements(IArticle article)
        {
            InitializeComponent();

            Article = article;

            HitomiArticle ha = article as HitomiArticle;
            if (HitomiLog.Instance.Contains(ha.Magic))
                Downloaded.Visibility = Visibility.Visible;
            Title.Text = $"제목 : {ha.Title}";
            if (ha.Artists != null)
                InfoPanel.Children.Add(new TextBlock
                {
                    Text = $"작가 : " + string.Join(", ", ha.Artists),
                    FontSize = 17,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10, 0, 0, 0)
                });
            if (ha.Groups != null)
                InfoPanel.Children.Add(new TextBlock
                {
                    Text = $"그룹 : " + string.Join(", ", ha.Groups),
                    FontSize = 17,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10, 0, 0, 0)
                });
            if (ha.Series != null)
                InfoPanel.Children.Add(new TextBlock
                {
                    Text = $"시리즈 : " + string.Join(", ", ha.Series),
                    FontSize = 17,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10, 0, 0, 0)
                });
            if (ha.Characters != null)
                InfoPanel.Children.Add(new TextBlock
                {
                    Text = $"캐릭터 : " + string.Join(", ", ha.Characters),
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
                    e.FontSize = 10;
                    e.Margin = new Thickness(1, 1, 1, 1);

                    Tags.Children.Add(e);
                }
            }

            Loaded += SearchElements_Loaded;
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
                    b.BeginInit();
                    b.UriSource = new Uri(ha.Thumbnail);
                    b.EndInit();
                    b.DownloadCompleted += B_DownloadCompleted;
                    Page.Text = ha.ImagesLink.Count + " Pages";
                    Image.Source = b;
                    Image.Stretch = Stretch.Uniform;
                    Image.Width = b.Width * 200 / b.Height;
                }));
            });
        }

        private void B_DownloadCompleted(object sender, EventArgs e)
        {
            Image.Stretch = Stretch.Uniform;
            Image.Width = b.Width * 200 / b.Height;
        }

        BitmapImage b = new BitmapImage();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();

            HitomiArticle ha = Article as HitomiArticle;

            if (tag == "FindArtist")
            {
                if (ha.Artists != null)
                {
                    ArtistViewerWindow avw = new ArtistViewerWindow(ha.Artists[0]);
                    avw.Show();
                }
            }
            else if (tag == "ShowOnEX")
            {
                string result = ExHentaiTool.GetAddressFromMagicTitle(ha.Magic, ha.Title);
                if (result != "")
                    System.Diagnostics.Process.Start(result);
                else
                    // MessageBox.Show("익헨 주소를 찾지 못했습니다.", "Hitomi Copy",  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show("익헨 주소를 찾지 못했습니다.", "Koromo Copy", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
