namespace Hitomi_Copy_3
{
    partial class RecommendControl
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.pb1 = new System.Windows.Forms.PictureBox();
            this.pb2 = new System.Windows.Forms.PictureBox();
            this.pb3 = new System.Windows.Forms.PictureBox();
            this.pb4 = new System.Windows.Forms.PictureBox();
            this.pb5 = new System.Windows.Forms.PictureBox();
            this.tbArtist = new System.Windows.Forms.TextBox();
            this.bDetail = new System.Windows.Forms.Button();
            this.bOpen = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pb1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb5)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.label1.Location = new System.Drawing.Point(18, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "작가";
            // 
            // pb1
            // 
            this.pb1.Location = new System.Drawing.Point(168, 3);
            this.pb1.Name = "pb1";
            this.pb1.Size = new System.Drawing.Size(150, 200);
            this.pb1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb1.TabIndex = 1;
            this.pb1.TabStop = false;
            // 
            // pb2
            // 
            this.pb2.Location = new System.Drawing.Point(324, 3);
            this.pb2.Name = "pb2";
            this.pb2.Size = new System.Drawing.Size(150, 200);
            this.pb2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb2.TabIndex = 2;
            this.pb2.TabStop = false;
            // 
            // pb3
            // 
            this.pb3.Location = new System.Drawing.Point(480, 3);
            this.pb3.Name = "pb3";
            this.pb3.Size = new System.Drawing.Size(150, 200);
            this.pb3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb3.TabIndex = 3;
            this.pb3.TabStop = false;
            // 
            // pb4
            // 
            this.pb4.Location = new System.Drawing.Point(636, 3);
            this.pb4.Name = "pb4";
            this.pb4.Size = new System.Drawing.Size(150, 200);
            this.pb4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb4.TabIndex = 4;
            this.pb4.TabStop = false;
            // 
            // pb5
            // 
            this.pb5.Location = new System.Drawing.Point(792, 3);
            this.pb5.Name = "pb5";
            this.pb5.Size = new System.Drawing.Size(150, 200);
            this.pb5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb5.TabIndex = 5;
            this.pb5.TabStop = false;
            // 
            // tbArtist
            // 
            this.tbArtist.BackColor = System.Drawing.Color.White;
            this.tbArtist.Location = new System.Drawing.Point(22, 37);
            this.tbArtist.Name = "tbArtist";
            this.tbArtist.ReadOnly = true;
            this.tbArtist.Size = new System.Drawing.Size(140, 23);
            this.tbArtist.TabIndex = 8;
            // 
            // bDetail
            // 
            this.bDetail.BackColor = System.Drawing.SystemColors.Control;
            this.bDetail.Location = new System.Drawing.Point(960, 37);
            this.bDetail.Name = "bDetail";
            this.bDetail.Size = new System.Drawing.Size(119, 54);
            this.bDetail.TabIndex = 6;
            this.bDetail.Text = "자세히 보기";
            this.bDetail.UseVisualStyleBackColor = true;
            this.bDetail.Click += new System.EventHandler(this.bDetail_Click);
            // 
            // bOpen
            // 
            this.bOpen.BackColor = System.Drawing.SystemColors.Control;
            this.bOpen.Location = new System.Drawing.Point(960, 111);
            this.bOpen.Name = "bOpen";
            this.bOpen.Size = new System.Drawing.Size(119, 54);
            this.bOpen.TabIndex = 9;
            this.bOpen.Text = "폴더 열기";
            this.bOpen.UseVisualStyleBackColor = true;
            this.bOpen.Click += new System.EventHandler(this.bOpen_Click);
            // 
            // RecommendControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.bOpen);
            this.Controls.Add(this.tbArtist);
            this.Controls.Add(this.bDetail);
            this.Controls.Add(this.pb5);
            this.Controls.Add(this.pb4);
            this.Controls.Add(this.pb3);
            this.Controls.Add(this.pb2);
            this.Controls.Add(this.pb1);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "RecommendControl";
            this.Size = new System.Drawing.Size(1096, 208);
            this.Load += new System.EventHandler(this.RecommendControl_LoadAsync);
            ((System.ComponentModel.ISupportInitialize)(this.pb1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb5)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pb1;
        private System.Windows.Forms.PictureBox pb2;
        private System.Windows.Forms.PictureBox pb3;
        private System.Windows.Forms.PictureBox pb4;
        private System.Windows.Forms.PictureBox pb5;
        private System.Windows.Forms.TextBox tbArtist;
        private System.Windows.Forms.Button bDetail;
        private System.Windows.Forms.Button bOpen;
    }
}
