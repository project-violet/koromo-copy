/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using DCGallery.Domain;
using Koromo_Copy.Component.DC;
using Koromo_Copy.Interface;
using Koromo_Copy_UX.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCGallery
{
    public class DCGalleryAnalyzer : ILazy<DCGalleryAnalyzer>
    {
        DCGalleryModel model;

        public void Open(string filename = "list.txt")
        {
            model = JsonConvert.DeserializeObject<DCGalleryModel>(File.ReadAllText(filename));
        }

        public DCGalleryModel Model => model;
        public DCPageArticle[] Articles => model.articles;
    }

    public class DCGalleryTagData
    {
        public string Tag { get; set; }
        public int Count { get; set; }
    }

    public class DCGalleryAutoComplete : IAutoCompleteAlgorithm2
    {
        public List<DCGalleryTagData> GetResults(ref string word, ref int position)
        {
            var match = new List<DCGalleryTagData>();

            if (word.Contains(":"))
            {
                if (word.StartsWith("nick:"))
                {
                    word = word.Substring("nick:".Length);
                    position += "nick:".Length;
                    var dic = new Dictionary<string, int>();
                    foreach (var article in DCGalleryAnalyzer.Instance.Articles)
                        if (article.nick != null && article.nick.Contains(word))
                        {
                            if (!dic.ContainsKey(article.nick))
                                dic.Add(article.nick, 0);
                            dic[article.nick]++;
                        }
                    match = dic.Select(x => new DCGalleryTagData { Tag = x.Key, Count = x.Value }).ToList();
                }
                else if (word.StartsWith("ip:"))
                {
                    word = word.Substring("ip:".Length);
                    position += "ip:".Length;
                    var dic = new Dictionary<string, int>();
                    foreach (var article in DCGalleryAnalyzer.Instance.Articles)
                        if (article.ip != null && article.ip.Contains(word))
                        {
                            if (!dic.ContainsKey(article.ip))
                                dic.Add(article.ip, 0);
                            dic[article.ip]++;
                        }
                    match = dic.Select(x => new DCGalleryTagData { Tag = x.Key, Count = x.Value }).ToList();
                }
                else if (word.StartsWith("id:"))
                {
                    word = word.Substring("id:".Length);
                    position += "id:".Length;
                    var dic = new Dictionary<string, int>();
                    foreach (var article in DCGalleryAnalyzer.Instance.Articles)
                        if (article.uid != null && article.uid.Contains(word))
                        {
                            if (!dic.ContainsKey(article.uid))
                                dic.Add(article.uid, 0);
                            dic[article.uid]++;
                        }
                    match = dic.Select(x => new DCGalleryTagData { Tag = x.Key, Count = x.Value }).ToList();
                }
                else if (word.StartsWith("class:"))
                {
                    word = word.Substring("class:".Length);
                    position += "class:".Length;
                    var dic = new Dictionary<string, int>();
                    foreach (var article in DCGalleryAnalyzer.Instance.Articles)
                        if (article.classify != null && article.classify.Contains(word))
                        {
                            if (!dic.ContainsKey(article.classify))
                                dic.Add(article.classify, 0);
                            dic[article.classify]++;
                        }
                    match = dic.Select(x => new DCGalleryTagData { Tag = x.Key, Count = x.Value }).ToList();
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
            List<DCGalleryTagData> data_col = (from ix in match_target where ix.StartsWith(w) select new DCGalleryTagData { Tag = ix }).ToList();
            if (data_col.Count > 0)
                match.AddRange(data_col);

            return match;
        }
    }

    public class DCGalleryDataQuery
    {
        public List<string> Title;
        public List<string> Nickname;
        public List<string> Id;
        public List<string> Ip;
        public List<string> Type;
    }
}
