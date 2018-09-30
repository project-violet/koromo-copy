/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Hik.Sps;
using Koromo_Copy.Interface;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Koromo_Copy.Plugin
{
    [PlugInApplication("Koromo Copy")]
    public class PlugInModel : PlugInBasedApplication<KoromoCopyPlugIn>, KoromoCopyPlugInBasedApplication
    {
        public void Send(string message, bool err)
        {
            if (err == true)
            {
                throw new System.Exception($"An error occurred in '{Name}' plugin. {message}");
            }
            else
            {
                Monitor.Instance.Push($"[{Name} Plugin] {message}");
            }
        }
    }

    /// <summary>
    /// 모든 플러그인을 관리하는 클래스입니다.
    /// </summary>
    public class PlugInManager : ILazy<PlugInManager>
    {
        PlugInModel model;

        public PlugInManager()
        {
            model = new PlugInModel();
            model.PlugInFolder = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "plugin");
            model.LoadPlugIns();
        }

        public List<string> GetLoadedPlugins()
        {
            List<string> result = new List<string>();
            model.PlugIns.ForEach(x => result.Add(x.Name));
            return result;
        }

        public PlugInModel Model { get; }
    }
}
