/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Koromo_Copy.Component.Hitomi.Analysis
{
    public class HitomiAnalysisRelatedTags
    {
        private static readonly Lazy<HitomiAnalysisRelatedTags> instance = new Lazy<HitomiAnalysisRelatedTags>(() => new HitomiAnalysisRelatedTags());
        public static HitomiAnalysisRelatedTags Instance => instance.Value;

        public Dictionary<string, List<Tuple<string, double>>> result = new Dictionary<string, List<Tuple<string, double>>>();

        public Dictionary<string, List<int>> tags_dic = new Dictionary<string, List<int>>();
        public List<KeyValuePair<string, List<int>>> tags_list;
        public List<Tuple<string, string, double>> results = new List<Tuple<string, string, double>>();

        public bool IncludeFemaleMaleOnly = false;
        public double Threshold = 0.1;

        public void Initialize()
        {
            result.Clear();
            results.Clear();
            tags_dic.Clear();
            if (tags_list != null) tags_list.Clear();

            bool IFM = IncludeFemaleMaleOnly;

            foreach (var data in HitomiIndex.Instance.metadata_collection)
            {
                if (data.Tags != null)
                {
                    foreach (var _tag in data.Tags)
                    {
                        var tag = HitomiIndex.Instance.index.Tags[_tag];
                        if (IFM && !tag.StartsWith("female:") && !tag.StartsWith("male:")) continue;
                        if (tags_dic.ContainsKey(tag))
                            tags_dic[tag].Add(data.ID);
                        else
                            tags_dic.Add(tag, new List<int> { data.ID });
                    }
                }
            }

            tags_list = tags_dic.ToList();

            tags_list.ForEach(x => x.Value.Sort());
            tags_list.Sort((a, b) => a.Value.Count.CompareTo(b.Value.Count));
        }

        public static int manually_intersect(List<int> a, List<int> b)
        {
            int intersect = 0;
            int i = 0, j = 0;
            for (; i < a.Count && j < b.Count;)
            {
                if (a[i] == b[j])
                {
                    intersect++;
                    i++;
                    j++;
                }
                else if (a[i] < b[j])
                {
                    i++;
                }
                else
                {
                    j++;
                }
            }
            return intersect;
        }

        public List<Tuple<string, string, double>> Intersect(int i)
        {
            List<Tuple<string, string, double>> result = new List<Tuple<string, string, double>>();

            for (int j = i + 1; j < tags_list.Count; j++)
            {
                int intersect = manually_intersect(tags_list[i].Value, tags_list[j].Value);
                int i_size = tags_list[i].Value.Count;
                int j_size = tags_list[j].Value.Count;
                double rate = (double)(intersect) / (i_size + j_size - intersect);
                if (rate >= Threshold)
                    result.Add(new Tuple<string, string, double>(tags_list[i].Key, tags_list[j].Key,
                        rate));
            }

            return result;
        }

        public void Merge()
        {
            foreach (var tuple in results)
            {
                if (result.ContainsKey(tuple.Item1))
                    result[tuple.Item1].Add(new Tuple<string, double>(tuple.Item2, tuple.Item3));
                else
                    result.Add(tuple.Item1, new List<Tuple<string, double>> { new Tuple<string, double>(tuple.Item2, tuple.Item3) });
                if (result.ContainsKey(tuple.Item2))
                    result[tuple.Item2].Add(new Tuple<string, double>(tuple.Item1, tuple.Item3));
                else
                    result.Add(tuple.Item2, new List<Tuple<string, double>> { new Tuple<string, double>(tuple.Item1, tuple.Item3) });
            }
            result.ToList().ForEach(x => x.Value.Sort((a, b) => b.Item2.CompareTo(a.Item2)));
            results.Clear();
        }
    }
}
