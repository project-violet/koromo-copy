/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Koromo_Copy.Console
{
    public enum CommandType
    {
        OPTION,
        ARGUMENTS,
        EQUAL,
    }

    /// <summary>
    /// 커맨드 라인을 분석하기 위한 Attribute 모델입니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class CommandLine : Attribute
    {
        public CommandType CType { get; private set; }
        public string Option { get; private set; }

        /// <summary>
        /// 해당 명령어의 사용법 정보입니다.
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// 인자 규칙이 맞지 않을 경우에 삽입할 기본 인자입니다.
        /// </summary>
        public bool DefaultArgument { get; set; }

        /// <summary>
        /// 파이프를 사용할 변수임을 나타냅니다.
        /// </summary>
        public bool Pipe { get; set; }

        /// <summary>
        /// 명령 구문이 틀렸을때 보여줄 메세지 입니다.
        /// </summary>
        public string Help { get; set; }

        /// <summary>
        /// 파이프에서 아무것도 입력되지 않을 시 이 값이 true로 설정됩니다.
        /// </summary>
        public bool PipeDefault { get; set; } = false;

        /// <summary>
        /// 아무것도 입력되지 않을 시 이 값이 true로 설정됩니다.
        /// </summary>
        public bool Default { get; set; } = false;

        /// <summary>
        /// ARGUMENTS 타입일경우 인자의 개수를 지정합니다.
        /// </summary>
        public int ArgumentsCount { get; set; } = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="option">옵션 토큰입니다.</param>
        /// <param name="type"></param>
        public CommandLine(string option, CommandType type)
        {
            Option = option;
            CType = type;
        }
    }

    /// <summary>
    /// 커맨드 라인을 정리하는 도구 모음입니다.
    /// </summary>
    public class CommandLineUtil
    {
        /// <summary>
        /// 인자배열에 옵션이 있는지 없는지 검사합니다.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool AnyOption(string[] args)
        {
            return args.ToList().Any(x => x[0] == '-');
        }

        /// <summary>
        /// 인자배열에 문자열이 있는지 없는지 검사합니다.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool AnyStrings(string[] args)
        {
            return args.ToList().Any(x => x[0] != '-');
        }

        /// <summary>
        /// 특정 인자가 포함되어있는지의 여부를 가져옵니다.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static bool AnyArgument(string[] args, string arg)
        {
            return args.ToList().Any(x => x == arg);
        }

        /// <summary>
        /// 특정 인자를 삭제합니다.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="arg"></param>
        public static string[] DeleteArgument(string[] args, string arg)
        {
            var list = args.ToList();
            list.Remove(arg);
            return list.ToArray();
        }

        /// <summary>
        /// 특정 옵션을 맨 앞에 넣습니다.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string[] PushFront(string[] args, string option)
        {
            var list = args.ToList();
            list.Insert(0, option);
            return list.ToArray();
        }

        /// <summary>
        /// 특정 옵션을 특정 위치에 넣습니다.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string[] Insert(string[] args, string option, int index)
        {
            var list = args.ToList();
            list.Insert(index, option);
            return list.ToArray();
        }

        /// <summary>
        /// 형식에 어긋난 인자가 있다면 그것을 가져옵니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argv"></param>
        /// <returns></returns>
        public static List<int> GetWeirdArguments<T>(string[] argv)
            where T : IConsoleOption, new()
        {
            var field = CommandLineParser<T>.GetFields();
            List<int> result = new List<int>();

            for (int i = 0; i < argv.Length; i++)
            {
                string token = argv[i].Split('=')[0];
                if (field.ContainsKey(token))
                {
                    var cl = field[token];
                    if (cl.Item2.CType == CommandType.ARGUMENTS)
                        i += cl.Item2.ArgumentsCount;
                }
                else
                {
                    result.Add(i);
                }
            }

            return result;
        }

        /// <summary>
        /// 기본 인자를 삽입합니다.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="pipe"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string[] InsertWeirdArguments<T>(string[] args, bool pipe, string option)
            where T : IConsoleOption, new()
        {
            var weird = GetWeirdArguments<T>(args);

            if (weird.Count > 0 && pipe)
                args = Insert(args, option, weird[0]);

            return args;
        }

        /// <summary>
        /// 합쳐진 인자를 분리합니다.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string[] SplitCombinedOptions(string[] args)
        {
            List<string> result = new List<string>();
            foreach (var arg in args)
            {
                if (arg.Length > 1 && arg.StartsWith("-") && !arg.StartsWith("--") && !arg.Contains("="))
                {
                    for (int i = 1; i < arg.Length; i++)
                        result.Add($"-{arg[i]}");
                }
                else
                {
                    result.Add(arg);
                }
            }
            return result.ToArray();
        }
    }

    /// <summary>
    /// 커맨드 라인 구문분석 도구입니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandLineParser<T>
        where T : IConsoleOption, new()
    {
        /// <summary>
        /// Attribute 정보가 들어있는 필드 정보를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Tuple<string, CommandLine>> GetFields()
        {
            Type type = typeof(T);
            FieldInfo[] fields = type.GetFields();
            var field = new Dictionary<string, Tuple<string, CommandLine>>();

            foreach (FieldInfo m in fields)
            {
                object[] attrs = m.GetCustomAttributes(false);

                foreach (var cl in attrs)
                {
                    var clcast = cl as CommandLine;
                    if (clcast != null)
                        field.Add(clcast.Option, Tuple.Create(m.Name, clcast));
                }
            }

            return field;
        }

        /// <summary>
        /// Attribute를 기반으로 커맨드라인을 분석합니다.
        /// </summary>
        /// <param name="argv"></param>
        /// <param name="pipe"></param>
        /// <returns></returns>
        public static T Parse(string[] argv, bool pipe = false, string contents = "")
        {
            var field = GetFields();
            T result = new T();

            //
            // 옵션이 없는 경우 이 플래그가 활성화됨
            //
            bool any_option = true;

            for (int i = 0; i < argv.Length; i++)
            {
                string token = argv[i].Split('=')[0];
                if (field.ContainsKey(token))
                {
                    var cl = field[token];
                    if (cl.Item2.CType == CommandType.OPTION)
                    {
                        //
                        // OPTION 타입의 경우, 해당 변수를 true로 지정하면 되므로
                        //
                        typeof(T).GetField(cl.Item1).SetValue(result, true);
                    }
                    else if (cl.Item2.CType == CommandType.ARGUMENTS)
                    {
                        List<string> sub_args = new List<string>();

                        int arguments_count = cl.Item2.ArgumentsCount;

                        if (cl.Item2.Pipe == true && pipe == true)
                        {
                            arguments_count--;
                            sub_args.Add(contents);
                        }

                        for (int j = 1; j <= arguments_count; j++)
                        {
                            if (i + j == argv.Length)
                            {
                                typeof(T).GetField("Error").SetValue(result, true);
                                typeof(T).GetField("ErrorMessage").SetValue(result, $"'{argv[i]}' require {arguments_count - j + 1} more sub arguments.");
                                typeof(T).GetField("HelpMessage").SetValue(result, cl.Item2.Help);
                                return result;
                            }

                            sub_args.Add(argv[i + j]);
                        }

                        i += cl.Item2.ArgumentsCount;

                        typeof(T).GetField(cl.Item1).SetValue(result, sub_args.ToArray());
                    }
                    else if (cl.Item2.CType == CommandType.EQUAL)
                    {
                        string[] split = argv[i].Split('=');

                        if (split.Length == 1)
                        {
                            typeof(T).GetField("Error").SetValue(result, true);
                            typeof(T).GetField("ErrorMessage").SetValue(result, $"'{split[0]}' must have equal delimiter.");
                            typeof(T).GetField("HelpMessage").SetValue(result, cl.Item2.Help);
                            return result;
                        }

                        typeof(T).GetField(cl.Item1).SetValue(result, split[1]);
                    }
                    any_option = false;
                }
                else
                {
                    typeof(T).GetField("Error").SetValue(result, true);
                    typeof(T).GetField("ErrorMessage").SetValue(result, $"'{argv[i]}' is not correct arguments.");
                    return result;
                }
            }

            if (any_option)
            {
                //
                // 첫 번째로 만나는 Default를 찾아 활성화시킴
                //
                foreach (var kv in field)
                {
                    if (!pipe && kv.Value.Item2.Default)
                    {
                        typeof(T).GetField(kv.Value.Item1).SetValue(result, true);
                        break;
                    }
                    else if (pipe && kv.Value.Item2.PipeDefault)
                    {
                        typeof(T).GetField(kv.Value.Item1).SetValue(result, new[] { contents });
                        break;
                    }
                }
            }

            return result;
        }
    }
}
