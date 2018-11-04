/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // GC 설정
            GCLatencyMode oldMode = GCSettings.LatencyMode;
            RuntimeHelpers.PrepareConstrainedRegions();
            GCSettings.LatencyMode = GCLatencyMode.Batch;

            // Metadata 로딩
            Task.Run(() => {
                HitomiData.Instance.LoadMetadataJson();
                HitomiData.Instance.LoadHiddendataJson();
                Koromo_Copy.Monitor.Instance.Push($"Loaded metadata: '{HitomiData.Instance.metadata_collection.Count.ToString("#,#")}' articles.");
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }).ContinueWith(t =>
            {
                TotalProgress.IsIndeterminate = false;
                TotalProgress.Value = 100;
                IsMetadataLoaded = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());

            KeyDown += MainWindow_KeyDown;
            Closing += MainWindow_Closing;
        }

        public bool IsMetadataLoaded = false;

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Koromo_Copy.Monitor.IsValueCreated)
            {
                Koromo_Copy.Monitor.Instance.Save();
                if (Koromo_Copy.Monitor.Instance.ControlEnable)
                    Koromo_Copy.Console.Console.Instance.Stop();
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Koromo_Copy.Monitor.Instance.ControlEnable = true;
                Koromo_Copy.Monitor.Instance.Push("Hello!");
                Koromo_Copy.Monitor.Instance.Start();
            }
        }

        private async void AppendAsync(string content)
        {
            var result = await HitomiDataParser.SearchAsync(content);
            
            SearchCount.Text = $"검색된 항목: {result.Count.ToString("#,#")}개";
            _ = Task.Run(() => LoadThumbnail(result));
        }

        private void LoadThumbnail(List<HitomiMetadata> md)
        {
            List<Task> task = new List<Task>();
            foreach (var metadata in md)
            {
                Task.Run(() => LoadThumbnail(metadata));
                Thread.Sleep(100);
            }
        }

        private void LoadThumbnail(HitomiMetadata md)
        {
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                // Put code that needs to run on the UI thread here
                var se = new SearchElements(HitomiLegalize.MetadataToArticle(md));
                SearchPanel.Children.Add(se);
                SearchPanel.Children.Add(new Separator());
                Koromo_Copy.Monitor.Instance.Push("[AddSearchElements] Hitomi Metadata " + md.ID);
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsMetadataLoaded)
            {
                AppendAsync(SearchText.Text);
            }
        }

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(SearchButton);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
            }
        }
    }
}
