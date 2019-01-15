/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Koromo_Copy.Component.EH
{
    public class EHentaiParser
    {
        /// <summary>
        /// 이헨 게시글 소스를 분석합니다.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static EHentaiArticle ParseArticleData(string source)
        {
            EHentaiArticle article = new EHentaiArticle();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(source);
            HtmlNode nodes = document.DocumentNode.SelectNodes("//div[@class='gm']")[0];

            article.Thumbnail = Regex.Match(nodes.SelectSingleNode(".//div[@id='gleft']//div//div").GetAttributeValue("style", ""), @"https://ehgt.org/.*?(?=\))").Groups[0].Value;

            article.Title = nodes.SelectSingleNode(".//div[@id='gd2']//h1[@id='gn']").InnerText;
            article.SubTitle = nodes.SelectSingleNode(".//div[@id='gd2']//h1[@id='gj']").InnerText;

            article.Type = nodes.SelectSingleNode(".//div[@id='gmid']//div//div[@id='gdc']//a//img").GetAttributeValue("alt", "");
            article.Uploader = nodes.SelectSingleNode(".//div[@id='gmid']//div//div[@id='gdn']//a").InnerText;

            HtmlNodeCollection nodes_static = nodes.SelectNodes(".//div[@id='gmid']//div//div[@id='gdd']//table//tr");

            article.Posted = nodes_static[0].SelectSingleNode(".//td[@class='gdt2']").InnerText;
            article.Parent = nodes_static[1].SelectSingleNode(".//td[@class='gdt2']").InnerText;
            article.Visible = nodes_static[2].SelectSingleNode(".//td[@class='gdt2']").InnerText;
            article.Language = nodes_static[3].SelectSingleNode(".//td[@class='gdt2']").InnerText.Split(' ')[0].ToLower();
            article.FileSize = nodes_static[4].SelectSingleNode(".//td[@class='gdt2']").InnerText;
            int.TryParse(nodes_static[5].SelectSingleNode(".//td[@class='gdt2']").InnerText.Split(' ')[0], out article.Length);
            int.TryParse(nodes_static[6].SelectSingleNode(".//td[@class='gdt2']").InnerText.Split(' ')[0], out article.Favorited);

            HtmlNodeCollection nodes_data = nodes.SelectNodes(".//div[@id='gmid']//div[@id='gd4']//table//tr");

            Dictionary<string, string[]> information = new Dictionary<string, string[]>();

            foreach (var i in nodes_data)
            {
                try
                {
                    information.Add(i.SelectNodes(".//td")[0].InnerText.Trim(),
                        i.SelectNodes(".//td")[1].SelectNodes(".//div").Select(e => e.SelectSingleNode(".//a").InnerText).ToArray());
                }
                catch { }
            }

            if (information.ContainsKey("language:")) article.language = information["language:"];
            if (information.ContainsKey("group:")) article.group = information["group:"];
            if (information.ContainsKey("parody:")) article.parody = information["parody:"];
            if (information.ContainsKey("character:")) article.character = information["character:"];
            if (information.ContainsKey("artist:")) article.artist = information["artist:"];
            if (information.ContainsKey("male:")) article.male = information["male:"];
            if (information.ContainsKey("female:")) article.female = information["female:"];
            if (information.ContainsKey("misc:")) article.misc = information["misc:"];

            HtmlNode nodesc = document.DocumentNode.SelectNodes("//div[@id='cdiv']")[0];
            HtmlNodeCollection nodes_datac = nodesc.SelectNodes(".//div[@class='c1']");
            List<Tuple<DateTime, string, string>> comments = new List<Tuple<DateTime, string, string>>();

            foreach (var i in nodes_datac ?? Enumerable.Empty<HtmlNode>())
            {
                try
                {
                    string date = HttpUtility.HtmlDecode(i.SelectNodes(".//div[@class='c2']//div[@class='c3']")[0].InnerText.Trim());
                    string author = HttpUtility.HtmlDecode(i.SelectNodes(".//div[@class='c2']//div[@class='c3']//a")[0].InnerText.Trim());
                    string contents = Regex.Replace(HttpUtility.HtmlDecode(i.SelectNodes(".//div[@class='c6']")[0].InnerHtml.Trim()), @"<br>", "\r\n");
                    comments.Add(new Tuple<DateTime, string, string>(
                        DateTime.Parse(date.Remove(date.IndexOf(" UTC")).Substring("Posted on ".Length) + "Z"),
                        author,
                        contents));
                }
                catch { }
            }

            comments.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            article.comment = comments.ToArray();

            return article;
        }
    }
}
