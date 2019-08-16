/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Console;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using Koromo_Copy_UX.Domain;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.WindowsAPICodePack.Taskbar;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Koromo_Copy_UX
{
    /// <summary>
    /// DownloadSpace.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DownloadSpace : UserControl
    {
        public static DownloadSpace Instance;
        private static DownloadDataGridViewModel view_model;
        DispatcherTimer timer;

        public DownloadSpace()
        {
            InitializeComponent();
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);

            Instance = this;
            InstanceMonitor.Instances.Add("downloadspace", Instance);
            DataContext = view_model = new DownloadDataGridViewModel();

            DownloadGroup.Instance.Complete += Instance_Complete;
            DownloadGroup.Instance.DownloadComplete += Instance_DownloadComplete;
            DownloadGroup.Instance.DownloadStatus += Instance_DownloadStatus;
            DownloadGroup.Instance.NotifySize += Instance_NotifySize;
            DownloadGroup.Instance.Retry += Instance_Retry;
            DownloadGroup.Instance.CompleteGroup += Instance_CompleteGroup;

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();

            init();
        }

        #region Live Chart
        
        public SeriesCollection LastHourSeries { get; set; }

        private void init()
        {
            var ls =
                new LineSeries
                {
                    AreaLimit = -10,
                    Values = new ChartValues<ObservableValue>
                    {
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0)
                    }
                };
            ls.Foreground = Brushes.Pink;
            ls.Stroke = Brushes.Pink;
            ls.LineSmoothness = 1;
            ls.PointGeometrySize = 0;
            ls.Fill = Brushes.Transparent;
            LastHourSeries = new SeriesCollection { ls };
            NetworkChart.Series = LastHourSeries;
            NetworkChart.AxisY.Clear();
            NetworkChart.AxisY.Add(new Axis
            {
                LabelFormatter = value => {
                    var tt = (int)value;
                    if (tt == 0) return "0 KB";
                    if (tt < 1024)
                        return value.ToString("#,#") + " KB";
                    else
                        return (value/1024).ToString("#,#") + " MB";
                    },
                Separator = new LiveCharts.Wpf.Separator(),
                MinValue = 0,
            });
        }
        
        #endregion

        private void Instance_Retry(object sender, Tuple<string, object> e)
        {
            Koromo_Copy.Monitor.Instance.Push("[Retry Download] " + e.Item1);
        }

        long download_size = 0;
        object size_lock = new object();
        private void Instance_NotifySize(object sender, Tuple<string, long, object> e)
        {
            lock (size_lock) download_size += e.Item2;
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                TotalSize.Text = ((double)download_size / 1000 / 1000).ToString("#,#.#") + " MB";
            }));
        }

        long status_size = 0;
        object status_lock = new object();
        private void Instance_DownloadStatus(object sender, Tuple<string, int, object> e)
        {
            lock (status_lock) status_size += e.Item2;
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                DownloadSize.Text = ((double)status_size / 1000 / 1000).ToString("#,#.#") + " MB";
            }));
        }

        private void Instance_DownloadComplete(object sender, EventArgs e)
        {
            MainWindow.Instance.FadeOut_MiddlePopup((string)FindResource("msg_download_complete"), false);
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            }));
        }

        private void Instance_Complete(object sender, Tuple<string, string, object> e)
        {
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                Progress.Value += 1;
                Status.Text = $"{Progress.Value} / {Progress.Maximum}";
                view_model.Items.Remove(view_model.Items.Where(x => x.경로 == e.Item2).ToList()[0]);
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                TaskbarManager.Instance.SetProgressValue((int)Progress.Value, (int)Progress.Maximum);
            }));
            
            Monitor.Instance.Push("[Complete File] " + e.Item2 + " " + e.Item1);
        }

        object log_lock = new object();
        private void Instance_CompleteGroup(object sender, Tuple<string, object> e)
        {
            var tuple = e.Item2 as Tuple<string, IArticle>;
            Monitor.Instance.Push("[Complete Group] " + (tuple.Item1));

            if (tuple.Item2 is HitomiArticle ha)
            {
                lock(log_lock)
                {
                    HitomiLog.Instance.AddArticle(ha);
                    HitomiLog.Instance.Save();

                    if (Settings.Instance.Hitomi.SaveJsonFile)
                    {
                        var hj = new HitomiJson(tuple.Item1);
                        hj.SetModelFromArticle(ha);
                        hj.Save();
                    }
                }
            }

            if (Settings.Instance.Model.AutoZip)
            {
                Monitor.Instance.Push("[Zip Start] " + (tuple.Item1));
                MainWindow.Instance.ZipCountUp();
                Task.Run(() => Zip(tuple.Item1));
            }
        }

        private void Zip(string address)
        {
            address = address.Remove(address.Length - 1);
            if (File.Exists($"{address}.zip"))
                File.Delete($"{address}.zip");
            try
            {
                if (!Directory.Exists(address))
                    throw new Exception($"'{address}' directory has not found!");
                ZipFile.CreateFromDirectory(address, $"{address}.zip");
                Directory.Delete(address, true);
                Monitor.Instance.Push("[Zip End] " + address);
            }
            catch (Exception e) { Monitor.Instance.Push("[Zip Error]" + e.Message + "\r\n" + e.StackTrace); }
            MainWindow.Instance.ZipCountDown();
        }

        long latest_status_size = 0;
        private void Timer_Tick(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                if (status_size == latest_status_size)
                    DownloadSpeed.Text = "0 KB/S";
                else
                    DownloadSpeed.Text = ((double)(status_size - latest_status_size) / 1000).ToString("#,#.#") + " KB/S";
                LastHourSeries[0].Values.Add(new ObservableValue(((double)(status_size - latest_status_size) / 1000)));
                LastHourSeries[0].Values.RemoveAt(0);
                latest_status_size = status_size;
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Pause.Content.ToString() == FindResource("pause"))
            {
                DownloadGroup.Instance.Preempt();
                MainWindow.Instance.FadeOut_MiddlePopup((string)FindResource("msg_download_pause"), false);
                Pause.Content = FindResource("resume");
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Paused);
            }
            else
            {
                DownloadGroup.Instance.Reactivation();
                MainWindow.Instance.FadeOut_MiddlePopup((string)FindResource("msg_download_resume"), false);
                Pause.Content = FindResource("pause");
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            }
        }
        
        int index_count = 0;
        object count_lock = new object();
        public void RequestDownload(string title, string[] urls, string[] paths, SemaphoreExtends se, string folder, IArticle article)
        {
            lock (count_lock)
            {
                if (!Settings.Instance.Model.DownloadWithRawFileName)
                    paths = PathFilenameSorter.Sort(paths);
                for (int i = 0; i < urls.Length; i++)
                {
                    Application.Current.Dispatcher.Invoke(new Action(
                    delegate
                    {
                        view_model.Items.Add(new DownloadDataGridItemViewModel
                        {
                            인덱스 = (++index_count).ToString(),
                            제목 = title,
                            경로 = paths[i]
                        });

                        Progress.Maximum += 1;
                        Status.Text = $"{Progress.Value} / {Progress.Maximum}";
                    }));
                }
                DownloadGroup.Instance.Add(urls, paths, Tuple.Create(folder, article), null, se);
            }
        }
    }
}
