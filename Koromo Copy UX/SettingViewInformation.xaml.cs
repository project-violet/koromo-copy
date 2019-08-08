/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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

namespace Koromo_Copy_UX
{
    /// <summary>
    /// SettingViewInformation.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingViewInformation : UserControl
    {
        public SettingViewInformation()
        {
            InitializeComponent();
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);

            VersionText.Text += Koromo_Copy.Version.Text + " (x64)";
            UpdateSyncDate();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void UpdateSyncDate()
        {
            if (HitomiIndex.Instance.CheckMetadataExist())
            {
                var dt = HitomiIndex.Instance.DateTimeMetadata();
                var dd = (DateTime.Now - dt).Days;
                var dh = (DateTime.Now - dt).Hours;
                SyncDate.Text = $"{dt.ToString("yyyy년 MM월 dd일 ")} ({dd}일 {dh}시간 지남)";
                if (dd > 0)
                {
                    NNSync.Visibility = Visibility.Visible;
                }
            }
            else
            {
                SyncDate.Text = "데이터 없음";
            }
        }

        DispatcherTimer timer = null;
        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            SyncButton.IsEnabled = false;
            SyncProgress.IsIndeterminate = true;
            MainWindow.Instance.FadeOut_MiddlePopup("데이터 동기화를 시작합니다!");

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

#if false
            Task t1 = new Task(() => DownloadThread("https://github.com/dc-koromo/e-archive/releases/download/metadata/metadata.compress"));
            t1.Start();
            await t1;
#endif
            Task t2 = new Task(() => DownloadThread("https://github.com/dc-koromo/e-archive/raw/master/index-metadata.compress"));
            t2.Start();
            await t2;

            if (Settings.Instance.Hitomi.UsingOriginalTitle)
            {
                Task t3 = new Task(() => DownloadThread("https://raw.githubusercontent.com/dc-koromo/e-archive/master/origin-title.compress"));
                t3.Start();
                await t3;
            }

            HitomiIndex.Instance.WriteData();
            
            SyncButton.IsEnabled = true;
            UpdateSyncDate();
            MainWindow.Instance.FadeOut_MiddlePopup("데이터 동기화 완료!", false);
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
        
        public List<HitomiIndexMetadata> metadata_collection = new List<HitomiIndexMetadata>();
        public HitomiIndexDataModel hidm;

        private object post_length_lock = new object();
        private object post_status_lock = new object();
        private object start_lock = new object();

        long download_size = 0;
        long status_size = 0;
        volatile int load_count = 0;
        int complete_count = 0;

        private void WaitThread()
        {
            lock (start_lock)
            {
                while (load_count < 1)
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
#if false
                SyncLoad.Text = $"[{load_count}/{number_of_gallery_jsons}]";
#endif
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
                            
                            if (url == "https://github.com/dc-koromo/e-archive/raw/master/index-metadata.compress")
                            {
                                lock (metadata_collection)
                                {
                                    var str = (outputStream as MemoryStream).ToArray().UnzipByte();
                                    HitomiIndex.Instance.LoadFromBytes(str);
                                }
                            }
                            else if (url == "https://raw.githubusercontent.com/dc-koromo/e-archive/master/origin-title.compress")
                            {
                                File.WriteAllBytes("origin-title.json", (outputStream as MemoryStream).ToArray().UnzipByte());
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
        }

    }
}
