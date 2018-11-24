/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Hik.Sps;
using Koromo_Copy.Interface;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Koromo_Copy.Plugin
{
    [PlugInApplication("Koromo Copy")]
    public class PlugInModel : PlugInBasedApplication<KoromoCopyPlugIn>, KoromoCopyPlugInBasedApplication
    {
        public void Send(KoromoCopyPlugIn plugin, string message, bool err)
        {
            if (err == true)
            {
                throw new System.Exception($"An error occurred in '{plugin.Name}' plugin. {message}");
            }
            else
            {
                Monitor.Instance.Push($"[{plugin.Name} Plugin] {message}");
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
            model.PlugIns.Select(x=> x.PlugInProxy).OfType<NonePlugin>().ToList().ForEach(x => x.Send(Version.Text));
        }

        public List<string> GetLoadedPlugins()
        {
            List<string> result = new List<string>();
            model.PlugIns.ForEach(x => result.Add($"{x.PlugInProxy.Name} {x.PlugInProxy.Version}"));
            return result;
        }

        public List<ConsolePlugIn> GetConsolePlugins()
        {
            return model.PlugIns.Select(x => x.PlugInProxy)
                .Where(x => x.Type == KoromoCopyPlugInType.Console)
                .OfType<ConsolePlugIn>().ToList();
        }

        public List<DownloadPlugIn> GetDownloadPlugins()
        {
            return model.PlugIns.Select(x => x.PlugInProxy)
                .Where(x => x.Type == KoromoCopyPlugInType.Download)
                .OfType<DownloadPlugIn>().ToList();
        }

        public PlugInModel Model { get => model; }
    }
}
