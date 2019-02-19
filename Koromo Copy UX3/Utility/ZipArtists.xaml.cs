/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Fs;
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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Koromo_Copy_UX3.Utility
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
        }

        #region IO

        List<KeyValuePair<string, ZipArtistsArtistModel>> artist_list;
        ZipArtistsModel model;
        ZipArtistsRatingModel rating_model;

        int show_elem_per_page = 20;

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
                            //article_dic.Add(file.FullName.Substring(root_directory.Length), new ZipListingArticleModel { ArticleData = json_model, Size = file.Length, CreatedDate = file.CreationTime.ToString() });
                            article_data.Add(Path.GetFileName(file.FullName), json_model);
                        }
                    }
                    catch (Exception e)
                    {
                        Monitor.Instance.Push($"[Zip Artists] {e.Message}");
                    }
                }
                if (article_data.Count == 0) continue;
                artist_dic.Add(x.Item1.Substring(root_directory.Length), new ZipArtistsArtistModel { ArticleData = article_data, CreatedDate = Directory.GetCreationTime(x.Item1).ToString(), ArtistName = Path.GetDirectoryName(x.Item1), Size= (long)x.Item2});
            }

            model = new ZipArtistsModel();
            model.RootDirectory = root_directory;
            model.Tag = path;
            model.ArtistList = artist_dic.ToArray();
            var tick = DateTime.Now.Ticks;
            ZipArtistsModelManager.SaveModel($"zipartists-result-{tick}.json", model);

            rate_filename = $"zipartists-result-{tick}-rating.json";
            
            artist_list = artist_dic.ToList();
            //elems.Clear();
            //artist_list.ForEach(x => elems.Add(Tuple.Create(x, new Lazy<ZipListingElements>(() =>
            //{
            //    return new ZipArtistsElements(root_directory + x.Key, x.Value.ArticleData, 0);
            //}))));
            //day_before = raws = elems;
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
        }

        private void show_page_impl(int page)
        {
            SeriesPanel.Children.Clear();

            //for (int i = page * show_elem_per_page; i < (page + 1) * show_elem_per_page && i < elems.Count; i++)
            //{
            //    SeriesPanel.Children.Add(elems[i].Item2.Value);
            //}
        }

        private void sort_data(int column, int row)
        {
            //if (column == 0)
            //{
            //    elems.Sort((x, y) => SortAlgorithm.ComparePath(x.Item1.Key, y.Item1.Key));
            //}
            //else if (column == 1)
            //{
            //    elems.Sort((x, y) => DateTime.Parse(x.Item1.Value.CreatedDate).CompareTo(DateTime.Parse(y.Item1.Value.CreatedDate)));
            //}
            //else if (column == 2)
            //{
            //    elems.Sort((x, y) => x.Item1.Value.Size.CompareTo(y.Item1.Value.Size));
            //}
            //else if (column == 3)
            //{
            //    elems.Sort((x, y) => x.Item1.Value.ArticleData.Title.CompareTo(y.Item1.Value.ArticleData.Title));
            //}
            //else if (column == 4)
            //{
            //    elems.Sort((x, y) => Convert.ToInt32(x.Item1.Value.ArticleData.Id).CompareTo(Convert.ToInt32(y.Item1.Value.ArticleData.Id)));
            //}
            //else if (column == 5)
            //{
            //    elems.Sort((x, y) => x.Item1.Value.ArticleData.Pages.CompareTo(y.Item1.Value.ArticleData.Pages));
            //}
            //else if (column == 6)
            //{
            //    elems.Sort((x, y) => get_rate(Convert.ToInt32(x.Item1.Value.ArticleData.Id)).CompareTo(get_rate(Convert.ToInt32(y.Item1.Value.ArticleData.Id))));
            //}
            //
            //if (row == 1) elems.Reverse();
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
            public int current_page_segment;
            public double scroll_status;
            
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
                current_page_segment = current_page_segment,
                scroll_status = ScrollViewer.VerticalOffset,

                align_column = align_column,
                align_row = align_row,
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
            //elems = raws;
            starts = elem.starts;
            ends = elem.ends;
            align_row = elem.align_row;
            align_column = elem.align_column;

            //max_page = elem.max_page;
            current_page_segment = elem.current_page_segment;

            //sort_data(align_column, align_row);
            //filter_data();
            initialize_page(false);
            show_page(elem.selected_page);
            ScrollViewer.ScrollToVerticalOffset(elem.scroll_status);
        }

        #endregion
    }
}
