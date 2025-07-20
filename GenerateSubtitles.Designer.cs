namespace TeletextRecoveryEditor
{
    partial class GenerateSubtitles
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
            this.tbSourceFile = new System.Windows.Forms.TextBox();
            this.btnOpenSource = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnGo = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOpenTarget = new System.Windows.Forms.Button();
            this.tbTargetFile = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbSourceFile
            // 
            this.tbSourceFile.Location = new System.Drawing.Point(133, 12);
            this.tbSourceFile.Name = "tbSourceFile";
            this.tbSourceFile.Size = new System.Drawing.Size(278, 20);
            this.tbSourceFile.TabIndex = 0;
            // 
            // btnOpenSource
            // 
            this.btnOpenSource.Location = new System.Drawing.Point(417, 10);
            this.btnOpenSource.Name = "btnOpenSource";
            this.btnOpenSource.Size = new System.Drawing.Size(75, 23);
            this.btnOpenSource.TabIndex = 1;
            this.btnOpenSource.Text = "Open...";
            this.btnOpenSource.UseVisualStyleBackColor = true;
            this.btnOpenSource.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(297, 94);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 23);
            this.btnGo.TabIndex = 2;
            this.btnGo.Text = "Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Source Subtitles File";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Target T42 File";
            // 
            // btnOpenTarget
            // 
            this.btnOpenTarget.Location = new System.Drawing.Point(417, 45);
            this.btnOpenTarget.Name = "btnOpenTarget";
            this.btnOpenTarget.Size = new System.Drawing.Size(75, 23);
            this.btnOpenTarget.TabIndex = 5;
            this.btnOpenTarget.Text = "Open...";
            this.btnOpenTarget.UseVisualStyleBackColor = true;
            this.btnOpenTarget.Click += new System.EventHandler(this.btnOpenTarget_Click);
            // 
            // tbTargetFile
            // 
            this.tbTargetFile.Location = new System.Drawing.Point(133, 47);
            this.tbTargetFile.Name = "tbTargetFile";
            this.tbTargetFile.Size = new System.Drawing.Size(278, 20);
            this.tbTargetFile.TabIndex = 4;
            // 
            // GenerateSubtitles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(601, 172);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnOpenTarget);
            this.Controls.Add(this.tbTargetFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.btnOpenSource);
            this.Controls.Add(this.tbSourceFile);
            this.Name = "GenerateSubtitles";
            this.Text = "GenerateSubtitles";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbSourceFile;
        private System.Windows.Forms.Button btnOpenSource;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOpenTarget;
        private System.Windows.Forms.TextBox tbTargetFile;
    }
}
