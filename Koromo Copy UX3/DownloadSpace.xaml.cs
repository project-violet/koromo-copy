/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// DownloadSpace.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DownloadSpace : UserControl
    {
        public static DownloadSpace Instance;

        public DownloadSpace()
        {
            InitializeComponent();

            Instance = this;
            DownloadGroup.Instance.Complete += Instance_Complete;
            DownloadGroup.Instance.DownloadComplete += Instance_DownloadComplete;
            DownloadGroup.Instance.DownloadStatus += Instance_DownloadStatus;
            DownloadGroup.Instance.NotifySize += Instance_NotifySize;
            DownloadGroup.Instance.Retry += Instance_Retry;
        }

        private void Instance_Retry(object sender, Tuple<string, object> e)
        {
            Koromo_Copy.Monitor.Instance.Push("[Retry Download] " + e.Item1);
        }

        private void Instance_NotifySize(object sender, Tuple<string, long, object> e)
        {
        }

        private void Instance_DownloadStatus(object sender, Tuple<string, int, object> e)
        {
        }

        private void Instance_DownloadComplete(object sender, EventArgs e)
        {
            MainWindow.Instance.FadeOut_MiddlePopup("다운로드가 완료되었습니다!", false);
        }

        private void Instance_Complete(object sender, Tuple<string, string, object> e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
