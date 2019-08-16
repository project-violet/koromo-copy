/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Pinterest;
using Koromo_Copy.Component.Pixiv;
using Koromo_Copy.Interface;
using Koromo_Copy.KSS;
using Koromo_Copy.Net;
using Koromo_Copy.UX;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Threading;

namespace Koromo_Copy
{
    public class SettingModel
    {
        [JsonProperty]
        public HitomiSetting Hitomi;

        [JsonProperty]
        public HitomiAnalysisSetting HitomiAnalysis;

        [JsonProperty]
        public UXSetting UXSetting;

        [JsonProperty]
        public PixivSetting Pixiv;

        [JsonProperty]
        public PinSetting Pinterest;

        [JsonProperty]
        public NetSetting Net;

        [JsonProperty]
        public KSSSetting KSS;

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

        /// <summary>
        /// 다운로드한 파일이 있는 폴더를 자동으로 압축합니다.
        /// 이후 다운로드된 파일은 삭제됩니다.
        /// </summary>
        [JsonProperty]
        public bool AutoZip;

        /// <summary>
        /// 표시할 이미지의 화질입니다.
        /// 0이 가장 높으며 숫자가 높아질 수록 낮아집니다.
        /// </summary>
        [JsonProperty]
        public int ImageQuality;

        /// <summary>
        /// 해상도가 낮은 이미지를 불러옵니다.
        /// </summary>
        [JsonProperty]
        public bool LowQualityImage;

        /// <summary>
        /// 다운로드할 파일이름을 업로드된 이름으로 지정합니다.
        /// </summary>
        [JsonProperty]
        public bool DownloadWithRawFileName;

        /// <summary>
        /// 마지막으로 다운로드한 스크립트 패키지의 버전입니다.
        /// </summary>
        [JsonProperty]
        public int ScriptPackageVersion;
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
                var lang = Thread.CurrentThread.CurrentCulture.ToString();
                var language = "all";

                switch (lang)
                {
                    case "ko-KR":
                        language = "korean";
                        break;

                    case "ja-JP":
                        language = "japanese";
                        break;

                    case "en-US":
                        language = "english";
                        break;
                }

                model = new SettingModel
                {
                    Thread = Environment.ProcessorCount * 3,
                    SensitiveUpdateCheck = false,
                    AutoZip = false,

                    Hitomi = new HitomiSetting
                    {
                        Path = @"C:\Hitomi\{Artists}\[{Id}] {Title}\",
                        Language = language,
                        CustomAutoComplete = new string[] { "recent:0-25" },
                        UsingSettingLanguageWhenAdvanceSearch = true,
                        UsingOptimization = true,
                        SaveJsonFile = true,
                        TextMatchingAccuracy = 5,
                        RecommendPerScroll = 10,
                        ExclusiveTag = new string[] { "female:mother", "male:anal", "male:guro", "female:guro", "male:snuff", "female:snuff" }
                    },

                    HitomiAnalysis = new HitomiAnalysisSetting
                    {
                        RecommendNMultipleWithLength = true,
                        UsingCosineAnalysis = true
                    },
                    
                    UXSetting = new UXSetting
                    {
                        ArtistViewerWheelSpeed = 1.5,
                        SearchSpaceWheelSpeed = 1.5,
                        DoNotHightlightAutoCompleteResults = false,
                        MaxCountOfAutoCompleteResult = 100,
                        ThemeColor = Color.Pink,
                        UsingThumbnailSearchElements = false
                    },

                    Pixiv = new PixivSetting
                    {
                        Path = @"C:\Pixiv\",
                        Id = "",
                        Password = ""
                    },

                    Pinterest = new PinSetting
                    {
                        Path = @"C:\Pinterest\",
                        Id = "",
                        Password = ""
                    },

                    Net = new NetSetting
                    {
                        TimeoutMillisecond = 10000,
                        DownloadBufferSize = 131072,
                        RetryCount = 100,
                        ServicePointConnectionLimit = 268435456
                    },
                };
            }
            else
            {
                FixSettings();
            }
            model.LatestAccessTime = DateTime.Now;
#if false
            model.Net.TimeoutInfinite = true;
#endif
            Save();

        }

        public void FixSettings()
        {
            if (Hitomi.ExclusiveTag == null)
            {
                Hitomi.ExclusiveTag = new string[0];
            }

            if (Pixiv == null)
            {
                model.Pixiv = new PixivSetting
                {
                    Path = @"C:\Pixiv\",
                    Id = "",
                    Password = ""
                };
            }

            if (Pinterest == null)
            {
                model.Pinterest = new PinSetting
                {
                    Path = @"C:\Pinterest\",
                    Id = "",
                    Password = ""
                };
            }

            if (Net == null)
            {
                model.Net = new NetSetting
                {
                    TimeoutMillisecond = 10000,
                    DownloadBufferSize = 131072,
                    RetryCount = 10,
                    ServicePointConnectionLimit = 268435456
                };
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

        public SettingModel Model { get { return model; } }
        public HitomiSetting Hitomi { get { return model.Hitomi; } }
        public HitomiAnalysisSetting HitomiAnalysis { get { return model.HitomiAnalysis; } }
        public UXSetting UXSetting { get { return model.UXSetting; } }
        public PixivSetting Pixiv { get { return model.Pixiv; } }
        public PinSetting Pinterest { get { return model.Pinterest; } }
        public NetSetting Net { get { return model.Net; } }
        public KSSSetting KSS { get { return model.KSS; } }
    }
}
