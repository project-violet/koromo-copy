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
        public static DCArticle ParseBoardView(string html)
        {
            DCArticle article = new DCArticle();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode node = document.DocumentNode.SelectNodes("//div[@class='view_content_wrap']")[0];

            article.Id = Regex.Match(html, @"name=""gallery_no"" value=""(\d+)""").Groups[1].Value;
            article.GalleryName = Regex.Match(html, @"<h4 class=""block_gallname"">\[(.*?) ").Groups[1].Value;
            article.Title = node.SelectSingleNode("//span[@class='title_subject']").InnerText;
            article.ImagesLink = node.SelectNodes("//ul[@class='appending_file']/li").Select(x => x.SelectSingleNode("./a").GetAttributeValue("href","")).ToList();
            article.FilesName = node.SelectNodes("//ul[@class='appending_file']/li").Select(x => x.SelectSingleNode("./a").InnerText).ToList();
            
            return article;
        }
    }
}
