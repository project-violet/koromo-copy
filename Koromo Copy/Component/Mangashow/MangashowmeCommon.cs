/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.Mangashow
{
    public class MangashowmeCommon
    {
        public static string GetDownloadMangaImageAddress(string x) => $"https://mangashow.me/bbs/board.php?bo_table=msm_manga&wr_id={x}";
    }
}
