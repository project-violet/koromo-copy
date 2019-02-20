/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Koromo_Copy_UX.Utility
{
    public class ZipListingArticleModel
    {
        [JsonProperty]
        public HitomiJsonModel ArticleData;

        [JsonProperty]
        public string CreatedDate;

        [JsonProperty]
        public long Size;
    }

    /// <summary>
    /// Zip Listing Tool Data Model
    /// </summary>
    public class ZipListingModel
    {
        /// <summary>
        /// 최초로 열거작업을 수행한 폴더입니다. 루트 경로를 찾을 수 없다면 대화형으로 경로를 찾습니다.
        /// </summary>
        [JsonProperty]
        public string RootDirectory;

        /// <summary>
        /// 사용자가 지정한 태그입니다.
        /// </summary>
        [JsonProperty]
        public string Tag;

        /// <summary>
        /// 튜플의 첫 번째 요소는 해당 파일의 상대경로이며, 두 번째 요소는 작품의 정보입니다.
        /// </summary>
        [JsonProperty]
        public KeyValuePair<string, ZipListingArticleModel>[] ArticleList;
    }

    public class ZipListingRatingModel
    {
        public Dictionary<int, int> Rating;
    }

    public class ZipListingModelManager
    {
        public static void SaveModel(string filename, ZipListingModel model)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            Monitor.Instance.Push($"Write file: {filename}");
            using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, model);
            }
        }

        public static ZipListingModel OpenModel(string filename)
        {
            return JsonConvert.DeserializeObject<ZipListingModel>(File.ReadAllText(filename));
        }

        public static void SaveRatingModel(string filename, ZipListingRatingModel model)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            Monitor.Instance.Push($"Write file: {filename}");
            using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, model);
            }
        }

        public static ZipListingRatingModel OpenRatingModel(string filename)
        {
            return JsonConvert.DeserializeObject<ZipListingRatingModel>(File.ReadAllText(filename));
        }
    }
}
