/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koromo_Copy.LP
{
    public class ParserAction
    {
        public Action<ParsingTree.ParsingTreeNode> SemanticAction;
        public static ParserAction Create(Action<ParsingTree.ParsingTreeNode> action)
            => new ParserAction { SemanticAction = action };
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

        public static ParserProduction operator |(ParserProduction p1, ParserProduction p2)
        {
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

    /// <summary>
    /// LR Parser Generator
    /// </summary>
    public class ParserGenerator
    {
        List<ParserProduction> production_rules;
        // (production_index, (priority, is_left_associativity?))
        Dictionary<int, Tuple<int, bool>> shift_reduce_conflict_solve;
        // (production_index, (sub_production_index, (priority, is_left_associativity?)))
        Dictionary<int, Dictionary<int, Tuple<int, bool>>> shift_reduce_conflict_solve_with_production_rule;

        public StringBuilder GlobalPrinter = new StringBuilder();

        public readonly static ParserProduction EmptyString = new ParserProduction { index = -2 };

        public ParserGenerator()
        {
            production_rules = new List<ParserProduction>();
            production_rules.Add(new ParserProduction { index = 0, production_name = "S'" });
            shift_reduce_conflict_solve = new Dictionary<int, Tuple<int, bool>>();
            shift_reduce_conflict_solve_with_production_rule = new Dictionary<int, Dictionary<int, Tuple<int, bool>>>();
        }

        public ParserProduction CreateNewProduction(string name = "", bool is_terminal = true)
        {
            var pp = new ParserProduction { index = production_rules.Count, production_name = name, isterminal = is_terminal };
            production_rules.Add(pp);
            return pp;
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
        
        #region String Hash Function
        // 원래 해시가 아니라 set로 구현해야하는게 일반적임
        // 집합끼리의 비교연산, 일치여부 교집합을 구해 좀 더 최적화가능하지만 귀찮으니 string-hash를 쓰도록한다.

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

        private string i2s(int a, int b, int c)
        {
            return $"{a},{b},{c}";
        }
        #endregion

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
                        if (rule.Last().isterminal == false)
                            foreach (var r in FOLLOW[i])
                                if (rule.Last().index > 0)
                                    FOLLOW[rule.Last().index].Add(r);

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
                        var pp = get_first_on_right_terminal(production_rules[tuple.Item2], tuple.Item3);

                        Tuple<int, bool> p1 = null, p2 = null;

                        if (shift_reduce_conflict_solve.ContainsKey(pp.index))
                            p1 = shift_reduce_conflict_solve[pp.index];
                        if (shift_reduce_conflict_solve.ContainsKey(tuple.Item1))
                            p2 = shift_reduce_conflict_solve[tuple.Item1];

                        if (shift_reduce_conflict_solve_with_production_rule.ContainsKey(tuple.Item2))
                            if (shift_reduce_conflict_solve_with_production_rule[tuple.Item2].ContainsKey(tuple.Item3))
                                p1 = shift_reduce_conflict_solve_with_production_rule[tuple.Item2][tuple.Item3];

                        if (shift_reduce_conflict_solve_with_production_rule.ContainsKey(states[tuple.Item1][0].Item1))
                            if (shift_reduce_conflict_solve_with_production_rule[states[tuple.Item1][0].Item1].ContainsKey(states[tuple.Item1][0].Item2))
                                p2 = shift_reduce_conflict_solve_with_production_rule[states[tuple.Item1][0].Item1][states[tuple.Item1][0].Item2];

                        if (p1 == null || p2 == null)
                            throw new Exception($"Specify the rules to resolve Shift-Reduce Conflict! Target: {production_rules[tuple.Item1].production_name} {pp.production_name}");

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
#if true
                        print_header("SHIFT-REDUCE CONFLICTS");
                        GlobalPrinter.Append($"Shift-Reduce Conflict! {(tuple.Item1 == -1 ? "$" : production_rules[tuple.Item1].production_name)}\r\n");
                        GlobalPrinter.Append($"States: {ms.Key} {small_shift_info[shift_tokens[tuple.Item1]].Item2}\r\n");
                        print_states(ms.Key, states[ms.Key]);
                        print_states(small_shift_info[shift_tokens[tuple.Item1]].Item2, states[small_shift_info[shift_tokens[tuple.Item1]].Item2]);
#endif
                        var pp = get_first_on_right_terminal(production_rules[tuple.Item2], tuple.Item3);

                        Tuple<int, bool> p1 = null, p2 = null;
                        
                        if (shift_reduce_conflict_solve.ContainsKey(pp.index))
                            p1 = shift_reduce_conflict_solve[pp.index];
                        if (shift_reduce_conflict_solve.ContainsKey(tuple.Item1))
                            p2 = shift_reduce_conflict_solve[tuple.Item1];
                        
                        if (shift_reduce_conflict_solve_with_production_rule.ContainsKey(tuple.Item2))
                            if (shift_reduce_conflict_solve_with_production_rule[tuple.Item2].ContainsKey(tuple.Item3))
                                p1 = shift_reduce_conflict_solve_with_production_rule[tuple.Item2][tuple.Item3];

                        //if (shift_reduce_conflict_solve_with_production_rule.ContainsKey(states[tuple.Item1][0].Item1))
                        //    if (shift_reduce_conflict_solve_with_production_rule[states[tuple.Item1][0].Item1].ContainsKey(states[tuple.Item1][0].Item2))
                        //        p2 = shift_reduce_conflict_solve_with_production_rule[states[tuple.Item1][0].Item1][states[tuple.Item1][0].Item2];

                        if (p1 == null || p2 == null)
                            throw new Exception($"Specify the rules to resolve Shift-Reduce Conflict! Target: {production_rules[tuple.Item1].production_name} {pp.production_name}");

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

            number_of_states = merged_states.Count;
        }
        #endregion

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
                        builder.Append("".PadLeft(max_len+2) + "|");
                    }
                }

                builder.Append("\r\n");
            }
            builder.Append(split_line);
            
            print_header("PARSING TABLE");
            GlobalPrinter.Append(builder.ToString() + "\r\n");
        }

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
                    production_rules[p].sub_productions.Where(x => x.Count > 0).ToList().ForEach(y => q.Enqueue(y[0].index));
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
                    if (sub_pr[0].isterminal == false)
                        for (int j = 0; j < production_rules[sub_pr[0].index].sub_productions.Count; j++)
                            first_q.Enqueue(new Tuple<int, int>(sub_pr[0].index, j));
                }
            }
            return first_l;
        }

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

        private ParserProduction get_first_on_right_terminal(ParserProduction pp, int sub_production)
        {
            for (int i = pp.sub_productions[sub_production].Count - 1; i >= 0; i--)
                if (pp.sub_productions[sub_production][i].isterminal)
                    return pp.sub_productions[sub_production][i];
            throw new Exception($"Cannot solve shift-reduce conflict!\r\nProduction Name: {pp.production_name}\r\nProduction Index: {sub_production}");
        }

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
    }

    /// <summary>
    /// Shift-Reduce Parser for LR(1)
    /// </summary>
    public class ShiftReduceParser
    {
        Dictionary<string, int> symbol_name_index = new Dictionary<string, int>();
        List<string> symbol_index_name = new List<string>();
        Stack<int> state_stack = new Stack<int>();
        Stack<ParsingTree.ParsingTreeNode> treenode_stack = new Stack<ParsingTree.ParsingTreeNode>();
        List<ParserAction> actions;

        // 3       1      2       0
        // Accept? Shift? Reduce? Error?
        int[][] jump_table;
        int[][] goto_table;
        int[][] production;
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
            actions[reduction_parent.ProductionRuleIndex].SemanticAction(reduction_parent);
        }
    }
}