/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy_UX.Domain;
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

        private async void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                //files.ToList().ForEach(x => LoadFolder(x));

                // 폴더인지 확인
                // 1. 작가/그룹 폴더인가?
                // 2. 작품 폴더인가?

                // 파일 확인
                // 1. Zip 파일이고 Info.json 파일이 들어있는가?
                // 2. Zip 파일이고 Info.json 파일이 들어있지 않은가?

                var dialog = new BookmarkAdd("섹스");
                if ((bool)(await DialogHost.Show(dialog, "BookmarkDialog")))
                {

                }
            }
        }
    }
}
