/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Koromo_Copy.Component.Hitomi
{
    public struct HitomiTagdata
    {
        public string Tag { get; set; }
        public int Count { get; set; }
    }

    public struct HitomiTagdataCollection
    {
        public List<HitomiTagdata> language { get; set; }
        public List<HitomiTagdata> female { get; set; }
        public List<HitomiTagdata> series { get; set; }
        public List<HitomiTagdata> character { get; set; }
        public List<HitomiTagdata> artist { get; set; }
        public List<HitomiTagdata> group { get; set; }
        public List<HitomiTagdata> tag { get; set; }
        public List<HitomiTagdata> male { get; set; }
        public List<HitomiTagdata> type { get; set; }
    }

    public struct HitomiMetadata
    {
        [JsonProperty(PropertyName = "a")]
        public string[] Artists { get; set; }
        [JsonProperty(PropertyName = "g")]
        public string[] Groups { get; set; }
        [JsonProperty(PropertyName = "p")]
        public string[] Parodies { get; set; }
        [JsonProperty(PropertyName = "t")]
        public string[] Tags { get; set; }
        [JsonProperty(PropertyName = "c")]
        public string[] Characters { get; set; }
        [JsonProperty(PropertyName = "l")]
        public string Language { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "id")]
        public int ID { get; set; }
    }
}
