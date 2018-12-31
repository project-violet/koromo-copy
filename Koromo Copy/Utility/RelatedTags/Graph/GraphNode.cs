/* Copyright (C) 2018-2019. Hitomi Parser Developers */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Hitomi_Copy_3.Graph
{
    public class GraphEdge
    {
        public int Index;
        public int StartsIndex;
        public int EndsIndex;
        public string Text;
        public string SelectionText;
        public Color Color;
        public float Thickness;
        public Point Starts;
        public Point Ends;
    }

    public class GraphVertex
    {
        public int Index;
        public string OuterText;
        public string InnerText;
        public Point Position;
        public Color Color;
        public float Radius;

        public List<Tuple<GraphVertex, GraphEdge>> Nodes;
    }
}
