/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Html;
using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Koromo_Copy_UX
{
    /// <summary>
    /// PreviewWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PreviewWindow : Window
    {
        public PreviewWindow()
        {
            InitializeComponent();
        }

        public PreviewWindow(IArticle article)
        {
            InitializeComponent();

            Article = article;
            Loaded += PreviewWindow_Loaded;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }

        public IArticle Article;

        private void PreviewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => LoadImages());
        }

        private void LoadImages()
        {
            for (int i = 0; i < Article.ImagesLink.Count; i++)
            {
                Application.Current.Dispatcher.Invoke(new Action(
                delegate
                {
                    string address = Article.ImagesLink[i];
                    if (Article is HitomiArticle ha)
                    {
                        address = HitomiCommon.GetDownloadImageAddress(ha.Magic, address);
                    }
                    ImageStack.Children.Add(new PreviewImageElements($"{i + 1} Page", address));
                }));
                Thread.Sleep(100);
            }

#if DEBUG
            if (Article is HitomiArticle ha2)
            {
                HtmlLocalServer.Instance.CreateImageServer(ha2.Title, ha2.ImagesLink.Select(x => HitomiCommon.GetDownloadImageAddress(ha2.Magic, x)).ToArray());
            }
#endif
        }
    }
}
