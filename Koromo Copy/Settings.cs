/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Hitomi;
using Koromo_Copy.Interface;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Koromo_Copy
{
    public class SettingModel
    {
        [JsonProperty]
        public HitomiSetting Hitomi;

        [JsonProperty]
        public HitomiAnalysisSetting HitomiAnalysis;

        /// <summary>
        /// 이미지 다운로드에 사용할 쓰레드 수를 지정합니다.
        /// 기본으로 논리 코어수 * 3 만큼 설정됩니다.
        /// </summary>
        [JsonProperty]
        public int Thread;
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
                model.Thread = Environment.ProcessorCount * 3;

                model.Hitomi = new HitomiSetting();
                model.Hitomi.Path = @"C:\Hitomi\{Artists}\[{Id}] {Title}\";
                model.Hitomi.Language = "korean";

                model.HitomiAnalysis = new HitomiAnalysisSetting();
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
        public ref HitomiAnalysisSetting HitomiAnalysis { get { return ref model.HitomiAnalysis; } }
    }
}
