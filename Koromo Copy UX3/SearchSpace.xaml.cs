/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy_UX3.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// SearchPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SearchSpace : UserControl
    {
        public SearchSpace()
        {
            InitializeComponent();

            // Metadata 로딩
            Task.Run(() => {
                HitomiData.Instance.LoadMetadataJson();
                HitomiData.Instance.LoadHiddendataJson();
                HitomiData.Instance.RebuildTagData();
                if (HitomiData.Instance.metadata_collection != null)
                {
                    Koromo_Copy.Monitor.Instance.Push($"Loaded metadata: '{HitomiData.Instance.metadata_collection.Count.ToString("#,#")}' articles.");

                    if (Settings.Instance.Hitomi.UsingOptimization)
                    {
                        HitomiData.Instance.OptimizeMetadata();
                        Koromo_Copy.Monitor.Instance.Push($"Optimize metadata: '{HitomiData.Instance.metadata_collection.Count.ToString("#,#")}' articles.");
                    }
                }
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }).ContinueWith(t =>
            {
                TotalProgress.IsIndeterminate = false;
                TotalProgress.Value = 0;
                IsMetadataLoaded = true;
                //TotalProgress.Visibility = Visibility.Hidden;
            }, TaskScheduler.FromCurrentSynchronizationContext());

            Loaded += SearchSpace_Loaded;
        }

        private void SearchSpace_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            // 이거 지우면 디자이너 오류남
            if (w != null)
            {
                w.LocationChanged += (object obj, EventArgs args) =>
                {
                    var offset = AutoComplete.HorizontalOffset;
                    AutoComplete.HorizontalOffset = offset + 1;
                    AutoComplete.HorizontalOffset = offset;
                };
            }

            logic = new AutoCompleteLogic(SearchText, AutoComplete, AutoCompleteList);
        }

        public bool IsMetadataLoaded = false;

        private void SearchSpace_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Koromo_Copy.Monitor.IsValueCreated)
            {
                Koromo_Copy.Monitor.Instance.Save();
                if (Koromo_Copy.Monitor.Instance.ControlEnable)
                    Koromo_Copy.Console.Console.Instance.Stop();
            }
        }

        private async void AppendAsync(string content)
        {
            try
            {
                List<HitomiMetadata> result;
                if (!Settings.Instance.Hitomi.UsingAdvancedSearch)
                    result = await HitomiDataParser.SearchAsync(content.Trim());
                else
                    result = await HitomiDataSearchAdvanced.Search(content.Trim());

                SearchCount.Text = $"검색된 항목: {result.Count.ToString("#,#")}개";
                _ = Task.Run(() => LoadThumbnail(result));
            }
            catch
            {
                SearchCount.Text = "검색 문법이 잘못되었습니다.";
            }
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
            var tag = (sender as Button).Tag.ToString();

            if (tag == "Search" && IsMetadataLoaded)
            {
                AppendAsync(SearchText.Text);
            }
            else if (tag == "Tidy")
            {
                SearchPanel.Children.Clear();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }
            else if (tag == "SelectAll")
            {
                SearchPanel.Children.OfType<SearchElements>().ToList().ForEach(x => x.Select = true);
            }
            else if (tag == "DeSelectAll")
            {
                SearchPanel.Children.OfType<SearchElements>().ToList().ForEach(x => x.Select = false);
            }
        }

        #region Search Helper
        AutoCompleteLogic logic;

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !logic.skip_enter)
            {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(SearchButton);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
            }
            logic.skip_enter = false;
        }
        
        private void SearchText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            logic.SearchText_PreviewKeyDown(sender, e);
        }
        
        private void SearchText_KeyUp(object sender, KeyEventArgs e)
        {
            logic.SearchText_KeyUp(sender, e);
        }
        
        private void AutoCompleteList_KeyUp(object sender, KeyEventArgs e)
        {
            logic.AutoCompleteList_KeyUp(sender, e);
        }

        private void AutoCompleteList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            logic.AutoCompleteList_MouseDoubleClick(sender, e);
        }
        #endregion

    }
}
