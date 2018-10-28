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

        IArticle article;

        public SearchElements(IArticle article)
        {
            InitializeComponent();

            this.article = article;

            Title.Text = article.Title;
            
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
