/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy_UX3.Tools;
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

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// ToolSpace.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ToolSpace : UserControl
    {
        public ToolSpace()
        {
            InitializeComponent();

            ToolsTree.Items.Add(new TreeViewItem
            {
                Header = "Hyper Maintainer",
                DataContext = new HyperMaintainer()
            });
            ToolsTree.Items.Add(new TreeViewItem
            {
                Header = "Statistics",
                DataContext = new Statistics()
            });
            ToolsTree.Items.Add(new TreeViewItem
            {
                Header = "Index",
                DataContext = new Index()
            });
        }
        
        private void ToolsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = ToolsTree.SelectedItem;
            if (item is TreeViewItem tvi)
            {
                if (tvi.DataContext != null)
                {
                    ContentControl.Content = tvi.DataContext;
                }
            }
        }
    }
}
