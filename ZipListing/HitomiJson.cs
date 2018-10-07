/* Copyright (C) 2018. Hitomi Parser Developers */

using Newtonsoft.Json;

namespace Hitomi_Copy_2
{
    public class HitomiJsonModel
    {
        [JsonProperty]
        public string Id;
        [JsonProperty]
        public string Title;
        [JsonProperty]
        public string[] Artists;
        [JsonProperty]
        public string[] Series;
        [JsonProperty]
        public string Types;
        [JsonProperty]
        public int Pages;
        [JsonProperty]
        public string[] Tags;
    }

}