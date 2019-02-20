/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hitomi.Analysis;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
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

namespace Koromo_Copy_UX.Tools
{
    /// <summary>
    /// Statistics.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Statistics : UserControl
    {
        public Statistics()
        {
            InitializeComponent();

            Loaded += Statistics_Loaded;
            AddTrends();
            Trend.Text = "작품 업로드 변동 폭";
        }

        bool loaded = false;
        private void Statistics_Loaded(object sender, RoutedEventArgs e)
        {
            if (SearchSpace.Instance.IsMetadataLoaded && !loaded)
            {
                loaded = true;

                HitomiAnalysisTrend.Instance.UpdateGalleryVariation();

                AxisX.Title = "아이디 간격";
                AxisX.MinValue = 0;

                AxisY.Title = "작품 수";

                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "작품 수",
                        Values = new ChartValues<ScatterPoint> (
                            HitomiAnalysisTrend.Instance.samples[0].points.Select(x => new ScatterPoint(x.X, x.Y))),
                        Fill=Brushes.Transparent,
                        Stroke = Brushes.Pink,
                        LineSmoothness = 1,
                        PointGeometrySize = 0
                    }
                };

                DataContext = this;
            }
        }

        public SeriesCollection SeriesCollection { get; set; }

        private void AddTrends()
        {
            var trends = new string[] {
                "작품 업로드 변동 폭",
                "작품 업로드 누적 폭",
                "태그 누적",
                "태그 누적 - 한국어",
                "태그 변동 - 한국어",
                "작가 작품 수 누적",
                "작가 작품 수 누적 - 한국어",
                "작가 작품 수 변동 - 한국어",
                "그룹 누적 - 한국어",
                //"그룹 변동 - 한국어",
                "시리즈 누적 - 한국어",
                "시리즈 변동 - 한국어",
                "캐릭터 누적 - 한국어",
                //"캐릭터 변동 - 한국어"   
            };
            trends.ToList().ForEach(lang => Trend.Items.Add(new ComboBoxItem { Content = lang }));
        }

        private void LoadTrend(string title)
        {
            YSeparator.Step = HitomiAnalysisTrend.Instance.samples[0].points.Last().Y / 20;
            SeriesCollection.Clear();
            SeriesCollection.Add(new LineSeries
            {
                Title = title,
                Values = new ChartValues<ScatterPoint>(
                        HitomiAnalysisTrend.Instance.samples[0].points.Select(x => new ScatterPoint(x.X, x.Y))),
                Fill = Brushes.Transparent,
                Stroke = Brushes.Pink,
                LineSmoothness = 1,
                PointGeometrySize = 0
            });
        }

        private void LoadTrend2()
        {
            YSeparator.Step = HitomiAnalysisTrend.Instance.samples[0].points.Last().Y / 20;
            SeriesCollection.Clear();
            SeriesCollection.AddRange(
                HitomiAnalysisTrend.Instance.samples.Select(x =>
                {
                    Random rm = new Random(x.name.GetHashCode());
                    return new LineSeries
                    {
                        Title = x.name,
                        Values = new ChartValues<ScatterPoint>(
                            x.points.Select(y => new ScatterPoint(y.X, y.Y))),
                        Fill = Brushes.Transparent,
                        Stroke = new SolidColorBrush(Color.FromRgb((byte)rm.Next(256), (byte)rm.Next(256), (byte)rm.Next(256))),
                        LineSmoothness = 1,
                        PointGeometrySize = 0
                    };
                })
                .ToArray());
        }

        private void Trend_DropDownClosed(object sender, EventArgs e)
        {
            if (loaded)
            {
                switch (Trend.Text)
                {
                    case "작품 업로드 변동 폭":
                        HitomiAnalysisTrend.Instance.UpdateGalleryVariation();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 0;
                        AxisY.Title = "작품 수";
                        LoadTrend("작품 수");
                        break;

                    case "작품 업로드 누적 폭":
                        HitomiAnalysisTrend.Instance.UpdataGalleryIncrements();
                        AxisX.Title = "아이디 간격";
                        AxisY.Title = "누적 작품 수";
                        LoadTrend("누적 작품 수");
                        break;

                    case "태그 누적":
                        HitomiAnalysisTrend.Instance.UpdateTagIncrements();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 0;
                        AxisY.Title = "누적 작품 수";
                        LoadTrend2();
                        break;

                    case "태그 누적 - 한국어":
                        HitomiAnalysisTrend.Instance.UpdateTagKoreanIncrements();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 0;
                        AxisY.Title = "누적 작품 수";
                        LoadTrend2();
                        break;

                    case "태그 변동 - 한국어":
                        HitomiAnalysisTrend.Instance.UpdateTagKoreanVariation();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 1125000;
                        AxisY.Title = "작품 변동 수";
                        LoadTrend2();
                        break;

                    case "작가 작품 수 누적":
                        HitomiAnalysisTrend.Instance.UpdateArtistsIncremetns();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 0;
                        AxisY.Title = "누적 작품 수";
                        LoadTrend2();
                        break;

                    case "작가 작품 수 누적 - 한국어":
                        HitomiAnalysisTrend.Instance.UpdateArtistsKoreanIncremetns();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 0;
                        AxisY.Title = "누적 작품 수";
                        LoadTrend2();
                        break;

                    case "작가 작품 수 변동 - 한국어":
                        HitomiAnalysisTrend.Instance.UpdateArtistsKoreanVariation();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 1125000;
                        AxisY.Title = "작품 변동 수";
                        LoadTrend2();
                        break;

                    case "그룹 누적 - 한국어":
                        HitomiAnalysisTrend.Instance.UpdateGroupsKoreanIncremetns();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 0;
                        AxisY.Title = "누적 작품 수";
                        LoadTrend2();
                        break;

                    case "그룹 변동 - 한국어":
                        HitomiAnalysisTrend.Instance.UpdateGroupsKoreanVariation();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 1125000;
                        AxisY.Title = "작품 변동 수";
                        LoadTrend2();
                        break;

                    case "시리즈 누적 - 한국어":
                        HitomiAnalysisTrend.Instance.UpdateSeriesKoreanIncremetns();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 0;
                        AxisY.Title = "누적 작품 수";
                        LoadTrend2();
                        break;

                    case "시리즈 변동 - 한국어":
                        HitomiAnalysisTrend.Instance.UpdateSeriesKoreanVariation();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 1125000;
                        AxisY.Title = "작품 변동 수";
                        LoadTrend2();
                        break;

                    case "캐릭터 누적 - 한국어":
                        HitomiAnalysisTrend.Instance.UpdateCharactersKoreanIncremetns();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 0;
                        AxisY.Title = "누적 작품 수";
                        LoadTrend2();
                        break;

                    case "캐릭터 변동 - 한국어":
                        HitomiAnalysisTrend.Instance.UpdateCharactersKoreanVariation();
                        AxisX.Title = "아이디 간격";
                        AxisX.MinValue = 1125000;
                        AxisY.Title = "작품 변동 수";
                        LoadTrend2();
                        break;
                }
            }
        }
    }
}
