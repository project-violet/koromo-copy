/***

   Copyright (C) 2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.DC
{
    public class DCCommon
    {
        public static DCComment GetComments(DCArticle article, string page)
        {
            var wc = Net.NetCommon.GetDefaultClient();
            wc.Headers.Add("X-Requested-With", "XMLHttpRequest");
            wc.QueryString.Add("id", article.OriginalGalleryName);
            wc.QueryString.Add("no", article.Id);
            wc.QueryString.Add("cmt_id", article.OriginalGalleryName);
            wc.QueryString.Add("cmt_no", article.Id);
            wc.QueryString.Add("e_s_n_o", article.ESNO);
            wc.QueryString.Add("comment_page", page);
            return JsonConvert.DeserializeObject<DCComment>(Encoding.UTF8.GetString(wc.UploadValues("https://gall.dcinside.com/board/comment/", "POST", wc.QueryString)));
        }

        public static DCComment GetComments(DCGallery g, DCPageArticle article, string page)
        {
            var wc = Net.NetCommon.GetDefaultClient();
            wc.Headers.Add("X-Requested-With", "XMLHttpRequest");
            wc.QueryString.Add("id", g.id);
            wc.QueryString.Add("no", article.no);
            wc.QueryString.Add("cmt_id", g.id);
            wc.QueryString.Add("cmt_no", article.no);
            wc.QueryString.Add("e_s_n_o", g.esno);
            wc.QueryString.Add("comment_page", page);
            return JsonConvert.DeserializeObject<DCComment>(Encoding.UTF8.GetString(wc.UploadValues("https://gall.dcinside.com/board/comment/", "POST", wc.QueryString)));
        }

        public static SortedDictionary<string, string> GetGalleryList()
        {
            var dic = new SortedDictionary<string, string>();
            var src = Net.NetCommon.DownloadString("http://wstatic.dcinside.com/gallery/gallindex_iframe_new_gallery.html");

            var parse = new List<Match>();
            parse.AddRange(Regex.Matches(src, @"onmouseover=""gallery_view\('(\w+)'\);""\>[\s\S]*?\<.*?\>([\w\s]+)\<").Cast<Match>().ToList());
            parse.AddRange(Regex.Matches(src, @"onmouseover\=""gallery_view\('(\w+)'\);""\>\s*([\w\s]+)\<").Cast<Match>().ToList());
            foreach (var match in parse)
            {
                var identification = match.Groups[1].Value;
                var name = match.Groups[2].Value.Trim();

                if (!string.IsNullOrEmpty(name))
                {
                    if (name[0] == '-')
                        name = name.Remove(0, 1).Trim();
                    if (!dic.ContainsKey(name))
                        dic.Add(name, identification);
                }
            }

            return dic;
        }

        public static SortedDictionary<string, string> GetMinorGalleryList()
        {
            return JsonConvert.DeserializeObject<SortedDictionary<string, string>>(Net.NetCommon.DownloadString("https://raw.githubusercontent.com/dc-koromo/koromo-copy/master/dcinside-minor-gallery.json"));
        }

        public static SortedDictionary<string, string> GetMinorGalleryListRaw()
        {
            var dic = new SortedDictionary<string, string>();
            var html = Net.NetCommon.DownloadString("https://gall.dcinside.com/m");
            
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            foreach (var a in document.DocumentNode.SelectNodes("//a[@onmouseout='thumb_hide();']"))
                dic.Add(a.InnerText.Trim(), a.GetAttributeValue("href", "").Split('=').Last());

            var under_name = new List<string>();
            foreach (var b in document.DocumentNode.SelectNodes("//button[@class='btn_cate_more']"))
                under_name.Add(b.GetAttributeValue("data-lyr", ""));

            int count = 1;
            foreach (var un in under_name)
            {
            RETRY:
                var wc = Net.NetCommon.GetDefaultClient();
                wc.Headers.Add("X-Requested-With", "XMLHttpRequest");
                wc.QueryString.Add("under_name", un);
                var subhtml = Encoding.UTF8.GetString(wc.UploadValues("https://gall.dcinside.com/ajax/minor_ajax/get_under_gall", "POST", wc.QueryString));
                if (subhtml.Trim() == "")
                {
                    Console.Console.Instance.WriteLine($"[{count}/{under_name.Count}] Retry {un}...");
                    goto RETRY;
                }

                HtmlDocument document2 = new HtmlDocument();
                document2.LoadHtml(subhtml);
                foreach (var c in document2.DocumentNode.SelectNodes("//a[@class='list_title']"))
                    if (!dic.ContainsKey(c.InnerText.Trim()))
                        dic.Add(c.InnerText.Trim(), c.GetAttributeValue("href", "").Split('=').Last());
                Console.Console.Instance.WriteLine($"[{count++}/{under_name.Count}] Complete {un}");
            }

            return dic;
        }
    }
}
