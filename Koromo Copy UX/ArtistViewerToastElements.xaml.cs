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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX
{
    /// <summary>
    /// ArtistViewerToastElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ArtistViewerToastElements : UserControl
    {
        public string RawArtist;
        public ArtistViewerToastElements(string artist, string rate, string raw_artist)
        {
            InitializeComponent();

            Artist.Text = artist;
            Rate.Text = rate;
            RawArtist = raw_artist;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle.Background = new SolidColorBrush(Colors.Gray);
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                (new ArtistViewerWindow(RawArtist)).Show();
            }
        }
    }
}
