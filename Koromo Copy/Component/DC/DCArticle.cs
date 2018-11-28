/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.DC
{
    public class DCArticle : IArticle
    {
        public string Id { get; set; }
        public string GalleryName { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public List<string> ImagesLink { get; set; }
        public List<string> FilesName { get; set; }
    }
}
