/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Koromo_Copy.Script.SRCAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// Script 콘솔 옵션입니다.
    /// </summary>
    public class ScriptConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("--test", CommandType.OPTION, Pipe = true, DefaultArgument = true)]
        public bool Test;
    }

    /// <summary>
    /// </summary>
    public class ScriptConsole : IConsole
    {
        /// <summary>
        /// Script 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            ScriptConsoleOption option = CommandLineParser<ScriptConsoleOption>.Parse(arguments);

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
            else if (option.Test)
            {
                ProcessTest();
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
                "Script Console\r\n" +
                "\r\n"
                );

            var builder = new StringBuilder();
            CommandLineParser<HitomiConsoleOption>.GetFields().ToList().ForEach(
                x =>
                {
                    if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                        builder.Append($" {x.Key} ({x.Value.Item2.Help}) : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                    else
                        builder.Append($" {x.Key} : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                });
            Console.Instance.WriteLine(builder.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        static void ProcessTest()
        {
            var danbooru = Encoding.UTF8.GetString(Convert.FromBase64String("IyMNCiMjIEtvcm9tbyBDb3B5IFNSQ0FMIFNjcmlwdA0KIyMNCiMjIERhbmJvb3J1IERvd25sb2FkZXINCiMjDQoNCiMjDQojIyBBdHRyaWJ1dGVzDQojIw0KJFNjcmlwdE5hbWUgPSAiZGFuYm9vcnUtcGFnZXMiDQokU2NyaXB0VmVyc2lvbiA9ICIwLjEiDQokU2NyaXB0QXV0aG9yID0gImRjLWtvcm9tbyINCiRTY3JpcHRGb2xkZXJOYW1lID0gImRhbmJvb3J1Ig0KJFNjcmlwdFJlcXVlc3ROYW1lID0gImRhbmJvb3J1Ig0KJFVSTFNwZWNpZmllciA9ICJodHRwczovL2RhbmJvb3J1LmRvbm1haS51cy8iDQokVXNpbmdEcml2ZXIgPSAwDQoNCiMjDQojIyBQcm9jZWR1cmUNCiMjDQpyZXF1ZXN0X3VybCA9ICRSZXF1ZXN0VVJMDQptYXhfcGFnZSA9ICRJbmZpbml0eQ0KDQpsb29wIChpID0gMSB0byBtYXhfcGFnZSkgWw0KICAgICRMb2FkUGFnZSh1cmwocmVxdWVzdF91cmwsICImcGFnZT0iLCBpKSkNCiAgICBzdWJfdXJscyA9IGNhbCgiL2h0bWxbMV0vYm9keVsxXS9kaXZbMV0vZGl2WzNdL2RpdlsxXS9zZWN0aW9uWzFdL2RpdlszXS9kaXZbMV0vYXJ0aWNsZVt7MStpKjF9XS9hWzFdLCAjYXR0cltocmVmXSwgI2Zyb250W2h0dHBzOi8vZGFuYm9vcnUuZG9ubWFpLnVzIikNCg0KICAgIGZvcmVhY2ggKHN1Yl91cmwgOiBzdWJfdXJscykgWw0KICAgICAgICAkTG9hZFBhZ2Uoc3ViX3VybCkNCiAgICAgICAgaWYgKGVxdWFsKGNhbCgiL2h0bWxbMV0vYm9keVsxXS9kaXZbMV0vZGl2WzNdL2RpdlsxXS9zZWN0aW9uWzFdL2RpdlsyXS9zcGFuWzFdL2FbMV0sICNhdHRyW2lkXSIpLCAiaW1hZ2UtcmVzaXplLWxpbmsiKSkgWw0KICAgICAgICAgICAgaW1hZ2VfdXJsID0gY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsxXS9kaXZbM10vZGl2WzFdL3NlY3Rpb25bMV0vZGl2WzJdL3NwYW5bMV0vYVsxXSwgI2F0dHJbaHJlZl0iKQ0KICAgICAgICBdIGVsc2UgWw0KICAgICAgICAgICAgaW1hZ2VfdXJsID0gY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsxXS9kaXZbM10vZGl2WzFdL3NlY3Rpb25bMV0vc2VjdGlvblsxXS9pbWdbMV0sICNhdHRyW3NyY10iKQ0KICAgICAgICBdDQogICAgICAgIGZpbGVfbmFtZSA9IHNwbGl0KGltYWdlX3VybCwgJy8nKVstMV0NCiAgICAgICAgJEFwcGVuZEltYWdlKGltYWdlX3VybCwgZmlsZV9uYW1lKQ0KICAgIF0NCg0KICAgIGlmIChlcXVhbCgkTGF0ZXN0SW1hZ2VzQ291bnQsIDApKSBbIA0KICAgICAgICAkRXhpdExvb3AoKQ0KICAgIF0NCl0NCg0KJFJlcXVlc3REb3dubG9hZCgp"));
            var raw_script = danbooru.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
                ).ToList();

            SRCALParser parser = new SRCALParser();
            Monitor.SerializeObject(parser.Parse(raw_script));
        }
    }
}
