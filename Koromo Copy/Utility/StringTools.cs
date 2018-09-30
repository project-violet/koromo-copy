/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Fs;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Koromo_Copy.Utility
{
    public partial class StringTools : Form
    {
        public StringTools()
        {
            InitializeComponent();
        }

        #region 정렬

        #endregion

        #region 파일 목록

        private async void tbPath_KeyDownAsync(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;

                FileIndexor fi = new FileIndexor();
                await fi.ListingDirectoryAsync(tbPath.Text);
                StringBuilder builder = new StringBuilder();
                
                fi.Enumerate((x, y) =>
                {
                    if (!checkBox4.Checked)
                        x = x.Replace(fi.RootDirectory + '\\', "");
                    if (checkBox1.Checked)
                        builder.Append(x.TrimEnd('\\') + "\r\n");
                    if (checkBox3.Checked)
                        y.ForEach(z =>
                        {
                            if (!checkBox4.Checked)
                                builder.Append(z.FullName.Replace(fi.RootDirectory + '\\', "").TrimEnd('\\') + "\r\n");
                            else
                                builder.Append(z.FullName.TrimEnd('\\') + "\r\n");
                        });
                }, checkBox2.Checked);

                tbFileIndexing.Text = builder.ToString();
            }
        }

        #endregion
    }
}
