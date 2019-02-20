/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Data;

namespace Koromo_Copy_UX.Domain
{
    public static class SortAlgorithm
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);

        public static int ComparePath(string addr1, string addr2)
        {
            // 이 함수는 윈도우 파일시스템 정렬용으로 만들어진 string compare함수입니다.
            return StrCmpLogicalW(addr1, addr2);
        }
    }
    
    public class DataGridSorter<T> where T : new()
    {
        DataGrid data_grid;

        public class SortComparer : IComparer
        {
            private bool @ascending;
            private string column;
            private T comparator;

            public SortComparer(bool asc, string column)
            {
                this.@ascending = asc;
                comparator = new T();
                this.column = column;
            }

            public int Compare(object x, object y)
            {
                if ((x == null) | (y == null))
                    return 0;

                T xItem = (T)x;
                T yItem = (T)y;
                
                string xText = xItem.GetType().GetProperty(column, BindingFlags.Public | BindingFlags.Instance).GetValue(xItem).ToString();
                string yText = yItem.GetType().GetProperty(column, BindingFlags.Public | BindingFlags.Instance).GetValue(yItem).ToString();

                return SortAlgorithm.ComparePath(xText, yText) * (this.@ascending ? 1 : -1);
            }
        }

        public DataGridSorter(DataGrid data_grid)
        {
            this.data_grid = data_grid;
        }
        
        public void SortHandler(object sender, DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;
            IComparer comparer = null;

            ListSortDirection direction = (column.SortDirection != ListSortDirection.Ascending) 
                ? ListSortDirection.Ascending : ListSortDirection.Descending;
            column.SortDirection = direction;
            
            ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(data_grid.ItemsSource);
            
            comparer = new SortComparer(direction == 0 ? false : true, e.Column.SortMemberPath);
            lcv.CustomSort = comparer;

            e.Handled = true;
        }
    }
}
