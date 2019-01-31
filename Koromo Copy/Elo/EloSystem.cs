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
        public long Win;
        public long Lose;
        public long Draw;
        public double Rating;
        public List<Tuple<int, int>> History;

        public double W { get { return (double)Win / (Win + Lose + Draw); } }
        public double R { get { return Rating; } }
        public double E(EloPlayer p) => 1 / (1 + Math.Pow(10, (p.R - R) / 400));
        public void U(double S, double E) => Rating += 32 * (S - E);
    }

    public class EloSystem
    {
        List<EloPlayer> players;

        public List<EloPlayer> Players { get { return players; } }

        public EloSystem()
        {
            players = new List<EloPlayer>();
        }

        public void Save(string filename)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            Monitor.Instance.Push($"Write file: {filename}");
            using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, players);
            }
        }

        public void Open(string filename)
        {
            players = JsonConvert.DeserializeObject<List<EloPlayer>>(File.ReadAllText(filename));
        }

        public void AppendPlayer(int sz)
        {
            for (int i = 0; i < sz; i++)
                players.Add(new EloPlayer { Rating = 1500 });
        }

        public void Win(int index1, int index2)
        {
            players[index1].U(1, players[index1].E(players[index2]));
            players[index2].U(0, players[index2].E(players[index1]));
            players[index1].History.Add(Tuple.Create(index2, 1));
            players[index2].History.Add(Tuple.Create(index1, -1));
        }
        public void Lose(int index1, int index2) => Win(index2, index1);
        public void Draw(int index1, int index2)
        {
            players[index1].U(0.5, players[index1].E(players[index2]));
            players[index2].U(0.5, players[index2].E(players[index1]));
            players[index1].History.Add(Tuple.Create(index2, 0));
            players[index2].History.Add(Tuple.Create(index1, 0));
        }
    }
}
