namespace MT3Fine
{
    partial class Form2
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.ShowButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.ObsStart = new System.Windows.Forms.Button();
            this.ObsEndButton = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.ButtonSaveEnd = new System.Windows.Forms.Button();
            this.buttonMakeDark = new System.Windows.Forms.Button();
            this.checkBoxObsAuto = new System.Windows.Forms.CheckBox();
            this.label_frame_rate = new System.Windows.Forms.Label();
            this.label_ID = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.timerDisplay = new System.Windows.Forms.Timer(this.components);
            this.timerObsOnOff = new System.Windows.Forms.Timer(this.components);
            this.icImagingControl1 = new TIS.Imaging.ICImagingControl();
            this.timerMTmonSend = new System.Windows.Forms.Timer(this.components);
            this.timer1min = new System.Windows.Forms.Timer(this.components);
            this.timerWaitShutdown = new System.Windows.Forms.Timer(this.components);
            this.timerSaveTimeOver = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.icImagingControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.checkBox1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBox1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.richTextBox1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 59F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(646, 620);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableLayoutPanel1.SetColumnSpan(this.pictureBox1, 2);
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(640, 480);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.checkBox1.CheckAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.checkBox1.Location = new System.Drawing.Point(3, 489);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(75, 16);
            this.checkBox1.TabIndex = 1;
            this.checkBox1.Text = "DarkMode";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.ShowButton);
            this.flowLayoutPanel1.Controls.Add(this.CloseButton);
            this.flowLayoutPanel1.Controls.Add(this.ObsStart);
            this.flowLayoutPanel1.Controls.Add(this.ObsEndButton);
            this.flowLayoutPanel1.Controls.Add(this.buttonSave);
            this.flowLayoutPanel1.Controls.Add(this.ButtonSaveEnd);
            this.flowLayoutPanel1.Controls.Add(this.buttonMakeDark);
            this.flowLayoutPanel1.Controls.Add(this.checkBoxObsAuto);
            this.flowLayoutPanel1.Controls.Add(this.label_frame_rate);
            this.flowLayoutPanel1.Controls.Add(this.label_ID);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(99, 489);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(544, 48);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // ShowButton
            // 
            this.ShowButton.AutoSize = true;
            this.ShowButton.Location = new System.Drawing.Point(3, 3);
            this.ShowButton.Name = "ShowButton";
            this.ShowButton.Size = new System.Drawing.Size(58, 23);
            this.ShowButton.TabIndex = 0;
            this.ShowButton.Text = "Show";
            this.ShowButton.UseVisualStyleBackColor = true;
            // 
            // CloseButton
            // 
            this.CloseButton.AutoSize = true;
            this.CloseButton.Location = new System.Drawing.Point(67, 3);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(60, 23);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            // 
            // ObsStart
            // 
            this.ObsStart.Location = new System.Drawing.Point(133, 3);
            this.ObsStart.Name = "ObsStart";
            this.ObsStart.Size = new System.Drawing.Size(75, 23);
            this.ObsStart.TabIndex = 4;
            this.ObsStart.Text = "Obs Start";
            this.ObsStart.UseVisualStyleBackColor = true;
            // 
            // ObsEndButton
            // 
            this.ObsEndButton.AutoSize = true;
            this.ObsEndButton.Location = new System.Drawing.Point(214, 3);
            this.ObsEndButton.Name = "ObsEndButton";
            this.ObsEndButton.Size = new System.Drawing.Size(75, 23);
            this.ObsEndButton.TabIndex = 1;
            this.ObsEndButton.Text = "Obs End";
            this.ObsEndButton.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            this.buttonSave.AutoSize = true;
            this.buttonSave.Location = new System.Drawing.Point(295, 3);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 5;
            this.buttonSave.Text = "Save Start";
            this.buttonSave.UseVisualStyleBackColor = true;
            // 
            // ButtonSaveEnd
            // 
            this.ButtonSaveEnd.AutoSize = true;
            this.ButtonSaveEnd.Location = new System.Drawing.Point(376, 3);
            this.ButtonSaveEnd.Name = "ButtonSaveEnd";
            this.ButtonSaveEnd.Size = new System.Drawing.Size(75, 23);
            this.ButtonSaveEnd.TabIndex = 2;
            this.ButtonSaveEnd.Text = "Save End";
            this.ButtonSaveEnd.UseVisualStyleBackColor = true;
            // 
            // buttonMakeDark
            // 
            this.buttonMakeDark.Location = new System.Drawing.Point(457, 3);
            this.buttonMakeDark.Name = "buttonMakeDark";
            this.buttonMakeDark.Size = new System.Drawing.Size(70, 23);
            this.buttonMakeDark.TabIndex = 6;
            this.buttonMakeDark.Text = "MakeDark";
            this.buttonMakeDark.UseVisualStyleBackColor = true;
            // 
            // checkBoxObsAuto
            // 
            this.checkBoxObsAuto.AutoSize = true;
            this.checkBoxObsAuto.Checked = true;
            this.checkBoxObsAuto.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxObsAuto.Location = new System.Drawing.Point(3, 32);
            this.checkBoxObsAuto.Name = "checkBoxObsAuto";
            this.checkBoxObsAuto.Size = new System.Drawing.Size(68, 16);
            this.checkBoxObsAuto.TabIndex = 7;
            this.checkBoxObsAuto.Text = "ObsAuto";
            this.checkBoxObsAuto.UseVisualStyleBackColor = true;
            // 
            // label_frame_rate
            // 
            this.label_frame_rate.AutoSize = true;
            this.label_frame_rate.Location = new System.Drawing.Point(77, 29);
            this.label_frame_rate.Name = "label_frame_rate";
            this.label_frame_rate.Size = new System.Drawing.Size(35, 12);
            this.label_frame_rate.TabIndex = 9;
            this.label_frame_rate.Text = "label1";
            // 
            // label_ID
            // 
            this.label_ID.AutoSize = true;
            this.label_ID.Location = new System.Drawing.Point(118, 29);
            this.label_ID.Name = "label_ID";
            this.label_ID.Size = new System.Drawing.Size(35, 12);
            this.label_ID.TabIndex = 10;
            this.label_ID.Text = "label1";
            // 
            // textBox1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.textBox1, 2);
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(3, 602);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(640, 19);
            this.textBox1.TabIndex = 3;
            // 
            // richTextBox1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.richTextBox1, 2);
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(3, 543);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBox1.Size = new System.Drawing.Size(640, 53);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "JPEG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|All file" +
                "s (*.*)|*.* ";
            this.openFileDialog1.Title = "Select a picture file ";
            // 
            // timerDisplay
            // 
            this.timerDisplay.Enabled = true;
            // 
            // timerObsOnOff
            // 
            this.timerObsOnOff.Enabled = true;
            this.timerObsOnOff.Interval = 10000;
            // 
            // icImagingControl1
            // 
            this.icImagingControl1.BackColor = System.Drawing.Color.White;
            this.icImagingControl1.DeviceLostExecutionMode = TIS.Imaging.EventExecutionMode.AsyncInvoke;
            this.icImagingControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.icImagingControl1.ImageAvailableExecutionMode = TIS.Imaging.EventExecutionMode.MultiThreaded;
            this.icImagingControl1.LiveDisplay = false;
            this.icImagingControl1.LiveDisplayDefault = false;
            this.icImagingControl1.LiveDisplayPosition = new System.Drawing.Point(0, 0);
            this.icImagingControl1.Location = new System.Drawing.Point(0, 0);
            this.icImagingControl1.MemoryCurrentGrabberColorformat = TIS.Imaging.ICImagingControlColorformats.ICY800;
            this.icImagingControl1.Name = "icImagingControl1";
            this.icImagingControl1.Size = new System.Drawing.Size(646, 480);
            this.icImagingControl1.TabIndex = 1;
            // 
            // timerMTmonSend
            // 
            this.timerMTmonSend.Enabled = true;
            this.timerMTmonSend.Interval = 2000;
            // 
            // timer1min
            // 
            this.timer1min.Enabled = true;
            this.timer1min.Interval = 60000;
            // 
            // timerWaitShutdown
            // 
            this.timerWaitShutdown.Interval = 60000;
            // 
            // timerSaveTimeOver
            // 
            this.timerSaveTimeOver.Interval = 45000;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 620);
            this.Controls.Add(this.icImagingControl1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form2";
            this.Text = "MT3Fine";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.icImagingControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button ShowButton;
        private System.Windows.Forms.Button ObsEndButton;
        private System.Windows.Forms.Button ButtonSaveEnd;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button ObsStart;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button buttonMakeDark;
        private System.Windows.Forms.Timer timerDisplay;
        private System.Windows.Forms.Timer timerObsOnOff;
        private System.Windows.Forms.CheckBox checkBoxObsAuto;
        private TIS.Imaging.ICImagingControl icImagingControl1;
        private System.Windows.Forms.Label label_frame_rate;
        private System.Windows.Forms.Label label_ID;
        private System.Windows.Forms.Timer timerMTmonSend;
        private System.Windows.Forms.Timer timer1min;
        private System.Windows.Forms.Timer timerWaitShutdown;
        private System.Windows.Forms.Timer timerSaveTimeOver;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}