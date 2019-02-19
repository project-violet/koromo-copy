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

namespace Koromo_Copy_UX3.Utility
{
    public class ZipArtistsArtistModel
    {
        [JsonProperty]
        public Dictionary<string, HitomiJsonModel> ArticleData;

        [JsonProperty]
        public string CreatedDate;

        [JsonProperty]
        public long Size;

        [JsonProperty]
        public string ArtistName;
    }

    public class ZipArtistsModel
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
        /// 튜플의 첫 번째 요소는 해당 폴더의 상대경로이며, 두 번째 요소는 작가의 정보입니다.
        /// </summary>
        [JsonProperty]
        public KeyValuePair<string, ZipArtistsArtistModel>[] ArtistList;
    }
    
    public class ZipArtistsRatingModel
    {
        public Dictionary<string, int> Rating;

        public List<string> Bookmark;
    }

    public class ZipArtistsModelManager
    {
        public static void SaveModel(string filename, ZipArtistsModel model)
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

        public static ZipArtistsModel OpenModel(string filename)
        {
            return JsonConvert.DeserializeObject<ZipArtistsModel>(File.ReadAllText(filename));
        }

        public static void SaveRatingModel(string filename, ZipArtistsRatingModel model)
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

        public static ZipArtistsRatingModel OpenRatingModel(string filename)
        {
            return JsonConvert.DeserializeObject<ZipArtistsRatingModel>(File.ReadAllText(filename));
        }
    }
}
