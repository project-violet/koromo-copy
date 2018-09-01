/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Koromo_Copy.Console
{
    public enum CommandType
    {
        OPTION,
        ARGUMENTS,
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
    /// 커맨드 라인 구문분석 도구입니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandLineParser<T>
        where T: IConsoleOption, new()
    {
        public static T Parse(string[] argv)
        {
            Type type = typeof(T);
            FieldInfo[] fields = type.GetFields();
            Dictionary<string, Tuple<string, CommandLine>> field = new Dictionary<string, Tuple<string, CommandLine>>();
            
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
            
            T result = new T();

            //
            // 옵션이 없는 경우 이 플래그가 활성화됨
            //
            bool any_option = true;

            for (int i = 0; i < argv.Length; i++)
            {
                if (field.ContainsKey(argv[i]))
                {
                    var cl = field[argv[i]];
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
                        
                        for (int j = 1; j <= cl.Item2.ArgumentsCount; j++)
                        {
                            if (i + j == argv.Length)
                            {
                                typeof(T).GetField("Error").SetValue(result, true);
                                typeof(T).GetField("ErrorMessage").SetValue(result, $"'{argv[i]}' require one sub arguments.");
                                return result;
                            }

                            sub_args.Add(argv[i + j]);
                        }

                        i += cl.Item2.ArgumentsCount;
                        
                        typeof(T).GetField(cl.Item1).SetValue(result, sub_args.ToArray());
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
                    if (kv.Value.Item2.Default)
                    {
                        typeof(T).GetField(kv.Value.Item1).SetValue(result, true);
                        break;
                    }
                }
            }
            
            return result;
        }
    }

}
