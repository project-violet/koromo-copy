/* Copyright (C) 2018-2019. Hitomi Parser Developers */

using System;
using System.Drawing;

namespace Hitomi_Copy_3
{
    public sealed class ViewBuffer : IDisposable
    {
        private Graphics graphics;
        private Bitmap bitmap;
        private int w, h;

        public ViewBuffer()
        {
            w = 0;
            h = 0;
        }
        
        public void CreateGraphics(int w, int h)
        {
            if (bitmap != null) bitmap.Dispose();
            if (g != null) g.Dispose();
            this.w = w;
            this.h = h;
            bitmap = new Bitmap(w, h);
            graphics = Graphics.FromImage(bitmap);
        }
        
        public void Draw(Graphics g)
        {
            if (graphics != null)
            {
                g.DrawImage(bitmap, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);
            }
        }

        public void Dispose()
        {
            bitmap.Dispose();
        }

        public Graphics g
        {
            get { return graphics; }
        }
    }
}
