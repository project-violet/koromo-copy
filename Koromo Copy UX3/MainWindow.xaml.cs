/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy_UX3.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        public MainWindow()
        {
            InitializeComponent();

            // GC 설정
            GCLatencyMode oldMode = GCSettings.LatencyMode;
            RuntimeHelpers.PrepareConstrainedRegions();
            GCSettings.LatencyMode = GCLatencyMode.Batch;

            Closing += MainWindow_Closing;
            KeyDown += SearchSpace_KeyDown;
            Loaded += MainWindow_Loaded;

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            Title += Koromo_Copy.Version.SimpleText;
            VersionText.Text += Koromo_Copy.Version.SimpleText;
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            Instance = this;

            ServicePointManager.DefaultConnectionLimit = 999999999;
            
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("프로그램 내부에서 예외처리되지 않은 오류가 발생했습니다. 오류가 계속된다면 개발자에게 문의하십시오. " + (e.ExceptionObject as Exception).Source + "\nStackTrace: " + (e.ExceptionObject as Exception).StackTrace,
                Koromo_Copy.Version.Name + " " + Koromo_Copy.Version.Text, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            // 이거 지우면 디자이너 오류남
            if (w != null)
            {
                w.LocationChanged += (object obj, EventArgs args) =>
                {
                    var offset = MiddlePopup.HorizontalOffset;
                    MiddlePopup.HorizontalOffset = offset + 1;
                    MiddlePopup.HorizontalOffset = offset;
                };
            }

            Fade_MiddlePopup(true);

            Koromo_Copy.Version.RequireTidy();
        }

        public void Fade_MiddlePopup(bool fade, string text = "", bool progress = true)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
               new Action(() =>
               {
                   if (progress)
                       PopupProgress.Visibility = Visibility.Visible;
                   else
                       PopupProgress.Visibility = Visibility.Collapsed;

                   if (fade)
                   {
                       Storyboard sb = PopupBorder.FindResource("FadeOnEvent") as Storyboard;
                       BeginStoryboard(sb);
                   }
                   else
                   {
                       Storyboard sb = PopupBorder.FindResource("FadeOffEvent") as Storyboard;
                       BeginStoryboard(sb);
                   }

                   if (text != "")
                   {
                       PopupText.Text = text;
                   }
               }));
        }

        public void FadeOut_MiddlePopup(string text = "", bool progress = true)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
               new Action(() =>
               {
                   if (progress)
                       PopupProgress.Visibility = Visibility.Visible;
                   else
                       PopupProgress.Visibility = Visibility.Collapsed;

                   Storyboard sb = PopupBorder.FindResource("FadeOutEvent") as Storyboard;
                   BeginStoryboard(sb);

                   if (text != "")
                   {
                       PopupText.Text = text;
                   }
               }));
        }

        public void ModifyText_MiddlePopup(string text)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
               new Action(() =>
               {
                    PopupText.Text = text;
               }));
        }

        private void SearchSpace_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Koromo_Copy.Monitor.Instance.ControlEnable = true;
                Koromo_Copy.Console.Console.Instance.RedirectionAfterLoopInit = () => Domain.UXConsole.Register();
                Koromo_Copy.Monitor.Instance.Push("Hello!");
                Koromo_Copy.Monitor.Instance.Start();
            }
        }

        int zip_status = 0;
        object zip_lock = new object();
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Process proc = Process.GetCurrentProcess();
            MemoryStatus.Text = "Memory Usage :  " + (proc.PrivateMemorySize64 / 1000).ToString("#,#") + " KB";
            lock (zip_lock)
                if (zip_status > 0)
                    MemoryStatus.Text += $" ({zip_status})";
        }

        public void ZipCountUp() { lock (zip_lock) zip_status++; }
        public void ZipCountDown() {  lock (zip_lock) zip_status--; }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Koromo_Copy.Monitor.IsValueCreated)
            {
                Koromo_Copy.Monitor.Instance.Push("UX Close");
                Koromo_Copy.Monitor.Instance.Save();
                if (Koromo_Copy.Monitor.Instance.ControlEnable)
                    Koromo_Copy.Console.Console.Instance.Stop();
            }
            Application.Current.Shutdown();
            Process.GetCurrentProcess().Kill();
        }

        private void MemoryStatus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            FadeOut_MiddlePopup("메모리 최적화 완료!", false);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            DropShadow.BlurRadius = 8;
            DropShadow.Color = Colors.Gray;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            DropShadow.BlurRadius = 10;
            DropShadow.Color = SettingWrap.Instance.ThemeColor;
        }
        
        private void ThreeButton_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();
            if (tag == "Minimize")
            {
                WindowState = WindowState.Minimized;
            }
            else if (tag == "Maximize")
            {
                if (WindowState != WindowState.Maximized)
                {
                    TopBorder.Margin = new Thickness(0,5,0,0);
                    TopBorder.BorderThickness = new Thickness(0);
                    WindowState = WindowState.Maximized;
                    Maximize.Visibility = Visibility.Collapsed;
                    Restore.Visibility = Visibility.Visible;
                }
                else
                {
                    TopBorder.Margin = new Thickness(10);
                    TopBorder.BorderThickness = new Thickness(1);
                    WindowState = WindowState.Normal;
                    Restore.Visibility = Visibility.Collapsed;
                    Maximize.Visibility = Visibility.Visible;
                }
            }
            else if (tag == "Close")
            {
                Close();
            }
            else if (tag == "Console")
            {
                Koromo_Copy.Monitor.Instance.ControlEnable = true;
                Koromo_Copy.Console.Console.Instance.RedirectionAfterLoopInit = () => Domain.UXConsole.Register();
                Koromo_Copy.Monitor.Instance.Push("Hello!");
                Koromo_Copy.Monitor.Instance.Start();
            }
        }

        public void FocusDownload()
        {
            DownloadTab.Focus();
            UpdateLayout();
        }

        public void FocusRecommend()
        {
            RecommendTab.Focus();
            UpdateLayout();
        }
    }
}
