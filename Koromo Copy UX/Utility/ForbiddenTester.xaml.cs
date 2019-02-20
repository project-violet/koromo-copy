/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

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
using System.Windows.Shapes;

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// ForbiddenTester.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ForbiddenTester : Window
    {
        public ForbiddenTester()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    var html = Koromo_Copy.Net.NetCommon.DownloadString($"https://ltn.hitomi.la/galleryblock/{Id.Text}.html");
            //    var article = Koromo_Copy.Component.Hitomi.HitomiParser.ParseGalleryBlock(html);
            //    MessageBox.Show(Koromo_Copy.Monitor.SerializeObject(article));
            //}
            //catch (Exception ex)
            //{
            //    Koromo_Copy.Monitor.Instance.Push(ex.Message);
            //}

            var starts = Convert.ToInt32(Id.Text.Split('-')[0]);
            var ends = Convert.ToInt32(Id.Text.Split('-').Last());

            var htmls = Koromo_Copy.Net.EmiliaJob.Instance.AddJob(Enumerable.Range(starts, ends - starts + 1).Select(x => $"https://ltn.hitomi.la/galleryblock/{x}.html").ToList(), x =>
            {
                Koromo_Copy.Monitor.Instance.Push($"[Forbidden Test] {x}/{ends - starts + 1}");
            });

        }
    }
}
