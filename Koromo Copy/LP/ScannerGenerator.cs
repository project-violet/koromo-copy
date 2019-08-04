/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.LP
{
    /// <summary>
    /// Optimized DFA Generator
    /// </summary>
    public class SimpleRegex
    {
        public SimpleRegex(diagram dia) { Diagram = dia; }
        public SimpleRegex(string pattern) { Diagram = build(pattern); }
        public SimpleRegex() { }
        public List<string> build_errors = new List<string>();
        public diagram Diagram;
        public const char e_closure = (char)0;

        public class transition_node
        {
            public int index;

            public bool is_acceptable;

            // will be used scanner-generator
            public string accept_token_name;
            public List<string> accept_token_names;

            /// <summary>
            /// 0: e-closure
            /// 'a-z', 'A-Z', '0-9': shift terminals
            /// [+-*/[]()_=^&$#@!~]
            /// </summary>
            public List<Tuple<char, transition_node>> transition;
        }

        public class diagram
        {
            /// <summary>
            /// Starts node
            /// </summary>
            public transition_node start_node;

            /// <summary>
            /// All nodes
            /// </summary>
            public List<transition_node> nodes;

            public int count_of_vertex;
        }

        public void MakeNFA(string pattern) { Diagram = make_nfa("(" + pattern + ")"); }
        public void OptimizeNFA() { while (opt_nfa(Diagram)) ; }
        public void NFAtoDFA() { Diagram = nfa2dfa(Diagram); }
        public void MinimizeDFA() { opt_dfa(Diagram); }
        public string PrintDiagram() { return print_diagram(Diagram); }

        public static string PrintDiagram(diagram dia) { return print_diagram(dia); }
        public static string PrintGraph(diagram dia) { return print_diagram_for_graphviz(dia); }

        /// <summary>
        /// Try simple-regular-expression to optimized DFA
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private diagram build(string pattern)
        {
            var diagram = make_nfa("(" + pattern + ")");
            while (opt_nfa(diagram)) ;
            var dfa = nfa2dfa(diagram);
            opt_dfa(dfa);
            return dfa;
        }

        /// <summary>
        /// return starts and ends and node_count
        /// </summary>
        /// <param name="list"></param>
        /// <param name="starts"></param>
        /// <param name="ends"></param>
        /// <returns></returns>
        private Tuple<transition_node, transition_node, int> copy_nodes(ref List<transition_node> list, int starts, int ends)
        {
            var jump_count = ends - starts + 1;
            var llist = new List<transition_node>();
            for (int i = 0; i < jump_count; i++)
                llist.Add(new transition_node { transition = new List<Tuple<char, transition_node>>() });
            for (int i = starts; i <= ends; i++)
            {
                llist[i - starts].index = list.Count + i - starts;
                foreach (var ts in list[i].transition)
                    llist[i - starts].transition.Add(new Tuple<char, transition_node>(ts.Item1, llist[ts.Item2.index - starts]));
            }
            for (int i = 0; i < jump_count; i++)
                list.Add(llist[i]);
            return new Tuple<transition_node, transition_node, int>(llist[0], llist.Last(), ends - starts + 1);
        }

        /// <summary>
        /// Try simple-regular-expression to NFA.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private diagram make_nfa(string pattern)
        {
            var first_valid_stack = new Stack<transition_node>();
            var second_valid_stack = new Stack<transition_node>();
            var first_valid_stack_stack = new List<Stack<transition_node>>();
            var second_valid_stack_stack = new List<Stack<transition_node>>();
            var tail_nodes = new Stack<List<transition_node>>();
            var opstack = new Stack<char>();
            var diagram = new diagram();

            var index_count = 0;
            var cur = new transition_node();
            var nodes = new List<transition_node>();

            var depth = 0;

            cur.index = index_count++;
            cur.transition = new List<Tuple<char, transition_node>>();
            diagram.start_node = cur;
            first_valid_stack.Push(cur);
            nodes.Add(cur);

            for (int i = 0; i < pattern.Length; i++)
            {
                switch (pattern[i])
                {
                    case '(':
                        opstack.Push('(');
                        depth++;

                        // Copy stack and push to stack stack
                        first_valid_stack_stack.Add(new Stack<transition_node>(new Stack<transition_node>(first_valid_stack)));
                        second_valid_stack_stack.Add(new Stack<transition_node>(new Stack<transition_node>(second_valid_stack)));
                        second_valid_stack.Push(first_valid_stack.Peek());
                        first_valid_stack.Push(cur);
                        tail_nodes.Push(new List<transition_node>());
                        break;

                    case ')':
                        if (opstack.Count == 0 || opstack.Peek() != '(')
                        {
                            build_errors.Add($"[regex] {i} no opener!");
                            return null;
                        }
                        tail_nodes.Peek().Add(cur);
                        var ends_point = new transition_node { index = index_count++, transition = new List<Tuple<char, transition_node>>() };
                        cur = ends_point;
                        nodes.Add(cur);

                        // Connect tail nodes
                        foreach (var tail_node in tail_nodes.Peek())
                            tail_node.transition.Add(new Tuple<char, transition_node>(e_closure, cur));
                        tail_nodes.Pop();

                        // Pop from stack stack
                        first_valid_stack = first_valid_stack_stack.Last();
                        first_valid_stack_stack.RemoveAt(first_valid_stack_stack.Count - 1);
                        second_valid_stack = second_valid_stack_stack.Last();
                        second_valid_stack_stack.RemoveAt(second_valid_stack_stack.Count - 1);
                        second_valid_stack.Push(first_valid_stack.Peek());
                        first_valid_stack.Push(cur);

                        depth--;
                        break;

                    case '|':
                        tail_nodes.Peek().Add(cur);
                        cur = first_valid_stack_stack[first_valid_stack_stack.Count - 1].Peek();
                        break;

                    case '?':
                        second_valid_stack.Peek().transition.Add(new Tuple<char, transition_node>(e_closure, cur));
                        break;

                    case '+':
                        var ttc = copy_nodes(ref nodes, second_valid_stack.Peek().index, cur.index);
                        cur.transition.Add(new Tuple<char, transition_node>(e_closure, ttc.Item1));
                        ttc.Item2.transition.Add(new Tuple<char, transition_node>(e_closure, cur));
                        index_count += ttc.Item3;
                        break;

                    case '*':
                        second_valid_stack.Peek().transition.Add(new Tuple<char, transition_node>(e_closure, cur));
                        cur.transition.Add(new Tuple<char, transition_node>(e_closure, second_valid_stack.Peek()));
                        break;

                    case '[':
                        var ch_list = new List<char>();
                        i++;
                        bool inverse = false;
                        if (i < pattern.Length && pattern[i] == '^')
                        {
                            inverse = true;
                            i++;
                        }
                        for (; i < pattern.Length && pattern[i] != ']'; i++)
                        {
                            if (pattern[i] == '\\' && i + 1 < pattern.Length)
                            {
                                if (@"+-?*|()[].=<>/\".Contains(pattern[i + 1]))
                                    ch_list.Add(pattern[++i]);
                                else
                                {
                                    switch (pattern[++i])
                                    {
                                        case 'n':
                                            ch_list.Add('\n');
                                            break;
                                        case 't':
                                            ch_list.Add('\t');
                                            break;
                                        case 'r':
                                            ch_list.Add('\t');
                                            break;

                                        default:
                                            build_errors.Add($"{pattern[i]} escape character not found!");
                                            ch_list.Add(pattern[i]);
                                            break;
                                    }
                                }
                            }
                            else if (i + 2 < pattern.Length && pattern[i + 1] == '-')
                            {
                                for (int j = pattern[i]; j <= pattern[i + 2]; j++)
                                    ch_list.Add((char)j);
                                i += 2;
                            }
                            else
                                ch_list.Add(pattern[i]);
                        }
                        var ends_point2 = new transition_node { index = index_count++, transition = new List<Tuple<char, transition_node>>() };
                        if (inverse)
                        {
                            var set = new bool[128];
                            var nch_list = new List<char>();
                            foreach (var ch2 in ch_list)
                                set[ch2] = true;
                            for (int j = 0; j < 128; j++)
                                if (!set[j])
                                    nch_list.Add((char)j);
                            ch_list.Clear();
                            ch_list = nch_list;
                        }
                        foreach (var ch2 in ch_list)
                        {
                            cur.transition.Add(new Tuple<char, transition_node>(ch2, ends_point2));
                        }
                        cur = ends_point2;
                        nodes.Add(cur);
                        if (first_valid_stack.Count != 0)
                        {
                            second_valid_stack.Push(first_valid_stack.Peek());
                        }
                        first_valid_stack.Push(cur);
                        break;

                    case '.':
                        var ends_point3 = new transition_node { index = index_count++, transition = new List<Tuple<char, transition_node>>() };
                        for( int i2 = 0; i2 < 128; i2++)
                        {
                            cur.transition.Add(new Tuple<char, transition_node>((char)i2, ends_point3));
                        }
                        cur = ends_point3;
                        nodes.Add(cur);
                        if (first_valid_stack.Count != 0)
                        {
                            second_valid_stack.Push(first_valid_stack.Peek());
                        }
                        first_valid_stack.Push(cur);
                        break;

                    case '\\':
                    default:
                        char ch = pattern[i];
                        if (pattern[i] == '\\')
                        {
                            i++;
                            if (@"+-?*|()[].=<>/".Contains(pattern[i]))
                                ch = pattern[i];
                            else
                            {
                                switch (pattern[i])
                                {
                                    case 'n':
                                        ch = '\n';
                                        break;
                                    case 't':
                                        ch = '\t';
                                        break;
                                    case 'r':
                                        ch = '\r';
                                        break;

                                    default:
                                        build_errors.Add($"{pattern[i]} escape character not found!");
                                        ch = pattern[i];
                                        break;
                                }

                            }
                        }
                        var etn = new transition_node { index = index_count++, transition = new List<Tuple<char, transition_node>>() };
                        cur.transition.Add(new Tuple<char, transition_node>(e_closure, etn));
                        cur = etn;
                        nodes.Add(cur);
                        if (first_valid_stack.Count != 0)
                        {
                            second_valid_stack.Push(first_valid_stack.Peek());
                        }
                        first_valid_stack.Push(cur);
                        var tn = new transition_node { index = index_count++, transition = new List<Tuple<char, transition_node>>() };
                        cur.transition.Add(new Tuple<char, transition_node>(ch, tn));
                        cur = tn;
                        nodes.Add(cur);
                        if (first_valid_stack.Count != 0)
                        {
                            second_valid_stack.Push(first_valid_stack.Peek());
                        }
                        first_valid_stack.Push(cur);
                        break;
                }
            }
            diagram.count_of_vertex = index_count;
            diagram.nodes = nodes;
            nodes.Where(x => x.transition.Count == 0).ToList().ForEach(y => y.is_acceptable = true);
            return diagram;
        }

        /// <summary>
        /// Diagram to string
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static string print_diagram(diagram d)
        {
            var builder = new StringBuilder();
            var stack = new Stack<transition_node>();
            var check = new List<bool>(d.count_of_vertex);
            check.AddRange(Enumerable.Repeat(false, d.count_of_vertex));

            stack.Push(d.start_node);

            while (stack.Count != 0)
            {
                var tn = stack.Pop();
                if (check[tn.index]) continue;
                check[tn.index] = true;

                builder.Append($"{tn.index.ToString().PadLeft(4)}: ");
                foreach (var j in tn.transition)
                    builder.Append($"({(j.Item1 == 0 ? "null" : j.Item1.ToString())},{j.Item2.index}) ");
                if (tn.transition.Count == 0 || tn.is_acceptable == true)
                {
                    if (tn.accept_token_names == null)
                        builder.Append($"(ACCEPT,{tn.accept_token_name})");
                    else
                        builder.Append($"(ACCEPT,{string.Join(",", tn.accept_token_names)})");
                }
                builder.Append('\n');

                tn.transition.ForEach(x => stack.Push(x.Item2));
            }

            return builder.ToString();
        }

        /// <summary>
        /// GraphViz.Net(Jamie Dixon), Microsoft.Bcl.Immutable(Microsoft) 누겟 패키지 설치 필요
        /// 
        /// App.config 파일 수정해야함
        /// <?xml version="1.0" encoding="utf-8"?>
        /// <configuration>
        ///     <startup> 
        ///         <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/>
        ///     </startup>
        ///     <appSettings>
        ///       <add key="graphVizLocation" value="C:\Program Files (x86)\Graphviz2.38\bin"/>
        ///     </appSettings>
        /// </configuration>
        /// 
        /// public class Graph
        /// {
        ///     public static Bitmap ToImage(string str)
        ///     {
        ///         var getStartProcessQuery = new GetStartProcessQuery();
        ///         var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
        ///         var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
        ///         
        ///         var wrapper = new GraphGeneration(getStartProcessQuery,
        ///                                           getProcessStartInfoQuery,
        ///                                           registerLayoutPluginCommand);
        /// 
        ///         byte[] output = wrapper.GenerateGraph(str /*"digraph{a -> b; b -> c; c -> a;}"*/, Enums.GraphReturnType.Png);
        /// 
        ///         return ByteToImage(output);
        ///     }
        /// 
        /// 
        ///     private static Bitmap ByteToImage(byte[] blob)
        ///     {
        ///         MemoryStream mStream = new MemoryStream();
        ///         byte[] pData = blob;
        ///         mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
        ///         Bitmap bm = new Bitmap(mStream, false);
        ///         mStream.Dispose();
        ///         return bm;
        ///     }
        /// 
        /// }
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static string print_diagram_for_graphviz(diagram d)
        {
            var builder = new StringBuilder();
            var used = new HashSet<int>();

            var stack_used = new Stack<transition_node>();
            var check_used = new List<bool>(d.count_of_vertex);
            check_used.AddRange(Enumerable.Repeat(false, d.count_of_vertex));

            stack_used.Push(d.start_node);
            used.Add(d.start_node.index);

            while (stack_used.Count != 0)
            {
                var tn = stack_used.Pop();
                if (check_used[tn.index]) continue;
                check_used[tn.index] = true;

                used.Add(tn.index);

                tn.transition.ForEach(x => stack_used.Push(x.Item2));
            }

            builder.Append("digraph finite_state_machine {\r\n");
            builder.Append("    rankdir=LR;\r\n");
            builder.Append("    size=\"20,30\"\r\n");

            // print doublecircle
            builder.Append("    node [shape = doublecircle]; ");
            foreach (var dd in d.nodes)
                if (dd.is_acceptable && used.Contains(dd.index))
                    builder.Append(dd.index + "; ");
            builder.Append("\r\n");

            // print point
            builder.Append("    node [shape = point]; ss\r\n");
            
            // print circle
            builder.Append("    node [shape = circle];\r\n");

            var stack = new Stack<transition_node>();
            var check = new List<bool>(d.count_of_vertex);
            check.AddRange(Enumerable.Repeat(false, d.count_of_vertex));

            stack.Push(d.start_node);
            builder.Append($"    ss -> {d.start_node.index}");

            while (stack.Count != 0)
            {
                var tn = stack.Pop();
                if (check[tn.index]) continue;
                check[tn.index] = true;
                
                foreach (var j in tn.transition)
                {
                    string v = "";
                    if (j.Item1 == e_closure)
                        v = "&epsilon;";
                    else if (j.Item1 == '"')
                        v = "\"";
                    else if (j.Item1 == '\n')
                        v = "\\n";
                    else if (j.Item1 == '\r')
                        v = "\\r";
                    else if (j.Item1 == '\t')
                        v = "\\t";
                    else
                        v = new string(j.Item1, 1);

                    builder.Append($@"    {tn.index} -> {j.Item2.index} [ label = ""{v}"" ];" + "\r\n");
                }

                tn.transition.ForEach(x => stack.Push(x.Item2));
            }

            builder.Append("}");

            return builder.ToString();
        }

        /// <summary>
        /// Get inverse array of diagram nodes
        /// </summary>
        /// <param name="dia"></param>
        /// <returns></returns>
        private Dictionary<int, HashSet<int>> get_inverse_transtition(diagram dia)
        {
            var inverse_transition = new Dictionary<int, HashSet<int>>();
            var check = new List<bool>(dia.count_of_vertex);
            check.AddRange(Enumerable.Repeat(false, dia.count_of_vertex));

            // Build inverse transition map.
            var q = new Queue<transition_node>();
            q.Enqueue(dia.start_node);
            while (q.Count != 0)
            {
                var tn = q.Dequeue();
                if (check[tn.index]) continue;
                check[tn.index] = true;

                foreach (var j in tn.transition)
                    if (inverse_transition.ContainsKey(j.Item2.index))
                        inverse_transition[j.Item2.index].Add(tn.index);
                    else
                        inverse_transition.Add(j.Item2.index, new HashSet<int>() { tn.index });

                tn.transition.ForEach(x => q.Enqueue(x.Item2));
            }

            return inverse_transition;
        }

        /// <summary>
        /// Delete unnecessary e-closure
        /// </summary>
        /// <param name="dia"></param>
        /// <returns></returns>
        private bool opt_nfa(diagram dia)
        {
            var inverse_transition = get_inverse_transtition(dia);
            bool opt = false;

            // Optimize NFA
            var q = new Queue<transition_node>();
            var check = new List<bool>(dia.count_of_vertex);
            check.AddRange(Enumerable.Repeat(false, dia.count_of_vertex));
            q.Enqueue(dia.start_node);
            while (q.Count != 0)
            {
                var tn = q.Dequeue();
                if (check[tn.index]) continue;
                check[tn.index] = true;

                // Delete unnecessary e-closure with pull left
                if (tn.transition.Count == 1 && tn.transition[0].Item1 == 0)
                {
                    var index_left = tn.index;
                    var index_right = tn.transition[0].Item2.index;

                    if (inverse_transition.ContainsKey(index_left))
                        foreach (var inv in inverse_transition[index_left])
                            for (int i = 0; i < dia.nodes[inv].transition.Count; i++)
                            {
                                if (dia.nodes[inv].transition[i].Item2.index == tn.index)
                                {
                                    dia.nodes[inv].transition[i] = new Tuple<char, transition_node>(dia.nodes[inv].transition[i].Item1, dia.nodes[index_right]);
                                    opt = true;
                                }
                            }
                }

                // Delete recursive e-closure
                for (int i = 0; i < tn.transition.Count; i++)
                    if (tn.transition[i].Item1 == 0 && tn.transition[i].Item2.index == tn.index)
                        tn.transition.RemoveAt(i--);

                // Merge rounding e-closure
                for (int i = 0; i < tn.transition.Count; i++)
                    if (tn.transition[i].Item1 == 0)
                        for (int j = 0; j < tn.transition[i].Item2.transition.Count; j++)
                            if (tn.transition[i].Item2.transition[j].Item1 == 0 && tn.transition[i].Item2.transition[j].Item2.index == tn.index)
                            {
                                var index_left = tn.index;
                                var index_right = tn.transition[i].Item2.index;

                                if (tn.transition[i].Item2.is_acceptable)
                                {
                                    tn.is_acceptable = true;
                                    tn.accept_token_name = tn.transition[i].Item2.accept_token_name;
                                }
                                tn.transition[i].Item2.transition.RemoveAt(j--);
                                tn.transition.AddRange(dia.nodes[index_right].transition);

                                foreach (var inv in inverse_transition[index_right])
                                    for (int k = 0; k < dia.nodes[inv].transition.Count; k++)
                                        if (dia.nodes[inv].transition[k].Item2.index == index_right)
                                        {
                                            dia.nodes[inv].transition[k] = new Tuple<char, transition_node>(dia.nodes[inv].transition[k].Item1, tn);
                                        }

                                tn.transition.RemoveAt(i--);
                                opt = true;
                                break;
                            }

                // Delete unnecessary e-closure with pull right
                if (inverse_transition.ContainsKey(tn.index) && inverse_transition[tn.index].Count == 1)
                {
                    var index_left = inverse_transition[tn.index].First();
                    var index_right = tn.index;

                    for (int i = 0; i < dia.nodes[index_left].transition.Count; i++)
                        if (dia.nodes[index_left].transition[i].Item2.index == dia.nodes[index_right].index && dia.nodes[index_left].transition[i].Item1 == 0)
                        {
                            if (dia.nodes[index_left].transition[i].Item2.is_acceptable)
                            {
                                dia.nodes[index_left].is_acceptable = true;
                                dia.nodes[index_left].accept_token_name = dia.nodes[index_left].transition[i].Item2.accept_token_name;
                            }
                            dia.nodes[index_left].transition.RemoveAt(i);
                            dia.nodes[index_left].transition.AddRange(dia.nodes[index_right].transition);
                            opt = true;
                        }
                }

                tn.transition.ForEach(x => q.Enqueue(x.Item2));
            }

            // Accept Backpropagation
            var check2 = new List<bool>(dia.count_of_vertex);
            check2.AddRange(Enumerable.Repeat(false, dia.count_of_vertex));
            var acc_nodes = new Queue<int>();
            dia.nodes.Where(x => x.is_acceptable).ToList().ForEach(d => acc_nodes.Enqueue(d.index));
            // recalculate inverse transtion
            inverse_transition = get_inverse_transtition(dia);

            while (acc_nodes.Count != 0)
            {
                var top = acc_nodes.Dequeue();
                if (check2[top]) continue;
                check2[top] = true;
                dia.nodes[top].is_acceptable = true;
                if (inverse_transition.ContainsKey(top))
                    foreach (var inv in inverse_transition[top])
                        if (dia.nodes[inv].transition.Where(x => x.Item2.index == top).First().Item1 == 0)
                            acc_nodes.Enqueue(inv);
            }

            return opt;
        }

        private string set2str(HashSet<int> hs)
        {
            var list = hs.ToList();
            list.Sort();
            return string.Join(",", list);
        }

        /// <summary>
        /// NFA to DFA
        /// </summary>
        /// <param name="dia"></param>
        private diagram nfa2dfa(diagram dia)
        {
            // tn_name, diagram_index
            var transition_node_index = new Dictionary<string, int>();
            // tn_index, diagram_indexes
            var transition_node_set = new Dictionary<int, HashSet<int>>();
            // tn_index, (tn_attribute, tn_index)
            var transition_node = new Dictionary<int, Dictionary<char, int>>();
            int node_count = 0;

            // Create DFA transitions table
            var q = new Queue<int>(); // tn_index
            q.Enqueue(node_count);
            transition_node_index.Add(dia.start_node.index.ToString(), node_count);
            transition_node_set.Add(node_count, new HashSet<int> { dia.start_node.index });
            transition_node.Add(node_count++, new Dictionary<char, int>());
            while (q.Count != 0)
            {
                var hash = q.Dequeue();
                var hs = transition_node_set[hash];

                // (tn_attribute, diagram_indexes)
                var dic = new Dictionary<char, HashSet<int>>();
                var d_q = new Queue<Tuple<char, int>>(); // diagram indexes


                hs.ToList().ForEach(dd => dia.nodes[dd].transition.ForEach(
                    x => d_q.Enqueue(new Tuple<char, int>(x.Item1, x.Item2.index))));

                // ----------- Expand all e-closure -----------
                var check = new List<bool>(dia.count_of_vertex);
                check.AddRange(Enumerable.Repeat(false, dia.count_of_vertex));
                var e_q = new Queue<int>();
                d_q.ToList().Where(qe => qe.Item1 == 0).ToList().ForEach(qee => { e_q.Enqueue(qee.Item2); });

                foreach (var qe in d_q)
                {
                    if (qe.Item1 == 0)
                        e_q.Enqueue(qe.Item2);
                    else
                        check[qe.Item2] = true;
                }

                while (e_q.Count != 0)
                {
                    var d = e_q.Dequeue();
                    if (check[d]) continue;
                    check[d] = true;
                    foreach (var tns in dia.nodes[d].transition)
                        if (tns.Item1 == 0)
                            e_q.Enqueue(tns.Item2.index);
                        else
                            d_q.Enqueue(new Tuple<char, int>(tns.Item1, tns.Item2.index));
                }
                // --------------------------------------------

                // ----------- Collect  transitions -----------
                while (d_q.Count != 0)
                {
                    var dd = d_q.Dequeue();
                    if (dd.Item1 == 0) continue;
                    if (dic.ContainsKey(dd.Item1))
                        dic[dd.Item1].Add(dd.Item2);
                    else
                        dic.Add(dd.Item1, new HashSet<int> { dd.Item2 });
                    foreach (var node in dia.nodes[dd.Item2].transition)
                        if (node.Item1 == 0)
                            dic[dd.Item1].Add(node.Item2.index);
                }

                foreach (var p in dic)
                {
                    var hash_string = set2str(p.Value);
                    if (!transition_node_index.ContainsKey(hash_string))
                    {
                        transition_node_index.Add(hash_string, node_count);
                        transition_node_set.Add(node_count, p.Value);
                        transition_node.Add(node_count, new Dictionary<char, int>());
                        q.Enqueue(node_count++);
                    }
                    var hash_index = transition_node_index[hash_string];
                    transition_node[hash].Add(p.Key, hash_index);
                }
                // --------------------------------------------
            }

            // Build DFA diagram
            var diagram = new diagram();
            var transition_node_list = new List<transition_node>();
            var acc_nodes = new Dictionary<int, string>();

            for (int i = 0; i < transition_node.Count; i++)
                transition_node_list.Add(new transition_node { index = i, transition = new List<Tuple<char, SimpleRegex.transition_node>>() });

            dia.nodes.Where(x => x.is_acceptable).ToList().ForEach(d => acc_nodes.Add(d.index, d.accept_token_name));

            foreach (var p in transition_node)
            {
                foreach (var ts in p.Value)
                    transition_node_list[p.Key].transition.Add(new Tuple<char, transition_node>(ts.Key, transition_node_list[ts.Value]));
                foreach (var hh in transition_node_set[p.Key])
                    if (acc_nodes.ContainsKey(hh))
                    {
                        transition_node_list[p.Key].is_acceptable = true;
                        transition_node_list[p.Key].accept_token_name = acc_nodes[hh];
                        break;
                    }
            }
            
            diagram.count_of_vertex = transition_node_list.Count;
            diagram.nodes = transition_node_list;
            diagram.start_node = transition_node_list[0];
            return diagram;
        }

        private string dic2str(SortedDictionary<char, int> dic)
        {
            return string.Join(",", dic.ToList().Select(x => $"({x.Key},{x.Value})"));
        }
        
        /// <summary>
        /// Minimization DFA using Hopcroft Algorithm
        /// </summary>
        /// <param name="dia"></param>
        /// <returns></returns>
        private void opt_dfa(diagram dia)
        {
            var visit = new HashSet<string>();
            var queue = new Queue<List<int>>();

            // Enqueue Nodes
            var acc_nodes = new List<int>();
            var nacc_nodes = new List<int>();
            foreach (var node in dia.nodes)
                if (node.is_acceptable && node.accept_token_names == null)
                    acc_nodes.Add(node.index);
                else
                    nacc_nodes.Add(node.index);

            queue.Enqueue(acc_nodes);
            queue.Enqueue(nacc_nodes);

            var color = new List<int>();
            var color_count = 1;
            color.AddRange(Enumerable.Repeat(0, dia.count_of_vertex));

            acc_nodes.ForEach(x => color[x] = color_count);
            color_count = 2;

#if true    // For distingushiable states
            var dict_dist = new Dictionary<string, List<int>>();
            foreach (var node in dia.nodes)
                if (node.is_acceptable && node.accept_token_names != null)
                    if (dict_dist.ContainsKey(node.accept_token_names[0]))
                        dict_dist[node.accept_token_names[0]].Add(node.index);
                    else
                        dict_dist.Add(node.accept_token_names[0], new List<int> { node.index });

            foreach (var dist in dict_dist)
            {
                foreach (var dd in dist.Value)
                    color[dd] = color_count;
                queue.Enqueue(dist.Value);
                color_count++;
            }
#endif

            while (queue.Count > 0)
            {
                var front = queue.Dequeue();
                front.Sort();
                var str = string.Join(",", front);

                if (visit.Contains(str)) continue;
                visit.Add(str);
                
                // Collect transition color
                var dic = new Dictionary<int, SortedDictionary<char, int>>();
                foreach (var index in front)
                {
                    var node = dia.nodes[index];
                    foreach (var ts in node.transition)
                    {
                        if (!dic.ContainsKey(node.index))
                            dic.Add(node.index, new SortedDictionary<char, int>());
                        dic[node.index].Add(ts.Item1, color[ts.Item2.index]);
                    }
                }

                var list = dic.ToList();
                var group = new Dictionary<string, List<int>>();
                for (int i = 0; i < list.Count; i++)
                {
                    var ds = dic2str(list[i].Value);
                    if (!group.ContainsKey(ds))
                        group.Add(ds, new List<int>());
                    group[ds].Add(list[i].Key);
                }

                foreach (var gi in group)
                {
                    queue.Enqueue(gi.Value);
                    gi.Value.ForEach(x => color[x] = color_count);
                    color_count++;
                }
            }

            var dicc = new Dictionary<int, int>();
            var inverse_transition = get_inverse_transtition(dia);
            for (int i = 0; i < color.Count; i++)
                if (!dicc.ContainsKey(color[i]))
                    dicc.Add(color[i], i);
                else if (inverse_transition.ContainsKey(i))
                {
                    foreach (var inv in inverse_transition[i])
                        for (int j = 0; j < dia.nodes[inv].transition.Count; j++)
                            if (dia.nodes[inv].transition[j].Item2.index == i)
                                dia.nodes[inv].transition[j] = new Tuple<char, transition_node>(dia.nodes[inv].transition[j].Item1, dia.nodes[dicc[color[i]]]);
                }
        }
    }

    /// <summary>
    /// Lexical Analyzer Generator
    /// </summary>
    public class ScannerGenerator
    {
        bool freeze = false;
        List<Tuple<string, SimpleRegex.diagram>> tokens = new List<Tuple<string, SimpleRegex.diagram>>();
        SimpleRegex.diagram diagram;
        
        public string PrintDiagram()
        {
            if (!freeze) throw new Exception("Retry after generate!");
            return SimpleRegex.PrintDiagram(diagram);
        }

        public void PushRule(string token_name, string rule)
        {
            if (freeze) throw new Exception("You cannot push rule after generate! Please create new scanner-generator instance.");
            var sd = new SimpleRegex(rule);
            foreach (var node in sd.Diagram.nodes)
                if (node.is_acceptable)
                    node.accept_token_name = token_name;
            tokens.Add(new Tuple<string, SimpleRegex.diagram>(token_name, sd.Diagram));
        }

        /// <summary>
        /// Generate merged DFA using stack.
        /// </summary>
        public void Generate()
        {
            freeze = true;

            //                     * Warning *
            //
            // The merged_diagram index order is in the order of  DFA's 
            // pattern mapping. Consider the PushRule function with this.

            var merged_diagram = get_merged_diagram();

            // Generated transition nodes for DFA based patttern matching.
            var diagram = new SimpleRegex.diagram();
            var nodes = new List<SimpleRegex.transition_node>();
            var states = new Dictionary<string, SimpleRegex.transition_node>();
            var index = new Dictionary<int, string>();
            var states_count = 0;

            // (diagram_indexes)
            var q = new Queue<List<int>>();
            q.Enqueue(populate(merged_diagram, new List<int> { 0 }, SimpleRegex.e_closure));

            var t = new SimpleRegex.transition_node { index = states_count++, transition = new List<Tuple<char, SimpleRegex.transition_node>>() };
            states.Add(string.Join(",", q.Peek()), t);
            index.Add(t.index, string.Join(",", q.Peek()));
            nodes.Add(t);

            while (q.Count != 0)
            {
                var list = q.Dequeue();
                var list2str = string.Join(",", list);

                var tn = states[list2str];

                // Append accept tokens.
                foreach (var ix in list)
                    if (merged_diagram.nodes[ix].is_acceptable)
                    {
                        tn.is_acceptable = true;
                        if (tn.accept_token_names == null)
                            tn.accept_token_names = new List<string>();
                        tn.accept_token_names.Add(merged_diagram.nodes[ix].accept_token_name);
                    }

                var available = available_matches(merged_diagram, list);

                foreach (var pair in available)
                {
                    var populate = pair.Value.ToList();
                    var l2s = string.Join(",", populate);

                    if (!states.ContainsKey(l2s))
                    {
                        var tnt = new SimpleRegex.transition_node { index = states_count++, transition = new List<Tuple<char, SimpleRegex.transition_node>>() };
                        states.Add(l2s, tnt);
                        index.Add(tnt.index, l2s);
                        nodes.Add(tnt);
                        q.Enqueue(populate);
                    }

                    var state = states[l2s];
                    tn.transition.Add(new Tuple<char, SimpleRegex.transition_node>(pair.Key, state));
                }
            }

            diagram.nodes = nodes;
            diagram.start_node = nodes[0];
            diagram.count_of_vertex = nodes.Count;
            
            this.diagram = diagram;
        }

        /// <summary>
        /// Get merged diagram with e-closure
        /// </summary>
        /// <returns></returns>
        private SimpleRegex.diagram get_merged_diagram()
        {
            var merged_diagram = new SimpleRegex.diagram { nodes = new List<SimpleRegex.transition_node>() };
            merged_diagram.nodes.Add(new SimpleRegex.transition_node { index = 0, transition = new List<Tuple<char, SimpleRegex.transition_node>>() });
            merged_diagram.start_node = merged_diagram.nodes[0];

            // Append diagrams
            foreach (var token in tokens)
            {
                var count = merged_diagram.nodes.Count;
                for (int i = 0; i < token.Item2.nodes.Count; i++)
                    token.Item2.nodes[i].index += count;
                merged_diagram.nodes.AddRange(token.Item2.nodes);
                merged_diagram.start_node.transition.Add(new Tuple<char, SimpleRegex.transition_node>(SimpleRegex.e_closure, merged_diagram.nodes[count]));
            }

            merged_diagram.count_of_vertex = merged_diagram.nodes.Count;
            return merged_diagram;
        }

        /// <summary>
        /// Populate with next token.
        /// </summary>
        /// <param name="dia"></param>
        /// <param name="diagram_indexes"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        private List<int> populate(SimpleRegex.diagram dia, List<int> diagram_indexes, char match)
        {
            var result = new List<int>();
            foreach (var index in diagram_indexes)
                foreach (var transition in dia.nodes[index].transition)
                    if (transition.Item1 == match)
                        result.Add(transition.Item2.index);
            return result;
        }

        /// <summary>
        /// Get available next matches.
        /// </summary>
        /// <param name="dia"></param>
        /// <param name="diagram_indexes"></param>
        /// <returns></returns>
        private Dictionary<char, List<int>> available_matches(SimpleRegex.diagram dia, List<int> diagram_indexes)
        {
            // (match, (diagram_index, transition_index))
            var result = new Dictionary<char, List<int>>();
            foreach (var index in diagram_indexes)
                for (int i = 0; i < dia.nodes[index].transition.Count; i++)
                {
                    if (!result.ContainsKey(dia.nodes[index].transition[i].Item1))
                        result.Add(dia.nodes[index].transition[i].Item1, new List<int>());
                    result[dia.nodes[index].transition[i].Item1].Add(dia.nodes[index].transition[i].Item2.index);
                }
            return result;
        }

        public Scanner CreateScannerInstance(string delimiter = "\n\r ")
        {
            if (!freeze) throw new Exception("Retry after generate!");

            var table = new int[diagram.count_of_vertex][];
            var accept_table = new string[diagram.count_of_vertex];
            for (int i = 0; i < table.Length; i++)
            {
                table[i] = new int[255];
                for (int j = 0; j < 255; j++)
                    table[i][j] = -1;
            }

            // Fill transitions
            for (int i = 0; i < diagram.nodes.Count; i++)
                foreach (var transition in diagram.nodes[i].transition)
                    table[i][transition.Item1] = transition.Item2.index;

            // Fill accept table
            for (int i = 0; i < diagram.nodes.Count; i++)
                if (diagram.nodes[i].accept_token_names != null)
                    accept_table[i] = diagram.nodes[i].accept_token_names[0];

            return new Scanner(table, accept_table);
        }
    }

    /// <summary>
    /// Simple Scanner
    /// </summary>
    public class Scanner
    {
        int[][] transition_table;
        string[] accept_table;
        string target;
        int pos = 0;
        bool err = false;
        int latest_pos;
        List<int> err_pos;
        int current_line;
        int current_column;

        public Scanner(int[][] transition_table, string[] accept_table)
        {
            this.transition_table = transition_table;
            this.accept_table = accept_table;
        }

        public void AllocateTarget(string literal)
        {
            target = literal;
            pos = 0;
            current_line = 0;
            current_column = 0;
            err_pos = new List<int>();
            err = false;
        }

        public bool Valid()
        {
            return pos < target.Length;
        }

        public bool Error()
        {
            return err;
        }

        public int Position { get { return latest_pos; } }
        public int Line { get { return current_line; } set { current_line = value; } }
        public int Column { get { return current_column; } set { current_column = value; } }

        public Tuple<string, string, int, int> Next()
        {
            var builder = new StringBuilder();
            var node_pos = 0;
            latest_pos = pos;

            int cur_line = current_line;
            int cur_column = current_column;

            for (; pos < target.Length; pos++)
            {
                int next_transition = transition_table[node_pos][target[pos]];

                switch (next_transition)
                {
                    case -1:
                        // No-name
                        if (accept_table[node_pos] == "")
                        {
                            // Drop string and initialization
                            builder.Clear();
                            latest_pos = pos;
                            pos--;
                            node_pos = 0;
                            current_column--;
                            cur_line = current_line;
                            cur_column = current_column;
                            continue;
                        }
                        if (accept_table[node_pos] == null)
                        {
                            err = true;
                            err_pos.Add(pos);
                            continue;
                        }
                        return new Tuple<string, string, int, int> (accept_table[node_pos], builder.ToString(), cur_line + 1, cur_column + 1);

                    default:
                        if (target[pos] == '\n') { current_line++; current_column = 1; } else current_column++;
                        builder.Append(target[pos]);
                        break;
                }

                node_pos = next_transition;
            }
            if (accept_table[node_pos] == null)
                throw new Exception($"[SCANNER] Pattern not found! L:{cur_line}, C:{cur_column}, D:'{builder.ToString()}'");
            return new Tuple<string, string, int, int> (accept_table[node_pos], builder.ToString(), cur_line + 1, cur_column + 1);
        }

        public Tuple<string, string, int, int> Lookahead()
        {
            var npos = pos;
            var result = Next();
            pos = npos;
            return result;
        }
    }
}