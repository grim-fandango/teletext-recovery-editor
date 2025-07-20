namespace Teletext
{
    partial class frmBase64UrlImport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbBase64Url = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnBase64UrlImport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbBase64Url
            // 
            this.tbBase64Url.Location = new System.Drawing.Point(24, 43);
            this.tbBase64Url.Name = "tbBase64Url";
            this.tbBase64Url.Size = new System.Drawing.Size(713, 20);
            this.tbBase64Url.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Base64Url";
            // 
            // btnBase64UrlImport
            // 
            this.btnBase64UrlImport.Location = new System.Drawing.Point(760, 25);
            this.btnBase64UrlImport.Name = "btnBase64UrlImport";
            this.btnBase64UrlImport.Size = new System.Drawing.Size(75, 55);
            this.btnBase64UrlImport.TabIndex = 2;
            this.btnBase64UrlImport.Text = "Import to Clipboard";
            this.btnBase64UrlImport.UseVisualStyleBackColor = true;
            this.btnBase64UrlImport.Click += new System.EventHandler(this.btnBase64UrlImport_Click);
            // 
            // frmBase64UrlImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 100);
            this.Controls.Add(this.btnBase64UrlImport);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbBase64Url);
            this.Name = "frmBase64UrlImport";
            this.Text = "Base64Url Import";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbBase64Url;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnBase64UrlImport;
    }
}
