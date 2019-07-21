/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Fs;
using Koromo_Copy.Net;
using Koromo_Copy_UX.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// ZipIntegrity.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipIntegrity : Window
    {
        public ZipIntegrity()
        {
            InitializeComponent();
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);
        }

        private void append(string c)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                Status.AppendText(c + "\r\n");
                Status.ScrollToEnd();
            }));
        }

        List<string> file_list;
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Addr.Text.EndsWith(".txt"))
            {
                PB.IsIndeterminate = true;
                FileIndexor fi = new FileIndexor();
                await fi.ListingDirectoryAsync(Addr.Text);
                PB.IsIndeterminate = false;

                file_list = new List<string>();
                fi.Enumerate((string path, List<FileInfo> files) =>
                {
                    foreach (var iz in files)
                    {
                        if (Path.GetExtension(iz.Name) == ".zip")
                            file_list.Add(iz.FullName);
                    }
                });

                append(file_list.Count.ToString("#,#") + "개의 Zip 파일이 검색됨");
            }
            else
            {
                var lls = File.ReadAllLines(Addr.Text);

                var pp = new List<Tuple<string, string>>();

                var rx = new Regex(@"^\[(\d+)\]");
                foreach (var article in lls)
                {
                    var f = Path.GetFileNameWithoutExtension(article);
                    if (rx.Match(Path.GetFileNameWithoutExtension(article)).Success)
                    {
                        var id = rx.Match(System.IO.Path.GetFileNameWithoutExtension(article)).Groups[1].Value;
                        var artist = Path.GetFileName(Path.GetDirectoryName(article));

                        pp.Add(new Tuple<string, string>(id, artist));
                    }
                    else
                    {
                        append("[NO MATCH] " + article);
                    }
                }

                var articles = new List<HitomiArticle>();
                
                foreach (var p in pp)
                {
                    var aaa = HitomiLegalize.GetMetadataFromMagic(p.Item1);

                    if (!aaa.HasValue)
                    {
                        append("[NOT FOUND] " + p.Item1);
                        continue;
                    }

                    var xxx = HitomiLegalize.MetadataToArticle(aaa.Value);
                    xxx.Artists = new string[] { p.Item2 };
                    articles.Add(xxx);
                }

                await Task.Run(() =>
                {
                    int cnt = 0;
                    foreach (var at in articles)
                    {
                        try
                        {
                            var url = HitomiCommon.GetImagesLinkAddress(at.Magic);
                            var imgs = HitomiParser.GetImageLink(NetCommon.DownloadString(url));
                            at.ImagesLink = imgs;
                        }
                        catch
                        {
                            append("[FAIL DOWNLOAD] " + at.Magic);
                        }

                        cnt++;

                        Application.Current.Dispatcher.BeginInvoke(new Action(
                        delegate
                        {
                            PB.Value = cnt / (double)articles.Count * 100;
                        }));
                    }

                    int count = 0;

                    foreach (var ha in articles)
                    {
                        if (ha.ImagesLink == null) continue;
                        var prefix = HitomiCommon.MakeDownloadDirectory(ha);
                        Directory.CreateDirectory(prefix);
                        DownloadSpace.Instance.RequestDownload(ha.Title,
                            ha.ImagesLink.Select(y => HitomiCommon.GetDownloadImageAddress(ha.Magic, y)).ToArray(),
                            ha.ImagesLink.Select(y => Path.Combine(prefix, y)).ToArray(),
                            Koromo_Copy.Interface.SemaphoreExtends.Default, prefix, ha);
                        count++;
                    }

                    Application.Current.Dispatcher.BeginInvoke(new Action(
                    delegate
                    {
                        if (count > 0) MainWindow.Instance.FadeOut_MiddlePopup($"{count}{FindResource("msg_download_start")}");
                        MainWindow.Instance.Activate();
                        MainWindow.Instance.FocusDownload();
                    }));
                });
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            BB.IsEnabled = false;
            Task.Run(() => process());
        }

        private void process()
        {
            int cnt = 0;
            List<string> nv = new List<string>();
            var rx = new Regex(@"^\[(\d+)\]");
            foreach (var path in file_list)
            {
                try
                {
                    using (var zipFile = ZipFile.OpenRead(path))
                    {
                        var entries = zipFile.Entries;
                        if (zipFile.GetEntry("Info.json") == null)
                        {
                            append("[INFO NOT FOUND] " + path);
                            nv.Add(path);
                        }
                        else
                        {
                            using (var reader = new StreamReader(zipFile.GetEntry("Info.json").Open()))
                            {
                                var json_model = JsonConvert.DeserializeObject<HitomiJsonModel>(reader.ReadToEnd());
                                if (rx.Match(System.IO.Path.GetFileNameWithoutExtension(path)).Success)
                                {
                                    if (rx.Match(System.IO.Path.GetFileNameWithoutExtension(path)).Groups[1].Value !=
                                        json_model.Id)
                                    {
                                        append("[INFO NOT MATCH] " + path);
                                        nv.Add(path);
                                    }
                                    else
                                    {
                                        append("[VALID] " + path);
                                    }
                                }
                                else
                                {
                                    append("[FAIL INFO] " + path);
                                    nv.Add(path);
                                }
                            }
                        }
                    }
                }
                catch (InvalidDataException)
                {
                    append("[FAIL OPEN] " + path);
                    nv.Add(path);
                }

                cnt++;

                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    PB.Value = cnt / (double)file_list.Count * 100;
                }));
            }

            append($"{file_list.Count.ToString("#,#")}개의 파일 중 {nv.Count.ToString("#,#")}개의 파일이 유효하지 않습니다.");

            File.WriteAllLines($"integrity-{DateTime.Now.Ticks}.txt", nv.ToArray());

            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                BB.IsEnabled = true;
            }));
        }
    }
}
