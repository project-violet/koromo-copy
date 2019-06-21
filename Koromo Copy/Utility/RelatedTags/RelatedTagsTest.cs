/* Copyright (C) 2018-2019. Hitomi Parser Developers */

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hitomi.Analysis;
using Koromo_Copy.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hitomi_Copy_3
{
    public partial class RelatedTagsTest : Form
    {
        public RelatedTagsTest()
        {
            InitializeComponent();
        }

        private void RelatedTagsTest_Load(object sender, EventArgs e)
        {
            ColumnSorter.InitListView(listView1);
        }

        int max;

        List<string> result = new List<string>();
        
        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            double var;
            if (!double.TryParse(textBox1.Text, out var))
            {
                MessageBox.Show("ㅗ");
                return;
            }
            if (var <= 0.00000001)
            {
                MessageBox.Show("0.00000001 보단 높아야 합니다.");
                return;
            }
            if (var > 1.0)
            {
                MessageBox.Show("1.0 보단 낮아야 합니다.");
                return;
            }
            textBox1.Enabled = false;
            button1.Enabled = false;
            checkBox1.Enabled = false;
            listBox1.Items.Clear();
            listView1.Items.Clear();
            progressBar1.Value = 0;

            //
            //  계산 시작
            //

            label2.Text = "초기화 중...";
            await Task.Run(() => HitomiAnalysisRelatedTags.Instance.Initialize());
            HitomiAnalysisRelatedTags.Instance.Threshold = var;
            max = progressBar1.Maximum = HitomiAnalysisRelatedTags.Instance.tags_list.Count;

            await Task.WhenAll(Enumerable.Range(0, Environment.ProcessorCount).Select(no => Task.Run(() => process(no))));
            Monitor.Instance.Push($"[Related Tags] Merge...");
            await Task.Run(() => HitomiAnalysisRelatedTags.Instance.Merge());
            Monitor.Instance.Push($"[Related Tags] Complete!");

            //
            //  정렬을 위한 계산
            //

            var tag_count = HitomiAnalysisRelatedTags.Instance.tags_list.ToList();
            tag_count.Sort((a, b) => b.Value.Count.CompareTo(a.Value.Count));
            for (int i = tag_count.Count-1; i >=0; i--)
                if (!HitomiAnalysisRelatedTags.Instance.result.ContainsKey(tag_count[i].Key))
                    tag_count.RemoveAt(i);

            //
            //  결과 표시
            //

            listBox1.SuspendLayout();
            result.Clear();
            foreach (var tag in tag_count)
            {
                listBox1.Items.Add($"{Regex.Replace(tag.Key, " ", "_")} ({tag.Value.Count})");
                result.Add($"{Regex.Replace(tag.Key, " ", "_")} ({tag.Value.Count})");
            }
            listBox1.ResumeLayout();

            textBox1.Enabled = true;
            checkBox1.Enabled = true;
            button1.Enabled = true;
            label2.Text = $"분석 완료됨";
        }
        
        private void process(int i)
        {
            int min = this.max / Environment.ProcessorCount * i;
            int max = this.max / Environment.ProcessorCount * (i+1);
            if (max > this.max)
                max = this.max;

            Monitor.Instance.Push($"[Related Tags] {min}/{max} process start!");
            List<Tuple<string, string, double>> result = new List<Tuple<string, string, double>>();

            for (int j = max - 1; j >= min; j--)
            {
                result.AddRange(HitomiAnalysisRelatedTags.Instance.Intersect(j));
                this.Post(() => progressBar1.Value++);
                this.Post(() => label2.Text = $"{progressBar1.Value}/{this.max} 분석완료");
            }

            HitomiAnalysisRelatedTags.Instance.results.AddRange(result);
            Monitor.Instance.Push($"[Related Tags] {min}/{max} process finish!");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            HitomiAnalysisRelatedTags.Instance.IncludeFemaleMaleOnly = checkBox1.Checked;
        }

        #region 리스트 박스

        private async void listBox1_MouseDoubleClickAsync(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string tag = Regex.Replace(listBox1.GetItemText(listBox1.SelectedItem).Split(' ')[0], "_", " ");

                List<ListViewItem> items = new List<ListViewItem>();
                int i = 1;
                foreach (var v in HitomiAnalysisRelatedTags.Instance.result[tag])
                {
                    int gallery_count = 0;
                    if (items.Count < 400) gallery_count = await GetContainsGalleriesCount(tag, v.Item1);
                    items.Add(new ListViewItem(new string[] { i++.ToString(), tag, v.Item1, v.Item2.ToString(), gallery_count.ToString() }));
                }
                listView1.Items.Clear();
                listView1.Items.AddRange(items.ToArray());
                if (items.Count >= 400)
                    MessageBox.Show($"검색결과가 너무 많아 일부 연산을 생략했습니다!", "Hitomi Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        private async Task<int> GetContainsGalleriesCount(string tag1, string tag2)
        {
            int[] counts = new int[Environment.ProcessorCount];
            await Task.WhenAll(Enumerable.Range(0, Environment.ProcessorCount).Select(no => Task.Run(() => {
                counts[no] = get_galleries_count(tag1, tag2, no);
            })));
            return counts.Sum();
        }

        private int get_galleries_count(string tag1, string tag2, int no)
        {
            int count = 0;
            int min = HitomiIndex.Instance.metadata_collection.Count / Environment.ProcessorCount * no;
            int max = HitomiIndex.Instance.metadata_collection.Count / Environment.ProcessorCount * (no + 1);
            if (max > HitomiIndex.Instance.metadata_collection.Count)
                max = HitomiIndex.Instance.metadata_collection.Count;
            for (int i = min; i < max; i++)
                if (HitomiIndex.Instance.metadata_collection[i].Tags != null)
                    if (HitomiIndex.Instance.metadata_collection[i].Tags.Select(x => HitomiIndex.Instance.index.Tags[x]).Contains(tag1) && HitomiIndex.Instance.metadata_collection[i].Tags.Select(x => HitomiIndex.Instance.index.Tags[x]).Contains(tag2))
                        count++;
            return count;
        }

        #endregion

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            listBox1.SuspendLayout();
            List<string> items = new List<string>();
            foreach (var s in result)
            {
                if (s.Contains(textBox2.Text)) items.Add(s);
            }
            listBox1.Items.Clear();
            listBox1.Items.AddRange(items.ToArray());
            listBox1.ResumeLayout();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                //(new frmTagInfo(this, listView1.SelectedItems[0].SubItems[1].Text, listView1.SelectedItems[0].SubItems[2].Text)).Show();
            }
        }

        private void 비교할태그찾기FToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                textBox2.Text = "";
                string target = listView1.SelectedItems[0].SubItems[2].Text;
                foreach (var v in listBox1.Items)
                {
                    string tag = Regex.Replace(listBox1.GetItemText(v).Split(' ')[0], "_", " ");
                    if (tag == target)
                    {
                        listBox1.SelectedItem = v;
                        this.Post(() => listBox1_MouseDoubleClickAsync(null, null));
                        return;
                    }
                }
                MessageBox.Show($"'{target}'를 찾지 못했습니다. Threshold를 낮추고 다시 검색해보세요. ㅠㅠ", "Hitomi Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 비교될태그복사EToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Clipboard.SetText(listView1.SelectedItems[0].SubItems[1].Text);
            }
        }

        private void 비교ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Clipboard.SetText(listView1.SelectedItems[0].SubItems[2].Text);
            }
        }

        private void 그래프보기GToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                (new GraphViewer(listView1.SelectedItems[0].SubItems[1].Text)).Show();
            }
        }

        private void 대립태그검사IToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string target = listView1.SelectedItems[0].SubItems[1].Text;
                var ml = HitomiAnalysisRelatedTags.Instance.result[target].Select(x => x.Item1);

                List<Tuple<string, double>> result = new List<Tuple<string, double>>();
                foreach (var tuple in HitomiAnalysisRelatedTags.Instance.result)
                {
                    int intersect = tuple.Value.Select(x => x.Item1).Intersect(ml).Count();
                    int i_size = tuple.Value.Count;
                    int j_size = ml.Count();
                    double rate = (double)(intersect) / (i_size + j_size - intersect);
                    result.Add(new Tuple<string, double>(tuple.Key, 1.0 - rate));
                }

                StringBuilder builder = new StringBuilder();
                result.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                result.ForEach(x => builder.Append($"{x.Item1} ({x.Item2.ToString()})\r\n"));
                MessageBox.Show(builder.ToString());
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
    }
}
