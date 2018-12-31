/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Etier.IconHelper;
using Koromo_Copy.Fs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Koromo_Copy.Utility
{
    public partial class FsEnumerator : Form
    {
        ImageList smallImageList = new ImageList();
        ImageList largeImageList = new ImageList();
        IconListManager iconListManager;

        public FsEnumerator()
        {
            InitializeComponent();

            smallImageList.ColorDepth = ColorDepth.Depth32Bit;
            largeImageList.ColorDepth = ColorDepth.Depth32Bit;

            smallImageList.ImageSize = new System.Drawing.Size(16, 16);
            largeImageList.ImageSize = new System.Drawing.Size(32, 32);

            smallImageList.Images.Add(Fs.FileIcon.FolderIcon.GetFolderIcon(
                Fs.FileIcon.FolderIcon.IconSize.Small,
                Fs.FileIcon.FolderIcon.FolderType.Closed));

            iconListManager = new IconListManager(smallImageList, largeImageList);
        }

        private async void bStart_ClickAsync(object sender, EventArgs e)
        {
            if (cbIcon.Checked)
            {
                tvFs.ImageList = smallImageList;
            }
            else
            {
                tvFs.ImageList = null;
            }

            pbIndexing.Value = 0;
            listView1.Items.Clear();
            listView2.Items.Clear();
            tvFs.Nodes.Clear();
            tbPath.Enabled = false;
            bStart.Enabled = false;
            SetStatusMessage(StatusMessage.StartIndexing);
            indexor = new FileIndexor();
            await indexor.ListingDirectoryAsync(tbPath.Text);
            SetStatusMessage(StatusMessage.Enumerating);
            tbPath.Enabled = true;
            bStart.Enabled = true;
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
            Task.Run(() => ProcessingTreeView());
        }

        private string CapacityFormat(UInt64 i)
        {
            string capacity;
            if (i / 1024 / 1024 / 1024 != 0) capacity = ((double)i / 1024 / 1024 / 1024).ToString("#,0.#") + " GB";
            else if (i / 1024 / 1024 != 0) capacity = ((double)i / 1024 / 1024).ToString("#,0") + " MB";
            else capacity = ((double)i / 1024).ToString("#,0") + " KB";
            return capacity;
        }

        bool filesize = false;

        private void ProcessingTreeView()
        {
            FileIndexorNode node = indexor.GetRootNode();
            UInt64 total_size = indexor.GetTotalSize();
            var list = node.Nodes;
            list.Sort((a, b) => b.GetTotalSize().CompareTo(a.GetTotalSize()));

            string root_path = indexor.GetRootNode().Path;
            TreeNode root = new TreeNode($"[100.0%] {root_path}");
            root.Tag = root_path;

            foreach (FileIndexorNode n in list)
            {
                string tag = Path.GetFileName(n.Path.Remove(n.Path.Length - 1));
                string full_path = Path.Combine(root_path, n.Path);
                if (!filesize)
                    make_node(root.Nodes, $"[{((double)n.GetTotalSize() * 100 / total_size).ToString("0.0") + "%"}] {tag}", full_path);
                else
                    make_node(root.Nodes, $"[{CapacityFormat(n.GetTotalSize())}] {tag}", full_path);
                make_tree(n, root.Nodes[root.Nodes.Count - 1], total_size, full_path);
            }

            if (cbFileIndexing.Checked)
            {
                var file_list = node.Files; //new DirectoryInfo(fn.Path).GetFiles().ToList();
                file_list.Sort((a, b) => b.Length.CompareTo(a.Length));
                foreach (FileInfo f in file_list)
                    make_node(root.Nodes, $"[{((double)f.Length * 100 / total_size).ToString("0.0") + "%"}] {f.Name}", Path.Combine(root_path, f.Name), true);
            }

            this.Post(() => tvFs.Nodes.Add(root));
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }
        
        private void make_tree(FileIndexorNode fn, TreeNode tn, UInt64 total_size, string root_path)
        {
            var list = fn.Nodes;
            list.Sort((a, b) => b.GetTotalSize().CompareTo(a.GetTotalSize()));

            foreach (FileIndexorNode n in list)
            {
                string tag = Path.GetFileName(n.Path.Remove(n.Path.Length - 1));
                string full_path = Path.Combine(root_path, n.Path);
                if (!filesize)
                    make_node(tn.Nodes, $"[{((double)n.GetTotalSize() * 100 / total_size).ToString("0.0") + "%"}] {tag}", full_path);
                else
                    make_node(tn.Nodes, $"[{CapacityFormat(n.GetTotalSize())}] {tag}", full_path);
                make_tree(n, tn.Nodes[tn.Nodes.Count - 1], total_size, Path.Combine(root_path, n.Path));
            }

            if (cbFileIndexing.Checked)
            {
                var file_list = fn.Files; //new DirectoryInfo(fn.Path).GetFiles().ToList();
                file_list.Sort((a, b) => b.Length.CompareTo(a.Length));
                foreach (FileInfo f in file_list)
                    make_node(tn.Nodes, $"[{((double)f.Length * 100 / total_size).ToString("0.0") + "%"}] {f.Name}", Path.Combine(root_path, f.Name), true);
            }
        }

        private void make_node(TreeNodeCollection tnc, string path, string full_path, bool file = false)
        {
            TreeNode tn = new TreeNode(path);
            tn.Tag = full_path;
            tnc.Add(tn);

            if (file && cbIcon.Checked)
            {
                this.Post(() =>
                {
                    int index = 0;
                    try { index = iconListManager.AddFileIcon(full_path); } catch { };
                    tn.ImageIndex = index;
                    tn.SelectedImageIndex = index;
                });
            }
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

        private void tvFs_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            //e.Graphics.DrawString(e.Node.Text, Font, Brushes.Black, e.Bounds.Location + new Size(3 + e.Node.Level * 20, 3));
            e.DrawDefault = true;
        }

        #region 트리 사이즈 기능

        private void 열기OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvFs.SelectedNode != null)
            {
                string path = tvFs.SelectedNode.Tag as string;
                //TreeNode ppp = tvFs.SelectedNode.Parent;
                //for (; ppp != null; ppp = ppp.Parent)
                //    path = Path.Combine(ppp.Tag as string, path);
                Process.Start("explorer.exe", path);
            }
        }

        // https://stackoverflow.com/questions/1936682/how-do-i-display-a-files-properties-dialog-from-c
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;
        public static bool ShowFileProperties(string Filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = Filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            return ShellExecuteEx(ref info);
        }

        private void 속성RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvFs.SelectedNode != null)
            {
                string path = tvFs.SelectedNode.Tag as string;
                //TreeNode ppp = tvFs.SelectedNode.Parent;
                //for (; ppp != null; ppp = ppp.Parent)
                //    path = Path.Combine(ppp.Tag as string, path);
                ShowFileProperties(path);
            }
        }
        #endregion

    }
}
