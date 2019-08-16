/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Crypto;
using Koromo_Copy.LP.Code;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koromo_Copy.LP
{
    #region Parser Production

    public class ParserAction
    {
        public Action<ParsingTree.ParsingTreeNode> SemanticAction;
        public static ParserAction Create(Action<ParsingTree.ParsingTreeNode> action)
            => new ParserAction { SemanticAction = action };
        public Action<LPModule, LPFunction, LPBasicBlock, ParsingTree.ParsingTreeNode> TravelAction;
        public static ParserAction Create(Action<LPModule, LPFunction, LPBasicBlock, ParsingTree.ParsingTreeNode> action)
            => new ParserAction { TravelAction = action };
    }

    public class ParserProduction
    {
        public int index;
        public string production_name;
        public bool isterminal;
        public List<ParserProduction> contents = new List<ParserProduction>();
        public List<List<ParserProduction>> sub_productions = new List<List<ParserProduction>>();
        public List<ParserAction> temp_actions = new List<ParserAction>();
        public List<ParserAction> actions = new List<ParserAction>();
        ParserGenerator parent;

        public ParserProduction(ParserGenerator parent) { this.parent = parent; }

        public static ParserProduction operator +(ParserProduction p1, ParserProduction p2)
        {
            p1.contents.Add(p2);
            return p1;
        }

        public static ParserProduction operator +(ParserProduction pp, ParserAction ac)
        {
            pp.temp_actions.Add(ac);
            return pp;
        }

        public static ParserProduction operator +(ParserProduction pp, string name)
        {
            pp.contents.Add(pp.parent.TryCreateNewProduction(name));
            return pp;
        }

        public static ParserProduction operator +(string name, ParserProduction pp)
        {
            var p = pp.parent.TryCreateNewProduction(name);
            p.contents.Add(pp);
            return p;
        }

        public static ParserProduction operator |(ParserProduction p1, ParserProduction p2)
        {
            p2.contents.Insert(0, p2);
            p1.sub_productions.Add(new List<ParserProduction>(p2.contents));
            p1.actions.AddRange(p2.temp_actions);
            p2.temp_actions.Clear();
            p2.contents.Clear();
            return p1;
        }

        public static ParserProduction operator |(ParserProduction p1, string pt2)
        {
            var p2 = p1.parent.TryCreateNewProduction(pt2);
            p2.contents.Insert(0, p2);
            p1.sub_productions.Add(new List<ParserProduction>(p2.contents));
            p1.actions.AddRange(p2.temp_actions);
            p2.temp_actions.Clear();
            p2.contents.Clear();
            return p1;
        }

#if false
        public static ParserProduction operator +(ParserProduction p1, string p2)
        {
            p1.contents.Add(new ParserProduction { isterminal = true, token_specific = p2 });
            return p1;
        }

        public static ParserProduction operator|(ParserProduction p1, string p2)
        {
            p1.sub_productions.Add(new List<ParserProduction> { p1, new ParserProduction { isterminal = true, token_specific = p2 } });
            return p1;
        }
#endif
    }

    #endregion

    /// <summary>
    /// LR Parser Generator
    /// </summary>
    public class ParserGenerator
    {
        List<ParserProduction> production_rules;
        Dictionary<string, ParserProduction> production_dict;
        // (production_index, (priority, is_left_associativity?))
        Dictionary<int, Tuple<int, bool>> shift_reduce_conflict_solve;
        // (production_index, (sub_production_index, (priority, is_left_associativity?)))
        Dictionary<int, Dictionary<int, Tuple<int, bool>>> shift_reduce_conflict_solve_with_production_rule;

        public StringBuilder GlobalPrinter = new StringBuilder();

        public readonly static ParserProduction EmptyString = new ParserProduction(null) { index = -2 };

        public ParserGenerator()
        {
            production_rules = new List<ParserProduction>();
            production_rules.Add(new ParserProduction(this) { index = 0, production_name = "S'" });
            production_dict = new Dictionary<string, ParserProduction>();
            production_dict.Add("S'", production_rules[0]);
            shift_reduce_conflict_solve = new Dictionary<int, Tuple<int, bool>>();
            shift_reduce_conflict_solve_with_production_rule = new Dictionary<int, Dictionary<int, Tuple<int, bool>>>();
        }

        #region Parser Generating Helper

        public ParserProduction CreateNewProduction(string name = "", bool is_terminal = true)
        {
            var pp = new ParserProduction(this) { index = production_rules.Count, production_name = name, isterminal = is_terminal };
            if (production_dict.ContainsKey(name))
                throw new Exception(name + " is already exsits production!");
            production_dict.Add(name, pp);
            production_rules.Add(pp);
            return pp;
        }

        public ParserProduction TryCreateNewProduction(string name = "", bool is_terminal = true)
        {
            if (production_dict.ContainsKey(name))
                return production_dict[name];
            var pp = new ParserProduction(this) { index = production_rules.Count, production_name = name, isterminal = is_terminal };
            production_dict.Add(name, pp);
            production_rules.Add(pp);
            return pp;
        }

        /// <summary>
        /// A -> B (C D)*
        /// =>
        /// A -> B CD'
        /// CD' -> C D CD'1  { S.add(CD'1) }
        /// CD' -> e
        /// </summary>
        /// <param name="pp"></param>
        /// <returns></returns>
        public ParserProduction TryCreateZeroOrMoreProduction(ParserProduction pp)
        {
            pp.contents.Insert(0, pp);
            var pp_name = ("zeroormore" + string.Join(",", pp.contents.Select(x => $"{x.index}:{x.production_name}"))).GetHashSHA1();
            if (production_dict.ContainsKey(pp_name))
            {
                pp.contents.Clear();
                return production_dict[pp_name];
            }
            var nt = CreateNewProduction(pp_name);
            var nn = new List<ParserProduction>(pp.contents);
            nn.Add(nt);
            nt.sub_productions.Add(nn);
            nt.sub_productions.Add(new List<ParserProduction> { EmptyString });
            nt.actions.Add(ParserAction.Create((x) =>
            {
                var lx = new List<List<ParsingTree.ParsingTreeNode>>();
                var ll = new List<ParsingTree.ParsingTreeNode>();
                if (x.Childs.Count > 0)
                {
                    for (int i = 0; i < x.Childs.Count - 1; i++)
                        ll.Add(x.Childs[i]);
                    lx.Add(ll);
                }
                if (x.Childs.Last().UserContents != null)
                    lx.AddRange(x.Childs.Last().UserContents as List<List<ParsingTree.ParsingTreeNode>>);
                x.UserContents = lx;
            }));
            pp.contents.Clear();
            return nt;
        }

        /// <summary>
        /// A -> B (C D)+
        /// =>
        /// A -> B CD'
        /// CD' -> C D CD'1  { S.add(CD'1) }
        /// CD' -> C D { S.add({C D}) }
        /// </summary>
        /// <param name="pp"></param>
        /// <returns></returns>
        public ParserProduction TryCreateOneOrMoreProduction(ParserProduction pp)
        {
            pp.contents.Insert(0, pp);
            var pp_name = ("oneormore" + string.Join(",", pp.contents.Select(x => $"{x.index}:{x.production_name}"))).GetHashSHA1();
            if (production_dict.ContainsKey(pp_name))
            {
                pp.contents.Clear();
                return production_dict[pp_name];
            }
            var nt = CreateNewProduction(pp_name);
            var nn = new List<ParserProduction>(pp.contents);
            nn.Add(nt);
            nt.sub_productions.Add(nn);
            nt.sub_productions.Add(new List<ParserProduction>(pp.contents));
            nt.actions.Add(ParserAction.Create((x) =>
            {
                var lx = new List<List<ParsingTree.ParsingTreeNode>>();
                var ll = new List<ParsingTree.ParsingTreeNode>();
                for (int i = 0; i < x.Childs.Count - 1; i++)
                    ll.Add(x.Childs[i]);
                lx.Add(ll);
                if (x.Childs.Last().UserContents != null)
                    lx.AddRange(x.Childs.Last().UserContents as List<List<ParsingTree.ParsingTreeNode>>);
                x.UserContents = lx;
            }));
            nt.actions.Add(ParserAction.Create((x) =>
            {
                var lx = new List<List<ParsingTree.ParsingTreeNode>>();
                lx.Add(x.Childs);
                x.UserContents = lx;
            }));
            pp.contents.Clear();
            return nt;
        }

        /// <summary>
        /// A -> B (C D)?
        /// =>
        /// A -> B CD'
        /// CD' -> C D
        /// CD' -> e
        /// </summary>
        /// <param name="pp"></param>
        /// <returns></returns>
        public ParserProduction TryCreateZeroOrOneProduction(ParserProduction pp)
        {
            pp.contents.Insert(0, pp);
            var pp_name = ("zeroorone" + string.Join(",", pp.contents.Select(x => $"{x.index}:{x.production_name}"))).GetHashSHA1();
            if (production_dict.ContainsKey(pp_name))
            {
                pp.contents.Clear();
                return production_dict[pp_name];
            }
            var nt = CreateNewProduction(pp_name);
            nt.sub_productions.Add(new List<ParserProduction>(pp.contents));
            nt.sub_productions.Add(new List<ParserProduction> { EmptyString });
            pp.contents.Clear();
            return nt;
        }

        public void PushStarts(ParserProduction pp)
        {
            // Augment stats node
            production_rules[0].sub_productions.Add(new List<ParserProduction> { pp });
        }

        /// <summary>
        /// 터미널들의 Shift-Reduce Conflict solve 정보를 넣습니다.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="terminals"></param>
        public void PushConflictSolver(bool left, params ParserProduction[] terminals)
        {
            var priority = shift_reduce_conflict_solve.Count + shift_reduce_conflict_solve_with_production_rule.Count;
            foreach (var pp in terminals)
                shift_reduce_conflict_solve.Add(pp.index, new Tuple<int, bool>(priority, left));
        }

        /// <summary>
        /// 논터미널들의 Shift-Reduce Conflict solve 정보를 넣습니다.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="no"></param>
        public void PushConflictSolver(bool left, params Tuple<ParserProduction, int>[] no)
        {
            var priority = shift_reduce_conflict_solve.Count + shift_reduce_conflict_solve_with_production_rule.Count;
            foreach (var ppi in no)
            {
                if (!shift_reduce_conflict_solve_with_production_rule.ContainsKey(ppi.Item1.index))
                    shift_reduce_conflict_solve_with_production_rule.Add(ppi.Item1.index, new Dictionary<int, Tuple<int, bool>>());
                shift_reduce_conflict_solve_with_production_rule[ppi.Item1.index].Add(ppi.Item2, new Tuple<int, bool>(priority, left));
            }
        }

        #endregion

        #region Simple ParserDescription Parser

        /// <summary>
        /// 문자열로부터 ParserGenerator를 가져옵니다.
        /// </summary>
        /// <param name="nt_syms">논터미널 심볼</param>
        /// <param name="t_syms">터미널 심볼</param>
        /// <param name="production_rules">프로덕션 룰</param>
        /// <param name="sr_rules">Shift Reduce 규칙</param>
        /// <returns></returns>
        public static ParserGenerator GetGenerator(string[] nt_syms, Tuple<string, string>[] t_syms, string[] production_rules, string[] sr_rules)
        {
            var gen = new ParserGenerator();
            var non_terminals = new Dictionary<string, ParserProduction>();
            var terminals = new Dictionary<string, ParserProduction>();

            terminals.Add("''", EmptyString);

            foreach (var nt in nt_syms)
                non_terminals.Add(nt.Trim(), gen.CreateNewProduction(nt.Trim(), false));

            foreach (var t in t_syms)
            {
                var name = t.Item1;
                var pp = t.Item2;

                terminals.Add(pp, gen.CreateNewProduction(name.Trim()));
            }

            var prec = new Dictionary<string, List<Tuple<ParserProduction, int>>>();
            foreach (var pp in production_rules)
            {
                if (pp.Trim() == "") continue;

                var split = pp.Split(new[] { "->" }, StringSplitOptions.None);
                var left = split[0].Trim();
                var right = split[1].Split(' ');

                var prlist = new List<ParserProduction>();
                bool stay_prec = false;
                foreach (var ntt in right)
                {
                    if (string.IsNullOrEmpty(ntt)) continue;
                    if (ntt == "%prec") { stay_prec = true; continue; }
                    if (stay_prec)
                    {
                        if (!prec.ContainsKey(ntt))
                            prec.Add(ntt, new List<Tuple<ParserProduction, int>>());
                        prec[ntt].Add(new Tuple<ParserProduction, int>(non_terminals[left], non_terminals[left].sub_productions.Count));
                        continue;
                    }
                    if (non_terminals.ContainsKey(ntt))
                        prlist.Add(non_terminals[ntt]);
                    else if (terminals.ContainsKey(ntt))
                        prlist.Add(terminals[ntt]);
                    else
                        throw new Exception($"Production rule build error!\r\n{ntt} is neither non-terminal nor terminal!\r\nDeclare the token-name!");
                }
                non_terminals[left].sub_productions.Add(prlist);
            }

            for (int i = sr_rules.Length - 1; i >= 0; i--)
            {
                var line = sr_rules[i].Trim();
                if (line == "") continue;
                var tt = line.Split(' ')[0];
                var rr = line.Substring(tt.Length).Trim().Split(',');

                var left = true;
                var items1 = new List<Tuple<ParserProduction, int>>();
                var items2 = new List<ParserProduction>();

                if (tt == "%right") left = false;

                foreach (var ii in rr.Select(x => x.Trim()))
                {
                    if (string.IsNullOrEmpty(ii)) continue;
                    if (terminals.ContainsKey(ii))
                        items2.Add(terminals[ii]);
                    else if (prec.ContainsKey(ii))
                        items1.AddRange(prec[ii]);
                    else
                        throw new Exception($"Production rule build error!\r\n{ii} is neither non-terminal nor terminal!\r\nDeclare the token-name!");
                }

                if (items1.Count > 0)
                    gen.PushConflictSolver(left, items1.ToArray());
                else
                    gen.PushConflictSolver(left, items2.ToArray());
            }

            gen.PushStarts(non_terminals[nt_syms[0]]);

            return gen;
        }

        #endregion

        #region String Hash Function
        // 원래 해시가 아니라 set로 구현해야하는게 일반적임
        // 집합끼리의 비교연산, 일치여부 교집합을 구해 좀 더 최적화가능하지만 귀찮으니 string-hash를 쓰도록한다.
        //
        // # 2019-07-15
        // 확인결과 별도의 클래스를 만들어 set를 관리하는 것보다 string-hash가 더 빠르다
        // JSParserGenetor의 경우 set의 경우 13초, string-hash의 경우 11초로 string-hash가 더 빠른 속도를 내었다.
        // set은 dictionary에서사용하는 GetHashCode 및 Equals 함수와, state의 kernel을 고려하여 만든 클래스였다.

        private string t2s(Tuple<int, int, int> t)
        {
            return $"{t.Item1},{t.Item2},{t.Item3}";
        }

        private string t2s(Tuple<int, int, int, HashSet<int>> t)
        {
            var list = t.Item4.ToList();
            list.Sort();
            return $"{t.Item1},{t.Item2},{t.Item3},({string.Join(",", list)})";
        }

        private string l2s(List<Tuple<int, int, int>> h)
        {
            var list = h.ToList();
            list.Sort();
            return string.Join(",", list.Select(x => $"({x.Item1},{x.Item2},{x.Item3})"));
        }

        private string l2s(List<Tuple<int, int, int, HashSet<int>>> h)
        {
            var list = new List<Tuple<int, int, int, List<int>>>();
            foreach (var tt in h)
            {
                var ll = tt.Item4.ToList();
                ll.Sort();
                list.Add(new Tuple<int, int, int, List<int>>(tt.Item1, tt.Item2, tt.Item3, ll));
            }
            list.Sort();
            return string.Join(",", list.Select(x => $"({x.Item1},{x.Item2},{x.Item3},({(string.Join("/", x.Item4))}))"));
        }

        private static string l2sl(List<Tuple<int, int, int, HashSet<int>>> h, int kernel_cnt)
        {
            var list = new List<Tuple<int, int, int>>();
            var builder = new StringBuilder();
            for (int i = 0; i < kernel_cnt; i++)
                list.Add(new Tuple<int, int, int>(h[i].Item1, h[i].Item2, h[i].Item3));
            list.Sort();
            return string.Join(",", list.Select(x => $"({x.Item1},{x.Item2},{x.Item3})"));
        }

        private string i2s(int a, int b, int c)
        {
            return $"{a},{b},{c}";
        }
        #endregion

        #region Debug Printer

        private void print_hs(List<HashSet<int>> lhs, string prefix)
        {
            for (int i = 0; i < lhs.Count; i++)
                if (lhs[i].Count > 0)
                    GlobalPrinter.Append(
                        $"{prefix}({production_rules[i].production_name})={{{string.Join(",", lhs[i].ToList().Select(x => x == -1 ? "$" : production_rules[x].production_name))}}}\r\n");
        }

        private void print_header(string head_text)
        {
            GlobalPrinter.Append("\r\n" + new string('=', 50) + "\r\n\r\n");
            int spaces = 50 - head_text.Length;
            int padLeft = spaces / 2 + head_text.Length;
            GlobalPrinter.Append(head_text.PadLeft(padLeft).PadRight(50));
            GlobalPrinter.Append("\r\n\r\n" + new string('=', 50) + "\r\n");
        }

        private void print_states(int state, List<Tuple<int, int, int, HashSet<int>>> items)
        {
            var builder = new StringBuilder();
            builder.Append("-----" + "I" + state + "-----\r\n");

            foreach (var item in items)
            {
                builder.Append($"{production_rules[item.Item1].production_name.ToString().PadLeft(10)} -> ");

                var builder2 = new StringBuilder();
                for (int i = 0; i < production_rules[item.Item1].sub_productions[item.Item2].Count; i++)
                {
                    if (i == item.Item3)
                        builder2.Append("·");
                    builder2.Append(production_rules[item.Item1].sub_productions[item.Item2][i].production_name + " ");
                    if (item.Item3 == production_rules[item.Item1].sub_productions[item.Item2].Count && i == item.Item3 - 1)
                        builder2.Append("·");
                }
                builder.Append(builder2.ToString().PadRight(30));

                builder.Append($"{string.Join("/", item.Item4.ToList().Select(x => x == -1 ? "$" : production_rules[x].production_name))}\r\n");
            }

            GlobalPrinter.Append(builder.ToString());
        }

        private void print_merged_states(int state, List<Tuple<int, int, int, HashSet<int>>> items, List<List<List<int>>> external_gotos)
        {
            var builder = new StringBuilder();
            builder.Append("-----" + "I" + state + "-----\r\n");

            for (int j = 0; j < items.Count; j++)
            {
                var item = items[j];

                builder.Append($"{production_rules[item.Item1].production_name.ToString().PadLeft(10)} -> ");

                var builder2 = new StringBuilder();
                for (int i = 0; i < production_rules[item.Item1].sub_productions[item.Item2].Count; i++)
                {
                    if (i == item.Item3)
                        builder2.Append("·");
                    builder2.Append(production_rules[item.Item1].sub_productions[item.Item2][i].production_name + " ");
                    if (item.Item3 == production_rules[item.Item1].sub_productions[item.Item2].Count && i == item.Item3 - 1)
                        builder2.Append("·");
                }
                builder.Append(builder2.ToString().PadRight(30));

                builder.Append($"[{string.Join("/", item.Item4.ToList().Select(x => x == -1 ? "$" : production_rules[x].production_name))}] ");
                for (int i = 0; i < external_gotos.Count; i++)
                    builder.Append($"[{string.Join("/", external_gotos[i][j].ToList().Select(x => x == -1 ? "$" : production_rules[x].production_name))}] ");
                builder.Append("\r\n");
            }

            GlobalPrinter.Append(builder.ToString());
        }

        #endregion

        int number_of_states = -1;
        Dictionary<int, List<Tuple<int, int>>> shift_info;
        Dictionary<int, List<Tuple<int, int, int>>> reduce_info;

        #region SLR Generator
        /// <summary>
        /// Generate SimpleLR Table
        /// </summary>
        public void Generate()
        {
            // --------------- Expand EmptyString ---------------
            for (int i = 0; i < production_rules.Count; i++)
                if (!production_rules[i].isterminal)
                    for (int j = 0; j < production_rules[i].sub_productions.Count; j++)
                        if (production_rules[i].sub_productions[j][0].index == EmptyString.index)
                        {
                            production_rules[i].sub_productions.RemoveAt(j--);
                            for (int ii = 0; ii < production_rules.Count; ii++)
                                if (!production_rules[ii].isterminal)
                                    for (int jj = 0; jj < production_rules[ii].sub_productions.Count; jj++)
                                        for (int kk = 0; kk < production_rules[ii].sub_productions[jj].Count; kk++)
                                            if (production_rules[ii].sub_productions[jj][kk].index == production_rules[i].index)
                                            {
                                                var ll = new List<ParserProduction>(production_rules[ii].sub_productions[jj]);
                                                ll.RemoveAt(kk);
                                                production_rules[ii].sub_productions.Add(ll);
                                            }
                        }
            // --------------------------------------------------

            // --------------- Collect FIRST,FOLLOW Set ---------------
            var FIRST = new List<HashSet<int>>();
            foreach (var rule in production_rules)
                FIRST.Add(first_terminals(rule.index));

            var FOLLOW = follow_terminals(FIRST);

#if true
            print_header("FISRT, FOLLOW SETS");
            print_hs(FIRST, "FIRST");
            print_hs(FOLLOW, "FOLLOW");
#endif
            // --------------------------------------------------------

            // (state_index, (production_rule_index, sub_productions_pos, dot_position)
            var states = new Dictionary<int, List<Tuple<int, int, int>>>();
            // (state_specify, state_index)
            var state_index = new Dictionary<string, int>();
            // (state_index, (reduce_what, state_index))
            shift_info = new Dictionary<int, List<Tuple<int, int>>>();
            // (state_index, (shift_what, production_rule_index, sub_productions_pos))
            reduce_info = new Dictionary<int, List<Tuple<int, int, int>>>();
            var index_count = 0;

            // -------------------- Put first eater -------------------
            var first_l = first_nonterminals(0);
            state_index.Add(l2s(first_l), 0);
            states.Add(0, first_l);
            // --------------------------------------------------------

            // Create all LR states
            // (states)
            var q = new Queue<int>();
            q.Enqueue(index_count++);
            while (q.Count != 0)
            {
                var p = q.Dequeue();

                // Collect goto
                // (state_index, (production_rule_index, sub_productions_pos, dot_position))
                var gotos = new Dictionary<int, List<Tuple<int, int, int>>>();
                foreach (var transition in states[p])
                    if (production_rules[transition.Item1].sub_productions[transition.Item2].Count > transition.Item3)
                    {
                        var pi = production_rules[transition.Item1].sub_productions[transition.Item2][transition.Item3].index;
                        if (!gotos.ContainsKey(pi))
                            gotos.Add(pi, new List<Tuple<int, int, int>>());
                        gotos[pi].Add(new Tuple<int, int, int>(transition.Item1, transition.Item2, transition.Item3 + 1));
                    }

                // Populate empty-string closure
                foreach (var goto_unit in gotos)
                {
                    var set = new HashSet<string>();
                    // Push exists transitions
                    foreach (var psd in goto_unit.Value)
                        set.Add(t2s(psd));
                    // Find all transitions
                    var new_trans = new List<Tuple<int, int, int>>();
                    foreach (var psd in goto_unit.Value)
                    {
                        if (production_rules[psd.Item1].sub_productions[psd.Item2].Count == psd.Item3) continue;
                        if (production_rules[psd.Item1].sub_productions[psd.Item2][psd.Item3].isterminal) continue;
                        var first_nt = first_nonterminals(production_rules[psd.Item1].sub_productions[psd.Item2][psd.Item3].index);
                        foreach (var nts in first_nt)
                            if (!set.Contains(t2s(nts)))
                            {
                                new_trans.Add(nts);
                                set.Add(t2s(nts));
                            }
                    }
                    goto_unit.Value.AddRange(new_trans);
                }

                // Build shift transitions ignore terminal, non-terminal
                foreach (var pp in gotos)
                {
                    var hash = l2s(pp.Value);
                    if (!state_index.ContainsKey(hash))
                    {
                        states.Add(index_count, pp.Value);
                        state_index.Add(hash, index_count);
                        q.Enqueue(index_count++);
                    }
                    var index = state_index[hash];

                    if (!shift_info.ContainsKey(p))
                        shift_info.Add(p, new List<Tuple<int, int>>());
                    shift_info[p].Add(new Tuple<int, int>(pp.Key, index));
                }

                // Check require reduce and build reduce transitions
                foreach (var transition in states[p])
                    if (production_rules[transition.Item1].sub_productions[transition.Item2].Count == transition.Item3)
                    {
                        if (!reduce_info.ContainsKey(p))
                            reduce_info.Add(p, new List<Tuple<int, int, int>>());
                        foreach (var term in FOLLOW[transition.Item1])
                            reduce_info[p].Add(new Tuple<int, int, int>(term, transition.Item1, transition.Item2));
                    }
            }

            number_of_states = states.Count;
        }
        #endregion

        #region LR(1) Generator
        /// <summary>
        /// Generate LR(1) Table
        /// 
        /// There is a lookahead propagation error while trying to get the LR(1) item, and it is under review.
        /// Don't use this function
        /// </summary>
        public void GenerateLR1()
        {
            // --------------- Delete EmptyString ---------------
            for (int i = 0; i < production_rules.Count; i++)
                if (!production_rules[i].isterminal)
                    for (int j = 0; j < production_rules[i].sub_productions.Count; j++)
                        if (production_rules[i].sub_productions[j][0].index == EmptyString.index)
                            production_rules[i].sub_productions[j].Clear();
            // --------------------------------------------------

            // --------------- Collect FIRST,FOLLOW Set ---------------
            var FIRST = new List<HashSet<int>>();
            foreach (var rule in production_rules)
                FIRST.Add(first_terminals(rule.index));

            var FOLLOW = follow_terminals(FIRST);

#if true
            print_header("FISRT, FOLLOW SETS");
            print_hs(FIRST, "FIRST");
            print_hs(FOLLOW, "FOLLOW");
#endif
            // --------------------------------------------------------

            // (state_index, (production_rule_index, sub_productions_pos, dot_position, (lookahead))
            var states = new Dictionary<int, List<Tuple<int, int, int, HashSet<int>>>>();
            // (state_specify, state_index)
            var state_index = new Dictionary<string, int>();
            var goto_table = new List<Tuple<int, List<Tuple<int, int>>>>();
            // (state_index, (reduce_what, state_index))
            shift_info = new Dictionary<int, List<Tuple<int, int>>>();
            // (state_index, (shift_what, production_rule_index, sub_productions_pos))
            reduce_info = new Dictionary<int, List<Tuple<int, int, int>>>();
            var index_count = 0;

            // -------------------- Put first eater -------------------
            var first_l = first_with_lookahead(0, 0, 0, new HashSet<int>());
            state_index.Add(l2s(first_l), 0);
            states.Add(0, first_l);
            // --------------------------------------------------------

            // Create all LR states
            // (states)
            var q = new Queue<int>();
            q.Enqueue(index_count++);
            while (q.Count != 0)
            {
                var p = q.Dequeue();

                // Collect goto
                // (state_index, (production_rule_index, sub_productions_pos, dot_position, lookahead))
                var gotos = new Dictionary<int, List<Tuple<int, int, int, HashSet<int>>>>();
                foreach (var transition in states[p])
                    if (production_rules[transition.Item1].sub_productions[transition.Item2].Count > transition.Item3)
                    {
                        var pi = production_rules[transition.Item1].sub_productions[transition.Item2][transition.Item3].index;
                        if (!gotos.ContainsKey(pi))
                            gotos.Add(pi, new List<Tuple<int, int, int, HashSet<int>>>());
                        gotos[pi].Add(new Tuple<int, int, int, HashSet<int>>(transition.Item1, transition.Item2, transition.Item3 + 1, transition.Item4));
                    }

                // Populate empty-string closure
                foreach (var goto_unit in gotos)
                {
                    var set = new HashSet<string>();
                    // Push exists transitions
                    foreach (var psd in goto_unit.Value)
                        set.Add(t2s(psd));
                    // Find all transitions
                    var new_trans = new List<Tuple<int, int, int, HashSet<int>>>();
                    var trans_dic = new Dictionary<string, int>();
                    foreach (var psd in goto_unit.Value)
                    {
                        if (production_rules[psd.Item1].sub_productions[psd.Item2].Count == psd.Item3) continue;
                        if (production_rules[psd.Item1].sub_productions[psd.Item2][psd.Item3].isterminal) continue;
                        var first_nt = first_with_lookahead(psd.Item1, psd.Item2, psd.Item3, psd.Item4);
                        foreach (var nts in first_nt)
                            if (!set.Contains(t2s(nts)))
                            {
                                var ts = t2s(new Tuple<int, int, int>(nts.Item1, nts.Item2, nts.Item3));
                                if (trans_dic.ContainsKey(ts))
                                {
                                    nts.Item4.ToList().ForEach(x => new_trans[trans_dic[ts]].Item4.Add(x));
                                }
                                else
                                {
                                    trans_dic.Add(ts, new_trans.Count);
                                    new_trans.Add(nts);
                                    set.Add(t2s(nts));
                                }
                            }
                    }
                    goto_unit.Value.AddRange(new_trans);
                }

                //// Build shift transitions ignore terminal, non-terminal
                //foreach (var pp in gotos)
                //{
                //    var hash = l2s(pp.Value);
                //    if (!state_index.ContainsKey(hash))
                //    {
                //        states.Add(index_count, pp.Value);
                //        state_index.Add(hash, index_count);
                //        q.Enqueue(index_count++);
                //    }
                //    var index = state_index[hash];
                //
                //    if (!shift_info.ContainsKey(p))
                //        shift_info.Add(p, new List<Tuple<int, int>>());
                //    shift_info[p].Add(new Tuple<int, int>(pp.Key, index));
                //}
                //
                //// Check require reduce and build reduce transitions
                //foreach (var transition in states[p])
                //    if (production_rules[transition.Item1].sub_productions[transition.Item2].Count == transition.Item3)
                //    {
                //        if (!reduce_info.ContainsKey(p))
                //            reduce_info.Add(p, new List<Tuple<int, int, int>>());
                //        foreach (var term in transition.Item4)
                //            reduce_info[p].Add(new Tuple<int, int, int>(term, transition.Item1, transition.Item2));
                //    }

                // Build goto transitions ignore terminal, non-terminal anywhere
                var index_list = new List<Tuple<int, int>>();
                foreach (var pp in gotos)
                {
                    var hash = l2s(pp.Value);
                    if (!state_index.ContainsKey(hash))
                    {
                        states.Add(index_count, pp.Value);
                        state_index.Add(hash, index_count);
                        q.Enqueue(index_count++);
                    }
                    index_list.Add(new Tuple<int, int>(pp.Key, state_index[hash]));
                }

                goto_table.Add(new Tuple<int, List<Tuple<int, int>>>(p, index_list));
            }

            var occurred_conflict = false;

            // ------------- Find Shift-Reduce Conflict ------------
            foreach (var ms in states)
            {
                // (shift_what, state_index)
                var small_shift_info = new List<Tuple<int, int>>();
                // (reduce_what, production_rule_index, sub_productions_pos)
                var small_reduce_info = new List<Tuple<int, int, int>>();

                // Fill Shift Info
                foreach (var pp in goto_table[ms.Key].Item2)
                    small_shift_info.Add(new Tuple<int, int>(pp.Item1, pp.Item2));

                // Fill Reduce Info
                foreach (var transition in ms.Value)
                    if (production_rules[transition.Item1].sub_productions[transition.Item2].Count == transition.Item3)
                    {
                        foreach (var term in transition.Item4)
                            small_reduce_info.Add(new Tuple<int, int, int>(term, transition.Item1, transition.Item2));
                    }

                // Conflict Check
                // (shift_what, small_shift_info_index)
                var shift_tokens = new Dictionary<int, int>();
                for (int i = 0; i < small_shift_info.Count; i++)
                    shift_tokens.Add(small_shift_info[i].Item1, i);
                var completes = new HashSet<int>();

                foreach (var tuple in small_reduce_info)
                {
                    if (completes.Contains(tuple.Item1))
                    {
                        // It's already added so do not have to work anymore.
                        continue;
                    }

                    if (shift_tokens.ContainsKey(tuple.Item1))
                    {
#if true
                        print_header("SHIFT-REDUCE CONFLICTS");
                        GlobalPrinter.Append($"Shift-Reduce Conflict! {(tuple.Item1 == -1 ? "$" : production_rules[tuple.Item1].production_name)}\r\n");
                        GlobalPrinter.Append($"States: {ms.Key} {small_shift_info[shift_tokens[tuple.Item1]].Item2}\r\n");
                        print_states(ms.Key, states[ms.Key]);
                        print_states(small_shift_info[shift_tokens[tuple.Item1]].Item2, states[small_shift_info[shift_tokens[tuple.Item1]].Item2]);
#endif
                        Tuple<int, bool> p1 = null, p2 = null;

                        try
                        {
                            var pp = get_first_on_right_terminal(production_rules[tuple.Item2], tuple.Item3);
                            if (shift_reduce_conflict_solve.ContainsKey(pp.index))
                                p1 = shift_reduce_conflict_solve[pp.index];
                        }
                        catch (Exception e)
                        {
                            GlobalPrinter.Append(e.Message + "\r\n");
                        }

                        if (shift_reduce_conflict_solve.ContainsKey(tuple.Item1))
                            p2 = shift_reduce_conflict_solve[tuple.Item1];

                        if (shift_reduce_conflict_solve_with_production_rule.ContainsKey(tuple.Item2))
                            if (shift_reduce_conflict_solve_with_production_rule[tuple.Item2].ContainsKey(tuple.Item3))
                                p1 = shift_reduce_conflict_solve_with_production_rule[tuple.Item2][tuple.Item3];

                        //if (shift_reduce_conflict_solve_with_production_rule.ContainsKey(states[tuple.Item1][0].Item1))
                        //    if (shift_reduce_conflict_solve_with_production_rule[states[tuple.Item1][0].Item1].ContainsKey(states[tuple.Item1][0].Item2))
                        //        p2 = shift_reduce_conflict_solve_with_production_rule[states[tuple.Item1][0].Item1][states[tuple.Item1][0].Item2];

                        if (p1 == null || p2 == null)
                        {
                            occurred_conflict = true;
                            continue;
                        }

                        if (p1.Item1 < p2.Item1 || (p1.Item1 == p2.Item1 && p1.Item2))
                        {
                            // Reduce
                            if (!reduce_info.ContainsKey(ms.Key))
                                reduce_info.Add(ms.Key, new List<Tuple<int, int, int>>());
                            reduce_info[ms.Key].Add(new Tuple<int, int, int>(tuple.Item1, tuple.Item2, tuple.Item3));
                        }
                        else
                        {
                            // Shift
                            if (!shift_info.ContainsKey(ms.Key))
                                shift_info.Add(ms.Key, new List<Tuple<int, int>>());
                            shift_info[ms.Key].Add(new Tuple<int, int>(tuple.Item1, small_shift_info[shift_tokens[tuple.Item1]].Item2));
                        }

                        completes.Add(tuple.Item1);
                    }
                    else
                    {
                        // Just add reduce item
                        if (!reduce_info.ContainsKey(ms.Key))
                            reduce_info.Add(ms.Key, new List<Tuple<int, int, int>>());
                        reduce_info[ms.Key].Add(new Tuple<int, int, int>(tuple.Item1, tuple.Item2, tuple.Item3));

                        completes.Add(tuple.Item1);
                    }
                }

                foreach (var pair in shift_tokens)
                {
                    if (completes.Contains(pair.Key)) continue;
                    var shift = small_shift_info[pair.Value];
                    if (!shift_info.ContainsKey(ms.Key))
                        shift_info.Add(ms.Key, new List<Tuple<int, int>>());
                    shift_info[ms.Key].Add(new Tuple<int, int>(shift.Item1, shift.Item2));
                }
            }
            // -----------------------------------------------------

            if (occurred_conflict)
                throw new Exception("Specify the rules to resolve Shift-Reduce Conflict!");

            number_of_states = states.Count;
#if true
            print_header("STATES INFO");
            foreach (var s in states)
                print_states(s.Key, s.Value);
#endif
        }
        #endregion

        #region LALR Generator
        /// <summary>
        /// Generate LALR Table
        /// </summary>
        public void GenerateLALR()
        {
            // --------------- Delete EmptyString ---------------
            for (int i = 0; i < production_rules.Count; i++)
                if (!production_rules[i].isterminal)
                    for (int j = 0; j < production_rules[i].sub_productions.Count; j++)
                        if (production_rules[i].sub_productions[j][0].index == EmptyString.index)
                            production_rules[i].sub_productions[j].Clear();
            // --------------------------------------------------

            // --------------- Collect FIRST,FOLLOW Set ---------------
            var FIRST = new List<HashSet<int>>();
            foreach (var rule in production_rules)
                FIRST.Add(first_terminals(rule.index));

            var FOLLOW = follow_terminals(FIRST);

#if true
            print_header("FISRT, FOLLOW SETS");
            print_hs(FIRST, "FIRST");
            print_hs(FOLLOW, "FOLLOW");
#endif
            // --------------------------------------------------------

            // (state_index, (production_rule_index, sub_productions_pos, dot_position, (lookahead))
            var states = new Dictionary<int, List<Tuple<int, int, int, HashSet<int>>>>();
            // (state_specify, state_index)
            var state_index = new Dictionary<string, int>();
            var goto_table = new List<Tuple<int, List<Tuple<int, int>>>>();
            // (state_index, (shift_what, state_index))
            shift_info = new Dictionary<int, List<Tuple<int, int>>>();
            // (state_index, (reduce_what, production_rule_index, sub_productions_pos))
            reduce_info = new Dictionary<int, List<Tuple<int, int, int>>>();
            var index_count = 0;

            // -------------------- Put first eater -------------------
            var first_l = first_with_lookahead(0, 0, 0, new HashSet<int>());
            state_index.Add(l2s(first_l), 0);
            states.Add(0, first_l);
            // --------------------------------------------------------

            // Create all LR states
            // (states)
            var q = new Queue<int>();
            q.Enqueue(index_count++);
            while (q.Count != 0)
            {
                var p = q.Dequeue();

                // Collect goto
                // (state_index, (production_rule_index, sub_productions_pos, dot_position, lookahead))
                var gotos = new Dictionary<int, List<Tuple<int, int, int, HashSet<int>>>>();
                foreach (var transition in states[p])
                    if (production_rules[transition.Item1].sub_productions[transition.Item2].Count > transition.Item3)
                    {
                        var pi = production_rules[transition.Item1].sub_productions[transition.Item2][transition.Item3].index;
                        if (!gotos.ContainsKey(pi))
                            gotos.Add(pi, new List<Tuple<int, int, int, HashSet<int>>>());
                        gotos[pi].Add(new Tuple<int, int, int, HashSet<int>>(transition.Item1, transition.Item2, transition.Item3 + 1, transition.Item4));
                    }

                // Populate empty-string closure
                foreach (var goto_unit in gotos)
                {
                    var set = new HashSet<string>();
                    // Push exists transitions
                    foreach (var psd in goto_unit.Value)
                        set.Add(t2s(psd));
                    // Find all transitions
                    var new_trans = new List<Tuple<int, int, int, HashSet<int>>>();
                    var trans_dic = new Dictionary<string, int>();
                    foreach (var psd in goto_unit.Value)
                    {
                        if (production_rules[psd.Item1].sub_productions[psd.Item2].Count == psd.Item3) continue;
                        if (production_rules[psd.Item1].sub_productions[psd.Item2][psd.Item3].isterminal) continue;
                        var first_nt = first_with_lookahead(psd.Item1, psd.Item2, psd.Item3, psd.Item4);
                        foreach (var nts in first_nt)
                            if (!set.Contains(t2s(nts)))
                            {
                                var ts = t2s(new Tuple<int, int, int>(nts.Item1, nts.Item2, nts.Item3));
                                if (trans_dic.ContainsKey(ts))
                                {
                                    nts.Item4.ToList().ForEach(x => new_trans[trans_dic[ts]].Item4.Add(x));
                                }
                                else
                                {
                                    trans_dic.Add(ts, new_trans.Count);
                                    new_trans.Add(nts);
                                    set.Add(t2s(nts));
                                }
                            }
                    }
                    goto_unit.Value.AddRange(new_trans);
                }

                // Build goto transitions ignore terminal, non-terminal anywhere
                var index_list = new List<Tuple<int, int>>();
                foreach (var pp in gotos)
                {
                    try
                    {
                        var hash = l2s(pp.Value);
                        if (!state_index.ContainsKey(hash))
                        {
                            states.Add(index_count, pp.Value);
                            state_index.Add(hash, index_count);
                            q.Enqueue(index_count++);
                        }
                        index_list.Add(new Tuple<int, int>(pp.Key, state_index[hash]));
                    }
                    catch
                    {
                        // Now this error is not hit
                        // For debugging
                        print_header("GOTO CONFLICT!!");
                        GlobalPrinter.Append($"Cannot solve lookahead overlapping!\r\n");
                        GlobalPrinter.Append($"Please uses non-associative option or adds extra token to handle with shift-reduce conflict!\r\n");
                        print_states(p, states[p]);
                        print_header("INCOMPLETE STATES");
                        foreach (var s in states)
                            print_states(s.Key, s.Value);
                        return;
                    }
                }

                goto_table.Add(new Tuple<int, List<Tuple<int, int>>>(p, index_list));
            }

#if true
            print_header("UNMERGED STATES");
            foreach (var s in states)
                print_states(s.Key, s.Value);
#endif

            // -------------------- Merge States -------------------
            var merged_states = new Dictionary<int, List<int>>();
            var merged_states_index = new Dictionary<string, int>();
            var merged_index = new Dictionary<int, int>();
            var merged_merged_index = new Dictionary<int, int>();
            var merged_merged_inverse_index = new Dictionary<string, int>();
            var count_of_completes_states = 0;

            for (int i = 0; i < states.Count; i++)
            {
                var str = l2s(states[i].Select(x => new Tuple<int, int, int>(x.Item1, x.Item2, x.Item3)).ToList());

                if (!merged_states_index.ContainsKey(str))
                {
                    merged_states_index.Add(str, i);
                    merged_states.Add(i, new List<int>());
                    merged_index.Add(i, i);
                    merged_merged_inverse_index.Add(str, count_of_completes_states);
                    merged_merged_index.Add(i, count_of_completes_states++);
                }
                else
                {
                    merged_states[merged_states_index[str]].Add(i);
                    merged_index.Add(i, merged_states_index[str]);
                    merged_merged_index.Add(i, merged_merged_inverse_index[str]);
                }
            }

#if true
            print_header("MERGED STATES WITH SOME SETS");
            foreach (var s in merged_states)
                print_merged_states(s.Key, states[s.Key], s.Value.Select(x => states[x].Select(y => y.Item4.ToList()).ToList()).ToList());
#endif

            foreach (var pair in merged_states)
            {
                for (int i = 0; i < states[pair.Key].Count; i++)
                {
                    foreach (var ii in pair.Value)
                        foreach (var lookahead in states[ii][i].Item4)
                            states[pair.Key][i].Item4.Add(lookahead);
                }
            }

#if true
            print_header("MERGED STATES");
            foreach (var s in merged_states)
                print_states(s.Key, states[s.Key]);
#endif
            // -----------------------------------------------------

            var occurred_conflict = false;

            // ------------- Find Shift-Reduce Conflict ------------
            foreach (var ms in merged_states)
            {
                // (shift_what, state_index)
                var small_shift_info = new List<Tuple<int, int>>();
                // (reduce_what, production_rule_index, sub_productions_pos)
                var small_reduce_info = new List<Tuple<int, int, int>>();

                // Fill Shift Info
                foreach (var pp in goto_table[ms.Key].Item2)
                    small_shift_info.Add(new Tuple<int, int>(pp.Item1, merged_index[pp.Item2]));

                // Fill Reduce Info
                ms.Value.Add(ms.Key);
                foreach (var index in ms.Value)
                    foreach (var transition in states[index])
                        if (production_rules[transition.Item1].sub_productions[transition.Item2].Count == transition.Item3)
                        {
                            foreach (var term in transition.Item4)
                                small_reduce_info.Add(new Tuple<int, int, int>(term, transition.Item1, transition.Item2));
                        }

                // Conflict Check
                // (shift_what, small_shift_info_index)
                var shift_tokens = new Dictionary<int, int>();
                for (int i = 0; i < small_shift_info.Count; i++)
                    shift_tokens.Add(small_shift_info[i].Item1, i);
                var completes = new HashSet<int>();

                foreach (var tuple in small_reduce_info)
                {
                    if (completes.Contains(tuple.Item1))
                    {
                        // It's already added so do not have to work anymore.
                        continue;
                    }

                    if (shift_tokens.ContainsKey(tuple.Item1))
                    {
#if !DEBUG
                        print_header("SHIFT-REDUCE CONFLICTS");
                        GlobalPrinter.Append($"Shift-Reduce Conflict! {(tuple.Item1 == -1 ? "$" : production_rules[tuple.Item1].production_name)}\r\n");
                        GlobalPrinter.Append($"States: {ms.Key} {small_shift_info[shift_tokens[tuple.Item1]].Item2}\r\n");
                        print_states(ms.Key, states[ms.Key]);
                        print_states(small_shift_info[shift_tokens[tuple.Item1]].Item2, states[small_shift_info[shift_tokens[tuple.Item1]].Item2]);
#endif
                        Tuple<int, bool> p1 = null, p2 = null;

#if DEBUG
                        string mm = "";
#endif
                        try
                        {
                            var pp = get_first_on_right_terminal(production_rules[tuple.Item2], tuple.Item3);
                            if (shift_reduce_conflict_solve.ContainsKey(pp.index))
                                p1 = shift_reduce_conflict_solve[pp.index];
                        }
                        catch (Exception e)
                        {
#if !DEBUG
                            GlobalPrinter.Append(e.Message + "\r\n");
#else
                            mm = e.Message + "\r\n";
#endif
                        }

                        if (shift_reduce_conflict_solve.ContainsKey(tuple.Item1))
                            p2 = shift_reduce_conflict_solve[tuple.Item1];

                        if (shift_reduce_conflict_solve_with_production_rule.ContainsKey(tuple.Item2))
                            if (shift_reduce_conflict_solve_with_production_rule[tuple.Item2].ContainsKey(tuple.Item3))
                                p1 = shift_reduce_conflict_solve_with_production_rule[tuple.Item2][tuple.Item3];

                        //if (shift_reduce_conflict_solve_with_production_rule.ContainsKey(states[tuple.Item1][0].Item1))
                        //    if (shift_reduce_conflict_solve_with_production_rule[states[tuple.Item1][0].Item1].ContainsKey(states[tuple.Item1][0].Item2))
                        //        p2 = shift_reduce_conflict_solve_with_production_rule[states[tuple.Item1][0].Item1][states[tuple.Item1][0].Item2];

                        if (p1 == null || p2 == null)
                        {
#if DEBUG
                            print_header("SHIFT-REDUCE CONFLICTS");
                            GlobalPrinter.Append($"Shift-Reduce Conflict! {(tuple.Item1 == -1 ? "$" : production_rules[tuple.Item1].production_name)}\r\n");
                            GlobalPrinter.Append($"States: {ms.Key} {small_shift_info[shift_tokens[tuple.Item1]].Item2}\r\n");
                            print_states(ms.Key, states[ms.Key]);
                            print_states(small_shift_info[shift_tokens[tuple.Item1]].Item2, states[small_shift_info[shift_tokens[tuple.Item1]].Item2]);
                            if (mm != "")
                                GlobalPrinter.Append(mm);
#endif
                            occurred_conflict = true;
                            continue;
                        }

                        if (p1.Item1 < p2.Item1 || (p1.Item1 == p2.Item1 && p1.Item2))
                        {
                            // Reduce
                            if (!reduce_info.ContainsKey(merged_merged_index[ms.Key]))
                                reduce_info.Add(merged_merged_index[ms.Key], new List<Tuple<int, int, int>>());
                            reduce_info[merged_merged_index[ms.Key]].Add(new Tuple<int, int, int>(tuple.Item1, tuple.Item2, tuple.Item3));
                        }
                        else
                        {
                            // Shift
                            if (!shift_info.ContainsKey(merged_merged_index[ms.Key]))
                                shift_info.Add(merged_merged_index[ms.Key], new List<Tuple<int, int>>());
                            shift_info[merged_merged_index[ms.Key]].Add(new Tuple<int, int>(tuple.Item1, merged_merged_index[small_shift_info[shift_tokens[tuple.Item1]].Item2]));
                        }

                        completes.Add(tuple.Item1);
                    }
                    else
                    {
                        // Just add reduce item
                        if (!reduce_info.ContainsKey(merged_merged_index[ms.Key]))
                            reduce_info.Add(merged_merged_index[ms.Key], new List<Tuple<int, int, int>>());
                        reduce_info[merged_merged_index[ms.Key]].Add(new Tuple<int, int, int>(tuple.Item1, tuple.Item2, tuple.Item3));

                        completes.Add(tuple.Item1);
                    }
                }

                foreach (var pair in shift_tokens)
                {
                    if (completes.Contains(pair.Key)) continue;
                    var shift = small_shift_info[pair.Value];
                    if (!shift_info.ContainsKey(merged_merged_index[ms.Key]))
                        shift_info.Add(merged_merged_index[ms.Key], new List<Tuple<int, int>>());
                    shift_info[merged_merged_index[ms.Key]].Add(new Tuple<int, int>(shift.Item1, merged_merged_index[shift.Item2]));
                }
            }
            // -----------------------------------------------------

            if (occurred_conflict)
                throw new Exception("Specify the rules to resolve Shift-Reduce Conflict!");

            number_of_states = merged_states.Count;
        }
        #endregion

        #region LALR Generator From LR(0) Items

        /// <summary>
        /// http://3e8.org/pub/scheme/doc/parsing/Efficient%20Computation%20of%20LALR(1)%20Look-Ahead%20Sets.pdf
        /// 
        /// DEREMER, Frank; PENNELLO, Thomas. Efficient computation of LALR (1) look-ahead sets.
        /// ACM Transactions on Programming Languages and Systems (TOPLAS), 1982, 4.4: 615-649.
        /// 
        /// Dragon Book 4.7.5 - Efficient Construction of LALR Parsing Table
        /// </summary>
        public void GenerateLALR2()
        {
            // --------------- Delete EmptyString ---------------
            for (int i = 0; i < production_rules.Count; i++)
                if (!production_rules[i].isterminal)
                    for (int j = 0; j < production_rules[i].sub_productions.Count; j++)
                        if (production_rules[i].sub_productions[j][0].index == EmptyString.index)
                            production_rules[i].sub_productions[j].Clear();
            // --------------------------------------------------

            // --------------- Collect FIRST,FOLLOW Set ---------------
            var FIRST = new List<HashSet<int>>();
            foreach (var rule in production_rules)
                FIRST.Add(first_terminals(rule.index));

            var FOLLOW = follow_terminals(FIRST);

#if true
            print_header("FISRT, FOLLOW SETS");
            print_hs(FIRST, "FIRST");
            print_hs(FOLLOW, "FOLLOW");
#endif
            // --------------------------------------------------------

            // --------------- Determine exists epsillon ---------------
            var include_epsillon = Enumerable.Repeat(false, production_rules.Count).ToList();

            for (int i = 0; i < production_rules.Count; i++)
                if (!production_rules[i].isterminal)
                    if (production_rules[i].sub_productions.Any(x => x.Count == 0))
                        include_epsillon[i] = true;

            // Find productions contained epsillon.
            while (true)
            {
                var change = false;

                for (int i = 0; i < production_rules.Count; i++)
                    for (int j = 0; j < production_rules[i].sub_productions.Count; j++)
                        if (production_rules[i].sub_productions[j].All(x => include_epsillon[x.index]))
                        {
                            if (include_epsillon[i] == false)
                            {
                                include_epsillon[i] = true;
                                change = true;
                            }
                            break;
                        }

                if (!change) break;
            }
            // ---------------------------------------------------------

            // (state_index, (production_rule_index, sub_productions_pos, dot_position, (lookahead))
            var states = new Dictionary<int, List<Tuple<int, int, int, HashSet<int>>>>();
            // (state_specify, state_index)
            var state_index = new Dictionary<string, int>();
            // (state_index, (state_item_index, (handle_position, parent_state_index, parent_state_item_index)))
            var pred = new Dictionary<int, Dictionary<int, List<Tuple<int, int, int>>>>();
            // (from_state_index, (when_state_index, to_state_index))
            var goto_table = new List<Tuple<int, List<Tuple<int, int>>>>();
            // (state_index, (shift_what, state_index))
            shift_info = new Dictionary<int, List<Tuple<int, int>>>();
            // (state_index, (reduce_what, production_rule_index, sub_productions_pos))
            reduce_info = new Dictionary<int, List<Tuple<int, int, int>>>();
            var index_count = 0;

            // -------------------- Put first eater -------------------
            var first_l = closure(0, 0, 0).Select(x => new Tuple<int, int, int, HashSet<int>>(x.Item1, x.Item2, x.Item3, new HashSet<int>())).ToList();
            state_index.Add(l2sl(first_l, 1), 0);
            states.Add(0, first_l);
            // --------------------------------------------------------

            // Create all LR states
            // (states)
            var q = new Queue<int>();
            q.Enqueue(index_count++);
            while (q.Count != 0)
            {
                var p = q.Dequeue();

                // Collect goto
                // (state_index, (production_rule_index, sub_productions_pos, dot_position, lookahead))
                var gotos = new Dictionary<int, List<Tuple<int, int, int, HashSet<int>>>>();
                // (state_index, kernel_count)
                var kernel_cnt = new Dictionary<int, int>();
                // (state_index, (handle_position, parent_state_item_index))
                var ppred = new Dictionary<int, List<Tuple<int, int>>>();
                for (int i = 0; i < states[p].Count; i++)
                {
                    var transition = states[p][i];
                    if (production_rules[transition.Item1].sub_productions[transition.Item2].Count > transition.Item3)
                    {
                        var pi = production_rules[transition.Item1].sub_productions[transition.Item2][transition.Item3].index;
                        if (!gotos.ContainsKey(pi))
                        {
                            gotos.Add(pi, new List<Tuple<int, int, int, HashSet<int>>>());
                            kernel_cnt.Add(pi, 0);
                            ppred.Add(pi, new List<Tuple<int, int>>());
                        }
                        gotos[pi].Add(new Tuple<int, int, int, HashSet<int>>(transition.Item1, transition.Item2, transition.Item3 + 1, new HashSet<int>()));
                        kernel_cnt[pi] = kernel_cnt[pi] + 1;
                        ppred[pi].Add(new Tuple<int, int>(transition.Item3, i));
                    }
                }

                // Populate closures
                foreach (var goto_unit in gotos)
                {
                    var set = new HashSet<string>();
                    // Push exists transitions
                    foreach (var psd in goto_unit.Value)
                        set.Add(i2s(psd.Item1, psd.Item2, psd.Item3));
                    // Find all transitions
                    var new_trans = new List<Tuple<int, int, int, HashSet<int>>>();
                    foreach (var psd in goto_unit.Value)
                    {
                        if (production_rules[psd.Item1].sub_productions[psd.Item2].Count == psd.Item3) continue;
                        if (production_rules[psd.Item1].sub_productions[psd.Item2][psd.Item3].isterminal) continue;
                        var first_nt = closure(psd.Item1, psd.Item2, psd.Item3);
                        foreach (var nts in first_nt)
                            if (!set.Contains(t2s(nts)))
                            {
                                new_trans.Add(new Tuple<int, int, int, HashSet<int>>(nts.Item1, nts.Item2, nts.Item3, new HashSet<int>()));
                                set.Add(t2s(nts));
                            }
                    }
                    goto_unit.Value.AddRange(new_trans);
                }

                // Build goto transitions ignore terminal, non-terminal anywhere
                var index_list = new List<Tuple<int, int>>();
                foreach (var pp in gotos)
                {
                    var kernels = kernel_cnt[pp.Key];
                    var hash = l2sl(pp.Value, kernels);
                    if (!state_index.ContainsKey(hash))
                    {
                        states.Add(index_count, pp.Value);
                        state_index.Add(hash, index_count);

                        if (!pred.ContainsKey(index_count))
                            pred.Add(index_count, new Dictionary<int, List<Tuple<int, int, int>>>());
                        for (int i = 0; i < kernels; i++)
                        {
                            if (!pred[index_count].ContainsKey(i))
                                pred[index_count].Add(i, new List<Tuple<int, int, int>>());
                            pred[index_count][i].Add(new Tuple<int, int, int>(ppred[pp.Key][i].Item1, p, ppred[pp.Key][i].Item2));
                        }

                        q.Enqueue(index_count++);
                    }
                    else
                    {
                        var index = state_index[hash];
                        if (!pred.ContainsKey(index))
                            pred.Add(index, new Dictionary<int, List<Tuple<int, int, int>>>());
                        for (int i = 0; i < kernels; i++)
                        {
                            if (!pred[index].ContainsKey(i))
                                pred[index].Add(i, new List<Tuple<int, int, int>>());
                            pred[index][i].Add(new Tuple<int, int, int>(ppred[pp.Key][i].Item1, p, ppred[pp.Key][i].Item2));
                        }
                    }
                    index_list.Add(new Tuple<int, int>(pp.Key, state_index[hash]));
                }

                goto_table.Add(new Tuple<int, List<Tuple<int, int>>>(p, index_list));
            }

#if false
            print_header("LR0 Items");
            foreach (var s in states)
                print_states(s.Key, s.Value);
#endif

            // -------------------- Fill Lookahead -------------------

            // Insert EOP (End of Parsing marker)
            states[0][0].Item4.Add(-1);

            // Find all reduceable LR(0) states item and fill lookahead
            while (true)
            {
                lookahead_change = false;
                foreach (var state in states)
                {
                    for (int i = 0; i < state.Value.Count; i++)
                    {
                        // Find the state that the handle is declared.
                        // If blew is declared,
                        // A -> abc.
                        // then trace location of handle definition recursive.
                        // A -> ab.c
                        // A -> a.bc
                        // A -> .abc
                        var lrs = state.Value[i];
                        if (production_rules[lrs.Item1].sub_productions[lrs.Item2].Count == lrs.Item3)
                        {
                            fill_lookahead(FIRST, include_epsillon, states, pred, state.Key, i);
                        }
                    }
                }

                if (!lookahead_change)
                    break;
            }

            //var visit = Enumerable.Repeat<bool>(false, goto_table.Count);
            //var indegree_count = new Dictionary<int, int>();
            //for (int i = 0; i < goto_table.Count; i++)
            //    for (int j = 0; j < goto_table[i].Item2.Count; j++)
            //    {
            //        var from = goto_table[i].Item2[j].Item2;
            //        if (!indegree_count.ContainsKey(from))
            //            indegree_count.Add(from, 0);
            //        indegree_count[from] = from + 1;
            //    }

            // -------------------------------------------------------

#if true
            print_header("LALR STATES");
            foreach (var s in states)
                print_states(s.Key, s.Value);
#endif

            var occurred_conflict = false;

            // ------------- Find Shift-Reduce Conflict ------------
            foreach (var state in states)
            {
                // (shift_what, state_index)
                var small_shift_info = new List<Tuple<int, int>>();
                // (reduce_what, production_rule_index, sub_productions_pos)
                var small_reduce_info = new List<Tuple<int, int, int>>();

                // Fill Shift Info
                foreach (var pp in goto_table[state.Key].Item2)
                    small_shift_info.Add(new Tuple<int, int>(pp.Item1, pp.Item2));

                // Fill Reduce Info
                foreach (var transition in state.Value)
                    if (production_rules[transition.Item1].sub_productions[transition.Item2].Count == transition.Item3)
                    {
                        foreach (var term in transition.Item4)
                            small_reduce_info.Add(new Tuple<int, int, int>(term, transition.Item1, transition.Item2));
                    }

                // Conflict Check
                // (shift_what, small_shift_info_index)
                var shift_tokens = new Dictionary<int, int>();
                for (int i = 0; i < small_shift_info.Count; i++)
                    shift_tokens.Add(small_shift_info[i].Item1, i);
                var completes = new HashSet<int>();

                foreach (var tuple in small_reduce_info)
                {
                    if (completes.Contains(tuple.Item1))
                    {
                        // It's already added so do not have to work anymore.
                        continue;
                    }

                    if (shift_tokens.ContainsKey(tuple.Item1))
                    {
#if false
                        print_header("SHIFT-REDUCE CONFLICTS");
                        GlobalPrinter.Append($"Shift-Reduce Conflict! {(tuple.Item1 == -1 ? "$" : production_rules[tuple.Item1].production_name)}\r\n");
                        GlobalPrinter.Append($"States: {ms.Key} {small_shift_info[shift_tokens[tuple.Item1]].Item2}\r\n");
                        print_states(ms.Key, states[ms.Key]);
                        print_states(small_shift_info[shift_tokens[tuple.Item1]].Item2, states[small_shift_info[shift_tokens[tuple.Item1]].Item2]);
#endif
                        Tuple<int, bool> p1 = null, p2 = null;

#if DEBUG
                        string mm = "";
#endif
                        try
                        {
                            var pp = get_first_on_right_terminal(production_rules[tuple.Item2], tuple.Item3);
                            if (shift_reduce_conflict_solve.ContainsKey(pp.index))
                                p1 = shift_reduce_conflict_solve[pp.index];
                        }
                        catch (Exception e)
                        {
#if !DEBUG
                            GlobalPrinter.Append(e.Message + "\r\n");
#else
                            mm = e.Message + "\r\n";
#endif
                        }

                        if (shift_reduce_conflict_solve.ContainsKey(tuple.Item1))
                            p2 = shift_reduce_conflict_solve[tuple.Item1];

                        if (shift_reduce_conflict_solve_with_production_rule.ContainsKey(tuple.Item2))
                            if (shift_reduce_conflict_solve_with_production_rule[tuple.Item2].ContainsKey(tuple.Item3))
                                p1 = shift_reduce_conflict_solve_with_production_rule[tuple.Item2][tuple.Item3];

                        //if (shift_reduce_conflict_solve_with_production_rule.ContainsKey(states[tuple.Item1][0].Item1))
                        //    if (shift_reduce_conflict_solve_with_production_rule[states[tuple.Item1][0].Item1].ContainsKey(states[tuple.Item1][0].Item2))
                        //        p2 = shift_reduce_conflict_solve_with_production_rule[states[tuple.Item1][0].Item1][states[tuple.Item1][0].Item2];

                        if (p1 == null || p2 == null)
                        {
#if DEBUG
                            print_header("SHIFT-REDUCE CONFLICTS");
                            GlobalPrinter.Append($"Shift-Reduce Conflict! {(tuple.Item1 == -1 ? "$" : production_rules[tuple.Item1].production_name)}\r\n");
                            GlobalPrinter.Append($"States: {state.Key} {small_shift_info[shift_tokens[tuple.Item1]].Item2}\r\n");
                            print_states(state.Key, state.Value);
                            print_states(small_shift_info[shift_tokens[tuple.Item1]].Item2, states[small_shift_info[shift_tokens[tuple.Item1]].Item2]);
                            if (mm != "")
                                GlobalPrinter.Append(mm);
#endif
                            occurred_conflict = true;
                            continue;
                        }

                        if (p1.Item1 < p2.Item1 || (p1.Item1 == p2.Item1 && p1.Item2))
                        {
                            // Reduce
                            if (!reduce_info.ContainsKey(state.Key))
                                reduce_info.Add(state.Key, new List<Tuple<int, int, int>>());
                            reduce_info[state.Key].Add(new Tuple<int, int, int>(tuple.Item1, tuple.Item2, tuple.Item3));
                        }
                        else
                        {
                            // Shift
                            if (!shift_info.ContainsKey(state.Key))
                                shift_info.Add(state.Key, new List<Tuple<int, int>>());
                            shift_info[state.Key].Add(new Tuple<int, int>(tuple.Item1, small_shift_info[shift_tokens[tuple.Item1]].Item2));
                        }

                        completes.Add(tuple.Item1);
                    }
                    else
                    {
                        // Just add reduce item
                        if (!reduce_info.ContainsKey(state.Key))
                            reduce_info.Add(state.Key, new List<Tuple<int, int, int>>());
                        reduce_info[state.Key].Add(new Tuple<int, int, int>(tuple.Item1, tuple.Item2, tuple.Item3));

                        completes.Add(tuple.Item1);
                    }
                }

                foreach (var pair in shift_tokens)
                {
                    if (completes.Contains(pair.Key)) continue;
                    var shift = small_shift_info[pair.Value];
                    if (!shift_info.ContainsKey(state.Key))
                        shift_info.Add(state.Key, new List<Tuple<int, int>>());
                    shift_info[state.Key].Add(new Tuple<int, int>(shift.Item1, shift.Item2));
                }
            }
            // -----------------------------------------------------

            if (occurred_conflict)
                throw new Exception("Specify the rules to resolve Shift-Reduce Conflict!");

            number_of_states = states.Count;
        }

        #endregion

        #region Printer

        public void PrintProductionRules()
        {
            print_header("PRODUCTION RULES");
            int count = 1;
            var builder = new StringBuilder();
            foreach (var pp in production_rules)
            {
                foreach (var p in pp.sub_productions)
                {
                    builder.Append($"{(count++).ToString().PadLeft(4)}: ");
                    builder.Append($"{pp.production_name.ToString().PadLeft(10)} -> ");

                    for (int i = 0; i < p.Count; i++)
                    {
                        builder.Append(p[i].production_name + " ");
                    }

                    builder.Append("\r\n");
                }
            }
            GlobalPrinter.Append(builder.ToString());
        }

        /// <summary>
        /// 파싱 테이블을 집합형태로 출력합니다.
        /// </summary>
        public void PrintStates()
        {
            print_header("FINAL STATES");
            for (int i = 0; i < number_of_states; i++)
            {
                var builder = new StringBuilder();
                var x = $"I{i} => ";
                builder.Append(x);
                if (shift_info.ContainsKey(i))
                {
                    builder.Append("SHIFT{" + string.Join(",", shift_info[i].Select(y => $"({production_rules[y.Item1].production_name},I{y.Item2})")) + "}");
                    if (reduce_info.ContainsKey(i))
                        builder.Append("\r\n" + "".PadLeft(x.Length) + "REDUCE{" + string.Join(",", reduce_info[i].Select(y => $"({(y.Item1 == -1 ? "$" : production_rules[y.Item1].production_name)},{(y.Item2 == 0 ? "accept" : production_rules[y.Item2].production_name)},{y.Item3})")) + "}");
                }
                else if (reduce_info.ContainsKey(i))
                    builder.Append("REDUCE{" + string.Join(",", reduce_info[i].Select(y => $"({(y.Item1 == -1 ? "$" : production_rules[y.Item1].production_name)},{(y.Item2 == 0 ? "accept" : production_rules[y.Item2].production_name)},{y.Item3})")) + "}");
                GlobalPrinter.Append(builder.ToString() + "\r\n");
            }
        }

        /// <summary>
        /// 파싱테이블을 테이블 형태로 출력합니다.
        /// </summary>
        public void PrintTable()
        {
            var production_mapping = new List<List<int>>();
            var pm_count = 0;

            foreach (var pr in production_rules)
            {
                var pm = new List<int>();
                foreach (var sub_pr in pr.sub_productions)
                    pm.Add(pm_count++);
                production_mapping.Add(pm);
            }

            var builder = new StringBuilder();

            var tokens = new Dictionary<int, int>();
            var max_len = 0;
            foreach (var pp in production_rules)
                if (pp.isterminal)
                    tokens.Add(tokens.Count, pp.index);
            tokens.Add(tokens.Count, -1);
            foreach (var pp in production_rules)
            {
                if (pp.index == 0) continue;
                if (!pp.isterminal)
                    tokens.Add(tokens.Count, pp.index);
                max_len = Math.Max(max_len, pp.production_name.Length);
            }

            var split_line = "+" + new string('*', production_rules.Count + 1).Replace("*", new string('-', max_len + 2) + "+") + "\r\n";
            builder.Append(split_line);

            // print production rule
            builder.Append('|' + "".PadLeft(max_len + 2) + '|');
            for (int i = 0; i < tokens.Count; i++)
            {
                builder.Append(" " + (tokens[i] == -1 ? "$" : production_rules[tokens[i]].production_name).PadLeft(max_len) + " ");
                builder.Append('|');
            }
            builder.Append("\r\n");
            builder.Append(split_line);

            // print states
            for (int i = 0; i < number_of_states; i++)
            {
                builder.Append('|' + "  " + $"{i}".PadLeft(max_len - 2) + "  |");

                // (what, (state_index, isshift))
                var sr_info = new Dictionary<int, Tuple<int, bool>>();

                if (shift_info.ContainsKey(i))
                {
                    foreach (var si in shift_info[i])
                        if (!sr_info.ContainsKey(si.Item1))
                            sr_info.Add(si.Item1, new Tuple<int, bool>(si.Item2, true));
                }
                if (reduce_info.ContainsKey(i))
                {
                    foreach (var ri in reduce_info[i])
                        if (!sr_info.ContainsKey(ri.Item1))
                            sr_info.Add(ri.Item1, new Tuple<int, bool>(production_mapping[ri.Item2][ri.Item3], false));
                }

                for (int j = 0; j < tokens.Count; j++)
                {
                    var k = tokens[j];
                    if (sr_info.ContainsKey(k))
                    {
                        var ss = "";
                        if (sr_info[k].Item2)
                        {
                            if (production_rules[k].isterminal)
                                ss += "s" + sr_info[k].Item1;
                            else
                                ss = sr_info[k].Item1.ToString();
                        }
                        else
                        {
                            if (sr_info[k].Item1 == 0)
                                ss += "acc";
                            else
                                ss += "r" + sr_info[k].Item1;
                        }
                        builder.Append(" " + ss.PadLeft(max_len) + " |");
                    }
                    else
                    {
                        builder.Append("".PadLeft(max_len + 2) + "|");
                    }
                }

                builder.Append("\r\n");
            }
            builder.Append(split_line);

            print_header("PARSING TABLE");
            GlobalPrinter.Append(builder.ToString() + "\r\n");
        }

        #endregion

        #region FIRST

        /// <summary>
        /// Calculate FIRST only Terminals
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private HashSet<int> first_terminals(int index)
        {
            var result = new HashSet<int>();
            var q = new Queue<int>();
            var visit = new List<bool>();
            visit.AddRange(Enumerable.Repeat(false, production_rules.Count));
            q.Enqueue(index);

            while (q.Count != 0)
            {
                var p = q.Dequeue();
                if (p < 0 || visit[p]) continue;
                visit[p] = true;

                if (p < 0 || production_rules[p].isterminal)
                    result.Add(p);
                else
                {
                    foreach (var pp in production_rules[p].sub_productions.Where(x => x.Count > 0))
                    {
                        foreach (var ppp in pp)
                        {
                            q.Enqueue(ppp.index);
                            if (ppp.sub_productions.All(x => x.Count != 0))
                                break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Calculate FIRST only Non-Terminals
        /// </summary>
        /// <param name="production_rule_index"></param>
        /// <returns></returns>
        private List<Tuple<int, int, int>> first_nonterminals(int production_rule_index)
        {
            // (production_rule_index, sub_productions_pos, dot_position)
            var first_l = new List<Tuple<int, int, int>>();
            // (production_rule_index, sub_productions_pos)
            var first_q = new Queue<Tuple<int, int>>();
            // (production_rule_index, (sub_productions_pos))
            var first_visit = new Dictionary<int, HashSet<int>>();
            first_q.Enqueue(new Tuple<int, int>(production_rule_index, 0));
            for (int j = 0; j < production_rules[production_rule_index].sub_productions.Count; j++)
                first_q.Enqueue(new Tuple<int, int>(production_rule_index, j));
            // Get all of starts node FIRST non-terminals
            while (first_q.Count != 0)
            {
                var t = first_q.Dequeue();
                if (first_visit.ContainsKey(t.Item1) && first_visit[t.Item1].Contains(t.Item2)) continue;
                if (!first_visit.ContainsKey(t.Item1)) first_visit.Add(t.Item1, new HashSet<int>());
                first_visit[t.Item1].Add(t.Item2);
                first_l.Add(new Tuple<int, int, int>(t.Item1, t.Item2, 0));
                for (int i = 0; i < production_rules[t.Item1].sub_productions.Count; i++)
                {
                    var sub_pr = production_rules[t.Item1].sub_productions[i];
                    if (sub_pr.Count > 0 && sub_pr[0].isterminal == false)
                        for (int j = 0; j < production_rules[sub_pr[0].index].sub_productions.Count; j++)
                            first_q.Enqueue(new Tuple<int, int>(sub_pr[0].index, j));
                }
            }
            return first_l;
        }

        #endregion

        #region FOLLOW

        /// <summary>
        /// Get FOLLOW set for all production-rules
        /// </summary>
        /// <param name="FIRST"></param>
        /// <returns></returns>
        private List<HashSet<int>> follow_terminals(List<HashSet<int>> FIRST)
        {
            var FOLLOW = new List<HashSet<int>>();
            for (int i = 0; i < production_rules.Count; i++)
                FOLLOW.Add(new HashSet<int>());
            FOLLOW[0].Add(-1); // -1: Sentinel

            // 1. B -> a A b, Add FIRST(b) to FOLLOW(A)
            for (int i = 0; i < production_rules.Count; i++)
                if (!production_rules[i].isterminal)
                    foreach (var rule in production_rules[i].sub_productions)
                        for (int j = 0; j < rule.Count - 1; j++)
                            if (rule[j].isterminal == false || rule[j + 1].isterminal)
                                foreach (var r in FIRST[rule[j + 1].index])
                                    FOLLOW[rule[j].index].Add(r);

            // 2. B -> a A b and empty -> FIRST(b), Add FOLLOW(B) to FOLLOW(A)
            for (int i = 0; i < production_rules.Count; i++)
                if (!production_rules[i].isterminal)
                    foreach (var rule in production_rules[i].sub_productions)
                        if (rule.Count > 2 && rule[rule.Count - 2].isterminal == false && FIRST[rule.Last().index].Contains(EmptyString.index))
                            foreach (var r in FOLLOW[i])
                                FOLLOW[rule[rule.Count - 2].index].Add(r);

            // 3. B -> a A, Add FOLLOW(B) to FOLLOW(A)
            for (int i = 0; i < production_rules.Count; i++)
                if (!production_rules[i].isterminal)
                    foreach (var rule in production_rules[i].sub_productions)
                        if (rule.Count > 0 && rule.Last().isterminal == false)
                            foreach (var r in FOLLOW[i])
                                if (rule.Last().index > 0)
                                    FOLLOW[rule.Last().index].Add(r);

            return FOLLOW;
        }

        #endregion

        #region Closure with Lookahead

        /// <summary>
        /// Get lookahead states item with first item's closure
        /// This function is used in first_with_lookahead function. 
        /// -1: Sentinel lookahead
        /// </summary>
        /// <param name="production_rule_index"></param>
        /// <returns></returns>
        private List<Tuple<int, int, int, HashSet<int>>> lookahead_with_first(int production_rule_index, int sub_production, int sub_production_index, HashSet<int> pred)
        {
            // (production_rule_index, sub_productions_pos, dot_position, (lookahead))
            var states = new List<Tuple<int, int, int, HashSet<int>>>();
            // (production_rule_index, (sub_productions_pos))
            var first_visit = new Dictionary<int, HashSet<int>>();
            states.Add(new Tuple<int, int, int, HashSet<int>>(production_rule_index, sub_production, sub_production_index, pred));
            if (production_rule_index == 0 && sub_production == 0 && sub_production_index == 0)
                pred.Add(-1); // Push sentinel
            if (production_rules[production_rule_index].sub_productions[sub_production].Count > sub_production_index)
            {
                if (!production_rules[production_rule_index].sub_productions[sub_production][sub_production_index].isterminal)
                {
                    var index_populate = production_rules[production_rule_index].sub_productions[sub_production][sub_production_index].index;
                    if (production_rules[production_rule_index].sub_productions[sub_production].Count <= sub_production_index + 1)
                    {
                        for (int i = 0; i < production_rules[index_populate].sub_productions.Count; i++)
                            states.Add(new Tuple<int, int, int, HashSet<int>>(index_populate, i, 0, new HashSet<int>(pred)));
                    }
                    else
                    {
                        var first_lookahead = first_terminals(production_rules[production_rule_index].sub_productions[sub_production][sub_production_index + 1].index);
                        for (int i = 0; i < production_rules[index_populate].sub_productions.Count; i++)
                            states.Add(new Tuple<int, int, int, HashSet<int>>(index_populate, i, 0, new HashSet<int>(first_lookahead)));
                    }
                }
            }
            return states;
        }

        /// <summary>
        /// Get FIRST items with lookahead (Build specific states completely)
        /// 
        /// TODO: Fix issues #4:first_terminals
        /// </summary>
        /// <param name="production_rule_index"></param>
        /// <param name="sub_production"></param>
        /// <param name="sub_production_index"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        private List<Tuple<int, int, int, HashSet<int>>> first_with_lookahead(int production_rule_index, int sub_production, int sub_production_index, HashSet<int> pred)
        {
            // (production_rule_index, sub_productions_pos, dot_position, (lookahead))
            var states = new List<Tuple<int, int, int, HashSet<int>>>();
            // (production_rule_index + sub_productions_pos + dot_position), (states_index)
            var states_prefix = new Dictionary<string, int>();

            var q = new Queue<List<Tuple<int, int, int, HashSet<int>>>>();
            q.Enqueue(lookahead_with_first(production_rule_index, sub_production, sub_production_index, pred));
            while (q.Count != 0)
            {
                var ll = q.Dequeue();
                foreach (var e in ll)
                {
                    var ii = i2s(e.Item1, e.Item2, e.Item3);
                    if (!states_prefix.ContainsKey(ii))
                    {
                        states_prefix.Add(ii, states.Count);
                        states.Add(e);
                        q.Enqueue(lookahead_with_first(e.Item1, e.Item2, e.Item3, e.Item4));
                    }
                    else
                    {
                        foreach (var hse in e.Item4)
                            states[states_prefix[ii]].Item4.Add(hse);
                    }
                }
            }

            // (production_rule_index + sub_productions_pos + dot_position), (states_index)
            var states_prefix2 = new Dictionary<string, int>();
            var states_count = 0;
            bool change = false;

            do
            {
                change = false;
                q.Enqueue(lookahead_with_first(production_rule_index, sub_production, sub_production_index, pred));
                while (q.Count != 0)
                {
                    var ll = q.Dequeue();
                    foreach (var e in ll)
                    {
                        var ii = i2s(e.Item1, e.Item2, e.Item3);
                        if (!states_prefix2.ContainsKey(ii))
                        {
                            states_prefix2.Add(ii, states_count);
                            foreach (var hse in e.Item4)
                                if (!states[states_prefix[ii]].Item4.Contains(hse))
                                {
                                    change = true;
                                    states[states_prefix[ii]].Item4.Add(hse);
                                }
                            q.Enqueue(lookahead_with_first(e.Item1, e.Item2, e.Item3, states[states_count].Item4));
                            states_count++;
                        }
                        else
                        {
                            foreach (var hse in e.Item4)
                                if (!states[states_prefix[ii]].Item4.Contains(hse))
                                {
                                    change = true;
                                    states[states_prefix[ii]].Item4.Add(hse);
                                }
                        }
                    }
                }
            } while (change);

            return states;
        }

        #endregion

        #region Closure

        /// <summary>
        /// Get states item with first item's closure
        /// This function is used in closure function. 
        /// -1: Sentinel lookahead
        /// </summary>
        /// <param name="production_rule_index"></param>
        /// <returns></returns>
        private List<Tuple<int, int, int>> closure_first(int production_rule_index, int sub_production, int sub_production_index)
        {
            // (production_rule_index, sub_productions_pos, dot_position, (lookahead))
            var states = new List<Tuple<int, int, int>>();
            states.Add(new Tuple<int, int, int>(production_rule_index, sub_production, sub_production_index));
            if (production_rules[production_rule_index].sub_productions[sub_production].Count > sub_production_index)
            {
                if (!production_rules[production_rule_index].sub_productions[sub_production][sub_production_index].isterminal)
                {
                    var index_populate = production_rules[production_rule_index].sub_productions[sub_production][sub_production_index].index;
                    for (int i = 0; i < production_rules[index_populate].sub_productions.Count; i++)
                        states.Add(new Tuple<int, int, int>(index_populate, i, 0));
                }
            }
            return states;
        }

        /// <summary>
        /// Get CLOSURE items (Build specific states completely)
        /// </summary>
        /// <param name="production_rule_index"></param>
        /// <param name="sub_production"></param>
        /// <param name="sub_production_index"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        private List<Tuple<int, int, int>> closure(int production_rule_index, int sub_production, int sub_production_index)
        {
            // (production_rule_index, sub_productions_pos, dot_position, (lookahead))
            var states = new List<Tuple<int, int, int>>();
            // (production_rule_index + sub_productions_pos + dot_position), (states_index)
            var states_prefix = new Dictionary<string, int>();

            var q = new Queue<List<Tuple<int, int, int>>>();
            q.Enqueue(closure_first(production_rule_index, sub_production, sub_production_index));
            while (q.Count != 0)
            {
                var ll = q.Dequeue();
                foreach (var e in ll)
                {
                    var ii = i2s(e.Item1, e.Item2, e.Item3);
                    if (!states_prefix.ContainsKey(ii))
                    {
                        states_prefix.Add(ii, states.Count);
                        states.Add(e);
                        q.Enqueue(closure_first(e.Item1, e.Item2, e.Item3));
                    }
                }
            }

            // (production_rule_index + sub_productions_pos + dot_position), (states_index)
            var states_prefix2 = new Dictionary<string, int>();
            var states_count = 0;

            q.Enqueue(closure_first(production_rule_index, sub_production, sub_production_index));
            while (q.Count != 0)
            {
                var ll = q.Dequeue();
                foreach (var e in ll)
                {
                    var ii = i2s(e.Item1, e.Item2, e.Item3);
                    if (!states_prefix2.ContainsKey(ii))
                    {
                        states_prefix2.Add(ii, states_count);
                        q.Enqueue(closure_first(e.Item1, e.Item2, e.Item3));
                        states_count++;
                    }
                }
            }

            return states;
        }

        #endregion

        #region SCC

        /// <summary>
        /// Determine group using tarjan's strongly connected components algorithm.
        /// </summary>
        /// <param name="goto_table"></param>
        /// <returns></returns>
        private Dictionary<int, int> determine_group(Dictionary<int, List<int>> degree)
        {
            return null;
        }

        /// <summary>
        /// Returns the graph traversal rule and SCC information
        /// </summary>
        /// <param name="goto_table"></param>
        /// <returns></returns>
        private Tuple<List<int>, Dictionary<int, int>> how_to_visit(
            // (from_state_index, (when_state_index, to_state_index))
            List<Tuple<int, List<Tuple<int, int>>>> goto_table)
        {
            // to edge
            var degree = new Dictionary<int, List<int>>();
            for (int i = 0; i < goto_table.Count; i++)
            {
                degree.Add(i, new List<int>());
                for (int j = 0; j < goto_table[i].Item2.Count; i++)
                    degree[i].Add(goto_table[i].Item2[j].Item2);
            }

            return null;
        }

        #endregion

        #region Lookahead

        /// <summary>
        /// Get FIRST items of production rule item
        /// </summary>
        /// <param name="production_rule_index"></param>
        /// <param name="sub_production"></param>
        /// <param name="sub_production_index"></param>
        /// <returns></returns>
        private HashSet<int> first_terminals(
            List<HashSet<int>> first, List<bool> include_epsillon,
            int production_rule_index, int sub_production, int sub_production_index, HashSet<int> lookahead)
        {
            // 1. If the handle points last of production rule item,
            // A -> abc. [~]
            // then return only lookaheads.
            if (production_rules[production_rule_index].sub_productions[sub_production].Count <= sub_production_index)
                return lookahead;

            // 2. Check is terminal
            // A -> aB.c [~]  (a,c=terminal, B=non-terminal)
            // If 'c' is terminal, then just return 'c'.
            if (production_rules[production_rule_index].sub_productions[sub_production][sub_production_index].isterminal)
                return new HashSet<int> { production_rules[production_rule_index].sub_productions[sub_production][sub_production_index].index };

            // 3. Get FIRST of Non-terminal
            // A -> a.Bc [~]  (a,c=terminal, B=non-terminal)
            // If 'B' is non-terminal, then get FIRST set of non-terminal 'B'.
            var result = new HashSet<int>(first[production_rules[production_rule_index].sub_productions[sub_production][sub_production_index].index]);

            // 4. Check empty-string
            // A -> a.BC [~]  (a=terminal, B,C=non-terminal)
            // If epsillon contains in FIRST(BC) then add lookahead to result.
            var fully_empty_string = true;
            for (int i = sub_production_index; i < production_rules[production_rule_index].sub_productions[sub_production].Count; i++)
            {
                var index = production_rules[production_rule_index].sub_productions[sub_production][i];
                if (index.isterminal || !include_epsillon[index.index])
                {
                    fully_empty_string = false;
                    break;
                }

                if (i != sub_production_index)
                    foreach (var lk in first[index.index])
                        result.Add(lk);
            }

            // 5. If all of after symbol contains epsillon
            // A -> a.BCD [~]  (a=terminal, B,C,D=non-terminal, B,C,D contains epsillon)
            // Then insert FOLLOW(A) or lookahead to FIRST(BCD)
            if (fully_empty_string)
                foreach (var lk in lookahead)
                    result.Add(lk);

            return result;
        }

        /// <summary>
        /// Calculate lookahead from specific non-terminal symbol on LR(0) states
        /// </summary>
        /// <param name="states"></param>
        /// <param name="state"></param>
        /// <param name="production_rule_index"></param>
        /// <param name="lookahead"></param>
        /// <returns></returns>
        private HashSet<int> first_from_nonterminal(
            List<HashSet<int>> first, List<bool> include_epsillon,
            // (state_index, (production_rule_index, sub_productions_pos, dot_position, (lookahead))
            Dictionary<int, List<Tuple<int, int, int, HashSet<int>>>> states,
            int state_index, int /* non-terminal */ production_rule_index)
        {
            var result = new HashSet<int>();

            if (production_rule_index == 0)
                result.Add(-1);

            foreach (var state in states[state_index])
            {
                // If another item handle points input non-terminal symbol,
                if (production_rules[state.Item1].sub_productions[state.Item2].Count != 0 &&
                    production_rules[state.Item1].sub_productions[state.Item2].Count > state.Item3 &&
                    production_rules[state.Item1].sub_productions[state.Item2][state.Item3].index == production_rule_index)
                {
                    var ft = first_terminals(first, include_epsillon, state.Item1, state.Item2, state.Item3 + 1, state.Item4);
                    foreach (var lookahead in ft)
                        result.Add(lookahead);
                }
            }

            return result;
        }

        bool lookahead_change = false;

        /// <summary>
        /// Fill lookahead recursive.
        /// </summary>
        /// <param name="states"></param>
        /// <param name="pred"></param>
        /// <param name="state"></param>
        /// <param name="state_index"></param>
        /// <returns></returns>
        private HashSet<int> fill_lookahead(
            List<HashSet<int>> first, List<bool> include_epsillon,
            // (state_index, (production_rule_index, sub_productions_pos, dot_position, (lookahead))
            Dictionary<int, List<Tuple<int, int, int, HashSet<int>>>> states,
            // (state_index, (state_item_index, (handle_position, parent_state_index, parent_state_item_index)))
            Dictionary<int, Dictionary<int, List<Tuple<int, int, int>>>> pred,
            int state, int state_index, int depth = int.MaxValue)
        {
            var lookaheads = new HashSet<int>();

            if (state == 0 || depth == 0 || !pred[state].ContainsKey(state_index))
            {
                var my_lookahead = first_from_nonterminal(first, include_epsillon, states, state, states[state][state_index].Item1);
                foreach (var lk in my_lookahead)
                    lookaheads.Add(lk);

                // 같은 production rule에 전파
                foreach (var item in states[state])
                    if (item.Item1 == states[state][state_index].Item1)
                        foreach (var lk in lookaheads)
                            if (!item.Item4.Contains(lk))
                            {
                                item.Item4.Add(lk);
                                lookahead_change = true;
                            }
            }
            else
            {
                foreach (var tracing in pred[state][state_index])
                {
                    var lookahead = fill_lookahead(first, include_epsillon, states, pred, tracing.Item2, tracing.Item3, tracing.Item1);

                    foreach (var lk in lookahead)
                        lookaheads.Add(lk);
                }
            }

            foreach (var lk in lookaheads)
                if (!states[state][state_index].Item4.Contains(lk))
                {
                    states[state][state_index].Item4.Add(lk);
                    lookahead_change = true;
                }

            propagate_lookahead(states, state, state_index, first_terminals(first, include_epsillon,
                states[state][state_index].Item1, states[state][state_index].Item2, states[state][state_index].Item3 + 1, states[state][state_index].Item4));

            return lookaheads;
        }

        /// <summary>
        /// Propagate lookahead for same level item of states.
        /// </summary>
        /// <param name="states"></param>
        /// <param name="state"></param>
        /// <param name="state_index"></param>
        /// <param name="lookahead"></param>
        /// <returns></returns>
        private void propagate_lookahead(
            // (state_index, (production_rule_index, sub_productions_pos, dot_position, (lookahead))
            Dictionary<int, List<Tuple<int, int, int, HashSet<int>>>> states,
            int state, int state_index, HashSet<int> lookahead)
        {
            var item = states[state][state_index];
            var nts = first_nonterminals(item.Item1, item.Item2, item.Item3);

            var index = 0;
            foreach (var state_item in states[state])
            {
                if (nts.Contains(state_item.Item1))
                {
                    var change = false;
                    var spont = new HashSet<int>(state_item.Item4);

                    foreach (var lk in lookahead)
                    {
                        if (!state_item.Item4.Contains(lk))
                        {
                            change = true;
                            lookahead_change = true;
                            state_item.Item4.Add(lk);
                        }
                    }

                    if (change)
                        propagate_lookahead(states, state, index, state_item.Item4);
                }
                index++;
            }
        }

        /// <summary>
        /// Get FIRST nonterminals of specific LALR(1) item
        /// </summary>
        /// <param name="states"></param>
        /// <param name="state"></param>
        /// <param name="state_index"></param>
        /// <returns></returns>
        private HashSet<int> first_nonterminals(
            int production_rule_index, int sub_production_index, int dot_position)
        {
            var result = new HashSet<int>();

            for (int i = dot_position; i < production_rules[production_rule_index].sub_productions[sub_production_index].Count; i++)
            {
                var item = production_rules[production_rule_index].sub_productions[sub_production_index][i];

                // We interested in only non-terminals.
                if (item.isterminal)
                    break;

                result.Add(item.index);

                // Check this non-terminal contains epsillon.
                // If not contains, then break
                if (item.sub_productions.All(x => x.Count > 0))
                    break;
            }

            return result;
        }

        #endregion

        #region Shift Reduce Conflict Solver Helper

        private ParserProduction get_first_on_right_terminal(ParserProduction pp, int sub_production)
        {
            for (int i = pp.sub_productions[sub_production].Count - 1; i >= 0; i--)
                if (pp.sub_productions[sub_production][i].isterminal)
                    return pp.sub_productions[sub_production][i];
            throw new Exception($"Cannot solve shift-reduce conflict!\r\nProduction Name: {pp.production_name}\r\nProduction Index: {sub_production}");
        }

        #endregion

        #region Create Parser Instance

        /// <summary>
        /// Create ShiftReduce Parser
        /// </summary>
        /// <returns></returns>
        public ShiftReduceParser CreateShiftReduceParserInstance()
        {
            var symbol_table = new Dictionary<string, int>();
            var jump_table = new int[number_of_states][];
            var goto_table = new int[number_of_states][];
            var grammar = new List<List<int>>();
            var grammar_group = new List<int>();
            var production_mapping = new List<List<int>>();
            var semantic_rules = new List<ParserAction>();
            var pm_count = 0;

            foreach (var pr in production_rules)
            {
                var ll = new List<List<int>>();
                var pm = new List<int>();
                foreach (var sub_pr in pr.sub_productions)
                {
                    ll.Add(sub_pr.Select(x => x.index).ToList());
                    pm.Add(pm_count++);
                    grammar_group.Add(production_mapping.Count);
                }
                grammar.AddRange(ll);
                production_mapping.Add(pm);
                semantic_rules.AddRange(pr.actions);
            }

            for (int i = 0; i < number_of_states; i++)
            {
                // Last elements is sentinel
                jump_table[i] = new int[production_rules.Count + 1];
                goto_table[i] = new int[production_rules.Count + 1];
            }

            foreach (var pr in production_rules)
                symbol_table.Add(pr.production_name ?? "^", pr.index);
            symbol_table.Add("$", production_rules.Count);

            foreach (var shift in shift_info)
                foreach (var elem in shift.Value)
                {
                    jump_table[shift.Key][elem.Item1] = 1;
                    goto_table[shift.Key][elem.Item1] = elem.Item2;
                }

            foreach (var reduce in reduce_info)
                foreach (var elem in reduce.Value)
                {
                    var index = elem.Item1;
                    if (index == -1) index = production_rules.Count;
                    if (jump_table[reduce.Key][index] != 0)
                        throw new Exception($"Error! Shift-Reduce Conflict is not solved! Please use LALR or LR(1) parser!\r\nJump-Table: {reduce.Key} {index}");
                    if (elem.Item2 == 0)
                        jump_table[reduce.Key][index] = 3;
                    else
                    {
                        jump_table[reduce.Key][index] = 2;
                        goto_table[reduce.Key][index] = production_mapping[elem.Item2][elem.Item3];
                    }
                }

            return new ShiftReduceParser(symbol_table, jump_table, goto_table, grammar_group.ToArray(), grammar.Select(x => x.ToArray()).ToArray(), semantic_rules);
        }

        /// <summary>
        /// Create ShiftReduce Parser
        /// </summary>
        /// <returns></returns>
        public ExtendedShiftReduceParser CreateExtendedShiftReduceParserInstance()
        {
            var symbol_table = new Dictionary<string, int>();
            var table = new int[number_of_states][];
            var grammar = new List<List<int>>();
            var grammar_group = new List<int>();
            var production_mapping = new List<List<int>>();
            var semantic_rules = new List<ParserAction>();
            var pm_count = 0;

            foreach (var pr in production_rules)
            {
                var ll = new List<List<int>>();
                var pm = new List<int>();
                foreach (var sub_pr in pr.sub_productions)
                {
                    ll.Add(sub_pr.Select(x => x.index).ToList());
                    pm.Add(pm_count++);
                    grammar_group.Add(production_mapping.Count);
                }
                grammar.AddRange(ll);
                production_mapping.Add(pm);
                semantic_rules.AddRange(pr.actions);
            }

            for (int i = 0; i < number_of_states; i++)
            {
                // Last elements is sentinel
                table[i] = new int[production_rules.Count + 1];
            }

            var acc_max = 0;
            foreach (var pr in production_rules)
                symbol_table.Add(pr.production_name ?? "^", pr.index);
            symbol_table.Add("$", production_rules.Count);

            foreach (var shift in shift_info)
                foreach (var elem in shift.Value)
                {
                    table[shift.Key][elem.Item1] = elem.Item2;
                    if (elem.Item2 > acc_max)
                        acc_max = elem.Item2;
                }
            
            foreach (var reduce in reduce_info)
                foreach (var elem in reduce.Value)
                {
                    var index = elem.Item1;
                    if (index == -1) index = production_rules.Count;
                    if (table[reduce.Key][index] != 0)
                        throw new Exception($"Error! Shift-Reduce Conflict is not solved! Please use LALR or LR(1) parser!\r\nJump-Table: {reduce.Key} {index}");
                    if (elem.Item2 == 0)
                        table[reduce.Key][index] = acc_max + 1;
                    else
                    {
                        table[reduce.Key][index] = -production_mapping[elem.Item2][elem.Item3];
                    }
                }

            return new ExtendedShiftReduceParser(symbol_table, acc_max + 1, table, grammar_group.ToArray(), grammar.Select(x => x.Count).ToArray(), semantic_rules);
        }

        #endregion
    }

    public class ParsingTree
    {
        public class ParsingTreeNode
        {
            public string Production;
            public string Contents;
            public object UserContents;
            public int ProductionRuleIndex;
            public ParsingTreeNode Parent;
            public List<ParsingTreeNode> Childs;
            public Action<LPModule, LPFunction, LPBasicBlock, ParsingTreeNode> Action;

            public static ParsingTreeNode NewNode()
                => new ParsingTreeNode { Parent = null, Childs = new List<ParsingTreeNode>() };
            public static ParsingTreeNode NewNode(string production)
                => new ParsingTreeNode { Parent = null, Childs = new List<ParsingTreeNode>(), Production = production };
            public static ParsingTreeNode NewNode(string production, string contents)
                => new ParsingTreeNode { Parent = null, Childs = new List<ParsingTreeNode>(), Production = production, Contents = contents };
        }

        public ParsingTreeNode root;

        public ParsingTree(ParsingTreeNode root)
        {
            this.root = root;
        }

        static void inner_print(StringBuilder builder, ParsingTreeNode node, string indent, bool last)
        {
            builder.Append(indent);
            if (last)
            {
                builder.Append("+-");
                indent += "  ";
            }
            else
            {
                builder.Append("|-");
                indent += "| ";
            }

            if (node.Childs.Count == 0)
            {
                builder.Append(node.Production + " " + node.Contents + "\r\n");
            }
            else
            {
                builder.Append(node.Production + "\r\n");
            }
            for (int i = 0; i < node.Childs.Count; i++)
                inner_print(builder, node.Childs[i], indent, i == node.Childs.Count - 1);
        }

        public void Print(StringBuilder builder)
        {
            inner_print(builder, root, "", true);
        }
    }

    /// <summary>
    /// Shift-Reduce Parser for LR(1)
    /// </summary>
    public class ShiftReduceParser
    {
        [JsonProperty]
        Dictionary<string, int> symbol_name_index = new Dictionary<string, int>();
        List<string> symbol_index_name = new List<string>();
        Stack<int> state_stack = new Stack<int>();
        Stack<ParsingTree.ParsingTreeNode> treenode_stack = new Stack<ParsingTree.ParsingTreeNode>();
        List<ParserAction> actions;

        // 3       1      2       0
        // Accept? Shift? Reduce? Error?
        [JsonProperty]
        int[][] jump_table;
        [JsonProperty]
        int[][] goto_table;
        [JsonProperty]
        int[][] production;
        [JsonProperty]
        int[] group_table;

        public ShiftReduceParser(Dictionary<string, int> symbol_table, int[][] jump_table, int[][] goto_table, int[] group_table, int[][] production, List<ParserAction> actions)
        {
            symbol_name_index = symbol_table;
            this.jump_table = jump_table;
            this.goto_table = goto_table;
            this.production = production;
            this.group_table = group_table;
            this.actions = actions;
            var l = symbol_table.ToList().Select(x => new Tuple<int, string>(x.Value, x.Key)).ToList();
            l.Sort();
            l.ForEach(x => symbol_index_name.Add(x.Item2));
        }

        bool latest_error;
        bool latest_reduce;
        public bool Accept() => state_stack.Count == 0;
        public bool Error() => latest_error;
        public bool Reduce() => latest_reduce;

        public void Clear()
        {
            latest_error = latest_reduce = false;
            state_stack.Clear();
            treenode_stack.Clear();
        }

        [JsonIgnore]
        public ParsingTree Tree => new ParsingTree(treenode_stack.Peek());

        public string Stack() => string.Join(" ", new Stack<int>(state_stack));

        public void Insert(string token_name, string contents) => Insert(symbol_name_index[token_name], contents);
        public void Insert(int index, string contents)
        {
            if (state_stack.Count == 0)
            {
                state_stack.Push(0);
                latest_error = false;
            }
            latest_reduce = false;

            switch (jump_table[state_stack.Peek()][index])
            {
                case 0:
                    // Panic mode
                    state_stack.Clear();
                    treenode_stack.Clear();
                    latest_error = true;
                    break;

                case 1:
                    // Shift
                    state_stack.Push(goto_table[state_stack.Peek()][index]);
                    treenode_stack.Push(ParsingTree.ParsingTreeNode.NewNode(symbol_index_name[index], contents));
                    break;

                case 2:
                    // Reduce
                    reduce(index);
                    latest_reduce = true;
                    break;

                case 3:
                    // Nothing
                    break;
            }
        }

        public ParsingTree.ParsingTreeNode LatestReduce() => treenode_stack.Peek();
        private void reduce(int index)
        {
            var reduce_production = goto_table[state_stack.Peek()][index];
            var reduce_treenodes = new List<ParsingTree.ParsingTreeNode>();

            // Reduce Stack
            for (int i = 0; i < production[reduce_production].Length; i++)
            {
                state_stack.Pop();
                reduce_treenodes.Insert(0, treenode_stack.Pop());
            }

            state_stack.Push(goto_table[state_stack.Peek()][group_table[reduce_production]]);

            var reduction_parent = ParsingTree.ParsingTreeNode.NewNode(symbol_index_name[group_table[reduce_production]]);
            reduction_parent.ProductionRuleIndex = reduce_production - 1;
            reduce_treenodes.ForEach(x => x.Parent = reduction_parent);
            reduction_parent.Contents = string.Join("", reduce_treenodes.Select(x => x.Contents));
            reduction_parent.Childs = reduce_treenodes;
            treenode_stack.Push(reduction_parent);
            if (actions.Count != 0)
                actions[reduction_parent.ProductionRuleIndex].SemanticAction(reduction_parent);
        }

        public static ShiftReduceParser FromString(string json)
            => JsonConvert.DeserializeObject<ShiftReduceParser>(json);
        public override string ToString()
            => JsonConvert.SerializeObject(this, Formatting.None);
        public string ToCSCode(string class_name)
        {
            var builder = new StringBuilder();
            var indent = "";
            Action up_indent = () => { indent += "    "; };
            Action down_indent = () => { if (indent.Length > 0) indent = indent.Substring(4); };
            Action<string> append = (string s) => { builder.Append($"{indent}{s}\r\n"); };
            append("public class " + class_name);
            append("{");
            up_indent();

            ///////////////////
            append("Dictionary<string, int> symbol_table = new Dictionary<string, int>()");
            append("{");
            up_indent();
            foreach (var st in symbol_name_index)
                append("{" + ('"' + st.Key + '"').PadLeft(symbol_name_index.Select(x => x.Key.Length).Max() + 3) + "," + st.Value.ToString().PadLeft(4) + " },");
            down_indent();
            append("};");
            append("");

            ///////////////////
            append("int[][] jump_table = new int[][] {");
            up_indent();
            foreach (var gt in jump_table)
                append("new int[] {" + string.Join(",", gt.Select(x => x.ToString().PadLeft(4))) + " },");
            down_indent();
            append("};");
            append("");

            ///////////////////
            append("int[][] goto_table = new int[][] {");
            up_indent();
            foreach (var gt in goto_table)
                append("new int[] {" + string.Join(",", gt.Select(x => x.ToString().PadLeft(4))) + " },");
            down_indent();
            append("};");
            append("");

            ///////////////////
            append("int[][] production = new int[][] {");
            up_indent();
            foreach (var gt in production)
                append("new int[] {" + string.Join(",", gt.Select(x => x.ToString().PadLeft(4))) + " },");
            down_indent();
            append("};");
            append("");

            ///////////////////
            append("int[] group_table = new int[] {");
            up_indent();
            append(string.Join(",", group_table.Select(x => x.ToString().PadLeft(4))));
            down_indent();
            append("};");
            append("");

            ///////////////////
            append("public ShiftReduceParser Parser => new ShiftReduceParser(");
            append("    symbol_table, jump_table, goto_table, group_table, production, ");
            append("    Enumerable.Repeat(new ParserAction { SemanticAction = (ParsingTree.ParsingTreeNode node) => { } }, production.Length).ToList());");

            down_indent();
            append("}");
            return builder.ToString();
        }
    }
    
    /// <summary>
    /// Shift-Reduce Parser for LR(1)
    /// </summary>
    public class ExtendedShiftReduceParser
    {
        [JsonProperty]
        Dictionary<string, int> symbol_name_index = new Dictionary<string, int>();
        List<string> symbol_index_name = new List<string>();
        Stack<int> state_stack = new Stack<int>();
        Stack<ParsingTree.ParsingTreeNode> treenode_stack = new Stack<ParsingTree.ParsingTreeNode>();
        List<ParserAction> actions;

        // accept  +      -       0
        // Accept? Shift? Reduce? Error?
        [JsonProperty]
        int[][] table;
        [JsonProperty]
        int[] production;
        [JsonProperty]
        int[] group_table;
        [JsonProperty]
        int accept;

        public ExtendedShiftReduceParser(Dictionary<string, int> symbol_table, int accept_code, int[][] table, int[] group_table, int[] production, List<ParserAction> actions)
        {
            symbol_name_index = symbol_table;
            this.table = table;
            this.production = production;
            this.group_table = group_table;
            this.actions = actions;
            this.accept = accept_code;
            var l = symbol_table.ToList().Select(x => new Tuple<int, string>(x.Value, x.Key)).ToList();
            l.Sort();
            l.ForEach(x => symbol_index_name.Add(x.Item2));
        }

        bool latest_error;
        bool latest_reduce;
        public bool Accept() => state_stack.Count == 0;
        public bool Error() => latest_error;
        public bool Reduce() => latest_reduce;

        public void Clear()
        {
            latest_error = latest_reduce = false;
            state_stack.Clear();
            treenode_stack.Clear();
        }

        [JsonIgnore]
        public ParsingTree Tree => new ParsingTree(treenode_stack.Peek());

        public string Stack() => string.Join(" ", new Stack<int>(state_stack));

        public void Insert(string token_name, string contents) => Insert(symbol_name_index[token_name], contents);
        public void Insert(int index, string contents)
        {
            if (state_stack.Count == 0)
            {
                state_stack.Push(0);
                latest_error = false;
            }
            latest_reduce = false;

            int code = table[state_stack.Peek()][index];

            if (code == accept)
            {
                // Nothing
            }
            else if (code > 0)
            {
                // Shift
                state_stack.Push(table[state_stack.Peek()][index]);
                treenode_stack.Push(ParsingTree.ParsingTreeNode.NewNode(symbol_index_name[index], contents));
            }
            else if (code < 0)
            {
                // Reduce
                reduce(index);
                latest_reduce = true;
            }
            else
            {
                // Panic mode
                state_stack.Clear();
                treenode_stack.Clear();
                latest_error = true;
            }
        }

        public ParsingTree.ParsingTreeNode LatestReduce() => treenode_stack.Peek();
        private void reduce(int index)
        {
            var reduce_production = -table[state_stack.Peek()][index];
            var reduce_treenodes = new List<ParsingTree.ParsingTreeNode>();

            // Reduce Stack
            for (int i = 0; i < production[reduce_production]; i++)
            {
                state_stack.Pop();
                reduce_treenodes.Insert(0, treenode_stack.Pop());
            }

            state_stack.Push(table[state_stack.Peek()][group_table[reduce_production]]);

            var reduction_parent = ParsingTree.ParsingTreeNode.NewNode(symbol_index_name[group_table[reduce_production]]);
            reduction_parent.ProductionRuleIndex = reduce_production - 1;
            reduce_treenodes.ForEach(x => x.Parent = reduction_parent);
            reduction_parent.Contents = string.Join("", reduce_treenodes.Select(x => x.Contents));
            reduction_parent.Childs = reduce_treenodes;
            treenode_stack.Push(reduction_parent);
            if (actions.Count != 0)
                reduction_parent.Action = actions[reduction_parent.ProductionRuleIndex].TravelAction;
        }
        
        public static ShiftReduceParser FromString(string json)
            => JsonConvert.DeserializeObject<ShiftReduceParser>(json);
        public override string ToString()
            => JsonConvert.SerializeObject(this, Formatting.None);
        public string ToCSCode(string class_name)
        {
            var builder = new StringBuilder();
            var indent = "";
            Action up_indent = () => { indent += "    "; };
            Action down_indent = () => { if (indent.Length > 0) indent = indent.Substring(4); };
            Action<string> append = (string s) => { builder.Append($"{indent}{s}\r\n"); };
            append("public class " + class_name);
            append("{");
            up_indent();

            ///////////////////
            append("Dictionary<string, int> symbol_table = new Dictionary<string, int>()");
            append("{");
            up_indent();
            foreach (var st in symbol_name_index)
                append("{" + ('"' + st.Key + '"').PadLeft(symbol_name_index.Select(x => x.Key.Length).Max() + 3) + "," + st.Value.ToString().PadLeft(4) + " },");
            down_indent();
            append("};");
            append("");
            
            ///////////////////
            append("int[][] goto_table = new int[][] {");
            up_indent();
            foreach (var gt in table)
                append("new int[] {" + string.Join(",", gt.Select(x => x.ToString().PadLeft(4))) + " },");
            down_indent();
            append("};");
            append("");

            ///////////////////
            append("int[] production = new int[] {");
            up_indent();
            append(string.Join(",", production.Select(x => x.ToString().PadLeft(4))));
            down_indent();
            append("};");
            append("");

            ///////////////////
            append("int[] group_table = new int[] {");
            up_indent();
            append(string.Join(",", group_table.Select(x => x.ToString().PadLeft(4))));
            down_indent();
            append("};");
            append("");

            ///////////////////
            append("int accept = " + accept + ";");
            append("");

            ///////////////////
            append("public ExtendedShiftReduceParser Parser => new ExtendedShiftReduceParser(");
            append("    symbol_table, accept, goto_table, group_table, production, ");
            append("    Enumerable.Repeat(new ParserAction { SemanticAction = (ParsingTree.ParsingTreeNode node) => { } }, production.Length).ToList());");

            down_indent();
            append("}");
            return builder.ToString();
        }
    }
}