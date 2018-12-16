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
}
