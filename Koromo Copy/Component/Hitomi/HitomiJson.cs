/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;
using System;
using System.IO;

namespace Koromo_Copy.Component.Hitomi
{
    public class HitomiJsonModel
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
        public string Types;
        [JsonProperty]
        public int Pages;
        [JsonProperty]
        public string[] Tags;
    }

    public class HitomiJson
    {
        string path;
        HitomiJsonModel model;

        public HitomiJson(string path)
        {
            this.path = Path.Combine(path, "Info.json");
            if (File.Exists(this.path)) model = JsonConvert.DeserializeObject<HitomiJsonModel>(File.ReadAllText(this.path));
            if (model == null) model = new HitomiJsonModel();
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(model, Formatting.Indented);
                using (var fs = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write)))
                {
                    fs.Write(json);
                }
            }
            catch (Exception e)
            {
                Monitor.Instance.Push("[Hitomi Json] Skip save json. Fail " + e.Message);
            }
        }

        public void SetModelFromArticle(HitomiArticle article)
        {
            model.Id = article.Magic;
            model.Title = article.Title;
            model.Artists = article.Artists;
            model.Series = article.Series;
            model.Characters = article.Characters;
            model.Groups = article.Groups;
            model.Types = article.Type;
            model.Pages = article.ImagesLink.Count;
            model.Tags = article.Tags;
        }

        public ref HitomiJsonModel GetModel()
        {
            return ref model;
        }
    }
}
