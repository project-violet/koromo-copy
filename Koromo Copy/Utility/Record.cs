/* Copyright (C) 2018. Hitomi Parser Developers */

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Koromo_Copy.Utility
{
    public partial class Record : Form
    {
        string artist = "";
        string id = "";

        public Record()
        {
            InitializeComponent();
        }

        public Record(string artist = "", string id = "")
        {
            InitializeComponent();

            this.artist = artist;
            this.id = id;
        }
        
        private void Record_Load(object sender, EventArgs e)
        {
            ColumnSorter.InitListView(listView1);
            List<ListViewItem> lvil = new List<ListViewItem>();
            List<Tuple<DateTime, HitomiLogModel>> dt = new List<Tuple<DateTime, HitomiLogModel>>();
            for (int i = HitomiLog.Instance.GetList().Count()-1; i >= 0; i--)
            {
                var list = HitomiLog.Instance.GetList()[i];
                if (artist != "") if (list.Artists == null || !list.Artists.Contains(artist)) continue;
                if (id != "") if (list.Id != id) continue;
                lvil.Add(new ListViewItem(new string[]
                {
                    list.Id,
                    list.Title,
                    string.Join(",", list.Artists ?? Enumerable.Empty<string>()),
                    list.Time.ToString(),
                    string.Join(",", list.Tags ?? Enumerable.Empty<string>())
                }));
                dt.Add(new Tuple<DateTime, HitomiLogModel>(list.Time, list));
            }
            listView1.Items.AddRange(lvil.ToArray());
            Text = $"{lvil.Count.ToString("#,#")}개의 기록";

            ///////////////////////////////////////////////////////////

            var dateTime = new Dictionary<int, Dictionary<int, Dictionary<int, List<Tuple<DateTime, HitomiLogModel>>>>>();
            foreach (var elem in dt)
            {
                var dtt = elem.Item1;
                if (!dateTime.ContainsKey(dtt.Year))
                    dateTime.Add(dtt.Year, new Dictionary<int, Dictionary<int, List<Tuple<DateTime, HitomiLogModel>>>>());
                if (!dateTime[dtt.Year].ContainsKey(dtt.Month))
                    dateTime[dtt.Year].Add(dtt.Month, new Dictionary<int, List<Tuple<DateTime, HitomiLogModel>>>());
                if (!dateTime[dtt.Year][dtt.Month].ContainsKey(dtt.Day))
                    dateTime[dtt.Year][dtt.Month].Add(dtt.Day, new List<Tuple<DateTime, HitomiLogModel>>());
                dateTime[dtt.Year][dtt.Month][dtt.Day].Add(elem);
            }

            var listdt = dateTime.ToList();
            listdt.Sort((a, b) => b.Key.CompareTo(a.Key));
            tvDate.SuspendLayout();
            foreach (var year in listdt)
            {
                int total_counts = 0;
                foreach (var month in year.Value)
                    foreach (var day in month.Value)
                        total_counts += day.Value.Count;

                var tn = make_node(tvDate.Nodes, $"{year.Key}년 ({total_counts})", year.Value.First().Value.First().Value.First().Item2);
                foreach (var month in year.Value)
                {
                    total_counts = 0;
                    foreach (var day in month.Value)
                        total_counts += day.Value.Count;

                    var tn2 = make_node(tn.Nodes, $"{month.Key.ToString("00")}월 ({total_counts})", month.Value.First().Value.First().Item2);
                    foreach (var day in month.Value)
                    {
                        var tn3 = make_node(tn2.Nodes, $"{day.Key.ToString("00")}일 ({day.Value.Count})", day.Value[0].Item2);
                        foreach (var elem in day.Value)
                            if (elem.Item2.Artists != null)
                                make_node(tn3.Nodes, $"({elem.Item1.Hour.ToString("00")}시 {elem.Item1.Minute.ToString("00")}분) [({elem.Item2.Id.PadLeft(7, '0')}) {string.Join(", ", elem.Item2.Artists)}] {elem.Item2.Title}", elem.Item2);
                            else
                                make_node(tn3.Nodes, $"({elem.Item1.Hour.ToString("00")}시 {elem.Item1.Minute.ToString("00")}분) [({elem.Item2.Id.PadLeft(7, '0')})] {elem.Item2.Title}", elem.Item2);
                    }
                }
            }
            tvDate.ResumeLayout();

            if (artist != "" || id != "")
            {
                tvDate.ExpandAll();
                if (tvDate.Nodes.Count > 0)
                    tvDate.Nodes[0].EnsureVisible();
            }
        }

        private TreeNode make_node(TreeNodeCollection tnc, string value, HitomiLogModel dt)
        {
            TreeNode tn = new TreeNode(value);
            tn.Tag = dt;
            tnc.Add(tn);
            return tn;
        }
        
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var hitomi_data = HitomiIndex.Instance.metadata_collection;
                string target = listView1.SelectedItems[0].SubItems[0].Text;
                foreach (var metadata in hitomi_data)
                {
                    if (metadata.ID.ToString() == target)
                    {
                        //(new frmGalleryInfo(this, metadata)).Show();
                        return;
                    }
                }
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void tvDate_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (tvDate.SelectedNode != null)
            {
                DateTime dt = ((HitomiLogModel)(tvDate.SelectedNode.Tag)).Time;
                string id = ((HitomiLogModel)(tvDate.SelectedNode.Tag)).Id;

                foreach (var lvi in listView1.Items.OfType<ListViewItem>())
                {
                    if (lvi.SubItems[0].Text == id && lvi.SubItems[3].Text == dt.ToString())
                    {
                        lvi.Selected = true;
                        lvi.Focused = true;
                        listView1.Focus();
                        lvi.EnsureVisible();
                        return;
                    }
                }
            }
        }
    }
}
