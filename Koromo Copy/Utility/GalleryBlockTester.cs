/* Copyright (C) 2018. Hitomi Parser Developers */

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hitomi_Copy_3._403
{
    public partial class GalleryBlockTester : Form
    {
        public GalleryBlockTester()
        {
            InitializeComponent();
        }

        public void PushString(string str)
        {
            if (textBox3.InvokeRequired)
            {
                Invoke(new Action<string>(PushString), new object[] { str }); return;
            }
            textBox3.SuspendLayout();
            textBox3.AppendText(str + "\r\n");
            textBox3.ResumeLayout();
        }
        
        int status = 0;
        int maximum = 0;
        int minimum = 0;
        HashSet<int> exists = new HashSet<int>();
        List<HitomiArticle> result = new List<HitomiArticle>();
        private void GalleryBlockTester_Load(object sender, EventArgs e)
        {
            foreach (var metadata in HitomiIndex.Instance.metadata_collection)
            {
                exists.Add(metadata.ID);
            }
            textBox1.BackColor = Color.White;
            textBox1.Text = (HitomiIndex.Instance.metadata_collection[0].ID - 2000).ToString();
            textBox2.Text = (HitomiIndex.Instance.metadata_collection[0].ID + 2000).ToString();
        }

        DateTime start;
        private void button1_Click(object sender, EventArgs e)
        {
            status = minimum = textBox1.Text.ToInt32();
            maximum = textBox2.Text.ToInt32();
            progressBar1.Maximum = textBox2.Text.ToInt32() - textBox1.Text.ToInt32();
            start = DateTime.Now;
            button1.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            for (int i = 0; i < 100; i++)
            {
                lock (notify_lock) Notify();
            }
        }
        
        private void process(int i)
        {
            try
            {
                if (exists.Contains(i)) goto FINISH;
                WebClient wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                string x;
                x = wc.DownloadString("https://ltn.hitomi.la/galleryblock/" + i + ".html");
                var aa = HitomiParser.ParseGalleryBlock(x);
                try
                {
                    x = wc.DownloadString("https://hitomi.la/galleries/" + i + ".html");
                    var a2 = HitomiParser.ParseGallery(x);
                    aa.Groups = a2.Groups;
                    aa.Characters = a2.Characters;
                }
                catch { }
                result.Add(aa);
                PushString($"New! {i}");
            }
            catch (Exception ex)
            {
                Koromo_Copy.Console.Console.Instance.WriteLine(ex.Message + " " + i);
            }

        FINISH:
            
            this.Post(() => progressBar1.Value++);
            this.Post(() => label3.Text = $"{progressBar1.Value}/{maximum - minimum + 1} 분석완료");
            this.Post(() => label5.Text = $"{(new DateTime((DateTime.Now - start).Ticks * (maximum - minimum + 1 - progressBar1.Value) / progressBar1.Value)).ToString("HH시간 mm분 ss초")}");

            lock (int_lock) mtx--;
            lock (notify_lock) Notify();
        }

        private void Notify()
        {
            lock (int_lock)
            {
                int i = status;
                if (i < maximum) { Task.Run(() => process(i)); status++; mtx++; }
                if (i >= maximum && mtx == 0)
                    lock (result) File.WriteAllText("gallery_block.json", Monitor.SerializeObject(result));
            }
        }

        int mtx = 0;

        object int_lock = new object();
        object notify_lock = new object();

        private void timer1_Tick(object sender, EventArgs e)
        {
            lock (result) File.WriteAllText($"snapshot_{DateTime.Now.Ticks.ToString()}.json", Monitor.SerializeObject(result));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lock (result) File.WriteAllText("gallery_block.json", Monitor.SerializeObject(result));
            PushString("완료됨!");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<HitomiArticle> articles = JsonConvert.DeserializeObject<List<HitomiArticle>>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "hiddendata.json")));
            articles.AddRange(result);
            HashSet<string> overlap = new HashSet<string>();
            List<HitomiArticle> pure = new List<HitomiArticle>();
            foreach (var article in articles)
            {
                if (!overlap.Contains(article.Magic))
                {
                    pure.Add(article);
                    overlap.Add(article.Magic);
                }
            }

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "hiddendata.json")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, pure);
            }
            PushString("머지완료됨!");
        }
    }
}
