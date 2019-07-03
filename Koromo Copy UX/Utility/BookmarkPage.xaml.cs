/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy_UX.Domain;
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

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// BookmarkPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BookmarkPage : UserControl
    {
        public BookmarkPage(string classify_name)
        {
            InitializeComponent();

            Path.Text = classify_name.Replace("/", " / ");

            DataContext = new BookmarkPageDataGridViewModel();
            TagList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<BookmarkPageDataGridItemViewModel>(TagList).SortHandler);
        }

        private void DataGridRow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
