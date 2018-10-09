/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Etier.IconHelper;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Controls;
using Koromo_Copy.Fs;
using Koromo_Copy.Fs.FileIcon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Koromo_Copy.Utility
{
    public partial class HitomiExplorer : Form
    {
        ImageList smallImageList = new ImageList();
        ImageList largeImageList = new ImageList();
        IconListManager iconListManager;
        FileIndexor indexor;

        public HitomiExplorer()
        {
            InitializeComponent();

            smallImageList.ColorDepth = ColorDepth.Depth32Bit;
            largeImageList.ColorDepth = ColorDepth.Depth32Bit;

            smallImageList.ImageSize = new System.Drawing.Size(16, 16);
            largeImageList.ImageSize = new System.Drawing.Size(32, 32);

            smallImageList.Images.Add(FolderIcon.GetFolderIcon(
                FolderIcon.IconSize.Small,
                FolderIcon.FolderType.Closed));

            iconListManager = new IconListManager(smallImageList, largeImageList);

            PathTree.ImageList = smallImageList;
        }

        private void HitomiExplorer_Load(object sender, EventArgs e)
        {
            ColumnSorter.InitListView(AvailableList);

            string[] splits = Settings.Instance.Hitomi.Path.Split('\\');
            string path = "";
            for (int i = 0; i < splits.Length; i++)
            {
                if (!(splits[i].Contains("{") && splits[i].Contains("}")))
                    path += splits[i] + "\\";
                else
                    break;
            }
            tbPath.Text = path;
        }

        #region 파일 시스템 리스팅

        private async void bOpen_ClickAsync(object sender, EventArgs e)
        {
            indexor = null;
            indexor = new FileIndexor();
            PathTree.Nodes.Clear();
            AvailableList.Items.Clear();

            Monitor.Instance.Push($"[Hitomi Explorer] Open directory '{tbPath.Text}'");
            await indexor.ListingDirectoryAsync(tbPath.Text);
            Monitor.Instance.Push($"[Hitomi Explorer] Complete open! DirCount={indexor.Count}");

            listing(indexor);
        }

        private void listing(FileIndexor fi)
        {
            FileIndexorNode node = fi.GetRootNode();
            foreach (FileIndexorNode n in node.Nodes)
            {
                make_node(PathTree.Nodes, Path.GetFileName(n.Path.Remove(n.Path.Length - 1)));
                make_tree(n, PathTree.Nodes[PathTree.Nodes.Count - 1]);
            }
            foreach (FileInfo f in new DirectoryInfo(node.Path).GetFiles())
                make_node(PathTree.Nodes, f.Name);
        }

        private void make_tree(FileIndexorNode fn, TreeNode tn)
        {
            foreach (FileIndexorNode n in fn.Nodes)
            {
                make_node(tn.Nodes, Path.GetFileName(n.Path.Remove(n.Path.Length - 1)));
                make_tree(n, tn.Nodes[tn.Nodes.Count - 1]);
            }
            foreach (FileInfo f in new DirectoryInfo(fn.Path).GetFiles())
                make_node(tn.Nodes, f.Name);
        }

        private void make_node(TreeNodeCollection tnc, string path)
        {
            TreeNode tn = new TreeNode(path);
            tnc.Add(tn);
            string fullpath = Path.Combine(tbPath.Text, tn.FullPath);
            if (File.Exists(fullpath))
            {
                int index = iconListManager.AddFileIcon(fullpath);
                tn.ImageIndex = index;
                tn.SelectedImageIndex = index;
            }
            else
            {
                tn.ImageIndex = 0;
            }
        }

        #endregion

        #region 규칙 적용 및 추출

        List<Tuple<string, string, HitomiMetadata?>> metadatas = new List<Tuple<string, string, HitomiMetadata?>>();
        List<KeyValuePair<string, int>> artist_rank;
        int visit_count = 0;
        int available_count = 0;

        bool show_unavailables = true;
        bool deal_group_with_artist = false;
        bool search_subfiles_whenever = false;
        bool deal_artists_with_parent_folder = true;

        private void bApply_Click(object sender, EventArgs e)
        {
            var regexs = tbRule.Text.Split(',').ToList().Select(x => new Regex(x.Trim())).ToList();
            metadatas.Clear();
            visit_count = 0;
            available_count = 0;
            foreach (var node in PathTree.Nodes.OfType<TreeNode>())
                RecursiveVisit(node, regexs);
            Monitor.Instance.Push($"[Hitomi Explorer] Apply! visit_count={visit_count}, available_count={available_count}");

            List<ListViewItem> lvil = new List<ListViewItem>();
            for (int i = 0; i < metadatas.Count; i++)
            {
                if (show_unavailables == false && !metadatas[i].Item3.HasValue) continue;
                lvil.Add(new ListViewItem(new string[]
                {
                    (i+1).ToString(),
                    metadatas[i].Item1,
                    metadatas[i].Item2,
                    metadatas[i].Item3.HasValue.ToString()
                }));
            }
            AvailableList.Items.Clear();
            AvailableList.Items.AddRange(lvil.ToArray());

            //////////////////////////////////////////////////

            Dictionary<string, int> artist_count = new Dictionary<string, int>();
            foreach (var md in metadatas)
            {
                if (!deal_artists_with_parent_folder)
                {
                    if (md.Item3.HasValue && md.Item3.Value.Artists != null)
                        foreach (var artist in md.Item3.Value.Artists)
                            if (artist_count.ContainsKey(artist))
                                artist_count[artist] += 1;
                            else
                                artist_count.Add(artist, 1);
                    if (deal_group_with_artist && md.Item3.HasValue && md.Item3.Value.Groups != null)
                        foreach (var group in md.Item3.Value.Groups)
                        {
                            string tmp = "group:" + group;
                            if (artist_count.ContainsKey(tmp))
                                artist_count[tmp] += 1;
                            else
                                artist_count.Add(tmp, 1);
                        }
                }
                else
                {
                    var split = md.Item1.Split('\\');
                    string parent_folder = split.Length >= 2 ? split[split.Length - 2] : "";
                    if (artist_count.ContainsKey(parent_folder))
                        artist_count[parent_folder] += 1;
                    else
                        artist_count.Add(parent_folder, 1);
                }
            }

            artist_rank = artist_count.ToList();
            artist_rank.Sort((a, b) => b.Value.CompareTo(a.Value));
        }

        private void RecursiveVisit(TreeNode node, List<Regex> regex)
        {
            string match = "";
            if (regex.Any(x => {
                if (!x.Match(Path.GetFileNameWithoutExtension(node.Text)).Success) return false;
                match = x.Match(Path.GetFileNameWithoutExtension(node.Text)).Groups[1].Value;
                return true;
            }))
            {
                metadatas.Add(new Tuple<string, string, HitomiMetadata?>(node.FullPath, match, HitomiDataAnalysis.GetMetadataFromMagic(match)));
                available_count += 1;
                if (!search_subfiles_whenever)
                    return;
            }

            visit_count += 1;

            foreach (var cnode in node.Nodes.OfType<TreeNode>())
                RecursiveVisit(cnode, regex);
        }

        #endregion

    }
}
