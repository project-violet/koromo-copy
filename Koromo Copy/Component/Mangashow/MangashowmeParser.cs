/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using HtmlAgilityPack;
using Koromo_Copy.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.Mangashow
{
    public class MangashowmeParser
    {
        public static List<MangashowmeArticle> ParseSeries(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//div[@class='slot ' or @class='slot display-none']");

            List<MangashowmeArticle> articles = new List<MangashowmeArticle>();

            foreach (var node in nodes)
            {
                articles.Add(new MangashowmeArticle
                {
                    Title = node.SelectSingleNode(".//div[@class='title']").InnerText.Split('\n').Last(),
                    ArticleLink = node.GetAttributeValue("data-wrid", ""),
                });
            }

            return articles;
        }

        public static string ParseTitle(string html)
        {
            return Regex.Match(html, @"class=""red title"">(.*?)<").Groups[1].Value;
        }

        public static string ParseThumbnail(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            return document.DocumentNode.SelectSingleNode("//div[@class='manga-thumbnail']").GetAttributeValue("style", "").Split('(')[1].Split(')')[0];
        }

        public static List<string> ParseImages(string html)
        {
            //HtmlDocument document = new HtmlDocument();
            //document.LoadHtml(html);
            return JsonConvert.DeserializeObject<List<string>>(html.Split(new string[] { @"var img_list = " }, StringSplitOptions.None)[1].Split(';')[0]);
            //return document.DocumentNode.SelectNodes("//div[@class='view-content scroll-viewer']//img").Select(x => x.GetAttributeValue("src", "")).ToList();
        }

        public static List<Tuple<string,string>> ParseIndex(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            List<Tuple<string, string>> result = new List<Tuple<string, string>>();

            foreach (var article in document.DocumentNode.SelectNodes("//div[@class='post-row']"))
            {
                result.Add(Tuple.Create(article.SelectSingleNode(".//div[@class='img-wrap-back']").GetAttributeValue("style", "").Split('(')[1].Split(')')[0], article.SelectSingleNode(".//a").GetAttributeValue("href", "")));
            }

            return result;
        }

        public static int ParseMaxPage(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            return Convert.ToInt32(document.DocumentNode.SelectNodes("//ul[@class='pagination']/li").Last().SelectSingleNode("./a").GetAttributeValue("href", "").Split('(')[1].Split(')')[0]);
        }
    }
}
