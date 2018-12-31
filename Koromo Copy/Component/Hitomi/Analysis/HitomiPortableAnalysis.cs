/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Koromo_Copy.Component.Hitomi.Analysis
{
    public class HitomiPortableAnalysis
    {
        public List<Tuple<string, double, string>> Rank;
        public List<Tuple<string, int>> CustomAnalysis = new List<Tuple<string, int>>();
        
        public void Update()
        {
            HitomiAnalysisArtist user;
            user = new HitomiAnalysisArtist(CustomAnalysis);

            ///////////////////////////////

            Dictionary<string, Tuple<double, HitomiAnalysisArtist>> score = new Dictionary<string, Tuple<double, HitomiAnalysisArtist>>();
            bool rms = Settings.Instance.HitomiAnalysis.UsingRMSAanlysis;
            bool cos = Settings.Instance.HitomiAnalysis.UsingCosineAnalysis;
            if (!rms && !cos) cos = true;
            if (rms && cos)
            {
                System.Windows.Forms.MessageBox.Show("[작가 추천 설정 오류] rms, cos는 동시에 사용할 수 없습니다.", "Hitomi Copy", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            ///////////////////////////////

            if (rms == true && user.GetDictionary().Count != 0)
            {
                double x_mean = user.GetDictionary().Sum(x => x.Value) / user.GetDictionary().Count;

                double va = user.GetDictionary().Sum(x => (x.Value - x_mean) * (x.Value - x_mean));

                foreach (var data in HitomiAnalysis.Instance.datas)
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

                foreach (var data in HitomiAnalysis.Instance.datas)
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
            
            var list = score.ToList();
            
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
