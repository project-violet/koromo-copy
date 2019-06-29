/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Koromo_Copy.Script.SRCAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Koromo_Copy.Script.SRCAL.SRCALEngine;

namespace Koromo_Copy.Script
{
    public class ScriptManager : ILazy<ScriptManager>
    {
        List<SRCALScript> scripts = new List<SRCALScript>();
        
        class extract_model
        {
            public string Version;
            public List<Tuple<string, SRCALAttribute, string, string>> Scripts;
        }

        public List<SRCALScript> Scripts { get { return scripts; } }

        public void Initialization()
        {
            var script_dir = Path.Combine(Directory.GetCurrentDirectory(), "script");
            if (Version.LatestVersionModel.ScriptVersion + 1 > Settings.Instance.Model.ScriptPackageVersion)
            {
                Directory.CreateDirectory(script_dir);

                var scripts = Net.NetCommon.DownloadString("https://raw.githubusercontent.com/dc-koromo/koromo-copy/master/scripts.json");
                var em = JsonConvert.DeserializeObject<extract_model>(scripts);
                foreach (var pair in em.Scripts)
                    File.WriteAllText(Path.Combine(script_dir, pair.Item1), Encoding.UTF8.GetString(Convert.FromBase64String(pair.Item3)));
                Settings.Instance.Model.ScriptPackageVersion = Version.LatestVersionModel.ScriptVersion + 1;
                Settings.Instance.Save();
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
            try { Monitor.Instance.Push($"[Script Manager] Parse error {script_engine.Attribute().ScriptName} ({script_engine.Attribute().ScriptVersion}) script."); } catch { }
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
