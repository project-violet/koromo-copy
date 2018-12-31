/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Koromo_Copy.Component.Hitomi
{
    public class HitomiLogModel
    {
        [JsonProperty]
        public string Id;
        [JsonProperty]
        public string Title;
        [JsonProperty]
        public string[] Artists;
        [JsonProperty]
        public string[] Groups;
        [JsonProperty]
        public string[] Series;
        [JsonProperty]
        public string[] Characters;
        [JsonProperty]
        public string[] Tags;
        [JsonProperty]
        public DateTime Time;
    }

    public class HitomiLog : ILazy<HitomiLog>
    {
        string log_path = $"{Environment.CurrentDirectory}\\log.json";

        List<HitomiLogModel> model;
        HashSet<int> downloaded = new HashSet<int>();

        public HashSet<int> DownloadTable { get { return downloaded; } }

        public HitomiLog()
        {
            if (File.Exists(log_path)) model = JsonConvert.DeserializeObject<List<HitomiLogModel>>(File.ReadAllText(log_path));
            if (model == null) model = new List<HitomiLogModel>();
            foreach (var mm in model) downloaded.Add(Convert.ToInt32(mm.Id));
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(model, Formatting.Indented);
            using (var fs = new StreamWriter(new FileStream(log_path, FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }

        public void AddArticle(HitomiArticle article)
        {
            HitomiLogModel mm = new HitomiLogModel();
            mm.Id = article.Magic;
            mm.Title = article.Title;
            mm.Artists = article.Artists;
            mm.Groups = article.Groups;
            mm.Series = article.Series;
            mm.Characters = article.Characters;
            mm.Tags = article.Tags;
            mm.Time = DateTime.Now;
            model.Add(mm);
            downloaded.Add(Convert.ToInt32(article.Magic));
        }

        public bool Contains(int id)
        {
            return downloaded.Contains(id);
        }

        public bool Contains(string id)
        {
            return Contains(Convert.ToInt32(id));
        }

        public IEnumerable<HitomiLogModel> GetEnumerator()
        {
            return model;
        }

        public List<HitomiLogModel> GetList()
        {
            return model;
        }

        public DateTime GetLatestDownload(string id)
        {
            for (int i = model.Count - 1; i >= 0; i--)
                if (model[i].Id == id)
                    return model[i].Time;
            return DateTime.MinValue;
        }
    }
}
