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
using System.Text;

namespace Koromo_Copy.Script
{
    public class ScriptManager : ILazy<ScriptManager>
    {
        List<SRCALScript> scripts = new List<SRCALScript>();

        public readonly static Tuple<string, string>[] builtin_scripts =
        {
            new Tuple<string, string>("danbooru.srcal","IyMNCiMjIEtvcm9tbyBDb3B5IFNSQ0FMIFNjcmlwdA0KIyMNCiMjIERhbmJvb3J1IERvd25sb2FkZXINCiMjDQoNCiMjDQojIyBBdHRyaWJ1dGVzDQojIw0KJFNjcmlwdE5hbWUgPSAiZGFuYm9vcnUtcGFnZXMiDQokU2NyaXB0VmVyc2lvbiA9ICIwLjEiDQokU2NyaXB0QXV0aG9yID0gImRjLWtvcm9tbyINCiRTY3JpcHRGb2xkZXJOYW1lID0gImRhbmJvb3J1Ig0KJFNjcmlwdFJlcXVlc3ROYW1lID0gImRhbmJvb3J1Ig0KJFVSTFNwZWNpZmllciA9ICJodHRwczovL2RhbmJvb3J1LmRvbm1haS51cy8iDQokVXNpbmdEcml2ZXIgPSAwDQoNCiMjDQojIyBQcm9jZWR1cmUNCiMjDQpyZXF1ZXN0X3VybCA9ICRSZXF1ZXN0VVJMDQojIyByZXF1ZXN0X3VybCA9IHVybF9wYXJhbWV0ZXJfdGlkeShyZXF1ZXN0X3VybCwgInBhZ2UiKQ0KbWF4X3BhZ2UgPSAkSW5maW5pdHkNCg0KbG9vcCAoaSA9IDEgdG8gbWF4X3BhZ2UpIFsNCiAgICAjIyAkTG9hZFBhZ2UoY29uY2F0KHJlcXVlc3RfdXJsLCAiJnBhZ2U9IiwgaSkpDQogICAgJExvYWRQYWdlKHVybF9wYXJhbWV0ZXIocmVxdWVzdF91cmwsICJwYWdlIiwgaSkpDQogICAgc3ViX3VybHMgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlszXS9kaXZbMV0vc2VjdGlvblsxXS9kaXZbM10vZGl2WzFdL2FydGljbGVbezEraSoxfV0vYVsxXSwgI2F0dHJbaHJlZl0sICNmcm9udFtodHRwczovL2RhbmJvb3J1LmRvbm1haS51c10iKQ0KDQogICAgZm9yZWFjaCAoc3ViX3VybCA6IHN1Yl91cmxzKSBbDQogICAgICAgICRMb2FkUGFnZShzdWJfdXJsKQ0KICAgICAgICBpbWFnZV91cmwgPSAiIg0KICAgICAgICBpZiAoZXF1YWwoY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsxXS9kaXZbM10vZGl2WzFdL3NlY3Rpb25bMV0vZGl2WzFdL3NwYW5bMV0vYVsxXSwgI2F0dHJbaWRdIilbMF0sICJpbWFnZS1yZXNpemUtbGluayIpKSBbDQogICAgICAgICAgICBpbWFnZV91cmwgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlszXS9kaXZbMV0vc2VjdGlvblsxXS9kaXZbMV0vc3BhblsxXS9hWzFdLCAjYXR0cltocmVmXSIpWzBdDQogICAgICAgIF0gZWxzZSBpZiAoZXF1YWwoY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsxXS9kaXZbM10vZGl2WzFdL3NlY3Rpb25bMV0vZGl2WzJdL3NwYW5bMV0vYVsxXSwgI2F0dHJbaWRdIilbMF0sICJpbWFnZS1yZXNpemUtbGluayIpKSBbDQogICAgICAgICAgICBpbWFnZV91cmwgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlszXS9kaXZbMV0vc2VjdGlvblsxXS9kaXZbMl0vc3BhblsxXS9hWzFdLCAjYXR0cltocmVmXSIpWzBdDQogICAgICAgIF0gZWxzZSBbDQogICAgICAgICAgICBpbWFnZV91cmwgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlszXS9kaXZbMV0vc2VjdGlvblsxXS9zZWN0aW9uWzFdL2ltZ1sxXSwgI2F0dHJbc3JjXSIpWzBdDQogICAgICAgIF0NCiAgICAgICAgZmlsZV9uYW1lID0gc3BsaXQoaW1hZ2VfdXJsLCAiLyIpWy0xXQ0KICAgICAgICAkQXBwZW5kSW1hZ2UoaW1hZ2VfdXJsLCBmaWxlX25hbWUpDQogICAgXQ0KDQogICAgaWYgKGVxdWFsKCRMYXRlc3RJbWFnZXNDb3VudCwgMCkpIFsgDQogICAgICAgICRFeGl0TG9vcCgpDQogICAgXQ0KDQogICAgJENsZWFyZUltYWdlc0NvdW50KCkNCl0NCg0KJFJlcXVlc3REb3dubG9hZCgp"),
            new Tuple<string, string>("mangashowme-series.srcal","IyMNCiMjIEtvcm9tbyBDb3B5IFNSQ0FMIFNjcmlwdA0KIyMNCiMjIE1hbmdhc2hvd21lIFNlcmllcyBEb3dubG9hZGVyDQojIw0KDQojIw0KIyMgQXR0cmlidXRlcw0KIyMNCiRTY3JpcHROYW1lID0gIm1hbmdhc2hvd21lLXNlcmllcyINCiRTY3JpcHRWZXJzaW9uID0gIjAuMiINCiRTY3JpcHRBdXRob3IgPSAiZGMta29yb21vIg0KJFNjcmlwdEZvbGRlck5hbWUgPSAibWFuZ2FzaG93bWUiDQokU2NyaXB0UmVxdWVzdE5hbWUgPSAibWFuZ2FzaG93bWUiDQokVVJMU3BlY2lmaWVyID0gImh0dHBzOi8vbWFuZ2FzaG93Lm1lL2Jicy9wYWdlLnBocCINCiRVc2luZ0RyaXZlciA9IDANCg0KIyMNCiMjIFByb2NlZHVyZQ0KIyMNCnJlcXVlc3RfdXJsID0gJFJlcXVlc3RVUkwNCg0KdGl0bGUgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzFdL2RpdlsyXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsxXSIpWzBdDQpzdWJfdXJscyA9IGNhbCgiL2h0bWxbMV0vYm9keVsxXS9kaXZbMV0vZGl2WzJdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsyXS9kaXZbMl0vZGl2WzFdL2RpdlsxXS9kaXZbezEraSoxfV0vYVsxXSwgI2F0dHJbaHJlZl0iKQ0Kc3ViX3RpdGxlcyA9IGNhbCgiL2h0bWxbMV0vYm9keVsxXS9kaXZbMV0vZGl2WzJdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL2RpdlsyXS9kaXZbMl0vZGl2WzFdL2RpdlsxXS9kaXZbezEraSoxfV0vYVsxXS9kaXZbMV0sICNodGV4dCIpDQoNCiRNZXNzYWdlRmFkZU9uKHRydWUsIGNvbmNhdCh0aXRsZSwgIi4uLlswLyIsIGNvdW50KHN1Yl91cmxzKSwgIl0iKSkNCg0KbG9vcCAoaSA9IDAgdG8gYWRkKGNvdW50KHN1Yl91cmxzKSwgLTEpKSBbDQogICAgJExvYWRQYWdlKHN1Yl91cmxzW2ldKQ0KICAgICRNZXNzYWdlVGV4dChjb25jYXQodGl0bGUsICIuLi5bIiwgYWRkKGksMSksICIvIiwgY291bnQoc3ViX3VybHMpLCAiXSIpKQ0KICAgIGltYWdlcyA9IGNhbCgiL2h0bWxbMV0vYm9keVsxXS9kaXZbMV0vZGl2WzJdL2RpdlsxXS9kaXZbMV0vZGl2WzFdL3NlY3Rpb25bMV0vZGl2WzFdL2Zvcm1bMV0vZGl2WzFdL2Rpdlt7MStpKjF9XS9kaXZbMV0sICNhdHRyW3N0eWxlXSwgI3JlZ2V4W2h0dHBzOi8vW15cXCldKl0iKQ0KICAgIGZvcmVhY2ggKGltYWdlIDogaW1hZ2VzKSBbDQogICAgICAgIGZpbGVuYW1lID0gc3BsaXQoaW1hZ2UsICIvIilbLTFdDQogICAgICAgICRBcHBlbmRJbWFnZShpbWFnZSwgY29uY2F0KHRpdGxlLCAiLyIsIHN1Yl90aXRsZXNbaV0sICIvIiwgZmlsZW5hbWUpKQ0KICAgIF0NCl0NCg0KJE1lc3NhZ2VGYWRlT2ZmKHRydWUsIGNvbmNhdCh0aXRsZSwgIi4uLmNvbXBsZXRlIikpDQoNCiRSZXF1ZXN0RG93bmxvYWQoKQ=="),
            new Tuple<string, string>("dcinside-article.srcal","IyMNCiMjIEtvcm9tbyBDb3B5IFNSQ0FMIFNjcmlwdA0KIyMNCiMjIERDSW5zaWRlIEFydGljbGUgSW1hZ2UgRG93bmxvYWRlcg0KIyMNCg0KIyMNCiMjIEF0dHJpYnV0ZXMNCiMjDQokU2NyaXB0TmFtZSA9ICJkY2luc2lkZS1hcnRpY2xlIg0KJFNjcmlwdFZlcnNpb24gPSAiMC4xIg0KJFNjcmlwdEF1dGhvciA9ICJkYy1rb3JvbW8iDQokU2NyaXB0Rm9sZGVyTmFtZSA9ICJkY2luc2lkZSINCiRTY3JpcHRSZXF1ZXN0TmFtZSA9ICJkY2luc2lkZSINCiRVUkxTcGVjaWZpZXIgPSAiaHR0cDovL2dhbGwuZGNpbnNpZGUuY29tL2JvYXJkL3ZpZXcvIg0KJFVzaW5nRHJpdmVyID0gMA0KDQojIw0KIyMgUHJvY2VkdXJlDQojIw0KcmVxdWVzdF91cmwgPSAkUmVxdWVzdFVSTA0KDQpnYWxsZXJ5X25hbWUgPSBjYWwoIi9odG1sWzFdL2JvZHlbMV0vZGl2WzJdL2RpdlsyXS9tYWluWzFdL3NlY3Rpb25bMV0vaGVhZGVyWzFdL2RpdlsxXS9kaXZbMV0vaDJbMV0vYVsxXSIpDQp0aXRsZSA9IGNhbCgiL2h0bWxbMV0vYm9keVsxXS9kaXZbMl0vZGl2WzJdL21haW5bMV0vc2VjdGlvblsxXS9hcnRpY2xlWzJdL2RpdlsxXS9oZWFkZXJbMV0vZGl2WzFdL2gzWzFdL3NwYW5bMl0iKQ0KaW1hZ2VzID0gY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsyXS9kaXZbMl0vbWFpblsxXS9zZWN0aW9uWzFdL2FydGljbGVbMl0vZGl2WzFdL2RpdlsxXS9kaXZbNl0vdWxbMV0vbGlbezEraSoxfV0vYVsxXSwgI2F0dHJbaHJlZl0iKQ0KZmlsZW5hbWVzID0gY2FsKCIvaHRtbFsxXS9ib2R5WzFdL2RpdlsyXS9kaXZbMl0vbWFpblsxXS9zZWN0aW9uWzFdL2FydGljbGVbMl0vZGl2WzFdL2RpdlsxXS9kaXZbNl0vdWxbMV0vbGlbezEraSoxfV0vYVsxXSIpDQoNCmxvb3AgKGkgPSAwIHRvIGFkZChjb3VudChpbWFnZXMpLCAtMSkpDQogICAgJEFwcGVuZEltYWdlKGltYWdlc1tpXSwgY29uY2F0KGdhbGxlcnlfbmFtZSwgIi8iLCB0aXRsZSwgIi8iLCBmaWxlbmFtZXNbaV0pKQ0KDQokUmVxdWVzdERvd25sb2FkKCk="),
            //new Tuple<string, string>("",""),
        };

        public List<SRCALScript> Scripts { get { return scripts; } }

        public void Initialization()
        {
            var script_dir = Path.Combine(Directory.GetCurrentDirectory(), "script");
            if (!Directory.Exists(script_dir))
            {
                Directory.CreateDirectory(script_dir);
                foreach (var pair in builtin_scripts)
                    File.WriteAllText(Path.Combine(script_dir, pair.Item1), Encoding.UTF8.GetString(Convert.FromBase64String(pair.Item2)));
            }

            foreach (var file in Directory.GetFiles(script_dir))
            {
                Subscribe(File.ReadAllText(file));
            }
        }

        /// <summary>
        /// 스크립트를 등록합니다.
        /// 파싱에 실패할 경우 스크립트가 등록되지 않습니다.
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public bool Subscribe(string script)
        {
            var script_engine = new SRCALScript();
            if (script_engine.AllocScript(script))
            {
                if (ExsitsByName(script_engine.Attribute().ScriptName))
                {
                    Monitor.Instance.Push($"[Script Manager] '{script_engine.Attribute().ScriptName}' script already subsribed!");
                    return true;
                }
                if (string.IsNullOrEmpty(script_engine.Attribute().ScriptFolderName))
                {
                    Monitor.Instance.Push($"[Script Manager] '{script_engine.Attribute().ScriptName}' script folder name must be specified.");
                    return true;
                }
                var test_scr = GetScript(script_engine.Attribute().URLSpecifier);
                if (test_scr != null)
                {
                    Monitor.Instance.Push($"[Script Manager] '{script_engine.Attribute().URLSpecifier}' URL specifier is overlaps already registered item, '{test_scr.Attribute().ScriptName}' script.");
                    return true;
                }
                scripts.Add(script_engine);
                Monitor.Instance.Push($"[Script Manager] Subscribe {script_engine.Attribute().ScriptName} ({script_engine.Attribute().ScriptVersion}) script.");
                return false;
            }
            Monitor.Instance.Push($"[Script Manager] Parse error {script_engine.Attribute().ScriptName} ({script_engine.Attribute().ScriptVersion}) script."); 
            return true;
        }

        /// <summary>
        /// 스크립트 등록을 취소합니다.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int Unsubscribe(string name)
        {
            return scripts.RemoveAll(x => x.Attribute().ScriptName == name);
        }

        /// <summary>
        /// 스크립트의 존재여부를 확인합니다.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ExsitsByName(string name)
        {
            return scripts.Any(x => x.Attribute().ScriptName == name);
        }

        /// <summary>
        /// 특정 URL을 분석할 스크립트가 있는지 확인합니다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool SpecifyScript(string url)
        {
            return scripts.Any(x => x.SpecifyURL(url));
        }

        /// <summary>
        /// 특정 URL을 통해 식별된 스크립트를 가져옵니다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public SRCALScript GetScript(string url)
        {
            foreach (var engine in scripts)
                if (engine.SpecifyURL(url))
                    return engine;
            return null;
        }
    }
}
