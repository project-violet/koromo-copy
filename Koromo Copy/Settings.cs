/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Hitomi;
using Koromo_Copy.Interface;
using Newtonsoft.Json;
using System.IO;

namespace Koromo_Copy
{
    public class SettingModel
    {
        [JsonProperty]
        public HitomiSetting Hitomi;
    }

    public class Settings : ILazy<Settings>
    {
        string log_path = $"setting.json";
        SettingModel model;

        public Settings()
        {
            if (File.Exists(log_path)) model = JsonConvert.DeserializeObject<SettingModel>(File.ReadAllText(log_path));
            if (model == null)
            {
                model = new SettingModel();
                model.Hitomi = new HitomiSetting();
                model.Hitomi.Path = @"C:\Hitomi\{Artists}\[{Id}] {Title}\";
                model.Hitomi.Language = "korean";
            }
            Save();
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(model, Formatting.Indented);
            using (var fs = new StreamWriter(new FileStream(log_path, FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }

        public ref SettingModel Model { get { return ref model; } }
        public ref HitomiSetting Hitomi { get { return ref model.Hitomi; } }
    }
}
