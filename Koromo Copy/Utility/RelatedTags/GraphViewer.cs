/* Copyright (C) 2018-2019. Hitomi Parser Developers */

using Hitomi_Copy_3.Graph;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Hitomi_Copy_3
{
    public partial class GraphViewer : Form
    {
        string tag;

        public GraphViewer(string tag = "")
        {
            InitializeComponent();

            this.tag = tag;
            //graph_control.BackColor = Color.FromArgb(40, 40, 50);
        }

        private void GraphViewer_Load(object sender, EventArgs e)
        {
            graph_control.init_graph(tag);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

    }
}
