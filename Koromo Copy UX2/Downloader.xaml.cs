/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer
   
***/

using Koromo_Copy.Component.Hitomi;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Koromo_Copy_UX2
{
    /// <summary>
    /// Downloader.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Downloader : UserControl
    {
        public Downloader()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (HitomiData.IsValueCreated == false)
                {
                    //Task.Factory.StartNew(t =>
                    //{
                    //    //note you can use the message queue from any thread, but just for the demo here we 
                    //    //need to get the message queue from the snackbar, so need to be on the dispatcher
                    //    MainSnackbar.MessageQueue.Enqueue("Koromo Copy에 오신것을 환영합니다!");
                    //}, TaskScheduler.FromCurrentSynchronizationContext());
                    MainWindow.Snackbar.MessageQueue.Enqueue("메타데이터가 로딩되지 않아 검색할 수 없습니다.");
                }
            }
        }
    }
}
