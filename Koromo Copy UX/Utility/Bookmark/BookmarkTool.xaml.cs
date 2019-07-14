/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using MaterialDesignThemes.Wpf;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX.Utility.Bookmark
{
    /// <summary>
    /// BookmarkTool.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BookmarkTool : UserControl
    {
        public BookmarkTool()
        {
            InitializeComponent();
        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();
            switch (tag)
            {
                case "Import":
                case "Export":
                    break;

                case "Dangling":
                    {
                        var classes = new HashSet<string>();
                        foreach (var rc in BookmarkModelManager.Instance.Model.root_classes)
                            classes.Add(rc);
                        foreach (var sc in BookmarkModelManager.Instance.Model.sub_classes)
                            classes.Add($"{sc.Item1}/{sc.Item2}");

                        var dangling_articles = new List<Tuple<string, BookmarkItemModel>>();
                        var dangling_artists = new List<Tuple<string, BookmarkItemModel>>();
                        var dangling_groups = new List<Tuple<string, BookmarkItemModel>>();
                        var dangling_etcs = new List<Tuple<string, BookmarkEtcItemModel>>();
                        foreach (var elem in BookmarkModelManager.Instance.Model.articles)
                            if (!classes.Contains(elem.Item1))
                                dangling_articles.Add(elem);
                        foreach (var elem in BookmarkModelManager.Instance.Model.artists)
                            if (!classes.Contains(elem.Item1))
                                dangling_artists.Add(elem);
                        foreach (var elem in BookmarkModelManager.Instance.Model.groups)
                            if (!classes.Contains(elem.Item1))
                                dangling_groups.Add(elem);
                        foreach (var elem in BookmarkModelManager.Instance.Model.etcs)
                            if (!classes.Contains(elem.Item1))
                                dangling_etcs.Add(elem);

                        var total_cnt = dangling_articles.Count + dangling_artists.Count + dangling_etcs.Count + dangling_groups.Count;
                        if (total_cnt > 0)
                        {
                            var builder = new StringBuilder();
                            builder.Append($"총 {total_cnt}개의 떠돌이 항목이 검색됨\r\n\r\n");
                            if (dangling_articles.Count > 0)
                            {
                                builder.Append($"작품 떠돌이 항목 {dangling_articles.Count}개\r\n");
                                foreach (var item in dangling_articles)
                                    builder.Append($"{item.Item1} - {item.Item2.content} {item.Item2.etc} {item.Item2.stamp}\r\n");
                                builder.Append("\r\n");
                            }
                            if (dangling_artists.Count > 0)
                            {
                                builder.Append($"작가 떠돌이 항목 {dangling_artists.Count}개\r\n");
                                foreach (var item in dangling_artists)
                                    builder.Append($"{item.Item1} - {item.Item2.content} {item.Item2.etc} {item.Item2.stamp}\r\n");
                                builder.Append("\r\n");
                            }
                            if (dangling_groups.Count > 0)
                            {
                                builder.Append($"그룹 떠돌이 항목 {dangling_groups.Count}개\r\n");
                                foreach (var item in dangling_groups)
                                    builder.Append($"{item.Item1} - {item.Item2.content} {item.Item2.etc} {item.Item2.stamp}\r\n");
                                builder.Append("\r\n");
                            }
                            if (dangling_etcs.Count > 0)
                            {
                                builder.Append($"기타 떠돌이 항목 {dangling_etcs.Count}개\r\n");
                                foreach (var item in dangling_etcs)
                                    builder.Append($"{item.Item1} - {item.Item2.content} {item.Item2.etc} {item.Item2.stamp}\r\n");
                                builder.Append("\r\n");
                            }

                            var dialog = new BookmarkToolDangling(builder.ToString());
                            if ((bool)(await DialogHost.Show(dialog, "ToolDialog")))
                            {
                                var n_classes = new HashSet<string>();

                                foreach (var ic in dangling_articles)
                                {
                                    n_classes.Add(ic.Item1);
                                    BookmarkModelManager.Instance.Model.articles.Remove(ic);
                                    BookmarkModelManager.Instance.Model.articles.Add(new Tuple<string, BookmarkItemModel>("/떠돌이 항목" + ic.Item1, ic.Item2));
                                }
                                foreach (var ic in dangling_artists)
                                {
                                    n_classes.Add(ic.Item1);
                                    BookmarkModelManager.Instance.Model.artists.Remove(ic);
                                    BookmarkModelManager.Instance.Model.artists.Add(new Tuple<string, BookmarkItemModel>("/떠돌이 항목" + ic.Item1, ic.Item2));
                                }
                                foreach (var ic in dangling_groups)
                                {
                                    n_classes.Add(ic.Item1);
                                    BookmarkModelManager.Instance.Model.groups.Remove(ic);
                                    BookmarkModelManager.Instance.Model.groups.Add(new Tuple<string, BookmarkItemModel>("/떠돌이 항목" + ic.Item1, ic.Item2));
                                }
                                foreach (var ic in dangling_etcs)
                                {
                                    n_classes.Add(ic.Item1);
                                    BookmarkModelManager.Instance.Model.etcs.Remove(ic);
                                    BookmarkModelManager.Instance.Model.etcs.Add(new Tuple<string, BookmarkEtcItemModel>("/떠돌이 항목" + ic.Item1, ic.Item2));
                                }

                                var used = new HashSet<string>();
                                var sub_classes = new List<Tuple<string, string>>();
                                foreach (var cc in n_classes)
                                {
                                    var nname = "/떠돌이 항목" + cc;
                                    if (classes.Contains(nname))
                                        continue;
                                    
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
                                }
                                if (!BookmarkModelManager.Instance.Model.root_classes.Contains("/떠돌이 항목"))
                                    BookmarkModelManager.Instance.Model.root_classes.Add("/떠돌이 항목");
                                BookmarkModelManager.Instance.Model.sub_classes.AddRange(sub_classes);
                                BookmarkModelManager.Instance.Save();
                            }
                        }
                        else
                        {
                            var dialog = new BookmarkMessageOk("떠돌이 항목이 없습니다!");
                            await DialogHost.Show(dialog, "ToolDialog");
                        }
                    }
                    break;
            }

        }
    }
}
