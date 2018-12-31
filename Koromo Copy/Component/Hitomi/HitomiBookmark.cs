/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.Hitomi
{
    public class HitomiBookmarkModel
    {
        [JsonProperty]
        public List<Tuple<string, DateTime, string>> Artists;
        [JsonProperty]
        public List<Tuple<string, DateTime, string>> Groups;
        [JsonProperty]
        public List<Tuple<string, DateTime, string>> Articles;
        [JsonProperty]
        public List<Tuple<string, DateTime, string>> Tags;
        [JsonProperty]
        public List<Tuple<string, DateTime, string>> Characters;
        [JsonProperty]
        public List<Tuple<string, DateTime, string>> Series;
        [JsonProperty]
        public List<Tuple<string, List<Tuple<string, int>>, DateTime>> CustomTags;
    }

    public class HitomiBookmark : ILazy<HitomiBookmark>
    {
        string bk_path = $"{Environment.CurrentDirectory}\\bookmark.json";
        string bkbu_path = $"{Environment.CurrentDirectory}\\bookmark_backup.json";

        HitomiBookmarkModel model;

        public HitomiBookmark()
        {
            if (File.Exists(bk_path))
            {
                File.Copy(bk_path, bkbu_path, true);
                model = JsonConvert.DeserializeObject<HitomiBookmarkModel>(File.ReadAllText(bk_path));
            }
            if (model == null) model = new HitomiBookmarkModel();
            if (model.Artists == null) model.Artists = new List<Tuple<string, DateTime, string>>();
            if (model.Groups == null) model.Groups = new List<Tuple<string, DateTime, string>>();
            if (model.Articles == null) model.Articles = new List<Tuple<string, DateTime, string>>();
            if (model.Tags == null) model.Tags = new List<Tuple<string, DateTime, string>>();
            if (model.Characters == null) model.Characters = new List<Tuple<string, DateTime, string>>();
            if (model.Series == null) model.Series = new List<Tuple<string, DateTime, string>>();
            if (model.CustomTags == null) model.CustomTags = new List<Tuple<string, List<Tuple<string, int>>, DateTime>>();
            Save();
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(model, Formatting.Indented);
            using (var fs = new StreamWriter(new FileStream(bk_path, FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }

        public ref HitomiBookmarkModel GetModel()
        {
            return ref model;
        }
    }
}
