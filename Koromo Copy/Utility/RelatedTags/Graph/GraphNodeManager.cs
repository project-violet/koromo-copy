/* Copyright (C) 2018-2019. Hitomi Parser Developers */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Hitomi_Copy_3.Graph
{
    public class GraphNodeManager
    {
        public delegate void IterateVertex(GraphVertex p);
        public delegate void IterateEdge(GraphEdge p);

        public List<GraphVertex> vertexs;
        public List<GraphEdge> edges;

        public GraphNodeManager()
        {
            vertexs = new List<GraphVertex>();
            edges = new List<GraphEdge>();
        }

        public void Nomalize(GraphVertex v, List<Tuple<Point, double>> c_edges)
        {
            v.Position = new Point(0, 0);
            vertexs.Add(v);

            for (int i = 0; i < v.Nodes.Count; i++)
            {
                int x = (int)(Math.Cos(2 * Math.PI / v.Nodes.Count * i) * 1000);
                int y = (int)(Math.Sin(2 * Math.PI / v.Nodes.Count * i) * 1000);
                v.Nodes[i].Item1.Position = new Point(x, y);
                
                v.Nodes[i].Item2.Starts = new Point(0, 0);
                v.Nodes[i].Item2.Ends = new Point(x, y);

                vertexs.Add(v.Nodes[i].Item1);
                edges.Add(v.Nodes[i].Item2);
            }

            foreach (var p in c_edges)
            {
                Point p1 = vertexs[p.Item1.X].Position;
                Point p2 = vertexs[p.Item1.Y].Position;

                edges.Add(new GraphEdge()
                {
                    StartsIndex = p.Item1.X,
                    EndsIndex = p.Item1.Y,
                    Index = edges.Count,
                    Starts = p1,
                    Ends = p2,
                    Color = Color.DarkGray,
                    Text = "",
                    SelectionText = p.Item2.ToString().Substring(0, Math.Min(5, p.Item2.ToString().Length)),
                    Thickness = 3.0F
                });
            }
        }

        public int IncludePoint(Point p)
        {
            int selected_tmp = -1;
            for (int i = 0; i < vertexs.Count; i++)
            {
                float x = vertexs[i].Position.X - vertexs[i].Radius;
                float y = vertexs[i].Position.Y - vertexs[i].Radius;
                float xx = vertexs[i].Position.X + vertexs[i].Radius;
                float yy = vertexs[i].Position.Y + vertexs[i].Radius;

                if (x < p.X && y < p.Y && p.X < xx && p.Y < yy)
                {
                    selected_tmp = vertexs[i].Index;
                }
            }
            return selected_tmp;
        }

        public void IterateVertexs(IterateVertex ic)
        {
            foreach (var v in vertexs)
            {
                ic(v);
            }
        }

        public void IterateEdges(IterateEdge ie)
        {
            foreach (var v in edges)
            {
                ie(v);
            }
        }
    }

}
