using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Koromo_Copy_UX2
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
        public bool Select { get { return _Select; } set {
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
            } }

        private void SearchElements_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Select = !Select;
        }

        public IArticle Article;

        public SearchElements(IArticle article)
        {
            InitializeComponent();

            Article = article;

            Title.Text = $"제목 : {article.Title}";
            Page.Text = article.ImagesLink.Count + " Pages";

            if (article is HitomiArticle)
            {
                var ha = article as HitomiArticle;
                //Artist.Text = $"작가 : {art}"
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
            }

            b.BeginInit();
            b.UriSource = new Uri(article.Thumbnail);
            b.EndInit();
            b.DownloadCompleted += B_DownloadCompleted;
            Image.Source = b;
        }

        private void B_DownloadCompleted(object sender, EventArgs e)
        {
            Image.Stretch = Stretch.Uniform;
            Image.Width = b.Width * 200 / b.Height;
        }
        
        BitmapImage b = new BitmapImage();
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
