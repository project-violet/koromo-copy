/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

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
    /// ZipListingStatistics.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipListingStatistics : UserControl
    {
        Dictionary<string, List<int>> overlap = new Dictionary<string, List<int>>();

        public ZipListingStatistics(List<KeyValuePair<string, ZipListingArticleModel>> source_data)
        {
            InitializeComponent();

            // 중복 검사
            int count_overlap = 0;
            for (int i = 0; i < source_data.Count; i++)
            {
                string id = source_data[i].Value.ArticleData.Id;
                if (overlap.ContainsKey(id))
                {
                    overlap[id].Add(i);
                    count_overlap++;
                }
                else
                    overlap.Add(id, new List<int>(new int[] { i }));
            }

            Overlap.Text = $"{count_overlap}개 ({((double)count_overlap / source_data.Count * 100).ToString("F3")}%)";

            // 파편화 검사

            // 1. 작가 기반 파편화 검사

            // 2. 용량
            long bytes = 0;
            source_data.ForEach(x => bytes += x.Value.Size);
            if (bytes < 1024)
                Size.Text = $"{bytes.ToString("#,#")} Bytes";
            else if (bytes < 1024 * 1024)
                Size.Text = $"{(bytes / 1024).ToString("#,#")} KB";
            else if (bytes < 1024 * 1024 * 1024)
                Size.Text = $"{(bytes / 1024 / 1024).ToString("#,#")} MB";
            else
                Size.Text = $"{(bytes / 1024 / 1024 / 1024).ToString("#,#")} GB";
        }
    }
}
