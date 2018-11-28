/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Script
{
    public class ScriptEngineCore
    {
        [JsonProperty]
        public string[] UrlPrefix;
        [JsonProperty]
        public ScriptElement Id;
        [JsonProperty]
        public ScriptElement Title;
        [JsonProperty]
        public ScriptElement Images;
        [JsonProperty]
        public ScriptElement Paths;
    }

    public class ScriptEngine
    {
        
    }
}
