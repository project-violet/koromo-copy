/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using HtmlAgilityPack;
using Koromo_Copy.Component.Hitomi;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Koromo_Copy.Component.Hiyobi
{
    public class HiyobiParser
    {
        /// <summary>
        /// Info 페이지를 분석합니다.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static HitomiArticle ParseGalleryConents(string html)
        {
            HitomiArticle article = new HitomiArticle();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode node = document.DocumentNode.SelectNodes("//main/div")[0];

            //article.Magic = node.SelectSingleNode("./a").GetAttributeValue("href", "").Split('/')[2];
            article.Thumbnail = "https://hiyobi.me" + node.SelectSingleNode(".//img").GetAttributeValue("src", "");

            var span = node.SelectSingleNode("./span");
            article.Title = span.SelectSingleNode("./h5/a/b").InnerText;

            var table = span.SelectNodes("./table/tr");
            var table_dic = new Dictionary<string, HtmlNode>();
            table.ToList().ForEach(x => table_dic.Add(x.SelectSingleNode("./td[1]").InnerText.Remove(2), x));

            if (table_dic.ContainsKey("작가")) try { article.Artists = table_dic["작가"].SelectNodes("./td[2]/a").ToList().Select(x => x.InnerText).ToArray(); } catch { }
            if (table_dic.ContainsKey("원작")) try { article.Series = table_dic["원작"].SelectNodes("./td[2]/a").ToList().Select(x => $"{x.GetAttributeValue("data-original", "")}|{x.InnerText}").ToArray(); } catch { }
            if (table_dic.ContainsKey("종류")) try { article.Type = table_dic["종류"].SelectSingleNode("./td[2]/a").InnerText; } catch { }
            if (table_dic.ContainsKey("태그")) try { article.Tags = table_dic["태그"].SelectNodes("./td[2]/a").ToList().Select(x => $"{x.GetAttributeValue("data-original", "")}|{x.InnerText}").ToArray(); } catch { }
            if (table_dic.ContainsKey("캐릭")) try { article.Characters = table_dic["캐릭"].SelectNodes("./td[2]/a").ToList().Select(x => $"{x.GetAttributeValue("data-original", "")}|{x.InnerText}").ToArray(); } catch { }

            return article;
        }

        /// <summary>
        /// 아티클 페이지를 분석합니다.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<HiyobiArticle> ParseGalleryArticles(string html)
        {
            List<HiyobiArticle> articles = new List<HiyobiArticle>();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//main/div");

            foreach (var node in nodes)
            {
                HiyobiArticle article = new HiyobiArticle();

                article.Magic = node.SelectSingleNode("./a").GetAttributeValue("href", "").Split('/')[2];
                article.Thumbnail = node.SelectSingleNode("./a/img").GetAttributeValue("src", "");

                var span = node.SelectSingleNode("./span");
                article.Title = span.SelectSingleNode("./h5/a/b").InnerText;

                var table = span.SelectNodes("./table/tr");
                var table_dic = new Dictionary<string, HtmlNode>();
                table.ToList().ForEach(x => table_dic.Add(x.SelectSingleNode("./td[1]").InnerText.Remove(2), x));

                if (table_dic.ContainsKey("작가")) try { article.Artists = table_dic["작가"].SelectNodes("./td[2]/a").ToList().Select(x => x.InnerText).ToArray(); } catch { }
                if (table_dic.ContainsKey("원작")) try { article.Series = table_dic["원작"].SelectNodes("./td[2]/a").ToList().Select(x => $"{x.GetAttributeValue("data-original", "")}|{x.InnerText}").ToArray(); } catch { }
                if (table_dic.ContainsKey("종류")) try { article.Type = table_dic["종류"].SelectSingleNode("./td[2]/a").InnerText; } catch { }
                if (table_dic.ContainsKey("태그")) try { article.Tags = table_dic["태그"].SelectNodes("./td[2]/a").ToList().Select(x => $"{x.GetAttributeValue("data-original", "")}|{x.InnerText}").ToArray(); } catch { }
                if (table_dic.ContainsKey("캐릭")) try { article.Characters = table_dic["캐릭"].SelectNodes("./td[2]/a").ToList().Select(x => $"{x.GetAttributeValue("data-original", "")}|{x.InnerText}").ToArray(); } catch { }
                
                articles.Add(article);
            }

            return articles;
        }

        public static List<HiyobiArticle> ParseNonHArticles(string html)
        {
            List<HiyobiArticle> articles = new List<HiyobiArticle>();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes(@"//main[@class=""container""]//a[@target=""_blank""]");

            foreach (var node in nodes)
            {
                articles.Add(new HiyobiArticle
                {
                    Title = node.InnerText,
                    Magic = node.GetAttributeValue("href", "").Split('/')[3]
                });
            }

            return articles;
        }

        public static string ParseNonHTitle(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            return document.DocumentNode.SelectSingleNode("//main[@class='container']/h3").InnerText;
        }
    }
}
