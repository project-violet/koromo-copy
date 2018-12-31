/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Net
{
    public class NetSetting
    {
        [JsonProperty]
        public bool DisableSeleniumHeadless;

        [JsonProperty]
        public bool TimeoutInfinite;

        [JsonProperty]
        public bool UsingFirefoxWebDriver;

        [JsonProperty]
        public int TimeoutMillisecond;

        [JsonProperty]
        public int DownloadBufferSize;

        [JsonProperty]
        public int RetryCount;

        [JsonProperty]
        public int ServicePointConnectionLimit;
    }
}
