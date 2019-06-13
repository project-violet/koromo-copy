/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.DC;
using Koromo_Copy.Interface;
using Koromo_Copy_UX.Domain;
using Newtonsoft.Json;
using SkyrimGallery.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyrimGallery
{
    public class SkyrimGalleryAnalyzer : ILazy<SkyrimGalleryAnalyzer>
    {
        DCPageArticle[] articles;

        public SkyrimGalleryAnalyzer()
        {
            articles = JsonConvert.DeserializeObject<DCPageArticle[]>(File.ReadAllText("list.txt"));
        }

        public DCPageArticle[] Articles => articles;
    }

    public class SkyrimGalleryTagData
    {
        public string Tag { get; set; }
        public int Count { get; set; }
    }

    public class SkyrimGalleryAutoComplete : IAutoCompleteAlgorithm2
    {
        public List<SkyrimGalleryTagData> GetResults(ref string word, ref int position)
        {
            var match = new List<SkyrimGalleryTagData>();

            if (word.Contains(":"))
            {
                if (word.StartsWith("nick:"))
                {
                    word = word.Substring("nick:".Length);
                    position += "nick:".Length;
                    var dic = new Dictionary<string, int>();
                    foreach (var article in SkyrimGalleryAnalyzer.Instance.Articles)
                        if (article.nick != null && article.nick.Contains(word))
                        {
                            if (!dic.ContainsKey(article.nick))
                                dic.Add(article.nick, 0);
                            dic[article.nick]++;
                        }
                    match = dic.Select(x => new SkyrimGalleryTagData { Tag = x.Key, Count = x.Value }).ToList();
                }
                else if (word.StartsWith("ip:"))
                {
                    word = word.Substring("ip:".Length);
                    position += "ip:".Length;
                    var dic = new Dictionary<string, int>();
                    foreach (var article in SkyrimGalleryAnalyzer.Instance.Articles)
                        if (article.ip != null && article.ip.Contains(word))
                        {
                            if (!dic.ContainsKey(article.ip))
                                dic.Add(article.ip, 0);
                            dic[article.ip]++;
                        }
                    match = dic.Select(x => new SkyrimGalleryTagData { Tag = x.Key, Count = x.Value }).ToList();
                }
                else if (word.StartsWith("id:"))
                {
                    word = word.Substring("id:".Length);
                    position += "id:".Length;
                    var dic = new Dictionary<string, int>();
                    foreach (var article in SkyrimGalleryAnalyzer.Instance.Articles)
                        if (article.uid != null && article.uid.Contains(word))
                        {
                            if (!dic.ContainsKey(article.uid))
                                dic.Add(article.uid, 0);
                            dic[article.uid]++;
                        }
                    match = dic.Select(x => new SkyrimGalleryTagData { Tag = x.Key, Count = x.Value }).ToList();
                }
                else if (word.StartsWith("class:"))
                {
                    word = word.Substring("class:".Length);
                    position += "class:".Length;
                    var dic = new Dictionary<string, int>();
                    foreach (var article in SkyrimGalleryAnalyzer.Instance.Articles)
                        if (article.classify != null && article.classify.Contains(word))
                        {
                            if (!dic.ContainsKey(article.classify))
                                dic.Add(article.classify, 0);
                            dic[article.classify]++;
                        }
                    match = dic.Select(x => new SkyrimGalleryTagData { Tag = x.Key, Count = x.Value }).ToList();
                }
            }
            match.Sort((x, y) => y.Count.CompareTo(x.Count));

            string[] match_target = {
                    "nick:",
                    "ip:",
                    "id:",
                    "class:",
                };

            string w = word;
            List<SkyrimGalleryTagData> data_col = (from ix in match_target where ix.StartsWith(w) select new SkyrimGalleryTagData { Tag = ix }).ToList();
            if (data_col.Count > 0)
                match.AddRange(data_col);

            return match;
        }
    }

    public class SkyrimGalleryDataQuery
    {
        public List<string> Title;
        public List<string> Nickname;
        public List<string> Id;
        public List<string> Ip;
        public List<string> Type;
    }
}
