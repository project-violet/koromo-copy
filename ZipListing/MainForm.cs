/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Hitomi_Copy_3;
using Koromo_Copy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZipListing
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!Settings.Instance.SettingCompleted)
            {
                Hide();
                MessageBox.Show(
                    "settings.json을 통해 환경설정 후 재시작해주세요!\r\n" +
                    "SortOrderBy: 폴더 정렬 방법을 설정합니다.\r\n  - 0이면 이름순\r\n  - 1이면 수정날짜 최신순\r\n  - 2이면 수정날짜 오래된순\r\n  - 3이면 포함파일개수 내림차순\r\n  - 4이면 포함파일개수 오름차순",
                    "ZipListing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
                Application.Exit();
            }
        }

        int load_count;
        int total_count;

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);

        private void load_page(int page)
        {
            //this.Post(() => flowLayoutPanel1.Controls.Clear());
            flowLayoutPanel1.Controls.OfType<RecommendControl>().ToList().ForEach(x => x.Dispose());
            //var list = Directory.GetDirectories(dir).ToList();
            //list.Sort((x, y) => StrCmpLogicalW(x, y));

            //load_count = 0;
            //total_count = 0;

            //foreach (var folder in list)
            //{
            //    if (Directory.GetFiles(folder).Length > 0)
            //    {
            //        Interlocked.Increment(ref total_count);
            //        this.Post(() => flowLayoutPanel1.Controls.Add(new RecommendControl(folder)));
            //    }
            //    Thread.Sleep(300);
            //}

            numericUpDown1.Value = page;

            load_count = 0;
            total_count = 0;
            page -= 1;

            for (int i = page * 3; i < page * 3 + 3 && i < folders.Count; i++)
            {
                if (Directory.GetFiles(folders[i]).Length > 0)
                {
                    Interlocked.Increment(ref total_count);
                    this.Post(() => flowLayoutPanel1.Controls.Add(new RecommendControl(folders[i])));
                }
                Thread.Sleep(100);
            }
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }

        List<string> folders = new List<string>();
        int page_max;
        int page_now;

        private void load_folder_pre(string dir)
        {
            //this.Post(() => flowLayoutPanel1.Controls.Clear());
            flowLayoutPanel1.Controls.OfType<Control>().ToList().ForEach(x => x.Dispose());
            //flowLayoutPanel1.Controls.Clear();
            folders = Directory.GetDirectories(dir).Where(x => new DirectoryInfo(x).GetFiles().Length > 0).ToList();

            switch (Settings.Instance.Model.SortOrderBy)
            {
                case 0:
                    folders.Sort((x, y) => StrCmpLogicalW(x, y));
                    break;

                case 1:
                    folders.Sort((x, y) => new DirectoryInfo(y).LastWriteTime.CompareTo(new DirectoryInfo(x).LastWriteTime));
                    break;
                case 2:
                    folders.Sort((x, y) => new DirectoryInfo(x).LastWriteTime.CompareTo(new DirectoryInfo(y).LastWriteTime));
                    break;


                case 3:
                    folders.Sort((x, y) => new DirectoryInfo(y).GetFiles().Length.CompareTo(new DirectoryInfo(x).GetFiles().Length));
                    break;
                case 4:
                    folders.Sort((x, y) => new DirectoryInfo(x).GetFiles().Length.CompareTo(new DirectoryInfo(y).GetFiles().Length));
                    break;

            }
                

            page_max = folders.Count / 3 + ((folders.Count % 3) > 0 ? 1 : 0);
            page_now = 1;

            label1.Text = "/ " + page_max;
            numericUpDown1.Maximum = page_max;

            load_page(page_now);
        }

        public void load_complete()
        {
            Interlocked.Increment(ref load_count);
            Text = $"ZipListing by DCKoromo [{load_count}/{total_count}]";
        }

        private void flowLayoutPanel1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] folder = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (folder.Length > 1)
                {
                    MessageBox.Show("하나의 폴더만 끌어오세요!", "Zip Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    load_folder_pre(folder[0]);
                }
            }
        }

        private void flowLayoutPanel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (page_now > 1)
                page_now--;
            numericUpDown1.Value = page_now;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (page_now < page_max)
                page_now++;
            numericUpDown1.Value = page_now;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            load_page(page_now = (int)numericUpDown1.Value);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Process proc = Process.GetCurrentProcess();
            lMemoryUsage.Text = (proc.PrivateMemorySize64 / 1000).ToString("#,#") + " KB";
        }

        private void lMemoryUsage_Click(object sender, EventArgs e)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }
    }
}
