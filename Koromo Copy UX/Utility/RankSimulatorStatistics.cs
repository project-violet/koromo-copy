/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Elo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy_UX.Utility
{
    public class RankSimulatorStatistics
    {
        /// <summary>
        /// 레이팅 내림차순으로 정렬한 작가 통계를 가져옵니다.
        /// </summary>
        /// <param name="sys"></param>
        /// <returns></returns>
        public static List<EloPlayer> GetArtistRanking(EloSystem sys)
        {
            var players = sys.Players;
            players.Sort((x, y) => y.R.CompareTo(x.R));
            return players;
        }

        /// <summary>
        /// 레이팅 내림차순으로 정렬한 태그 통계를 가져옵니다.
        /// </summary>
        /// <param name="sys"></param>
        /// <returns></returns>
        public static List<EloPlayer> GetTagRanking(EloSystem sys)
        {
            // 태그 비교를 위해 태그시스템을 생성합니다.
            EloSystem tag_sys = new EloSystem();
            HashSet<string> tags = new HashSet<string>();

            foreach (var article in HitomiIndex.Instance.metadata_collection)
                if (article.Tags != null)
                    article.Tags.ToList().ForEach(x => tags.Add(HitomiIndex.Instance.index.Tags[x]));

            var list = tags.ToList();
            list.Sort();
            tag_sys.AppendPlayer(list.Count);

            var tag_dic = new Dictionary<string, int>();
            for (int i = 0; i < list.Count; i++)
            {
                tag_sys.Players[i].Indentity = list[i];
                tag_dic.Add(list[i], i);
            }

            // 레이팅 시작
            foreach (var d in sys.Model.DHistory)
            {
                var a1 = HitomiLegalize.GetMetadataFromMagic(d.Item4.ToString());
                var a2 = HitomiLegalize.GetMetadataFromMagic(d.Item5.ToString());

                if (!a1.HasValue || !a2.HasValue)
                    continue;

                if (a1.Value.Tags == null || a2.Value.Tags == null)
                    continue;

                if (a1.Value.Tags.Length == 0 || a2.Value.Tags.Length == 0)
                    continue;

                HashSet<string> first = new HashSet<string>();
                HashSet<string> second = new HashSet<string>();

                foreach (var tag in a1.Value.Tags)
                    first.Add(HitomiIndex.Instance.index.Tags[tag]);
                foreach (var tag in a2.Value.Tags)
                    second.Add(HitomiIndex.Instance.index.Tags[tag]);

                // 태그 vs 태그가 아닌 작품 vs 작품의 레이팅이므로
                // 1:1 엘로레이팅이아닌 다 vs 다 레이팅 방법을 사용함
                double r1 = 0.0;
                double r2 = 0.0;

                // 먼저 작품에 포함된 태그들 레이팅의 평균을 가져온다.
                a1.Value.Tags.ToList().ForEach(x => r1 += tag_sys.Players[tag_dic[HitomiIndex.Instance.index.Tags[x]]].Rating);
                a2.Value.Tags.ToList().ForEach(x => r2 += tag_sys.Players[tag_dic[HitomiIndex.Instance.index.Tags[x]]].Rating);

                r1 /= a1.Value.Tags.Length;
                r2 /= a2.Value.Tags.Length;

                // 아래 레이팅 결과는 극단적인 경우로 적중확률이 0에 가까움
                if (r1 < 0.5)
                    r1 = 1500.0;
                if (r2 < 0.5)
                    r2 = 1500.0;

                // 두 작품의 평균레이팅으로 작품 승률을 계산한다.
                double e1 = 1 / (1 + Math.Pow(10, (r2 - r1) / 400));
                double e2 = 1 / (1 + Math.Pow(10, (r1 - r2) / 400));

                // 1일 경우 a1의 승리, 0일 경우 무승부
                if (d.Item3 == 1)
                {
                    // a1이 승리하였으므로 Win을 갱신한다.
                    foreach (var tag in a1.Value.Tags)
                        if (!second.Contains(HitomiIndex.Instance.index.Tags[tag]))
                        {
                            // 아래 두 과정이 의미하는 바는 다음과 같다.
                            // 리그 오브 레전드의 5vs5 실버 랭크게임을 생각해보자.
                            // 또한, 각 팀을 A팀, B팀이라고 하자.
                            //
                            // 리그 오브 레전드의 매칭 시스템 특성상 A팀과 B팀의 평균 승률은 같을 것이다. 하지만, A팀에는 
                            // 다이아 실력을 가진 대리유저가 있을 수 있으며, 있다고 가정하자. 이때 대리유저의 레이팅은 분명히
                            // 낮을 것이지만, 우리의 태그 레이팅에는 이러한 인자가 존재할 수 없으니, 각 유저의 실제 실력을 
                            // 해당 유저의 레이팅으로 삼는게, 우리의 레이팅시스템과 비교하기에 적절하다. 태그 레이팅에 이러한 
                            // 인자가 존재할 수 없는 이유는 작품 대 작품 비교 기반의 레이팅 시스템에서는 각 태그의 레이팅을 
                            // 의도적으로 변경하기가 불가능하기 때문이다. 또한, A팀에는 의도적으로 아래 랭크로 내려가려는 
                            // 패작유저가 있다고 가정하자. 그렇다면 두 팀의 평균 승률은 같아 질 수 있다.
                            //
                            // 이때 A팀이 승리하게 된다면, 대리 유저의 승리 기여도가 다른 팀원들에 비해서 매우 높을 것이다.
                            // 하지만, 두 팀의 평균 레이팅에 비해 대리 유저의 레이팅이 높으므로, 대리유저가 레이팅 점수를 많이 
                            // 받는 것은 정당하지 않다. 따라서 우리의 레이팅 시스템은 두 작품의 평균 레이팅과 작품에 포함된 
                            // 태그에 상대적 레이팅을 기반으로 공식을 만들었다.
                            //
                            // 위 예제에서 A팀의 승률이 50%이고, A팀에 대한 대리유저의 상대적 승률이 90%라고 한다면,
                            // 대리유저의 최종 승률은 70%로 계산된다. 90%의 승률보다 낮게 계산된 이유는 팀 기여도가
                            // 그만큼 커졌기 때문이다. 즉, 대리유저는 승리확률이 70%인 게임에 참여하게 된것이다.
                            //
                            // 우리의 레이팅 시스템에선 항상 두 팀의 승률이 동일한 것은 아니다. 극단적인 경우도 있다.
                            // A팀의 승률이 80%이고, A팀에 대한 대리유저의 상대적 승률이 90%라고 한다면, 대리유저의 최종 
                            // 승률은 85%로 계산된다. 즉, 대리유저는 승리확률이 85%인 게임에 참여하게 된것이며, 이 게임에서
                            // 패배할 시엔 많은 레이팅 점수를 잃을 테지만, 승리한다해도 적은 레이팅점수만 받게된다.
                            // 이와는 다르게 A팀에 대한 패작유저의 상대적 승률이 10%라고 한다면, 패작유저의 최종 승률은 
                            // 45%로 계산된다. 즉, 이 게임에서 지게된다면 패작유저는 적지 않은 레이팅 점수를 잃게 될 것이다.
                            
                            // 작품의 평균레이팅에 대한 태그의 승률을 계산한다.
                            double ew = 1 / (1 + Math.Pow(10, (r1 - tag_sys.Players[tag_dic[HitomiIndex.Instance.index.Tags[tag]]].R) / 400));

                            // a1의 예측 승률과 ew 승률의 평균을 업데이트한다.
                            tag_sys.UpdateWin(tag_dic[HitomiIndex.Instance.index.Tags[tag]], (ew + e1) / 2);
                        }
                    foreach (var tag in a2.Value.Tags)
                        if (!first.Contains(HitomiIndex.Instance.index.Tags[tag]))
                        {
                            double ew = 1 / (1 + Math.Pow(10, (r2 - tag_sys.Players[tag_dic[HitomiIndex.Instance.index.Tags[tag]]].R) / 400));
                            tag_sys.UpdateLose(tag_dic[HitomiIndex.Instance.index.Tags[tag]], (ew + e2) / 2);
                        }
                }
                else if (d.Item3 == 0)
                {
                    foreach (var tag in a1.Value.Tags)
                        if (!second.Contains(HitomiIndex.Instance.index.Tags[tag]))
                        {
                            double ew = 1 / (1 + Math.Pow(10, (r1 - tag_sys.Players[tag_dic[HitomiIndex.Instance.index.Tags[tag]]].R) / 400));
                            tag_sys.UpdateDraw(tag_dic[HitomiIndex.Instance.index.Tags[tag]], (ew + e1) / 2);
                        }
                    foreach (var tag in a2.Value.Tags)
                        if (!first.Contains(HitomiIndex.Instance.index.Tags[tag]))
                        {
                            double ew = 1 / (1 + Math.Pow(10, (r2 - tag_sys.Players[tag_dic[HitomiIndex.Instance.index.Tags[tag]]].R) / 400));
                            tag_sys.UpdateDraw(tag_dic[HitomiIndex.Instance.index.Tags[tag]], (ew + e2) / 2);
                        }
                }
            }

            var result = tag_sys.Players.ToList();
            result.Sort((x, y) => y.R.CompareTo(x.R));
            return result;
        }

        /// <summary>
        /// 랭킹시스템에 더이상 포함시키지 않을 작가들을 가져옵니다.
        /// min_rating보다 작은 레이팅을 가진 태그가 하나라도 존재할 경우 하위 min_rate의 태그는 필터링 태그에 포함됩니다.
        /// filter_rate는 전체 작품에서 적어도 필터링 태그 목록의 태그를 filter_rate만큼 가진 작가를 필터링할 때 쓰입니다.
        /// </summary>
        /// <param name="sys"></param>
        /// <returns></returns>
        public static HashSet<int> FilterClosingArtists(EloSystem sys, double min_rating = 1400.0, 
            double min_rate = 0.01, double filter_rate = 0.8)
        {
            var result = new HashSet<int>();
            var tag_rating = GetTagRanking(sys);

            // min_rating보다 낮은 태그가 없다면 제외시키지 않음
            if (tag_rating.Last().R > min_rating) return result;

            // 필터링 시작 위치
            var starts = tag_rating.Count * (1 - min_rate);
            tag_rating.RemoveRange(0, (int)starts);
            var filter_tags = new HashSet<string>();
            tag_rating.ForEach(x => filter_tags.Add(x.Indentity));

            // 작가, 태그, 태그수
            var artist_tags = new Dictionary<string, Dictionary<string, int>>();
            // 작가, 작품수
            var article_cnt = new Dictionary<string, int>();

            // 작가 작품 수 및 태그 수 수집
            foreach (var md in HitomiIndex.Instance.metadata_collection)
            {
                var artist = "";
                if (md.Artists != null && md.Artists.Length != 0)
                    artist = HitomiIndex.Instance.index.Artists[md.Artists[0]].Replace(' ', '_');
                else if (md.Groups != null && md.Groups.Length != 0)
                    artist = HitomiIndex.Instance.index.Groups[md.Groups[0]].Replace(' ', '_');
                else
                    continue;

                if (article_cnt.ContainsKey(artist))
                    article_cnt[artist] += 1;
                else
                    article_cnt.Add(artist, 1);

                if (md.Tags == null)
                    continue;

                Dictionary<string, int> tags;
                if (artist_tags.ContainsKey(artist))
                    tags = artist_tags[artist];
                else
                    tags = new Dictionary<string, int>();
                foreach (var _tag in md.Tags)
                {
                    var tag = HitomiIndex.Instance.index.Tags[_tag];
                    if (!filter_tags.Contains(tag)) continue;
                    if (tags.ContainsKey(tag))
                        tags[tag] += 1;
                    else
                        tags.Add(tag, 1);
                }

                if (!artist_tags.ContainsKey(artist))
                    artist_tags.Add(artist, tags);
            }

            // 인덱스 정보 수집
            var tag_dic = new Dictionary<string, int>();
            for (int i = 0; i < sys.Players.Count; i++)
                tag_dic.Add(sys.Players[i].Indentity, i);

            // 필터링 여부 판단
            var artist_tags_list = artist_tags.ToList();
            for (int i = 0; i < artist_tags_list.Count; i++)
            {
                var artist = artist_tags_list[i].Key;
                var article_count = article_cnt[artist];

                foreach (var tag in artist_tags_list[i].Value)
                    if (tag.Value >= filter_rate * article_count)
                    {
                        if (tag_dic.ContainsKey(artist))
                            result.Add(tag_dic[artist]);
                        break;
                    }
            }

            return result;
        }
    }
}
