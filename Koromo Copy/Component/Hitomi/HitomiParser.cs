/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Koromo_Copy.Component.Hitomi
{
    public class HitomiParser
    {
        /// <summary>
        /// 갤러리 블록을 파싱합니다.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        static public HitomiArticle ParseGalleryBlock(string source)
        {
            HitomiArticle article = new HitomiArticle();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(source);
            HtmlNode nodes = document.DocumentNode.SelectNodes("/div")[0];

            article.Magic = nodes.SelectSingleNode("./a").GetAttributeValue("href", "").Split('/')[2].Split('.')[0];
            try { article.Thumbnail = nodes.SelectSingleNode("./a//img").GetAttributeValue("data-src", "").Substring("//tn.hitomi.la/".Length).Replace("smallbig", "big"); } catch
                { article.Thumbnail = nodes.SelectSingleNode("./a//img").GetAttributeValue("src", "").Substring("//tn.hitomi.la/".Length); }
            article.Title = nodes.SelectSingleNode("./h1").InnerText;

            try { article.Artists = nodes.SelectNodes(".//div[@class='artist-list']//li").Select(node => node.SelectSingleNode("./a").InnerText).ToArray(); }
            catch { article.Artists = new[] { "N/A" }; }

            var contents = nodes.SelectSingleNode("./div[2]/table");
            try { article.Series = contents.SelectNodes("./tr[1]/td[2]/ul/li").Select(node => node.SelectSingleNode(".//a").InnerText).ToArray(); } catch { }
            article.Type = contents.SelectSingleNode("./tr[2]/td[2]/a").InnerText;
            try { article.Language = HitomiLegalize.LegalizeLanguage(contents.SelectSingleNode("./tr[3]/td[2]/a").InnerText); } catch { }
            try { article.Tags = contents.SelectNodes("./tr[4]/td[2]/ul/li").Select(node => HitomiLegalize.LegalizeTag(node.SelectSingleNode(".//a").InnerText)).ToArray(); } catch { }

            article.DateTime = nodes.SelectSingleNode("./div[2]/p").InnerText;

            return article;
        }

        /// <summary>
        /// 갤러리를 파싱합니다.
        /// 그룹/시리즈만 파싱함
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        static public HitomiArticle ParseGallery(string source)
        {
            HitomiArticle article = new HitomiArticle();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(source);
            HtmlNode nodes = document.DocumentNode.SelectSingleNode("//div[@class='content']");

            article.Magic = nodes.SelectSingleNode("./div[3]/h1/a").GetAttributeValue("href", "").Split('/')[2].Split('.')[0];
            //article.Title = nodes.SelectSingleNode("./div[3]/h1").InnerText.Trim();
            //article.Thumbnail = nodes.SelectSingleNode("./div[2]/div/a/img").GetAttributeValue("src", "");
            //article.Artists = nodes.SelectSingleNode(".")

            foreach (var tr in document.DocumentNode.SelectNodes("//div[@class='gallery-info']/table/tr").ToList())
            {
                var tt = tr.SelectSingleNode("./td").InnerText.ToLower().Trim();
                if (tt == "group")
                {
                    article.Groups = tr.SelectNodes(".//a")?.Select(x => x.InnerText.Trim()).ToArray();
                }
                else if (tt == "characters")
                {
                    article.Characters = tr.SelectNodes(".//a")?.Select(x => x.InnerText.Trim()).ToArray();
                }
            }

            return article;
        }

        static public void FillGallery(string source, HitomiArticle article)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(source);
            HtmlNode nodes = document.DocumentNode.SelectSingleNode("/div[@class='gallery-info']/table/tbody");

            foreach (var tr in nodes.SelectNodes("./tr").ToList())
            {
                var tt = tr.SelectSingleNode("./td").InnerText.ToLower().Trim();
                if (tt == "group")
                {
                    article.Groups = tr.SelectNodes(".//a").Select(x => x.InnerText.Trim()).ToArray();
                }
                else if (tt == "characters")
                {
                    article.Characters = tr.SelectNodes(".//a").Select(x => x.InnerText.Trim()).ToArray();
                }
            }
        }

        /// <summary>
        /// 이미지 링크를 파싱합니다.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        static public List<string> GetImageLink(string json)
        {
            JArray arr = JArray.Parse(json.Substring(json.IndexOf('[')));
            List<string> result = new List<string>();
            foreach (var obj in arr)
                result.Add(obj.Value<string>("name"));
            return result;
        }
    }
}
