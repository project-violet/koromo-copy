/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using Koromo_Copy.Interface;

namespace Koromo_Copy.Component.EH
{
    public class EHentaiArticle : IArticle
    {
        public string Thumbnail { get; set; }

        public string Title { get; set; }
        public string SubTitle { get; set; }

        public string Type { get; set; }
        public string Uploader { get; set; }

        public string Posted;
        public string Parent;
        public string Visible;
        public string Language;
        public string FileSize;
        public int Length;
        public int Favorited;

        public string reclass;
        public string[] language;
        public string[] group;
        public string[] parody;
        public string[] character;
        public string[] artist;
        public string[] male;
        public string[] female;
        public string[] misc;

        public Tuple<DateTime, string, string>[] comment;
        public List<string> ImagesLink { get; set; }
        public string Archive { get; set; }
    }

    public class EHentaiResultArticle
    {
        public string URL;

        public string Thumbnail;
        public string Title;

        public string Uploader;
        public string Published;
        public string Files;
        public string Type;

        public Dictionary<string, List<string>> Descripts;
    }

}
