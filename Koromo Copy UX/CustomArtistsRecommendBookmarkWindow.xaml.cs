/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
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
using System.Windows.Shapes;

namespace Koromo_Copy_UX
{
    /// <summary>
    /// CustomArtistsRecommendBookmarkWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomArtistsRecommendBookmarkWindow : Window
    {
        public CustomArtistsRecommendBookmarkWindow(CustomArtistsRecommendWindow car)
        {
            InitializeComponent();

            BookmarktList.DataContext = new CustomArtistsRecommendationBookmarkDataGridViewModel();
            BookmarktList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<CustomArtistsRecommendationBookmarkDataGridItemViewModel>(BookmarktList).SortHandler);
            Owner = car;

            Loaded += CustomArtistsRecommendBookmarkWindow_Loaded;
        }

        private void CustomArtistsRecommendBookmarkWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var bldx = BookmarktList.DataContext as CustomArtistsRecommendationBookmarkDataGridViewModel;
            for (int i = 0; i < HitomiBookmark.Instance.GetModel().CustomTags.Count; i++)
            {
                int index = HitomiBookmark.Instance.GetModel().CustomTags.Count - i - 1;
                bldx.Items.Add(new CustomArtistsRecommendationBookmarkDataGridItemViewModel
                {
                    인덱스 = (i + 1).ToString(),
                    이름 = HitomiBookmark.Instance.GetModel().CustomTags[index].Item1,
                    날짜 = HitomiBookmark.Instance.GetModel().CustomTags[index].Item3.ToString(),
                    태그 = string.Join(", ", HitomiBookmark.Instance.GetModel().CustomTags[index].Item2)
                });
            }
        }

        private void ResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BookmarktList.SelectedItems.Count == 1)
            {
                (Owner as CustomArtistsRecommendWindow).RequestLoadCustomTags((BookmarktList.SelectedItems[0] as CustomArtistsRecommendationBookmarkDataGridItemViewModel).인덱스);
                Close();
            }
        }
    }
}
