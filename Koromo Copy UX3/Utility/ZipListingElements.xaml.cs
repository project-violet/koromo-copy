/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hitomi.Translate;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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

namespace Koromo_Copy_UX3.Utility
{
    /// <summary>
    /// ZipListingElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipListingElements : UserControl
    {
        string zip_file_name;

        public ZipListingElements(string path)
        {
            InitializeComponent();

            zip_file_name = path;
            Loaded += ZipViewerElements_Loaded;
        }

        ZipArchive zipFile;
        Stream zipStream;

        bool loaded = false;
        private void ZipViewerElements_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded) return;
            loaded = true;
            if (!File.Exists(zip_file_name))
            {
                Title.Text = $"파일을 찾을 수 없음";
                Artist.Text = zip_file_name;
                return;
            }
            Task.Run(() =>
            {
                try
                {
                    zipFile = ZipFile.Open(zip_file_name, ZipArchiveMode.Read);

                    try
                    {
                        using (var reader = new StreamReader(zipFile.GetEntry("Info.json").Open()))
                        {
                            var model = JsonConvert.DeserializeObject<HitomiJsonModel>(reader.ReadToEnd());
                            Application.Current.Dispatcher.BeginInvoke(new Action(
                            delegate
                            {
                                Title.Text = model.Title;
                                Artist.Text = model.Artists != null ? model.Artists[0] : "";
                                ImageCount.Text = $"{model.Pages} Pages";

                                bool seperator = false;
                                if (model.Artists != null)
                                {
                                    var stack = new StackPanel { Orientation = Orientation.Horizontal };
                                    stack.Children.Add(new PackIcon { Kind = PackIconKind.Artist, Opacity = .56 });
                                    stack.Children.Add(new TextBlock { Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Text = "작가 목록" });
                                    var menu_item = new MenuItem { Header = stack };
                                    foreach (var artist in model.Artists)
                                        menu_item.Items.Add(new MenuItem { Header = new TextBlock { Text = artist } });
                                    seperator = true;
                                    Menu.Items.Add(new Separator { Margin = new Thickness(4,0,4,0), Background = Brushes.Gray });
                                    Menu.Items.Add(menu_item);
                                }
                                if (model.Groups != null)
                                {
                                    var stack = new StackPanel { Orientation = Orientation.Horizontal };
                                    stack.Children.Add(new PackIcon { Kind = PackIconKind.UserGroup, Opacity = .56 });
                                    stack.Children.Add(new TextBlock { Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Text = "그룹 목록" });
                                    var menu_item = new MenuItem { Header = stack };
                                    foreach (var group in model.Groups)
                                        menu_item.Items.Add(new MenuItem { Header = new TextBlock { Text = group } });
                                    if (!seperator)
                                    {
                                        seperator = true;
                                        Menu.Items.Add(new Separator { Margin = new Thickness(8, 0, 8, 0), Background = Brushes.Gray });
                                    }
                                    Menu.Items.Add(menu_item);
                                }
                                if (model.Series != null)
                                {
                                    var stack = new StackPanel { Orientation = Orientation.Horizontal };
                                    stack.Children.Add(new PackIcon { Kind = PackIconKind.Book, Opacity = .56 });
                                    stack.Children.Add(new TextBlock { Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Text = "시리즈 목록" });
                                    var menu_item = new MenuItem { Header = stack };
                                    foreach (var series in model.Series)
                                        if (KoreanSeries.SeriesMap(series) == series)
                                            menu_item.Items.Add(new MenuItem { Header = new TextBlock { Text = series } });
                                        else
                                            menu_item.Items.Add(new MenuItem { Header = new TextBlock { Text = $"{series} ({KoreanSeries.SeriesMap(series)})" } });
                                    if (!seperator)
                                    {
                                        seperator = true;
                                        Menu.Items.Add(new Separator { Margin = new Thickness(8, 0, 8, 0), Background = Brushes.Gray });
                                    }
                                    Menu.Items.Add(menu_item);
                                }
                                if (model.Tags != null)
                                {
                                    var stack = new StackPanel { Orientation = Orientation.Horizontal };
                                    stack.Children.Add(new PackIcon { Kind = PackIconKind.Tag, Opacity = .56 });
                                    stack.Children.Add(new TextBlock { Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Text = "태그 목록" });
                                    var menu_item = new MenuItem { Header = stack };
                                    foreach (var tag in model.Tags)
                                        if (KoreanTag.TagMap(tag) == tag)
                                            menu_item.Items.Add(new MenuItem { Header = new TextBlock { Text = tag } });
                                        else if (KoreanTag.TagMap(tag).Contains(':'))
                                            menu_item.Items.Add(new MenuItem { Header = new TextBlock { Text = $"{tag} ({KoreanTag.TagMap(tag).Split(':')[1]})" } });
                                        else
                                            menu_item.Items.Add(new MenuItem { Header = new TextBlock { Text = $"{tag} ({KoreanTag.TagMap(tag)})" } });
                                    if (!seperator)
                                    {
                                        seperator = true;
                                        Menu.Items.Add(new Separator { Margin = new Thickness(8, 0, 8, 0), Background = Brushes.Gray });
                                    }
                                    Menu.Items.Add(menu_item);
                                }
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(
                        delegate
                        {
                            Title.Text = Path.GetFileNameWithoutExtension(zip_file_name);
                            Artist.Visibility = Visibility.Collapsed;
                        }));
                    }

                    var zipEntry = !zipFile.Entries[0].Name.EndsWith(".json") ? zipFile.Entries[0] : zipFile.Entries[1];
                    zipStream = zipEntry.Open();

                    Application.Current.Dispatcher.BeginInvoke(new Action(
                    delegate
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.DecodePixelWidth = 250;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = zipStream;
                        bitmap.EndInit();
                        bitmap.DownloadCompleted += Bitmap_DownloadCompleted;

                        Image.Source = bitmap;
                    }));
                }
                catch (Exception ex)
                {
                    Monitor.Instance.Push(ex.Message);
                    Monitor.Instance.Push(ex.StackTrace);
                }
            });
        }

        private void Bitmap_DownloadCompleted(object sender, EventArgs e)
        {
            zipStream.Close();
            zipFile.Dispose();
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start(zip_file_name);
        }
        
        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = (StackPanel)sender;
            if (item.Tag.ToString() == "OpenFolder")
            {
                Process.Start("explorer", "/select, \"" + zip_file_name + "\"");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            if (item.Tag.ToString() == "OpenFolder")
            {
                Process.Start("explorer", "/select, \"" + zip_file_name + "\"");
            }
        }
    }
}
