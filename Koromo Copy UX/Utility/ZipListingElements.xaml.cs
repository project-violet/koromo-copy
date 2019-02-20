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

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// ZipListingElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipListingElements : UserControl
    {
        string zip_file_name;
        bool offline_mode;
        HitomiJsonModel model;
        int rating = 0;
        bool download_thumbnail
#if DEBUG
            = true
#else
            = false
#endif
            ;
        
        public ZipListingElements(string path, HitomiJsonModel model, int rating, bool offline = false)
        {
            InitializeComponent();

            zip_file_name = path;
            offline_mode = offline;
            this.model = model;
            this.rating = rating;
            Rating.Text = Convert.ToString(rating);
            Loaded += ZipViewerElements_Loaded;
        }

        ZipArchive zipFile;
        Stream zipStream;

        bool loaded = false;
        private void ZipViewerElements_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded) return;
            loaded = true;
            if (offline_mode == false && !File.Exists(zip_file_name))
            {
                Title.Text = $"파일을 찾을 수 없음";
                Artist.Text = zip_file_name;
                return;
            }
            set_rateeffect();
            Task.Run(() =>
            {
                try
                {
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
                            {
                                var item = new MenuItem { Header = new TextBlock { Text = artist }, Tag = $"artist:{artist.Replace(' ', '_')}" };
                                item.Click += subitem_click;
                                menu_item.Items.Add(item);
                            }
                            seperator = true;
                            Menu.Items.Add(new Separator { Margin = new Thickness(4, 0, 4, 0), Background = Brushes.Gray });
                            Menu.Items.Add(menu_item);
                        }
                        if (model.Groups != null)
                        {
                            var stack = new StackPanel { Orientation = Orientation.Horizontal };
                            stack.Children.Add(new PackIcon { Kind = PackIconKind.UserGroup, Opacity = .56 });
                            stack.Children.Add(new TextBlock { Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Text = "그룹 목록" });
                            var menu_item = new MenuItem { Header = stack };
                            foreach (var group in model.Groups)
                            {
                                var item = new MenuItem { Header = new TextBlock { Text = group }, Tag = $"group:{group.Replace(' ', '_')}" };
                                item.Click += subitem_click;
                                menu_item.Items.Add(item);
                            }
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
                            {
                                MenuItem item = null;
                                if (KoreanSeries.SeriesMap(series) == series)
                                    item = new MenuItem { Header = new TextBlock { Text = series }, Tag = $"series:{series.Replace(' ', '_')}" };
                                else
                                    item = new MenuItem { Header = new TextBlock { Text = $"{series} ({KoreanSeries.SeriesMap(series)})" }, Tag = $"series:{series.Replace(' ', '_')}" };
                                item.Click += subitem_click;
                                menu_item.Items.Add(item);
                            }
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
                            {
                                MenuItem item = null;
                                if (KoreanTag.TagMap(tag) == tag)
                                    item = new MenuItem { Header = new TextBlock { Text = tag }, Tag = $"{(tag.StartsWith("female:") || tag.StartsWith("male:") ? tag.Replace(' ', '_') : $"tag:{tag.Replace(' ', '_')}")}" };
                                else if (KoreanTag.TagMap(tag).Contains(':'))
                                    item = new MenuItem { Header = new TextBlock { Text = $"{tag} ({KoreanTag.TagMap(tag).Split(':')[1]})" }, Tag = $"{(tag.StartsWith("female:") || tag.StartsWith("male:") ? tag.Replace(' ', '_') : $"tag:{tag.Replace(' ', '_')}")}" };
                                else
                                    item = new MenuItem { Header = new TextBlock { Text = $"{tag} ({KoreanTag.TagMap(tag)})" }, Tag = $"{(tag.StartsWith("female:") || tag.StartsWith("male:") ? tag.Replace(' ', '_') : $"tag:{tag.Replace(' ', '_')}")}" };
                                item.Click += subitem_click;
                                menu_item.Items.Add(item);
                            }
                            if (!seperator)
                            {
                                seperator = true;
                                Menu.Items.Add(new Separator { Margin = new Thickness(8, 0, 8, 0), Background = Brushes.Gray });
                            }
                            Menu.Items.Add(menu_item);
                        }
                    }));

                    if (offline_mode == false)
                    {
                        zipFile = ZipFile.Open(zip_file_name, ZipArchiveMode.Read);
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
                    else if (download_thumbnail)
                    {
                        var thumbnail = HitomiCommon.HitomiThumbnail + HitomiParser.ParseGalleryBlock(Koromo_Copy.Net.NetCommon.DownloadString(
                            $"{HitomiCommon.HitomiGalleryBlock}{model.Id}.html")).Thumbnail;
                        Application.Current.Dispatcher.BeginInvoke(new Action(
                        delegate
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(thumbnail);
                            bitmap.DecodePixelWidth = 240;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            //bitmap.DownloadCompleted += Bitmap_DownloadCompleted;
                            Image.Source = bitmap;
                        }));
                    }
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
            if (offline_mode == false)
            {
                zipStream.Close();
                zipFile.Dispose();
            }
        }

        bool ignore_double_click = false;
        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (offline_mode == false && ignore_double_click == false)
            {
                Process.Start(zip_file_name);
            }
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

        private void subitem_click(object sender, RoutedEventArgs e)
        {
            ((ZipListing)Window.GetWindow(this)).add_search_token((sender as MenuItem).Tag.ToString());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tag = ((Button)sender).Tag.ToString();

            if (tag == "UpVote")
            {
                rating++;
                Rating.Text = Convert.ToString(rating);
            }
            else if (tag == "DownVote")
            {
                rating--;
                if (rating < 0)
                    rating = 0;
                Rating.Text = Convert.ToString(rating);
            }
            ((ZipListing)Window.GetWindow(this)).set_rate(Convert.ToInt32(model.Id), rating);
            set_rateeffect();
        }

        private void set_rateeffect()
        {
            if (rating < 5)
            {
                RatingShadow.Opacity = 0;
                Rating.Opacity = 0.56;
                Rating.Foreground = Brushes.White;
                UpArrowIcon.Foreground = Brushes.White;
                UpArrowIcon.Opacity = 0.56;
                UpArrowShadow.Color = Colors.White;
                DownArrowIcon.Foreground = Brushes.White;
                DownArrowIcon.Opacity = 0.56;
                DownArrowShadow.Color = Colors.White;
            }
            else if (rating < 10)
            {
                RatingShadow.Opacity = 1;
                RatingShadow.Color = Colors.Yellow;
                Rating.Opacity = 1;
                Rating.Foreground = Brushes.Yellow;
                UpArrowIcon.Foreground = Brushes.Yellow;
                UpArrowIcon.Opacity = 1;
                UpArrowShadow.Color = Colors.Yellow;
                DownArrowIcon.Foreground = Brushes.Yellow;
                DownArrowIcon.Opacity = 1;
                DownArrowShadow.Color = Colors.Yellow;
            }
            else if (rating < 15)
            {
                RatingShadow.Opacity = 1;
                RatingShadow.Color = Colors.Orange;
                Rating.Opacity = 1;
                Rating.Foreground = Brushes.Orange;
                UpArrowIcon.Foreground = Brushes.Orange;
                UpArrowIcon.Opacity = 1;
                UpArrowShadow.Color = Colors.Orange;
                DownArrowIcon.Foreground = Brushes.Orange;
                DownArrowIcon.Opacity = 1;
                DownArrowShadow.Color = Colors.Orange;
            }
            else
            {
                RatingShadow.Opacity = 1;
                RatingShadow.Color = Colors.HotPink;
                Rating.Opacity = 1;
                Rating.Foreground = Brushes.HotPink;
                UpArrowIcon.Foreground = Brushes.HotPink;
                UpArrowIcon.Opacity = 1;
                UpArrowShadow.Color = Colors.HotPink;
                DownArrowIcon.Foreground = Brushes.HotPink;
                DownArrowIcon.Opacity = 1;
                DownArrowShadow.Color = Colors.HotPink;
            }
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            ignore_double_click = true;
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            ignore_double_click = false;
        }
    }
}
