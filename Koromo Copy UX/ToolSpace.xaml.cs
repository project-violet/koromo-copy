/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy_UX.Tools;
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
    /// ToolSpace.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ToolSpace : UserControl
    {
        public ToolSpace()
        {
            InitializeComponent();
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);

            ToolsTree.Items.Add(new TreeViewItem
            {
                Header = FindResource("tools_and_utility"),
                DataContext = new Link()
            });
            ToolsTree.Items.Add(new TreeViewItem
            {
                Header = FindResource("statistics"),
                DataContext = new Statistics()
            });
            //ToolsTree.Items.Add(new TreeViewItem
            //{
            //    Header = "Artists Map",
            //    DataContext = new ArtistsMap()
            //});
            ToolsTree.Items.Add(new TreeViewItem
            {
                Header = FindResource("index"),
                DataContext = new Index()
            });

            ContentControl.Content = (ToolsTree.Items[0] as TreeViewItem).DataContext;
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
