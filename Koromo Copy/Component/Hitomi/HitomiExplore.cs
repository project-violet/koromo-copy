/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.Hitomi
{
    public class HitomiExplore
    {
        /// <summary>
        /// 다운로드되지 않은 구간들을 가져옵니다.
        /// </summary>
        /// <param name="min_width">연속된 다운로드되지 않은 최소 작품 수 입니다.</param>
        /// <returns></returns>
        public static List<Tuple<int, int, int>> exploreNullSpace(int min_width = 5)
        {
            int count = 0;
            int starts = 0;
            int latest = 0;
            var result = new List<Tuple<int, int, int>>();
            foreach (var article in HitomiIndex.Instance.metadata_collection)
            {
                var lang = "n/a";
                if (article.Language >= 0) lang = HitomiIndex.Instance.index.Languages[article.Language];
                if (article.Language == null) lang = "n/a";
                if (Settings.Instance.Hitomi.Language != "all" &&
                    Settings.Instance.Hitomi.Language != lang)
                    continue;
                if (HitomiLog.Instance.Contains(article.ID))
                {
                    if (count > min_width)
                    {
                        result.Add(Tuple.Create(starts, latest, count));
                        count = 0;
                    }
                }
                else
                {
                    if (count == 0)
                        starts = article.ID;
                    count += 1;
                    latest = article.ID;
                }

            }
            return result;
        }
    }
}
