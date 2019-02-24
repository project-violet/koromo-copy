/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using MaterialDesignThemes.Wpf;
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

namespace Koromo_Copy_UX.Utility.ZipArtists
{
    /// <summary>
    /// ZipArtistsTool.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipArtistsTool : UserControl
    {
        public ZipArtistsTool()
        {
            InitializeComponent();
        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();
            if (tag == "Move")
            {
                var dialog = new ZipArtistsToolMove();
                await DialogHost.Show(dialog, "AnotherDialog");
            }
            else if (tag == "Transform")
            {
                var dialog = new ZipArtistsToolTransform();
                await DialogHost.Show(dialog, "AnotherDialog");
            }
        }
    }
}
