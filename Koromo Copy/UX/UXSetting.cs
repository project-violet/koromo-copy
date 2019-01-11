/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.UX
{
    public class UXSetting
    {
        /// <summary>
        /// Search space의 휠 속도입니다.
        /// </summary>
        [JsonProperty]
        public double SearchSpaceWheelSpeed;

        /// <summary>
        /// Artist Viewer Window의 휠 속도입니다.
        /// </summary>
        [JsonProperty]
        public double ArtistViewerWheelSpeed;

        /// <summary>
        /// 자동완성결과에 하이라이트를 적용할지의 여부를 설정합니다.
        /// </summary>
        [JsonProperty]
        public bool DoNotHightlightAutoCompleteResults;

        /// <summary>
        /// 자동완성결과의 최대 개수를 지정합니다.
        /// </summary>
        [JsonProperty]
        public int MaxCountOfAutoCompleteResult;

        /// <summary>
        /// 테마 색상입니다.
        /// </summary>
        public Color ThemeColor;

        /// <summary>
        /// 썸네일 검색결과를 보여줍니다.
        /// </summary>
        public bool UsingThumbnailSearchElements;
    }
}
