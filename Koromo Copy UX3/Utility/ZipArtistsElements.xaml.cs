/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy_UX3.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX3.Utility
{
    /// <summary>
    /// ZipArtistsElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipArtistsElements : UserControl
    {
        string path;
        List<string> sub_folder;

        public ZipArtistsElements(string path, ZipArtistsArtistModel model, int rating, bool offline = false)
        {
            InitializeComponent();

            ArtistTextBox.Text = model.ArtistName;
            Date.Text = model.CreatedDate;
            ArticleCount.Text = model.ArticleData.Count + " Articles";

            this.path = path;
            sub_folder = model.ArticleData.Select(x => x.Key).ToList();
            sub_folder.Sort((x,y) => SortAlgorithm.ComparePath(y,x));

            var tags = new Dictionary<string, int>();
            foreach (var v in model.ArticleData)
                if (v.Value.Tags != null)
                    foreach (var tag in v.Value.Tags)
                        if (tags.ContainsKey(tag))
                            tags[tag] += 1;
                        else
                            tags.Add(tag, 1);

            var tag_list = tags.ToList();
            tag_list.Sort((x, y) => y.Value.CompareTo(x.Value));
            
            ScoreTextBox.Text = string.Join("\r\n", tag_list.Select(x => x.Key));

            Loaded += ZipArtistsElements_Loaded;
        }
        
        Stream[] load_stream = new Stream[5];
        ZipArchive[] archives = new ZipArchive[5];
        BitmapImage[] BitmapImage = new BitmapImage[5];

        bool IsDataLoaded = false;
        private void ZipArtistsElements_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsDataLoaded) return;
            IsDataLoaded = true;
            Task.Run(() =>
            {
                List<string> titles = new List<string>();
                List<string> paths = new List<string>();
                
                for (int i = 0, j = 0; i < 5 && j < sub_folder.Count; j++)
                {
                    string ttitle = sub_folder[i].Split('|')[0];
                    if (titles.Count > 0 && !titles.TrueForAll((title) => Strings.ComputeLevenshteinDistance(ttitle, title) > Settings.Instance.Hitomi.TextMatchingAccuracy)) continue;

                    titles.Add(ttitle);
                    paths.Add(path+sub_folder[i]);
                    i++;

                }
                require_count = paths.Count;
                loaded_count = 0;
                Image[] images = { Image1, Image2, Image3, Image4, Image5 };
                for (int i = 0; i < paths.Count; i++)
                {
                    archives[i] = ZipFile.Open(paths[i], ZipArchiveMode.Read);
                    var zipEntry = !archives[i].Entries[0].Name.EndsWith(".json") ? archives[i].Entries[0] : archives[i].Entries[1];
                    load_stream[i] = zipEntry.Open();

                    int j = i;
                    Application.Current.Dispatcher.BeginInvoke(new Action(
                    delegate
                    {
                        BitmapImage[j] = new BitmapImage();
                        BitmapImage[j].BeginInit();
                        BitmapImage[j].DecodePixelWidth = 150;
                        BitmapImage[j].StreamSource = load_stream[j];
                        BitmapImage[j].CacheOption = BitmapCacheOption.OnLoad;
                        BitmapImage[j].DownloadCompleted += ZipArtistsElements_DownloadCompleted;
                        BitmapImage[j].EndInit();
                    }));
                }
            });
        }

        int require_count;
        int loaded_count;

        private void ZipArtistsElements_DownloadCompleted(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                Image[] images = { Image1, Image2, Image3, Image4, Image5 };

                for (int i = 0; i < 5; i++)
                    if (BitmapImage[i] == sender)
                    {
                        images[i].Source = BitmapImage[i];
                        break;
                    }

                if (Interlocked.Increment(ref loaded_count) == require_count)
                {
                    for (int i = 0; i < require_count; i++)
                    {
                        load_stream[i].Close();
                        load_stream[i].Dispose();
                        archives[i].Dispose();
                    }
                }
            }));
        }

        private void MenuPopupButton_OnClick(object sender, RoutedEventArgs e)
        {

        }
        
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
        }
    }
}
