/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hitomi.Analysis;
using Koromo_Copy_UX.Domain;
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
using System.Windows.Threading;

namespace Koromo_Copy_UX
{
    /// <summary>
    /// CustomArtistsRecommendWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomArtistsRecommendWindow : Window
    {
        public CustomArtistsRecommendWindow()
        {
            InitializeComponent();

            //TagList.DataContext = new ArtistDataGridViewModel();
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

        bool loaded = false;
        private void CustomArtistsRecommendWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded) return;
            loaded = true;

            if (!SearchSpace.Instance.IsMetadataLoaded) return;

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
            TagList.DataContext = new ArtistDataGridViewModel(list.Select(x => new ArtistDataGridItemViewModel
            {
                항목 = x.Key,
                카운트 = x.Value
            }));

            UpdateResultList();
        }

        private void UpdateResultList()
        {
            var list2 = HitomiAnalysis.Instance.Rank.ToList();
            var list = new List<CustomArtistsRecommendationDataGridItemViewModel>();
            for (int i = 0; i < list2.Count; i++)
            {
                list.Add(new CustomArtistsRecommendationDataGridItemViewModel
                {
                    순위 = (i + 1).ToString(),
                    작가 = list2[i].Item1,
                    작품수 = HitomiAnalysis.Instance.ArtistCount[list2[i].Item1].ToString(),
                    점수 = list2[i].Item2.ToString(),
                    태그 = Regex.Replace(list2[i].Item3, @"\r\n", ",")
                });
            }
            ResultList.DataContext = new CustomArtistsRecommendationDataGridViewModel(list);
        }
        
        private void ResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultList.SelectedItems.Count > 0)
            {
                (new ArtistViewerWindow((ResultList.SelectedItems[0] as CustomArtistsRecommendationDataGridItemViewModel).작가)).Show();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            HitomiAnalysis.Instance.UserDefined = true;
            HitomiAnalysis.Instance.CustomAnalysis.Clear();

            foreach (var lvi in TagList.Items.OfType<ArtistDataGridItemViewModel>())
                HitomiAnalysis.Instance.CustomAnalysis.Add(new Tuple<string, int>(lvi.항목, Convert.ToInt32(lvi.카운트)));

            await RecommendSpace.Instance.Update(Convert.ToInt32(StartPosition.Text) - 1);
            MainWindow.Instance.Activate();
            MainWindow.Instance.FocusRecommend();

            UpdateResultList();
        }

        private void TagList_KeyDown(object sender, KeyEventArgs e)
        {
            if (TagList.SelectedItems.Count > 0)
            {
                if (e.Key == Key.Delete)
                {
                    var list = TagList.SelectedItems.OfType<ArtistDataGridItemViewModel>().ToList();
                    var tldx = TagList.DataContext as ArtistDataGridViewModel;
                    for (int i = 0; i < list.Count; i++)
                    {
                        tldx.Items.Remove(list[i]);
                    }
                }
            }
        }

        public void RequestAddTags(string tags, string score)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
               new Action(() =>
               {
                   var tldx = TagList.DataContext as ArtistDataGridViewModel;
                   foreach (var ttag in tags.Trim().Split(' '))
                   {
                       string tag = ttag.Replace('_', ' ');
                       if (!TagList.Items.OfType<ArtistDataGridItemViewModel>().ToList().Any(x =>
                       {
                           if (x.항목 == tag)
                               x.카운트 = Convert.ToInt32(score);
                           return x.항목 == tag;
                       }))
                       {
                           tldx.Items.Add(new ArtistDataGridItemViewModel
                           {
                               항목 = tag,
                               카운트 = Convert.ToInt32(score)
                           });
                       }
                   }
               }));
        }

        public void RequestAddArtists(string artists, string score)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
               new Action(() =>
               {
                   Dictionary<string, int> tags = new Dictionary<string, int>();

                   foreach (var artist in artists.Trim().Split(' '))
                   {
                       foreach (var data in HitomiIndex.Instance.metadata_collection)
                       {
                           if (!Settings.Instance.HitomiAnalysis.RecommendLanguageALL)
                           {
                               var lang = "n/a";
                               if (data.Language >= 0) lang = HitomiIndex.Instance.index.Languages[data.Language];
                               if (Settings.Instance.Hitomi.Language != "ALL" &&
                                   Settings.Instance.Hitomi.Language != lang) continue;
                           }
                           if (data.Artists != null && data.Tags != null && data.Artists.Select(x => HitomiIndex.Instance.index.Artists[x]).Contains(artist.Replace('_', ' ')))
                           {
                               foreach (var _tag in data.Tags)
                               {
                                   var tag = HitomiIndex.Instance.index.Tags[_tag];
                                   if (tags.ContainsKey(tag))
                                       tags[tag] = tags[tag] + 1;
                                   else
                                       tags.Add(tag, 1);
                               }
                           }
                       }
                   }

                   var list = tags.ToList();
                   list.Sort((a, b) => b.Value.CompareTo(a.Value));

                   var tldx = TagList.DataContext as ArtistDataGridViewModel;
                   foreach (var tag in list)
                   {
                       if (!TagList.Items.OfType<ArtistDataGridItemViewModel>().ToList().Any(x =>
                       {
                           if (x.항목 == tag.Key)
                               x.카운트 += tag.Value * Convert.ToInt32(score);
                           return x.항목 == tag.Key;
                       }))
                       {
                           tldx.Items.Add(new ArtistDataGridItemViewModel
                           {
                               항목 = tag.Key,
                               카운트 = tag.Value * Convert.ToInt32(score)
                           });
                       }
                   }
               }));
        }

        public void RequestLoadCustomTags(string index)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
               new Action(() =>
               {
                   int ix = HitomiBookmark.Instance.GetModel().CustomTags.Count - Convert.ToInt32(index);
                   var lvil = new List<ArtistDataGridItemViewModel>();
                   var tldx = TagList.DataContext as ArtistDataGridViewModel;
                   tldx.Items.Clear();
                   foreach (var item in HitomiBookmark.Instance.GetModel().CustomTags[ix].Item2)
                       tldx.Items.Add(new ArtistDataGridItemViewModel
                       {
                           항목 = item.Item1,
                           카운트 = item.Item2
                       });
               }));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            (new CustomArtistsRecommendAddTagWindow(this)).ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            (new CustomArtistsRecommendAddArtistWindow(this)).ShowDialog();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            HitomiAnalysis.Instance.CustomAnalysis.Clear();

            foreach (var lvi in TagList.Items.OfType<ArtistDataGridItemViewModel>())
                HitomiAnalysis.Instance.CustomAnalysis.Add(new Tuple<string, int>(lvi.항목, Convert.ToInt32(lvi.카운트)));

            if (BookmarkName.Text.Trim() != "")
            {
                HitomiBookmark.Instance.GetModel().CustomTags.Add(new Tuple<string, List<Tuple<string, int>>, DateTime>(BookmarkName.Text.Trim(), HitomiAnalysis.Instance.CustomAnalysis, DateTime.Now));
                HitomiBookmark.Instance.Save();
                BookmarkName.Text = "";
                MessageBox.Show("북마크에 추가되었습니다!", "Hitomi Copy", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("북마크 이름이 비어있습니다.", "Hitomi Copy", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            (new CustomArtistsRecommendBookmarkWindow(this)).ShowDialog();
        }
    }
}
