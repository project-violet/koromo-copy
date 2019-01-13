/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using HtmlAgilityPack;
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

    public class ScriptResult
    {
        public string Title;
        public List<string> Images;
        public List<string> FileNames;
    }
    
    public class ScriptEngine : ILazy<ScriptEngine>
    {
        List<ScriptModel> models = new List<ScriptModel>();

        public void AddScript(string file_contents)
        {
            try
            {
                AddScript(JsonConvert.DeserializeObject<ScriptModel>(file_contents));
            }
            catch (Exception e)
            {
                Monitor.Instance.Push($"[Script Engine] Error " + e.Message);
            }
        }

        public void AddScript(ScriptModel model)
        {
            models.Add(model);
            Monitor.Instance.Push($"[Script Engine] Import model.");
            Monitor.Instance.Push(models.Last());
        }

        private int cal(string pp, int v)
        {
            // 1. i*b
            // 2. a+i*c
            if (pp.Contains('+'))
                return Convert.ToInt32(pp.Split('+')[0].Trim()) + v * Convert.ToInt32(pp.Split('+')[1].Split('*')[1].Trim());
            return v * Convert.ToInt32(pp.Split('*')[1].Trim());
        }

        private List<string> get(HtmlNode root, string pattern)
        {
            var result = new List<string>();

            try
            {
                if (pattern.Contains('{'))
                {
                    var _split = pattern.Split('{').ToList();
                    _split.RemoveAt(0);
                    var split = _split.Select(x => x.Split('}')[0]);
                    for (int i = 0; ; i++)
                    {
                        var builder = new StringBuilder(pattern);
                        foreach (var sss in split)
                        {
                            builder.Replace("{" + sss + "}", cal(sss, i).ToString());
                        }
                        string attr = "";
                        var pattern2 = builder.ToString();
                        if (pattern2.Contains("#text"))
                        {
                            attr = root.SelectSingleNode(pattern2.Remove(pattern2.IndexOf("/#text"))).InnerText;
                        }
                        else
                        {
                            attr = root.SelectSingleNode(pattern2).GetAttributeValue("src", "");
                            if (string.IsNullOrEmpty(attr))
                                attr = root.SelectSingleNode(pattern2).GetAttributeValue("data-src", "");
                            if (string.IsNullOrEmpty(attr))
                                attr = root.SelectSingleNode(pattern2).GetAttributeValue("href", "");
                            if (string.IsNullOrEmpty(attr))
                                attr = root.SelectSingleNode(pattern2).GetAttributeValue("content", "");
                        }
                        result.Add(attr);
                    }
                }
                else
                {
                    string attr = root.SelectSingleNode(pattern).GetAttributeValue("src", "");
                    if (string.IsNullOrEmpty(attr))
                        attr = root.SelectSingleNode(pattern).GetAttributeValue("data-src", "");
                    if (string.IsNullOrEmpty(attr))
                        attr = root.SelectSingleNode(pattern).GetAttributeValue("href", "");
                    if (string.IsNullOrEmpty(attr))
                        attr = root.SelectSingleNode(pattern).GetAttributeValue("content", "");
                    result.Add(attr);
                }
            }
            catch { }

            return result;
        }

        public ScriptResult Parse(string url, string html)
        {
            foreach (var model in models)
            {
                if (url.StartsWith(model.URLSpecifier))
                {
                    var result = new ScriptResult();

                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(html);
                    HtmlNode node = document.DocumentNode;

                    if (!model.TitleXPath.Contains("#text"))
                    {
                        result.Title = node.SelectSingleNode(model.TitleXPath).InnerText;
                    }
                    else
                    {
                        result.Title = node.SelectSingleNode(model.TitleXPath.Remove(model.TitleXPath.IndexOf("/#text"))).InnerText;
                    }
                    result.Images = get(node, model.ImagesXPath);
                    result.FileNames = get(node, model.FileNameXPath);

                    return result;
                }
            }
            return null;
        }
    }
}
