/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.Hitomi
{
    /// <summary>
    /// 콘솔용 검색 도구입니다.
    /// </summary>
    public class HitomiDataParser
    {
        public static async Task<List<HitomiIndexMetadata>> SearchAsync(string search)
        {
            HitomiDataQuery query = new HitomiDataQuery();
            List<string> positive_data = new List<string>();
            List<string> negative_data = new List<string>();
            List<string> request_number = new List<string>();
            int start_element = 0;
            int count_element = 0;
            bool recent = false;
            int recent_count = 0;
            int recent_start = 0;

            search.Trim().Split(' ').ToList().ForEach((a) => { if (a.StartsWith("/")) start_element = Convert.ToInt32(a.Substring(1)); });
            search.Trim().Split(' ').ToList().ForEach((a) => { if (a.StartsWith("?")) count_element = Convert.ToInt32(a.Substring(1)); });
            search.Trim().Split(' ').ToList().ForEach((a) => { if (!a.Contains(":") && !a.StartsWith("/") && !a.StartsWith("?")) positive_data.Add(a.Trim()); });
            query.Common = positive_data;
            query.Common.Add("");
            query.TagExclude = Settings.Instance.Hitomi.ExclusiveTag.ToList();
            foreach (var elem in from elem in search.Trim().Split(' ') where elem.Contains(":") where !elem.StartsWith("/") where !elem.StartsWith("?") select elem)
            {
                if (elem.StartsWith("tag:"))
                    if (query.TagInclude == null)
                        query.TagInclude = new List<string>() { elem.Substring("tag:".Length) };
                    else
                        query.TagInclude.Add(elem.Substring("tag:".Length));
                else if (elem.StartsWith("female:"))
                    if (query.TagInclude == null)
                        query.TagInclude = new List<string>() { elem };
                    else
                        query.TagInclude.Add(elem);
                else if (elem.StartsWith("male:"))
                    if (query.TagInclude == null)
                        query.TagInclude = new List<string>() { elem };
                    else
                        query.TagInclude.Add(elem);
                else if (elem.StartsWith("artist:"))
                    if (query.Artists == null)
                        query.Artists = new List<string>() { elem.Substring("artist:".Length) };
                    else
                        query.Artists.Add(elem.Substring("artist:".Length));
                else if (elem.StartsWith("series:"))
                    if (query.Series == null)
                        query.Series = new List<string>() { elem.Substring("series:".Length) };
                    else
                        query.Series.Add(elem.Substring("series:".Length));
                else if (elem.StartsWith("group:"))
                    if (query.Groups == null)
                        query.Groups = new List<string>() { elem.Substring("group:".Length) };
                    else
                        query.Groups.Add(elem.Substring("group:".Length));
                else if (elem.StartsWith("character:"))
                    if (query.Characters == null)
                        query.Characters = new List<string>() { elem.Substring("character:".Length) };
                    else
                        query.Characters.Add(elem.Substring("character:".Length));
                else if (elem.StartsWith("tagx:"))
                    if (query.TagExclude == null)
                        query.TagExclude = new List<string>() { elem.Substring("tagx:".Length) };
                    else
                        query.TagExclude.Add(elem.Substring("tagx:".Length));
                else if (elem.StartsWith("type:"))
                    if (query.Type == null)
                        query.Type = new List<string>() { elem.Substring("type:".Length) };
                    else
                        query.Type.Add(elem.Substring("type:".Length));
                else if (elem.StartsWith("lang:"))
                {
                    if (query.Language == null)
                        query.Language = elem.Substring("lang:".Length);
                }
                else if (elem.StartsWith("request:"))
                    request_number.Add(elem.Substring("request:".Length));
                else if (elem.StartsWith("recent:"))
                {
                    recent = true;
                    try
                    {
                        if (elem.Substring("recent:".Length).Contains("-"))
                        {
                            recent_start = Convert.ToInt32(elem.Substring("recent:".Length).Split('-')[0]);
                            recent_count = Convert.ToInt32(elem.Substring("recent:".Length).Split('-')[1]);
                        }
                        else
                            recent_count = Convert.ToInt32(elem.Substring("recent:".Length));
                    }
                    catch
                    {
                        Console.Console.Instance.WriteErrorLine($"'{elem}' is incorrect rule.");
                        return null;
                    }
                }
                else
                {
                    Console.Console.Instance.WriteErrorLine($"Unknown rule '{elem}'.");
                    return null;
                }
            }
            
            Stopwatch sw = Stopwatch.StartNew();
            List<HitomiIndexMetadata> query_result;
            if (recent == true)
            {
                Monitor.Instance.Push($"[Query GetSubsetOf] {recent_start} ~ {recent_count}");
                Monitor.Instance.Push(query);
                query_result = HitomiDataSearch.GetSubsetOf(recent_start, recent_count);
            }
            else
            {
                Monitor.Instance.Push("[Query HitomiMetadata] " + search);
                Monitor.Instance.Push(query);
                query_result = (await HitomiDataSearch.Search3(query));
            }
            var end = sw.ElapsedMilliseconds;
            sw.Stop();
            Monitor.Instance.Push($"[Query Results] {query_result.Count.ToString("#,#")} Articles ({end.ToString("#,#")} ms)");

            return query_result;
        }
    }
}
