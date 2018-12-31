/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Koromo_Copy.Component.Hitomi.Analysis
{
    public class HitomiAnalysisArtistCount
    {
        private static readonly Lazy<HitomiAnalysisArtistCount> instance = new Lazy<HitomiAnalysisArtistCount>(() => new HitomiAnalysisArtistCount());
        public static HitomiAnalysisArtistCount Instance => instance.Value;

        public List<KeyValuePair<string, int>> artist_count;

        public HitomiAnalysisArtistCount()
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (var artist in HitomiLog.Instance.GetEnumerator().Where(data => data.Artists != null).SelectMany(data => data.Artists))
            {
                if (dic.ContainsKey(artist))
                    dic[artist]++;
                else
                    dic.Add(artist, 1);
            }

            artist_count = dic.ToList();
            artist_count.Sort((a, b) => b.Value.CompareTo(a.Value));
        }
    }
}
