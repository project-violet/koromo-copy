/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi.Analysis;
using Koromo_Copy.Console;
using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Koromo_Copy_UX
{
    /// <summary>
    /// RecommendSpace.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecommendSpace : UserControl
    {
        public static RecommendSpace Instance;

        DispatcherTimer timer = new DispatcherTimer();

        public RecommendSpace()
        {
            InitializeComponent();
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);

            Instance = this;
            InstanceMonitor.Instances.Add("recommendspace", Instance);
            timer.Interval = new TimeSpan(0,0,2);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            stay = false;
            wait_count = 0;
            timer.Stop();
        }

        public int latest_load_count = 0;
        public int wait_count = 0;
        public bool stay = false;

        public async Task Update(int llc = 0)
        {
            latest_load_count = llc;
            Koromo_Copy.Monitor.Instance.Push("[Recommend] Start Update...");
            await Task.Run(() => HitomiAnalysis.Instance.Update());
            Koromo_Copy.Monitor.Instance.Push("[Recommend] Update Complete!");
            RecommendList.Children.Clear();
            await Task.Run(() => MoreLoad());
        }

        public async Task UpdateOnlyArtists(int llc = 0)
        {
            latest_load_count = llc;
            RecommendList.Children.Clear();
            await Task.Run(() => MoreLoad());
        }

        private void MoreLoad()
        {
            stay = true;
            timer.Start();
            for (int i = 0; i < Settings.Instance.Hitomi.RecommendPerScroll && latest_load_count < HitomiAnalysis.Instance.Rank.Count; i++, latest_load_count++)
            {
                int llc = latest_load_count;
                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    RecommendList.Children.Add(new RecommendArtistElements(HitomiAnalysis.Instance.Rank[llc].Item1,
                        HitomiAnalysis.Instance.Rank[llc].Item2.ToString(), HitomiAnalysis.Instance.Rank[llc].Item3) { LoadComplete = DownStay });
                    RecommendList.Children.Add(new Separator());
                }));
                wait_count++;
            }
        }

        private void DownStay()
        {
            if (Interlocked.Decrement(ref wait_count) <= 0)
            {
                wait_count = 0;
                stay = false;
            }
        }

        private void WheelSpeedScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (ScrollViewer.VerticalOffset == 0 || ScrollViewer.ScrollableHeight == 0) return;
            if (ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight && !stay)
            {
                MoreLoad();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int rc = RecommendList.Children.Count;
            int sp = Settings.Instance.Hitomi.RecommendPerScroll * 2;
            for (int i = 0; i < rc - sp; i++)
            {
                RecommendList.Children.RemoveAt(0);
            }
            stay = true;
            timer.Start();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            HitomiAnalysis.Instance.FilterArtists = HideToggle.IsChecked.Value;
            Update();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            (new CustomArtistsRecommendWindow()).Show();
        }

        private void SnapShotPNG(UIElement source, Uri destination)
        {
            try
            {
                double actualHeight = source.RenderSize.Height;
                double actualWidth = source.RenderSize.Width;
                
                RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)actualWidth, (int)actualHeight, 96, 96, PixelFormats.Pbgra32);
                source.Arrange(new Rect(source.RenderSize));
                renderTarget.Render(source);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                using (FileStream stream = new FileStream(destination.LocalPath, FileMode.Create, FileAccess.Write))
                {
                    encoder.Save(stream);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void SaveToImage()
        {
            var filename = $"snapshot-{DateTime.Now.Ticks}.png";
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            SnapShotPNG((UIElement)ScrollViewer.Content, new Uri(path));
            MainWindow.Instance.FadeOut_MiddlePopup($"저장되었습니다! {filename}", false);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveToImage();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((e.Parameter as string)[0] == 'S')
            {
                SaveToImage();
            }
        }
    }
}
