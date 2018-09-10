/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System.Net;
using System.Text;

namespace Koromo_Copy
{
    public partial class MainForm : MetroFramework.Forms.MetroForm
    {
        public MainForm()
        {
            InitializeComponent();

        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            Show();
            Monitor.Instance.ControlEnable = true;
            Monitor.Instance.Push("Hello!");
            //if (Monitor.Instance.ControlEnable)
                Monitor.Instance.Start();
        }

        private void metroButton1_Click(object sender, System.EventArgs e)
        {
            //WebClient wc = new WebClient();
            //wc.Encoding = Encoding.UTF8;
            //string gb = wc.DownloadString("https://ltn.hitomi.la/galleryblock/1217169.html");
            //var a = Hitomi.HitomiParser.ParseGalleryBlock(gb);
        }

        private void metroButton2_Click(object sender, System.EventArgs e)
        {
            (new Utility.FsEnumerator()).Show();
        }
    }
}
