/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

namespace Koromo_Copy.Component.Hiyobi
{
    public class HiyobiCommon
    {
        public string GetDownloadImageAddress(string x) => $"https://aa.hiyobi.me/data/json/{x}_list.js";
        public string GetInfoAddress(string x) => $"https://hiyobi.me/info/{x}";
    }
}
