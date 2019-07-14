/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy_UX.Utility.Bookmark
{
    public class BookmarkItemModel
    {
        public string content;
        public DateTime stamp;
        public string etc;
        public string path;
    }

    public class BookmarkEtcItemModel
    {
        public string classify;
        public string content;
        public DateTime stamp;
        public string etc;
        public string path;
    }

    public class BookmarkModel
    {
        // name, class
        public List<Tuple<string, BookmarkItemModel>> artists;
        public List<Tuple<string, BookmarkItemModel>> groups;
        public List<Tuple<string, BookmarkItemModel>> articles;

        // etc elements
        public List<Tuple<string, BookmarkEtcItemModel>> etcs;

        public List<string> root_classes;
        // parent class, class name
        public List<Tuple<string, string>> sub_classes;
    }

    public class BookmarkModelManager : ILazy<BookmarkModelManager>
    {
        string model_path = $"sbookmark.json";
        BookmarkModel model;

        public BookmarkModelManager()
        {
            if (File.Exists(model_path)) model = JsonConvert.DeserializeObject<BookmarkModel>(File.ReadAllText(model_path));
            if (model == null)
            {
                model = new BookmarkModel
                {
                    artists = new List<Tuple<string, BookmarkItemModel>>(),
                    groups = new List<Tuple<string, BookmarkItemModel>>(),
                    articles = new List<Tuple<string, BookmarkItemModel>>(),
                    etcs = new List<Tuple<string, BookmarkEtcItemModel>>(),
                    root_classes = new List<string> { "/미분류" },
                    sub_classes = new List<Tuple<string, string>>()
                };
                Save();
            }
            if (model.etcs == null) { model.etcs = new List<Tuple<string, BookmarkEtcItemModel>>(); Save(); }
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(model, Formatting.Indented);
            using (var fs = new StreamWriter(new FileStream(model_path, FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }
        
        public BookmarkModel Model { get { return model; } }
    }
}
