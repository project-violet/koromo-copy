/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Elo
{
    public class EloPlayer
    {
        public string Indentity; // artist name
        public long Win;
        public long Lose;
        public long Draw;
        public double Rating;
        public List<Tuple<int, int>> History;

        [JsonIgnore]
        public double W { get { return (double)Win / (Win + Lose); } }
        [JsonIgnore]
        public double R { get { return Rating; } }
        public double E(EloPlayer p) => 1 / (1 + Math.Pow(10, (p.R - R) / 400));
        public void U(double S, double E) => Rating += 32 * (S - E);
    }

    public class EloModel
    {
        public List<EloPlayer> Players;
        public List<Tuple<int, int, int>> History;
        public List<Tuple<int, int, int, int, int>> DHistory;
        public int GameCount;
    }

    public class EloSystem
    {
        EloModel model;

        public List<EloPlayer> Players { get { return model.Players; } }
        public EloModel Model { get { return model; } }

        public EloSystem()
        {
            model = new EloModel();
            model.Players = new List<EloPlayer>();
            model.History = new List<Tuple<int, int, int>>();
            model.DHistory = new List<Tuple<int, int, int, int, int>>();
        }

        public void Save(string filename = "rank-simulator.json")
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            Monitor.Instance.Push($"Write file: {filename}");
            using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, model);
            }
        }

        public void Open(string filename = "rank-simulator.json")
        {
            model = JsonConvert.DeserializeObject<EloModel>(File.ReadAllText(filename));
        }

        public void AppendPlayer(int sz)
        {
            for (int i = 0; i < sz; i++)
                model.Players.Add(new EloPlayer { Rating = 1500, History = new List<Tuple<int, int>>() });
        }
        
        public void Win(int index1, int index2, int id1, int id2)
        {
            model.Players[index1].U(1, model.Players[index1].E(model.Players[index2]));
            model.Players[index2].U(0, model.Players[index2].E(model.Players[index1]));
            model.Players[index1].Win += 1;
            model.Players[index2].Lose += 1;
            model.Players[index1].History.Add(Tuple.Create(index2, 1));
            model.Players[index2].History.Add(Tuple.Create(index1, -1));
            model.DHistory.Add(Tuple.Create(index1, index2, 1, id1, id2));
            Save();
        }
        public void Lose(int index1, int index2, int id1, int id2) => Win(index2, index1, id2, id1);
        public void Draw(int index1, int index2, int id1, int id2)
        {
            model.Players[index1].U(0.5, model.Players[index1].E(model.Players[index2]));
            model.Players[index2].U(0.5, model.Players[index2].E(model.Players[index1]));
            model.Players[index1].Draw += 1;
            model.Players[index2].Draw += 1;
            model.Players[index1].History.Add(Tuple.Create(index2, 0));
            model.Players[index2].History.Add(Tuple.Create(index1, 0));
            model.DHistory.Add(Tuple.Create(index1, index2, 0, id1, id2));
            Save();
        }
        public void UpdateWin(int index, double R)
        {
            model.Players[index].U(1, R);
            model.Players[index].Win += 1;
        }
        public void UpdateLose(int index, double R)
        {
            model.Players[index].U(0, R);
            model.Players[index].Lose += 1;
        }
        public void UpdateDraw(int index, double R)
        {
            model.Players[index].U(0.5, R);
            model.Players[index].Draw += 1;
        }
    }
}
