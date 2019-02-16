/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Koromo_Copy.Component.Hitomi
{
    /// <summary>
    /// 한 작품의 정보를 담고있는 클래스입니다.
    /// </summary>
    public class NHitomiArticle : IArticle
    {
        string[] artists;
        string[] characters;
        string[] groups;
        string language;
        string[] series;
        string[] tags;
        string type;

        string thumbnail;
        string magic_number;
        string title;

        List<string> images_link;

        [JsonProperty(PropertyName = "a")]
        public string[] Artists { get { return artists; } set { artists = value; } }
        [JsonProperty(PropertyName = "c")]
        public string[] Characters { get { return characters; } set { characters = value; } }
        [JsonProperty(PropertyName = "g")]
        public string[] Groups { get { return groups; } set { groups = value; } }
        [JsonProperty(PropertyName = "l")]
        public string Language { get { return language; } set { language = value; } }
        [JsonProperty(PropertyName = "s")]
        public string[] Series { get { return series; } set { series = value; } }
        [JsonProperty(PropertyName = "t")]
        public string[] Tags { get { return tags; } set { tags = value; } }
        [JsonProperty(PropertyName = "y")]
        public string Type { get { return type; } set { type = value; } }
        [JsonIgnore]
        public bool ManualPathOrdering { get; set; }
        [JsonIgnore]
        public string ManualAdditionalPath { get; set; }
        [JsonProperty(PropertyName = "d")]
        public string DateTime { get; set; }

        [JsonProperty(PropertyName = "u")]
        public string Thumbnail { get { return thumbnail; } set { thumbnail = value; } }
        [JsonProperty(PropertyName = "m")]
        public string Magic { get { return magic_number; } set { magic_number = value; } }
        [JsonProperty(PropertyName = "i")]
        public string Title { get { return title; } set { title = value; } }

        [JsonIgnore]
        public List<string> ImagesLink { get { return images_link; } set { images_link = value; } }

        [JsonIgnore]
        public string Archive { get; set; }

        [JsonIgnore]
        public bool IsUnstable;

        [JsonIgnore]
        public HArticleModel UnstableModel;
    }

    public class HitomiArticle : IArticle
    {
        string[] artists;
        string[] characters;
        string[] groups;
        string language;
        string[] series;
        string[] tags;
        string type;

        string thumbnail;
        string magic_number;
        string title;

        List<string> images_link;
        
        public string[] Artists { get { return artists; } set { artists = value; } }
        public string[] Characters { get { return characters; } set { characters = value; } }
        public string[] Groups { get { return groups; } set { groups = value; } }
        public string Language { get { return language; } set { language = value; } }
        public string[] Series { get { return series; } set { series = value; } }
        public string[] Tags { get { return tags; } set { tags = value; } }
        public string Type { get { return type; } set { type = value; } }
        public bool ManualPathOrdering { get; set; }
        public string ManualAdditionalPath { get; set; }
        public string DateTime { get; set; }

        public string Thumbnail { get { return thumbnail; } set { thumbnail = value; } }
        public string Magic { get { return magic_number; } set { magic_number = value; } }
        public string Title { get { return title; } set { title = value; } }
        
        public List<string> ImagesLink { get { return images_link; } set { images_link = value; } }
        
        public string Archive { get; set; }

        [JsonIgnore]
        public bool IsUnstable;

        [JsonIgnore]
        public HArticleModel UnstableModel;
    }
}
