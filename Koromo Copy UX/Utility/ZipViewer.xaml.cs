/***

   Copyright (C) 2018-2020. dc-koromo. All Rights Reserved.
   
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

            if (url != "")
                LoadFolder(url);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
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
