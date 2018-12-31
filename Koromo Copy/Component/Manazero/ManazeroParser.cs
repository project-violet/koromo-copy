/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.Manazero
{
    public class ManazeroParser
    {
        public static List<ManazeroArticle> ParseArticles(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode node = document.DocumentNode.SelectNodes("//ul[@class='rpw']").Last();

            List<ManazeroArticle> result = new List<ManazeroArticle>(); 

            foreach (var li in node.SelectNodes("./li"))
            {
                result.Add(new ManazeroArticle
                {
                    Title = li.InnerText,
                    ArticleLink = li.SelectSingleNode("./a").GetAttributeValue("href","")
                });
            }

            return result;
        }

        public static string ParseTitle(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            return document.DocumentNode.SelectSingleNode("//h3[@class='post-title entry-title']").InnerText;
        }

        public static string ParseThumbnail(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            return document.DocumentNode.SelectSingleNode("//div[@class='post-body entry-content']/img").GetAttributeValue("img","");
        }

        public static List<string> ParseImages(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode node = document.DocumentNode.SelectNodes("//div[@class='adsbygoogle']")[1];

            try
            {
                return node.SelectNodes("./div[2]/div/a/img").Select(x => x.GetAttributeValue("src", "")).ToList();
            } catch { }
            return node.SelectNodes(".//img").Select(x => x.GetAttributeValue("src", "")).ToList();
        }
    }
}
