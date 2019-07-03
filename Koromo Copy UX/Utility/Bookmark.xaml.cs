/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component;
using Koromo_Copy.Component.Hitomi;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    /// Bookmark.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Bookmark : Window
    {
        public Bookmark()
        {
            InitializeComponent();

            Instance = this;
            refresh();
        }

        public static Bookmark Instance;

        private void ClassifyTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = ClassifyTree.SelectedItem;
            if (item is TreeViewItem tvi)
            {
                if (tvi.DataContext != null)
                {
                    close_picturebox();
                    ContentControl.Content = tvi.DataContext;
                }
            }
        }

        private async void ClassButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BookmarkEditClass();
            if ((bool)(await DialogHost.Show(dialog, "BookmarkDialog")))
            {
                var sr = dialog.ClassifyRule;
                var lines = sr.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                );

                var used = new HashSet<string>();
                var root_classes = new List<string>();
                // parent class, class name
                var sub_classes = new List<Tuple<string, string>>();

                foreach (var r0 in lines)
                {
                    var r1 = r0.Trim();
                    if (r1 != "")
                    {
                        var ss = r1.Split('/');
                        if (!used.Contains(ss[1]))
                        {
                            used.Add(ss[1]);
                            root_classes.Add(ss[1]);
                        }
                        for (int i = 2; i < ss.Length; i++)
                        {
                            if (!used.Contains(ss[i]))
                            {
                                used.Add(ss[i]);
                                sub_classes.Add(new Tuple<string, string>(ss[i - 1], ss[i]));
                            }
                        }
                    }
                }

                BookmarkModelManager.Instance.Model.root_classes = root_classes;
                BookmarkModelManager.Instance.Model.sub_classes = sub_classes;
                BookmarkModelManager.Instance.Save();

                refresh();
            }
        }

        private void refresh()
        {
            ClassifyTree.Items.Clear();

            var name_dict = new Dictionary<string, TreeViewItem>();

            foreach (var root in BookmarkModelManager.Instance.Model.root_classes)
            {
                var tvi = new TreeViewItem
                {
                    Header = root,
                    DataContext = new BookmarkPage("/" + root),
                    AllowDrop = true
                };
                tvi.Drop += Tvi_Drop;
                tvi.DragEnter += Tvi_DragEnter;
                tvi.DragLeave += Tvi_DragLeave;
                name_dict.Add(root, tvi);
                ClassifyTree.Items.Add(tvi);
            }

            // Child, Parent
            var indegree = new Dictionary<string, string>();

            foreach (var sub in BookmarkModelManager.Instance.Model.sub_classes)
                indegree.Add(sub.Item2, sub.Item1);

            foreach (var sub in BookmarkModelManager.Instance.Model.sub_classes)
            {
                var fullname = "/" + sub.Item2;
                var nname = sub.Item2;

                while (indegree.ContainsKey(nname))
                {
                    nname = indegree[nname];
                    fullname = "/" + nname + fullname;
                }

                var tvi = new TreeViewItem
                {
                    Header = sub.Item2,
                    DataContext = new BookmarkPage(fullname),
                    AllowDrop = true
                };
                tvi.Drop += Tvi_Drop;
                tvi.DragEnter += Tvi_DragEnter;
                tvi.DragLeave += Tvi_DragLeave;
                name_dict.Add(sub.Item2, tvi);
                name_dict[sub.Item1].Items.Add(tvi);
            }

            ContentControl.Content = (ClassifyTree.Items[0] as TreeViewItem).DataContext;
        }

        private void Tvi_DragLeave(object sender, DragEventArgs e)
        {
            var tvi = sender as TreeViewItem;
            tvi.Foreground = Brushes.Black;
        }

        private void Tvi_DragEnter(object sender, DragEventArgs e)
        {
            var tvi = sender as TreeViewItem;
            tvi.Foreground = Brushes.Red;
        }

        private void Tvi_Drop(object sender, DragEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            refresh();
        }

        private void ToolButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BookmarkSetting();
            if ((bool)(await DialogHost.Show(dialog, "BookmarkDialog")))
            {
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #region 이미지

        double before_width = 0;
        double bb_width = 0;
        string latest_loaded = "";
        public string LastLoaded { get { return latest_loaded; } }

        public async void open_picturebox(string url)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                LeftImage.Source = null;
                LeftProgress.Visibility = Visibility.Visible;
            }));

            latest_loaded = url;

            if (PictureGrid.Width == 0)
            {
                MajorGrid.MinWidth = MajorGrid.ActualWidth;
                C1.Width = GridLength.Auto;
                before_width = Width;
                if (bb_width != 0)
                    Width = bb_width;
                else
                    Width += 350;
                C2.Width = new GridLength(1, GridUnitType.Star);
                PictureGrid.Width = Double.NaN;
            }

            if (url.StartsWith("http"))
                await Task.Run(() => LoadLeftImage(url, ""));
            else
                await Task.Run(async () => LoadLeftImage(await GetThumbnailAddress(url.ToInt32()), ""));
        }

        public void close_picturebox()
        {
            if (PictureGrid.Width != 0)
            {
                PictureGrid.Width = 0;
                bb_width = Width;
                Width = before_width;
                C1.Width = new GridLength(1, GridUnitType.Star);
                C2.Width = GridLength.Auto;
            }
        }

        Stream left_stream;

        private async Task<string> GetThumbnailAddress(int id)
        {
            try
            {
                var url = $"{HitomiCommon.HitomiGalleryBlock}{id}.html";
                lock (Monitor.Instance) Monitor.Instance.Push($"Download string: {url}");
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
                var har = HCommander.GetArticleData(id);
                if (!har.HasValue)
                {
                    await Application.Current.Dispatcher.BeginInvoke(new Action(
                    delegate
                    {
                        MessageBox.Show($"{id}를 로딩하지 못했습니다 ㅠㅠ", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                    return "";
                }
                return har.Value.Thumbnail;
            }
        }

        private void LoadLeftImage(string url, string referer)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Referer = referer;
                left_stream = req.GetResponse().GetResponseStream();
                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = left_stream;
                    bitmap.DownloadCompleted += LeftBitmap_DownloadCompleted;
                    bitmap.EndInit();
                    LeftImage.Source = bitmap;
                }));
            }
            catch { }
        }

        private void LeftBitmap_DownloadCompleted(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                LeftProgress.Visibility = Visibility.Collapsed;
            }));
            left_stream.Close();
            left_stream.Dispose();
        }

        #endregion
    }
}
