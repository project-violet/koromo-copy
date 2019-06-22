/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Crypto;
using Koromo_Copy.Interface;
using Koromo_Copy.Script.SRCAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static Koromo_Copy.Script.SRCAL.SRCALEngine;

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

        [CommandLine("--extract", CommandType.OPTION, DefaultArgument = true)]
        public bool Extract;
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
            else if (option.Extract)
            {
                ProcessExtract();
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
            CommandLineParser<ScriptConsoleOption>.GetFields().ToList().ForEach(
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
            var danbooru = Encoding.UTF8.GetString(Convert.FromBase64String("IyMNCiMjIEtvcm9tbyBDb3B5IFNSQ0FMIFNjcmlwdA0KIyMNCiMjIERhbmJvb3J1IERvd25sb2FkZXINCiMjDQoNCiMjDQojIyBBdHRyaWJ1dGVzDQojIw0KJFNjcmlwdE5hbWUgPSAiZGFuYm9vcnUtcGFnZXMiDQokU2NyaXB0VmVyc2lvbiA9ICIwLjEiDQokU2NyaXB0QXV0aG9yID0gImRjLWtvcm9tbyINCiRTY3JpcHRGb2xkZXJOYW1lID0gImRhbmJvb3J1Ig0KJFNjcmlwdFJlcXVlc3ROYW1lID0gImRhbmJvb3J1Ig0KJFVSTFNwZWNpZmllciA9ICJodHRwczovL2RhbmJvb3J1LmRvbm1haS51cy8iDQokVXNpbmdEcml2ZXIgPSAwDQoNCiMjDQojIyBQcm9jZWR1cmUNCiMjDQpyZXF1ZXN0X3VybCA9ICRSZXF1ZXN0VVJMDQojIyByZXF1ZXN0X3VybCA9IHVybF9wYXJhbWV0ZXJfdGlkeShyZXF1ZXN0X3VybCwgInBhZ2UiKQ0KbWF4X3BhZ2UgPSAkSW5maW5pdHkNCg0KbG9vcCAoaSA9IDEgdG8gbWF4X3BhZ2UpIFsNCiAgICAjIyAkTG9hZFBhZ2UoY29uY2F0KHJlcXVlc3RfdXJsLCAiJnBhZ2U9IiwgaSkpDQogICAgJExvYWRQYWdlKHVybF9wYXJhbWV0ZXIocmVxdWVzdF91cmwsICJwYWdlIiwgaSkpDQogICAgc3ViX3VybHMgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlszXS9kaXZbMV0vc2VjdGlvblsxXS9kaXZbM10vZGl2WzFdL2FydGljbGVbezEraSoxfV0vYVsxXSwgI2F0dHJbaHJlZl0sICNmcm9udFtodHRwczovL2RhbmJvb3J1LmRvbm1haS51c10iKQ0KDQogICAgZm9yZWFjaCAoc3ViX3VybCA6IHN1Yl91cmxzKSBbDQogICAgICAgICRMb2FkUGFnZShzdWJfdXJsKQ0KICAgICAgICBpbWFnZV91cmwgPSAiIg0KICAgICAgICBpZiAoZXF1YWwoY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsxXS9kaXZbM10vZGl2WzFdL3NlY3Rpb25bMV0vZGl2WzFdL3NwYW5bMV0vYVsxXSwgI2F0dHJbaWRdIilbMF0sICJpbWFnZS1yZXNpemUtbGluayIpKSBbDQogICAgICAgICAgICBpbWFnZV91cmwgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlszXS9kaXZbMV0vc2VjdGlvblsxXS9kaXZbMV0vc3BhblsxXS9hWzFdLCAjYXR0cltocmVmXSIpWzBdDQogICAgICAgIF0gZWxzZSBpZiAoZXF1YWwoY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsxXS9kaXZbM10vZGl2WzFdL3NlY3Rpb25bMV0vZGl2WzJdL3NwYW5bMV0vYVsxXSwgI2F0dHJbaWRdIilbMF0sICJpbWFnZS1yZXNpemUtbGluayIpKSBbDQogICAgICAgICAgICBpbWFnZV91cmwgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlszXS9kaXZbMV0vc2VjdGlvblsxXS9kaXZbMl0vc3BhblsxXS9hWzFdLCAjYXR0cltocmVmXSIpWzBdDQogICAgICAgIF0gZWxzZSBbDQogICAgICAgICAgICBpbWFnZV91cmwgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlszXS9kaXZbMV0vc2VjdGlvblsxXS9zZWN0aW9uWzFdL2ltZ1sxXSwgI2F0dHJbc3JjXSIpWzBdDQogICAgICAgIF0NCiAgICAgICAgZmlsZV9uYW1lID0gc3BsaXQoaW1hZ2VfdXJsLCAiLyIpWy0xXQ0KICAgICAgICAkQXBwZW5kSW1hZ2UoaW1hZ2VfdXJsLCBmaWxlX25hbWUpDQogICAgXQ0KDQogICAgaWYgKGVxdWFsKCRMYXRlc3RJbWFnZXNDb3VudCwgMCkpIFsgDQogICAgICAgICRFeGl0TG9vcCgpDQogICAgXQ0KDQogICAgJENsZWFyZUltYWdlc0NvdW50KCkNCl0NCg0KJFJlcXVlc3REb3dubG9hZCgp"));
            var mangashowme_series = Encoding.UTF8.GetString(Convert.FromBase64String("IyMNCiMjIEtvcm9tbyBDb3B5IFNSQ0FMIFNjcmlwdA0KIyMNCiMjIE1hbmdhc2hvd21lIFNlcmllcyBEb3dubG9hZGVyDQojIw0KDQojIw0KIyMgQXR0cmlidXRlcw0KIyMNCiRTY3JpcHROYW1lID0gIm1hbmdhc2hvd21lLXNlcmllcyINCiRTY3JpcHRWZXJzaW9uID0gIjAuMSINCiRTY3JpcHRBdXRob3IgPSAiZGMta29yb21vIg0KJFNjcmlwdEZvbGRlck5hbWUgPSAibWFuZ2FzaG93bWUiDQokU2NyaXB0UmVxdWVzdE5hbWUgPSAibWFuZ2FzaG93bWUiDQokVVJMU3BlY2lmaWVyID0gImh0dHBzOi8vbWFuZ2FzaG93Lm1lL2Jicy9wYWdlLnBocCINCiRVc2luZ0RyaXZlciA9IDANCg0KIyMNCiMjIFByb2NlZHVyZQ0KIyMNCnJlcXVlc3RfdXJsID0gJFJlcXVlc3RVUkwNCm1heF9wYWdlID0gJEluZmluaXR5DQoNCnRpdGxlID0gY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsxXS9kaXZbMl0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMV0iKVswXQ0Kc3ViX3VybHMgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlsyXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMl0vZGl2WzJdL2RpdlsxXS9kaXZbMV0vZGl2W3sxK2kqMX1dL2FbMV0sICNhdHRyW2hyZWZdIikNCnN1Yl90aXRsZXMgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlsyXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMl0vZGl2WzJdL2RpdlsxXS9kaXZbMV0vZGl2W3sxK2kqMX1dL2FbMV0vZGl2WzFdLCAjaHRleHQiKQ0KDQpsb29wIChpID0gMCB0byBhZGQoY291bnQoc3ViX3VybHMpLCAtMSkpIFsNCiAgICAkTG9hZFBhZ2Uoc3ViX3VybHNbaV0pDQogICAgaW1hZ2VzID0gY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsxXS9kaXZbMl0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vc2VjdGlvblsxXS9kaXZbMV0vZm9ybVsxXS9kaXZbMV0vZGl2W3sxK2kqMX1dL2RpdlsxXSwgI2F0dHJbc3R5bGVdLCAjcmVnZXhbaHR0cHM6Ly9bXlxcKV0qXSIpDQogICAgZm9yZWFjaCAoaW1hZ2UgOiBpbWFnZXMpIFsNCiAgICAgICAgZmlsZW5hbWUgPSBzcGxpdChpbWFnZSwgIi8iKVstMV0NCiAgICAgICAgJEFwcGVuZEltYWdlKGltYWdlLCBjb25jYXQodGl0bGUsICIvIiwgc3ViX3RpdGxlc1tpXSwgIi8iLCBmaWxlbmFtZSkpDQogICAgXQ0KXQ0KDQokUmVxdWVzdERvd25sb2FkKCk="));
            var nozomi = Encoding.UTF8.GetString(Convert.FromBase64String("IyMNCiMjIEtvcm9tbyBDb3B5IFNSQ0FMIFNjcmlwdA0KIyMNCiMjIE5vem9taS5sYSBEb3dubG9hZGVyDQojIw0KDQojIw0KIyMgQXR0cmlidXRlcw0KIyMNCiRTY3JpcHROYW1lID0gIm5vem9taSINCiRTY3JpcHRWZXJzaW9uID0gIjAuMSINCiRTY3JpcHRBdXRob3IgPSAiZGMta29yb21vIg0KJFNjcmlwdEZvbGRlck5hbWUgPSAibm96b21pIg0KJFNjcmlwdFJlcXVlc3ROYW1lID0gIm5vem9taSINCiRVUkxTcGVjaWZpZXIgPSAiaHR0cHM6Ly9ub3pvbWkubGEvIg0KJFVzaW5nRHJpdmVyID0gMQ0KDQojIw0KIyMgUHJvY2VkdXJlDQojIw0KcmVxdWVzdF91cmwgPSAkUmVxdWVzdFVSTA0KbWF4X3BhZ2UgPSBpbnQoY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsxXS9kaXZbMl0vZGl2WzFdL3VsWzFdL2xpWzVdL2FbMV0iKVswXSkNCg0KcHJlZml4ID0gc3BsaXQocmVxdWVzdF91cmwsICItIilbMF0NCg0KbG9vcCAoaSA9IDEgdG8gbWF4X3BhZ2UpIFsNCiAgICAkRHJpdmVyTmV3KCkNCiAgICAkRHJpdmVyTG9hZFBhZ2UoY29uY2F0KHByZWZpeCwgIi0iLCBpLCAiLmh0bWwiKSkNCiAgICBpbWFnZXMgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlsyXS9kaXZbMl0vZGl2W3sxK2kqMX1dL2FbMV0vaW1nWzFdLCAjYXR0cltzcmNdIikNCg0KICAgIGZvcmVhY2ggKGltYWdlIDogaW1hZ2VzKSBbDQogICAgICAgaW1hZ2UgPSBjb25jYXQoImh0dHBzOiIsIGltYWdlKQ0KICAgICAgIGZpbGVuYW1lID0gc3BsaXQoaW1hZ2UsICIvIilbLTFdDQogICAgICAgJEFwcGVuZEltYWdlKGltYWdlLCBmaWxlbmFtZSkNCiAgICBdDQpdDQoNCiRSZXF1ZXN0RG93bmxvYWQoKQ=="));
            var raw_script = nozomi.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
                ).ToList();

            //SRCALParser parser = new SRCALParser();
            //Monitor.SerializeObject(parser.Parse(raw_script));
            SRCALEngine engine = new SRCALEngine();
            engine.ParseScript(raw_script);
            //engine.RunScript("https://mangashow.me/bbs/page.php?hid=manga_detail&manga_name=%EC%99%95%20%EA%B2%8C%EC%9E%84%20%EC%A2%85%EA%B7%B9");
            //engine.RunScript("https://danbooru.donmai.us/posts?page=3&tags=yuri", null);
            engine.RunScript("https://nozomi.la/tag/mikagami_mamizu-1.html", null);
        }

        class extract_model
        {
            public string Version;
            public List<Tuple<string, SRCALAttribute, string, string>> Scripts;
        }
        
        static void ProcessExtract()
        {
            var version = "0";
            var script_dir = Path.Combine(Directory.GetCurrentDirectory(), "script");
            var em = new extract_model();
            em.Version = version;
            em.Scripts = new List<Tuple<string, SRCALAttribute, string, string>>();
            foreach (var file in Directory.GetFiles(script_dir))
            {
                string raw = File.ReadAllText(file);
                
                var raw_script = raw.Split(
                    new[] { Environment.NewLine },
                    StringSplitOptions.None
                    ).ToList();
                var parser = new SRCALParser();
                parser.Parse(raw_script);

                var attribute = new SRCALAttribute();
                attribute.ScriptName = parser.attributes["$ScriptName"];
                attribute.ScriptVersion = parser.attributes["$ScriptVersion"];
                attribute.ScriptAuthor = parser.attributes["$ScriptAuthor"];
                attribute.ScriptFolderName = parser.attributes["$ScriptFolderName"];
                attribute.ScriptRequestName = parser.attributes["$ScriptRequestName"];
                attribute.URLSpecifier = parser.attributes["$URLSpecifier"];
                attribute.UsingDriver = Convert.ToInt32(parser.attributes["$UsingDriver"]) == 0 ? false : true;

                em.Scripts.Add(Tuple.Create(Path.GetFileName(file), attribute, raw.ToBase64(), Hash.GetFileHash(file)));
            }

            string json = JsonConvert.SerializeObject(em, Formatting.Indented);
            using (var fs = new StreamWriter(new FileStream("scripts.json", FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }
    }
}
