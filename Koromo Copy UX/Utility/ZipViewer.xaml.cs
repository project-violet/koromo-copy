/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy_UX.Domain;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// ZipViewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipViewer : Window
    {
        public ZipViewer(string url = "")
        {
            InitializeComponent();

            Loaded += ZipViewer_Loaded;

            if (url != "")
                LoadFolder(url);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }

        private void ZipViewer_Loaded(object sender, RoutedEventArgs e)
        {
            string path = "https://cdn.clien.net/web/api/file/F01/5117849/e7482ad23fa6428693a.PNG?thumb=true";

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();

            WaterMark.Source = bitmap;
        }

        private void LoadFolder(string path)
        {
            var list = Directory.GetFiles(path).ToList();
            list.Sort((x, y) => SortAlgorithm.ComparePath(x, y));
            ImagePanel.Children.Clear();
            list.ForEach(x => {
                if (x.EndsWith(".zip"))
                    ImagePanel.Children.Add(new ZipViewerElements(x));
            });
        }

        private void ImagePanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                files.ToList().ForEach(x => LoadFolder(x));
            }
        }
    }
}
