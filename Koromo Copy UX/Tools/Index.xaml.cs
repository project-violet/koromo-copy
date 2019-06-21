/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hitomi.Translate;
using Koromo_Copy_UX.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace Koromo_Copy_UX.Tools
{
    /// <summary>
    /// Index.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Index : UserControl
    {
        public Index()
        {
            InitializeComponent();

            Loaded += Index_Loaded;
            KoreanTagList.DataContext = new ToolIndexDataGridViewModel();
            KoreanSeriesList.DataContext = new ToolIndexDataGridViewModel();
            KoreanTagList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<ToolIndexDataGridItemViewModel>(KoreanTagList).SortHandler);
            KoreanSeriesList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<ToolIndexDataGridItemViewModel>(KoreanSeriesList).SortHandler);
        }
        
        bool loaded = false;
        private void Index_Loaded(object sender, RoutedEventArgs e)
        {
            if (!SearchSpace.Instance.IsMetadataLoaded || loaded) return;
            loaded = true;

            List<HitomiTagdata> tags = new List<HitomiTagdata>();
            tags.AddRange(HitomiIndex.Instance.tagdata_collection.female);
            tags.AddRange(HitomiIndex.Instance.tagdata_collection.male);
            tags.AddRange(HitomiIndex.Instance.tagdata_collection.tag);

            List<Tuple<string, string, int>> tag_e2k = new List<Tuple<string, string, int>>();
            foreach (var tag in tags)
            {
                string k_try = KoreanTag.TagMap(tag.Tag);
                if (k_try != tag.Tag)
                {
                    if (k_try.Contains(":"))
                        tag_e2k.Add(new Tuple<string, string, int>(tag.Tag, k_try.Split(':')[1], tag.Count));
                    else
                        tag_e2k.Add(new Tuple<string, string, int>(tag.Tag, k_try, tag.Count));
                }
            }
            tag_e2k.Sort((a, b) => b.Item3.CompareTo(a.Item3));

            var tagdx = KoreanTagList.DataContext as ToolIndexDataGridViewModel;
            for (int i = 0; i < tag_e2k.Count; i++)
            {
                tagdx.Items.Add(new ToolIndexDataGridItemViewModel
                {
                    인덱스 = (i+1).ToString(),
                    영어 = tag_e2k[i].Item1,
                    한국어 = tag_e2k[i].Item2,
                    카운트 = tag_e2k[i].Item3.ToString()
                });
            }

            List<Tuple<string, string, int>> series_e2k = new List<Tuple<string, string, int>>();
            foreach (var tag in HitomiIndex.Instance.tagdata_collection.series)
            {
                string k_try = KoreanSeries.SeriesMap(tag.Tag);
                if (k_try != tag.Tag)
                {
                    series_e2k.Add(new Tuple<string, string, int>(tag.Tag, k_try, tag.Count));
                }
            }
            series_e2k.Sort((a, b) => b.Item3.CompareTo(a.Item3));

            var seriesdx = KoreanSeriesList.DataContext as ToolIndexDataGridViewModel;
            for (int i = 0; i < series_e2k.Count; i++)
            {
                seriesdx.Items.Add(new ToolIndexDataGridItemViewModel
                {
                    인덱스 = (i + 1).ToString(),
                    영어 = series_e2k[i].Item1,
                    한국어 = series_e2k[i].Item2,
                    카운트 = series_e2k[i].Item3.ToString()
                });
            }
        }
    }
}
