/* Copyright (C) 2018. Hitomi Parser Developers */

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Hitomi_Copy
{
    public partial class InfoForm : Form
    {
        Image image;

        public InfoForm(Image image, Size size)
        {
            InitializeComponent();

            this.image = image;
            this.Size = size;
            Disposed += OnDispose;
        }

        private const int WS_EX_TOPMOST = 0x00000008;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WS_EX_TOPMOST;
                return createParams;
            }
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }
        
        private void OnDispose(object sender, EventArgs e)
        {
            image.Dispose();
            Debug.WriteLine("Dispose!");
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            e.Graphics.DrawImage(image, 0, 0, Width, Height);
        }

        private void InfoForm_MouseEnter(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
