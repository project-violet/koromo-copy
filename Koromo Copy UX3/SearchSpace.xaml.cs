/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
                Koromo_Copy.Monitor.Instance.Push($"Loaded metadata: '{HitomiData.Instance.metadata_collection.Count.ToString("#,#")}' articles.");
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
        public int global_position = 0;
        public string global_text = "";
        public bool selected_part = false;
        public bool skip_enter = false;

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !skip_enter)
            {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(SearchButton);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
            }
            skip_enter = false;
        }
        
        private void SearchText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                AutoComplete.IsOpen = false;
                SearchText.Focus();
            }
            else
            {
                if (AutoComplete.IsOpen)
                {
                    if (e.Key == Key.Down)
                    {
                        AutoCompleteList.SelectedIndex = 0;
                        AutoCompleteList.Focus();
                    }
                    else if (e.Key == Key.Up)
                    {
                        AutoCompleteList.SelectedIndex = AutoCompleteList.Items.Count - 1;
                        AutoCompleteList.Focus();
                    }
                }

                if (selected_part)
                {
                    selected_part = false;
                    if (e.Key != Key.Back)
                    {
                        SearchText.SelectionStart = global_position;
                        SearchText.SelectionLength = 0;
                    }
                }
            }
        }

        private Size MeasureString(string candidate)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(SearchText.FontFamily, SearchText.FontStyle, SearchText.FontWeight, SearchText.FontStretch),
                SearchText.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                TextFormattingMode.Display);

            return new Size(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);
        }
        
        private void SearchText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) return;
            int position = SearchText.SelectionStart;
            while (position > 0 && SearchText.Text[position - 1] != ' ')
                position -= 1;

            string word = "";
            for (int i = position; i < SearchText.Text.Length; i++)
            {
                if (SearchText.Text[i] == ' ') break;
                word += SearchText.Text[i];
            }

            if (word == "") { AutoComplete.IsOpen = false; return; }
            
            List<HitomiTagdata> match = new List<HitomiTagdata>();
            if (word.Contains(":"))
            {
                if (word.StartsWith("artist:"))
                {
                    word = word.Substring("artist:".Length);
                    position += "artist:".Length;
                    match = HitomiDataAnalysis.GetArtistList(word);
                }
                else if (word.StartsWith("tag:"))
                {
                    word = word.Substring("tag:".Length);
                    position += "tag:".Length;
                    match = HitomiDataAnalysis.GetTagList(word);
                }
                else if (word.StartsWith("tagx:"))
                {
                    word = word.Substring("tagx:".Length);
                    position += "tagx:".Length;
                    match = HitomiDataAnalysis.GetTagList(word);
                }
                else if (word.StartsWith("character:"))
                {
                    word = word.Substring("character:".Length);
                    position += "character:".Length;
                    match = HitomiDataAnalysis.GetCharacterList(word);
                }
                else if (word.StartsWith("group:"))
                {
                    word = word.Substring("group:".Length);
                    position += "group:".Length;
                    match = HitomiDataAnalysis.GetGroupList(word);
                }
                else if (word.StartsWith("series:"))
                {
                    word = word.Substring("series:".Length);
                    position += "series:".Length;
                    match = HitomiDataAnalysis.GetSeriesList(word);
                }
                else if (word.StartsWith("type:"))
                {
                    word = word.Substring("type:".Length);
                    position += "type:".Length;
                    match = HitomiDataAnalysis.GetTypeList(word);
                }
            }

            string[] match_target = {
                    "artist:",
                    "character:",
                    "group:",
                    "recent:",
                    "series:",
                    "tag:",
                    "tagx:",
                    "type:"
                };

            List<HitomiTagdata> data_col = (from ix in match_target where ix.StartsWith(word) select new HitomiTagdata { Tag = ix }).ToList();
            if (data_col.Count > 0)
                match.AddRange(data_col);
            match.AddRange(HitomiDataAnalysis.GetTotalList(word));

            if (match.Count > 0)
            {
                AutoComplete.IsOpen = true;
                AutoCompleteList.Items.Clear();
                List<string> listing = new List<string>();
                for (int i = 0; i < 100 && i < match.Count; i++)
                {
                    if (match[i].Count != 0)
                        listing.Add(match[i].Tag + $" ({match[i].Count})");
                    else
                        listing.Add(match[i].Tag);
                }
                listing.ForEach(x => AutoCompleteList.Items.Add(x));
                AutoComplete.HorizontalOffset = MeasureString(SearchText.Text.Substring(0, position)).Width;
            }
            else { AutoComplete.IsOpen = false; return; }

            global_position = position;
            global_text = word;

            if (e.Key == Key.Down)
            {
                AutoCompleteList.SelectedIndex = 0;
                AutoCompleteList.Focus();
            }
            else if (e.Key == Key.Up)
            {
                AutoCompleteList.SelectedIndex = AutoCompleteList.Items.Count - 1;
                AutoCompleteList.Focus();
            }
            else if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                PutStringIntoTextBox(AutoCompleteList.Items[0].ToString());
            }
        }
        
        private void PutStringIntoTextBox(string text)
        {
            text = text.Split('(')[0].Trim();
            SearchText.Text = SearchText.Text.Substring(0, global_position) +
                text +
                SearchText.Text.Substring(global_position + global_text.Length);
            AutoComplete.IsOpen = false;

            SearchText.SelectionStart = global_position;
            SearchText.SelectionLength = text.Length;
            skip_enter = true;
            SearchText.Focus();

            global_position = global_position + SearchText.SelectionLength;
            selected_part = true;
        }

        private void AutoCompleteList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                if (AutoCompleteList.SelectedItems.Count > 0)
                    PutStringIntoTextBox(AutoCompleteList.SelectedItem.ToString());
            }
            else if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Escape)
            {
                AutoComplete.IsOpen = false;
                SearchText.Focus();
            }
        }

        private void AutoCompleteList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AutoCompleteList.SelectedItems.Count > 0)
            {
                PutStringIntoTextBox(AutoCompleteList.SelectedItem.ToString());
            }
        }

        #endregion

    }
}
