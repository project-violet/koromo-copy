/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Fs;
using Koromo_Copy_UX3.Domain;
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

namespace Koromo_Copy_UX3.Utility
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchText.Text) && SearchText.Text != "검색")
            {

            }
        }
        
        #endregion

        #region UI / IO
        
        List<KeyValuePair<string, ZipListingArticleModel>> article_list;
        ZipListingModel model;

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
            ZipListingModelManager.SaveModel($"ziplist-result-{DateTime.Now.Ticks}.json", model);

            algorithm.Build(model);
            article_list = article_dic.ToList();
            elems.Clear();
            article_list.ForEach(x => elems.Add(Tuple.Create(x, new Lazy<ZipListingElements>(() =>
            {
                return new ZipListingElements(root_directory + x.Key);
            }))));
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
        }

        int align_row = 0;
        int align_column = 0;
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
                    }
                    catch
                    {
                        MessageBox.Show("옳바른 파일이 아닙니다!", "Zip Listing", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!Directory.Exists(model.RootDirectory))
                    {
                        MessageBox.Show($"루트 디렉토리 {model.RootDirectory}를 찾을 수 없습니다! 루트 디렉토리를 수정해주세요!", "Zip Listing", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    algorithm.Build(model);
                    article_list = model.ArticleList.ToList();

                    // 초기화
                    elems.Clear();
                    article_list.ForEach(x => elems.Add(Tuple.Create(x, new Lazy<ZipListingElements>(() =>
                    {
                        return new ZipListingElements(model.RootDirectory + x.Key);
                    }))));
                    sort_data(align_column, align_row);
                    ArticleCount.Text = $"작품 {article_list.Count.ToString("#,#")}개";
                    PageCount.Text = $"이미지 {article_list.Select(x=>x.Value.ArticleData.Pages).Sum().ToString("#,#")}장";
                    max_page = article_list.Count / show_elem_per_page;
                    initialize_page();
                }
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
                }
            }
            else if (item.Tag.ToString() == "Filter")
            {

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
                elems.Sort((x, y) => x.Item1.Value.ArticleData.Id.CompareTo(y.Item1.Value.ArticleData.Id));
            }
            else if (column == 5)
            {
                elems.Sort((x, y) => x.Item1.Value.ArticleData.Pages.CompareTo(y.Item1.Value.ArticleData.Pages));
            }

            if (row == 1) elems.Reverse();
        }

        List<Tuple<KeyValuePair<string, ZipListingArticleModel>, Lazy<ZipListingElements>>> elems = new List<Tuple<KeyValuePair<string, ZipListingArticleModel>, Lazy<ZipListingElements>>>();
        private void show_page_impl(int page)
        {
            SeriesPanel.Children.Clear();

            for (int i = page * show_elem_per_page; i < (page + 1) * show_elem_per_page && i < elems.Count; i++)
            {
                SeriesPanel.Children.Add(elems[i].Item2.Value);
            }
        }

        #endregion

        #region Pager

        int max_page = 0; // 1 ~ 250
        int current_page_segment = 0;

        List<Button> page_number_buttons = new List<Button>();

        private void initialize_page()
        {
            current_page_segment = 0;
            page_number_buttons.ForEach(x => x.Visibility = Visibility.Visible);
            set_page_segment(0);
            show_page(0);
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
        }

        #endregion

        #region Search Helper
        ZipListingAutoComplete algorithm = new ZipListingAutoComplete();
        AutoCompleteBase logic;

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(SearchText.Text) && SearchText.Text != "검색")
                {
                    if (e.Key == Key.Return && !logic.skip_enter)
                    {
                        ButtonAutomationPeer peer = new ButtonAutomationPeer(SearchButton);
                        IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                        invokeProv.Invoke();
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
