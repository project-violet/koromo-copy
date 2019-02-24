/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Microsoft.WindowsAPICodePack.Dialogs;
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
    /// ZipArtistsToolTransform.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipArtistsToolTransform : UserControl
    {
        public ZipArtistsToolTransform()
        {
            InitializeComponent();
        }

        private void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();
            if (tag == "SelectFolder")
            {
                var cofd = new CommonOpenFileDialog();
                cofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                cofd.IsFolderPicker = true;
                if (cofd.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
                {
                    Folder.Text = cofd.FileName;
                }
            }
        }
    }
}
