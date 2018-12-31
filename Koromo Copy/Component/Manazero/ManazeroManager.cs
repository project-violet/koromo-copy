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

namespace Koromo_Copy.Component.Manazero
{
    public class ManazeroSeries : ISeries
    {
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public string[] Archive { get; set; }
        public List<IArticle> Articles { get; set; }
    }

    public class ManazeroManager : ILazy<ManazeroManager>, IManager
    {
        public ManagerType Type => ManagerType.SingleSeriesMultipleArticles;
        public ManagerEngineType EngineType => ManagerEngineType.UsingDriver;
        public string Name => "Manazero";
        public bool SpecifyUrl(string url) => url.Contains("manazero009i.blogspot.com");
        public WebClient GetWebClient() => null;

        public ISeries ParseSeries(string html)
        {
            var result = new ManazeroSeries();
            result.Title = ManazeroParser.ParseTitle(html);
            result.Thumbnail = ManazeroParser.ParseThumbnail(html);
            result.Articles = ManazeroParser.ParseArticles(html).OfType<IArticle>().ToList();
            result.Archive = result.Articles.Select(x => ((x as ManazeroArticle).ArticleLink)).ToArray();
            return result;
        }

        public IArticle ParseArticle(string html)
        {
            return null;
        }

        public List<string> ParseImages(string html, IArticle article)
        {
            return ManazeroParser.ParseImages(html);
        }

        public List<string> GetDownloadFileNames(IArticle article)
        {
            return article.ImagesLink.Select(x => HttpUtility.UrlDecode(HttpUtility.UrlDecode(x.Split('/').Last()))).ToList();
        }
    }
}
