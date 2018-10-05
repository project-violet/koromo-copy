/* Copyright (C) 2018. Hitomi Parser Developers */

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Hitomi_Copy
{
    public static class ColumnSorter
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);

        public static int ComparePath(string addr1, string addr2)
        {
            // 이 함수는 윈도우 파일시스템 정렬용으로 만들어진 string compare함수입니다.
            return StrCmpLogicalW(addr1, addr2);
        }

        public class SortWrapper
        {
            internal ListViewItem sortItem;

            internal int sortColumn;
            public SortWrapper(ListViewItem Item, int iColumn)
            {
                sortItem = Item;
                sortColumn = iColumn;
            }

            public string Text
            {
                get { return sortItem.SubItems[sortColumn].Text; }
            }

            public class SortComparer : IComparer
            {
                private bool @ascending;
                public SortComparer(bool asc)
                {
                    this.@ascending = asc;
                }

                public int Compare(object x, object y)
                {
                    if ((x == null) | (y == null))
                        return 0;

                    SortWrapper xItem = (SortWrapper)x;
                    SortWrapper yItem = (SortWrapper)y;

                    string xText = xItem.sortItem.SubItems[xItem.sortColumn].Text;
                    string yText = yItem.sortItem.SubItems[yItem.sortColumn].Text;

                    return ComparePath(xText, yText) * (this.@ascending ? 1 : -1);
                }
            }
        }

        public class ColHeader : ColumnHeader
        {
            public bool @ascending;
            public ColHeader(string text, int width, HorizontalAlignment align, bool asc)
            {
                this.Text = text;
                this.Width = width;
                this.TextAlign = align;
                this.@ascending = asc;
            }
        }

        public static void ColumnClickEvent(object sender, ColumnClickEventArgs e)
        {
            ListView lv = (sender as ListView);
            ColHeader clickedCol = (ColHeader)lv.Columns[e.Column];
            clickedCol.@ascending = !clickedCol.@ascending;
            int numItems = lv.Items.Count;
            lv.BeginUpdate();

            ArrayList SortArray = new ArrayList();
            int i = 0;
            for (i = 0; i <= numItems - 1; i++)
            {
                SortArray.Add(new SortWrapper(lv.Items[i], e.Column));
            }

            SortArray.Sort(0, SortArray.Count, new SortWrapper.SortComparer(clickedCol.@ascending));

            lv.Items.Clear();
            int z = 0;
            for (z = 0; z <= numItems - 1; z++)
            {
                lv.Items.Add(((SortWrapper)SortArray[z]).sortItem);
            }

            lv.EndUpdate();
        }

        public static void ColumnToColHeader(ListView lv)
        {
            // 일반 컬럼 헤더를 ColHeader로 변환
            List<ColHeader> columnsTrans = new List<ColHeader>();
            foreach (ColumnHeader column in lv.Columns)
            {
                columnsTrans.Add(new ColHeader(column.Text, column.Width, column.TextAlign, true));
            }
            lv.Columns.Clear();
            lv.Columns.AddRange(columnsTrans.ToArray());
        }

        public static void InitListView(ListView lv)
        {
            ColumnToColHeader(lv);
            lv.ColumnClick += ColumnClickEvent;
        }
    }
}
