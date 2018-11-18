using Koromo_Copy.Component.EH;
using Koromo_Copy.Net;
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
    /// CommentWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CommentWindow : Window
    {
        string url;

        public CommentWindow(string url)
        {
            InitializeComponent();

            this.url = url;
            Loaded += CommentWindow_Loaded;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }
        
        private void CommentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var html = NetCommon.DownloadExHentaiString(url);
            var article = ExHentaiParser.ParseArticleData(html);

            foreach (var comment in article.comment)
            {
                Domain.CommentViewModel cvm = new Domain.CommentViewModel
                {
                    Author = comment.Item2.Trim(),
                    Date = comment.Item1.ToString(),
                    Content = comment.Item3
                };

                Comments.Children.Add(new CommentElements { DataContext = cvm });
            }
        }
    }
}
