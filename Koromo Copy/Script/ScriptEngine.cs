/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using HtmlAgilityPack;
using Koromo_Copy.Html;
using Koromo_Copy.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Koromo_Copy.Script
{
    public class ScriptModel
    {
        public string ScriptName;
        public string ScriptVersion;
        public string ScriptAuthor;
        public string ScriptFolderName;
        public string ScriptRequestName;
        public int PerDelay;
        public bool UsingDriver;

        public string URLSpecifier;
        public string TitleCAL;
        public string ImagesCAL;
        public string FileNameCAL;

        public bool UsingSub;
        public string SubURLCAL;
        public string SubURLTitleCAL;
        public string SubTitleCAL;
        public string SubImagesCAL;
        public string SubFileNameCAL;
    }

    public class ScriptResult
    {
        public string Title;
        public string URL;
        public List<string> Images;
        public List<string> FileNames;
    }
    
    public class ScriptEngine : ILazy<ScriptEngine>
    {
        List<ScriptModel> models = new List<ScriptModel>();

        public ScriptEngine()
        {
            AddScript(new ScriptModel
            {
                ScriptName = "Mangashowme",
                ScriptVersion = "1.0",
                ScriptAuthor = "dc-koromo",
                ScriptFolderName = "mangashowme",
                ScriptRequestName = "mangashow-me",
                PerDelay = 1000,
                UsingDriver = false,

                URLSpecifier = "https://mangashow.me/",
                TitleCAL = "/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]",

                UsingSub = true,
                SubURLCAL = "/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div[2]/div[1]/div[1]/div[{1+i*1}]/a[1], #attr[href]",
                SubURLTitleCAL = "/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div[2]/div[1]/div[1]/div[{1+i*1}]/a[1]/div[1], #htext",
                SubImagesCAL = "/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/section[1]/div[1]/form[1]/div[1]/div[{1+i*1}]/div[1], #attr[style], #regex[https://[^\\)]*]",
                SubFileNameCAL = "/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/section[1]/div[1]/form[1]/div[1]/div[{1+i*1}]/div[1], #attr[style], #regex[https://[^\\)]*], #split[/,-1]",
            });
        }

        public void Extract(string script_name)
        {
            for (int i = 0; i < models.Count; i++)
            {
                if (models[i].ScriptName == script_name)
                {
                    Monitor.Instance.Push($"[Script Engine] Extract script: {models[i].ScriptName} ({models[i].ScriptVersion}, {models[i].ScriptAuthor})");
                    models.RemoveAt(i);
                    return;
                }
            }
        }

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
            Monitor.Instance.Push($"[Script Engine] Import script: {model.ScriptName} ({model.ScriptVersion}, {model.ScriptAuthor})");
        }

        public bool TestScript(string url)
        {
            foreach (var model in models)
                if (url.StartsWith(model.URLSpecifier))
                    return true;
            return false;
        }

        public ScriptModel FindModel(string url)
        {
            foreach (var model in models)
                if (url.StartsWith(model.URLSpecifier))
                    return model;
            return null;
        }
        
        public Tuple<string,List<ScriptResult>> Run(string url, Action<string> update = null)
        {
            foreach (var model in models)
            {
                if (url.StartsWith(model.URLSpecifier))
                {
                    var result = new ScriptResult();

                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(Net.NetCommon.DownloadString(url));
                    HtmlNode node = document.DocumentNode;

                    if (!model.UsingDriver)
                    {
                        result.Title = HtmlCAL.Calculate(model.TitleCAL, node)[0];
                        if (model.UsingSub)
                        {
                            return Tuple.Create(result.Title, run_subs(model, node, update));
                        }
                        else
                        {
                            update?.Invoke($"{result.Images.Count}개의 이미지를 찾았습니다.");
                            result.Images = HtmlCAL.Calculate(model.ImagesCAL, node);
                            result.FileNames = HtmlCAL.Calculate(model.FileNameCAL, node);
                            return Tuple.Create(result.Title, new List<ScriptResult> { result });
                        }
                    }
                }
            }
            return null;
        }

        private List<ScriptResult> run_subs(ScriptModel model, HtmlNode root_node, Action<string> update)
        {
            var result = new List<ScriptResult>();
            var sub_urls = HtmlCAL.Calculate(model.SubURLCAL, root_node);

            List<string> titles = null;
            if (!string.IsNullOrEmpty(model.SubURLTitleCAL))
                titles = HtmlCAL.Calculate(model.SubURLTitleCAL, root_node);

            update?.Invoke($"{sub_urls.Count}개의 하위 항목을 찾았습니다.");

            for (int i = 0; i < sub_urls.Count; i++)
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(Net.NetCommon.DownloadString(sub_urls[i]));
                HtmlNode node = document.DocumentNode;

                var title = "";
                if (titles != null)
                    title = titles[i];
                else
                    title = HtmlCAL.Calculate(model.SubURLTitleCAL, node)[0];

                result.Add(new ScriptResult
                {
                    URL = sub_urls[i],
                    Title = title,
                    Images = HtmlCAL.Calculate(model.SubImagesCAL, node),
                    FileNames = HtmlCAL.Calculate(model.SubFileNameCAL, node)
                });
                update?.Invoke($"이미지 목록을 만드는 중입니다...[{i+1}/{sub_urls.Count}]");
                Thread.Sleep(model.PerDelay);
            }

            return result;
        }
    }
}
