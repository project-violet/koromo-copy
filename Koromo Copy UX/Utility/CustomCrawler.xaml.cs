/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using HtmlAgilityPack;
using Koromo_Copy.Html;
using Koromo_Copy.Net;
using Koromo_Copy.Net.Driver;
using Koromo_Copy_UX.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// CustomCrawler.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomCrawler : Window
    {
        public CustomCrawler()
        {
            InitializeComponent();

            HTMLList.DataContext = new CustomCrawlerDataGridViewModel();
            HTMLList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<CustomCrawlerDataGridItemViewModel>(HTMLList).SortHandler);
        }

        HtmlTree tree;
        string root_url;
        string original_url;
        private void URLButton_Click(object sender, RoutedEventArgs e)
        {
            if (original_url == URLText.Text)
            {
                HTMLList.DataContext = new CustomCrawlerDataGridViewModel(GetLoadResults());
                return;
            }
            try
            {
                original_url = URLText.Text;
                try { root_url = string.Join("/", URLText.Text.Split(new char[] { '/' }, 4), 0, 3); } catch { }

                if (driverCheck.IsChecked == false)
                {
                    string html;
                    if (!File.Exists(URLText.Text))
                        html = NetCommon.DownloadString(URLText.Text);
                    else
                        html = File.ReadAllText(URLText.Text);
                    tree = new HtmlTree(html);
                    tree.BuildTree();
                    HTMLList.DataContext = new CustomCrawlerDataGridViewModel(GetLoadResults());
                }
                else
                {
                    var driver = new SeleniumWrapper();
                    driver.Navigate(URLText.Text);
                    tree = new HtmlTree(driver.GetHtml());
                    tree.BuildTree();
                    driver.Close();
                    HTMLList.DataContext = new CustomCrawlerDataGridViewModel(GetLoadResults());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<CustomCrawlerDataGridItemViewModel> GetLoadResults()
        {
            var filter = new List<string>();
            if (aCheck.IsChecked == true)
                filter.Add("a");
            if (imgCheck.IsChecked == true)
                filter.Add("img");
            if (linkCheck.IsChecked == true)
                filter.Add("link");
            if (scriptCheck.IsChecked == true)
                filter.Add("script");
            if (divCheck.IsChecked == true)
                filter.Add("div");
            if (metaCheck.IsChecked == true)
                filter.Add("meta");
            if (textCheck.IsChecked == true)
                filter.Add("#text");

            var list = new List<CustomCrawlerDataGridItemViewModel>();
            int index = 0;
            for (int i = 0; i <= tree.Height; i++)
            {
                for (int j = 0; j < tree[i].Count; j++)
                {
                    if (!filter.Contains(tree[i][j].OriginalName))
                        continue;

                    var src = imgCheck.IsChecked == true ? tree[i][j].GetAttributeValue("data-src", "") : "";
                    if (imgCheck.IsChecked == true && string.IsNullOrEmpty(src))
                        src = tree[i][j].GetAttributeValue("src", "");
                    if ((aCheck.IsChecked == true || linkCheck.IsChecked == true) && string.IsNullOrEmpty(src))
                        src = tree[i][j].GetAttributeValue("href", "");
                    if ((scriptCheck.IsChecked == true || metaCheck.IsChecked == true) && string.IsNullOrEmpty(src))
                        src = tree[i][j].GetAttributeValue("content", "");
                    if ((divCheck.IsChecked == true) && string.IsNullOrEmpty(src))
                        src = tree[i][j].GetAttributeValue("style", "");
                    if (textCheck.IsChecked == true && tree[i][j].InnerText.Trim() != "" && tree[i][j].OriginalName == "#text")
                        src = tree[i][j].InnerText.Trim();
                    if (string.IsNullOrEmpty(src))
                        continue;

                    list.Add(new CustomCrawlerDataGridItemViewModel
                    {
                        인덱스 = index++.ToString(),
                        깊이 = i.ToString(),
                        네임 = tree[i][j].OriginalName,
                        특정항목 = src,
                        i = i,
                        j = j,
                    });
                }
            }

            return list;
        }

        private void HTMLList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (HTMLList.SelectedItems.Count > 0)
            {
                try
                {
                    var name = (HTMLList.SelectedItems[0] as CustomCrawlerDataGridItemViewModel).네임;
                    var src = (HTMLList.SelectedItems[0] as CustomCrawlerDataGridItemViewModel).특정항목;

                    if (!src.StartsWith("http://") && !src.StartsWith("https://") && !src.StartsWith("//"))
                        src = root_url + src;
                    else if (src.StartsWith("//"))
                        src = "http:" + src;

                    if (name == "img")
                        (new CustomCrawlerImage(src, original_url)).Show();
                    else
                        Process.Start(src);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void LCA_Click(object sender, RoutedEventArgs e)
        {
            if (HTMLList.SelectedItems.Count <= 1)
            {
                MessageBox.Show("두 개 이상 선택하세요!", Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            try
            {
                var p_list = HTMLList.SelectedItems.OfType<CustomCrawlerDataGridItemViewModel>().ToList();
                p_list.Sort((x, y) => Convert.ToInt32(x.인덱스).CompareTo(Convert.ToInt32(y.인덱스)));
                var list = p_list.Select(x => Tuple.Create(x.i, tree[x.i][x.j], $"({x.i},{x.j},{tree[x.i][x.j].ParentNode.ChildNodes.ToList().FindIndex(y => tree[x.i][x.j] == y)})")).ToList();
                var min_depth = list.Min(x => x.Item1);

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Item1 > min_depth)
                    {
                        var node = list[i].Item2;
                        string route = list[i].Item3;
                        for (int j = 0; j < list[i].Item1 - min_depth; j++)
                        {
                            var p_index = tree[list[i].Item1 - j - 1].FindIndex(x => node.ParentNode == x);
                            route = $"({list[i].Item1 - j - 1},{p_index},{tree[list[i].Item1 - j - 1][p_index].ParentNode.ChildNodes.ToList().FindIndex(y => tree[list[i].Item1 - j - 1][p_index] == y)})->{route}";
                            node = node.ParentNode;
                        }
                        list[i] = Tuple.Create(min_depth, node, route);
                    }
                }

                for (int i = min_depth; i > 0; i--)
                {
                    var pp = list[0].Item2;
                    if (list.TrueForAll(x => x.Item2 == pp))
                        break;
                    for (int j = 0; j < list.Count; j++)
                    {
                        var p_index = tree[i - 1].FindIndex(x => list[j].Item2.ParentNode == x);
                        list[j] = Tuple.Create(i - 1, list[j].Item2.ParentNode, $"({i - 1},{p_index},{tree[i - 1][p_index].ParentNode.ChildNodes.ToList().FindIndex(y => tree[i - 1][p_index] == y)})->{list[j].Item3}");
                    }
                }

                var ix = tree[list[0].Item1].FindIndex(x => list[0].Item2 == x);

                var builder = new StringBuilder();
                builder.Append("시작 깊이: " + min_depth + "\r\n");
                builder.Append($"LCA 위치: ({list[0].Item1},{ix})\r\n");
                builder.Append($"LCA 루트:\r\n");
                for (int i = 0; i < list.Count; i++)
                    builder.Append($"[{i.ToString().PadLeft(4, '0')}] {list[i].Item3}\r\n");
                MessageBox.Show(builder.ToString(), Title);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private HtmlNode GetLCANode(List<Tuple<int,HtmlNode>> list)
        {
            var min_depth = list.Min(x => x.Item1);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Item1 > min_depth)
                {
                    var node = list[i].Item2;
                    for (int j = 0; j < list[i].Item1 - min_depth; j++)
                    {
                        node = node.ParentNode;
                    }
                    list[i] = Tuple.Create(min_depth, node);
                }
            }

            for (int i = min_depth; i > 0; i--)
            {
                var pp = list[0].Item2;
                if (list.TrueForAll(x => x.Item2 == pp))
                    break;
                for (int j = 0; j < list.Count; j++)
                {
                    var p_index = tree[i - 1].FindIndex(x => list[j].Item2.ParentNode == x);
                    list[j] = Tuple.Create(i - 1, list[j].Item2.ParentNode);
                }
            }

            return list[0].Item2;
        }

        private List<string> ParsePatternString(string pp)
        {
            var tokens = new List<string>();

            for (int i = 0; i < pp.Length; i++)
            {
                var builder = new StringBuilder();
                builder.Append(pp[i]);
                if (char.IsNumber(pp[i]))
                {
                    while (i < pp.Length - 1 && char.IsNumber(pp[i+1]))
                        builder.Append(pp[++i]);
                }
                else
                {
                    while (i < pp.Length - 1 && !char.IsNumber(pp[i+1]))
                        builder.Append(pp[++i]);
                }
                tokens.Add(builder.ToString());
            }

            return tokens;
        }

        private List<bool> ClassifyTokens(List<string> tokens)
        {
            var isnum = new List<bool>(tokens.Count);
            tokens.ForEach(x =>
            {
                int nn;
                if (int.TryParse(x, out nn))
                    isnum.Add(true);
                else
                    isnum.Add(false);
            });
            return isnum;
        }

        private string ExtractPattern(List<string> list)
        {
            if (list.Count < 3) return "At least 3 inputs are required.";
            var std = ParsePatternString(list[0]);
            var fix = ClassifyTokens(std);
            var tokens = list.Select(x => ParsePatternString(x)).ToList();
            var classes = tokens.Select(x => ClassifyTokens(x)).ToList();

            for (int i = 0; i < std.Count; i++)
            {
                if (!classes[0][i])
                {
                    for (int j = 0; j < classes.Count; j++)
                        if (classes[j].Count != classes[0].Count || tokens[j][i] != tokens[0][i])
                            return "Pattern Not Found.";
                }
            }

            var numbers = new List<List<int>>();
            for (int i = 0; i < fix.Count; i++)
            {
                if (fix[i])
                {
                    if (tokens[0][i] == tokens[1][i])
                    {
                        fix[i] = false;
                        continue;
                    }
                    
                    numbers.Add(tokens.Select(x => Convert.ToInt32(x[i])).ToList());
                }
            }

            var pattern = new List<string>();
            for (int i = 0; i < numbers.Count; i++)
            {
                int a = numbers[i][0];
                int b = numbers[i][1];
                int c = numbers[i][2];

                if (c-b == b-a)
                {
                    if (a == 0)
                        pattern.Add($"i*{c - b}");
                    else
                        pattern.Add($"{a}+i*{c - b}");
                }
                else
                {
                    return "Patterns must be linear increment functions.";
                }
            }

            var builder = new StringBuilder();
            for (int i = 0, j = 0; i < fix.Count; i++)
            {
                if (fix[i])
                    builder.Append("{" + pattern[j++] + "}");
                else
                    builder.Append(std[i]);
            }

            return builder.ToString();
        }

        private void XPath_Click(object sender, RoutedEventArgs e)
        {
            if (HTMLList.SelectedItems.Count > 0)
            {
                try
                {
                    var p_list = HTMLList.SelectedItems.OfType<CustomCrawlerDataGridItemViewModel>().ToList();
                    p_list.Sort((x, y) => Convert.ToInt32(x.인덱스).CompareTo(Convert.ToInt32(y.인덱스)));
                    var list = p_list.Select(x => Tuple.Create(x.i, tree[x.i][x.j])).ToList();
                    var lca_node = GetLCANode(list);

                    var builder = new StringBuilder();
                    builder.Append("LCA XPath: " + lca_node.XPath + "\r\n");
                    builder.Append("XPaths:\r\n");
                    var xpaths = p_list.Select(x => tree[x.i][x.j].XPath.Substring(lca_node.XPath.Length)).ToList();
                    for (int i = 0; i < p_list.Count; i++)
                        builder.Append($"[{i.ToString().PadLeft(4, '0')}] {xpaths[i]}\r\n");
                    builder.Append("Pattern: " + ExtractPattern(xpaths));
                    MessageBox.Show(builder.ToString(), Title);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Attributes_Click(object sender, RoutedEventArgs e)
        {
            if (HTMLList.SelectedItems.Count != 1)
            {
                MessageBox.Show("하나의 요소만 선택하세요!", Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var elem = (CustomCrawlerDataGridItemViewModel)HTMLList.SelectedItems[0];
            var node = tree[elem.i][elem.j];

            var builder = new StringBuilder();
            for (int i = 0; i < node.Attributes.Count; i++)
                builder.Append($"[{i.ToString().PadLeft(2, '0')}] {node.Attributes[i].Name}=\"{node.Attributes[i].Value}\"\r\n");
            MessageBox.Show(builder.ToString(), Title);
        }

        private void Tree_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var p_list = HTMLList.SelectedItems.OfType<CustomCrawlerDataGridItemViewModel>().ToList();
                p_list.Sort((x, y) => Convert.ToInt32(x.인덱스).CompareTo(Convert.ToInt32(y.인덱스)));
                (new CustomCrawlerTree(tree[0][0], p_list.Select(x => tree[x.i][x.j]).ToList())).Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LCATree_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var p_list = HTMLList.SelectedItems.OfType<CustomCrawlerDataGridItemViewModel>().ToList();
                p_list.Sort((x, y) => Convert.ToInt32(x.인덱스).CompareTo(Convert.ToInt32(y.인덱스)));
                var list = p_list.Select(x => Tuple.Create(x.i, tree[x.i][x.j])).ToList();
                var lca_node = GetLCANode(list);

                (new CustomCrawlerTree(lca_node, p_list.Select(x => tree[x.i][x.j]).ToList())).Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CAL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (new CustomCrawlerCAL(tree[0][0])).Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = Filter.Text.Trim();

            if (text == "")
                HTMLList.DataContext = new CustomCrawlerDataGridViewModel(GetLoadResults());
            else
                HTMLList.DataContext = new CustomCrawlerDataGridViewModel(GetLoadResults().Where(x => x.특정항목.Contains(text)));
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            (new CustomCrawlerCreate()).Show();
        }

        private void Manual_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/dc-koromo/koromo-copy/blob/master/Document/CustomCrawler.md");
        }
    }
}
