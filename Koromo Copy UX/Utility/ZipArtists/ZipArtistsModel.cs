/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Koromo_Copy_UX.Utility.ZipArtists
{
    public class ZipArtistsArtistModel
    {
        [JsonProperty]
        public Dictionary<string, HitomiJsonModel> ArticleData;

        [JsonProperty]
        public string CreatedDate;

        [JsonProperty]
        public string LastAccessDate;

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
        
        /// <summary>
        /// 튜플의 첫 번째 요소는 작가의 이름, 두 번째 요소는 카테고리, 세 번째 요소는 대표색 입니다.
        /// 하나의 작가라도 여러개의 카테고리를 가질 수 있습니다.
        /// </summary>
        public List<Tuple<string,string,Color>> BookmarkCategory;
    }

    public class ZipArtistsSettingModel
    {
        /// <summary>
        /// 한 페이지에 보여줄 최대 요소 개수입니다.
        /// </summary>
        public int PerElements;

        /// <summary>
        /// 페이지가 이동될 때마다 스크롤을 맨 위로 올립니다.
        /// 이 설정이 꺼져있는 경우 스크롤의 위치는 전 페이지의 위치와 동일하게 설정됩니다.
        /// </summary>
        public bool InitScroll;

        /// <summary>
        /// Online에서 이미지를 불러옵니다.
        /// </summary>
        public bool LoadFromOnline;

        /// <summary>
        /// 제목이 비슷한 작품들을 표시하지 않습니다.
        /// </summary>
        public bool UsingTextMatchingAccuracy;

        /// <summary>
        /// 제목이 비슷한 작품들을 표시하지않도록 설정하는 고유값입니다.
        /// </summary>
        public int TextMatchingAccuracy;
    }

    public class ZipArtistsModelManager : ILazy<ZipArtistsModelManager>
    {
        ZipArtistsSettingModel setting;
        const string setting_path = "za-setting.json";

        public ZipArtistsModelManager()
        {
            if (File.Exists(setting_path)) setting = JsonConvert.DeserializeObject<ZipArtistsSettingModel>(File.ReadAllText(setting_path));
            if (setting == null)
            {
                setting = new ZipArtistsSettingModel
                {
                    PerElements = 5,
                    InitScroll = true,
                    LoadFromOnline = false,
                    UsingTextMatchingAccuracy = false,
                    TextMatchingAccuracy = 5
                };
            }
            SaveSetting();
        }

        public void SaveSetting()
        {
            string json = JsonConvert.SerializeObject(setting, Formatting.Indented);
            using (var fs = new StreamWriter(new FileStream(setting_path, FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }

        public ZipArtistsSettingModel Setting { get { return setting; } }

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
