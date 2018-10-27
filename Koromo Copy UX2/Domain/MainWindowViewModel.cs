using System.Configuration;
using MaterialDesignDemo;
using MaterialDesignDemo.Domain;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;
using System.Windows.Controls;
using System;

namespace Koromo_Copy_UX2.Domain
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            if (snackbarMessageQueue == null) throw new ArgumentNullException(nameof(snackbarMessageQueue));

            DemoItems = new[]
            {
                new DemoItem("다운로더", new Downloader()),
                new DemoItem("통계", new Statistics()),
                new DemoItem("기록", new Record()),
                new DemoItem("도구", new Tools()),
                new DemoItem("환경설정", new Setting { DataContext = new SettingViewModel() }),
            };
        }

        public DemoItem[] DemoItems { get; }
    }
}