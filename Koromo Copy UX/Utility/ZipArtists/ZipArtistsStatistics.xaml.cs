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
using Koromo_Copy;

namespace Koromo_Copy_UX.Utility.ZipArtists
{
    /// <summary>
    /// ZipArtistsStatistics.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipArtistsStatistics : UserControl
    {
        public ZipArtistsStatistics(List<KeyValuePair<string, ZipArtistsArtistModel>> source_data)
        {
            InitializeComponent();

            // 중복 검사
            int count_overlap = 0;
            HashSet<int> overlap = new HashSet<int> ();
            int article_count = 0;
            foreach(var data in source_data)
            {
                foreach (var article in data.Value.ArticleData)
                {
                    var magic = Convert.ToInt32(article.Value.Id);
                    if (overlap.Contains(magic))
                        count_overlap++;
                    else
                        overlap.Add(magic);
                    article_count++;
                }
            }

            Overlap.Text = $"{count_overlap}개 ({((double)count_overlap / article_count * 100).ToString("F3")}%)";

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
