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
using System.Runtime.InteropServices;
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
using System.Windows.Shapes;

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// ZipListing.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipListing : Window
    {
        public ZipListing()
        {
            InitializeComponent();

            foreach (var page_number in PageNumberPanel.Children)
            {
                page_number_buttons.Add(page_number as Button);
            }
            initialize_page();

            logic = new AutoCompleteBase(algorithm, SearchText, AutoComplete, AutoCompleteList);

            KeyDown += ZipListing_KeyDown;

            SearchText.GotFocus += SearchText_GotFocus;
            SearchText.LostFocus += SearchText_LostFocus;
        }

        #region UI

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

        private void ZipListing_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < status_stack.Count; i++)
                {
                    builder.Append($"[{i + 1}] ");
                    builder.Append(JsonConvert.SerializeObject(status_stack[i]));
                    builder.Append("\r\n");
                }

                MessageBox.Show(builder.ToString(), "스택");
            }
        }

        #endregion

        #region Search

        public static List<Tuple<KeyValuePair<string, ZipListingArticleModel>, Lazy<ZipListingElements>>> Search(string serach_text, List<Tuple<KeyValuePair<string, ZipListingArticleModel>, Lazy<ZipListingElements>>> raw)
        {
            HitomiDataQuery query = new HitomiDataQuery();
            List<string> positive_data = new List<string>();
            List<string> negative_data = new List<string>();
            List<string> request_number = new List<string>();

            serach_text.Trim().Split(' ').ToList().ForEach((a) => { if (!a.Contains(":")) positive_data.Add(a.Trim()); });
            query.Common = positive_data;
            query.Common.Add("");
            foreach (var elem in from elem in serach_text.Trim().Split(' ') where elem.Contains(":") select elem)
            {
                if (elem.StartsWith("tag:"))
                    if (query.TagInclude == null)
                        query.TagInclude = new List<string>() { elem.Substring("tag:".Length) };
                    else
                        query.TagInclude.Add(elem.Substring("tag:".Length));
                else if (elem.StartsWith("female:"))
                    if (query.TagInclude == null)
                        query.TagInclude = new List<string>() { elem };
                    else
                        query.TagInclude.Add(elem);
                else if (elem.StartsWith("male:"))
                    if (query.TagInclude == null)
                        query.TagInclude = new List<string>() { elem };
                    else
                        query.TagInclude.Add(elem);
                else if (elem.StartsWith("artist:"))
                    if (query.Artists == null)
                        query.Artists = new List<string>() { elem.Substring("artist:".Length) };
                    else
                        query.Artists.Add(elem.Substring("artist:".Length));
                else if (elem.StartsWith("series:"))
                    if (query.Series == null)
                        query.Series = new List<string>() { elem.Substring("series:".Length) };
                    else
                        query.Series.Add(elem.Substring("series:".Length));
                else if (elem.StartsWith("group:"))
                    if (query.Groups == null)
                        query.Groups = new List<string>() { elem.Substring("group:".Length) };
                    else
                        query.Groups.Add(elem.Substring("group:".Length));
                else if (elem.StartsWith("character:"))
                    if (query.Characters == null)
                        query.Characters = new List<string>() { elem.Substring("character:".Length) };
                    else
                        query.Characters.Add(elem.Substring("character:".Length));
                else if (elem.StartsWith("tagx:"))
                    if (query.TagExclude == null)
                        query.TagExclude = new List<string>() { elem.Substring("tagx:".Length) };
                    else
                        query.TagExclude.Add(elem.Substring("tagx:".Length));
                else if (elem.StartsWith("type:"))
                    if (query.Type == null)
                        query.Type = new List<string>() { elem.Substring("type:".Length) };
                    else
                        query.Type.Add(elem.Substring("type:".Length));
                else
                {
                    Koromo_Copy.Console.Console.Instance.WriteErrorLine($"Unknown rule '{elem}'.");
                    return null;
                }
            }

            var result = new List<Tuple<KeyValuePair<string, ZipListingArticleModel>, Lazy<ZipListingElements>>>();

            for (int i = 0; i < raw.Count; i++)
            {
                var v = raw[i].Item1.Value.ArticleData;
                if (query.Common.Contains(v.Id.ToString()))
                {
                    result.Add(raw[i]);
                    continue;
                }
                if (query.TagExclude != null)
                {
                    if (v.Tags != null)
                    {
                        int intersec_count = 0;
                        foreach (var tag in query.TagExclude)
                        {
                            if (v.Tags.Any(vtag => vtag.ToLower().Replace(' ', '_') == tag.ToLower()))
                            {
                                intersec_count++;
                            }

                            if (intersec_count > 0) break;
                        }
                        if (intersec_count > 0) continue;
                    }
                }
                bool[] check = new bool[query.Common.Count];
                if (query.Common.Count > 0)
                {
                    HitomiDataSearch.IntersectCountSplit(v.Title.Split(' '), query.Common, ref check);
                    HitomiDataSearch.IntersectCountSplit(v.Tags, query.Common, ref check);
                    HitomiDataSearch.IntersectCountSplit(v.Artists, query.Common, ref check);
                    HitomiDataSearch.IntersectCountSplit(v.Groups, query.Common, ref check);
                    HitomiDataSearch.IntersectCountSplit(v.Series, query.Common, ref check);
                    HitomiDataSearch.IntersectCountSplit(v.Characters, query.Common, ref check);
                }
                bool connect = false;
                if (check.Length == 0) { check = new bool[1]; check[0] = true; }
                if (check[0] && v.Artists != null && query.Artists != null) { check[0] = HitomiDataSearch.IsIntersect(v.Artists, query.Artists); connect = true; } else if (query.Artists != null) check[0] = false;
                if (check[0] && v.Tags != null && query.TagInclude != null) { check[0] = HitomiDataSearch.IsIntersect(v.Tags, query.TagInclude); connect = true; } else if (query.TagInclude != null) check[0] = false;
                if (check[0] && v.Groups != null && query.Groups != null) { check[0] = HitomiDataSearch.IsIntersect(v.Groups, query.Groups); connect = true; } else if (query.Groups != null) check[0] = false;
                if (check[0] && v.Series != null && query.Series != null) { check[0] = HitomiDataSearch.IsIntersect(v.Series, query.Series); connect = true; } else if (query.Series != null) check[0] = false;
                if (check[0] && v.Characters != null && query.Characters != null) { check[0] = HitomiDataSearch.IsIntersect(v.Characters, query.Characters); connect = true; } else if (query.Characters != null) check[0] = false;
                if (check[0] && v.Types != null && query.Type != null) { check[0] = query.Type.Any(x => x == v.Types); connect = true; } else if (query.Type != null) check[0] = false;
                if (check.All((x => x)) && ((query.Common.Count == 0 && connect) || query.Common.Count > 0))
                    result.Add(raw[i]);
            }

            result.Sort((a, b) => Convert.ToInt32(b.Item1.Value.ArticleData.Id) - Convert.ToInt32(a.Item1.Value.ArticleData.Id));
            return result;
        }

        #endregion

        #region IO

        List<KeyValuePair<string, ZipListingArticleModel>> article_list;
        ZipListingModel model;
        ZipListingRatingModel rating_model;

        int show_elem_per_page = 20;

        private async void ProcessPath(string path)
        {
            FileIndexor fi = new FileIndexor();
            await fi.ListingDirectoryAsync(path);

            string root_directory = fi.RootDirectory;

            Dictionary<string, ZipListingArticleModel> article_dic = new Dictionary<string, ZipListingArticleModel>();
            foreach (var x in fi.Directories)
            {
                await Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    ProgressText.Text = x.Item1;
                }));
                foreach (var file in x.Item3)
                {
                    try
                    {
                        if (!file.FullName.EndsWith(".zip")) continue;
                        var zipFile = ZipFile.Open(file.FullName, ZipArchiveMode.Read);
                        if (zipFile.GetEntry("Info.json") == null) continue;
                        using (var reader = new StreamReader(zipFile.GetEntry("Info.json").Open()))
                        {
                            var json_model = JsonConvert.DeserializeObject<HitomiJsonModel>(reader.ReadToEnd());
                            article_dic.Add(file.FullName.Substring(root_directory.Length), new ZipListingArticleModel { ArticleData = json_model, Size = file.Length, CreatedDate = file.CreationTime.ToString() });
                        }
                    }
                    catch (Exception e)
                    {
                        Monitor.Instance.Push($"[Zip Listing] {e.Message}");
                    }
                }
            }

            model = new ZipListingModel();
            model.RootDirectory = root_directory;
            model.Tag = path;
            model.ArticleList = article_dic.ToArray();
            var tick = DateTime.Now.Ticks;
            ZipListingModelManager.SaveModel($"ziplist-result-{tick}.json", model);

            rate_filename = $"ziplist-result-{tick}-rating.json";

            algorithm.Build(model);
            article_list = article_dic.ToList();
            elems.Clear();
            article_list.ForEach(x => elems.Add(Tuple.Create(x, new Lazy<ZipListingElements>(() =>
            {
                return new ZipListingElements(root_directory + x.Key, x.Value.ArticleData, 0);
            }))));
            day_before = raws = elems;
            sort_data(align_column, align_row);

            await Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                CollectStatusPanel.Visibility = Visibility.Collapsed;
                ArticleCount.Text = $"작품 {article_dic.Count.ToString("#,#")}개";
                PageCount.Text = $"이미지 {article_list.Select(x => x.Value.ArticleData.Pages).Sum().ToString("#,#")}장";
                max_page = article_dic.Count / show_elem_per_page;
                initialize_page();
            }));

            stack_clear();
            stack_push();
        }

        int align_row = 0;
        int align_column = 0;
        DateTime? starts;
        DateTime? ends;
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
                        model = ZipListingModelManager.OpenModel(ofd.FileName);

                        var raw = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
                        rate_filename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ofd.FileName), raw + "-rating.json");

                        if (File.Exists(rate_filename))
                        {
                            rating_model = ZipListingModelManager.OpenRatingModel(rate_filename);
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
                    article_list = model.ArticleList.ToList();

                    // 초기화
                    elems.Clear();
                    article_list.ForEach(x => elems.Add(Tuple.Create(x, new Lazy<ZipListingElements>(() =>
                    {
                        return new ZipListingElements(model.RootDirectory + x.Key, x.Value.ArticleData, get_rate(Convert.ToInt32(x.Value.ArticleData.Id)), offline);
                    }))));
                    day_before = raws = elems;
                    sort_data(align_column, align_row);
                    ArticleCount.Text = $"작품 {article_list.Count.ToString("#,#")}개";
                    PageCount.Text = $"이미지 {article_list.Select(x=>x.Value.ArticleData.Pages).Sum().ToString("#,#")}장";
                    max_page = article_list.Count / show_elem_per_page;
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
                var dialog = new ZipListingSorting(align_column, align_row);
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
                var dialog = new ZipListingFilter(raws.Select(x => DateTime.Parse(x.Item1.Value.CreatedDate)).ToList(), starts, ends);
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
                    elems = day_before;
                    filter_data();
                    max_page = elems.Count / show_elem_per_page;
                    initialize_page();

                    stack_push();
                }
            }
            else if (item.Tag.ToString() == "Statistics")
            {
                if (article_list.Count == 0) return;
                var dialog = new ZipListingStatistics(article_list);
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
                elems.Sort((x, y) => DateTime.Parse(x.Item1.Value.CreatedDate).CompareTo(DateTime.Parse(y.Item1.Value.CreatedDate)));
            }
            else if (column == 2)
            {
                elems.Sort((x, y) => x.Item1.Value.Size.CompareTo(y.Item1.Value.Size));
            }
            else if (column == 3)
            {
                elems.Sort((x, y) => x.Item1.Value.ArticleData.Title.CompareTo(y.Item1.Value.ArticleData.Title));
            }
            else if (column == 4)
            {
                elems.Sort((x, y) => Convert.ToInt32(x.Item1.Value.ArticleData.Id).CompareTo(Convert.ToInt32(y.Item1.Value.ArticleData.Id)));
            }
            else if (column == 5)
            {
                elems.Sort((x, y) => x.Item1.Value.ArticleData.Pages.CompareTo(y.Item1.Value.ArticleData.Pages));
            }
            else if (column == 6)
            {
                elems.Sort((x, y) => get_rate(Convert.ToInt32(x.Item1.Value.ArticleData.Id)).CompareTo(get_rate(Convert.ToInt32(y.Item1.Value.ArticleData.Id))));
            }

            if (row == 1) elems.Reverse();
        }

        private void filter_data()
        {
            if (starts.HasValue)
            {
                elems = elems.Where(x => DateTime.Parse(x.Item1.Value.CreatedDate) >= starts).ToList();
            }
            if (ends.HasValue)
            {
                elems = elems.Where(x => DateTime.Parse(x.Item1.Value.CreatedDate) <= ends).ToList();
            }
        }

        List<Tuple<KeyValuePair<string, ZipListingArticleModel>, Lazy<ZipListingElements>>> raws = new List<Tuple<KeyValuePair<string, ZipListingArticleModel>, Lazy<ZipListingElements>>>();
        List<Tuple<KeyValuePair<string, ZipListingArticleModel>, Lazy<ZipListingElements>>> day_before = new List<Tuple<KeyValuePair<string, ZipListingArticleModel>, Lazy<ZipListingElements>>>();
        List<Tuple<KeyValuePair<string, ZipListingArticleModel>, Lazy<ZipListingElements>>> elems = new List<Tuple<KeyValuePair<string, ZipListingArticleModel>, Lazy<ZipListingElements>>>();
        private void show_page_impl(int page)
        {
            SeriesPanel.Children.Clear();

            for (int i = page * show_elem_per_page; i < (page + 1) * show_elem_per_page && i < elems.Count; i++)
            {
                SeriesPanel.Children.Add(elems[i].Item2.Value);
            }
        }

        public async void add_search_token(string token)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                if (SearchText.Text == "검색")
                    SearchText.Text = token;
                else
                    SearchText.Text = SearchText.Text + " " + token;
            }));
        }

        #endregion

        #region Rating

        string rate_filename = "";
        private int get_rate(int magic)
        {
            if (rating_model == null || rating_model.Rating == null) return 0;
            if (rating_model.Rating.ContainsKey(magic))
                return rating_model.Rating[magic];
            return 0;
        }

        public void set_rate(int magic, int rate)
        {
            if (rating_model == null)
                rating_model = new ZipListingRatingModel();
            if (rating_model.Rating == null)
                rating_model.Rating = new Dictionary<int, int>();
            if (rating_model.Rating.ContainsKey(magic))
                rating_model.Rating[magic] = rate;
            else
                rating_model.Rating.Add(magic, rate);
            ZipListingModelManager.SaveRatingModel(rate_filename, rating_model);
        }

        #endregion

        #region Pager

        int max_page = 0; // 1 ~ 250
        int current_page_segment = 0;
        int selected_page = 0;

        List<Button> page_number_buttons = new List<Button>();

        private void initialize_page(bool show = true)
        {
            current_page_segment = 0;
            page_number_buttons.ForEach(x => x.Visibility = Visibility.Visible);
            set_page_segment(0);
            if (show) show_page(0);
        }

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
        }

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
            initialize_page(false);
            show_page(elem.selected_page);
            ScrollViewer.ScrollToVerticalOffset(elem.scroll_status);
        }

        #endregion

        #region Search Helper
        ZipListingAutoComplete algorithm = new ZipListingAutoComplete();
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

        #region ShortCut

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (article_list.Count == 0) return;
                var dialog = new ZipListingPage(max_page);
                if ((bool)(await DialogHost.Show(dialog, "RootDialog")))
                {
                    int v = 0;
                    if (int.TryParse(dialog.Page.Text, out v))
                    {
                        v--;
                        if (v < 0 || v >= max_page) return;
                        current_page_segment = v / 10;
                        set_page_segment(current_page_segment);
                        show_page(v);
                        stack_push();
                    }
                }
            }
            else if (e.Key == Key.R && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (article_list.Count == 0) return;
                var artists = new Dictionary<string, int>();
                model.ArticleList.Select(x => x.Value).ToList().ForEach(x =>
                {
                    var artist = x.ArticleData.Artists != null ? x.ArticleData.Artists[0] : "";
                    if (artists.ContainsKey(artist))
                        artists[artist] += get_rate(Convert.ToInt32(x.ArticleData.Id));
                    else
                        artists.Add(artist, get_rate(Convert.ToInt32(x.ArticleData.Id)));
                });
                var list = artists.ToList();
                list.Sort((x, y) => y.Value.CompareTo(x.Value));
                list.ForEach(x => Monitor.Instance.Push($"{x.Key} [{x.Value}]"));
            }
        }

        #endregion
    }
}
