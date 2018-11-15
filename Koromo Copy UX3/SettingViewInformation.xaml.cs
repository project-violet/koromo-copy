/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// SettingViewInformation.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingViewInformation : UserControl
    {
        public SettingViewInformation()
        {
            InitializeComponent();

            VersionText.Text += Koromo_Copy.Version.Text;
            UpdateSyncDate();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void UpdateSyncDate()
        {
            if (HitomiData.Instance.CheckMetadataExist())
            {
                var dt = HitomiData.Instance.DateTimeMetadata();
                var dd = (DateTime.Now - dt).Days;
                var dh = (DateTime.Now - dt).Hours;
                SyncDate.Text = $"{dt.ToString("yyyy년 MM월 dd일 ")} ({dd}일 {dh}시간 지남)";
                if (dd > 0)
                {
                    NNSync.Visibility = Visibility.Visible;
                }
            }
        }

        DispatcherTimer timer = null;
        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            SyncButton.IsEnabled = false;
            SyncProgress.IsIndeterminate = true;

            download_size = 0;
            status_size = 0;
            load_count = 0;
            complete_count = 0;
            SyncProgress.Value = 0;
            seconds = 0;
            prev_bytes = 0;
            metadata_collection.Clear();

            Thread thread = new Thread(WaitThread);
            thread.Start();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
            timer.Start();
            await Task.WhenAll(Enumerable.Range(0, number_of_gallery_jsons).Select(no => Task.Run(() => DownloadThread(gallerie_json_uri(no)))));

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            Koromo_Copy.Monitor.Instance.Push("Write file: metadata.json");
            using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "metadata.json")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, metadata_collection);
            }

            HitomiData.Instance.metadata_collection = metadata_collection;
            HitomiData.Instance.LoadHiddendataJson();

            SyncButton.IsEnabled = true;
            UpdateSyncDate();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            seconds += 1;
            SyncRegret.Text = new TimeSpan(0, 0, seconds).ToString();

            long remain_bytes = download_size - status_size;
            long term_bytes = status_size - prev_bytes;

            if (term_bytes != 0)
            {
                SyncRemain.Text = new TimeSpan(0, 0, (int)(remain_bytes / term_bytes)).ToString();
            }
            else
            {
                SyncRemain.Text = "Infinite";
            }

            if (seconds > 5 && download_size == status_size)
            {
                SyncRemain.Text = "Complete!";
                timer.Stop();
            }

            prev_bytes = status_size;
        }

        int seconds = 0;
        long prev_bytes = 0;

        public static int number_of_gallery_jsons = 20;
        public static string gallerie_json_uri(int no) => $"https://ltn.hitomi.la/galleries{no}.json";
        public List<HitomiMetadata> metadata_collection = new List<HitomiMetadata>();

        private object post_length_lock = new object();
        private object post_status_lock = new object();
        private object start_lock = new object();

        long download_size = 0;
        long status_size = 0;
        int load_count = 0;
        int complete_count = 0;

        private void WaitThread()
        {
            lock (start_lock)
            {
                while (load_count < 20)
                {
                    Thread.Sleep(100);
                }
            }
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                SyncProgress.IsIndeterminate = false;
            }));
        }

        private void PostLength(long len)
        {
            download_size += len;
            load_count++;
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                SyncTotalBytes.Text = ((double)download_size / 1000 / 1000).ToString("#,#.#") + " MB";
                SyncLoad.Text = $"[{load_count}/{number_of_gallery_jsons}]";
            }));
        }

        private void PostStatus(int read)
        {
            status_size += read;
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                SyncDownloadBytes.Text = ((double)status_size / 1000 / 1000).ToString("#,#.#") + " MB";
                SyncPercent.Text = ((double)status_size / download_size * 100).ToString("#.#######") + "%";
                SyncProgress.Value = (int)((double)status_size / download_size * 100);
            }));
        }

        private void PostStatusM(int read)
        {
            status_size -= read;
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                SyncDownloadBytes.Text = ((double)status_size / 1000 / 1000).ToString("#,#.#") + " MB";
                SyncPercent.Text = ((double)status_size / download_size * 100).ToString("#.#######") + "%";
                SyncProgress.Value = (int)((double)status_size / download_size * 100);
            }));
        }

        private void DownloadThread(string url)
        {
            bool retry = false;
            int retry_count = 0;
            int read = 0;
            RETRY_LABEL:
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36";
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
                            if (!retry) lock (post_length_lock) PostLength(response.ContentLength);
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
                                metadata_collection.AddRange(JsonConvert.DeserializeObject<IEnumerable<HitomiMetadata>>(str));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                lock (post_status_lock) PostStatusM(read);
                Koromo_Copy.Monitor.Instance.Push($"Retry: {++retry_count}th {url} :\r\nMessage: {e.Message}\r\nStackTrace: {e.StackTrace}");
                read = 0;
                retry = true;
                goto RETRY_LABEL;
            }

            Interlocked.Increment(ref complete_count);
            Koromo_Copy.Monitor.Instance.Push($"Download complete: [{complete_count.ToString("00")}/{number_of_gallery_jsons}] {url}");
        }

    }
}
