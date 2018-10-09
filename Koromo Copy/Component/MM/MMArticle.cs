/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System.Collections.Generic;
using Koromo_Copy.Interface;

namespace Koromo_Copy.Component.MM
{
    public class MMSeries
    {
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public string[] Archive { get; set; }
    }

    public class MMArticle : IArticle
    {
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public List<string> ImagesLink { get; set; }
    }
}
