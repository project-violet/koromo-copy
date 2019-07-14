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
                tvi.MouseMove += Tvi_MouseMove;
                tvi.PreviewMouseLeftButtonDown += Tvi_PreviewMouseLeftButtonDown;
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
                tvi.MouseMove += Tvi_MouseMove;
                tvi.PreviewMouseLeftButtonDown += Tvi_PreviewMouseLeftButtonDown;
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

        #region 드래그 앤 드롭 처리

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

        Point start;
        private void Tvi_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            start = e.GetPosition(null);
        }

        private void Tvi_MouseMove(object sender, MouseEventArgs e)
        {
            Point mpos = e.GetPosition(null);
            Vector diff = start - mpos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                DataObject data = new DataObject();
                data.SetData("classify", (sender as TreeViewItem).Tag);
                DragDrop.DoDragDrop(this, data, DragDropEffects.Move | DragDropEffects.Copy);
                ClassifyTree.AllowDrop = true;
            }
        }

        object drop_lock = new object();
        bool drop_checker = false;
        private async void Tvi_DropAsync(object sender, DragEventArgs e)
        {
            // Drop 이벤트에선 Bubbling이 일어나기 때문에 e.Handled를 true로 조작하거나
            // lock을 사용하여 처리해줄 필요가 있음
            // 단, lock을 사용할경우 unlock처리 지연을 위한 루틴이 하나이상 있어야함
            ClassifyTree.AllowDrop = false;
            var iscopy = (e.AllowedEffects & DragDropEffects.Copy) != 0 && Keyboard.IsKeyDown(Key.LeftCtrl);
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

                var message = $"{tt.Item2.Count}개 항목을 {tt.Item1}에서 {(sender as TreeViewItem).Tag}로 {(iscopy ? "복사할" : "옮길")}까요?";
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
            }
            else if (e.Data.GetDataPresent("classify"))
            {
                var from = e.Data.GetData("classify") as string;
                var to = (sender as TreeViewItem).Tag as string;

                lock (drop_lock)
                    if (drop_checker == true)
                    {
                        var tvi = sender as TreeViewItem;
                        tvi.Foreground = Brushes.Black;
                        return;
                    }
                lock (drop_lock)
                    drop_checker = true;

                if (from == to || (to.StartsWith(from) && !iscopy))
                {
                    var dialog = new BookmarkMessageOk($"같은 분류 또는 하위 분류로는 {(iscopy ? "복사할" : "옮길")} 수 없어요!");
                    await DialogHost.Show(dialog, "BookmarkDialog");
                    goto END;
                }

                await classify_move(from, to, iscopy);
            }
         END:
            lock (drop_lock)
                drop_checker = false;
            var tvi2 = sender as TreeViewItem;
            tvi2.Foreground = Brushes.Black;
        }

        private async void ClassifyTree_Drop(object sender, DragEventArgs e)
        {
            ClassifyTree.AllowDrop = false;
            if (e.Data.GetDataPresent("classify"))
            {
                var from = e.Data.GetData("classify") as string;
                var to = "/";
                var iscopy = (e.AllowedEffects & DragDropEffects.Copy) != 0 && Keyboard.IsKeyDown(Key.LeftCtrl);

                lock (drop_lock)
                    if (drop_checker == true)
                        return;

                if (!iscopy && (from == to || from.Count(x => x == '/') == 1))
                {
                    var dialog = new BookmarkMessageOk($"같은 분류 또는 하위 분류로는 {(iscopy ? "복사할" : "옮길")} 수 없어요!");
                    await DialogHost.Show(dialog, "BookmarkDialog");
                    return;
                }

                await classify_move(from, to, iscopy);
            }
        }

        private async Task classify_move(string from, string to, bool copy)
        {
            var name = from.Split('/').Last();
            var from_parent = from.Remove(from.Length - name.Length);
            var warning = false;
            var not_valid = false;
            if (to == "/")
            {
                foreach (var ss in BookmarkModelManager.Instance.Model.root_classes)
                    if (ss == "/" + name)
                        warning = true;
            }
            else
            {
                foreach (var ss in BookmarkModelManager.Instance.Model.sub_classes)
                    if (ss.Item1 == to && ss.Item2 == name)
                        warning = true;
            }

            if (from.Remove(from.Length - from.Split('/').Last().Length - 1) == to)
                not_valid = true;

            if (warning || not_valid)
            {
                if (!not_valid)
                {
                    var dialog_error = new BookmarkMessage($"경고! {to}에 이미 {name}이 존재하기 때문에 {(copy ? "복사할" : "옮길")} 수 없어요!\r\n그래도 모든 경고를 무시하고 {(copy ? "복사를" : "이동을")} 진행할까요?\r\n하위 항목들은 자동으로 합쳐져요.");
                    if (!(bool)(await DialogHost.Show(dialog_error, "BookmarkDialog")))
                        return;
                }
                else
                {
                    var dialog_error = new BookmarkMessageOk($"부모로 바로 연결되는 상위 분류로는 {(copy ? "복사할" : "옮길")} 수 없어요!");
                    await DialogHost.Show(dialog_error, "BookmarkDialog");
                    return;
                }
            }

            if (to == "/")
                to = "";
            var dialog = new BookmarkMessage($"{name}을 {from_parent}에서 {to + "/"}로 {(copy ? "복사할" : "옮길")}까요?");
            if ((bool)(await DialogHost.Show(dialog, "BookmarkDialog")))
            {
                if (copy)
                    copyto(from, to);
                else
                    moveto(from, to);
                BookmarkModelManager.Instance.Save();
                init();
            }
        }

        private bool check_class_exists(string path)
        {
            var ss = path.Split('/');

            if (BookmarkModelManager.Instance.Model.root_classes.Contains("/" + ss[1]))
                return true;

            var used = new HashSet<string>();
            foreach (var sc in BookmarkModelManager.Instance.Model.sub_classes)
                used.Add(sc.Item1 + "/" + sc.Item2);

            var parent = "/" + ss[1];
            for (int i = 2; i < ss.Length; i++)
            {
                if (used.Contains(parent + "/" + ss[i]))
                    return true;
                parent += "/" + ss[i];
            }

            return false;
        }
        
        private void create_class(string path)
        {
            var used = new HashSet<string>();
            foreach (var sc in BookmarkModelManager.Instance.Model.sub_classes)
                used.Add(sc.Item1 + "/" + sc.Item2);

            var sub_classes = new List<Tuple<string, string>>();
            var nname = path;
            var ss = nname.Split('/');
            var root = "/" + ss[1];
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
            if (!BookmarkModelManager.Instance.Model.root_classes.Contains(root))
                BookmarkModelManager.Instance.Model.root_classes.Add(root);
            BookmarkModelManager.Instance.Model.sub_classes.AddRange(sub_classes);
        }
        
        private void copyto(string from_path, string to_path)
        {
            copy_class(from_path, to_path);
            copy_items(from_path, to_path);
        }

        private void moveto(string from_path, string to_path)
        {
            copyto(from_path, to_path);
            remove_class_with_items(from_path);
        }

        private List<string> collect_subclasses(string path)
        {
            var cc = new List<string>();
            var queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                var fr = queue.Dequeue();
                cc.Add(fr.Substring(path.Length));

                foreach (var sc in BookmarkModelManager.Instance.Model.sub_classes)
                    if (sc.Item1 == fr)
                        queue.Enqueue(sc.Item1 + "/" + sc.Item2);
            }
            return cc;
        }

        private void copy_class(string /* donot pass root class */ from_path, string to_path)
        {
            var cc = collect_subclasses(from_path).Select(x => "/" + from_path.Split('/').Last() + x);
            foreach (var c1 in cc)
                create_class(to_path + c1);
        }

        private void copy_items(string from_path, string to_path)
        {
            var src = collect_subclasses(from_path);
            var from = src.Select(x => from_path + x).ToList();
            var to = collect_subclasses(from_path).Select(x => to_path + "/" + from_path.Split('/').Last() + x).ToList();

            var n_articles = new List<Tuple<string, BookmarkItemModel>>();
            var n_artists = new List<Tuple<string, BookmarkItemModel>>();
            var n_groups = new List<Tuple<string, BookmarkItemModel>>();
            var n_etcs = new List<Tuple<string, BookmarkEtcItemModel>>();

            for (int i = 0; i < from.Count; i++)
            {
                foreach (var item in BookmarkModelManager.Instance.Model.articles)
                    if (item.Item1 == from[i])
                        n_articles.Add(new Tuple<string, BookmarkItemModel>(to[i], item.Item2));
                foreach (var item in BookmarkModelManager.Instance.Model.artists)
                    if (item.Item1 == from[i])
                        n_artists.Add(new Tuple<string, BookmarkItemModel>(to[i], item.Item2));
                foreach (var item in BookmarkModelManager.Instance.Model.groups)
                    if (item.Item1 == from[i])
                        n_groups.Add(new Tuple<string, BookmarkItemModel>(to[i], item.Item2));
                foreach (var item in BookmarkModelManager.Instance.Model.etcs)
                    if (item.Item1 == from[i])
                        n_etcs.Add(new Tuple<string, BookmarkEtcItemModel>(to[i], item.Item2));
            }

            BookmarkModelManager.Instance.Model.articles.AddRange(n_articles);
            BookmarkModelManager.Instance.Model.artists.AddRange(n_artists);
            BookmarkModelManager.Instance.Model.groups.AddRange(n_groups);
            BookmarkModelManager.Instance.Model.etcs.AddRange(n_etcs);
        }

        private void remove_class_with_items(string path)
        {
            var cc = collect_subclasses(path).Select(x => path + x);

            // remove items
            foreach (var xx in cc)
            {
                for (int i = 0; i < BookmarkModelManager.Instance.Model.articles.Count; i++)
                    if (BookmarkModelManager.Instance.Model.articles[i].Item1 == xx)
                        BookmarkModelManager.Instance.Model.articles.RemoveAt(i--);
                for (int i = 0; i < BookmarkModelManager.Instance.Model.artists.Count; i++)
                    if (BookmarkModelManager.Instance.Model.artists[i].Item1 == xx)
                        BookmarkModelManager.Instance.Model.artists.RemoveAt(i--);
                for (int i = 0; i < BookmarkModelManager.Instance.Model.groups.Count; i++)
                    if (BookmarkModelManager.Instance.Model.groups[i].Item1 == xx)
                        BookmarkModelManager.Instance.Model.groups.RemoveAt(i--);
                for (int i = 0; i < BookmarkModelManager.Instance.Model.etcs.Count; i++)
                    if (BookmarkModelManager.Instance.Model.etcs[i].Item1 == xx)
                        BookmarkModelManager.Instance.Model.etcs.RemoveAt(i--);
            }

            // remove class
            foreach (var xx in cc)
            {
                var name = xx.Split('/').Last();
                var parent = xx.Remove(xx.Length - name.Length - 1);

                for (int i = 0; i < BookmarkModelManager.Instance.Model.sub_classes.Count; i++)
                    if (BookmarkModelManager.Instance.Model.sub_classes[i].Item1 == parent && BookmarkModelManager.Instance.Model.sub_classes[i].Item2 == name)
                    {
                        BookmarkModelManager.Instance.Model.sub_classes.RemoveAt(i);
                        break;
                    }
            }

            if (BookmarkModelManager.Instance.Model.root_classes.Contains(cc.First()))
                BookmarkModelManager.Instance.Model.root_classes.Remove(cc.First());
        }

        #endregion

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
