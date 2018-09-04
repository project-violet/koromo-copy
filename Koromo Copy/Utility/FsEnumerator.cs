/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Fs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Koromo_Copy.Utility
{
    public partial class FsEnumerator : Form
    {
        public FsEnumerator()
        {
            InitializeComponent();
        }
        
        private async void bStart_ClickAsync(object sender, EventArgs e)
        {
            tbPath.Enabled = false;
            bStart.Enabled = false;
            SetStatusMessage(StatusMessage.StartIndexing);
            indexor = new FileIndexor();
            await indexor.ListingDirectoryWithFilesAsync(tbPath.Text);
            SetStatusMessage(StatusMessage.Enumerating);
            await Task.Run(() => Processing());
        }

        #region 마무리

        FileIndexor indexor;
        
        private void Processing()
        {
            int i = 1;
            UInt64 total_size = indexor.GetTotalSize();
            UInt64 acc_size = 0;
            this.Post(() => label2.Text = $"{CapacityFormat(total_size)} ({total_size.ToString("#,0")} 바이트)");
            List<ListViewItem> l = new List<ListViewItem>();
            this.Post(() => pbIndexing.Maximum = indexor.Count);
            foreach (var v in indexor.GetListSortWithNativeSize())
            {
                acc_size += v.Item2;
                l.Add(new ListViewItem(new string[] { i++.ToString(), v.Item1,
                        CapacityFormat(v.Item2), ((double)v.Item2 * 100 / total_size).ToString("0.0") + "%",
                            ((double)acc_size * 100/ total_size).ToString("0.0") + "%"}));
                this.Post(() => pbIndexing.Value++);
            }
            this.Post(() => listView1.Items.AddRange(l.ToArray()));

            FileIndexorNode node = null;
            i = 1;
            node = indexor.GetPathNode(indexor.Node.Path);
            foreach (FileIndexorNode n in node.GetListSortWithSize())
            {
                this.Post(() => listView2.Items.Add(new ListViewItem(new string[] { i++.ToString(), n.Path, CapacityFormat(n.Size),
                        ((double)n.Size * 100/ node.Size).ToString("0.0") + "%",
                        ((double)n.Size * 100/ indexor.GetTotalSize()).ToString("0.0") + "%"})));
            }
            this.Post(() => SetStatusMessage(StatusMessage.EndIndexing));
        }

        private string CapacityFormat(UInt64 i)
        {
            string capacity;
            if (i / 1024 / 1024 / 1024 != 0) capacity = ((double)i / 1024 / 1024 / 1024).ToString("#,0.#") + " GB";
            else if (i / 1024 / 1024 != 0) capacity = ((double)i / 1024 / 1024).ToString("#,0") + " MB";
            else capacity = ((double)i / 1024).ToString("#,0") + " KB";
            return capacity;
        }

        #endregion
        
        #region 폴더 클릭

        string now_path;
        Stack<string> ss = new Stack<string>();

        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                string selected_path = listView2.SelectedItems[0].SubItems[1].Text;
                bool c = listView2.SelectedItems[0].SubItems[0].Text == "..";
                FileIndexorNode node = null;

                listView2.Items.Clear();
                if (c)
                {
                    string peek = ss.Pop();
                    if (peek == null)
                        peek = indexor.GetRootNode().Path;
                    node = indexor.GetPathNode(peek);
                    if (ss.Count != 0)
                        listView2.Items.Add(new ListViewItem(new string[] { "..", peek }));
                    now_path = peek;
                }
                else
                {
                    node = indexor.GetPathNode(selected_path);
                    listView2.Items.Add(new ListViewItem(new string[] { "..", now_path }));
                    ss.Push(now_path);
                    now_path = selected_path;
                }

                int i = 1;
                foreach (FileIndexorNode n in node.GetListSortWithSize())
                {
                    listView2.Items.Add(new ListViewItem(new string[] { i++.ToString(), n.Path, CapacityFormat(n.Size),
                        ((double)n.Size * 100/ node.Size).ToString("0.0") + "%",
                        ((double)n.Size * 100/ indexor.GetTotalSize()).ToString("0.0") + "%"}));
                }
            }
        }

        #endregion

        private enum StatusMessage
        {
            StartIndexing,
            EndIndexing,
            Enumerating,
            Idle,
        }

        private void SetStatusMessage(StatusMessage type)
        {
            switch (type)
            {
                case StatusMessage.Idle:
                    lStatus.Text = "Idle";
                    lStatusMessage.Text = "어떤 동작도 수행하지 않음";
                    break;
                case StatusMessage.StartIndexing:
                    lStatus.Text = "Indexing..";
                    lStatusMessage.Text = "폴더 인덱싱을 수행 중 입니다";
                    pbIndexing.Style = ProgressBarStyle.Marquee;
                    break;
                case StatusMessage.Enumerating:
                    lStatus.Text = "Enumerating..";
                    lStatusMessage.Text = "나열 중 입니다";
                    pbIndexing.Style = ProgressBarStyle.Blocks;
                    break;
                case StatusMessage.EndIndexing:
                    lStatus.Text = "Success";
                    lStatusMessage.Text = $"폴더 인덱싱이 성공적으로 수행되었습니다. ({indexor.Count}개의 폴더가 인덱싱됨)";
                    pbIndexing.Style = ProgressBarStyle.Blocks;
                    break;
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView lv = sender as ListView;
            if (lv.SelectedItems.Count > 0)
            {
                Process.Start("explorer.exe", lv.SelectedItems[0].SubItems[1].Text);
            }
        }
    }
}
