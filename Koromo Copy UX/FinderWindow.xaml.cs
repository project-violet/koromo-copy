/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy_UX.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace Koromo_Copy_UX
{
    /// <summary>
    /// FinderWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FinderWindow : Window
    {
        string searcher;
        public FinderWindow(string searcher = "")
        {
            InitializeComponent();

            DataContext = new Domain.FinderDataGridViewModel();

            SearchList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<FinderDataGridItemViewModel>(SearchList).SortHandler);
            Loaded += FinderWindow_Loaded;
            this.searcher = searcher;
        }

        private void FinderWindow_Loaded(object sender, RoutedEventArgs e)
        {
            logic = new AutoCompleteLogic(SearchText, AutoComplete, AutoCompleteList);
            SearchList.VerticalGridLinesBrush = SearchList.HorizontalGridLinesBrush;

            if (searcher != "")
            {
                SearchText.Text = searcher;
                SearchAsync(searcher);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();

            if (tag == "Search")
            {
                SearchAsync(SearchText.Text);
            }
        }

        public async void SearchAsync(string content)
        {
            try
            {
                List<HitomiIndexMetadata> result;
                Stopwatch sw = Stopwatch.StartNew();
                var end = sw.ElapsedMilliseconds;
                
                int start_element = 0;
                int count_element = 0;

                if (content.Contains('/'))
                {
                    var elem = content.Split(' ').Where(x => x.StartsWith("/")).ElementAt(0);
                    start_element = Convert.ToInt32(elem.Substring(1));
                    content = content.Replace(elem, " ");
                }

                if (content.Contains('?'))
                {
                    var elem = content.Split(' ').Where(x => x.StartsWith("?")).ElementAt(0);
                    count_element = Convert.ToInt32(elem.Substring(1));
                    content = content.Replace(elem, " ");
                }

                if (!Settings.Instance.Hitomi.UsingAdvancedSearch || content.Contains("recent:"))
                {
                    result = await HitomiDataParser.SearchAsync(content.Trim());
                    end = sw.ElapsedMilliseconds - end;
                    sw.Stop();
                    if (content.Contains("recent:"))
                    {
                        var elem = content.Split(' ').Where(x => x.StartsWith("recent:")).ElementAt(0);
                        int recent_count = 0;
                        int recent_start = 0;
                        if (elem.Substring("recent:".Length).Contains("-"))
                        {
                            recent_start = Convert.ToInt32(elem.Substring("recent:".Length).Split('-')[0]);
                            recent_count = Convert.ToInt32(elem.Substring("recent:".Length).Split('-')[1]);
                        }
                        else
                            recent_count = Convert.ToInt32(elem.Substring("recent:".Length));
                        SearchText.Text = "recent:" + (recent_start + recent_count) + "-" + recent_count;
                    }
                }
                else
                {
                    result = await HitomiDataSearchAdvanced.Search(content.Trim());
                    end = sw.ElapsedMilliseconds - end;
                    sw.Stop();
                }

                if (start_element != 0 && start_element <= result.Count) result.RemoveRange(0, start_element);
                if (count_element != 0 && count_element < result.Count) result.RemoveRange(count_element, result.Count - count_element);

                var vm = DataContext as Domain.FinderDataGridViewModel;
                result.Sort((a, b) => b.ID.CompareTo(a.ID));
                vm.Items.Clear();
                foreach (var article in result)
                    vm.Items.Add(new Domain.FinderDataGridItemViewModel
                    {
                        아이디 = article.ID.ToString(),
                        제목 = article.Name,
                        타입 = article.Type >= 0 ? HitomiIndex.Instance.index.Types[article.Type] : "",
                        작가 = string.Join(",", (article.Artists ?? Enumerable.Empty<int>()).Select(x => HitomiIndex.Instance.index.Artists[x])),
                        그룹 = string.Join(",", (article.Groups ?? Enumerable.Empty<int>()).Select(x => HitomiIndex.Instance.index.Groups[x])),
                        시리즈 = string.Join(",", (article.Parodies ?? Enumerable.Empty<int>()).Select(x => HitomiIndex.Instance.index.Series[x])),
                        캐릭터 = string.Join(",", (article.Characters ?? Enumerable.Empty<int>()).Select(x => HitomiIndex.Instance.index.Characters[x])),
                        업로드_시간 = HitomiDate.estimate_datetime(article.ID).ToString(),
                        태그 = string.Join(",", (article.Tags ?? Enumerable.Empty<int>()).Select(x => HitomiIndex.Instance.index.Tags[x])),
                        다운 = HitomiLog.Instance.Contains(article.ID.ToString()) ? "★" : ""
                    });

                ResultText.Text = $"{result.Count.ToString("#,#")}개 ({end / 1000.0} 초)";
            }
            catch
            {

            }
        }
        
        private void SearchList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SearchList.SelectedItems.Count > 0)
            {
                (new ArticleInfoWindow(
                    HitomiLegalize.MetadataToArticle(
                    HitomiLegalize.GetMetadataFromMagic((SearchList.SelectedItems[0] as FinderDataGridItemViewModel).아이디).Value)
                    )).Show();
            }
        }

        #region Search Helper
        AutoCompleteLogic logic;

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !logic.skip_enter)
            {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(SearchButton);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
            }
            logic.skip_enter = false;
        }

        private void SearchText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            logic.SearchText_PreviewKeyDown(sender, e);
        }

        private void SearchText_KeyUp(object sender, KeyEventArgs e)
        {
            logic.SearchText_KeyUp(sender, e);
        }

        private void AutoCompleteList_KeyUp(object sender, KeyEventArgs e)
        {
            logic.AutoCompleteList_KeyUp(sender, e);
        }

        private void AutoCompleteList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            logic.AutoCompleteList_MouseDoubleClick(sender, e);
        }
        #endregion
    }
}
