/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy_UX.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX.Utility.ZipArtists
{
    /// <summary>
    /// ZipArtistsElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipArtistsElements : UserControl
    {
        string path;
        List<string> sub_folder;
        List<string> magics;
        bool offline;

        public ZipArtistsElements(string path, ZipArtistsArtistModel model, int rating, bool offline = false)
        {
            InitializeComponent();

            ArtistTextBox.Text = model.ArtistName;
            Date.Text = model.LastAccessDate;
            ArticleCount.Text = model.ArticleData.Count + " Articles";

            this.path = path;
            sub_folder = model.ArticleData.Select(x => x.Key).ToList();
            sub_folder.Sort((x,y) => SortAlgorithm.ComparePath(y,x));
            magics = model.ArticleData.Select(x => x.Value.Id).ToList();
            magics.Sort((x, y) => y.ToInt32().CompareTo(x.ToInt32()));

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

            this.offline = offline;
            if (offline == true)
                OpenFolder.IsEnabled = false;

        }
        
        Stream[] load_stream = new Stream[5];
        ZipArchive[] archives = new ZipArchive[5];
        BitmapImage[] BitmapImage = new BitmapImage[5];
        string[] zip_paths = new string[5];

        bool IsDataLoaded = false;
        private void ZipArtistsElements_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsDataLoaded) return;
            IsDataLoaded = true;

            var parent = ((ZipArtists)Window.GetWindow(this));
            if (parent.IsBookmarked(ArtistTextBox.Text))
            {
                Bookmark.Kind = MaterialDesignThemes.Wpf.PackIconKind.Star;
                (BookmarkButton.FindResource("GlowOn") as Storyboard).Begin(BookmarkButton);
                Bookmark.Foreground = new SolidColorBrush(parent.GetBookmarkColor(ArtistTextBox.Text));
            }
            else
            {
                Bookmark.Kind = MaterialDesignThemes.Wpf.PackIconKind.StarOutline;
                (BookmarkButton.FindResource("GlowOff") as Storyboard).Begin(BookmarkButton);
            }

            Task.Run(() =>
            {
                List<string> titles = new List<string>();
                List<string> paths = new List<string>();
                List<string> ids = new List<string>();
                
                for (int i = 0, j = 0; i < 5 && j < sub_folder.Count; j++)
                {
                    string ttitle = sub_folder[i].Split('|')[0];
                    if (ZipArtistsModelManager.Instance.Setting.UsingTextMatchingAccuracy && titles.Count > 0 && !titles.TrueForAll((title) =>
                        Strings.ComputeLevenshteinDistance(ttitle, title) > ZipArtistsModelManager.Instance.Setting.TextMatchingAccuracy)) continue;

                    titles.Add(ttitle);
                    paths.Add(path+sub_folder[i]);
                    zip_paths[i] = (path + sub_folder[i]);
                    ids.Add(magics[i]);
                    i++;
                }

                if (!ZipArtistsModelManager.Instance.Setting.LoadFromOnline)
                    ImageLoadFromOffline(paths);
                else
                    ImageLoadFromOnline(ids);
            });
        }

        #region Image Loading

        private void ImageLoadFromOffline(List<string> paths)
        {
            require_count = paths.Count;
            loaded_count = 0;

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
        }

        private async Task<string> GetThumbnailAddress(string id)
        {
            try
            {
                var url = $"{HitomiCommon.HitomiGalleryBlock}{id}.html";
                lock (Koromo_Copy.Monitor.Instance) Koromo_Copy.Monitor.Instance.Push($"Download string: {url}");
                using (var wc = Koromo_Copy.Net.NetCommon.GetDefaultClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    wc.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                    wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36");
                    var html = await wc.DownloadStringTaskAsync(url);
                    return HitomiCommon.HitomiThumbnail + HitomiParser.ParseGalleryBlock(html).Thumbnail;
                }
            }
            catch
            {
                var har = HCommander.GetArticleData(Convert.ToInt32(id));
                if (!har.HasValue)
                    return "";
                return har.Value.Thumbnail;
            }
        }

        private void ImageLoadFromOnline(List<string> ids)
        {
            require_count = ids.Count;
            loaded_count = 0;

            for (int i = 0; i < ids.Count; i++)
            {
                int j = i;
                Task.Run(async () =>
                {
                    try
                    {
                        var req = WebRequest.Create(await GetThumbnailAddress(ids[j]));
                        load_stream[j] = req.GetResponse().GetResponseStream();
                        await Application.Current.Dispatcher.BeginInvoke(new Action(
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
                    catch (Exception e)
                    {
                        Koromo_Copy.Monitor.Instance.Push($"[Zip AritstsE] {e.Message}");
                    }
                });
            }
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
                        images[i].Tag = zip_paths[i];
                        break;
                    }

                if (Interlocked.Increment(ref loaded_count) == require_count)
                {
                    for (int i = 0; i < require_count; i++)
                    {
                        load_stream[i].Close();
                        load_stream[i].Dispose();

                        if (!ZipArtistsModelManager.Instance.Setting.LoadFromOnline)
                            archives[i].Dispose();
                    }
                }
            }));
        }

        #endregion

        private void MenuPopupButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button.Tag.ToString() == "Folder")
            {
                Process.Start(path);
            }
            else if (button.Tag.ToString() == "Detail")
            {
                (new ZipViewer(path)).Show();
            }
        }
        
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var parent = ((ZipArtists)Window.GetWindow(this));
            if (parent.IsBookmarked(ArtistTextBox.Text))
            {
                parent.RemoveBookmark(ArtistTextBox.Text);
                Bookmark.Kind = MaterialDesignThemes.Wpf.PackIconKind.StarOutline;
                (BookmarkButton.FindResource("GlowOff") as Storyboard).Begin(BookmarkButton);
            }
            else
            {
                parent.AddBookmark(ArtistTextBox.Text);
                Bookmark.Kind = MaterialDesignThemes.Wpf.PackIconKind.Star;
                (BookmarkButton.FindResource("GlowOn") as Storyboard).Begin(BookmarkButton);
                Bookmark.Foreground = new SolidColorBrush(parent.GetBookmarkColor(ArtistTextBox.Text));
            }
        }
        
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                if (((Image)sender).Tag is string path)
                {
                    Process.Start(path);
                }
            }
        }
    }
}
