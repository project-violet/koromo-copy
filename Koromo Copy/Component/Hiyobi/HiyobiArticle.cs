/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System.Collections.Generic;

namespace Koromo_Copy.Component.Hiyobi
{
    public class HiyobiArticle : IArticle
    {
        string[] artists;
        string[] characters;
        string[] groups;
        string[] series;
        string[] tags;
        bool tags_kor = false;
        string type;

        string thumbnail;
        string magic_number;
        string title;

        List<string> images_link;

        public string[] Artists { get { return artists; } set { artists = value; } }
        public string[] Characters { get { return characters; } set { characters = value; } }
        public string[] Groups { get { return groups; } set { groups = value; } }
        public string[] Series { get { return series; } set { series = value; } }
        public string[] Tags { get { return tags; } set { tags = value; } }
        public bool TagsKor { get { return tags_kor; } set { tags_kor = value; } }
        public string Type { get { return type; } set { type = value; } }
        public bool ManualPathOrdering { get; set; }
        public string ManualAdditionalPath { get; set; }
        public string DateTime { get; set; }

        public string Thumbnail { get { return thumbnail; } set { thumbnail = value; } }
        public string Magic { get { return magic_number; } set { magic_number = value; } }
        public string Title { get { return title; } set { title = value; } }

        public List<string> ImagesLink { get { return images_link; } set { images_link = value; } }
        public string Archive { get; set; }
    }
}
