/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy_UX.Utility.Bookmark
{
    public class BookmarkSettingsModel
    {
        [JsonProperty]
        public bool ShowFileSize;

        [JsonProperty]
        public string ParsingPattern;

        [JsonProperty]
        public string ArticleExecution;
        [JsonProperty]
        public string ArtistExecution;
        [JsonProperty]
        public string GroupExecution;
    }

    public class BookmarkSettings : ILazy<BookmarkSettings>
    {
        string model_path = $"sbookmark-setting.json";
        BookmarkSettingsModel model;

        public BookmarkSettings()
        {
            if (File.Exists(model_path)) model = JsonConvert.DeserializeObject<BookmarkSettingsModel>(File.ReadAllText(model_path));
            if (model == null)
            {
                model = new BookmarkSettingsModel
                {
                    ShowFileSize = false,
                    ParsingPattern = @"",
                    ArticleExecution = "",
                    ArtistExecution = "",
                    GroupExecution = "",
                };
                Save();
            }
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(model, Formatting.Indented);
            using (var fs = new StreamWriter(new FileStream(model_path, FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }

        public BookmarkSettingsModel Model { get { return model; } }
    }
}
