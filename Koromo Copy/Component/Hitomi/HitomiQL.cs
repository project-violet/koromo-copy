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
    /// <summary>
    /// Hitomi Qurey Library
    /// </summary>
    public class HitomiQL
    {
        public class Artist
        {
            public string name;
            public HashSet<int> articles;
            public Dictionary<string, int> tags;
            public Dictionary<string, int> series;
            public Dictionary<string, int> languages;
            public Dictionary<string, int> artists;
            public Dictionary<string, int> groups;
            public Dictionary<string, int> types;
        }

        public static List<Artist> GetArtistsList(bool language_filtering = true)
        {
            var dict = new Dictionary<string, HashSet<int>>();
            foreach (var md in HitomiData.Instance.metadata_collection)
            {
                if (language_filtering)
                {
                    var lang = md.Language;
                    if (md.Language == null) lang = "n/a";
                    if (Settings.Instance.Hitomi.Language != "all" &&
                        Settings.Instance.Hitomi.Language != lang) continue;
                }

                if (md.Artists != null)
                {
                    foreach (var artist in md.Artists)
                    {
                        if (!dict.ContainsKey(artist))
                            dict.Add(artist, new HashSet<int>());
                        dict[artist].Add(md.ID);
                    }
                }

                if (md.Groups != null)
                {
                    foreach (var group in md.Groups)
                    {
                        if (!dict.ContainsKey(group))
                            dict.Add(group, new HashSet<int>());
                        dict[group].Add(md.ID);
                    }
                }
            }
            return dict.Select(x => new Artist { name = x.Key, articles = x.Value }).ToList();
        }

        #region Allocation
        public static void AllocateTags(List<Artist> artists)
        {
            if (artists.Count == 0 || artists[0].tags != null) return;
            for (int i = 0; i < artists.Count; i++)
            {
                var dict = new Dictionary<string, int>();
                foreach (var article in artists[i].articles)
                {
                    var md = HitomiLegalize.GetMetadataFromMagic(article.ToString()).Value;
                    if (md.Tags == null) continue;

                    foreach (var tag in md.Tags)
                    {
                        //if (!dict.ContainsKey(tag))
                        //    dict.Add(tag, 0);
                        //dict[tag]++;
                    }
                }
                artists[i].tags = dict;
            }
        }
        public static void AllocateSeries(List<Artist> artists)
        {
            if (artists.Count == 0 || artists[0].series != null) return;
            for (int i = 0; i < artists.Count; i++)
            {
                var dict = new Dictionary<string, int>();
                foreach (var article in artists[i].articles)
                {
                    var md = HitomiLegalize.GetMetadataFromMagic(article.ToString()).Value;
                    if (md.Parodies == null) continue;

                    foreach (var series in md.Parodies)
                    {
                        //if (!dict.ContainsKey(series))
                        //    dict.Add(series, 0);
                        //dict[series]++;
                    }
                }
                artists[i].series = dict;
            }
        }
        public static void AllocateLanguages(List<Artist> artists)
        {
            if (artists.Count == 0 || artists[0].languages != null) return;
            for (int i = 0; i < artists.Count; i++)
            {
                var dict = new Dictionary<string, int>();
                foreach (var article in artists[i].articles)
                {
                    var md = HitomiLegalize.GetMetadataFromMagic(article.ToString()).Value;

                    var lang = md.Language;
                    //if (md.Language == null) lang = "n/a";
                    
                    //if (!dict.ContainsKey(lang))
                    //    dict.Add(lang, 0);
                    //dict[lang]++;
                }
                artists[i].languages = dict;
            }
        }
        public static void AllocateArtists(List<Artist> artists)
        {
            if (artists.Count == 0 || artists[0].artists != null) return;
            for (int i = 0; i < artists.Count; i++)
            {
                var dict = new Dictionary<string, int>();
                foreach (var article in artists[i].articles)
                {
                    var md = HitomiLegalize.GetMetadataFromMagic(article.ToString()).Value;
                    if (md.Artists == null) continue;

                    foreach (var artist in md.Artists)
                    {
                        //if (!dict.ContainsKey(artist))
                        //    dict.Add(artist, 0);
                        //dict[artist]++;
                    }
                }
                artists[i].artists = dict;
            }
        }
        public static void AllocateGroups(List<Artist> artists)
        {
            if (artists.Count == 0 || artists[0].groups != null) return;
            for (int i = 0; i < artists.Count; i++)
            {
                var dict = new Dictionary<string, int>();
                foreach (var article in artists[i].articles)
                {
                    var md = HitomiLegalize.GetMetadataFromMagic(article.ToString()).Value;
                    if (md.Groups == null) continue;

                    foreach (var group in md.Groups)
                    {
                        //if (!dict.ContainsKey(group))
                        //    dict.Add(group, 0);
                        //dict[group]++;
                    }
                }
                artists[i].groups = dict;
            }
        }
        public static void AllocateTypes(List<Artist> artists)
        {
            if (artists.Count == 0 || artists[0].types != null) return;
            for (int i = 0; i < artists.Count; i++)
            {
                var dict = new Dictionary<string, int>();
                foreach (var article in artists[i].articles)
                {
                    var md = HitomiLegalize.GetMetadataFromMagic(article.ToString()).Value;

                    var type = md.Type;
                    //if (md.Language == null) type = "n/a";

                    //if (!dict.ContainsKey(type))
                    //    dict.Add(type, 0);
                    //dict[type]++;
                }
                artists[i].types = dict;
            }
        }
        #endregion

        #region Filtering
        public static List<Artist> FilterTagContains(List<Artist> artists, List<string> tags)
        {
            AllocateTags(artists);
            var result = new List<Artist>();
            foreach (var artist in artists)
            {
                if (tags.All(x => artist.artists.ContainsKey(x)))
                    result.Add(artist);
            }
            return result;
        }
        #endregion

        #region Sorting
        public void SortingTagCount(List<Artist> artists, string tag)
        {
            artists.Sort((x, y) => y.artists[tag].CompareTo(x.artists[tag]));
        }

        public List<double> SortingTagRate(List<Artist> artists, string tag)
        {
            return null;
        }
        #endregion
    }
}
