/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System.Collections.Generic;

namespace Koromo_Copy.Interface
{
    /// <summary>
    /// 한 작품을 나타내는 단위입니다.
    /// </summary>
    public interface IArticle
    {
        string Thumbnail { get; set; }
        string Title { get; set; }
        string Archive { get; set; }
        List<string> ImagesLink { get; set; }
    }

    /// <summary>
    /// 한 시리즈를 나타내는 단위입니다.
    /// </summary>
    public interface ISeries
    {
        string Thumbnail { get; set; }
        string Title { get; set; }
        string[] Archive { get; set; }
        List<IArticle> Articles { get; set; }
    }
}
