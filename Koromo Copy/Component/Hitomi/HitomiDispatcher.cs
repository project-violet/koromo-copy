/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;

namespace Koromo_Copy.Component.Hitomi
{
    /// <summary>
    /// 히토미 다운로드에 필요한 모든 정보를 모으는 디스패쳐입니다.
    /// </summary>
    public class HitomiDispatcher : IDispatchable
    {
        public static async Task<IArticle> Collect(string uri)
        {
            string magic = Regex.Match(uri, "(\\d+)").Value;
            string html_source = await Task.Run(() => NetCommon.DownloadString($"{HitomiCommon.HitomiGalleryBlock}{magic}.html"));
            HitomiArticle article = HitomiParser.ParseGalleryBlock(html_source);
            return article;
        }
        
        Task<IArticle> IDispatchable.CollectAsync(string uri)
        {
            return Collect(uri);
        }
    }
}
