/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Etier.IconHelper;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Controls;
using Koromo_Copy.Fs;
using Koromo_Copy.Fs.FileIcon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
            ColumnSorter.InitListView(listView1);

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

        List<Tuple<string, string, HitomiIndexMetadata?>> metadatas = new List<Tuple<string, string, HitomiIndexMetadata?>>();
        List<KeyValuePair<string, int>> artist_rank;
        Dictionary<string, int> artist_rank_dic;
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
                        foreach (var _artist in md.Item3.Value.Artists)
                        {
                            var artist = HitomiIndex.Instance.index.Artists[_artist];
                            if (artist_count.ContainsKey(artist))
                                artist_count[artist] += 1;
                            else
                                artist_count.Add(artist, 1);
                        }
                    if (deal_group_with_artist && md.Item3.HasValue && md.Item3.Value.Groups != null)
                        foreach (var _group in md.Item3.Value.Groups)
                        {
                            var group = HitomiIndex.Instance.index.Artists[_group];
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

            artist_rank_dic = new Dictionary<string, int>();

            lvil.Clear();
            for (int i = 0; i < artist_rank.Count; i++)
            {
                lvil.Add(new ListViewItem(new string[]
                {
                    (i+1).ToString(),
                    artist_rank[i].Key,
                    artist_rank[i].Value.ToString()
                }));
                artist_rank_dic.Add(artist_rank[i].Key, i);
            }
            lvArtistPriority.Items.Clear();
            lvArtistPriority.Items.AddRange(lvil.ToArray());
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
                metadatas.Add(new Tuple<string, string, HitomiIndexMetadata?>(node.FullPath, match, HitomiDataAnalysis.GetMetadataFromMagic(match)));
                available_count += 1;
                if (!search_subfiles_whenever)
                    return;
            }

            visit_count += 1;

            foreach (var cnode in node.Nodes.OfType<TreeNode>())
                RecursiveVisit(cnode, regex);
        }

        #endregion

        #region 재배치 도구

        private string MakeDownloadDirectory(string source, string artists, HitomiIndexMetadata metadata, string extension)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            string title = metadata.Name ?? "";
            string type = metadata.Type < 0 ? "" : HitomiIndex.Instance.index.Types[metadata.Type];
            string series = "";
            //if (HitomiSetting.Instance.GetModel().ReplaceArtistsWithTitle)
            //{
            //    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            //    artists = textInfo.ToTitleCase(artists);
            //}
            if (metadata.Parodies != null) series = HitomiIndex.Instance.index.Series[metadata.Parodies[0]];
            if (title != null)
                foreach (char c in invalid) title = title.Replace(c.ToString(), "");
            if (artists != null)
                foreach (char c in invalid) artists = artists.Replace(c.ToString(), "");
            if (series != null)
                foreach (char c in invalid) series = series.Replace(c.ToString(), "");
            if (artists.StartsWith("group:"))
                artists = artists.Substring("group:".Length);

            string path = source;
            path = Regex.Replace(path, "{Title}", title, RegexOptions.IgnoreCase);
            path = Regex.Replace(path, "{Artists}", artists, RegexOptions.IgnoreCase);
            path = Regex.Replace(path, "{Id}", metadata.ID.ToString(), RegexOptions.IgnoreCase);
            path = Regex.Replace(path, "{Type}", type, RegexOptions.IgnoreCase);
            path = Regex.Replace(path, "{Date}", DateTime.Now.ToString(), RegexOptions.IgnoreCase);
            path = Regex.Replace(path, "{Series}", series, RegexOptions.IgnoreCase);
            path += extension;
            return path;
        }

        private List<Tuple<string, string>> GetResult(string source, bool rename = false)
        {
            List<Tuple<string, string>> result = new List<Tuple<string, string>>();

            foreach (var md in metadatas)
            {
                if (!md.Item3.HasValue) continue;
                string extension = Path.GetExtension(md.Item1);
                string dir = rename ? Path.GetDirectoryName(md.Item1) + '\\' : "";
                if (md.Item3.Value.Artists == null && md.Item3.Value.Groups == null)
                {
                    result.Add(new Tuple<string, string>(md.Item1, dir + MakeDownloadDirectory(source, "", md.Item3.Value, extension)));
                    continue;
                }

                List<string> artist_group = new List<string>();
                if (md.Item3.Value.Artists != null)
                    foreach (var artist in md.Item3.Value.Artists)
                        artist_group.Add(HitomiIndex.Instance.index.Artists[artist]);
                else if (md.Item3.Value.Groups != null)
                    foreach (var group in md.Item3.Value.Groups)
                        artist_group.Add(HitomiIndex.Instance.index.Groups[group]);
                //if (tgAEG.Checked == true && md.Item3.Value.Groups != null)
                //    foreach (var group in md.Item3.Value.Groups)
                //        artist_group.Add("group:" + group);

                int top_rank = 0;
                //if (md.Item3.Value.Artists != null)
                //{
                //    for (int i = 1; i < artist_group.Count; i++)
                //    {
                //        if (artist_rank_dic[artist_group[top_rank]] > artist_rank_dic[artist_group[i]])
                //            top_rank = i;
                //    }
                //}

                result.Add(new Tuple<string, string>(md.Item1, dir + MakeDownloadDirectory(source, artist_group[top_rank], md.Item3.Value, extension)));
            }
            return result;
        }

        private void bReplaceTest_Click(object sender, EventArgs e)
        {
            var result = GetResult(tbDownloadPath.Text);
            List<ListViewItem> lvil = new List<ListViewItem>();
            HashSet<string> overlapping_check = new HashSet<string>();
            for (int i = 0; i < result.Count; i++)
            {
                bool err = false;
                string err_msg = "Already exists";
                if (File.Exists(result[i].Item2)) err = true;
                else if (Directory.Exists(result[i].Item2)) err = true;
                if (overlapping_check.Contains(result[i].Item2)) { err = true; err_msg = "Overlapping"; }
                else overlapping_check.Add(result[i].Item2);
                lvil.Add(new ListViewItem(new string[]
                {
                    (i+1).ToString(),
                    result[i].Item1,
                    result[i].Item2,
                    (err ? err_msg : "")
                }));
                if (err)
                {
                    lvil[i].BackColor = Color.Orange;
                    lvil[i].ForeColor = Color.White;
                }
            }
            lvReplacerTestResult.Items.Clear();
            lvReplacerTestResult.Items.AddRange(lvil.ToArray());
        }

        private void bMove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("이 과정은 되돌릴 수 없습니다. 계속하시겠습니까?", "Hitomi Copy Article Replacer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;

            var result = GetResult(tbDownloadPath.Text);

            for (int i = 0; i < result.Count; i++)
            {
                try
                {
                    string src = Path.Combine(indexor.RootDirectory, result[i].Item1);
                    string dest = result[i].Item2;
                    string dir = Path.GetDirectoryName(result[i].Item2);
                    Directory.CreateDirectory(dir);
                    if (File.Exists(src))
                        File.Move(src, dest);
                    else
                        Directory.Move(src, dest);
                    Monitor.Instance.Push($"[Replacer] Move '{src}' => '{dest}'");
                }
                catch (Exception ex)
                {
                    Monitor.Instance.Push($"[Replacer] Error occurred! {Path.Combine(indexor.RootDirectory, result[i].Item1)} => {result[i].Item2}");
                    Monitor.Instance.Push(ex);
                }
            }
        }

        #endregion

        #region 이름 바꾸기 도구

        private void bRenameTest_Click(object sender, EventArgs e)
        {
            var result = GetResult(tbRenameRule.Text, true);
            List<ListViewItem> lvil = new List<ListViewItem>();
            HashSet<string> overlapping_check = new HashSet<string>();
            for (int i = 0; i < result.Count; i++)
            {
                bool err = false;
                string err_msg = "Alread exists";
                string dest = Path.Combine(indexor.RootDirectory, result[i].Item2);
                if (File.Exists(dest)) err = true;
                else if (Directory.Exists(dest)) err = true;
                if (overlapping_check.Contains(dest)) { err = true; err_msg = "Overlapping"; }
                else overlapping_check.Add(dest);
                lvil.Add(new ListViewItem(new string[]
                {
                    (i+1).ToString(),
                    result[i].Item1,
                    result[i].Item2,
                    (err ? err_msg : "")
                }));
                if (err)
                {
                    lvil[i].BackColor = Color.Orange;
                    lvil[i].ForeColor = Color.White;
                }
            }
            lvRenamerTestResult.Items.Clear();
            lvRenamerTestResult.Items.AddRange(lvil.ToArray());
        }

        private void bRename_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("이 과정은 되돌릴 수 없습니다. 계속하시겠습니까?", "Hitomi Copy Article Renamer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;

            var result = GetResult(tbRenameRule.Text, true);

            for (int i = 0; i < result.Count; i++)
            {
                try
                {
                    string src = Path.Combine(indexor.RootDirectory, result[i].Item1);
                    string dest = Path.Combine(indexor.RootDirectory, result[i].Item2);
                    string dir = Path.GetDirectoryName(result[i].Item2);
                    Directory.CreateDirectory(dir);
                    if (File.Exists(src))
                        File.Move(src, dest);
                    else
                        Directory.Move(src, dest);
                    Monitor.Instance.Push($"[Renamer] Move '{src}' => '{dest}'");
                }
                catch (Exception ex)
                {
                    Monitor.Instance.Push($"[Renamer] Error occurred! {Path.Combine(indexor.RootDirectory, result[i].Item1)} => {result[i].Item2}");
                    Monitor.Instance.Push(ex);
                }
            }
        }

        #endregion

        #region 로그 매칭 도구

        private void bExtract_Click(object sender, EventArgs e)
        {
            Dictionary<int, HitomiIndexMetadata?> map = new Dictionary<int, HitomiIndexMetadata?>();
            
            foreach (var tuple in metadatas)
            {
                int key = Convert.ToInt32(tuple.Item2);
                if (!map.ContainsKey(key))
                    map.Add(key, tuple.Item3);
                else
                {
                }
            }
            
            List<ListViewItem> lvil = new List<ListViewItem>();
            int i = 0;
            foreach (var h in HitomiLog.Instance.DownloadTable)
            {
                if (!map.ContainsKey(h))
                {
                    var md = HitomiDataAnalysis.GetMetadataFromMagic(h.ToString());
                    List<string> artists = new List<string>();
                    if (md.HasValue)
                    {
                        if (md.Value.Artists != null)
                            md.Value.Artists.ToList().ForEach(x => artists.Add(HitomiIndex.Instance.index.Artists[x]));
                    }

                    lvil.Add(new ListViewItem(new string[]
                    {
                            (i+1).ToString(),
                            h.ToString(),
                            string.Join(",", artists)
                    }));
                    i++;
                }
            }
            listView1.Items.Clear();
            listView1.Items.AddRange(lvil.ToArray());
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string artist = listView1.SelectedItems[0].SubItems[2].Text.Split(',')[0].Trim();

                if (!string.IsNullOrEmpty(artist))
                    Global.ShowArtistView(artist);
            }
        }

        #endregion

    }
}
