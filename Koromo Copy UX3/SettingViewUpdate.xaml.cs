/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy_UX3.Domain;
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
            UpdatePatchNotes();
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

        #region Update Note

        private void UpdatePatchNotes()
        {
            // 0.8
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 0.8 Alpha",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - Pinterest 다운로더(setting.json에서 로그인 필요)\r\n\r\n" +
                    " - 작가 추천에 다운로드된 작가 숨기기 기능 추가\r\n\r\n" +
                    "[수정된 기능]\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018. dc-koromo. All Rights Reserved."
                }
            });

            // 0.7
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 0.7 Alpha",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - 픽시브 다운로더\r\n\r\n" +
                    " - 검색시 제외할 태그 추가 기능\r\n\r\n" +
                    " - 다운로드 플러그인 연동 기능\r\n\r\n" +
                    "[수정된 기능]\r\n" +
                    " - 업데이트 후 임시파일이 삭제되지 않는 버그\r\n\r\n" +
                    " - setting.json 파일이 형식에 맞지않게 변경되었을 때 발생하는 오류\r\n\r\n" +
                    " - 작가추천 기능에서 정리 후 자동스크롤되어 다음 작가 목록이 로딩되는 문제\r\n\r\n" +
                    "[삭제된 기능]\r\n" +
                    " - 마루마루 다운로더\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018. dc-koromo. All Rights Reserved."
                }
            });

            // 0.6
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8});
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 0.6 Alpha",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - 작가 추천 기능 구현\r\n\r\n" +
                    " - 수동 업데이트 도구 추가\r\n\r\n" +
                    " - 작가창 오른쪽에 추천 작가 표시 기능, 작품 날짜표시 기능\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018. dc-koromo. All Rights Reserved."
                }
            });

            // 0.5
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 0.5 Alpha",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - 다운로더 기능\r\n\r\n" +
                    " - 미리보기 기능\r\n\r\n" +
                    " - 작가/그룹창에서 모두 다운로드/다운로드 기능\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018. dc-koromo. All Rights Reserved."
                }
            });

            // 0.4
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 0.4 Alpha",
                    Content = 
                    "[추가된 기능]\r\n" +
                    " - 작품 업로드 날짜 표시\r\n\r\n" +
                    " - 검색어 자동완성\r\n\r\n" +
                    " - 고급 검색 기능\r\n\r\n" +
                    " - 댓글 보기 기능\r\n\r\n" +
                    " - Window Style을 Default에서 None으로 변경하고 DropShadow를 지원하는 새로운 윈도우로 대체함\r\n\r\n" +
                    " - 설정 탭을 추가함\r\n\r\n" +
                    " - 시작시 필요한 데이터를 다운로드\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018. dc-koromo. All Rights Reserved."
                }
            });

            // 0.3
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 0.3 Alpha",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - 메타데이터 검색 구현\r\n\r\n" +
                    " - Material Control을 적용해서 구현함\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018. dc-koromo. All Rights Reserved."
                }
            });
        }

        #endregion
    }
}
