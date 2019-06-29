/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using DCGallery;
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

namespace DCGallery.Domain
{
    public interface IAutoCompleteAlgorithm2
    {
        List<DCGalleryTagData> GetResults(ref string word, ref int position);
    }

    public class AutoCompleteBase2
    {
        private TextBox SearchText;
        private Popup AutoComplete;
        private ListBox AutoCompleteList;
        private IAutoCompleteAlgorithm2 Algorithm;

        private int global_position = 0;
        private string global_text = "";
        private bool selected_part = false;
        public bool skip_enter = false;

        public bool IsOpen => AutoComplete.IsOpen;

        public AutoCompleteBase2(IAutoCompleteAlgorithm2 Algorithm, TextBox SearchText, Popup AutoComplete, ListBox AutoCompleteList)
        {
            this.SearchText = SearchText;
            this.AutoComplete = AutoComplete;
            this.AutoCompleteList = AutoCompleteList;
            this.Algorithm = Algorithm;
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
            var match = Algorithm.GetResults(ref word, ref position);
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
                var MaxColoredTextLength = word.Length;
                var ColoredTargetText = word;
                listing.ForEach(x => {
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
                var inline = (AutoCompleteList.Items[0] as TextBlock).Inlines;
                PutStringIntoTextBox(string.Join("", inline.Select(x => new TextRange(x.ContentStart, x.ContentEnd).Text)));
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
                    var inline = (AutoCompleteList.SelectedItem as TextBlock).Inlines;
                    PutStringIntoTextBox(string.Join("", inline.Select(x => new TextRange(x.ContentStart, x.ContentEnd).Text)));
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
            var inline = (AutoCompleteList.SelectedItem as TextBlock).Inlines;
            PutStringIntoTextBox(string.Join("", inline.Select(x => new TextRange(x.ContentStart, x.ContentEnd).Text)));
        }
    }
}
