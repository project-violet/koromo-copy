/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Koromo_Copy.Component.Hitomi
{
    public class HitomiIndexModel
    {
        public Tuple<int, string>[] Artists;
        public Tuple<int, string>[] Groups;
        public Tuple<int, string>[] Series;
        public Tuple<int, string>[] Characters;
        public Tuple<int, string>[] Languages;
        public Tuple<int, string>[] Types;
        public Tuple<int, string>[] Tags;
    }

    public class HitomiIndexMetadata
    {
        [JsonProperty(PropertyName = "a")]
        public int[] Artists { get; set; }
        [JsonProperty(PropertyName = "g")]
        public int[] Groups { get; set; }
        [JsonProperty(PropertyName = "p")]
        public int[] Parodies { get; set; }
        [JsonProperty(PropertyName = "t")]
        public int[] Tags { get; set; }
        [JsonProperty(PropertyName = "c")]
        public int[] Characters { get; set; }
        [JsonProperty(PropertyName = "l")]
        public int Language { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "type")]
        public int Type { get; set; }
        [JsonProperty(PropertyName = "id")]
        public int ID { get; set; }
    }

    public class HitomiIndex : ILazy<HitomiIndex>
    {
        private static void add(Dictionary<string, int> dic, string arr)
        {
            if (arr == null) return;
            if (!dic.ContainsKey(arr))
                dic.Add(arr, dic.Count);
        }

        private static void add(Dictionary<string, int> dic, string[] arr)
        {
            if (arr == null) return;
            foreach (var item in arr)
                if (!dic.ContainsKey(item))
                    dic.Add(item, dic.Count);
        }

        public HitomiTagdataCollection tagdata_collection = new HitomiTagdataCollection();
        public List<HitomiIndexMetadata> metadata_collection = new List<HitomiIndexMetadata>();

        public static void MakeIndex()
        {
            var artists = new Dictionary<string, int>();
            var groups = new Dictionary<string, int>();
            var series = new Dictionary<string, int>();
            var characters = new Dictionary<string, int>();
            var languages = new Dictionary<string, int>();
            var types = new Dictionary<string, int>();
            var tags = new Dictionary<string, int>();

            foreach (var md in HitomiData.Instance.metadata_collection)
            {
                add(artists, md.Artists);
                add(groups, md.Groups);
                add(series, md.Parodies);
                add(characters, md.Characters);
                add(languages, md.Language);
                add(types, md.Type);
                add(tags, md.Tags);
            }

            var index = new HitomiIndexModel();

            index.Artists = artists.Select(x => new Tuple<int, string>(x.Value, x.Key)).ToArray();
            index.Groups = groups.Select(x => new Tuple<int, string>(x.Value, x.Key)).ToArray();
            index.Series = series.Select(x => new Tuple<int, string>(x.Value, x.Key)).ToArray();
            index.Characters = characters.Select(x => new Tuple<int, string>(x.Value, x.Key)).ToArray();
            index.Languages = languages.Select(x => new Tuple<int, string>(x.Value, x.Key)).ToArray();
            index.Types = types.Select(x => new Tuple<int, string>(x.Value, x.Key)).ToArray();
            index.Tags = tags.Select(x => new Tuple<int, string>(x.Value, x.Key)).ToArray();

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            Monitor.Instance.Push("Write file: index.json");
            using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "index.json")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, index);
            }

            var mdl = new List<HitomiIndexMetadata>();

            foreach (var md in HitomiData.Instance.metadata_collection)
            {
                var him = new HitomiIndexMetadata();
                him.ID = md.ID;
                him.Name = md.Name;
                if (md.Artists != null) him.Artists = md.Artists.Select(x => artists[x]).ToArray();
                if (md.Groups != null) him.Groups = md.Groups.Select(x => groups[x]).ToArray();
                if (md.Parodies != null) him.Parodies = md.Parodies.Select(x => series[x]).ToArray();
                if (md.Characters != null) him.Characters = md.Characters.Select(x => characters[x]).ToArray();
                if (md.Language != null) him.Language = languages[md.Language];
                if (md.Type != null) him.Type = types[md.Type];
                if (md.Tags != null) him.Tags = md.Tags.Select(x => tags[x]).ToArray();
                mdl.Add(him);
            }

            Monitor.Instance.Push("Write file: index-metadata.json");
            using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "index-metadata.json")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, mdl);
            }
        }

        public void LoadMetadataJson()
        {
            if (CheckMetadataExist())
                metadata_collection = JsonConvert.DeserializeObject<List<HitomiIndexMetadata>>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "index-metadata.json")));
        }

        public bool CheckMetadataExist()
        {
            return File.Exists(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "index-metadata.json"));
        }

        public void SortMetadata()
        {
            metadata_collection.Sort((a, b) => b.ID.CompareTo(a.ID));
        }

        public DateTime DateTimeMetadata()
        {
            return File.GetLastWriteTime(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "index-metadata.json"));
        }

        public DateTime DateTimeHiddendata()
        {
            return File.GetLastWriteTime(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "index-hiddendata.json"));
        }

        public bool CheckHiddendataExist()
        {
            return File.Exists(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "index-hiddendata.json"));
        }
    }
}
