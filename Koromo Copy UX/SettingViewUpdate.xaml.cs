/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy_UX.Domain;
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

namespace Koromo_Copy_UX
{
    /// <summary>
    /// SettingViewUpdate.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingViewUpdate : UserControl
    {
        public SettingViewUpdate()
        {
            InitializeComponent();
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);

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
            MainWindow.Instance.Fade_MiddlePopup(true, $"{FindResource("check_update")}");
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
                                MainWindow.Instance.ModifyText_MiddlePopup($"{FindResource("downloading")} [{read}/{length}]");
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
                MainWindow.Instance.ModifyText_MiddlePopup($"{FindResource("retry")}");
                read = 0;
                retry = true;
                goto RETRY_LABEL;
            }
        }

        #region Update Note

        private void UpdatePatchNotes()
        {
            // 1.14
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.14 Beta",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - 원래 제목 표시 기능 구현\r\n\r\n" +
                    "[변경된 기능]\r\n" +
                    " - 몇몇 작품들에서 타입이 표시되지 않던 오류 해결\r\n\r\n" +
                    " - 더이상 유효하지 않은 토큰 |를 삭제하지 않고 한글 ㅣ로 치환\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.13
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.13 Beta",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - 익헨 모듈 복구\r\n\r\n" +
                    " - 검색 중지 기능 구현\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.12
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.12 Beta",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - 북마크 기능 추가\r\n\r\n" +
                    "[삭제된 기능]\r\n" +
                    " - 익헨 모듈 삭제\r\n\r\n" +
                    "[기능 조치]\r\n" +
                    " - TimeoutInfinite 강제 활성화\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.11
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.11 Beta",
                    Content =
                    "[변경된 기능]\r\n" +
                    " - 데이터 저장방식 변경\r\n\r\n" +
                    " - 검색창에서 그룹명과 캐릭터가 표시되지 않던 오류 수정\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.10
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.10 Beta",
                    Content =
                    "[변경된 기능]\r\n" +
                    " - 썸네일 로딩 오류 해결\r\n\r\n" +
                    " - 다운로드 문제 해결\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.9
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.9 Beta",
                    Content =
                    "[변경된 기능]\r\n" +
                    " - 썸네일 로딩 오류 해결\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.9
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.9 Beta",
                    Content =
                    "[변경된 기능]\r\n" +
                    " - 썸네일 로딩 오류 해결\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.7
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.7 Beta",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - 랭크 시뮬레이터\r\n\r\n" +
                    " - Zip-Listing 페이지 이동기능(Ctrl + P) 및 레이팅 시스템 추가\r\n\r\n" +
                    "[변경된 기능]\r\n" +
                    " - 히토미 정책에 따른 metadata/hiddendata 다운로드 경로 변경\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.6
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.6 Beta",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - 스크립트 패키지 다운로드 기능 추가\r\n\r\n" +
                    "[관리가 중단된 도구]\r\n" +
                    " - 시리즈 매니져\r\n\r\n" +
                    " - 만화 크롤러\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.5
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.5 Beta",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - SRCAL-CDL 스크립트 언어 파서 및 실행기 지원\r\n\r\n" +
                    " - 통합 Hentai 403, 404 다운로더 지원\r\n\r\n" +
                    " - 히요비 H 다운로더 추가\r\n\r\n" +
                    "[기타]\r\n" +
                    " - 스크립트에 관해선 https://github.com/dc-koromo/koromo-copy/blob/master/Document/SRCAL.md를 참고하세요.\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.4
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.4 Beta",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - 검색결과 썸네일로 보기 기능 추가\r\n\r\n" +
                    " - 커스텀 크롤러 도구 추가\r\n\r\n" +
                    " - 통합 Hentai 403, 404 다운로더 시험 추가\r\n\r\n" +
                    "[수정된 기능]\r\n" +
                    " - 망가쇼미 다운로더\r\n\r\n" +
                    " - 고급검색에서 제외태그가 적용되지 않는 오류\r\n\r\n" +
                    " - Zip-Listing에 스택, 오프라인 기능 추가\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.3
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.3 Beta",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - Zip Listing 도구 추가\r\n\r\n" +
                    " - 도구 탭에 Link 추가\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.2
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.2 Beta",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - Manga Crawler 기능 시험 추가 (콘솔에서 ux -window crawler 명령으로 실행 가능)\r\n\r\n" +
                    " - 만화 다운로드전 이미지 정보 수집시 병렬로 수집합니다 (시리즈 매니져에서만 가능)\r\n\r\n" +
                    "[수정된 기능]\r\n" +
                    " - Mangashowme 사이트 변경에따른 다운로더 수정\r\n\r\n" +
                    "[삭제된 기능]\r\n" +
                    " - 기존의 Task 기반의 다운로드큐가 삭제되고, Emilia Queue를 기본 다운로드큐로 대체됩니다.\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.1
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.1 Beta",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - Zip Viewer 기능 추가 (Ctrl + E 로 실행 가능)\r\n\r\n" +
                    " - 시리즈 매니저 기능 추가 (Ctrl + S 로 실행 가능)\r\n\r\n" +
                    "[수정된 기능]\r\n" +
                    " - Emilia Queue가 활성화된 상태에서 프로그램실행시 간헐적으로 발생하는 오류 수정\r\n\r\n" +
                    " - Hiyobi Non-H 다운로더 버그 수정\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 1.0
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 1.0 Beta",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - Emilia Queue 추가\r\n\r\n" +
                    " - Instance level monitor 구현\r\n\r\n" +
                    "[수정된 기능]\r\n" +
                    " - Manazero 다운로드가 인식안되는 오류 수정\r\n\r\n" +
                    " - Mangashow.me 사이트 개편에 따른 다운로더 오류 수정\r\n\r\n" +
                    " - 경로 설정 중 \\문자 다음에 토큰이 나오지 않을때 경로 토큰으로 인식되지 않던 오류 수정\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 0.9
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 0.9 Alpha",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - Hiyobi Non-H 다운로더 추가\r\n\r\n" +
                    " - Mangashow-me 추가\r\n\r\n" +
                    " - 커스텀 작가 추천 기능\r\n\r\n" +
                    " - 작가 추천 목록 이미지 저장기능 (마우스 오른쪽 클릭 메뉴에서 가능)\r\n\r\n" +
                    " - 고급 설정탭 추가\r\n\r\n" +
                    "[수정된 기능]\r\n" +
                    " - 컬럼 정렬시 옳바르게 정렬되지 않는 문제\r\n\r\n" +
                    " - 자동완성 창이 닫히지 않거나 다른 창을 가리는 문제\r\n\r\n" +
                    " - 댓글에서 하이퍼링크 토큰이 삭제되지 않는 문제\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });

            // 0.8
            UpdateLogs.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
            UpdateLogs.Children.Add(new PatchNoteElements
            {
                DataContext = new PatchNoteViewModel
                {
                    Version = "Koromo Copy 0.8 Alpha",
                    Content =
                    "[추가된 기능]\r\n" +
                    " - Pinterest 다운로더 추가(setting.json에서 로그인 필요)\r\n\r\n" +
                    " - Manazero 다운로더 추가\r\n\r\n" +
                    " - DCInside 게시글 이미지 다운로더 추가\r\n\r\n" +
                    " - 작가 추천에 다운로드된 작가 숨기기 기능 추가\r\n\r\n" +
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
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
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
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
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
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
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
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
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
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
                    "\r\nKoromo Copy Project\r\nCopyright (C) 2018-2019. dc-koromo. All Rights Reserved."
                }
            });
        }

        #endregion
    }
}
