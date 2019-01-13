/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Script
{
    public class ScriptModel
    {
        public string ScriptName;
        public string ScriptVersion;
        public string URLSpecifier;
        public string TitleXPath;
        public string ImagesXPath;
        public string FileNameXPath;
    }

    public class ScriptEngine : ILazy<ScriptEngine>
    {
        List<ScriptModel> models = new List<ScriptModel>();

        public void AddScript(string file_contents)
        {
            try
            {
                models.Add(JsonConvert.DeserializeObject<ScriptModel>(file_contents));
                Monitor.Instance.Push($"[Script Engine] Import model.");
                Monitor.Instance.Push(models.Last());
            }
            catch (Exception e)
            {
                Monitor.Instance.Push($"[Script Engine] Error " + e.Message);
            }
        }
    }
}
