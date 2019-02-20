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
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
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
    /// CustomArtistsRecommendAddArtistWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomArtistsRecommendAddArtistWindow : Window
    {
        public CustomArtistsRecommendAddArtistWindow(CustomArtistsRecommendWindow car)
        {
            InitializeComponent();

            logic = new AutoCompleteLogic(TagSearchText, AutoComplete, AutoCompleteList, true, false);
            Owner = car;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TagSearchText.Text == "")
            {
                MessageBox.Show("작가를 입력해주세요.", "Custom Recommendation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            int score = 0;
            if (TagCountText.Text == "" || !int.TryParse(TagCountText.Text, out score))
            {
                MessageBox.Show("올바른 개수를 입력하세요.", "Custom Recommendation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            (Owner as CustomArtistsRecommendWindow).RequestAddArtists(TagSearchText.Text, TagCountText.Text);
            DialogResult = true;
            Close();
        }

        #region Search Helper
        AutoCompleteLogic logic;

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            logic.skip_enter = false;
        }

        private void SearchText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            logic.SearchText_PreviewKeyDown(sender, e);
        }

        private void SearchText_KeyUp(object sender, KeyEventArgs e)
        {
            logic.SearchText_KeyUp(sender, e);
        }

        private void AutoCompleteList_KeyUp(object sender, KeyEventArgs e)
        {
            logic.AutoCompleteList_KeyUp(sender, e);
        }

        private void AutoCompleteList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            logic.AutoCompleteList_MouseDoubleClick(sender, e);
        }
        #endregion
    }
}
