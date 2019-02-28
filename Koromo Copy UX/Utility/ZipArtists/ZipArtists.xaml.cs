/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Fs;
using Koromo_Copy_UX.Domain;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Koromo_Copy_UX.Utility.ZipArtists
{
    /// <summary>
    /// ZipArtists.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipArtists : Window
    {
        public ZipArtists()
        {
            InitializeComponent();

            foreach (var page_number in PageNumberPanel.Children)
            {
                page_number_buttons.Add(page_number as Button);
            }
            initialize_page();

            logic = new AutoCompleteBase(algorithm, SearchText, AutoComplete, AutoCompleteList);
            
            SearchText.GotFocus += SearchText_GotFocus;
            SearchText.LostFocus += SearchText_LostFocus;
            Loaded += ZipArtists_Loaded;
        }

        #region IO

        private void ZipArtists_Loaded(object sender, RoutedEventArgs e)
        {
            show_elem_per_page = ZipArtistsModelManager.Instance.Setting.PerElements;
            init_scroll = ZipArtistsModelManager.Instance.Setting.InitScroll;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.B && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Explorer.OpenFolderAndSelectFiles(model.RootDirectory, model.ArtistList.Where(x => IsBookmarked(x.Value.ArtistName)).Select(x => model.RootDirectory + x.Key).ToArray());
            }
            else if (e.Key == Key.E && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (rating_model == null) return;
                var artists = new List<string>();
                rating_model.BookmarkCategory.ForEach(x => artists.Add(x.Item1));
                artists.Sort();
                var builder = new StringBuilder();
                artists.ForEach(x => builder.Append(x + "\r\n"));
                Monitor.Instance.Push("[Zip Artists] bookmark: \r\n" + builder);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var offset = AutoComplete.HorizontalOffset;
            AutoComplete.HorizontalOffset = offset + 1;
            AutoComplete.HorizontalOffset = offset;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            var offset = AutoComplete.HorizontalOffset;
            AutoComplete.HorizontalOffset = offset + 1;
            AutoComplete.HorizontalOffset = offset;
        }

        List<KeyValuePair<string, ZipArtistsArtistModel>> artist_list;
        ZipArtistsModel model;
        ZipArtistsRatingModel rating_model;

        /// <summary>
        /// 한 페이지에 표시될 작가의 개수입니다.
        /// </summary>
        int show_elem_per_page = 5;

        /// <summary>
        /// 페이지가 이동될 때마다 스크롤을 맨 위로 올립니다.
        /// 이 설정이 꺼져있는 경우 스크롤의 위치는 전 페이지의 위치와 동일하게 설정됩니다.
        /// </summary>
        bool init_scroll = true;

        /// <summary>
        /// 디렉토리를 탐색하고 데이터베이스 파일을 생성합니다.
        /// </summary>
        /// <param name="path"></param>
        private async void ProcessPath(string path)
        {
            FileIndexor fi = new FileIndexor();
            await fi.ListingDirectoryAsync(path);

            string root_directory = fi.RootDirectory;

            Dictionary<string, ZipArtistsArtistModel> artist_dic = new Dictionary<string, ZipArtistsArtistModel>();
            foreach (var x in fi.Directories)
            {
                await Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    ProgressText.Text = x.Item1;
                }));
                Dictionary<string, HitomiJsonModel> article_data = new Dictionary<string, HitomiJsonModel>();
                DateTime last_access_time = DateTime.MinValue;
                DateTime craete_time = DateTime.Now;
                foreach (var file in x.Item3)
                {
                    if (!file.FullName.EndsWith(".zip")) continue;
                    var zipFile = ZipFile.Open(file.FullName, ZipArchiveMode.Read);
                    if (zipFile.GetEntry("Info.json") == null) continue;
                    using (var reader = new StreamReader(zipFile.GetEntry("Info.json").Open()))
                    {
                        var json_model = JsonConvert.DeserializeObject<HitomiJsonModel>(reader.ReadToEnd());
                        article_data.Add(Path.GetFileName(file.FullName), json_model);
                    }
                    if (file.LastWriteTime < craete_time)
                        craete_time = file.LastWriteTime;
                    if (last_access_time < file.LastWriteTime)
                        last_access_time = file.LastWriteTime;
                }
                if (article_data.Count == 0) continue;
                artist_dic.Add(x.Item1.Substring(root_directory.Length), new ZipArtistsArtistModel { ArticleData = article_data, CreatedDate = craete_time.ToString(), LastAccessDate = last_access_time.ToString(), ArtistName = Path.GetFileName(Path.GetDirectoryName(x.Item1)), Size = (long)x.Item2});
            }

            model = new ZipArtistsModel();
            model.RootDirectory = root_directory;
            model.Tag = path;
            model.ArtistList = artist_dic.ToArray();
            var tick = DateTime.Now.Ticks;
            ZipArtistsModelManager.SaveModel($"zipartists-{Path.GetFileName(root_directory)}-{tick}.json", model);

            rate_filename = $"zipartists-{Path.GetFileName(root_directory)}-{tick}-rating.json";

            algorithm.Build(model);
            artist_list = artist_dic.ToList();
            elems.Clear();
            artist_list.ForEach(x => elems.Add(Tuple.Create(x, Tuple.Create(root_directory + x.Key, 0, false))));
            day_before = raws = elems;
            sort_data(align_column, align_row);

            await Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                CollectStatusPanel.Visibility = Visibility.Collapsed;
                ArticleCount.Text = $"작가 {artist_dic.Count.ToString("#,#")}명";
                PageCount.Text = $"작품 {artist_list.Select(x => x.Value.ArticleData.Count).Sum().ToString("#,#")}개";
                max_page = artist_dic.Count / show_elem_per_page;
                initialize_page();
            }));

            stack_clear();
            stack_push();
        }

        int align_row = 0;
        int align_column = 0;
        DateTime? starts;
        DateTime? ends;
        bool show_bookmark = false;
        private async void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;

            if (item.Tag.ToString() == "New")
            {
                var cofd = new CommonOpenFileDialog();
                cofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                cofd.IsFolderPicker = true;
                if (cofd.ShowDialog(this) == CommonFileDialogResult.Ok)
                {
                    CollectStatusPanel.Visibility = Visibility.Visible;
                    await Task.Run(() => ProcessPath(cofd.FileName));
                }
            }
            else if (item.Tag.ToString() == "Open")
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                ofd.Filter = "데이터 파일 (*.json)|*.json";
                if (ofd.ShowDialog() == true)
                {
                    // 열기
                    try
                    {
                        model = ZipArtistsModelManager.OpenModel(ofd.FileName);

                        var raw = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
                        rate_filename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ofd.FileName), raw + "-rating.json");

                        if (File.Exists(rate_filename))
                        {
                            rating_model = ZipArtistsModelManager.OpenRatingModel(rate_filename);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("옳바른 파일이 아닙니다!", "Zip Listing", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    bool offline = false;
                    if (!Directory.Exists(model.RootDirectory))
                    {
                        if (MessageBox.Show($"루트 디렉토리 \"{model.RootDirectory}\"를 찾을 수 없습니다! 디렉토리 위치가 변경되었다면 직접 루트 디렉토리를 수정해주세요!\r\n오프라인 모드로 열까요?", "Zip Listing", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
                            return;
                        offline = true;
                    }

                    algorithm.Build(model);
                    artist_list = model.ArtistList.ToList();

                    // 초기화
                    elems.Clear();
                    artist_list.ForEach(x => elems.Add(Tuple.Create(x, Tuple.Create(model.RootDirectory + x.Key, 0, offline))));
                    day_before = raws = elems;
                    sort_data(align_column, align_row);
                    ArticleCount.Text = $"작가 {artist_list.Count.ToString("#,#")}명";
                    PageCount.Text = $"작품 {artist_list.Select(x => x.Value.ArticleData.Count).Sum().ToString("#,#")}개";
                    max_page = artist_list.Count / show_elem_per_page;
                    initialize_page();
                    stack_clear();
                    stack_push();
                }
            }
            else if (item.Tag.ToString() == "Back")
            {
                stack_back();
            }
            else if (item.Tag.ToString() == "Forward")
            {
                stack_forward();
            }
            else if (item.Tag.ToString() == "Align")
            {
                var dialog = new ZipArtistsSorting(align_column, align_row);
                if ((bool)(await DialogHost.Show(dialog, "RootDialog")))
                {
                    int column = dialog.AlignColumnIndex;
                    int row = dialog.AlignRowIndex;

                    if (column == align_column && row == align_row) return;
                    sort_data(column, row);
                    align_column = column;
                    align_row = row;
                    initialize_page();

                    stack_push();
                }
            }
            else if (item.Tag.ToString() == "Filter")
            {
                if (raws.Count == 0) return;
                var dialog = new ZipArtistsFilter(raws.Select(x => DateTime.Parse(x.Item1.Value.LastAccessDate)).ToList(), starts, ends, show_bookmark);
                if ((bool)(await DialogHost.Show(dialog, "RootDialog")))
                {
                    if (dialog.StartDate.SelectedDate.HasValue)
                    {
                        starts = dialog.StartDate.SelectedDate;
                    }
                    if (dialog.EndDate.SelectedDate.HasValue)
                    {
                        ends = dialog.EndDate.SelectedDate.Value.AddMilliseconds(23 * 60 * 60 * 1000 + 59 * 60 * 1000 + 59 * 1000 + 999);
                    }
                    if (dialog.ShowBookmark.IsChecked.HasValue)
                    {
                        show_bookmark = dialog.ShowBookmark.IsChecked.Value;
                    }
                    elems = day_before;
                    filter_data();
                    max_page = elems.Count / show_elem_per_page;
                    initialize_page();

                    stack_push();
                }
            }
            else if (item.Tag.ToString() == "Statistics")
            {
                if (artist_list == null || artist_list.Count == 0) return;
                var dialog = new ZipArtistsStatistics(artist_list);
                await DialogHost.Show(dialog, "RootDialog");
            }
            else if (item.Tag.ToString() == "Tool")
            {
                var dialog = new ZipArtistsTool();
                await DialogHost.Show(dialog, "RootDialog");
            }
        }

        private void sort_data(int column, int row)
        {
            if (column == 0)
            {
                elems.Sort((x, y) => SortAlgorithm.ComparePath(x.Item1.Key, y.Item1.Key));
            }
            else if (column == 1)
            {
                elems.Sort((x, y) => DateTime.Parse(x.Item1.Value.LastAccessDate).CompareTo(DateTime.Parse(y.Item1.Value.LastAccessDate)));
            }
            else if (column == 2)
            {
                elems.Sort((x, y) => x.Item1.Value.Size.CompareTo(y.Item1.Value.Size));
            }
            else if (column == 3)
            {
                elems.Sort((x, y) => x.Item1.Value.ArtistName.CompareTo(y.Item1.Value.ArtistName));
            }
            else if (column == 4)
            {
                elems.Sort((x, y) => x.Item1.Value.ArticleData.Count.CompareTo(y.Item1.Value.ArticleData.Count));
            }
            //else if (column == 5)
            //{
            //    elems.Sort((x, y) => get_rate(Convert.ToInt32(x.Item1.Value.ArtistName)).CompareTo(get_rate(Convert.ToInt32(y.Item1.Value.ArtistName))));
            //}
            else if (column == 5)
            {
                elems.Sort((x, y) => x.Item1.Value.ArticleData.Select(z => z.Value.Pages).Sum().CompareTo(y.Item1.Value.ArticleData.Select(z => z.Value.Pages).Sum()));
            }
            else if (column == 6)
            {
                elems.Sort((x, y) => ((double)x.Item1.Value.ArticleData.Select(z => z.Value.Pages).Sum() / x.Item1.Value.ArticleData.Count).CompareTo(
                    ((double)y.Item1.Value.ArticleData.Select(z => z.Value.Pages).Sum() / y.Item1.Value.ArticleData.Count)));
            }

            if (row == 1) elems.Reverse();
        }

        private void filter_data()
        {
            if (starts.HasValue)
            {
                elems = elems.Where(x => DateTime.Parse(x.Item1.Value.LastAccessDate) >= starts).ToList();
            }
            if (ends.HasValue)
            {
                elems = elems.Where(x => DateTime.Parse(x.Item1.Value.LastAccessDate) <= ends).ToList();
            }
            if (show_bookmark && rating_model != null)
            {
                elems = elems.Where(x => rating_model.BookmarkCategory.Any(y => x.Item1.Value.ArtistName == y.Item1)).ToList();
            }
        }

        /// <summary>
        /// 원본 요소들 입니다.
        /// </summary>
        List<Tuple<KeyValuePair<string, ZipArtistsArtistModel>, Tuple<string, int, bool>>> raws = new List<Tuple<KeyValuePair<string, ZipArtistsArtistModel>, Tuple<string, int, bool>>>();

        /// <summary>
        /// 필터가 적용되지 않은 요소들 입니다.
        /// </summary>
        List<Tuple<KeyValuePair<string, ZipArtistsArtistModel>, Tuple<string, int, bool>>> day_before = new List<Tuple<KeyValuePair<string, ZipArtistsArtistModel>, Tuple<string, int, bool>>>();

        /// <summary>
        /// 페이저에 표시될 요소들 입니다.
        /// </summary>
        List<Tuple<KeyValuePair<string, ZipArtistsArtistModel>, Tuple<string, int, bool>>> elems = new List<Tuple<KeyValuePair<string, ZipArtistsArtistModel>, Tuple<string, int, bool>>>();
        private void show_page_impl(int page)
        {
            SeriesPanel.Children.Clear();

            for (int i = page * show_elem_per_page; i < (page + 1) * show_elem_per_page && i < elems.Count; i++)
            {
                SeriesPanel.Children.Add(new ZipArtistsElements(elems[i].Item2.Item1, elems[i].Item1.Value, elems[i].Item2.Item2, elems[i].Item2.Item3));
            }
        }

        #endregion

        #region Rating

        string rate_filename = "";
        private int get_rate(int magic)
        {
            //if (rating_model == null || rating_model.Rating == null) return 0;
            //if (rating_model.Rating.ContainsKey(magic))
            //    return rating_model.Rating[magic];
            return 0;
        }

        public void set_rate(int magic, int rate)
        {
            //if (rating_model == null)
            //    rating_model = new ZipArtistsRatingModel();
            //if (rating_model.Rating == null)
            //    rating_model.Rating = new Dictionary<int, int>();
            //if (rating_model.Rating.ContainsKey(magic))
            //    rating_model.Rating[magic] = rate;
            //else
            //    rating_model.Rating.Add(magic, rate);
            //ZipArtistsModelManager.SaveRatingModel(rate_filename, rating_model);
        }
        
        public bool IsBookmarked(string artist)
        {
            if (rating_model == null)
                return false;
            return rating_model.BookmarkCategory.Any(x => x.Item1 == artist);
        }

        public void AddBookmark(string artist, string category = "None")
        {
            if (rating_model == null)
            {
                rating_model = new ZipArtistsRatingModel();
                rating_model.BookmarkCategory = new List<Tuple<string, string, Color>>();
                rating_model.Rating = new Dictionary<string, int>();
            }

            if (rating_model.BookmarkCategory.Any(x => x.Item1 == artist && x.Item2 == category))
                return;

            rating_model.BookmarkCategory.Add(Tuple.Create(artist, category, Colors.White));
            ZipArtistsModelManager.SaveRatingModel(rate_filename, rating_model);
        }

        public void RemoveBookmark(string artist, string category = "None")
        {
            if (rating_model == null)
                return;

            rating_model.BookmarkCategory.RemoveAll(x => x.Item1 == artist && x.Item2 == category);
            ZipArtistsModelManager.SaveRatingModel(rate_filename, rating_model);
        }

        public Color GetBookmarkColor(string artist)
        {
            return rating_model.BookmarkCategory.First(x => x.Item1 == artist).Item3;
        }

        #endregion

        #region Pager

        int max_page = 0; // 1 ~ 250
        int current_page_segment = 0;
        int selected_page = 0;

        List<Button> page_number_buttons = new List<Button>();

        /// <summary>
        /// 페이저를 초기화합니다.
        /// </summary>
        /// <param name="show"></param>
        private void initialize_page(bool show = true)
        {
            current_page_segment = 0;
            page_number_buttons.ForEach(x => x.Visibility = Visibility.Visible);
            set_page_segment(0);
            if (show) show_page(0);
        }

        /// <summary>
        /// 특정 페이지로 이동합니다.
        /// </summary>
        /// <param name="i"></param>
        private void show_page(int i)
        {
            page_number_buttons.ForEach(x => {
                x.Background = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30));
                x.Foreground = new SolidColorBrush(Color.FromRgb(0x71, 0x71, 0x71));
            });
            page_number_buttons[i % 10].Background = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
            page_number_buttons[i % 10].Foreground = new SolidColorBrush(Color.FromRgb(0x17, 0x17, 0x17));

            show_page_impl(i);
            selected_page = i;

            if (init_scroll) ScrollViewer.ScrollToTop();
        }

        /// <summary>
        /// 페이저 세그먼트의 표시여부를 설정합니다.
        /// </summary>
        /// <param name="seg"></param>
        private void set_page_segment(int seg)
        {
            for (int i = 0, j = current_page_segment * 10; i < 10; i++, j++)
            {
                page_number_buttons[i].Content = (j + 1).ToString();

                if (j <= max_page)
                    page_number_buttons[i].Visibility = Visibility.Visible;
                else
                    page_number_buttons[i].Visibility = Visibility.Collapsed;
            }
        }

        private void PageNumber_Click(object sender, RoutedEventArgs e)
        {
            show_page(Convert.ToInt32((string)(sender as Button).Content) - 1);

            stack_push();
        }

        private void PageFunction_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Tag.ToString())
            {
                case "LeftLeft":
                    if (current_page_segment == 0) break;

                    current_page_segment = 0;
                    set_page_segment(0);
                    show_page(0);
                    break;

                case "Left":
                    if (current_page_segment == 0) break;

                    current_page_segment--;
                    set_page_segment(current_page_segment);
                    show_page(current_page_segment * 10);
                    break;

                case "Right":
                    if (max_page < 10) break;
                    if (current_page_segment == max_page / 10) break;

                    current_page_segment++;
                    set_page_segment(current_page_segment);
                    show_page(current_page_segment * 10);
                    break;

                case "RightRight":
                    if (max_page < 10) break;
                    if (current_page_segment == max_page / 10) break;

                    current_page_segment = max_page / 10;
                    set_page_segment(current_page_segment);
                    show_page(max_page);
                    break;
            }

            stack_push();
        }

        #endregion

        #region Stack

        public struct ZipListingStackElements
        {
            public int selected_page;
            public int max_page;
            public int current_page_segment;
            public double scroll_status;

            public string search_text;
            public DateTime? starts;
            public DateTime? ends;
            public bool show_bookmark;
            public int align_row;
            public int align_column;
        }

        List<ZipListingStackElements> status_stack = new List<ZipListingStackElements>();
        int stack_pointer = -1;

        private void stack_clear()
        {
            status_stack.Clear();
            stack_pointer = -1;
        }

        private void stack_push()
        {
            if (stack_pointer >= 0 && stack_pointer != status_stack.Count - 1)
            {
                status_stack.RemoveRange(stack_pointer + 1, status_stack.Count - stack_pointer - 1);
                stack_pointer = status_stack.Count - 1;
            }
            status_stack.Add(new ZipListingStackElements
            {
                selected_page = selected_page,
                max_page = max_page,
                current_page_segment = current_page_segment,
                scroll_status = ScrollViewer.VerticalOffset,

                align_column = align_column,
                align_row = align_row,
                search_text = latest_search_text,
                show_bookmark = show_bookmark,
                starts = starts,
                ends = ends,
            });
            stack_pointer++;
        }

        private void stack_back()
        {
            if (stack_pointer <= 0) return;
            stack_pointer--;
            stack_regression(stack_pointer);
        }

        private void stack_forward()
        {
            if (stack_pointer >= status_stack.Count - 1) return;
            stack_pointer++;
            stack_regression(stack_pointer);
        }

        private void stack_jump(int ptr)
        {
            stack_pointer = ptr;
            stack_regression(ptr);
        }

        string latest_search = "";
        private void stack_regression(int ptr)
        {
            var elem = status_stack[ptr];
            elems = raws;
            starts = elem.starts;
            ends = elem.ends;
            show_bookmark = elem.show_bookmark;
            align_row = elem.align_row;
            align_column = elem.align_column;
            if (latest_search != elem.search_text)
            {
                day_before = elems = Search(elem.search_text, raws);
                SearchText.Text = elem.search_text;
                latest_search = elem.search_text;
            }

            max_page = elem.max_page;
            current_page_segment = elem.current_page_segment;

            sort_data(align_column, align_row);
            filter_data();
            page_number_buttons.ForEach(x => x.Visibility = Visibility.Visible);
            set_page_segment(current_page_segment);
            show_page(elem.selected_page);
            ScrollViewer.ScrollToVerticalOffset(elem.scroll_status);
        }

        #endregion

        #region Search 

        private void SearchText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchText.Text))
                SearchText.Text = "검색";
        }

        private void SearchText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchText.Text == "검색")
                SearchText.Text = "";
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            PathIcon.Foreground = new SolidColorBrush(Color.FromRgb(0x9A, 0x9A, 0x9A));
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            PathIcon.Foreground = new SolidColorBrush(Color.FromRgb(0x71, 0x71, 0x71));
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PathIcon.Margin = new Thickness(2, 0, 0, 0);
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PathIcon.Margin = new Thickness(0, 0, 0, 0);
        }

        string latest_search_text = "";
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (SearchText.Text != "검색")
            {
                if (!string.IsNullOrEmpty(SearchText.Text.Trim()))
                {
                    day_before = elems = Search(SearchText.Text, raws);
                    SearchResult.Visibility = Visibility.Visible;
                    SearchResult.Text = $"검색결과: {elems.Count.ToString("#,#")}개";
                }
                else
                {
                    day_before = elems = raws;
                    SearchResult.Visibility = Visibility.Collapsed;
                }
                latest_search_text = SearchText.Text;
                filter_data();
                sort_data(align_column, align_row);
                max_page = elems.Count / show_elem_per_page;
                initialize_page();
                stack_push();
            }
        }

        public static List<Tuple<KeyValuePair<string, ZipArtistsArtistModel>, Tuple<string, int, bool>>> Search(string serach_text, List<Tuple<KeyValuePair<string, ZipArtistsArtistModel>, Tuple<string, int, bool>>> raw)
        {
            HitomiDataQuery query = new HitomiDataQuery();
            List<string> positive_data = new List<string>();
            List<string> negative_data = new List<string>();
            List<string> request_number = new List<string>();

            serach_text.Trim().Split(' ').ToList().ForEach((a) => { if (!a.Contains(":") && a.Trim() != "") positive_data.Add(a.Trim()); });
            query.Common = positive_data;
            foreach (var elem in from elem in serach_text.Trim().Split(' ') where elem.Contains(":") select elem)
            {
                if (elem.StartsWith("artist:"))
                    if (query.Artists == null)
                        query.Artists = new List<string>() { elem.Substring("artist:".Length) };
                    else
                        query.Artists.Add(elem.Substring("artist:".Length));
                else
                {
                    Koromo_Copy.Console.Console.Instance.WriteErrorLine($"Unknown rule '{elem}'.");
                    return null;
                }
            }

            var result = new List<Tuple<KeyValuePair<string, ZipArtistsArtistModel>, Tuple<string, int, bool>>>();

            for (int i = 0; i < raw.Count; i++)
            {
                if (query.Artists != null && !query.Artists.Contains(raw[i].Item1.Value.ArtistName.Replace(' ', '_')))
                    continue;
                if (query.Common.Count != 0 && !query.Common.Contains(raw[i].Item1.Value.ArtistName.Replace(' ', '_')))
                    continue;
                result.Add(raw[i]);
            }
            
            return result;
        }
        #endregion

        #region Search Helper

        ZipArtistsAutoComplete algorithm = new ZipArtistsAutoComplete();
        AutoCompleteBase logic;

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SearchText.Text != "검색")
                {
                    if (e.Key == Key.Return && !logic.skip_enter)
                    {
                        ButtonAutomationPeer peer = new ButtonAutomationPeer(SearchButton);
                        IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                        invokeProv.Invoke();
                        logic.ClosePopup();
                    }
                    logic.skip_enter = false;
                }
            }
        }

        private void SearchText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            AutoCompleteList.Width = SearchText.RenderSize.Width;
            logic.SearchText_PreviewKeyDown(sender, e);
        }

        private void SearchText_KeyUp(object sender, KeyEventArgs e)
        {
            AutoCompleteList.Width = SearchText.RenderSize.Width;
            logic.SearchText_KeyUp(sender, e);
        }

        private void AutoCompleteList_KeyUp(object sender, KeyEventArgs e)
        {
            AutoCompleteList.Width = SearchText.RenderSize.Width;
            logic.AutoCompleteList_KeyUp(sender, e);
        }

        private void AutoCompleteList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AutoCompleteList.Width = SearchText.RenderSize.Width;
            logic.AutoCompleteList_MouseDoubleClick(sender, e);
        }

        #endregion
    }
}
