using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hitomi_Copy;
using Hitomi_Copy_3;
using System.Drawing.Drawing2D;
using System.IO;
using System.Diagnostics;
using Hitomi_Copy_2;

namespace ZipViewer
{
    public interface IPicElement
    {
        bool Selected { get; set; }
        bool MouseIn { get; set; }
        Image Image { get; set; }
        string Label { get; set; }
        Font Font { set; }
        PictureBox Picture { get; }
        string Path { get; set; }
        HitomiJsonModel Log { get; set; }
        void SetImageFromAddress(string addr, int pannelw, int pannelh, bool title = true);
        void Invalidate();
    }

    public partial class PicElement : UserControl, IPicElement
    {
        Image image;
        bool selected = false;
        string label = "";
        Font font;
        bool mouse_enter = false;
        bool downloaded_overlapping = false;
        bool hidden_data = false;
        bool bookmark = false;
        PictureBox pb = new PictureBox();
        Lazy<InfoForm> info;
        Form parent;

        public PicElement(Form parent, ToolTip tooltip = null)
        {
            InitializeComponent();

            this.Paint += PicElement_Paint;
            this.BackColor = Color.WhiteSmoke;
            this.parent = parent;
            this.DoubleBuffered = true;

            MouseEnter += Thumbnail_MouseEnter;
            MouseLeave += Thumbnail_MouseLeave;
            MouseClick += Thumbnail_MouseClick;
            MouseDoubleClick += Thumbnail_MouseDoubleClick;

            Disposed += OnDispose;
        }

        private void Thumbnail_MouseEnter(object sender, EventArgs e)
        { ((PicElement)sender).MouseIn = true; }
        private void Thumbnail_MouseLeave(object sender, EventArgs e)
        { ((PicElement)sender).MouseIn = false; }
        private void Thumbnail_MouseClick(object sender, EventArgs e)
        { if (((MouseEventArgs)e).Button == MouseButtons.Left) { ((PicElement)sender).Selected = !((PicElement)sender).Selected; } }
        private void Thumbnail_MouseDoubleClick(object sender, EventArgs e)
        { Process.Start(Path); }

        private void PicElement_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            RectangleF LabelRect = new RectangleF(pb.Location.X, pb.Location.Y + pb.Size.Height + 2, pb.Width, 30);
            g.DrawString(label, font, Brushes.Black, LabelRect);

            if (selected)
            {
                SolidBrush basicBrushes = new SolidBrush(Color.FromArgb(200, 234, 202, 233));
                g.FillRectangle(basicBrushes, 0, 0, Width, Height);
                g.DrawRectangle(new Pen(Color.LightPink, 2), 2, 2, this.Width - 4, this.Height - 4);
            }
            else if (mouse_enter)
            {
                SolidBrush basicBrushes = new SolidBrush(Color.FromArgb(100, 234, 202, 233));
                g.FillRectangle(basicBrushes, 0, 0, Width, Height);
                g.DrawRectangle(new Pen(Color.FromArgb(255, 174, 201), 1), 1, 1, this.Width - 2, this.Height - 2);

            }

            if (callfrom_paint == false)
            {
                callfrom_panel = true;
                pb.Invalidate();
            }
            callfrom_paint = false;
        }

        bool callfrom_paint = false;
        bool callfrom_panel = false;

        private void Picture_Paint(object sender, PaintEventArgs e)
        {
            ViewBuffer buffer = new ViewBuffer();
            buffer.CreateGraphics(pb.Width, pb.Height);

            Graphics g = buffer.g;
            
            if (selected)
            {
                SolidBrush basicBrushes = new SolidBrush(Color.FromArgb(170, 234, 202, 233));
                g.FillRectangle(basicBrushes, 0, 0, Width, Height);
            }
            else if (mouse_enter)
            {
                SolidBrush basicBrushes = new SolidBrush(Color.FromArgb(100, 234, 202, 233));
                g.FillRectangle(basicBrushes, 0, 0, Width, Height);
            }

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            
            if (callfrom_panel == false)
            {
                callfrom_paint = true;
                Invalidate();
            }
            callfrom_panel = false;

            buffer.Draw(e.Graphics);
            buffer.Dispose();
        }
        private void Invalidall()
        { callfrom_panel = callfrom_paint = false; Invalidate(); }
        private void Picture_MouseEnter(object sender, EventArgs e)
        { mouse_enter = true; info.Value.Location = Cursor.Position; info.Value.Show(); Invalidall(); }
        private void Picture_MouseLeave(object sender, EventArgs e)
        { mouse_enter = false; info.Value.Location = Cursor.Position; info.Value.Hide(); Invalidall(); }
        private void Picture_MouseMove(object sender, EventArgs e)
        {
            int sw = SystemInformation.VirtualScreen.Width;
            int sh = SystemInformation.VirtualScreen.Height;
            int cx = Cursor.Position.X + 15;
            int cy = Cursor.Position.Y;
            if (sw < cx - 15 + info.Value.Width)
                cx = Cursor.Position.X - 15 - info.Value.Width;
            if (sh < info.Value.Height + cy)
                cy = sh - info.Value.Height;
            info.Value.Location = new Point(cx, cy);
        }
        private void Picture_MouseClick(object sender, EventArgs e)
        { if (((MouseEventArgs)e).Button == MouseButtons.Left) { selected = !selected; Invalidall(); } }

        private void OnDispose(object sender, EventArgs e)
        {
            if (image != null)
                image.Dispose();
            if (info != null && info.IsValueCreated)
                info.Value.Dispose();
        }

        private new void Resize(object sedner, EventArgs e)
        {
            Invalidate();
        }

        public Size Adjust(Size image, Size match)
        {
            decimal r1 = (decimal)image.Width / image.Height;
            decimal r2 = (decimal)(match.Width) / (match.Height);
            int w = (match.Width);
            int h = match.Height;
            if (r1 > r2)
            {
                w = (match.Width);
                h = (int)(w / r1);
            }
            else if (r1 < r2)
            {
                h = match.Height;
                w = (int)(r1 * h);
            }
            return new Size(w, h);
        }

        public void SetImageFromAddress(string addr, int pannelw, int pannelh, bool title = true)
        {
            Dock = DockStyle.Bottom;
            try
            {
                pb.Location = new Point(3, 3);
                if (title == true)
                    pb.Size = new Size(pannelw - 6, pannelh - 30);
                else
                    pb.Size = new Size(pannelw - 6, pannelh - 6);
                using (FileStream fs = new FileStream(addr, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
                {
                    image = Image.FromStream(fs);
                    Size sz = image.Size;//Adjust(image.Size, new Size((pannelw - 6) * 8, (pannelh - 30) * 8));
                    pb.Image = image;//image.GetThumbnailImage(sz.Width, sz.Height, () => false, IntPtr.Zero);
                }
                pb.SizeMode = PictureBoxSizeMode.Zoom;
                pb.Paint += Picture_Paint;
                pb.MouseEnter += Picture_MouseEnter;
                pb.MouseLeave += Picture_MouseLeave;
                pb.MouseClick += Picture_MouseClick;
                pb.MouseDoubleClick += Thumbnail_MouseDoubleClick;
                pb.MouseMove += Picture_MouseMove;
                info = new Lazy<InfoForm>(() => new InfoForm(Image, Adjust(Image.Size, new Size(150*3,200*3))));
                this.Width = pannelw;
                this.Height = pannelh;
                this.Controls.Add(pb);
            }
            catch (Exception ex)
            {
            }
        }

        public HitomiJsonModel Log
        { get; set; }
        public string Path
        { get; set; }
        public bool Selected
        { get { return selected; } set { selected = value; Invalidate(); } }
        public bool MouseIn
        { get { return mouse_enter; } set { mouse_enter = value; Invalidate(); } }
        public Image Image
        { get { return image; } set { image = value; } }
        public string Label
        { get { return label; } set { label = value; } }
        public override Font Font
        { set { font = value; } }
        public PictureBox Picture
        { get { return pb; } }
        public bool IsHidden
        { get { return hidden_data; } }
        public bool IsDownloaded
        { get { return downloaded_overlapping; } }
    }
}
