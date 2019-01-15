/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;

namespace Koromo_Copy.Component.Hiyobi
{
    public class HiyobiDispatcher : IDispatchable
    {
        public static async Task<IArticle> Collect(string uri)
        {
            string magic = Regex.Match(uri, "(\\d+)").Value;
            string html_source = await Task.Run(() => NetCommon.DownloadString(HiyobiCommon.GetInfoAddress(magic)));
            HitomiArticle article = HiyobiParser.ParseGalleryConents(html_source);
            return article;
        }

        Task<IArticle> IDispatchable.CollectAsync(string uri)
        {
            return Collect(uri);
        }
    }
}
