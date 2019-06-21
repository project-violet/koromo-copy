/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy_UX.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy_UX.Utility.ZipArtists
{
    public class ZipArtistsAutoComplete : IAutoCompleteAlgorithm
    {
        HitomiTagdataCollection tagdata_collection;
        bool builded = false;

        private void Add(Dictionary<string, int> dic, string key)
        {
            if (dic.ContainsKey(key))
                dic[key] += 1;
            else
                dic.Add(key, 1);
        }

        public void Build(ZipArtistsModel src)
        {
            tagdata_collection = new HitomiTagdataCollection();
            tagdata_collection.artist?.Clear();

            Dictionary<string, int> artist = new Dictionary<string, int>();
            
            src.ArtistList.Select(x => x.Value).Select(x => x.ArticleData.Select(y => y.Value)).ToList().ForEach(x => x.ToList().ForEach(article =>
            {
                if (article.Artists != null) article.Artists.ToList().ForEach(z => Add(artist, z));
            }));

            tagdata_collection.artist = artist.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.artist.Sort((a, b) => b.Count.CompareTo(a.Count));
            builded = true;
        }

        public List<HitomiTagdata> GetResults(ref string word, ref int position)
        {
            var match = new List<HitomiTagdata>();

            if (!builded) return match;

            var old_data = HitomiIndex.Instance.tagdata_collection;
            HitomiIndex.Instance.tagdata_collection = tagdata_collection;

            if (word.Contains(":"))
            {
                if (word.StartsWith("artist:"))
                {
                    word = word.Substring("artist:".Length);
                    position += "artist:".Length;
                    match = HitomiDataAnalysis.GetArtistList(word);
                }
            }

            string[] match_target = {
                    "artist:",
                };

            string w = word;
            List<HitomiTagdata> data_col = (from ix in match_target where ix.StartsWith(w) select new HitomiTagdata { Tag = ix }).ToList();
            if (data_col.Count > 0)
                match.AddRange(data_col);
            match.AddRange(HitomiDataAnalysis.GetArtistList(word));

            HitomiIndex.Instance.tagdata_collection = old_data;

            return match;
        }
    }
}
