/* Copyright (C) 2018. Hitomi Parser Developers */

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hitomi.Analysis;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Hitomi_Copy_3
{
    public partial class Statistics : Form
    {
        public Statistics()
        {
            InitializeComponent();
        }

        private void frmStatistics_Load(object sender, System.EventArgs e)
        {
            UpdateWeekChart();
            UpdateTimeChart();

            if (HitomiLog.Instance.GetList().Count > 0)
            {
                UpdateTagsChart();
                UpdateArtistsChart();
            }

            UpdateEtc();

            comboBox1.SelectedIndex = 0;
        }

        #region Date Time Chart

        private void UpdateWeekChart()
        {
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            chart1.ChartAreas.Add("ChartArea1");
            
            chart1.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart1.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart1.ChartAreas[0].AxisX.Title = "요일 간격";
            chart1.ChartAreas[0].AxisX.TitleFont = Font;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart1.ChartAreas[0].AxisY.Title = "작품 수";
            chart1.ChartAreas[0].AxisY.TitleFont = Font;
            chart1.ChartAreas[0].AxisY.Interval = HitomiAnalysisDateTime.Instance.week_count.Max() / 10;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart1.ChartAreas[0].BackColor = Color.Gray;

            Series series = new Series();
            series.Name = "요일별 작품 수";
            series.Font = Font;
            series.ChartArea = "ChartArea1";
            series.ChartType = SeriesChartType.Spline;
            series.Color = Color.LightPink;
            series.LabelBackColor = Color.Gray;
            series.BorderWidth = 3;

            for (int i = 0; i < HitomiAnalysisDateTime.Instance.week_count.Length; i++)
                series.Points.AddXY("일월화수목금토"[i]+"요일", HitomiAnalysisDateTime.Instance.week_count[i]);

            chart1.Series.Add(series);
        }

        private void UpdateTimeChart()
        {
            chart2.Series.Clear();
            chart2.ChartAreas.Clear();
            chart2.ChartAreas.Add("ChartArea1");
            
            chart2.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart2.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart2.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart2.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart2.ChartAreas[0].AxisX.Title = "시간 간격";
            chart2.ChartAreas[0].AxisX.TitleFont = Font;
            chart2.ChartAreas[0].AxisX.Minimum = 0;
            chart2.ChartAreas[0].AxisX.Interval = 1;
            chart2.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart2.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart2.ChartAreas[0].AxisY.Title = "작품 수";
            chart2.ChartAreas[0].AxisY.TitleFont = Font;
            chart2.ChartAreas[0].AxisY.Interval = HitomiAnalysisDateTime.Instance.time_count.Max() / 10;
            chart2.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart2.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart2.ChartAreas[0].BackColor = Color.Gray;

            Series series = new Series();
            series.Name = "시간대별 작품 수";
            series.Font = Font;
            series.ChartArea = "ChartArea1";
            series.ChartType = SeriesChartType.Spline;
            series.Color = Color.LightPink;
            series.LabelBackColor = Color.Gray;
            series.BorderWidth = 3;

            for (int i = 0; i < HitomiAnalysisDateTime.Instance.time_count.Length; i++)
                series.Points.Add(new DataPoint(i, HitomiAnalysisDateTime.Instance.time_count[i]));

            chart2.Series.Add(series);
        }

        #endregion

        #region Tag Chart

        private void UpdateTagsChart()
        {
            chart3.Series.Clear();
            chart3.ChartAreas.Clear();
            chart3.ChartAreas.Add("ChartArea1");

            chart3.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart3.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart3.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart3.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart3.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart3.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart3.ChartAreas[0].AxisX.Title = "태그";
            chart3.ChartAreas[0].AxisX.TitleFont = Font;
            chart3.ChartAreas[0].AxisX.Minimum = 0;
            chart3.ChartAreas[0].AxisX.Interval = 1;
            chart3.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart3.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart3.ChartAreas[0].AxisY.Title = "작품 수";
            chart3.ChartAreas[0].AxisY.TitleFont = Font;
            chart3.ChartAreas[0].AxisY.Interval = HitomiAnalysisTagCount.Instance.tag_count[0].Value / 30;
            chart3.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart3.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart3.ChartAreas[0].BackColor = Color.Gray;

            Series series = new Series();
            series.Name = "총 태그수";
            series.Font = Font;
            series.ChartArea = "ChartArea1";
            series.ChartType = SeriesChartType.Spline;
            series.Color = Color.LightPink;
            series.LabelBackColor = Color.Gray;
            series.BorderWidth = 3;

            for (int i = 0; i < HitomiAnalysisTagCount.Instance.tag_count.Count; i++)
                series.Points.AddXY(HitomiAnalysisTagCount.Instance.tag_count[i].Key, HitomiAnalysisTagCount.Instance.tag_count[i].Value);

            chart3.Series.Add(series);
        }

        #endregion

        #region Artist Chart

        private void UpdateArtistsChart()
        {
            chart5.Series.Clear();
            chart5.ChartAreas.Clear();
            chart5.ChartAreas.Add("ChartArea1");

            chart5.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart5.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart5.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart5.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart5.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart5.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart5.ChartAreas[0].AxisX.Title = "작가";
            chart5.ChartAreas[0].AxisX.TitleFont = Font;
            chart5.ChartAreas[0].AxisX.Minimum = 0;
            chart5.ChartAreas[0].AxisX.Interval = 1;
            chart5.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart5.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart5.ChartAreas[0].AxisY.Title = "작품 수";
            chart5.ChartAreas[0].AxisY.TitleFont = Font;
            chart5.ChartAreas[0].AxisY.Interval = HitomiAnalysisArtistCount.Instance.artist_count[0].Value / 30;
            chart5.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart5.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart5.ChartAreas[0].BackColor = Color.Gray;

            Series series = new Series();
            series.Name = "총 작품수";
            series.Font = Font;
            series.ChartArea = "ChartArea1";
            series.ChartType = SeriesChartType.Spline;
            series.Color = Color.LightPink;
            series.LabelBackColor = Color.Gray;
            series.BorderWidth = 3;

            for (int i = 0; i < HitomiAnalysisArtistCount.Instance.artist_count.Count; i++)
                series.Points.AddXY(HitomiAnalysisArtistCount.Instance.artist_count[i].Key, HitomiAnalysisArtistCount.Instance.artist_count[i].Value);

            chart5.Series.Add(series);
        }

        #endregion

        #region Chart

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateChart();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            UpdateChart();
        }

        public void UpdateChart()
        {
            if (comboBox1.SelectedIndex == 0)
                UpdateGalleryVariationChart();
            else if (comboBox1.SelectedIndex == 1)
                UpdateGalleryIncrements();
            else if (comboBox1.SelectedIndex == 2)
                UpdateTagIncrements();
            else if (comboBox1.SelectedIndex == 3)
                UpdateTagKoreanIncrements();
            else if (comboBox1.SelectedIndex == 4)
                UpdateTagKoreanVariation();
            else if (comboBox1.SelectedIndex == 5)
                UpdateArtistsIncremetns();
            else if (comboBox1.SelectedIndex == 6)
                UpdateArtistsKoreanIncremetns();
            else if (comboBox1.SelectedIndex == 7)
                UpdateArtistsKoreanVariation();
            else if (comboBox1.SelectedIndex == 8)
                UpdateGroupsKoreanIncremetns();
            else if (comboBox1.SelectedIndex == 9)
                UpdateGroupsKoreanVariation();
            else if (comboBox1.SelectedIndex == 10)
                UpdateSeriesKoreanIncremetns();
            else if (comboBox1.SelectedIndex == 11)
                UpdateSeriesKoreanVariation();
            else if (comboBox1.SelectedIndex == 12)
                UpdateCharactersKoreanIncremetns();
            else if (comboBox1.SelectedIndex == 13)
                UpdateCharactersKoreanVariation();
        }

        public void UpdateGalleryVariationChart()
        {
            HitomiAnalysisTrend.Instance.UpdateGalleryVariation();

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 0;
            chart4.ChartAreas[0].AxisX.Interval = 100000 / 2;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "작품 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 500;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Spline;
                series.Color = Color.LightPink;
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 3;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }

        public void UpdateGalleryIncrements()
        {
            HitomiAnalysisTrend.Instance.UpdataGalleryIncrements();

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 0;
            chart4.ChartAreas[0].AxisX.Interval = 100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "누적 작품 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 50000;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Spline;
                series.Color = Color.LightPink;
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 3;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }

        public void UpdateTagIncrements()
        {
            HitomiAnalysisTrend.Instance.UpdateTagIncrements();

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 0;
            chart4.ChartAreas[0].AxisX.Interval = 100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "누적 작품 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 10000;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Spline;
                Random rm = new Random(sample.name.GetHashCode());
                series.Color = Color.FromArgb(rm.Next(256), rm.Next(256), rm.Next(256));
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 2;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }

        public void UpdateTagKoreanIncrements()
        {
            HitomiAnalysisTrend.Instance.UpdateTagKoreanIncrements();

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 0;
            chart4.ChartAreas[0].AxisX.Interval = 100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "누적 작품 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 1000;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Spline;
                Random rm = new Random(sample.name.GetHashCode());
                series.Color = Color.FromArgb(rm.Next(256), rm.Next(256), rm.Next(256));
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 2;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }


        public void UpdateTagKoreanVariation()
        {
            HitomiAnalysisTrend.Instance.UpdateTagKoreanVariation();

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 1125000;//0;
            chart4.ChartAreas[0].AxisX.Interval = 10000;//100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "작품 변동 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 50;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Spline;
                Random rm = new Random(sample.name.GetHashCode());
                series.Color = Color.FromArgb(rm.Next(256), rm.Next(256), rm.Next(256));
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 2;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }

        public void UpdateArtistsIncremetns()
        {
            HitomiAnalysisTrend.Instance.UpdateArtistsIncremetns(checkBox1.Checked);

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 0;
            chart4.ChartAreas[0].AxisX.Interval = 100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "누적 작품 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 100;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Line;
                Random rm = new Random(sample.name.GetHashCode());
                series.Color = Color.FromArgb(rm.Next(256), rm.Next(256), rm.Next(256));
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 2;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }

        public void UpdateArtistsKoreanIncremetns()
        {
            HitomiAnalysisTrend.Instance.UpdateArtistsKoreanIncremetns(checkBox1.Checked);

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 0;
            chart4.ChartAreas[0].AxisX.Interval = 100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "누적 작품 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 10;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Line;
                Random rm = new Random(sample.name.GetHashCode());
                series.Color = Color.FromArgb(rm.Next(256), rm.Next(256), rm.Next(256));
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 2;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }

        public void UpdateArtistsKoreanVariation()
        {
            HitomiAnalysisTrend.Instance.UpdateArtistsKoreanVariation();

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 1125000;//0;
            chart4.ChartAreas[0].AxisX.Interval = 10000;//100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "작품 변동 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 5;
            chart4.ChartAreas[0].AxisY.Minimum = 0;
            chart4.ChartAreas[0].AxisY.Maximum = 20;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Spline;
                Random rm = new Random(sample.name.GetHashCode());
                series.Color = Color.FromArgb(rm.Next(256), rm.Next(256), rm.Next(256));
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 2;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }

        public void UpdateGroupsKoreanIncremetns()
        {
            HitomiAnalysisTrend.Instance.UpdateGroupsKoreanIncremetns(checkBox1.Checked);

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 0;
            chart4.ChartAreas[0].AxisX.Interval = 100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "누적 작품 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 10;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Line;
                Random rm = new Random(sample.name.GetHashCode());
                series.Color = Color.FromArgb(rm.Next(256), rm.Next(256), rm.Next(256));
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 2;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }

        public void UpdateGroupsKoreanVariation()
        {
            HitomiAnalysisTrend.Instance.UpdateGroupsKoreanVariation();

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 1125000;//0;
            chart4.ChartAreas[0].AxisX.Interval = 10000;//100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "작품 변동 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 5;
            chart4.ChartAreas[0].AxisY.Minimum = 0;
            chart4.ChartAreas[0].AxisY.Maximum = 15;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Spline;
                Random rm = new Random(sample.name.GetHashCode());
                series.Color = Color.FromArgb(rm.Next(256), rm.Next(256), rm.Next(256));
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 2;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }

        public void UpdateSeriesKoreanIncremetns()
        {
            HitomiAnalysisTrend.Instance.UpdateSeriesKoreanIncremetns(checkBox1.Checked);

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 0;
            chart4.ChartAreas[0].AxisX.Interval = 100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "누적 작품 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 100;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Line;
                Random rm = new Random(sample.name.GetHashCode());
                series.Color = Color.FromArgb(rm.Next(256), rm.Next(256), rm.Next(256));
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 2;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }

        public void UpdateSeriesKoreanVariation()
        {
            HitomiAnalysisTrend.Instance.UpdateSeriesKoreanVariation();

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 1125000;//0;
            chart4.ChartAreas[0].AxisX.Interval = 10000;//100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "작품 변동 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 5;
            chart4.ChartAreas[0].AxisY.Minimum = 0;
            chart4.ChartAreas[0].AxisY.Maximum = 30;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Spline;
                Random rm = new Random(sample.name.GetHashCode());
                series.Color = Color.FromArgb(rm.Next(256), rm.Next(256), rm.Next(256));
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 2;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }


        public void UpdateCharactersKoreanIncremetns()
        {
            HitomiAnalysisTrend.Instance.UpdateCharactersKoreanIncremetns(checkBox1.Checked);

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 0;
            chart4.ChartAreas[0].AxisX.Interval = 100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "누적 작품 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 50;
            chart4.ChartAreas[0].AxisY.Maximum = 300;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Line;
                Random rm = new Random(sample.name.GetHashCode());
                series.Color = Color.FromArgb(rm.Next(256), rm.Next(256), rm.Next(256));
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 2;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }

        public void UpdateCharactersKoreanVariation()
        {
            HitomiAnalysisTrend.Instance.UpdateCharactersKoreanVariation();

            chart4.Series.Clear();
            chart4.ChartAreas.Clear();
            chart4.ChartAreas.Add("ChartArea1");

            chart4.ChartAreas["ChartArea1"].CursorX.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserEnabled = true;
            chart4.ChartAreas["ChartArea1"].CursorY.IsUserSelectionEnabled = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisX.ScrollBar.IsPositionedInside = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
            chart4.ChartAreas["ChartArea1"].AxisY.ScrollBar.IsPositionedInside = true;

            chart4.ChartAreas[0].AxisX.Title = "아이디 간격";
            chart4.ChartAreas[0].AxisX.TitleFont = Font;
            chart4.ChartAreas[0].AxisX.Minimum = 1125000;//0;
            chart4.ChartAreas[0].AxisX.Interval = 10000;//100000 / 4 * 3;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].AxisY.Title = "작품 변동 수";
            chart4.ChartAreas[0].AxisY.TitleFont = Font;
            chart4.ChartAreas[0].AxisY.Interval = 5;
            chart4.ChartAreas[0].AxisY.Minimum = 0;
            chart4.ChartAreas[0].AxisY.Maximum = 20;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.DarkGray;
            chart4.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart4.ChartAreas[0].BackColor = Color.Gray;

            foreach (var sample in HitomiAnalysisTrend.Instance.samples)
            {
                Series series = new Series();
                series.Name = sample.name;
                series.Font = Font;
                series.ChartArea = "ChartArea1";
                series.ChartType = SeriesChartType.Spline;
                Random rm = new Random(sample.name.GetHashCode());
                series.Color = Color.FromArgb(rm.Next(256), rm.Next(256), rm.Next(256));
                series.LabelBackColor = Color.Gray;
                series.BorderWidth = 2;

                foreach (var point in sample.points)
                    series.Points.Add(new DataPoint(point.X, point.Y));

                chart4.Series.Add(series);
            }
        }

        Point? prevPosition = null;
        ToolTip tooltip = new ToolTip();

        private void chart4_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.Location;
            if (prevPosition.HasValue && pos == prevPosition.Value)
                return;
            tooltip.RemoveAll();
            prevPosition = pos;
            var results = chart4.HitTest(pos.X, pos.Y, false,
                                            ChartElementType.DataPoint);
            foreach (var result in results.Where(result => result.ChartElementType == ChartElementType.DataPoint))
            {
                if (result.Object is DataPoint prop)
                {
                    try
                    {
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        tooltip.Show($"{result.Series.Name}, X={prop.XValue}, Y={prop.YValues[0]} ({HitomiDate.estimate_datetime((int)prop.XValue)})", this.chart4,
                            pos.X, pos.Y - 15);
                    }
                    catch { }
                }
            }
        }
        #endregion

        #region Etc Statistics

        private void UpdateEtc()
        {
            metroLabel9.Text = HitomiData.Instance.metadata_collection.Count.ToString("#,#") + " 개";
            metroLabel10.Text = HitomiData.Instance.tagdata_collection.artist.Count.ToString("#,#") + " 명";
            int tags = 0;
            tags += HitomiData.Instance.tagdata_collection.female.Count;
            tags += HitomiData.Instance.tagdata_collection.male.Count;
            tags += HitomiData.Instance.tagdata_collection.tag.Count;
            metroLabel11.Text = tags.ToString("#,#") + " 개";

            Dictionary<string, int> artist_count = new Dictionary<string, int>();
            foreach (var data in HitomiData.Instance.metadata_collection)
            {
                if (data.Artists == null) continue;
                foreach (var artist in data.Artists)
                    if (artist_count.ContainsKey(artist))
                        artist_count[artist]++;
                    else
                        artist_count.Add(artist, 1);
            }
            var list = artist_count.ToList();
            list.Sort((a, b) => b.Value.CompareTo(a.Value));
            metroLabel14.Text = list[0].Key + $" ({list[0].Value.ToString("#,#")} 개)";
            artist_count.Clear();
            list.Clear();

            string most_tag = "";
            int most_tagc = 0;
            string most_artist = "";
            int most_artistc = 0;
            string most_title = "";
            int count_korean = 0;
            foreach (var data in HitomiData.Instance.metadata_collection)
            {
                if (data.Name != null && most_title.Length < data.Name.Length)
                    most_title = data.Name;
                if (data.Tags != null)
                {
                    if (most_tagc < data.Tags.Length)
                    {
                        most_tag = string.Join(", ", data.Name) + $" ({data.ID}, {most_tagc.ToString("#,#")} 개)";
                        most_tagc = data.Tags.Length;
                    }
                }
                if (data.Artists != null)
                {
                    if (most_artistc < data.Artists.Length)
                    {
                        most_artist = data.Name + $" ({data.ID}, {most_artistc.ToString("#,#")} 명)";
                        most_artistc = data.Artists.Length;
                    }
                }
                if (data.Language == "korean")
                    count_korean++;
            }
            metroLabel15.Text = most_tag;
            metroLabel17.Text = most_artist;
            metroLabel19.Text = most_title;

            ////////////////////////////////////////////////////////

            if (HitomiLog.Instance.GetList().Count > 0)
            {
                metroLabel29.Text = HitomiLog.Instance.GetEnumerator().Count().ToString("#,#") + " 개";
                metroLabel27.Text = HitomiAnalysisArtistCount.Instance.artist_count[0].Key + $" ({HitomiAnalysisArtistCount.Instance.artist_count[0].Value.ToString("#,#")} 개)";
                metroLabel26.Text = HitomiAnalysisTagCount.Instance.tag_count[0].Key + $" ({HitomiAnalysisTagCount.Instance.tag_count[0].Value.ToString("#,#")} 개)";
            }

            ////////////////////////////////////////////////////////

            HashSet<string> id = new HashSet<string>();
            foreach (var hlm in HitomiLog.Instance.GetList())
            {
                id.Add(hlm.Id);
            }

            int count_korean_downloaded = 0;

            foreach (var data in HitomiData.Instance.metadata_collection)
                if (data.Language == "korean" && id.Contains(data.ID.ToString()))
                    count_korean_downloaded++;
            metroLabel28.Text = $"{(double)count_korean_downloaded / count_korean * 100} % ({count_korean_downloaded}/{count_korean})";
            metroLabel23.Text = $"{(double)id.Count / HitomiData.Instance.metadata_collection.Count * 100} % ({id.Count}/{HitomiData.Instance.metadata_collection.Count})";

        }

        #endregion
    }
}
