namespace Teletext
{
    partial class frmCreateTeletextStream
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
            this.tbLinesPerField = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnGenerateTextStream = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbLinesPerField
            // 
            this.tbLinesPerField.Location = new System.Drawing.Point(12, 12);
            this.tbLinesPerField.Name = "tbLinesPerField";
            this.tbLinesPerField.Size = new System.Drawing.Size(23, 20);
            this.tbLinesPerField.TabIndex = 0;
            this.tbLinesPerField.Text = "12";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "VBI Lines per Field (max 32)";
            // 
            // btnGenerateTextStream
            // 
            this.btnGenerateTextStream.Location = new System.Drawing.Point(104, 226);
            this.btnGenerateTextStream.Name = "btnGenerateTextStream";
            this.btnGenerateTextStream.Size = new System.Drawing.Size(75, 23);
            this.btnGenerateTextStream.TabIndex = 2;
            this.btnGenerateTextStream.Text = "Generate";
            this.btnGenerateTextStream.UseVisualStyleBackColor = true;
            this.btnGenerateTextStream.Click += new System.EventHandler(this.btnGenerateTextStream_Click);
            // 
            // frmCreateTeletextStream
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.btnGenerateTextStream);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbLinesPerField);
            this.Name = "frmCreateTeletextStream";
            this.Text = "CreateTeletextStream";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbLinesPerField;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnGenerateTextStream;
    }
}
