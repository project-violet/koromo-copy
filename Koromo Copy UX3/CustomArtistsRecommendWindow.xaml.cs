/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hitomi.Analysis;
using Koromo_Copy_UX3.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// CustomArtistsRecommendWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomArtistsRecommendWindow : Window
    {
        public CustomArtistsRecommendWindow()
        {
            InitializeComponent();

            TagList.DataContext = new ArtistDataGridViewModel();
            ResultList.DataContext = new CustomArtistsRecommendationDataGridViewModel();
            TagList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<ArtistDataGridItemViewModel>(TagList).SortHandler);
            ResultList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<CustomArtistsRecommendationDataGridItemViewModel>(ResultList).SortHandler);

            Loaded += CustomArtistsRecommendWindow_Loaded;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }

        private void CustomArtistsRecommendWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Dictionary<string, int> tags_map = new Dictionary<string, int>();

            if (!HitomiAnalysis.Instance.UserDefined)
            {
                foreach (var log in HitomiLog.Instance.GetEnumerator().Where(log => log.Tags != null))
                {
                    foreach (var tag in log.Tags)
                    {
                        if (Settings.Instance.HitomiAnalysis.UsingOnlyFMTagsOnAnalysis &&
                            !tag.StartsWith("female:") && !tag.StartsWith("male:")) continue;
                        if (tags_map.ContainsKey(HitomiLegalize.LegalizeTag(tag)))
                            tags_map[HitomiLegalize.LegalizeTag(tag)] += 1;
                        else
                            tags_map.Add(HitomiLegalize.LegalizeTag(tag), 1);
                    }
                }
            }

            var list = tags_map.ToList();
            if (HitomiAnalysis.Instance.UserDefined)
                list = HitomiAnalysis.Instance.CustomAnalysis.Select(x => new KeyValuePair<string, int>(x.Item1, x.Item2)).ToList();
            list.Sort((a, b) => b.Value.CompareTo(a.Value));
            var tldx = TagList.DataContext as ArtistDataGridViewModel;
            foreach (var tag in list)
            {
                tldx.Items.Add(new ArtistDataGridItemViewModel
                {
                    항목 = tag.Key,
                    카운트 = tag.Value
                });
            }

            UpdateResultList();
        }

        private void UpdateResultList()
        {
            var list2 = HitomiAnalysis.Instance.Rank.ToList();
            var rldx = ResultList.DataContext as CustomArtistsRecommendationDataGridViewModel;
            for (int i = 0; i < list2.Count; i++)
            {
                rldx.Items.Add(new CustomArtistsRecommendationDataGridItemViewModel
                {
                    순위 = (i + 1).ToString(),
                    작가 = list2[i].Item1,
                    작품수 = HitomiAnalysis.Instance.ArtistCount[list2[i].Item1].ToString(),
                    점수 = list2[i].Item2.ToString(),
                    태그 = Regex.Replace(list2[i].Item3, @"\t|\n|\r", "")
                });
            }
        }
        
        private void ResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultList.SelectedItems.Count > 0)
            {
                (new ArtistViewerWindow((ResultList.SelectedItems[0] as CustomArtistsRecommendationDataGridItemViewModel).작가)).Show();
            }
        }
    }
}
