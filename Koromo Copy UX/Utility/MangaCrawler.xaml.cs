/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Mangashow;
using Koromo_Copy.Net;
using Koromo_Copy_UX.Domain;
using System;
using System.Collections.Generic;
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

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// MangaCrawler.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MangaCrawler : Window
    {
        public MangaCrawler()
        {
            InitializeComponent();

            foreach (var page_number in PageNumberPanel.Children)
            {
                page_number_buttons.Add(page_number as Button);
            }
            initialize_page();

            SearchText.GotFocus += SearchText_GotFocus;
            SearchText.LostFocus += SearchText_LostFocus;

            Loaded += MangaCrawler_Loaded;
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

        List<Tuple<string, string, string>> mangas = new List<Tuple<string, string, string>>();
        private void MangaCrawler_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                string base_url = "https://mangashow.me/bbs/page.php?hid=manga_list&page=";

                int max = MangashowmeParser.ParseMaxPage(NetCommon.DownloadString(base_url));
                var result = EmiliaJob.Instance.AddJob(Enumerable.Range(0, max).Select(x => base_url + x).ToList(),
                    (count) =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(
                        delegate
                        {
                            ProgressText.Text = $"가져오는 중...[{count}/{max}]";
                        }));
                    });

                List<Tuple<string, string>> articles = new List<Tuple<string, string>>();

                for (int i = 0; i < result.Count; i++)
                {
                    articles.AddRange(MangashowmeParser.ParseIndex(result[i]));
                }

                foreach (var article in articles)
                {
                    mangas.Add(Tuple.Create(article.Item1, article.Item2, article.Item2.Split('=').Last().Trim()));
                }

                mangas.Sort((x, y) => SortAlgorithm.ComparePath(x.Item3, y.Item3));

                foreach (var manga in mangas)
                {
                    elems.Add(new Lazy<MangaCrawlerElements>(() =>
                    {
                        return new MangaCrawlerElements(manga.Item1, manga.Item2, manga.Item3);
                    }));
                }

                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    CollectStatusPanel.Visibility = Visibility.Collapsed;

                    max_page = (mangas.Count - 1) / 36;
                    set_page_segment(0);
                    show_page(0);
                }));
            });
        }

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(SearchText.Text) && SearchText.Text != "검색")
                {
                    //SeriesPanel.Children.Insert(0, new SeriesManagerElements(SearchText.Text));
                }
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            SearchIcon.Foreground = new SolidColorBrush(Color.FromRgb(0x9A, 0x9A, 0x9A));
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            SearchIcon.Foreground = new SolidColorBrush(Color.FromRgb(0x71, 0x71, 0x71));
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchIcon.Margin = new Thickness(2, 0, 0, 0);
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SearchIcon.Margin = new Thickness(0, 0, 0, 0);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchText.Text) && SearchText.Text != "검색")
            {
                SeriesPanel.Children.Insert(0, new SeriesManagerElements(SearchText.Text));
                SearchText.Text = "검색";
            }
        }

        #region Pager

        int max_page = 0; // 1 ~ 250
        int current_page_segment = 0;

        List<Button> page_number_buttons = new List<Button>();

        private void initialize_page()
        {
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

        List<Lazy<MangaCrawlerElements>> elems = new List<Lazy<MangaCrawlerElements>>();

        private void show_page_impl(int page)
        {
            SeriesPanel.Children.Clear();

            for (int i = page * 36; i < (page + 1) * 36 && i < elems.Count; i++)
            {
                SeriesPanel.Children.Add(elems[i].Value);
            }
        }
    }
}
