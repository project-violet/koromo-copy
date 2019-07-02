/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using HtmlAgilityPack;
using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.DC
{
    public class DCParser
    {
        public static DCArticle ParseBoardView(string html, bool is_minor = false)
        {
            DCArticle article = new DCArticle();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode node = document.DocumentNode.SelectNodes("//div[@class='view_content_wrap']")[0];

            article.Id = Regex.Match(html, @"name=""gallery_no"" value=""(\d+)""").Groups[1].Value;
            article.GalleryName = Regex.Match(html, @"<h4 class=""block_gallname"">\[(.*?) ").Groups[1].Value;
            article.OriginalGalleryName = document.DocumentNode.SelectSingleNode("//input[@id='gallery_id']").GetAttributeValue("value", "");
            if (is_minor)
                article.Class = node.SelectSingleNode("//span[@class='title_headtext']").InnerText;
            article.Contents = node.SelectSingleNode("//div[@class='writing_view_box']").InnerHtml.ToBase64();
            article.Title = node.SelectSingleNode("//span[@class='title_subject']").InnerText;
            try
            {
                article.ImagesLink = node.SelectNodes("//ul[@class='appending_file']/li").Select(x => x.SelectSingleNode("./a").GetAttributeValue("href", "")).ToList();
                article.FilesName = node.SelectNodes("//ul[@class='appending_file']/li").Select(x => x.SelectSingleNode("./a").InnerText).ToList();
            } catch { }
            article.ESNO = document.DocumentNode.SelectSingleNode("//input[@id='e_s_n_o']").GetAttributeValue("value", "");

            return article;
        }

        public static DCGallery ParseGallery(string html)
        {
            DCGallery gall = new DCGallery();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode node = document.DocumentNode.SelectNodes("//tbody")[0];

            gall.id = document.DocumentNode.SelectSingleNode("//input[@id='gallery_id']").GetAttributeValue("value", "");
            gall.name = document.DocumentNode.SelectSingleNode("//meta[@property='og:title']").GetAttributeValue("content", "");
            gall.esno = document.DocumentNode.SelectSingleNode("//input[@id='e_s_n_o']").GetAttributeValue("value", "");
            gall.cur_page = document.DocumentNode.SelectSingleNode("//div[@class='bottom_paging_box']/em").InnerText;
            try { gall.max_page = document.DocumentNode.SelectSingleNode("//a[@class='page_end']").GetAttributeValue("href", "").Split('=').Last(); } catch { }

            List<DCPageArticle> pas = new List<DCPageArticle>();

            foreach (var tr in node.SelectNodes("./tr"))
            {
                var gall_num = tr.SelectSingleNode("./td[1]").InnerText;
                int v;
                if (!int.TryParse(gall_num, out v)) continue;

                var pa = new DCPageArticle();
                pa.no = gall_num;
                pa.type = tr.SelectSingleNode("./td[2]/a/em").GetAttributeValue("class", "").Split(' ')[1];
                pa.title = tr.SelectSingleNode("./td[2]/a").InnerText;
                try { pa.replay_num = tr.SelectSingleNode(".//span[@class='reply_num']").InnerText; } catch { }
                pa.nick = tr.SelectSingleNode("./td[3]").GetAttributeValue("data-nick", "");
                pa.uid = tr.SelectSingleNode("./td[3]").GetAttributeValue("data-uid", "");
                pa.ip = tr.SelectSingleNode("./td[3]").GetAttributeValue("data-ip", "");
                if (pa.ip == "")
                {
                    pa.islogined = true;
                    if (tr.SelectSingleNode("./td[3]/a/img").GetAttributeValue("src", "").Contains("fix_nik.gif"))
                        pa.isfixed = true;
                }
                pa.date = DateTime.Parse(tr.SelectSingleNode("./td[4]").GetAttributeValue("title", ""));
                pa.count = tr.SelectSingleNode("./td[5]").InnerText;
                pa.recommend = tr.SelectSingleNode("./td[6]").InnerText;

                pas.Add(pa);
            }

            gall.articles = pas.ToArray();

            return gall;
        }

        public static DCGallery ParseMinorGallery(string html)
        {
            DCGallery gall = new DCGallery();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode node = document.DocumentNode.SelectNodes("//tbody")[0];

            gall.id = document.DocumentNode.SelectSingleNode("//input[@id='gallery_id']").GetAttributeValue("value", "");
            gall.name = document.DocumentNode.SelectSingleNode("//meta[@property='og:title']").GetAttributeValue("content", "");
            gall.esno = document.DocumentNode.SelectSingleNode("//input[@id='e_s_n_o']").GetAttributeValue("value", "");
            gall.cur_page = document.DocumentNode.SelectSingleNode("//div[@class='bottom_paging_box']/em").InnerText;
            try { gall.max_page = document.DocumentNode.SelectSingleNode("//a[@class='page_end']").GetAttributeValue("href", "").Split('=').Last(); } catch { }

            List<DCPageArticle> pas = new List<DCPageArticle>();

            foreach (var tr in node.SelectNodes("./tr"))
            {
                try
                {
                    var gall_num = tr.SelectSingleNode("./td[1]").InnerText;
                    int v;
                    if (!int.TryParse(gall_num, out v)) continue;

                    var pa = new DCPageArticle();
                    pa.no = gall_num;
                    pa.classify = tr.SelectSingleNode("./td[2]").InnerText;
                    pa.type = tr.SelectSingleNode("./td[3]/a/em").GetAttributeValue("class", "").Split(' ')[1];
                    pa.title = tr.SelectSingleNode("./td[3]/a").InnerText;
                    try { pa.replay_num = tr.SelectSingleNode(".//span[@class='reply_num']").InnerText; } catch { }
                    pa.nick = tr.SelectSingleNode("./td[4]").GetAttributeValue("data-nick", "");
                    pa.uid = tr.SelectSingleNode("./td[4]").GetAttributeValue("data-uid", "");
                    pa.ip = tr.SelectSingleNode("./td[4]").GetAttributeValue("data-ip", "");
                    if (pa.ip == "")
                    {
                        pa.islogined = true;
                        if (tr.SelectSingleNode("./td[4]/a/img").GetAttributeValue("src", "").Contains("fix_nik.gif"))
                            pa.isfixed = true;
                    }
                    pa.date = DateTime.Parse(tr.SelectSingleNode("./td[5]").GetAttributeValue("title", ""));
                    pa.count = tr.SelectSingleNode("./td[6]").InnerText;
                    pa.recommend = tr.SelectSingleNode("./td[7]").InnerText;

                    pas.Add(pa);
                }
                catch { }
            }

            gall.articles = pas.ToArray();

            return gall;
        }
    }
}
