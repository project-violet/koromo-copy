/***

   Copyright (C) 2018-2020. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

namespace Koromo_Copy.Component.Hiyobi
{
    public class HiyobiCommon
    {
        public static string GetDownloadImageAddress(string x) => $"https://hiyobi.me/data/json/{x}_list.js";
        public static string GetDownloadMangaImageAddress(string x) => $"https://hiyobi.me/data_m/json/{x}_list.js";
        public static string GetInfoAddress(string x) => $"https://hiyobi.me/info/{x}";
    }
}
