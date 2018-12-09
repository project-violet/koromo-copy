/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
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
            return Regex.Match(html, @"class=""red"">(.*?)<").Groups[1].Value;
        }

        public static List<string> ParseImages(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            return document.DocumentNode.SelectNodes("//div[@class='view-content scroll-viewer']//img").Select(x => x.GetAttributeValue("src", "")).ToList();
        }
    }
}
