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

namespace Koromo_Copy.Component.DC
{
    public class DCParser
    {
        public static DCArticle ParseBoardView(string html)
        {
            DCArticle article = new DCArticle();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode node = document.DocumentNode.SelectNodes("//div[@class='view_content_wrap']")[0];

            article.Title = node.SelectSingleNode("//span[@class='title_subject']").InnerText;
            article.ImagesLink = node.SelectNodes("//ul[@class='appending_file']/li").Select(x => x.SelectSingleNode("./a").GetAttributeValue("href","")).ToList();
            article.FilesName = node.SelectNodes("//ul[@class='appending_file']/li").Select(x => x.SelectSingleNode("./a").InnerText).ToList();
            //Regex.Matches(node.SelectSingleNode("//div[@class='appending_file_box']").InnerHtml, @"<img.*?src=""(.*?)""").OfType<Match>().Select(x => x.Groups[1].Value).ToList();

            return article;
        }
    }
}
