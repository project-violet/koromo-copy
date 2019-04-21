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

        SortedDictionary<string, string> gallery_list = new SortedDictionary<string, string>();
        SortedDictionary<string, string> mgallery_list = new SortedDictionary<string, string>();
        SortedDictionary<string, string> gl = new SortedDictionary<string, string>();

        bool loaded = false;
        private void DCTools_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded) return;
            loaded = true;

            gallery_list = DCCommon.GetGalleryList();
            mgallery_list = DCCommon.GetMinorGalleryList();
            
            gallery_list.ToList().ForEach(x => gl.Add(x.Key + " 갤러리", x.Value));
            mgallery_list.ToList().ForEach(x => { gl.Add(x.Key + " 마이너 갤러리", x.Value); });

            gl.ToList().ForEach(x => gall_list.Items.Add(new ComboBoxItem() { Content = $"{x.Key} ({x.Value})" }));
        }

        private void gall_list_TextInput(object sender, TextCompositionEventArgs e)
        {
            gall_list.IsDropDownOpen = true;
            gl.ToList().Where(p => p.Key.Contains(e.Text)).ToList().ForEach(x => gall_list.Items.Add(new ComboBoxItem() { Content = $"{x.Key} ({x.Value})" }));
        }

        private void gall_list_TextChanged(object sender, TextChangedEventArgs e)
        {
            gall_list.IsDropDownOpen = true;
        }
    }
}
