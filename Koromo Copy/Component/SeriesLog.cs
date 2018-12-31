/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hiyobi;
using Koromo_Copy.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Component
{
    public class SeriesLogModel
    {
        [JsonProperty]
        public string URL;

        [JsonProperty]
        public string Title;

        [JsonProperty]
        public string Thumbnail;

        [JsonProperty]
        public string[] Subtitle;

        [JsonProperty]
        public string[] Archive;

        [JsonProperty]
        public DateTime LatestUpdateTime;
    }

    /// <summary>
    /// 시리즈 정보를 관리합니다.
    /// </summary>
    public class SeriesLog : ILazy<SeriesLog>
    {
        string log_path = $"series.json";
        List<SeriesLogModel> model;

        public SeriesLog()
        {
            if (File.Exists(log_path)) model = JsonConvert.DeserializeObject<List<SeriesLogModel>>(File.ReadAllText(log_path));
            if (model == null)
            {
                model = new List<SeriesLogModel>();
            }
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(model, Formatting.Indented);
            using (var fs = new StreamWriter(new FileStream(log_path, FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }

        public void Add(SeriesLogModel model)
        {
            this.model.Add(model);
        }

        public List<SeriesLogModel> Model { get { return model; } }
    }
}
