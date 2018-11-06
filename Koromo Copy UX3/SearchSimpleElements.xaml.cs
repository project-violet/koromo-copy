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
                Border.BorderBrush = Brushes.LightPink;
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
                    PinkRectangle.Fill = Brushes.Pink;
                    Border.BorderThickness = new Thickness(2);
                    Border.BorderBrush = Brushes.LightPink;
                    BorderCollapsed.BorderThickness = new Thickness(0);
                }
                else
                {
                    PinkRectangle.Fill = new SolidColorBrush(Color.FromArgb(100, 234, 202, 233));
                    Border.BorderThickness = new Thickness(1);
                    Border.BorderBrush = Brushes.LightPink;
                    BorderCollapsed.BorderThickness = new Thickness(1);
                }
            }
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
                    b.BeginInit();
                    b.UriSource = new Uri(ha.Thumbnail);
                    b.EndInit();
                    //b.DownloadCompleted += B_DownloadCompleted;
                    //.Text = ha.ImagesLink.Count + " Pages";
                    Title.Text = ha.Title;
                    Image.Source = b;
                }));
            });
        }

        BitmapImage b = new BitmapImage();
    }
}
