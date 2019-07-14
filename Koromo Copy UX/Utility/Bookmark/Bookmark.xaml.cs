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

namespace Koromo_Copy_UX.Utility.Bookmark
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
            init();
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
                    if (r1 != "" && r1.Contains('/'))
                    {
                        var ss = r1.Split('/');
                        var root = "/" + ss[1];
                        if (!used.Contains(root))
                        {
                            used.Add(root);
                            root_classes.Add(root);
                        }
                        var parent = root;
                        for (int i = 2; i < ss.Length; i++)
                        {
                            if (!used.Contains(parent + "/" + ss[i]))
                            {
                                used.Add(parent + "/" + ss[i]);
                                sub_classes.Add(new Tuple<string, string>(parent, ss[i]));
                            }
                            parent += "/" + ss[i];
                        }
                    }
                }

                BookmarkModelManager.Instance.Model.root_classes = root_classes;
                BookmarkModelManager.Instance.Model.sub_classes = sub_classes;
                BookmarkModelManager.Instance.Save();

                init();
            }
        }

        TreeViewItem[] obj;
        private void init()
        {
            ClassifyTree.Items.Clear();

            var obj = new List<TreeViewItem>();
            var name_dict = new Dictionary<string, TreeViewItem>();

            foreach (var root in BookmarkModelManager.Instance.Model.root_classes)
            {
                var tvi = new TreeViewItem
                {
                    Header = root.Substring(1),
                    DataContext = new BookmarkPage(root),
                    AllowDrop = true,
                    Tag = root
                };
                tvi.Drop += Tvi_DropAsync;
                tvi.DragEnter += Tvi_DragEnter;
                tvi.DragLeave += Tvi_DragLeave;
                name_dict.Add(root, tvi);
                ClassifyTree.Items.Add(tvi);
                obj.Add(tvi);
            }

            // Child, Parent
            foreach (var sub in BookmarkModelManager.Instance.Model.sub_classes)
            {
                var fullname = sub.Item1 + "/" + sub.Item2;
                
                var tvi = new TreeViewItem
                {
                    Header = sub.Item2,
                    DataContext = new BookmarkPage(fullname),
                    AllowDrop = true,
                    Tag = fullname
                };
                tvi.Drop += Tvi_DropAsync;
                tvi.DragEnter += Tvi_DragEnter;
                tvi.DragLeave += Tvi_DragLeave;
                name_dict.Add(fullname, tvi);
                name_dict[sub.Item1].Items.Add(tvi);
                obj.Add(tvi);
            }

            this.obj = obj.ToArray();

            ContentControl.Content = (ClassifyTree.Items[0] as TreeViewItem).DataContext;
        }

        private void refresh()
        {
            foreach (var o in obj)
            {
                (o.DataContext as BookmarkPage).refresh();
            }
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

        object drop_lock = new object();
        bool drop_checker = false;
        private async void Tvi_DropAsync(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("registries"))
            {
                var tt = e.Data.GetData("registries") as Tuple<string, List<BookmarkPageDataGridItemViewModel>>;

                lock (drop_lock)
                    if (drop_checker == true)
                    {
                        var tvi = sender as TreeViewItem;
                        tvi.Foreground = Brushes.Black;
                        return;
                    }
                lock (drop_lock)
                    drop_checker = true;

                var message = $"{tt.Item2.Count}개 항목을 {tt.Item1}에서 {(sender as TreeViewItem).Tag}로 옮길까요?";
                var iscopy = false;
                if ((e.AllowedEffects & DragDropEffects.Copy) != 0)
                {
                    message = $"{tt.Item2.Count}개 항목을 {tt.Item1}에서 {(sender as TreeViewItem).Tag}로 복사할까요?";
                    iscopy = true;
                }
                var dialog = new BookmarkMessage(message);
                if ((bool)(await DialogHost.Show(dialog, "BookmarkDialog")))
                {
                    if (iscopy == false)
                    {
                        // 기존항목들 삭제
                        foreach (var ll in tt.Item2)
                        {
                            List<Tuple<string, BookmarkItemModel>> rl;
                            if (ll.유형 == "작가")
                                rl = BookmarkModelManager.Instance.Model.artists;
                            else if (ll.유형 == "그룹")
                                rl = BookmarkModelManager.Instance.Model.groups;
                            else
                                rl = BookmarkModelManager.Instance.Model.articles;

                            for (int i = 0; i < rl.Count; i++)
                            {
                                if (rl[i].Item1 == tt.Item1 && rl[i].Item2.Equals(ll.BIM))
                                {
                                    rl.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    }

                    // 추가
                    foreach (var ll in tt.Item2)
                    {
                        List<Tuple<string, BookmarkItemModel>> rl;
                        if (ll.유형 == "작가")
                            rl = BookmarkModelManager.Instance.Model.artists;
                        else if (ll.유형 == "그룹")
                            rl = BookmarkModelManager.Instance.Model.groups;
                        else
                            rl = BookmarkModelManager.Instance.Model.articles;

                        rl.Add(new Tuple<string, BookmarkItemModel>((sender as TreeViewItem).Tag.ToString(), ll.BIM));
                    }

                    BookmarkModelManager.Instance.Save();
                    refresh();
                }
                lock (drop_lock)
                    drop_checker = false;
            }
            var tvi2 = sender as TreeViewItem;
            tvi2.Foreground = Brushes.Black;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            init();
        }

        private async void ToolButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BookmarkTool();
            if ((bool)(await DialogHost.Show(dialog, "BookmarkDialog")))
            {

            }
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
                latest_loaded = "";
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
