/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.Manazero
{
    public class ManazeroArticle : IArticle
    {
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public List<string> ImagesLink { get; set; }
        public string ArticleLink { get; set; }
        public string Archive { get; set; }
    }
}
