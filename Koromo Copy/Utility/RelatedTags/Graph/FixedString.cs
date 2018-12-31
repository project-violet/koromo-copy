/* Copyright (C) 2018-2019. Hitomi Parser Developers */

using System.Drawing;

namespace Hitomi_Copy_3.Graph
{
    public class FixedString
    {
        public string Message;
        public Point Position;
        public Font Font;
        public Brush Brush;

        public FixedString(string msg, Point p, Font f, Brush b)
        {
            Message = msg;
            Position = p;
            Font = f;
            Brush = b;
        }
    }
}
