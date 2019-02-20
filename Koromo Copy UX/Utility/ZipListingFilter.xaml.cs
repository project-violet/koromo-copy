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
    /// ZipListingFilter.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZipListingFilter : UserControl
    {
        public ZipListingFilter(List<DateTime> dates, DateTime? starts, DateTime? ends)
        {
            InitializeComponent();

            if (dates.Count > 0)
            {
                StartDate.DisplayDateStart = dates.Min();
                StartDate.DisplayDate = dates.Min();
                StartDate.DisplayDateEnd = dates.Max();

                if (starts.HasValue) StartDate.SelectedDate = starts;

                EndDate.DisplayDateStart = dates.Min();
                EndDate.DisplayDateEnd = dates.Max();

                if (ends.HasValue) EndDate.SelectedDate = ends;
            }
        }

        private void StartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            EndDate.DisplayDateStart = StartDate.SelectedDate;
        }

        private void EndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            StartDate.DisplayDateEnd = EndDate.SelectedDate;
        }
    }
}
