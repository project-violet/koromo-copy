namespace Koromo_Copy.Utility
{
    partial class HitomiExplorer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HitomiExplorer));
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.metroLabel4 = new MetroFramework.Controls.MetroLabel();
            this.AvailableList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.metroLabel5 = new MetroFramework.Controls.MetroLabel();
            this.bApply = new System.Windows.Forms.Button();
            this.tbRule = new System.Windows.Forms.TextBox();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.bOpen = new System.Windows.Forms.Button();
            this.tbPath = new System.Windows.Forms.TextBox();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.PathTree = new Koromo_Copy.Controls.NativeTreeView();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // metroLabel1
            // 
            this.metroLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(702, 11);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(468, 19);
            this.metroLabel1.TabIndex = 5;
            this.metroLabel1.Text = "Copyright (C) 2018. DCInside Programming Gallery Union. All Rights Reserved.";
            this.metroLabel1.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 33);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1158, 527);
            this.tabControl1.TabIndex = 6;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Controls.Add(this.bApply);
            this.tabPage1.Controls.Add(this.tbRule);
            this.tabPage1.Controls.Add(this.metroLabel3);
            this.tabPage1.Controls.Add(this.bOpen);
            this.tabPage1.Controls.Add(this.tbPath);
            this.tabPage1.Controls.Add(this.metroLabel2);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1150, 499);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "메인";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(29, 86);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.PathTree);
            this.splitContainer1.Panel1.Controls.Add(this.metroLabel4);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.AvailableList);
            this.splitContainer1.Panel2.Controls.Add(this.metroLabel5);
            this.splitContainer1.Size = new System.Drawing.Size(1094, 379);
            this.splitContainer1.SplitterDistance = 318;
            this.splitContainer1.TabIndex = 16;
            // 
            // metroLabel4
            // 
            this.metroLabel4.AutoSize = true;
            this.metroLabel4.Location = new System.Drawing.Point(3, 0);
            this.metroLabel4.Name = "metroLabel4";
            this.metroLabel4.Size = new System.Drawing.Size(94, 19);
            this.metroLabel4.TabIndex = 14;
            this.metroLabel4.Text = "파일 시스템 : ";
            this.metroLabel4.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // AvailableList
            // 
            this.AvailableList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AvailableList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.AvailableList.FullRowSelect = true;
            this.AvailableList.GridLines = true;
            this.AvailableList.Location = new System.Drawing.Point(3, 22);
            this.AvailableList.Name = "AvailableList";
            this.AvailableList.Size = new System.Drawing.Size(768, 354);
            this.AvailableList.TabIndex = 13;
            this.AvailableList.UseCompatibleStateImageBehavior = false;
            this.AvailableList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "인덱스";
            this.columnHeader1.Width = 78;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "파일경로";
            this.columnHeader2.Width = 446;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "추출된 이름";
            this.columnHeader3.Width = 137;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "가용성";
            // 
            // metroLabel5
            // 
            this.metroLabel5.AutoSize = true;
            this.metroLabel5.Location = new System.Drawing.Point(3, 0);
            this.metroLabel5.Name = "metroLabel5";
            this.metroLabel5.Size = new System.Drawing.Size(122, 19);
            this.metroLabel5.TabIndex = 15;
            this.metroLabel5.Text = "가용성있는 항목 : ";
            this.metroLabel5.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // bApply
            // 
            this.bApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bApply.Location = new System.Drawing.Point(1049, 51);
            this.bApply.Name = "bApply";
            this.bApply.Size = new System.Drawing.Size(74, 24);
            this.bApply.TabIndex = 12;
            this.bApply.Text = "적용";
            this.bApply.UseVisualStyleBackColor = true;
            this.bApply.Click += new System.EventHandler(this.bApply_Click);
            // 
            // tbRule
            // 
            this.tbRule.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRule.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbRule.Location = new System.Drawing.Point(83, 52);
            this.tbRule.Name = "tbRule";
            this.tbRule.Size = new System.Drawing.Size(960, 22);
            this.tbRule.TabIndex = 10;
            this.tbRule.Text = "^\\[(\\d+)\\], ^\\[.*?\\((\\d+)\\).*?\\], \\((\\d+)\\)$";
            // 
            // metroLabel3
            // 
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.Location = new System.Drawing.Point(29, 54);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(48, 19);
            this.metroLabel3.TabIndex = 9;
            this.metroLabel3.Text = "규칙 : ";
            this.metroLabel3.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // bOpen
            // 
            this.bOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bOpen.Location = new System.Drawing.Point(1049, 22);
            this.bOpen.Name = "bOpen";
            this.bOpen.Size = new System.Drawing.Size(74, 24);
            this.bOpen.TabIndex = 8;
            this.bOpen.Text = "열기";
            this.bOpen.UseVisualStyleBackColor = true;
            this.bOpen.Click += new System.EventHandler(this.bOpen_ClickAsync);
            // 
            // tbPath
            // 
            this.tbPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPath.Font = new System.Drawing.Font("Consolas", 9F);
            this.tbPath.Location = new System.Drawing.Point(83, 23);
            this.tbPath.Name = "tbPath";
            this.tbPath.Size = new System.Drawing.Size(960, 22);
            this.tbPath.TabIndex = 7;
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(29, 25);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(48, 19);
            this.metroLabel2.TabIndex = 6;
            this.metroLabel2.Text = "경로 : ";
            this.metroLabel2.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1150, 499);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "작업";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // PathTree
            // 
            this.PathTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PathTree.Location = new System.Drawing.Point(3, 22);
            this.PathTree.Name = "PathTree";
            this.PathTree.ShowLines = false;
            this.PathTree.Size = new System.Drawing.Size(312, 354);
            this.PathTree.TabIndex = 15;
            // 
            // HitomiExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1182, 568);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.metroLabel1);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "HitomiExplorer";
            this.Text = "Hitomi Explorer";
            this.Load += new System.EventHandler(this.HitomiExplorer_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroLabel metroLabel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private MetroFramework.Controls.MetroLabel metroLabel4;
        private System.Windows.Forms.ListView AvailableList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private MetroFramework.Controls.MetroLabel metroLabel5;
        private System.Windows.Forms.Button bApply;
        private System.Windows.Forms.TextBox tbRule;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private System.Windows.Forms.Button bOpen;
        private System.Windows.Forms.TextBox tbPath;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private System.Windows.Forms.TabPage tabPage2;
        private Controls.NativeTreeView PathTree;
    }
}