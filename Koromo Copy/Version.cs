/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Net;
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
        public string VersionBinary;
        public string PatchNote;
        public int ScriptVersion;
        public List<Tuple<string, DateTime, string>> Notifications;
    }

    public class Version
    {
        public const string Name = "Koromo Copy";
        public static string Text { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string SimpleText { get; } = $"{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}";

        public const string UpdateCheckUrl = "https://raw.githubusercontent.com/dc-koromo/koromo-copy/master/version";
        public static VersionModel LatestVersionModel;

        private static bool already_check = false;
        private static bool update_required = false;

        public static bool UpdateRequired()
        {
            if (already_check) return update_required;
            already_check = true;

            var download = NetCommon.DownloadString(UpdateCheckUrl);
            var net_data = JsonConvert.DeserializeObject<VersionModel>(download);

            LatestVersionModel = net_data;

            int major = Assembly.GetExecutingAssembly().GetName().Version.Major;
            int minor = Assembly.GetExecutingAssembly().GetName().Version.Minor;
            int build = Assembly.GetExecutingAssembly().GetName().Version.Build;
            int revis = Assembly.GetExecutingAssembly().GetName().Version.Revision;

            bool require = false;

            if (net_data.MajorVersion > major)
                require = true;
            else if (net_data.MajorVersion == major && net_data.MinorVersion > minor)
                require = true;

            if (Settings.Instance.Model.SensitiveUpdateCheck)
            {
                if (net_data.MajorVersion == major && net_data.MinorVersion == minor && net_data.BuildVersion > build)
                    require = true;
                else if (net_data.MajorVersion == major && net_data.MinorVersion == minor && net_data.BuildVersion == build && net_data.RevisionVersion > revis)
                    require = true;
            }

            return update_required = require;
        }

        public static bool RequireTidy(string program_path)
        {
            if (File.Exists(program_path + ".tmp"))
            {
                File.Delete(program_path + ".tmp");
                return true;
            }
            return false;
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
            vm.ScriptVersion = 0;
            
            string json = JsonConvert.SerializeObject(vm, Formatting.None);
            using (var fs = new StreamWriter(new FileStream("version", FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }
    }
}
