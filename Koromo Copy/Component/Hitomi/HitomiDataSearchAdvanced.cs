/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.Hitomi
{
    /// <summary>
    /// 쿼리에 부여되는 결합 옵션목록입니다.
    /// </summary>
    public enum HitomiDataAdvancedQueryCombinationOption
    {
        /// <summary>
        /// And 연산으로 하위 옵션을 결합합니다.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Or 연산으로 하위 옵션을 결합합니다.
        /// </summary>
        Combination = 1,

        /// <summary>
        /// 차집합 연산입니다.
        /// Right operand에 포함된 모든 정보는 결합에 제외됩니다.
        /// </summary>
        Difference = 2,
    }

    /// <summary>
    /// 쿼리의 각 토큰에 부여되는 옵션목록입니다.
    /// </summary>
    public enum HitomiDataAdvancedQueryTokenOption
    {
        /// <summary>
        /// 기본 설정입니다. 
        /// 해당 토큰은 하위 역할 중 어떤 역할도 갖지 않습니다.
        /// </summary>
        Default = 0,
        
        /// <summary>
        /// 여집합 설정입니다.
        /// </summary>
        Complement = 1,
    }

    public enum HitomiDataAdvancedQueryTokenType
    {
        None,
        Common,
        Artists,
        Title,
        Groups,
        Series,
        Characters,
        Type,
        Language,
        Id,
        Tag,
        FemaleTag,
        MaleTag,
    }
    
    /// <summary>
    /// 고급검색을 위한 쿼리입니다.
    /// </summary>
    public class HitomiDataAdvancedQuery
    {
        public HitomiDataAdvancedQueryCombinationOption combination;
        public HitomiDataAdvancedQueryTokenOption option;

        public HitomiDataAdvancedQuery left_query;
        public HitomiDataAdvancedQuery right_query;

        public bool is_operator;

        public HitomiDataAdvancedQueryTokenType token_type;
        public string token;
    }
    
    public class HitomiDataSearchAdvanced
    {
        static Dictionary<char, int> priority_dic = new Dictionary<char, int>
        {
            {'(', -1},
            {'-',  0},
            {'+',  0},
            {'&',  0},
            {'|',  1},
            {'~',  2},
        };

        static Dictionary<string, HitomiDataAdvancedQueryTokenType> token_dic = new Dictionary<string, HitomiDataAdvancedQueryTokenType>()
        {
            {"tag", HitomiDataAdvancedQueryTokenType.Tag},
            {"female", HitomiDataAdvancedQueryTokenType.FemaleTag},
            {"male", HitomiDataAdvancedQueryTokenType.MaleTag},
            {"artist", HitomiDataAdvancedQueryTokenType.Artists},
            {"series", HitomiDataAdvancedQueryTokenType.Series},
            {"group", HitomiDataAdvancedQueryTokenType.Groups},
            {"character", HitomiDataAdvancedQueryTokenType.Characters},
            {"type", HitomiDataAdvancedQueryTokenType.Type},
            {"lang", HitomiDataAdvancedQueryTokenType.Language},
        };

        public static int get_priority(char op)
        {
            return priority_dic[op];
        }

        public static Stack<string> to_postfix(string query_string)
        {
            var stack = new Stack<char>();
            var result_stack = new Stack<string>();
            bool latest = false;
            bool complement = false;
            for (int i = 0; i < query_string.Length; i++)
            {
                var builder = new StringBuilder();
                while (i < query_string.Length && char.IsWhiteSpace(query_string[i])) i++;

                if ("()-+&|~".Contains(query_string[i]))
                    builder.Append(query_string[i]);
                else
                {
                    for (; i < query_string.Length && !char.IsWhiteSpace(query_string[i]); i++)
                    {
                        if ("()-+&|~".Contains(query_string[i])) break;
                        builder.Append(query_string[i]);
                    }
                }
                var token = builder.ToString().ToLower();

                if (token == "and") token = "+";
                else if (token == "or") token = "|";

                switch (token[0])
                {
                    case '(':
                        if (latest)
                        {
                            stack.Push('+');
                            latest = false;
                        }
                        if (complement)
                        {
                            stack.Push('~');
                            complement = false;
                        }
                        stack.Push('(');
                        break;

                    case ')':
                        while (stack.Peek() != '(' && stack.Count > 0) result_stack.Push(stack.Pop().ToString());
                        if (stack.Count == 0)
                        {
                            Monitor.Instance.Push($"[Advanced Search] Missmatch ')' token on '{query_string}'.");
                            throw new Exception("Missmatch closer!");
                        }
                        stack.Pop();
                        if (stack.Count > 0 && stack.Peek() == '~')
                            result_stack.Push(stack.Pop().ToString());
                        break;

                    case '-':
                    case '+':
                    case '&':
                    case '|':
                        var p = get_priority(token[0]);
                        while (stack.Count > 0)
                        {
                            if (get_priority(stack.Peek()) >= p)
                                result_stack.Push(stack.Pop().ToString());
                            else break;
                        }
                        stack.Push(token[0]);
                        latest = false;
                        break;

                    case '~':
                        complement = true;
                        break;

                    default:
                        if (latest == true)
                        {
                            var p1 = get_priority('+');
                            while (stack.Count > 0)
                            {
                                if (get_priority(stack.Peek()) >= p1)
                                    result_stack.Push(stack.Pop().ToString());
                                else break;
                            }
                            stack.Push('+');
                        }
                        result_stack.Push(token);
                        if (complement)
                        {
                            result_stack.Push("~");
                            complement = false;
                        }
                        latest = true;
                        break;
                }
            }

            while (stack.Count > 0) result_stack.Push(stack.Pop().ToString());

            return new Stack<string>(result_stack);
        }

        public static HitomiDataAdvancedQuery make_tree(string query_string)
        {
            var stack = new Stack<HitomiDataAdvancedQuery>();
            var postfix = to_postfix(query_string);

            while (postfix.Count > 0)
            {
                string token = postfix.Pop();

                switch (token[0])
                {
                    case '(': break;

                    case '-':
                        {
                            var s1 = stack.Pop();
                            var s2 = stack.Pop();
                            stack.Push(new HitomiDataAdvancedQuery
                            {
                                combination = HitomiDataAdvancedQueryCombinationOption.Difference,
                                left_query = s2,
                                right_query = s1,
                                is_operator = true
                            });
                        }
                        break;

                    case '|':
                        {
                            var s1 = stack.Pop();
                            var s2 = stack.Pop();
                            stack.Push(new HitomiDataAdvancedQuery
                            {
                                combination = HitomiDataAdvancedQueryCombinationOption.Combination,
                                left_query = s2,
                                right_query = s1,
                                is_operator = true
                            });
                        }
                        break;

                    case '&':
                    case '+':
                        {
                            var s1 = stack.Pop();
                            var s2 = stack.Pop();
                            stack.Push(new HitomiDataAdvancedQuery
                            {
                                combination = HitomiDataAdvancedQueryCombinationOption.Default,
                                left_query = s2,
                                right_query = s1,
                                is_operator = true
                            });
                        }
                        break;

                    case '~':
                        {
                            var s = stack.Pop();
                            s.option = HitomiDataAdvancedQueryTokenOption.Complement;
                            stack.Push(s);
                        }
                        break;

                    default:
                        var query_node = new HitomiDataAdvancedQuery
                        {
                            token = token,
                            token_type = HitomiDataAdvancedQueryTokenType.Common,
                            is_operator = false
                        };
                        if (token.Contains(':'))
                        {
                            if (token_dic.ContainsKey(token.Split(':')[0]))
                                query_node.token_type = token_dic[token.Split(':')[0]];
                            else
                                query_node.token_type = HitomiDataAdvancedQueryTokenType.None;
                            query_node.token = token.Split(':')[1];
                        }
                        stack.Push(query_node);
                        break;
                }
            }

            return stack.Pop();
        }

        public static object[] to_linear(HitomiDataAdvancedQuery query)
        {
            var querys = new object[100];
            var stack = new Stack<HitomiDataAdvancedQuery>();
            var pos = new Stack<int>();
            var max = 1;

            stack.Push(query);
            pos.Push(1);

            while (stack.Count > 0)
            {
                var pop = stack.Pop();
                var ps = pos.Pop();

                max = Math.Max(max, ps);
                querys[ps] = pop;

                if (pop.left_query != null)
                {
                    stack.Push(pop.left_query);
                    pos.Push(ps * 2);
                }
                if (pop.right_query != null)
                {
                    stack.Push(pop.right_query);
                    pos.Push(ps * 2 + 1);
                }
            }

            return querys.Take(max + 1).ToArray();
        }

        public static bool match(object[] queries, HitomiIndexMetadata md)
        {
            bool[] checker = new bool[queries.Length];

            for (int i = 1; i < queries.Length; i++)
            {
                if (queries[i] is HitomiDataAdvancedQuery query)
                {
                    if (query.is_operator == false)
                    {
                        string token = query.token.Replace('_', ' ');
                        switch (query.token_type)
                        {
                            case HitomiDataAdvancedQueryTokenType.None:
                                Console.Console.Instance.WriteErrorLine("Query type not found!");
                                Console.Console.Instance.Write(Monitor.SerializeObject(query));
                                throw new Exception($"Query system error!");

                            case HitomiDataAdvancedQueryTokenType.Common:
                                if (md.Artists != null)
                                    checker[i] = md.Artists.Any(x => HitomiIndex.Instance.index.Artists[x].Contains(token));
                                if (md.Name != null)
                                    checker[i] = md.Name.ToLower().Contains(token);
                                if (md.Groups != null)
                                    checker[i] = md.Groups.Any(x => HitomiIndex.Instance.index.Groups[x].Contains(token));
                                if (md.Parodies != null)
                                    checker[i] = md.Parodies.Any(x => HitomiIndex.Instance.index.Series[x].Contains(token));
                                if (md.Characters != null)
                                    checker[i] = md.Characters.Any(x => HitomiIndex.Instance.index.Characters[x].Contains(token));
                                if (md.Language >= 0 && HitomiIndex.Instance.index.Languages[md.Language] == token)
                                    checker[i] = true;
                                if (md.Type >= 0 && HitomiIndex.Instance.index.Types[md.Type] == token)
                                    checker[i] = true;
                                if (md.ID.ToString() == token)
                                    checker[i] = true;
                                if (md.Tags != null)
                                    checker[i] = md.Tags.Any(x => HitomiIndex.Instance.index.Tags[x].Contains(token));
                                break;

                            case HitomiDataAdvancedQueryTokenType.Artists:
                                if (md.Artists != null)
                                    checker[i] = md.Artists.Select(x => HitomiIndex.Instance.index.Artists[x]).Contains(token);
                                break;

                            case HitomiDataAdvancedQueryTokenType.Title:
                                if (md.Name != null)
                                    checker[i] = token.ToLower().Contains(token);
                                break;

                            case HitomiDataAdvancedQueryTokenType.Groups:
                                if (md.Groups != null)
                                    checker[i] = md.Groups.Select(x => HitomiIndex.Instance.index.Groups[x]).Contains(token);
                                break;

                            case HitomiDataAdvancedQueryTokenType.Series:
                                if (md.Parodies != null)
                                    checker[i] = md.Parodies.Select(x => HitomiIndex.Instance.index.Series[x]).Contains(token);
                                break;

                            case HitomiDataAdvancedQueryTokenType.Characters:
                                if (md.Characters != null)
                                    checker[i] = md.Characters.Select(x => HitomiIndex.Instance.index.Characters[x]).Contains(token);
                                break;

                            case HitomiDataAdvancedQueryTokenType.Type:
                                if (md.Type >= 0 && HitomiIndex.Instance.index.Types[md.Type] == token)
                                    checker[i] = true;
                                break;

                            case HitomiDataAdvancedQueryTokenType.Language:
                                var lang = "n/a"; //md.Language;
                                if (md.Language >= 0)
                                    lang = HitomiIndex.Instance.index.Languages[md.Language];
                                if (lang == token)
                                    checker[i] = true;
                                break;

                            case HitomiDataAdvancedQueryTokenType.Id:
                                if (md.ID.ToString() == token)
                                    checker[i] = true;
                                break;

                            case HitomiDataAdvancedQueryTokenType.Tag:
                                if (md.Tags != null)
                                    checker[i] = md.Tags.Select(x => HitomiIndex.Instance.index.Tags[x]).Contains(token);
                                break;

                            case HitomiDataAdvancedQueryTokenType.FemaleTag:
                                if (md.Tags != null)
                                    checker[i] = md.Tags.Select(x => HitomiIndex.Instance.index.Tags[x]).Contains("female:" + token);
                                break;

                            case HitomiDataAdvancedQueryTokenType.MaleTag:
                                if (md.Tags != null)
                                    checker[i] = md.Tags.Select(x => HitomiIndex.Instance.index.Tags[x]).Contains("male:" + token);
                                break;
                        }
                        if (query.option == HitomiDataAdvancedQueryTokenOption.Complement)
                            checker[i] = !checker[i];
                    }
                }
            }
            
            for (int i = queries.Length - 1; i >= 0; i--)
            {
                if (queries[i] is HitomiDataAdvancedQuery query)
                {
                    if (query.is_operator == true)
                    {
                        int s1 = i * 2;
                        int s2 = i * 2 + 1;

                        var qop = queries[i] as HitomiDataAdvancedQuery;

                        if (qop.combination == HitomiDataAdvancedQueryCombinationOption.Default)
                            checker[i] = checker[s1] && checker[s2];
                        else if (qop.combination == HitomiDataAdvancedQueryCombinationOption.Combination)
                            checker[i] = checker[s1] || checker[s2];
                        else if (qop.combination == HitomiDataAdvancedQueryCombinationOption.Difference)
                            checker[i] = checker[s1] && !checker[s2];

                        if (qop.option == HitomiDataAdvancedQueryTokenOption.Complement)
                            checker[i] = !checker[i];
                    }
                }
            }

            return checker[1];
        }

        public static object[] query = null;
        public static async Task<List<HitomiIndexMetadata>> Search(string query_string)
        {
            Stopwatch sw = Stopwatch.StartNew();

            int number = Environment.ProcessorCount;
            int term = HitomiIndex.Instance.metadata_collection.Count / number;

            if (Settings.Instance.Hitomi.UsingSettingLanguageWhenAdvanceSearch)
                query_string = "lang:" + Settings.Instance.Hitomi.Language + " " + query_string;

            query = to_linear(make_tree(query_string));
            Monitor.Instance.Push("[AdvancedQuery HitomiMetadata] " + query_string);
            Monitor.Instance.Push(query[1]);

            List<Task<List<HitomiIndexMetadata>>> arr_task = new List<Task<List<HitomiIndexMetadata>>>();
            for (int i = 0; i < number; i++)
            {
                int k = i;
                if (k != number - 1)
                    arr_task.Add(new Task<List<HitomiIndexMetadata>>(() => search_internal(k * term, k * term + term)));
                else
                    arr_task.Add(new Task<List<HitomiIndexMetadata>>(() => search_internal(k * term, HitomiIndex.Instance.metadata_collection.Count)));
            }

            Parallel.ForEach(arr_task, task => task.Start());
            await Task.WhenAll(arr_task);

            List<HitomiIndexMetadata> result = new List<HitomiIndexMetadata>();
            for (int i = 0; i < number; i++)
            {
                result.AddRange(arr_task[i].Result);
            }
            result.Sort((a, b) => b.ID - a.ID);

            var end = sw.ElapsedMilliseconds;
            sw.Stop();
            Monitor.Instance.Push($"[Query Results] {result.Count.ToString("#,#")} Articles ({end.ToString("#,#")} ms)");

            return result;
        }

        private static List<HitomiIndexMetadata> search_internal(int starts, int ends)
        {
            List<HitomiIndexMetadata> result = new List<HitomiIndexMetadata>();
            for (int i = starts; i < ends; i++)
            {
                var v = HitomiIndex.Instance.metadata_collection[i];
                if (Settings.Instance.Hitomi.ExclusiveTag != null && v.Tags != null)
                {
                    int intersec_count = 0;
                    foreach (var tag in Settings.Instance.Hitomi.ExclusiveTag)
                    {
                        if (v.Tags.Any(vtag => HitomiIndex.Instance.index.Tags[vtag].ToLower().Replace(' ', '_') == tag.ToLower()))
                        {
                            intersec_count++;
                        }
                        if (intersec_count > 0) break;
                    }
                    if (intersec_count > 0) continue;
                }
                if (match(query, v))
                    result.Add(v);
            }
            result.Sort((a, b) => b.ID - a.ID);
            return result;
        }
    }
}
