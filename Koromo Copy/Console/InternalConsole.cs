/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Fs;
using Koromo_Copy.Hitomi;
using Koromo_Copy.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// Internal 콘솔 옵션입니다.
    /// </summary>
    public class InternalConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        //[CommandLine("-e", CommandType.ARGUMENTS, Pipe = true, DefaultArgument = true)]
        //public string[] Input;

        [CommandLine("-e", CommandType.ARGUMENTS, ArgumentsCount = 1)]
        public string[] Enumerate;

        [CommandLine("-F", CommandType.OPTION)]
        public bool EnumerateWithForms;
        [CommandLine("-P", CommandType.OPTION)]
        public bool EnumerateWithPrivate;
        [CommandLine("-I", CommandType.OPTION)]
        public bool EnumerateWithInstances;
        [CommandLine("-S", CommandType.OPTION)]
        public bool EnumerateWithStatic;
        [CommandLine("-E", CommandType.OPTION)]
        public bool EnumerateWithMethod;

        [CommandLine("--get", CommandType.ARGUMENTS, ArgumentsCount = 1, Help = "--get \"<path1> <path2> ...\" ")]
        public string[] Get;

        [CommandLine("--call", CommandType.ARGUMENTS, ArgumentsCount = 1)]
        public string[] Call;
    }

    /// <summary>
    /// 파일의 내용을 가져옵니다.
    /// </summary>
    public class InternalConsole : IConsole
    {
        /// <summary>
        /// Internal 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            arguments = CommandLineUtil.SplitCombinedOptions(arguments);
            if (CommandLineUtil.AnyArgument(arguments, "-e"))
            {
                arguments = CommandLineUtil.DeleteArgument(arguments, "-e");
                if (!CommandLineUtil.AnyStrings(arguments))
                    arguments = CommandLineUtil.PushFront(arguments, "");
            }
            arguments = CommandLineUtil.InsertWeirdArguments<InternalConsoleOption>(arguments, true, "-e");
            InternalConsoleOption option = CommandLineParser<InternalConsoleOption>.Parse(arguments);

            if (option.Error)
            {
                Console.Instance.WriteLine(option.ErrorMessage);
                if (option.HelpMessage != null)
                    Console.Instance.WriteLine(option.HelpMessage);
                return false;
            }
            else if (option.Help)
            {
                PrintHelp();
            }
            else if (option.Enumerate != null)
            {
                ProcessEnumerate(option.Enumerate, option.EnumerateWithForms, option.EnumerateWithPrivate, 
                    option.EnumerateWithInstances, option.EnumerateWithStatic, option.EnumerateWithMethod);
            }
            else if (option.Get != null)
            {
                ProcessGet(option.Get, option.EnumerateWithForms, option.EnumerateWithInstances);
            }

            return true;
        }

        bool IConsole.Redirect(string[] arguments, string contents)
        {
            return Redirect(arguments, contents);
        }

        static void PrintHelp()
        {
            Console.Instance.WriteLine(
                "Internal Console\r\n" +
                "\r\n" +
                " -e [-F | -P | -I | -S] <path> : Enumerate method."
                );
        }

        static Dictionary<string, object> instances = new Dictionary<string, object> {
            {"setting", Settings.Instance},
            {"data", HitomiData.Instance},
            {"monitor", Monitor.Instance},
            {"console", Console.Instance},
            {"journal", Journal.Instance},
        };

        /// <summary>
        /// 특정 클래스의 내용을 나열합니다.
        /// </summary>
        /// <param name="e_private"></param>
        /// <param name="e_instance"></param>
        /// <param name="e_static"></param>
        static void ProcessEnumerate(string[] args, bool e_form, bool e_private, bool e_instance, bool e_static, bool e_method)
        {
            var split = args[0].Split(' ');

            bool default_out = false;

            if (split[0] == "" && split.Length == 1)
            {
                default_out = true;
            }

            var list = new List<string>();

            var option = Internal.CommonBinding;
            if (e_private)
                option = Internal.DefaultBinding;
            if (e_static)
                option |= BindingFlags.Static;

            if (default_out && e_form)
            {
                foreach (var f in Application.OpenForms)
                {
                    list.Add(f.GetType().Name);
                }
            }
            else if (e_form)
            {
                if (!e_method)
                {
                    list.AddRange(
                        Internal.enum_recursion(Application.OpenForms[split[0]], split, 1, option)
                        .Select(x => $"{x.Name.PadRight(25)} [{x.FieldType.ToString()}]"));
                }
                else
                {
                    list.AddRange(
                        Internal.enum_methods(Application.OpenForms[split[0]], split, 1, option)
                        .Select(x => $"{x.Name.PadRight(25)} [return:({x.ReturnType.ToString()}), args:({string.Join(", ", x.GetParameters().Select(y => $"{y.Name}: {y.ParameterType.ToString()}"))})]"));
                }
            }

            if (default_out && e_instance)
            {
                foreach (var pair in instances)
                {
                    list.Add(pair.Key);
                }
            }
            else if (e_instance)
            {
                if (!e_method)
                {
                    list.AddRange(
                        Internal.enum_recursion(instances[split[0]], split, 1, option)
                        .Select(x => $"{x.Name.PadRight(25)} [{x.FieldType.ToString()}]"));
                }
                else
                {
                    list.AddRange(
                        Internal.enum_methods(instances[split[0]], split, 1, option)
                        .Select(x => $"{x.Name.PadRight(25)} [return:({x.ReturnType.ToString()}), args:({string.Join(", ", x.GetParameters().Select(y => $"{y.Name}: {y.ParameterType.ToString()}"))})]"));
                }
            }

            list.ForEach(x => Console.Instance.WriteLine(x));
        }

        /// <summary>
        /// 특정 변수의 데이터를 가져옵니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessGet(string[] args, bool e_form, bool e_instance)
        {
            var split = args[0].Split(' ');
            
            if (e_form)
            {
                Console.Instance.WriteLine(Monitor.SerializeObject(Internal.get_recursion(Application.OpenForms[split[0]], split, 1)));
            }
            else if (e_instance)
            {
                Console.Instance.WriteLine(Monitor.SerializeObject(Internal.get_recursion(instances[split[0]], split, 1)));
            }
            
        }
    }
}
