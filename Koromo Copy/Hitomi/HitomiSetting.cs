/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;

namespace Koromo_Copy.Hitomi
{
    public class HitomiSetting
    {
        [JsonProperty]
        public string Path;
        [JsonProperty]
        public string Language;
        [JsonProperty]
        public bool AutoSync;

        [JsonProperty]
        public bool UsingOptimization;
    }
}
