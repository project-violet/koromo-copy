/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Koromo_Copy.Component.Hitomi.Analysis
{
    public class HitomiAnalysis
    {
        private static readonly Lazy<HitomiAnalysis> instance = new Lazy<HitomiAnalysis>(() => new HitomiAnalysis());
        public static HitomiAnalysis Instance => instance.Value;

        public List<HitomiAnalysisArtist> datas = new List<HitomiAnalysisArtist>();

        public List<Tuple<string, double, string>> Rank;
        public Dictionary<string, int> ArtistCount = new Dictionary<string, int>();

        public bool FilterArtists = false;
        public bool UserDefined = false;
        public bool MustInclude = false;
        public List<Tuple<string, int>> CustomAnalysis = new List<Tuple<string, int>>();

        public HitomiAnalysis()
        {
            Dictionary<string, List<HitomiIndexMetadata>> artists = new Dictionary<string, List<HitomiIndexMetadata>>();
            foreach (var metadata in HitomiIndex.Instance.metadata_collection)
                if (metadata.Artists != null)
                    foreach (var _artist in metadata.Artists)
                    {
                        var artist = HitomiIndex.Instance.index.Artists[_artist];
                        if (artists.ContainsKey(artist))
                            artists[artist].Add(metadata);
                        else
                            artists.Add(artist, new List<HitomiIndexMetadata>() { metadata });
                    }

            foreach (var pair in artists)
                datas.Add(new HitomiAnalysisArtist(pair.Key, pair.Value));
            foreach (var haa in datas)
                ArtistCount.Add(haa.Aritst, haa.MetadataCount);
        }

        public void Update()
        {
            HitomiAnalysisArtist user;
            if (!UserDefined)
                user = new HitomiAnalysisArtist(HitomiLog.Instance.GetEnumerator());
            else
                user = new HitomiAnalysisArtist(CustomAnalysis);

            ///////////////////////////////

            Dictionary<string, Tuple<double, HitomiAnalysisArtist>> score = new Dictionary<string, Tuple<double, HitomiAnalysisArtist>>();
            bool xi = Settings.Instance.HitomiAnalysis.UsingXiAanlysis;
            bool rms = Settings.Instance.HitomiAnalysis.UsingRMSAanlysis;
            bool cos = Settings.Instance.HitomiAnalysis.UsingCosineAnalysis;
            if ((!(xi ^ rms ^ cos) && (xi | rms | cos)) || (xi & rms & cos))
            {
                System.Windows.Forms.MessageBox.Show("[작가 추천 설정 오류] xi, rms, cos는 동시에 사용할 수 없습니다.", "Hitomi Copy", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            ///////////////////////////////

            if (!rms && !cos)
            {
                foreach (var pair in user.GetDictionary())
                {
                    foreach (var data in datas)
                    {
                        if (!xi)
                        {
                            if (data.IsExsit(pair.Key))
                                if (score.ContainsKey(data.Aritst))
                                    score[data.Aritst] = new Tuple<double, HitomiAnalysisArtist>(score[data.Aritst].Item1 + pair.Value * data.GetRate(pair.Key), score[data.Aritst].Item2);
                                else
                                    score.Add(data.Aritst, new Tuple<double, HitomiAnalysisArtist>(pair.Value * data.GetRate(pair.Key), data));
                        }
                        else
                        {
                            if (data.IsExsit(pair.Key))
                            {
                                if (score.ContainsKey(data.Aritst))
                                    score[data.Aritst] = new Tuple<double, HitomiAnalysisArtist>(score[data.Aritst].Item1 + -Math.Pow(Math.Abs(pair.Value - data.GetRate(pair.Key)), 2) * pair.Value, score[data.Aritst].Item2);
                                else
                                    score.Add(data.Aritst, new Tuple<double, HitomiAnalysisArtist>(-Math.Pow(Math.Abs(pair.Value - data.GetRate(pair.Key)), 2) * pair.Value, data));
                            }
                        }
                    }
                }
            }
            else if (rms == true && user.GetDictionary().Count != 0)
            {
                double x_mean = user.GetDictionary().Sum(x => x.Value) / user.GetDictionary().Count;

                double va = user.GetDictionary().Sum(x => (x.Value - x_mean) * (x.Value - x_mean));

                foreach (var data in datas)
                {
                    if (data.GetDictionary().Count == 0) continue;
                    double y_mean = data.GetDictionary().Sum(x => x.Value) / data.GetDictionary().Count;

                    double a = 0.0;
                    HashSet<string> map = new HashSet<string>();

                    // 기본 형태는 rms 기반이나, rms는 예측을 하는 방법이지, 유사도 측정 방법이 아님
                    // 따라서 계산에 사용되지 않는 태그들은 비율에 따라 반전시켜 계산하면
                    // tangent가 요동치므로 신뢰성있는 결과를 얻을 수 있을 것 같음 ㅇㅅㅇ
                    foreach (var pair in user.GetDictionary())
                    {
                        if (data.IsExsit(pair.Key))
                            a += (data.GetRate(pair.Key) - y_mean) * (pair.Value - x_mean);
                        else
                            a += -y_mean * (pair.Value - x_mean) * (pair.Value - x_mean);
                        map.Add(pair.Key);
                    }

                    foreach (var pair in data.GetDictionary())
                    {
                        if (!map.Contains(pair.Key))
                            a += -x_mean * x_mean * Math.Abs(data.GetRate(pair.Key) - y_mean);
                    }

                    if (a / va >= 1.0)
                        score.Add(data.Aritst, new Tuple<double, HitomiAnalysisArtist>(va / a * 100, data));
                    else if (a / va > 0.0)
                        score.Add(data.Aritst, new Tuple<double, HitomiAnalysisArtist>(a / va * 100, data));
                    else
                    {
                        double v = -a / va;
                        if (v >= 1.0)
                            score.Add(data.Aritst, new Tuple<double, HitomiAnalysisArtist>(va / a * 100, data));
                        else
                            score.Add(data.Aritst, new Tuple<double, HitomiAnalysisArtist>(a / va * 100, data));
                    }
                }
            }
            else if (cos == true && user.GetDictionary().Count != 0)
            {
                double s_user = Math.Sqrt(user.GetDictionary().Sum(x => x.Value * x.Value));

                foreach (var data in datas)
                {
                    double s_data = Math.Sqrt(data.GetDictionary().Sum(x => x.Value * x.Value));
                    double dist = 0.0;

                    if (s_user * s_data == 0.0) continue;
                    
                    foreach (var pair in user.GetDictionary())
                    {
                        if (data.IsExsit(pair.Key))
                            dist += data.GetRate(pair.Key) * pair.Value;
                    }

                    score.Add(data.Aritst, new Tuple<double, HitomiAnalysisArtist>(dist / (s_user * s_data) * 100, data));
                }
            }

            ///////////////////////////////

            // 태그 차집합 감소 연산
            if (xi)
            {
                foreach (var pair in user.GetDictionary())
                {
                    foreach (var data in datas)
                    {
                        if (!data.IsExsit(pair.Key) && score.ContainsKey(data.Aritst))
                            score[data.Aritst] = new Tuple<double, HitomiAnalysisArtist>(score[data.Aritst].Item1 + -pair.Value * pair.Value * pair.Value, score[data.Aritst].Item2);
                    }
                }
            }

            ///////////////////////////////

            var list = score.ToList();

            // 하한값을 0으로 설정
            if (xi)
            {
                double min_score = 0.0;
                foreach (var tuple in list)
                {
                    min_score = Math.Min(min_score, tuple.Value.Item1);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = new KeyValuePair<string, Tuple<double, HitomiAnalysisArtist>>(list[i].Key, new Tuple<double, HitomiAnalysisArtist>((list[i].Value.Item1 - min_score) * 100, list[i].Value.Item2));
                }
            }

            ///////////////////////////////

            if (FilterArtists)
            {
                Dictionary<string, int> artists_galleris_count_log = new Dictionary<string, int>();

                foreach (var artist in HitomiLog.Instance.GetEnumerator().Where(data => data.Artists != null).SelectMany(data => data.Artists))
                {
                    if (artists_galleris_count_log.ContainsKey(artist))
                        artists_galleris_count_log[artist] += 1;
                    else
                        artists_galleris_count_log.Add(artist, 1);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    if (artists_galleris_count_log.ContainsKey(list[i].Key))
                    {
                        float mul = 1 - (float)artists_galleris_count_log[list[i].Key] / list[i].Value.Item2.MetadataCount;
                        list[i] = new KeyValuePair<string, Tuple<double, HitomiAnalysisArtist>>(list[i].Key, new Tuple<double, HitomiAnalysisArtist>(list[i].Value.Item1 * mul, list[i].Value.Item2));
                    }
                }
            }

            ///////////////////////////////

            if (MustInclude)
            {
                int count = CustomAnalysis.Count;
                for (int i = 0; i < list.Count; i++)
                {
                    if (CustomAnalysis.Any(x => !list[i].Value.Item2.IsExsit(x.Item1)))
                        list.RemoveAt(i--);
                }
            }

            ///////////////////////////////

            list.Sort((p1, p2) => p2.Value.Item1.CompareTo(p1.Value.Item1));

            if (Rank != null) Rank.Clear();
            else Rank = new List<Tuple<string, double, string>>();
            
            foreach (var item in list)
            {
                Rank.Add(new Tuple<string, double, string>(item.Key, item.Value.Item1, item.Value.Item2.GetDetail(user)));
            }
        }
    }
}
