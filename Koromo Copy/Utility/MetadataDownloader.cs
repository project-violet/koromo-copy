/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Koromo_Copy.Utility
{
    public partial class MetadataDownloader : Form
    {
        public MetadataDownloader()
        {
            InitializeComponent();
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            button1.Enabled = false;
            ServicePointManager.DefaultConnectionLimit = 999999999;

            Thread thread = new Thread(WaitThread);
            thread.Start();

            timer1.Start();
            await Task.WhenAll(Enumerable.Range(0, number_of_gallery_jsons).Select(no => Task.Run(() => DownloadThread(gallerie_json_uri(no)))));
            
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            Monitor.Instance.Push("Write file: metadata.json");
            using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "metadata.json")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, metadata_collection);
            }
            button1.Enabled = true;
        }

        public static int number_of_gallery_jsons = 20;
        public static string gallerie_json_uri(int no) => $"https://ltn.hitomi.la/galleries{no}.json";
        public List<HitomiIndexMetadata> metadata_collection = new List<HitomiIndexMetadata>();

        private object post_length_lock = new object();
        private object post_status_lock = new object();
        private object start_lock = new object();

        long download_size = 0;
        long status_size = 0;
        int load_count = 0;
        int complete_count = 0;

        private void PostLength(long len)
        {
            download_size += len;
            load_count++;
            this.Post(() => label4.Text = ((double)download_size / 1000 / 1000).ToString("#,#.#") + " MB");
            this.Post(() => label8.Text = $"[{load_count}/{number_of_gallery_jsons}]");
        }

        private void PostStatus(int read)
        {
            status_size += read;
            this.Post(() => label5.Text = ((double)status_size / 1000 / 1000).ToString("#,#.#") + " MB");
            this.Post(() => label2.Text = ((double)status_size / download_size * 100).ToString("#.#######") + "%");
            this.Post(() => progressBar1.Value = (int)((double)status_size / download_size * 100));
        }

        private void PostStatusM(int read)
        {
            status_size -= read;
            this.Post(() => label5.Text = ((double)status_size / 1000 / 1000).ToString("#,#.#") + " MB");
            this.Post(() => label2.Text = ((double)status_size / download_size * 100).ToString("#.#######") + "%");
            this.Post(() => progressBar1.Value = (int)((double)status_size / download_size * 100));
        }

        private void DownloadThread(string url)
        {
            bool retry = false;
            int retry_count = 0;
            int read = 0;
        RETRY_LABEL:
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            new SemaphoreExtends().RunPass(ref request);
            request.Timeout = 10000;
            request.KeepAlive = true;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if ((response.StatusCode == HttpStatusCode.OK ||
                        response.StatusCode == HttpStatusCode.Moved ||
                        response.StatusCode == HttpStatusCode.Redirect))
                    {
                        using (Stream inputStream = response.GetResponseStream())
                        using (Stream outputStream = new MemoryStream())
                        {
                            byte[] buffer = new byte[131072];
                            int bytesRead;
                            if (!retry) lock(post_length_lock) PostLength(response.ContentLength);
                            lock (start_lock) { }
                            do
                            {
                                bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                                outputStream.Write(buffer, 0, bytesRead);
                                read += bytesRead;
                                lock (post_status_lock) PostStatus(bytesRead);
                            } while (bytesRead != 0);

                            lock (metadata_collection)
                            {
                                string str = Encoding.UTF8.GetString((outputStream as MemoryStream).ToArray());
                                metadata_collection.AddRange(JsonConvert.DeserializeObject<IEnumerable<HitomiIndexMetadata>>(str));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                lock(post_status_lock) PostStatusM(read);
                Monitor.Instance.Push($"Retry: {++retry_count}th {url} :\r\nMessage: {e.Message}\r\nStackTrace: {e.StackTrace}");
                read = 0;
                retry = true;
                goto RETRY_LABEL;
            }

            Interlocked.Increment(ref complete_count);
            Monitor.Instance.Push($"Download complete: [{complete_count.ToString("00")}/{number_of_gallery_jsons}] {url}");
        }

        private void WaitThread()
        {
            lock (start_lock)
            {
                while (load_count < 20)
                {
                    Thread.Sleep(100);
                }
            }
        }
        
        int seconds = 0;
        long prev_bytes = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            seconds += 1;
            label11.Text = new TimeSpan(0, 0, seconds).ToString();

            long remain_bytes = download_size - status_size;
            long term_bytes = status_size - prev_bytes;
            
            if (term_bytes != 0)
            {
                label12.Text = new TimeSpan(0, 0, (int)(remain_bytes / term_bytes)).ToString();
            }
            else
            {
                label12.Text = "Infinite";
            }
            
            if (seconds > 5 && download_size == status_size)
            {
                label12.Text = "Complete!";
                timer1.Stop();
            }

            prev_bytes = status_size;
        }
    }
}
