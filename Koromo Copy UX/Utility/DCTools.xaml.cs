/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.DC;
using Koromo_Copy.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// DCTools.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DCTools : Window
    {
        public DCTools()
        {
            InitializeComponent();

            Loaded += DCTools_Loaded;
        }

        SortedDictionary<string, string> gallery_list = new SortedDictionary<string, string>();
        SortedDictionary<string, string> mgallery_list = new SortedDictionary<string, string>();
        SortedDictionary<string, string> gl = new SortedDictionary<string, string>();

        bool loaded = false;
        private void DCTools_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded) return;
            loaded = true;

            gallery_list = DCCommon.GetGalleryList();
            mgallery_list = DCCommon.GetMinorGalleryList();
            
            gallery_list.ToList().ForEach(x => gl.Add(x.Key + " 갤러리", x.Value));
            mgallery_list.ToList().ForEach(x => { gl.Add(x.Key + " 마이너 갤러리", x.Value); });

            gl.ToList().ForEach(x => gall_list.Items.Add(new ComboBoxItem() { Content = $"{x.Key} ({x.Value})" }));
        }

        private void gall_list_TextInput(object sender, TextCompositionEventArgs e)
        {
            gall_list.IsDropDownOpen = true;
            gl.ToList().Where(p => p.Key.Contains(e.Text)).ToList().ForEach(x => gall_list.Items.Add(new ComboBoxItem() { Content = $"{x.Key} ({x.Value})" }));
        }

        private void gall_list_TextChanged(object sender, TextChangedEventArgs e)
        {
            gall_list.IsDropDownOpen = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var gall_code = gall_list.Text.Split('(').Last().Split(')')[0];
            var content = nick.Text;
            var t = new Thread(() => tracker(gall_code, content.ToLower(), !gallery_list.ContainsValue(gall_code)));
            t.Start();
        }

        private void append(string c)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                result.AppendText(c + "\r\n");
                result.ScrollToEnd();
            }));
        }

        private void tracker(string gall_code, string contents, bool isminor)
        {
            append($"Thread starts! {gall_code} {contents}" + (isminor ? " mg" : " g"));

            for (int i = 1; i < 10; i++)
            {
                var url = "https://gall.dcinside.com/board/lists?id=" + gall_code + $"&page={i}";
                //append("download url " + url + "...");
                var html = NetCommon.DownloadString(url);
                var g = DCParser.ParseGallery(html);
                //append($"mp={g.max_page},esno={g.esno},krn={g.name},cnt={g.articles.Length} loaded!");

                foreach (var a in g.articles)
                {
                    if (a.nick.ToLower().Contains(contents) || a.title.ToLower().Contains(contents))
                        append($"article found! title={a.title},no={a.no},nick={a.nick}");

                    if (a.replay_num != "")
                    {
                        for (int j = 1; j < 100; j++)
                        {
                            //append("download comment " + a.title  + " " + a.no + " " + j);
                            var c = DCCommon.GetComments(g, a, j.ToString());
                            if (c.comments == null || c.comments.Length == 0)
                                break;
                            foreach (var b in c.comments)
                                if (b.name.ToLower().Contains(contents) || b.memo.ToLower().Contains(contents))
                                    append($"coment found! title={a.title},no={a.no},name={b.name},memo={b.memo}");
                        }
                    }
                }
            }
        }
    }
}
