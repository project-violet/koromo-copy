/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Pixiv;
using Koromo_Copy.Interface;
using System.Threading.Tasks;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// Pixiv 콘솔 옵션입니다.
    /// </summary>
    public class PixivConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-user", CommandType.ARGUMENTS, DefaultArgument = true,
            Info = "Get user data.")]
        public string[] User;

        [CommandLine("-login", CommandType.ARGUMENTS, ArgumentsCount = 2, 
            Info = "Login Pixiv.")]
        public string[] Login;
        
        [CommandLine("-image", CommandType.ARGUMENTS,
            Info = "Download article images link list.")]
        public string[] ImageLink;
    }

    /// <summary>
    /// 픽시브 콘솔입니다.
    /// </summary>
    public class PixivConsole : IConsole
    {
        /// <summary>
        /// 픽시브 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            arguments = CommandLineUtil.InsertWeirdArguments<PixivConsoleOption>(arguments, true, "-user");
            PixivConsoleOption option = CommandLineParser<PixivConsoleOption>.Parse(arguments);

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
            else if (option.Login != null)
            {
                ProcessLogin(option.Login);
            }
            else if (option.User != null)
            {
                ProcessUser(option.User);
            }
            else if (option.ImageLink != null)
            {
                ProcessImageLink(option.ImageLink);
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
                "Pixiv Console Core\r\n" +
                "\r\n" +
                " -login <Id> <PassWord> : Login on pixiv.\r\n" +
                " -user <Id> : Get user data.\r\n" +
                " -image <Id> : Get Image Link."
                );
        }

        /// <summary>
        /// 로그인을 수행합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessLogin(string[] args)
        {
            if (PixivTool.Instance.IsLogin)
            {
                Console.Instance.WriteErrorLine("Already login.");
                return;
            }

            Console.Instance.GlobalTask.Add(Task.Run(async () => {
                try
                {
                    await PixivTool.Instance.Login(args[0], args[1]);
                    Console.Instance.WriteLine("Succesful login!");
                    Console.Instance.WriteLine($"Access token: {PixivTool.Instance.GetAccessToken()}");
                }
                catch
                {
                    Console.Instance.WriteErrorLine("Login error!");
                }
            }));
        }

        /// <summary>
        /// 유저 정보를 가져옵니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessUser(string[] args)
        {
            if (!PixivTool.Instance.IsLogin)
            {
                Console.Instance.WriteErrorLine("Please login to continue.");
                return;
            }

            Console.Instance.GlobalTask.Add(Task.Run(async () => {
                try
                {
                    Console.Instance.WriteLine(await PixivTool.Instance.GetUserAsync(args[0]));
                }
                catch
                {
                    Console.Instance.WriteErrorLine("User not found!");
                }
            }));
        }

        /// <summary>
        /// 유저 이미지 리스트를 가져옵니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessImageLink(string[] args)
        {
            if (!PixivTool.Instance.IsLogin)
            {
                Console.Instance.WriteErrorLine("Please login to continue.");
                return;
            }

            Console.Instance.GlobalTask.Add(Task.Run(async () => {
                try
                {
                    Console.Instance.WriteLine(Monitor.SerializeObject(await PixivTool.Instance.GetDownloadUrlsAsync(args[0])));
                }
                catch
                {
                    Console.Instance.WriteErrorLine("User not found!");
                }
            }));
        }
    }
}
