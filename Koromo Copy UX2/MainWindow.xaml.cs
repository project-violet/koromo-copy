/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer
   
***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy_UX2.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX2
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Snackbar Snackbar;

        public MainWindow()
        {
            InitializeComponent();

            // GC 설정
            GCLatencyMode oldMode = GCSettings.LatencyMode;
            RuntimeHelpers.PrepareConstrainedRegions();
            GCSettings.LatencyMode = GCLatencyMode.Batch;

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
            }).ContinueWith(t =>
            {
                //note you can use the message queue from any thread, but just for the demo here we 
                //need to get the message queue from the snackbar, so need to be on the dispatcher
                MainSnackbar.MessageQueue.Enqueue("Koromo Copy에 오신것을 환영합니다!");
            }, TaskScheduler.FromCurrentSynchronizationContext());

            DataContext = new MainWindowViewModel(MainSnackbar.MessageQueue);
            Snackbar = this.MainSnackbar;

            Closing += MainWindow_Closing;

            //new PaletteHelper().ReplacePrimaryColor(swatch);

            Task.Factory.StartNew(() =>
            {
                HitomiData.Instance.LoadMetadataJson();
                HitomiData.Instance.LoadHiddendataJson();
                Koromo_Copy.Monitor.Instance.Push($"Loaded metadata: '{HitomiData.Instance.metadata_collection.Count.ToString("#,#")}' articles.");
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }).ContinueWith(t =>
            {
                TotalProgress.IsIndeterminate = false;
                MainSnackbar.MessageQueue.Enqueue("메타데이터 로딩이 완료되었습니다!");
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void EnqueueMessage(string message)
        {
             MainSnackbar.MessageQueue.Enqueue(message);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Koromo_Copy.Monitor.IsValueCreated)
            {
                Koromo_Copy.Monitor.Instance.Save();
                if (Koromo_Copy.Monitor.Instance.ControlEnable)
                    Koromo_Copy.Console.Console.Instance.Stop();
            }
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //until we had a StaysOpen glag to Drawer, this will help with scroll bars
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar) return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            MenuToggleButton.IsChecked = false;
        }
        
        private string TranslateMenuPopupButtonMessage(string tag)
        {
            switch(tag)
            {
                case "Version":
                    return $"Koromo Copy {Koromo_Copy.Version.Text}";
            }
            return tag;
        }

        private async void MenuPopupButton_OnClick(object sender, RoutedEventArgs e)
        {
            var content = ((ButtonBase)sender).Tag.ToString();

            if (content == "Console")
            {
                Koromo_Copy.Monitor.Instance.ControlEnable = true;
                Koromo_Copy.Monitor.Instance.Push("Hello!");
                Koromo_Copy.Monitor.Instance.Start();
                return;
            }

            var sampleMessageDialog = new SampleMessageDialog
            {
                Message = { Text = TranslateMenuPopupButtonMessage(content) }
            };

            await DialogHost.Show(sampleMessageDialog, "RootDialog");
        }
    }
}
