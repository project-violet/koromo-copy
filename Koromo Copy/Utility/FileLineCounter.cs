/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Controls;
using Koromo_Copy.Fs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Koromo_Copy.Utility
{
    public partial class FileLineCounter : Form
    {
        public FileLineCounter()
        {
            InitializeComponent();

            ColumnSorter.InitListView(ListView1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private async void button1_Click_1Async(object sender, EventArgs e)
        {
            var ss = textBox1.Text.Split('|');
            FileIndexor indexor = new FileIndexor();
            await indexor.ListingDirectoryAsync(textBox2.Text);
            var x = indexor.GetDirectories();

            var line = 0;
            foreach (var f in x)
            {
                if (f.Contains(@"\packages")) continue;
                var folder = Directory.GetFiles(f);
                foreach (var fn in folder)
                {
                    var extn = Path.GetExtension(fn).ToLower();
                    if (!ss.Contains(extn)) continue;
                    var cc = File.ReadLines(fn).Count();
                    line += cc;
                    
                    ListView1.Items.Add(new ListViewItem(new string[]
                    {
                        fn, cc.ToString() 
                    }));
                }
            }
            label4.Text = line + " 줄";
        }
    }
}
