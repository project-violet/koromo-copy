/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Koromo_Copy.Component.Mangashow
{
    public class MangashowmeSeries : ISeries
    {
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public string[] Archive { get; set; }
        public List<IArticle> Articles { get; set; }
    }

    public class MangashowmeManager : ILazy<MangashowmeManager>, IManager
    {
        public ManagerType Type => ManagerType.SingleSeriesMultipleArticles;
        public ManagerEngineType EngineType => ManagerEngineType.None;
        public string Name => "Mangashowme";
        public bool SpecifyUrl(string url) => url.Contains("mangashow.me");
        public WebClient GetWebClient() => null;

        public ISeries ParseSeries(string html)
        {
            var result = new MangashowmeSeries();
            result.Title = MangashowmeParser.ParseTitle(html);
            result.Thumbnail = MangashowmeParser.ParseThumbnail(html);
            result.Articles = MangashowmeParser.ParseSeries(html).OfType<IArticle>().ToList();
            result.Archive = result.Articles.Select(x => MangashowmeCommon.GetDownloadMangaImageAddress((x as MangashowmeArticle).ArticleLink)).ToArray();
            return result;
        }

        public IArticle ParseArticle(string html)
        {
            return null;
        }

        public List<string> ParseImages(string html, IArticle article)
        {
            return MangashowmeParser.ParseImages(html);
        }

        public List<string> GetDownloadFileNames(IArticle article)
        {
            return article.ImagesLink.Select(x => HttpUtility.UrlDecode(HttpUtility.UrlDecode(x.Split('/').Last()))).ToList();
        }
    }
}
