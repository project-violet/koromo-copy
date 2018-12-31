/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Net;
using System.Text.RegularExpressions;

namespace Koromo_Copy.Component.EH
{
    public class ExHentaiTool
    {
        /// <summary>
        /// 제목으로 특정 아티클을 검색합니다.
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string GetAddressFromMagicTitle(string magic, string title)
        {
            string html = NetCommon.DownloadExHentaiString($"https://exhentai.org/?f_search={title}&page=0");
            if (html.Contains($"/{magic}/"))
                return Regex.Match(html, $"(https://exhentai.org/g/{magic}/\\w+/)").Value;
            return "";
        }
    }
}
