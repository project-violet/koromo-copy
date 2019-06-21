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

namespace Koromo_Copy_UX.Utility
{
    public class ZipListingAutoComplete : IAutoCompleteAlgorithm
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

        public void Build(ZipListingModel src)
        {
            tagdata_collection = new HitomiTagdataCollection();

            tagdata_collection.artist?.Clear();
            tagdata_collection.tag?.Clear();
            tagdata_collection.female?.Clear();
            tagdata_collection.male?.Clear();
            tagdata_collection.group?.Clear();
            tagdata_collection.character?.Clear();
            tagdata_collection.series?.Clear();
            tagdata_collection.type?.Clear();
            tagdata_collection.language?.Clear();

            Dictionary<string, int> artist = new Dictionary<string, int>();
            Dictionary<string, int> tag = new Dictionary<string, int>();
            Dictionary<string, int> female = new Dictionary<string, int>();
            Dictionary<string, int> male = new Dictionary<string, int>();
            Dictionary<string, int> group = new Dictionary<string, int>();
            Dictionary<string, int> character = new Dictionary<string, int>();
            Dictionary<string, int> series = new Dictionary<string, int>();
            Dictionary<string, int> type = new Dictionary<string, int>();

            foreach (var article in src.ArticleList.Select(x => x.Value.ArticleData))
            {
                if (article.Artists != null) article.Artists.ToList().ForEach(x => Add(artist, x));
                if (article.Tags != null) article.Tags.ToList().ForEach(x => { if (x.StartsWith("female:")) Add(female, x); else if (x.StartsWith("male:")) Add(male, x); else Add(tag, x); });
                if (article.Groups != null) article.Groups.ToList().ForEach(x => Add(group, x));
                if (article.Characters != null) article.Characters.ToList().ForEach(x => Add(character, x));
                if (article.Series != null) article.Series.ToList().ForEach(x => Add(series, x));
                if (article.Types != null) Add(type, article.Types);
            }

            tagdata_collection.artist = artist.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.tag = tag.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.female = female.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.male = male.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.group = group.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.character = character.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.series = series.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.type = type.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.language = new List<HitomiTagdata>();
            
            tagdata_collection.artist.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.tag.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.female.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.male.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.group.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.character.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.series.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.type.Sort((a, b) => b.Count.CompareTo(a.Count));

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
                else if (word.StartsWith("tag:"))
                {
                    word = word.Substring("tag:".Length);
                    position += "tag:".Length;
                    match = HitomiDataAnalysis.GetTagList(word);
                }
                else if (word.StartsWith("tagx:"))
                {
                    word = word.Substring("tagx:".Length);
                    position += "tagx:".Length;
                    match = HitomiDataAnalysis.GetTagList(word);
                }
                else if (word.StartsWith("character:"))
                {
                    word = word.Substring("character:".Length);
                    position += "character:".Length;
                    match = HitomiDataAnalysis.GetCharacterList(word);
                }
                else if (word.StartsWith("group:"))
                {
                    word = word.Substring("group:".Length);
                    position += "group:".Length;
                    match = HitomiDataAnalysis.GetGroupList(word);
                }
                else if (word.StartsWith("series:"))
                {
                    word = word.Substring("series:".Length);
                    position += "series:".Length;
                    match = HitomiDataAnalysis.GetSeriesList(word);
                }
                else if (word.StartsWith("type:"))
                {
                    word = word.Substring("type:".Length);
                    position += "type:".Length;
                    match = HitomiDataAnalysis.GetTypeList(word);
                }
            }

            string[] match_target = {
                    "artist:",
                    "character:",
                    "group:",
                    "recent:",
                    "series:",
                    "tag:",
                    "tagx:",
                    "type:",
                };

            string w = word;
            List<HitomiTagdata> data_col = (from ix in match_target where ix.StartsWith(w) select new HitomiTagdata { Tag = ix }).ToList();
            if (data_col.Count > 0)
                match.AddRange(data_col);
            match.AddRange(HitomiDataAnalysis.GetTotalList(word));

            HitomiIndex.Instance.tagdata_collection = old_data;

            return match;
        }
    }
}
