/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
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
        /// 세밀한 버전 확인을 제공합니다.
        /// </summary>
        [JsonProperty]
        public bool SensitiveUpdateCheck;

        /// <summary>
        /// 가장 최근에 접근한 시간을 가져옵니다.
        /// </summary>
        [JsonProperty]
        public DateTime LatestAccessTime;

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
                model.SensitiveUpdateCheck = false;

                model.Hitomi = new HitomiSetting();
                model.Hitomi.Path = @"C:\Hitomi\{Artists}\[{Id}] {Title}\";
                model.Hitomi.Language = "korean";

                model.HitomiAnalysis = new HitomiAnalysisSetting();
            }
            else
            {
                model.LatestAccessTime = DateTime.Now;
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

        public SettingModel Model { get { return model; } }
        public HitomiSetting Hitomi { get { return model.Hitomi; } }
        public HitomiAnalysisSetting HitomiAnalysis { get { return model.HitomiAnalysis; } }
    }
}
