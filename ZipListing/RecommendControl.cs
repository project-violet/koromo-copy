/* Copyright (C) 2018. Hitomi Parser Developers */

using Hitomi_Copy;
using Koromo_Copy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZipListing;

namespace Hitomi_Copy_3
{
    public partial class RecommendControl : UserControl, IDisposable
    {
        InfoWrapper[] info = new InfoWrapper[5];
        string artist;
        string directory_name;
        
        public RecommendControl(string directory_name)
        {
            InitializeComponent();

            this.directory_name = directory_name;
            artist = tbArtist.Text = Path.GetFileName(directory_name);

            Disposed += OnDispose;
        }

        private void OnDispose(object sender, EventArgs e)
        {
            foreach (var iw in info.Where(iw => iw != null))
                iw.Dispose();
        }
        
        private async void RecommendControl_LoadAsync(object sender, System.EventArgs e)
        {
            await LoadThumbnailAsync();
        }

        private async Task LoadThumbnailAsync()
        {
            List<string> titles = new List<string>();
            
            titles = Directory.GetFiles(directory_name).ToList();
            titles.Sort((x, y) => MainForm.StrCmpLogicalW(y, x));

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < titles.Count && i < 5; i++)
            {
                tasks.Add(Task.Factory.StartNew(x =>
                {
                    int ix = (int)x;
                   AddMetadataToPanel(ix, titles[ix]); 
                }, i));
                //tasks.Add(Task.Run(() =>  try { AddMetadataToPanel(i, titles[i])) } catch { });
            }

            await Task.WhenAll(tasks);

            Application.OpenForms[0].Post(() => (Application.OpenForms[0] as MainForm).load_complete());
        }

        private void AddMetadataToPanel(int i, string file_name)
        {
            string tmp = Path.GetTempFileName();
            using (var zip = ZipFile.Open(file_name, ZipArchiveMode.Read))
            {
                if (!zip.Entries[0].Name.EndsWith(".json"))
                    zip.Entries[0].ExtractToFile(tmp, true);
                else
                    zip.Entries[1].ExtractToFile(tmp, true);
            }

            Image img;
            using (FileStream fs = new FileStream(tmp, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
            {
                img = Image.FromStream(fs);
                //img = img.GetThumbnailImage(img.Width, img.Height, () => false, (IntPtr)null);
            }
            info[i] = new InfoWrapper(img.Clone() as Image);

            PictureBox[] pbs = { pb1, pb2, pb3, pb4, pb5 };
            pbs[i].MouseEnter += info[i].Picture_MouseEnter;
            pbs[i].MouseMove += info[i].Picture_MouseMove;
            pbs[i].MouseLeave += info[i].Picture_MouseLeave;

            if (pbs[i].InvokeRequired)
                pbs[i].Invoke(new Action(() => { pbs[i].Image = img; }));
            else
                pbs[i].Image = img;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }

        private void bDetail_Click(object sender, System.EventArgs e)
        {
            Process.Start(@"C:\Tools\ZipViewer.exe", $@"""{directory_name}""");
            //(new frmArtistInfo(tbArtist.Text)).Show();
        }

        private void bOpen_Click(object sender, EventArgs e)
        {
            Process.Start(directory_name);
        }
    }

    public sealed class InfoWrapper : IDisposable
    {
        Lazy<InfoForm> info;

        public InfoWrapper(Image image)
        {
            info = new Lazy<InfoForm> (() => new InfoForm(image, Adjust(image.Size, new Size(150 * 3, 200 * 3))));
        }

        public static Size Adjust(Size image, Size match)
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

        public void Dispose()
        {
            if (info.IsValueCreated)
                info.Value.Dispose();
        }

        public void Picture_MouseEnter(object sender, EventArgs e)
        { info.Value.Location = Cursor.Position; info.Value.Show(); }
        public void Picture_MouseLeave(object sender, EventArgs e)
        { info.Value.Location = Cursor.Position; info.Value.Hide(); }
        public void Picture_MouseMove(object sender, EventArgs e)
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
    }

}
