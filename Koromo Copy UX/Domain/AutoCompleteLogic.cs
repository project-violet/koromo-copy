/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Koromo_Copy_UX.Domain
{
    public class AutoCompleteLogic
    {
        private TextBox SearchText;
        private Popup AutoComplete;
        private ListBox AutoCompleteList;

        private int global_position = 0;
        private string global_text = "";
        private bool selected_part = false;
        private bool specific = false;
        private bool specific_tag = false;
        public bool skip_enter = false;

        public bool IsOpen => AutoComplete.IsOpen;

        public AutoCompleteLogic(TextBox SearchText, Popup AutoComplete, ListBox AutoCompleteList, bool Specific = false, bool SpecificTag = false)
        {
            this.SearchText = SearchText;
            this.AutoComplete = AutoComplete;
            this.AutoCompleteList = AutoCompleteList;
            specific = Specific;
            specific_tag = SpecificTag;
        }

        public void ClosePopup()
        {
            AutoComplete.IsOpen = false;
        }

        public void SearchText_PreviewKeyDown(object sender, KeyEventArgs e)
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

        public Size MeasureString(string candidate)
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


        public void SearchText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) return;
            int position = SearchText.SelectionStart;
            while (position > 0 && !" ()-+&|~".Contains(SearchText.Text[position - 1]))
                position -= 1;

            string word = "";
            for (int i = position; i < SearchText.Text.Length; i++)
            {
                if (" ()-+&|~".Contains(SearchText.Text[i])) break;
                word += SearchText.Text[i];
            }

            if (word == "") { AutoComplete.IsOpen = false; return; }

            List<HitomiTagdata> match = new List<HitomiTagdata>();

            if (!specific)
            {

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
                    else if (word.StartsWith("lang:"))
                    {
                        word = word.Substring("lang:".Length);
                        position += "lang:".Length;
                        match = HitomiDataAnalysis.GetLanguageList(word);
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
                    "type:",
                    "lang:"
                };

                List<HitomiTagdata> data_col = (from ix in match_target where ix.StartsWith(word) select new HitomiTagdata { Tag = ix }).ToList();
                if (Settings.Instance.Hitomi.CustomAutoComplete != null)
                    data_col.AddRange(from ix in Settings.Instance.Hitomi.CustomAutoComplete where ix.StartsWith(word) select new HitomiTagdata { Tag = ix });
                if (data_col.Count > 0)
                    match.AddRange(data_col);
                match.AddRange(HitomiDataAnalysis.GetTotalList(word));
            }
            else if (specific_tag)
            {
                match = HitomiDataAnalysis.GetTagList(word, true);
            }
            else
            {
                match = HitomiDataAnalysis.GetArtistList(word, true);
            }

            if (match.Count > 0)
            {
                AutoComplete.IsOpen = true;
                AutoCompleteList.Items.Clear();
                List<string> listing = new List<string>();
                for (int i = 0; i < SettingWrap.Instance.MaxCountOfAutoCompleteResult && i < match.Count; i++)
                {
                    if (match[i].Count != 0)
                        listing.Add(match[i].Tag + $" ({match[i].Count})");
                    else
                        listing.Add(match[i].Tag);
                }
                var MaxColoredTextLength = word.Length;
                var ColoredTargetText = word;
                listing.ForEach(x => {
                    if (SettingWrap.Instance.DoNotHightlightAutoCompleteResults)
                    {
                        AutoCompleteList.Items.Add(x);
                    }
                    else if (!Settings.Instance.Hitomi.UsingFuzzy)
                    {
                        try
                        {
                            var Result = new TextBlock();
                            Result.Foreground = Brushes.Black;
                            int StartColoredTextPosition = x.IndexOf(ColoredTargetText);
                            string firstdraw = x.Substring(0, StartColoredTextPosition);
                            Result.Text = firstdraw;

                            var Detected = new Run();
                            Detected.Foreground = Brushes.HotPink;
                            string seconddraw = x.Substring(StartColoredTextPosition, MaxColoredTextLength);
                            Detected.Text = seconddraw;

                            var Postfix = new Run();
                            Postfix.Foreground = Brushes.Black;
                            Postfix.Text = x.Substring(StartColoredTextPosition + MaxColoredTextLength);

                            Result.Inlines.Add(Detected);
                            Result.Inlines.Add(Postfix);
                            AutoCompleteList.Items.Add(Result);
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            var Result = new TextBlock();
                            Result.Foreground = Brushes.Black;
                            string prefix = "";
                            if (x.Contains(":") && !ColoredTargetText.Contains(":") && x.Split(':')[1] != "")
                            {
                                prefix = x.Split(':')[0] + ":";
                                x = x.Split(':')[1];
                            }
                            string postfix = x.Split(' ').Length > 1 ? x.Split(' ')[1] : "";
                            x = x.Split(' ')[0];

                            if (prefix != "")
                            {
                                Result.Text = prefix;
                            }
                            int[] diff = Strings.GetLevenshteinDistance(x, ColoredTargetText);
                            for (int i = 0; i < x.Length; i++)
                            {
                                var Temp = new Run();
                                Temp.Text = x[i].ToString();
                                if (diff[i + 1] == 1)
                                    Temp.Foreground = Brushes.HotPink;
                                else
                                    Temp.Foreground = Brushes.Black;
                                Result.Inlines.Add(Temp);
                            }
                            var Postfix = new Run();
                            Postfix.Text = postfix;
                            Postfix.Foreground = Brushes.Black;
                            Result.Inlines.Add(Postfix);
                            AutoCompleteList.Items.Add(Result);
                        }
                        catch
                        {
                        }
                    }
                });
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
                if (SettingWrap.Instance.DoNotHightlightAutoCompleteResults)
                {
                    PutStringIntoTextBox(AutoCompleteList.Items[0].ToString());
                }
                else
                {
                    var inline = (AutoCompleteList.Items[0] as TextBlock).Inlines;
                    PutStringIntoTextBox(string.Join("", inline.Select(x => new TextRange(x.ContentStart, x.ContentEnd).Text)));
                }
            }
        }

        public void PutStringIntoTextBox(string text)
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

        public void AutoCompleteList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                if (AutoCompleteList.SelectedItems.Count > 0)
                {
                    if (SettingWrap.Instance.DoNotHightlightAutoCompleteResults)
                    {
                        PutStringIntoTextBox(AutoCompleteList.SelectedItem.ToString());
                    }
                    else
                    {
                        var inline = (AutoCompleteList.SelectedItem as TextBlock).Inlines;
                        PutStringIntoTextBox(string.Join("", inline.Select(x => new TextRange(x.ContentStart, x.ContentEnd).Text)));
                    }
                }
            }
            else if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Escape)
            {
                AutoComplete.IsOpen = false;
                SearchText.Focus();
            }
        }

        public void AutoCompleteList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SettingWrap.Instance.DoNotHightlightAutoCompleteResults)
            {
                PutStringIntoTextBox(AutoCompleteList.SelectedItem.ToString());
            }
            else
            {
                var inline = (AutoCompleteList.SelectedItem as TextBlock).Inlines;
                PutStringIntoTextBox(string.Join("", inline.Select(x => new TextRange(x.ContentStart, x.ContentEnd).Text)));
            }
        }
    }
}
