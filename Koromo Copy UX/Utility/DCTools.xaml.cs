/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.DC;
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
    /// DCTools.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DCTools : Window
    {
        public DCTools()
        {
            InitializeComponent();

            Loaded += DCTools_Loaded;
        }

        bool loaded = false;
        private void DCTools_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded) return;
            loaded = true;

            var gl = DCCommon.GetGalleryList();
            var mgl = DCCommon.GetMinorGalleryList();
        }
    }
}
