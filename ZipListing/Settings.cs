/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;
using System;
using System.IO;

namespace ZipListing
{
    public class SettingsModel
    {
        [JsonProperty]
        public int SortOrderBy;
    }

    public class Settings
    {
        private static readonly Lazy<Settings> instance = new Lazy<Settings>(() => new Settings());
        public static Settings Instance => instance.Value;
        string log_path = $"{Environment.CurrentDirectory}\\setting.json";

        public SettingsModel Model { get; private set; }
        public bool SettingCompleted { get; private set; } = true;

        public Settings()
        {
            if (File.Exists(log_path)) Model = JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText(log_path));
            if (Model == null)
            {
                SettingCompleted = false;
                Model = new SettingsModel();

                string json = JsonConvert.SerializeObject(Model, Formatting.Indented);
                using (var fs = new StreamWriter(new FileStream(log_path, FileMode.Create, FileAccess.Write)))
                {
                    fs.Write(json);
                }
            }
        }
    }
}
