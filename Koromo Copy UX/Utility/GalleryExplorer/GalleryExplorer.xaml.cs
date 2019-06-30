/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using DCGallery.Domain;
using Koromo_Copy.Component.DC;
using Koromo_Copy_UX.Domain;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DCGallery
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GalleryExplorer : Window
    {
        public static GalleryExplorer Instance;

        public GalleryExplorer()
        {
            InitializeComponent();
            //Hide();

            var ofd = new OpenFileDialog();
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            ofd.Filter = "데이터 파일 (*.txt)|*.txt";
            if (ofd.ShowDialog() == false)
            {
                MessageBox.Show("데이터 파일이 없으면 이용할 수 없습니다!", "Skyrim Gallery", MessageBoxButton.OK, MessageBoxImage.Error);
                //Application.Current.Shutdown();
                Close();
                return;
            }

            SearchText.GotFocus += SearchText_GotFocus;
            SearchText.LostFocus += SearchText_LostFocus;

            ResultList.DataContext = new GalleryDataGridViewModel();
            ResultList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<GalleryDataGridItemViewModel>(ResultList).SortHandler);
            logic = new AutoCompleteBase2(algorithm, SearchText, AutoComplete, AutoCompleteList);
            
            //Monitor.Instance.ControlEnable = true;
            //Monitor.Instance.Push("Hello!");
            //Monitor.Instance.Start();

            Instance = this;

            DCGalleryAnalyzer.Instance.Open(ofd.FileName);
        }
        
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

        static void IntersectCountSplit(string[] target, List<string> source, ref bool[] check)
        {
            if (target != null)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    if (target.Any(e => e.ToLower().Split(' ').Any(x => x.Contains(source[i].ToLower()))))
                        check[i] = true;
                    else if (target.Any(e => e.ToLower().Replace(' ', '_') == source[i]))
                        check[i] = true;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var search = SearchText.Text;
            if (search == "검색") search = "";

            DCGalleryDataQuery query = new DCGalleryDataQuery();
            List<string> positive_data = new List<string>();

            search.Trim().Split(' ').ToList().ForEach((a) => { if (!a.Contains(":") && !a.StartsWith("/") && !a.StartsWith("?")) positive_data.Add(a.Trim()); });
            query.Title = positive_data;
            foreach (var elem in from elem in search.Trim().Split(' ') where elem.Contains(":") where !elem.StartsWith("/") where !elem.StartsWith("?") select elem)
            {
                if (elem.StartsWith("nick:"))
                    if (query.Nickname == null)
                        query.Nickname = new List<string>() { elem.Substring("nick:".Length) };
                    else
                        query.Nickname.Add(elem.Substring("nick:".Length));
                else if (elem.StartsWith("id:"))
                    if (query.Id == null)
                        query.Id = new List<string>() { elem.Substring("id:".Length) };
                    else
                        query.Id.Add(elem.Substring("id:".Length));
                else if (elem.StartsWith("ip:"))
                    if (query.Ip == null)
                        query.Ip = new List<string>() { elem.Substring("ip:".Length) };
                    else
                        query.Ip.Add(elem.Substring("ip:".Length));
                else if (elem.StartsWith("class:"))
                    if (query.Type == null)
                        query.Type = new List<string>() { elem.Substring("class:".Length) };
                    else
                        query.Type.Add(elem.Substring("class:".Length));
                else
                {
                    Koromo_Copy.Console.Console.Instance.WriteErrorLine($"Unknown rule '{elem}'.");
                }
            }

            var result = new List<DCPageArticle>();
            foreach (var article in DCGalleryAnalyzer.Instance.Articles)
            {
                if (query.Type != null)
                {
                    if (article.classify == null)
                        continue;
                    else if (article.classify != query.Type[0])
                        continue;
                }

                if (query.Nickname != null)
                {
                    if (article.nick == null)
                        continue;
                    else if (article.nick != query.Nickname[0])
                        continue;
                }

                if (query.Id != null)
                {
                    if (article.uid == null)
                        continue;
                    else if (article.uid != query.Id[0])
                        continue;
                }

                if (query.Ip != null)
                {
                    if (article.ip == null)
                        continue;
                    else if (article.ip != query.Ip[0])
                        continue;
                }

                if (query.Title != null)
                {
                    bool[] check = new bool[query.Title.Count];
                    IntersectCountSplit(article.title.Split(' '), query.Title, ref check);
                    if (!check.All((x => x)))
                        continue;
                }
                
                result.Add(article);
            }

            var tldx = ResultList.DataContext as GalleryDataGridViewModel;
            tldx.Items.Clear();
            foreach (var article in result)
            {
                tldx.Items.Add(new GalleryDataGridItemViewModel
                {
                    번호 = article.no,
                    제목 = article.title ?? "",
                    클래스 = article.classify ?? "",
                    날짜 = article.date.ToString(),
                    닉네임 = article.nick ?? "",
                    답글 = article.replay_num ?? "",
                    아이디 = article.uid != "" ? article.uid : $"({article.ip})",
                    조회수 = article.count ?? "",
                    추천수 = article.recommend ?? "",
                });
            }
        }

        #endregion

        #region Search Helper

        DCGalleryAutoComplete algorithm = new DCGalleryAutoComplete();
        AutoCompleteBase2 logic;

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
        
        private void ResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultList.SelectedItems.Count > 0)
            {
                var no = (ResultList.SelectedItems[0] as GalleryDataGridItemViewModel).번호;
                var id = DCGalleryAnalyzer.Instance.Model.gallery_id;

                if (DCGalleryAnalyzer.Instance.Model.is_minor_gallery)
                    Process.Start($"https://gall.dcinside.com/mgallery/board/view/?id={id}&no={no}");
                else
                    Process.Start($"https://gall.dcinside.com/board/view/?id={id}&no={no}");
            }
        }
    }
}
