/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Koromo_Copy.Component.Hiyobi
{
    public class HiyobiNonHSeries : ISeries
    {
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public string[] Archive { get; set; }
        public List<IArticle> Articles { get; set; }
    }

    public class HiyobiNonHManager : ILazy<HiyobiNonHManager>, IManager
    {
        public ManagerType Type => ManagerType.SingleSeriesMultipleArticles;
        public ManagerEngineType EngineType => ManagerEngineType.None;
        public string Name => "Hiyobi-Manga";
        public bool SpecifyUrl(string url) => url.Contains("hiyobi.me/manga/info");
        public WebClient GetWebClient() => null;

        public ISeries ParseSeries(string html)
        {
            var result = new HiyobiNonHSeries();
            result.Title = HiyobiParser.ParseNonHTitle(html);
            //result.Thumbnail = MangashowmeParser.ParseThumbnail(html);
            result.Articles = HiyobiParser.ParseNonHArticles(html).OfType<IArticle>().ToList();
            result.Archive = result.Articles.Select(x => HiyobiCommon.GetDownloadMangaImageAddress((x as HiyobiArticle).Magic)).ToArray();
            return result;
        }

        public IArticle ParseArticle(string html)
        {
            return null;
        }

        public List<string> ParseImages(string html, IArticle article)
        {
            var article_to = article as HiyobiArticle;
            return HitomiParser.GetImageLink(html).Select(x => x.StartsWith("http://") || x.StartsWith("https://") ? $"https://aa.hiyobi.me/data_m/{article_to.Magic}/{Uri.EscapeDataString(x.Split('/').Last())}" : $"https://aa.hiyobi.me/data_m/{article_to.Magic}/{x}").ToList();
        }

        public List<string> GetDownloadFileNames(IArticle article)
        {
            return article.ImagesLink.Select(x => !x.StartsWith("http://images-blogger-opensocial.googleusercontent.com/") ?
                            HttpUtility.UrlDecode(HttpUtility.UrlDecode(x.Split('/').Last())) :
                            HttpUtility.UrlDecode(HttpUtility.UrlDecode(HttpUtility.ParseQueryString(new Uri(x).Query).Get("url").Split('/').Last()))).ToList();
        }
    }
}
