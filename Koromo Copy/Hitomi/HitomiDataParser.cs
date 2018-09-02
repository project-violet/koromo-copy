/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Koromo_Copy.Hitomi
{
    public class HitomiDataParser
    {
        public static async Task<List<HitomiMetadata>> SearchAsync(string search)
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
            //tbExcludeTag.Text.Trim().Split(' ').ToList().ForEach((a) => negative_data.Add(Regex.Replace(a.Trim(), ",", "")));
            query.Common = positive_data;
            query.TagExclude = negative_data;
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
                        //MetroMessageBox.Show(this, $"recent 규칙 오류입니다. \"{elem}\"", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                    //tbSearch.Text = "recent:" + (recent_start + recent_count) + "-" + recent_count;
                }
                else
                {
                    Console.Console.Instance.WriteErrorLine($"Unknown rule '{elem}'.");
                    //MetroMessageBox.Show(this, $"알 수 없는 규칙입니다. \"{elem}\"", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            List<HitomiMetadata> query_result;
            if (recent == true)
            {
                query_result = HitomiDataSearch.GetSubsetOf(recent_start, recent_count);
            }
            else
            {
                query_result = (await HitomiDataSearch.Search3(query));
            }
            return query_result;
        }
    }
}
