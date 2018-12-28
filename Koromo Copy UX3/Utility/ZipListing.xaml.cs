/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Fs;
using Microsoft.Win32;
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
            
            PathText.GotFocus += PathText_GotFocus;
            PathText.LostFocus += PathText_LostFocus;
        }

        #region UI

        private void PathText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PathText.Text))
                PathText.Text = "경로";
        }

        private void PathText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PathText.Text == "경로")
                PathText.Text = "";
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
            if (!string.IsNullOrEmpty(PathText.Text) && PathText.Text != "경로")
            {
                CollectStatusPanel.Visibility = Visibility.Visible;
                string path = PathText.Text;
                Task.Run(() => ProcessPath(path));
            }
        }

        private void PathText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(PathText.Text) && PathText.Text != "경로")
                {
                    CollectStatusPanel.Visibility = Visibility.Visible;
                    string path = PathText.Text;
                    Task.Run(() => ProcessPath(path));
                }
            }
        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        #endregion

        #region UI / IO

        List<KeyValuePair<string, HitomiJsonModel>> article_list;
        ZipListingModel model;

        int show_elem_per_page = 20;

        private async void ProcessPath(string path)
        {
            FileIndexor fi = new FileIndexor();
            await fi.ListingDirectoryAsync(path);

            string root_directory = fi.RootDirectory;

            Dictionary<string, HitomiJsonModel> article_dic;
            article_dic = new Dictionary<string, HitomiJsonModel>();
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
                            article_dic.Add(file.FullName.Substring(root_directory.Length), json_model);
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

            article_list = article_dic.ToList();
            elems.Clear();
            article_list.ForEach(x => elems.Add(new Lazy<ZipListingElements>(() =>
            {
                return new ZipListingElements(root_directory + x.Key);
            })));
            
            await Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                CollectStatusPanel.Visibility = Visibility.Collapsed;
                ArticleCount.Text = $"작품 {article_dic.Count.ToString("#,#")}개";
                max_page = article_dic.Count / show_elem_per_page;
                initialize_page();
            }));
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;

            if (item.Tag.ToString() == "Open")
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                ofd.Filter = "데이터 파일 (*.json)|*.json";
                if (ofd.ShowDialog() == true)
                {
                    // 열기
                    model = ZipListingModelManager.OpenModel(ofd.FileName);
                    if (model == null || model.RootDirectory == null)
                    {
                        MessageBox.Show("옳바른 파일이 아닙니다!", "Zip Listing", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    article_list = model.ArticleList.ToList();
                    article_list.Sort((x, y) => y.Value.Pages.CompareTo(x.Value.Pages));

                    // 기본 필터링
                    article_list = article_list.Where(x => x.Value.Tags != null && x.Value.Tags.Contains("tankoubon")).ToList();

                    // 초기화
                    elems.Clear();
                    article_list.ForEach(x => elems.Add(new Lazy<ZipListingElements>(() =>
                    {
                        return new ZipListingElements(model.RootDirectory + x.Key);
                    })));
                    ArticleCount.Text = $"작품 {article_list.Count.ToString("#,#")}개";
                    max_page = article_list.Count / show_elem_per_page;
                    initialize_page();
                }
            }
        }

        List<Lazy<ZipListingElements>> elems = new List<Lazy<ZipListingElements>>();
        private void show_page_impl(int page)
        {
            SeriesPanel.Children.Clear();

            for (int i = page * show_elem_per_page; i < (page + 1) * show_elem_per_page && i < elems.Count; i++)
            {
                SeriesPanel.Children.Add(elems[i].Value);
            }
        }

        #endregion

        #region Pager

        int max_page = 0; // 1 ~ 250
        int current_page_segment = 0;

        List<Button> page_number_buttons = new List<Button>();

        private void initialize_page()
        {
            page_number_buttons.ForEach(x => x.Visibility = Visibility.Visible);
            if (max_page < 10)
            {
                for (int i = max_page + 1; i < 10; i++)
                    page_number_buttons[i].Visibility = Visibility.Collapsed;
            }
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

    }
}
