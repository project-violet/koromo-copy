/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// SettingViewUpdate.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingViewUpdate : UserControl
    {
        public SettingViewUpdate()
        {
            InitializeComponent();

            NowVersion.Text = Koromo_Copy.Version.Text;
            Loaded += SettingViewUpdate_Loaded;
        }

        private void SettingViewUpdate_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                if (Koromo_Copy.Version.UpdateRequired())
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(
                    delegate
                    {
                        RequireUpdate.Visibility = Visibility.Visible;
                        UpdateButton.IsEnabled = true;
                    }));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(
                    delegate
                    {
                        NotRequireUpdate.Visibility = Visibility.Visible;
                    }));
                }

                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    LatestVersion.Text = $"{Koromo_Copy.Version.LatestVersionModel.MajorVersion}.{Koromo_Copy.Version.LatestVersionModel.MinorVersion}.{Koromo_Copy.Version.LatestVersionModel.BuildVersion}.{Koromo_Copy.Version.LatestVersionModel.RevisionVersion}";
                }));
            });
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.Fade_MiddlePopup(true, "업데이트를 확인하는 중...");
            Task.Run(() => Download());
        }
        
        private void Download()
        {
            bool retry = false;
            int read = 0;
            long length = 0;
            string temp = Path.GetTempFileName();
        RETRY_LABEL:
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Koromo_Copy.Version.LatestVersionModel.VersionBinary);
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
                        using (Stream outputStream = File.OpenWrite(temp))
                        {
                            byte[] buffer = new byte[131072];
                            int bytesRead;
                            if (!retry) length = response.ContentLength;
                            do
                            {
                                bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                                outputStream.Write(buffer, 0, bytesRead);
                                read += bytesRead;
                                MainWindow.Instance.ModifyText_MiddlePopup($"다운로드 중... [{read}/{length}]");
                            } while (bytesRead != 0);
                        }

                        string now_fpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        File.Move(now_fpath, now_fpath + ".tmp");
                        File.Move(temp, now_fpath);

                        Process.Start(now_fpath);
                        
                        Application.Current.Dispatcher.BeginInvoke(new Action(
                        delegate
                        {
                            Application.Current.Shutdown();
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                MainWindow.Instance.ModifyText_MiddlePopup($"다운로드 재시도 중입니다...");
                read = 0;
                retry = true;
                goto RETRY_LABEL;
            }
        }
    }
}
