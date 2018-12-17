/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component;
using Koromo_Copy.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Koromo_Copy_UX3.Utility
{
    /// <summary>
    /// SeriesManager.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SeriesManager : Window
    {
        public SeriesManager()
        {
            InitializeComponent();

            Loaded += SeriesManager_Loaded;
            Closed += SeriesManager_Closed;

            DownloadText.GotFocus += DownloadText_GotFocus;
            DownloadText.LostFocus += DownloadText_LostFocus;

            FilterText.GotFocus += FilterText_GotFocus;
            FilterText.LostFocus += FilterText_LostFocus;
        }

        private void FilterText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FilterText.Text))
                FilterText.Text = "필터링";
        }

        private void FilterText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FilterText.Text == "필터링")
                FilterText.Text = "";
        }

        private void DownloadText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DownloadText.Text))
                DownloadText.Text = "다운로드";
        }

        private void DownloadText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DownloadText.Text == "다운로드")
                DownloadText.Text = "";
        }

        private void SeriesManager_Closed(object sender, EventArgs e)
        {
            //EmiliaDispatcher.Instance.Abort();
        }

        private void SeriesManager_Loaded(object sender, RoutedEventArgs e)
        {
            SeriesCount.Text = $"시리즈 {SeriesLog.Instance.Model.Count}개";
            ArticleCount.Text = $"작품 {SeriesLog.Instance.Model.Select(x => x.Archive.Length).Sum()}개";
            foreach (var log in SeriesLog.Instance.Model)
            {
                SeriesPanel.Children.Insert(0, new SeriesManagerElements(log));
            }

            string path = "https://vignette.wikia.nocookie.net/inoubattlewanichijoukeinonakade/images/f/ff/Hatokochan.png/revision/latest?cb=20150121165834";

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();

            WaterMark.Source = bitmap;
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            DownloadIcon.Foreground = new SolidColorBrush(Color.FromRgb(0x9A, 0x9A, 0x9A));
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            DownloadIcon.Foreground = new SolidColorBrush(Color.FromRgb(0x71, 0x71, 0x71));
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DownloadIcon.Margin = new Thickness(2, 0, 0, 0);
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DownloadIcon.Margin = new Thickness(0, 0, 0, 0);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(DownloadText.Text) && DownloadText.Text != "다운로드")
            {
                SeriesPanel.Children.Insert(0, new SeriesManagerElements(DownloadText.Text));
                DownloadText.Text = "다운로드";
            }
        }

        private void DownloadText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(DownloadText.Text) && DownloadText.Text != "다운로드")
                {
                    SeriesPanel.Children.Insert(0, new SeriesManagerElements(DownloadText.Text));
                }
            }
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;
            
            if (item.Tag.ToString() == "Refresh")
            {
                foreach (var control in SeriesPanel.Children)
                    (control as SeriesManagerElements).RePrepareSync();

            }
            else if (item.Tag.ToString() == "SyncAll")
            {
                foreach (var control in SeriesPanel.Children)
                    if ((control as SeriesManagerElements).RequireSync)
                        Task.Run(() => (control as SeriesManagerElements).Sync());
            }
            else if(item.Tag.ToString() == "Pause")
            {
                if (PauseButtonIcon.Kind == MaterialDesignThemes.Wpf.PackIconKind.Pause)
                {
                    EmiliaDispatcher.Instance.Preempt();
                    PauseButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                    PauseButton.Text = "다시 실행";
                }
                else
                {
                    EmiliaDispatcher.Instance.Reactivation();
                    PauseButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                    PauseButton.Text = "일시정지";
                }
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var toggle = sender as System.Windows.Controls.Primitives.ToggleButton;
            if (toggle.IsChecked == true)
            {
                foreach (var control in SeriesPanel.Children)
                    if (!(control as SeriesManagerElements).RequireSync)
                        (control as SeriesManagerElements).Visibility = Visibility.Collapsed;
            }
            else
            {
                foreach (var control in SeriesPanel.Children)
                    (control as SeriesManagerElements).Visibility = Visibility.Visible;
            }
        }
        
        public string[] cc = new[] { "r", "R", "rt", "s", "sw", "sg", "e", "E", "f", "fr", "fa", "fq", "ft", "fe",
            "fv", "fg", "a", "q", "Q", "qt", "t", "T", "d", "w", "W", "c", "z", "e", "v", "g", "k", "o", "i", "O",
            "j", "p", "u", "P", "h", "hk", "ho", "hl", "y", "n", "nj", "np", "nl", "b", "m", "ml", "l", " ", "ss",
            "se", "st", " ", "frt", "fe", "fqt", " ", "fg", "aq", "at", " ", " ", "qr", "qe", "qtr", "qte", "qw",
            "qe", " ", " ", "tr", "ts", "te", "tq", "tw", " ", "dd", "d", "dt", " ", " ", "gg", " ", "yi", "yO", "yl", "bu", "bP", "bl" };
        public char[] cc1 = new[] { 'r', 'R', 's', 'e', 'E', 'f', 'a', 'q', 'Q', 't', 'T', 'd', 'w', 'W', 'c', 'z', 'x', 'v', 'g' };
        public string[] cc2 = new[] { "k", "o", "i", "O", "j", "p", "u", "P", "h", "hk", "ho", "hl", "y", "n", "nj", "np", "nl", "b", "m", "ml", "l" };
        public string[] cc3 = new[] { "", "r", "R", "rt", "s", "sw", "sg", "e", "f", "fr", "fa", "fq", "ft", "fx", "fv", "fg", "a", "q", "qt", "t", "T", "d", "w", "c", "z", "x", "v", "g" };

        public string hangul_disassembly(char letter)
        {
            if (0xAC00 <= letter && letter <= 0xD7FB)
            {
                int unis = letter - 0xAC00;
                return cc1[unis / (21 * 28)] + cc2[(unis % (21 * 28)) / 28] + cc3[(unis % (21 * 28)) % 28];
            }
            else if (0x3131 <= letter && letter <= 0x3163)
            {
                int unis = letter;
                return cc[unis - 0x3131];
            }
            else
            {
                return letter.ToString();
            }
        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = string.Join("", FilterText.Text.Select(x => hangul_disassembly(x)));
            foreach (var control in SeriesPanel.Children)
                if (string.Join("", (control as SeriesManagerElements).RawTitle.ToLower().Select(x => hangul_disassembly(x))).Contains(text) || (control as SeriesManagerElements).URLSource.ToLower().Contains(text))
                    (control as SeriesManagerElements).Visibility = Visibility.Visible;
                else
                    (control as SeriesManagerElements).Visibility = Visibility.Collapsed;
        }
    }
}
