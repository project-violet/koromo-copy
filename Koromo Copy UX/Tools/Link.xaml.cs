/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Utility;
using Koromo_Copy_UX.Utility;
using Koromo_Copy_UX.Utility.Bookmark;
using Koromo_Copy_UX.Utility.ZipArtists;
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

namespace Koromo_Copy_UX.Tools
{
    /// <summary>
    /// Link.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Link : UserControl
    {
        public Link()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();

            switch (tag)
            {
                case "SeriesManager":
                    (new SeriesManager()).Show();
                    break;
                case "ZipViewer":
                    (new ZipViewer()).Show();
                    break;
                case "ZipListing":
                    (new ZipListing()).Show();
                    break;
                case "ZipArtists":
                    (new ZipArtists()).Show();
                    break;
                case "MangaCrawler":
                    (new MangaCrawler()).Show();
                    break;
                case "CustomCrawler":
                    (new CustomCrawler()).Show();
                    break;
                case "ScriptEditor":
                    (new ScriptEditor()).Show();
                    break;
                case "RankSimulator":
                    (new RankSimulator()).Show();
                    break;
                case "Bookmark":
                    (new Bookmark()).Show();
                    break;
                case "Integrity":
                    (new ZipIntegrity()).Show();
                    break;

                case "HitomiExplorer":
                case "FsEnumerator":
                case "RelatedTagsTest":
                case "StringTools":
                case "GalleryBlockTester":
                case "Record":
                    UtilityDelegator.Run(tag);
                    break;
            }

        }
    }
}
