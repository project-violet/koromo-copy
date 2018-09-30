/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Koromo_Copy
{
    public class VersionModel
    {
        public int MajorVersion;
        public int MinorVersion;
        public int BuildVersion;
        public int RevisionVersion;
        public DateTime UpdateTime;
        public List<Tuple<DateTime, string>> Notifications;
    }

    public class Version
    {
        public const string Name = "Koromo Copy";
        public static string Text { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public const string UpdateCheckUrl = "";
        
        public static bool UpdateRequired()
        {
            return true;
        }

        public static void ExportVersion()
        {
            int major = Assembly.GetExecutingAssembly().GetName().Version.Major;
            int minor = Assembly.GetExecutingAssembly().GetName().Version.Minor;
            int build = Assembly.GetExecutingAssembly().GetName().Version.Build;
            int revis = Assembly.GetExecutingAssembly().GetName().Version.Revision;

            VersionModel vm = new VersionModel();
            vm.MajorVersion = major;
            vm.MinorVersion = minor;
            vm.BuildVersion = build;
            vm.RevisionVersion = revis;
            vm.UpdateTime = DateTime.Now;
            
            string json = JsonConvert.SerializeObject(vm, Formatting.None);
            using (var fs = new StreamWriter(new FileStream("version", FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }
    }
}
