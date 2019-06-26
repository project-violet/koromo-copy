/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.Hitomi
{
    public class HitomiDataSearch
    {
        public static bool IsIntersect(string[] target, List<string> source)
        {
            bool[] check = new bool[source.Count];
            for (int i = 0; i < source.Count; i++)
                if (target.Any(e => e.ToLower().Replace(' ', '_') == source[i].ToLower()))
                    check[i] = true;
            return check.All((x => x));
        }

        public static void IntersectCountSplit(string[] target, List<string> source, ref bool[] check)
        {
            if (target != null)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    if (target.Any(e => e.ToLower().Split(' ').Any(x => x.Contains(source[i].ToLower()))))
                        check[i] = true;
                    else if (target.Any(e => e.ToLower().Replace(' ', '_') == source[i]))
                        check[i] = true;
                }
            }
        }

        public static List<HitomiIndexMetadata> GetSubsetOf(int start, int count)
        {
            List<HitomiIndexMetadata> result = new List<HitomiIndexMetadata>();
            List<string> x_tag = Settings.Instance.Hitomi.ExclusiveTag.ToList();
            foreach (var v in HitomiIndex.Instance.metadata_collection)
            {
                string lang = "n/a";
                if (v.Language >= 0) lang = HitomiIndex.Instance.index.Languages[v.Language];
                if (Settings.Instance.Hitomi.Language != "all" &&
                    Settings.Instance.Hitomi.Language != lang) continue;
                if (v.Tags != null)
                {
                    int intersec_count = 0;
                    foreach (var tag in x_tag)
                    {
                        if (v.Tags.Any(vtag => HitomiIndex.Instance.index.Tags[vtag].ToLower().Replace(' ', '_') == tag.ToLower()))
                        {
                            intersec_count++;
                        }

                        if (intersec_count > 0) break;
                    }
                    if (intersec_count > 0) continue;
                }
                if (start > 0) { start--; continue; }
                result.Add(v);
                if (--count == 0)
                    break;
            }
            return result;
        }

        public static async Task<List<HitomiIndexMetadata>> Search3(HitomiDataQuery query)
        {
            int number = Environment.ProcessorCount;
            int term = HitomiIndex.Instance.metadata_collection.Count / number;

            List<Task<List<HitomiIndexMetadata>>> arr_task = new List<Task<List<HitomiIndexMetadata>>>();
            for (int i = 0; i < number; i++)
            {
                int k = i;
                if (k != number - 1)
                    arr_task.Add(new Task<List<HitomiIndexMetadata>>(() => search_internal(query, k * term, k * term + term)));
                else
                    arr_task.Add(new Task<List<HitomiIndexMetadata>>(() => search_internal(query, k * term, HitomiIndex.Instance.metadata_collection.Count)));
            }

            Parallel.ForEach(arr_task, task => task.Start());
            await Task.WhenAll(arr_task);

            List<HitomiIndexMetadata> result = new List<HitomiIndexMetadata>();
            for (int i = 0; i < number; i++)
            {
                result.AddRange(arr_task[i].Result);
            }
            result.Sort((a, b) => b.ID - a.ID);

            return result;
        }

        private static List<HitomiIndexMetadata> search_internal(HitomiDataQuery query, int starts, int ends)
        {
            List<HitomiIndexMetadata> result = new List<HitomiIndexMetadata>();
            for (int i = starts; i < ends; i++)
            {
                var v = HitomiIndex.Instance.metadata_collection[i];
                if (query.Common.Contains(v.ID.ToString()))
                {
                    result.Add(v);
                    continue;
                }
                string lang = "n/a"; //v.Language;
                if (v.Language >= 0) lang = HitomiIndex.Instance.index.Languages[v.Language];
                if (Settings.Instance.Hitomi.Language != "all" &&
                    Settings.Instance.Hitomi.Language != lang) continue;
                if (query.Language != null &&
                    query.Language != lang) continue;
                if (query.TagExclude != null)
                {
                    if (v.Tags != null)
                    {
                        int intersec_count = 0;
                        foreach (var tag in query.TagExclude)
                        {
                            if (v.Tags.Any(vtag => HitomiIndex.Instance.index.Tags[vtag].ToLower().Replace(' ', '_') == tag.ToLower()))
                            {
                                intersec_count++;
                            }

                            if (intersec_count > 0) break;
                        }
                        if (intersec_count > 0) continue;
                    }
                }
                bool[] check = new bool[query.Common.Count];
                if (query.Common.Count > 0)
                {
                    IntersectCountSplit(v.Name.Split(' '), query.Common, ref check);
                    if (v.Tags != null) IntersectCountSplit(v.Tags.Select(x => HitomiIndex.Instance.index.Tags[x]).ToArray(), query.Common, ref check);
                    if (v.Artists != null) IntersectCountSplit(v.Artists.Select(x => HitomiIndex.Instance.index.Artists[x]).ToArray(), query.Common, ref check);
                    if (v.Groups != null) IntersectCountSplit(v.Groups.Select(x => HitomiIndex.Instance.index.Groups[x]).ToArray(), query.Common, ref check);
                    if (v.Parodies != null) IntersectCountSplit(v.Parodies.Select(x => HitomiIndex.Instance.index.Series[x]).ToArray(), query.Common, ref check);
                    if (v.Characters != null) IntersectCountSplit(v.Characters.Select(x => HitomiIndex.Instance.index.Characters[x]).ToArray(), query.Common, ref check);
                }
                bool connect = false;
                if (check.Length == 0) { check = new bool[1]; check[0] = true; }
                if (check[0] && v.Artists != null && query.Artists != null) { check[0] = IsIntersect(v.Artists.Select(x => HitomiIndex.Instance.index.Artists[x]).ToArray(), query.Artists); connect = true; } else if (query.Artists != null) check[0] = false;
                if (check[0] && v.Tags != null && query.TagInclude != null) { check[0] = IsIntersect(v.Tags.Select(x => HitomiIndex.Instance.index.Tags[x]).ToArray(), query.TagInclude); connect = true; } else if (query.TagInclude != null) check[0] = false;
                if (check[0] && v.Groups != null && query.Groups != null) { check[0] = IsIntersect(v.Groups.Select(x => HitomiIndex.Instance.index.Groups[x]).ToArray(), query.Groups); connect = true; } else if (query.Groups != null) check[0] = false;
                if (check[0] && v.Parodies != null && query.Series != null) { check[0] = IsIntersect(v.Parodies.Select(x => HitomiIndex.Instance.index.Series[x]).ToArray(), query.Series); connect = true; } else if (query.Series != null) check[0] = false;
                if (check[0] && v.Characters != null && query.Characters != null) { check[0] = IsIntersect(v.Characters.Select(x => HitomiIndex.Instance.index.Characters[x]).ToArray(), query.Characters); connect = true; } else if (query.Characters != null) check[0] = false;
                if (check[0] && v.Type != null && query.Type != null) { check[0] = query.Type.Any(x => v.Type >= 0 && x == HitomiIndex.Instance.index.Types[v.Type].Replace(' ', '_')); connect = true; } else if (query.Type != null) check[0] = false;
                if (check.All((x => x)) && ((query.Common.Count == 0 && connect) || query.Common.Count > 0))
                    result.Add(v);
            }

            // required
            result.Sort((a, b) => b.ID - a.ID);
            return result;
        }
    }
}
