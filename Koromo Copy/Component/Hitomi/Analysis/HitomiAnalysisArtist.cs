/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koromo_Copy.Component.Hitomi.Analysis
{
    public class HitomiAnalysisArtist
    {
        Dictionary<string, float> rate = new Dictionary<string, float>();
        int tags_count = 0;
        public int MetadataCount { get; set; } = 0;
        public string Aritst { get; set; }

        public HitomiAnalysisArtist(string artist, List<HitomiIndexMetadata> metadatas)
        {
            Dictionary<string, int> tags_map = new Dictionary<string, int>();
            Aritst = artist;

            foreach (var metadata in metadatas)
            {
                if (metadata.Tags == null) continue;
                if (!Settings.Instance.HitomiAnalysis.RecommendLanguageALL)
                {
                    string lang = "N/A";
                    if (metadata.Language >= 0) lang = HitomiIndex.Instance.index.Languages[metadata.Language];
                    if (Settings.Instance.Hitomi.Language != "ALL" &&
                        Settings.Instance.Hitomi.Language != lang) continue;
                }
                tags_count += metadata.Tags.Length;
                MetadataCount += 1;
                foreach (var _tag in metadata.Tags)
                {
                    var tag = HitomiIndex.Instance.index.Tags[_tag];
                    if (Settings.Instance.HitomiAnalysis.UsingOnlyFMTagsOnAnalysis && 
                        !tag.StartsWith("female:") && !tag.StartsWith("male:")) continue;
                    if (tags_map.ContainsKey(tag))
                        tags_map[tag] += 1;
                    else
                        tags_map.Add(tag, 1);
                }
            }
            
            foreach(var pair in tags_map)
            {
                if (!Settings.Instance.HitomiAnalysis.UsingCosineAnalysis)
                {
                    if (!Settings.Instance.HitomiAnalysis.RecommendNMultipleWithLength)
                        rate.Add(pair.Key, pair.Value * pair.Value / (float)tags_count);
                    else
                        rate.Add(pair.Key, pair.Value / (float)tags_count);
                }
                else
                {
                    rate.Add(pair.Key, pair.Value);
                }
            }
        }

        public HitomiAnalysisArtist(IEnumerable<HitomiLogModel> logs)
        {
            Dictionary<string, int> tags_map = new Dictionary<string, int>();

            foreach (var log in logs.Where(log => log.Tags != null))
            {
                foreach (var tag in log.Tags)
                {
                    if (Settings.Instance.HitomiAnalysis.UsingOnlyFMTagsOnAnalysis && 
                        !tag.StartsWith("female:") && !tag.StartsWith("male:")) continue;
                    tags_count += 1;
                    if (tags_map.ContainsKey(HitomiLegalize.LegalizeTag(tag)))
                        tags_map[HitomiLegalize.LegalizeTag(tag)] += 1;
                    else
                        tags_map.Add(HitomiLegalize.LegalizeTag(tag), 1);
                }
            }
            
            foreach (var pair in tags_map)
            {
                if (!Settings.Instance.HitomiAnalysis.UsingCosineAnalysis)
                {
                    if (!Settings.Instance.HitomiAnalysis.RecommendNMultipleWithLength)
                        rate.Add(pair.Key, pair.Value * pair.Value / (float)tags_count);
                    else
                        rate.Add(pair.Key, pair.Value / (float)tags_count);
                }
                else
                {
                    rate.Add(pair.Key, pair.Value);
                }
            }
        }

        public HitomiAnalysisArtist(List<Tuple<string, int>> custom)
        {
            foreach (var c in custom)
                tags_count += c.Item2;

            foreach (var pair in custom)
            {
                if (!Settings.Instance.HitomiAnalysis.UsingCosineAnalysis)
                {
                    if (!Settings.Instance.HitomiAnalysis.RecommendNMultipleWithLength)
                        rate.Add(pair.Item1, pair.Item2 * pair.Item2 / (float)tags_count);
                    else
                        rate.Add(pair.Item1, pair.Item2 / (float)tags_count);
                }
                else
                {
                    rate.Add(pair.Item1, pair.Item2);
                }
            }
        }

        public bool IsExsit(string tag)
        {
            return rate.ContainsKey(tag);
        }

        public float GetRate(string tag)
        {
            return rate[tag];
        }

        public int Size()
        {
            return tags_count;
        }

        public Dictionary<string, float> GetDictionary()
        {
            return rate;
        }

        public string GetDetail(HitomiAnalysisArtist user)
        {
            Dictionary<string, double> tags_rate = new Dictionary<string, double>();
            foreach (var artist in rate)
                if (user.IsExsit(artist.Key))
                {
                    if (tags_rate.ContainsKey(artist.Key))
                        tags_rate[artist.Key] += user.GetRate(artist.Key) * artist.Value;
                    else
                        tags_rate.Add(artist.Key, user.GetRate(artist.Key) * artist.Value);
                }

            var list = tags_rate.ToList();
            list.Sort((p1, p2) => p2.Value.CompareTo(p1.Value));

            StringBuilder builder = new StringBuilder();
            foreach (var pair in list)
                builder.Append($"{pair.Key}\r\n");
                //builder.Append($"{pair.Key}({pair.Value}),");
            return builder.ToString();
        }
    }
}
