/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System.Collections.Generic;

namespace Koromo_Copy.Hitomi
{
    /// <summary>
    /// 히토미 데이터베이스를 총괄하는 클래스입니다.
    /// </summary>
    public class HitomiData : ILazy<HitomiData>
    {
        public const int max_number_of_result = 10;
        public const int number_of_gallery_jsons = 20;
        
        public static string tag_json_uri = @"https://ltn.hitomi.la/tags.json";
        public static string gallerie_json_uri(int no) => $"https://ltn.hitomi.la/galleries{no}.json";

        public static string hidden_data_url = @"https://github.com/dc-koromo/hitomi-downloader-2/releases/download/hiddendata/hiddendata.json";

        public HitomiTagdataCollection tagdata_collection = new HitomiTagdataCollection();
        public List<HitomiMetadata> metadata_collection;
        public Dictionary<string, string> thumbnail_collection;

    }
}
