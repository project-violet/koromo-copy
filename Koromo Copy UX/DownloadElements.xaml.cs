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
    /// DownloadElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DownloadElements : UserControl
    {
        public DownloadElements()
        {
            InitializeComponent();
        }

        public DownloadElements(BitmapImage image, string[] urls, string[] paths, string title)
        {
            InitializeComponent();

            Image.Source = image;
            Title.Text = title;
        }

        public StackPanel ParentControl { get; set; }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            ImageToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            ImageToolTip.HorizontalOffset = e.GetPosition((IInputElement)sender).X + 10;
            ImageToolTip.VerticalOffset = e.GetPosition((IInputElement)sender).Y;
        }

        private void RemoveMe()
        {
            if (this.ParentControl != null)
                this.ParentControl.Children.Remove(this);
        }
    }
}
