using Hitomi_Copy;
using Hitomi_Copy_2;
using Koromo_Copy;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZipViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private const string MenuName = "Directory\\shell\\AZipViewer";
        public const string Command = "Directory\\shell\\AZipViewer\\command";

        private void Form1_Load(object sender, EventArgs e)
        {
            ColumnSorter.InitListView(lvMyTagRank);
            var x = Environment.GetCommandLineArgs();
            
            if (x.Length > 1)
            {
                load_folder(x[1]);
            }
            else
            {
                if (Registry.ClassesRoot.OpenSubKey(MenuName) != null)
                    return;

                if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                {
                    MessageBox.Show("프로그램을 등록하려면 관리자권한으로 시작하세요!", "Zip Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }

                RegistryKey regmenu = null;
                RegistryKey regcmd = null;
                try
                {
                    regmenu = Registry.ClassesRoot.CreateSubKey(MenuName);
                    if (regmenu != null)
                    {
                        regmenu.SetValue("", "Zip 파일들 미리보기");
                        regmenu.SetValue("icon", "C:\\Tools\\ZipViewer.exe");
                    }
                    regcmd = Registry.ClassesRoot.CreateSubKey(Command);
                    if (regcmd != null)
                        regcmd.SetValue("", $"C:\\Tools\\ZipViewer.exe \"%1\"");
                    Directory.CreateDirectory("C:\\Tools\\");

                    if (File.Exists("C:\\Tools\\ZipViewer.exe"))
                        File.Delete("C:\\Tools\\ZipViewer.exe");

                    File.Move(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, "C:\\Tools\\ZipViewer.exe");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.ToString());
                }
                finally
                {
                    if (regmenu != null)
                        regmenu.Close();
                    if (regcmd != null)
                        regcmd.Close();
                }
                MessageBox.Show("프로그램이 정상적으로 등록되었습니다!", "Zip Viewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);

        public static int ComparePath(string addr1, string addr2)
        {
            return StrCmpLogicalW(addr1, addr2);
        }

        private async void load_folder(string dir)
        {
            prefix = Text = "ZipViewer by DC Koromo - " + Path.GetFileName(dir);
            flowLayoutPanel1.Controls.Clear();
            var list = Directory.GetFiles(dir).ToList();
            list.Sort((x, y) => ComparePath(y, x));
            tags = new Dictionary<string, int>();
            lvMyTagRank.Items.Clear();
            count_load = max_load = 0;

            List<Task> tasks = new List<Task>();
            
            foreach (var files in list)
            {
                if (!files.EndsWith(".zip"))
                    continue;

                tasks.Add(Task.Run(() => load_zip(files)));
                Interlocked.Increment(ref max_load);
            }

            await Task.WhenAll(tasks);

            var result = tags.ToList();
            result.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            List<ListViewItem> lvil = new List<ListViewItem>();
            for (int i = 0; i < result.Count; i++)
            {
                lvil.Add(new ListViewItem(new string[]
                {
                     result[i].Key,
                     result[i].Value.ToString()
                }));
            }
            this.Post(() => lvMyTagRank.Items.AddRange(lvil.ToArray()));
        }

        Dictionary<string, int> tags = new Dictionary<string, int>();
        string prefix;
        int count_load;
        int max_load;

        private void load_zip(string files)
        {
            IPicElement pe;
            pe = new PicElement(this);

            pe.Path = files;
            pe.Label = Path.GetFileNameWithoutExtension(files);

            using (var zip = ZipFile.Open(files, ZipArchiveMode.Read))
            {
                string tmp = Path.GetTempFileName();
                if (!zip.Entries[0].Name.EndsWith(".json"))
                    zip.Entries[0].ExtractToFile(tmp, true);
                else
                    zip.Entries[1].ExtractToFile(tmp, true);
                try
                {
                    pe.Log = JsonConvert.DeserializeObject<HitomiJsonModel>(new StreamReader(zip.GetEntry("Info.json").Open()).ReadToEnd());
                    if (pe.Log.Tags != null)
                        lock (tags)
                        {
                            foreach (var tag in pe.Log.Tags)
                            {
                                if (tags.ContainsKey(tag))
                                    tags[tag] += 1;
                                else
                                    tags.Add(tag, 1);
                            }
                        }
                }
                catch(Exception e)
                {
                }
                pe.SetImageFromAddress(tmp, 150, 200);
            }

            pe.Font = this.Font;
            Interlocked.Increment(ref count_load);
            this.Post(() => Text = $"{prefix} [{count_load}/{max_load}]"); 
            this.Post(() => flowLayoutPanel1.Controls.Add(pe as Control));
            this.Post(() => SortThumbnail());
        }

        private void SortThumbnail()
        {
            List<Control> controls = new List<Control>();
            for (int i = 0; i < flowLayoutPanel1.Controls.Count; i++)
                controls.Add(flowLayoutPanel1.Controls[i]);
            controls.Sort((a, b) => ComparePath((b as IPicElement).Path, (a as IPicElement).Path));
            for (int i = 0; i < controls.Count; i++)
                flowLayoutPanel1.Controls.SetChildIndex(controls[i], i);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
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

        private void flowLayoutPanel1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] folder = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (folder.Length>1)
                {
                    MessageBox.Show("하나의 폴더만 끌어오세요!", "Zip Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    load_folder(folder[0]);
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

        private void lvMyTagRank_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvMyTagRank.SelectedItems.Count > 0)
            {
                string[] tags = lvMyTagRank.SelectedItems.OfType<ListViewItem>().Select(x => x.SubItems[0].Text).ToArray();
                flowLayoutPanel1.SuspendLayout();
                for (int i = 0; i < flowLayoutPanel1.Controls.Count; i++)
                {
                    PicElement pe = flowLayoutPanel1.Controls[i] as PicElement;
                    if (tags.All(x => pe.Log.Tags != null && pe.Log.Tags.Contains(x)))
                        pe.Selected = true;
                    else
                        pe.Selected = false;
                }
                flowLayoutPanel1.ResumeLayout();
            }
            else
            {
                for (int i = 0; i < flowLayoutPanel1.Controls.Count; i++)
                {
                    (flowLayoutPanel1.Controls[i] as PicElement).Selected = false;
                }
            }
        }
    }
}
