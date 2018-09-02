/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        #region Metadata
        public int downloadCount = 0;
        public object lock_count = new object();

        public async Task DownloadMetadata()
        {
            ServicePointManager.DefaultConnectionLimit = 999999999;
            metadata_collection = new List<HitomiMetadata>();
            downloadCount = 0;
            await Task.WhenAll(Enumerable.Range(0, number_of_gallery_jsons).Select(no => downloadMetadata(no)));
            SortMetadata();

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            Monitor.Instance.Push("Write file: metadata.json");
            using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "metadata.json")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, metadata_collection);
            }
        }

        public void SortMetadata()
        {
            metadata_collection.Sort((a, b) => b.ID.CompareTo(a.ID));
        }

        private async Task downloadMetadata(int no)
        {
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 0, 0, Timeout.Infinite);
            var data = await client.GetStringAsync(gallerie_json_uri(no));
            if (data.Trim() == "")
            {
                Monitor.Instance.Push($"Error: '{gallerie_json_uri(no)}' is empty database!");
                return;
            }
            lock (metadata_collection)
                metadata_collection.AddRange(JsonConvert.DeserializeObject<IEnumerable<HitomiMetadata>>(data));
            lock (lock_count)
            {
                downloadCount++;
                Monitor.Instance.Push($"Download complete: [{downloadCount.ToString("00")}/{number_of_gallery_jsons}] {gallerie_json_uri(no)}");
            }
        }
        #endregion

    }
}
