/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Elo;
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
using System.Windows.Shapes;

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// RankSimulator.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RankSimulator : Window
    {
        public RankSimulator()
        {
            InitializeComponent();

            Loaded += RankSimulator_Loaded;
        }

        Stream left_stream;
        Stream right_stream;
        EloSystem sys;

        private void RankSimulator_Loaded(object sender, RoutedEventArgs e)
        {
            if (!SearchSpace.Instance.IsMetadataLoaded)
            {
                MessageBox.Show("메타데이터 로딩 후 다시 실행해주세요!", Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Close();
                return;
            }

            string path = "https://cdn.clien.net/web/api/file/F01/5117849/e7482ad23fa6428693a.PNG?thumb=true";
            LoadLeftImage(path);
            LoadRightImage(path);

            sys = new EloSystem();
            if (File.Exists("rank-simulator.json"))
            {
                sys.Open();
                RountText.Text = $"{sys.Model.GameCount.ToString()} 라운드";
            }
            else
            {
                HashSet<string> id = new HashSet<string>();

                foreach (var article in HitomiIndex.Instance.metadata_collection)
                {
                    if (article.Artists != null)
                        id.Add(HitomiIndex.Instance.index.Artists[article.Artists[0]].Replace(' ', '_'));
                    else if (article.Groups != null)
                        id.Add(HitomiIndex.Instance.index.Artists[article.Groups[0]].Replace(' ', '_'));
                }
                
                sys.AppendPlayer(id.Count);
                var list = id.ToList();
                for (int i = 0; i < list.Count; i++)
                    sys.Players[i].Indentity = list[i];
                sys.Model.GameCount = 1;
                sys.Save();
            }

            Task.Run(() => LoadNext());
        }

        private void LeftBitmap_DownloadCompleted(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                LeftProgress.Visibility = Visibility.Collapsed;
            }));
            left_stream.Close();
            left_stream.Dispose();
        }

        private void RightBitmap_DownloadCompleted(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                RightProgress.Visibility = Visibility.Collapsed;
            }));
            right_stream.Close();
            right_stream.Dispose();
        }

        private void LoadLeftImage(string url)
        {
            try
            {
                var req = WebRequest.Create(url);
                left_stream = req.GetResponse().GetResponseStream();
                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = left_stream;
                    bitmap.DownloadCompleted += LeftBitmap_DownloadCompleted;
                    bitmap.EndInit();
                    LeftImage.Source = bitmap;
                }));
            }
            catch { }
        }

        private void LoadRightImage(string url)
        {
            try
            {
                var wc = WebRequest.Create(url);
                right_stream = wc.GetResponse().GetResponseStream();
                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = right_stream;
                    bitmap.DownloadCompleted += RightBitmap_DownloadCompleted;
                    bitmap.EndInit();
                    RightImage.Source = bitmap;
                }));
            }
            catch { }
        }

        private async Task<string> GetThumbnailAddress(int id)
        {
            try
            {
                var url = $"{HitomiCommon.HitomiGalleryBlock}{id}.html";
                lock (Monitor.Instance) Monitor.Instance.Push($"Download string: {url}");
                using (var wc = Koromo_Copy.Net.NetCommon.GetDefaultClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    wc.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                    wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36");
                    var html = await wc.DownloadStringTaskAsync(url);
                    return HitomiCommon.HitomiThumbnail + HitomiParser.ParseGalleryBlock(html).Thumbnail;
                }
            }
            catch
            {
                var har = HCommander.GetArticleData(id);
                if (!har.HasValue)
                {
                    await Application.Current.Dispatcher.BeginInvoke(new Action(
                    delegate
                    {
                        MessageBox.Show($"{id}를 로딩하지 못했습니다 ㅠㅠ", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                    return "";
                }
                return har.Value.Thumbnail;
            }
        }

        private async void LoadNext()
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                LeftImage.Source = null;
                LeftProgress.Visibility = Visibility.Visible;
                RightImage.Source = null;
                RightProgress.Visibility = Visibility.Visible;
            }));

            var exclude = RankSimulatorStatistics.FilterClosingArtists(sys);
            var ava_list = new List<int>();
            for (int i = 0; i < sys.Players.Count; i++)
                if (!exclude.Contains(i))
                    ava_list.Add(i);

            var max = ava_list.Count;
            var rand = new Random();
            var r_count = 0;
        RETRY:
            if (r_count == 10)
            {
                await Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    MessageBox.Show("몇 가지 오류가 발생하여 진행할 수 없습니다. 개발자에게 문의하세요!", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }));
                return;
            }
            r_count++;
            var first = ava_list[rand.Next(max)];
            var second = ava_list[rand.Next(max)];

            if (first == second)
                goto RETRY;
            
            var fresult = await HitomiDataParser.SearchAsync($"artist:{sys.Players[first].Indentity}");
            var sresult = await HitomiDataParser.SearchAsync($"artist:{sys.Players[second].Indentity}");

            if (fresult.Count == 0)
                fresult = await HitomiDataParser.SearchAsync($"group:{sys.Players[first].Indentity}");
            if (sresult.Count == 0)
                sresult = await HitomiDataParser.SearchAsync($"group:{sys.Players[second].Indentity}");

            if (fresult.Count == 0 || sresult.Count == 0)
                goto RETRY;

            left = first;
            right = second;

            var ffirst = rand.Next(fresult.Count);
            var ssecond = rand.Next(sresult.Count);

            await Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                LeftText.Text = sys.Players[first].Indentity;
                RightText.Text = sys.Players[second].Indentity;
            }));
            LoadLeftImage(await GetThumbnailAddress(id1 = fresult[ffirst].ID));
            LoadRightImage(await GetThumbnailAddress(id2 = sresult[ssecond].ID));
        }

        int id1 = -1;
        int id2 = -1;
        int left = -1;
        int right = -1;
        private void LeftImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (left < 0 || right < 0) return;
            sys.Win(left, right, id1, id2);
            sys.Model.GameCount++;
            sys.Save();
            RountText.Text = $"{sys.Model.GameCount.ToString()} 라운드";
            Task.Run(() => LoadNext());
        }

        private void RightImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (left < 0 || right < 0) return;
            sys.Lose(left, right, id1, id2);
            sys.Model.GameCount++;
            sys.Save();
            RountText.Text = $"{sys.Model.GameCount.ToString()} 라운드";
            Task.Run(() => LoadNext());
        }

        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            if (left < 0 || right < 0) return;
            sys.Draw(left, right, id1, id2);
            sys.Model.GameCount++;
            sys.Save();
            RountText.Text = $"{sys.Model.GameCount.ToString()} 라운드";
            Task.Run(() => LoadNext());
        }

        public class TagComparer : IComparer<EloPlayer> { public int Compare(EloPlayer p1, EloPlayer p2) { return p1.Indentity.CompareTo(p2.Indentity); } }

        private void Stat_Click(object sender, RoutedEventArgs e)
        {
            RankSimulatorStatistics.GetTagRanking(sys).ForEach(x =>
            {
                if (x.Win + x.Lose + x.Draw == 0) return;
                Monitor.Instance.Push($"{x.Win}승".PadLeft(6) + $"{x.Lose}패".PadLeft(6) + $"{x.Draw}무".PadLeft(6) + $" ({(x.W * 100).ToString("0.###")}%):".PadLeft(12) + "  " + $"{x.Indentity}".PadRight(32) + $"{x.Rating.ToString("0.##")}점".PadLeft(14));
            });

            RankSimulatorStatistics.GetArtistRanking(sys).ForEach(x =>
            {
                if (x.Win + x.Lose + x.Draw == 0) return;
                Monitor.Instance.Push($"{x.Win}승".PadLeft(6) + $"{x.Lose}패".PadLeft(6) + $"{x.Draw}무".PadLeft(6) + $" ({(x.W * 100).ToString("0.###")}%):".PadLeft(12) + "  " + $"{x.Indentity}".PadRight(32) + $"{x.Rating.ToString("0.##")}점".PadLeft(14));
            });
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var builder = new StringBuilder();

            builder.Append("Koromo Copy Rank Simulator\r\n");
            builder.Append("Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.\r\n");
            builder.Append("\r\n");
            builder.Append("\r\n");
            builder.Append("=================================================================================\r\n");
            builder.Append("\r\n");
            builder.Append("                                    통산 기록\r\n");
            builder.Append("\r\n");
            builder.Append("=================================================================================\r\n");
            builder.Append("\r\n");
            builder.Append($" 게임 횟수: {sys.Model.DHistory.Count}\r\n");
            builder.Append($" 참가자 수: {sys.Players.Count}\r\n");
            builder.Append("\r\n");
            builder.Append("\r\n");
            builder.Append("=================================================================================\r\n");
            builder.Append("\r\n");
            builder.Append("                                   전체 게임 기록\r\n");
            builder.Append("\r\n");
            builder.Append("=================================================================================\r\n");
            builder.Append("\r\n");
            int gc = 0;
            sys.Model.DHistory.ForEach(x =>
            {
                builder.Append($" {(++gc).ToString("#,#").PadLeft(7)} 게임: {sys.Players[x.Item1].Indentity.PadLeft(30)} ({x.Item4.ToString().PadLeft(7)}) vs {sys.Players[x.Item2].Indentity.PadLeft(30)} ({x.Item5.ToString().PadLeft(7)}) ");

                if (x.Item3 == 1)
                    builder.Append($"       Left Win\r\n");
                else
                    builder.Append($"       Draw\r\n");
            });
            builder.Append("\r\n");
            builder.Append("\r\n");
            builder.Append("=================================================================================\r\n");
            builder.Append("\r\n");
            builder.Append("                                태그 엘로 레이팅 기록\r\n");
            builder.Append("\r\n");
            builder.Append("=================================================================================\r\n");
            builder.Append("\r\n");
            RankSimulatorStatistics.GetTagRanking(sys).ForEach(x =>
            {
                if (x.Win + x.Lose + x.Draw == 0) return;
                builder.Append($"{x.Win}승".PadLeft(6) + $"{x.Lose}패".PadLeft(6) + $"{x.Draw}무".PadLeft(6) + $" ({(x.W * 100).ToString("0.###")}%):".PadLeft(12) + "  " + $"{x.Indentity}".PadRight(32) + $"{x.Rating.ToString("0.##")}점".PadLeft(14) + "\r\n");
            });
            builder.Append("\r\n");
            builder.Append("\r\n");
            builder.Append("=================================================================================\r\n");
            builder.Append("\r\n");
            builder.Append("                                작가 엘로 레이팅 기록\r\n");
            builder.Append("\r\n");
            builder.Append("=================================================================================\r\n");
            builder.Append("\r\n");
            RankSimulatorStatistics.GetArtistRanking(sys).ForEach(x =>
            {
                if (x.Win + x.Lose + x.Draw == 0) return;
                builder.Append($"{x.Win}승".PadLeft(6) + $"{x.Lose}패".PadLeft(6) + $"{x.Draw}무".PadLeft(6) + $" ({(x.W * 100).ToString("0.###")}%):".PadLeft(12) + "  " + $"{x.Indentity}".PadRight(32) + $"{x.Rating.ToString("0.##")}점".PadLeft(14) + "\r\n");
            });
            builder.Append("\r\n");
            builder.Append("끝");

            File.WriteAllText("rank-simulator-result.log", builder.ToString());
            Process.Start("notepad", "rank-simulator-result.log");
        }
    }
}
