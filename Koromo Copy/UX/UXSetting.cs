/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    }
}
