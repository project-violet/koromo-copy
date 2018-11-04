/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
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

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// ArtistViewerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ArtistViewerWindow : Window
    {
        public ArtistViewerWindow()
        {
            InitializeComponent();

            Task.Run(() =>
            {
                var result = HitomiDataParser.SearchAsync("artist:michiking").Result;
                _ = Task.Run(() => LoadThumbnail(result));
            });
        }

        public ArtistViewerWindow(string artist)
        {
            InitializeComponent();

            Task.Run(() =>
            {
                var result = HitomiDataParser.SearchAsync($"artist:{artist.ToLower().Replace(' ', '_')}").Result;
                _ = Task.Run(() => LoadThumbnail(result));
            });
        }
        
        private void LoadThumbnail(List<HitomiMetadata> md)
        {
            List<Task> task = new List<Task>();
            foreach (var metadata in md)
            {
                Task.Run(() => LoadThumbnail(metadata));
                Thread.Sleep(100);
            }
        }

        private void LoadThumbnail(HitomiMetadata md)
        {
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                // Put code that needs to run on the UI thread here
                var se = new SearchSimpleElements(HitomiLegalize.MetadataToArticle(md));
                ArticlePanel.Children.Add(se);
                Koromo_Copy.Monitor.Instance.Push("[AddSearchElements] Hitomi Metadata " + md.ID);
            }));
        }
    }
}
