namespace Teletext
{
    partial class ExportSubtitles
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
            this.tbOutputFile = new System.Windows.Forms.TextBox();
            this.btnChooseSrtFile = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.btnGo = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.numFrames = new System.Windows.Forms.NumericUpDown();
            this.cbAllFrames = new System.Windows.Forms.CheckBox();
            this.cbResolution = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbTextOnly = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.mtbSubsPage = new System.Windows.Forms.MaskedTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numFrames)).BeginInit();
            this.SuspendLayout();
            // 
            // tbOutputFile
            // 
            this.tbOutputFile.Enabled = false;
            this.tbOutputFile.Location = new System.Drawing.Point(114, 13);
            this.tbOutputFile.Name = "tbOutputFile";
            this.tbOutputFile.Size = new System.Drawing.Size(218, 20);
            this.tbOutputFile.TabIndex = 0;
            // 
            // btnChooseSrtFile
            // 
            this.btnChooseSrtFile.Location = new System.Drawing.Point(338, 11);
            this.btnChooseSrtFile.Name = "btnChooseSrtFile";
            this.btnChooseSrtFile.Size = new System.Drawing.Size(75, 23);
            this.btnChooseSrtFile.TabIndex = 1;
            this.btnChooseSrtFile.Text = "Open";
            this.btnChooseSrtFile.UseVisualStyleBackColor = true;
            this.btnChooseSrtFile.Click += new System.EventHandler(this.btnChooseSrtFile_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Subtitle Output File";
            // 
            // btnGo
            // 
            this.btnGo.Enabled = false;
            this.btnGo.Location = new System.Drawing.Point(338, 166);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 23);
            this.btnGo.TabIndex = 3;
            this.btnGo.Text = "Export";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Output n frames";
            // 
            // numFrames
            // 
            this.numFrames.Location = new System.Drawing.Point(114, 93);
            this.numFrames.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numFrames.Name = "numFrames";
            this.numFrames.Size = new System.Drawing.Size(58, 20);
            this.numFrames.TabIndex = 5;
            this.numFrames.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // cbAllFrames
            // 
            this.cbAllFrames.AutoSize = true;
            this.cbAllFrames.Location = new System.Drawing.Point(304, 96);
            this.cbAllFrames.Name = "cbAllFrames";
            this.cbAllFrames.Size = new System.Drawing.Size(109, 17);
            this.cbAllFrames.TabIndex = 6;
            this.cbAllFrames.Text = "Output All Frames";
            this.cbAllFrames.UseVisualStyleBackColor = true;
            this.cbAllFrames.CheckedChanged += new System.EventHandler(this.cbAllFrames_CheckedChanged);
            // 
            // cbResolution
            // 
            this.cbResolution.FormattingEnabled = true;
            this.cbResolution.Items.AddRange(new object[] {
            "PAL 4:3 768 x 576",
            "HD 16:9 720",
            "HD 16:9 1080"});
            this.cbResolution.Location = new System.Drawing.Point(114, 130);
            this.cbResolution.Name = "cbResolution";
            this.cbResolution.Size = new System.Drawing.Size(121, 21);
            this.cbResolution.TabIndex = 9;
            this.cbResolution.Text = "PAL 4:3 768 x 576";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(51, 133);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Resolution";
            // 
            // cbTextOnly
            // 
            this.cbTextOnly.AutoSize = true;
            this.cbTextOnly.Location = new System.Drawing.Point(114, 170);
            this.cbTextOnly.Name = "cbTextOnly";
            this.cbTextOnly.Size = new System.Drawing.Size(158, 17);
            this.cbTextOnly.TabIndex = 11;
            this.cbTextOnly.Text = "Output Text Only (no PNGs)";
            this.cbTextOnly.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Subtitles Page";
            // 
            // mtbSubsPage
            // 
            this.mtbSubsPage.Location = new System.Drawing.Point(114, 52);
            this.mtbSubsPage.Mask = "0AA";
            this.mtbSubsPage.Name = "mtbSubsPage";
            this.mtbSubsPage.Size = new System.Drawing.Size(27, 20);
            this.mtbSubsPage.TabIndex = 14;
            this.mtbSubsPage.Text = "888";
            // 
            // ExportSubtitles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(442, 208);
            this.Controls.Add(this.mtbSubsPage);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbTextOnly);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbResolution);
            this.Controls.Add(this.cbAllFrames);
            this.Controls.Add(this.numFrames);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnChooseSrtFile);
            this.Controls.Add(this.tbOutputFile);
            this.Name = "ExportSubtitles";
            this.Text = "ExportSubtitles";
            ((System.ComponentModel.ISupportInitialize)(this.numFrames)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbOutputFile;
        private System.Windows.Forms.Button btnChooseSrtFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numFrames;
        private System.Windows.Forms.CheckBox cbAllFrames;
        private System.Windows.Forms.ComboBox cbResolution;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbTextOnly;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MaskedTextBox mtbSubsPage;
    }
}
