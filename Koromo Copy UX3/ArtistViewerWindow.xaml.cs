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

            DataContext = new Domain.ArtistDataGridViewModel();

            Task.Run(() =>
            {
                var result = HitomiDataParser.SearchAsync("artist:michiking").Result;
                _ = Task.Run(() => LoadThumbnail(result));
            });
        }

        public ArtistViewerWindow(string artist)
        {
            InitializeComponent();

            DataContext = new Domain.ArtistDataGridViewModel();

            var dictionary = new Dictionary<string, int>();
            Task.Run(() =>
            {
                var result = HitomiDataParser.SearchAsync($"artist:{artist.ToLower().Replace(' ', '_')}").Result;
                _ = Task.Run(() => LoadThumbnail(result));

                foreach (var md in result)
                {
                    if (md.Tags != null)
                        foreach (var tag in md.Tags)
                            if (dictionary.ContainsKey(tag))
                                dictionary[tag] += 1;
                            else
                                dictionary.Add(tag, 1);
                }
            }).ContinueWith(t => {
                var vm = DataContext as Domain.ArtistDataGridViewModel;
                var list = dictionary.ToList();
                list.Sort((a, b) => b.Value.CompareTo(a.Value));
                foreach (var tag in list)
                    vm.Items.Add(new Domain.ArtistDataGridItemViewModel
                    {
                        항목=tag.Key,
                        카운트=tag.Value
                    });
            }, TaskScheduler.FromCurrentSynchronizationContext());
            
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // We know longer need to size to the contents.
            ClearValue(SizeToContentProperty);
            // We want our control to shrink/expand with the window.
            ArticlePanel.ClearValue(WidthProperty);
            ArticlePanel.ClearValue(HeightProperty);
            TagList.ClearValue(WidthProperty);
            TagList.ClearValue(HeightProperty);
            // Don't want our window to be able to get any smaller than this.
            SetValue(MinWidthProperty, this.Width);
            SetValue(MinHeightProperty, this.Height);
        }
    }
}
