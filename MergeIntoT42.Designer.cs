namespace Teletext
{
    partial class frmMergeIntoT42
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
            this.tbBinariesFolder = new System.Windows.Forms.TextBox();
            this.lblBinariesFolder = new System.Windows.Forms.Label();
            this.btnSelectBinaries = new System.Windows.Forms.Button();
            this.lblT42File = new System.Windows.Forms.Label();
            this.tbT42File = new System.Windows.Forms.TextBox();
            this.btnSelectT42 = new System.Windows.Forms.Button();
            this.lblMergedFile = new System.Windows.Forms.Label();
            this.tbMergedFile = new System.Windows.Forms.TextBox();
            this.btnMergedFile = new System.Windows.Forms.Button();
            this.btnMerge = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.stLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.stProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.cbThreaded = new System.Windows.Forms.CheckBox();
            this.cbClearDB = new System.Windows.Forms.CheckBox();
            this.cbTimeCodedSubpages = new System.Windows.Forms.CheckBox();
            this.tbMergeOnlyPage = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbFixClock = new System.Windows.Forms.CheckBox();
            this.cbFixHeader = new System.Windows.Forms.CheckBox();
            this.tbHeaderTemplate = new System.Windows.Forms.TextBox();
            this.cbSubpagesAreSeparateFiles = new System.Windows.Forms.CheckBox();
            this.nudLinePadding = new System.Windows.Forms.NumericUpDown();
            this.lblLinePadding = new System.Windows.Forms.Label();
            this.tbClockTemplate = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbForceInitClockDigits = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLinePadding)).BeginInit();
            this.SuspendLayout();
            // 
            // tbBinariesFolder
            // 
            this.tbBinariesFolder.AllowDrop = true;
            this.tbBinariesFolder.Location = new System.Drawing.Point(102, 25);
            this.tbBinariesFolder.Name = "tbBinariesFolder";
            this.tbBinariesFolder.Size = new System.Drawing.Size(676, 20);
            this.tbBinariesFolder.TabIndex = 0;
            this.tbBinariesFolder.Text = "C:\\Users\\jasonrob\\Documents\\Personal\\Teletext\\bbc1_1993-01-03_guesser\\restored";
            this.tbBinariesFolder.TextChanged += new System.EventHandler(this.TbBinariesFolder_TextChanged);
            this.tbBinariesFolder.DragDrop += new System.Windows.Forms.DragEventHandler(this.TbBinariesFolder_DragDrop);
            this.tbBinariesFolder.DragEnter += new System.Windows.Forms.DragEventHandler(this.TbBinariesFolder_DragEnter);
            // 
            // lblBinariesFolder
            // 
            this.lblBinariesFolder.AutoSize = true;
            this.lblBinariesFolder.Location = new System.Drawing.Point(20, 28);
            this.lblBinariesFolder.Name = "lblBinariesFolder";
            this.lblBinariesFolder.Size = new System.Drawing.Size(76, 13);
            this.lblBinariesFolder.TabIndex = 1;
            this.lblBinariesFolder.Text = "Binaries Folder";
            // 
            // btnSelectBinaries
            // 
            this.btnSelectBinaries.Enabled = false;
            this.btnSelectBinaries.Location = new System.Drawing.Point(787, 23);
            this.btnSelectBinaries.Name = "btnSelectBinaries";
            this.btnSelectBinaries.Size = new System.Drawing.Size(75, 23);
            this.btnSelectBinaries.TabIndex = 2;
            this.btnSelectBinaries.Text = "Select...";
            this.btnSelectBinaries.UseVisualStyleBackColor = true;
            // 
            // lblT42File
            // 
            this.lblT42File.AutoSize = true;
            this.lblT42File.Location = new System.Drawing.Point(51, 65);
            this.lblT42File.Name = "lblT42File";
            this.lblT42File.Size = new System.Drawing.Size(45, 13);
            this.lblT42File.TabIndex = 3;
            this.lblT42File.Text = "T42 File";
            // 
            // tbT42File
            // 
            this.tbT42File.AllowDrop = true;
            this.tbT42File.Location = new System.Drawing.Point(102, 62);
            this.tbT42File.Name = "tbT42File";
            this.tbT42File.Size = new System.Drawing.Size(676, 20);
            this.tbT42File.TabIndex = 4;
            this.tbT42File.Text = "C:\\Users\\jasonrob\\Documents\\Personal\\Teletext\\bbc1_1993-01-03_guesser\\BBC1-1993-0" +
    "1-03-1302-1400-sp.t42";
            this.tbT42File.TextChanged += new System.EventHandler(this.TbT42File_TextChanged);
            this.tbT42File.DragDrop += new System.Windows.Forms.DragEventHandler(this.TbT42File_DragDrop);
            this.tbT42File.DragEnter += new System.Windows.Forms.DragEventHandler(this.TbT42File_DragEnter);
            // 
            // btnSelectT42
            // 
            this.btnSelectT42.Enabled = false;
            this.btnSelectT42.Location = new System.Drawing.Point(787, 60);
            this.btnSelectT42.Name = "btnSelectT42";
            this.btnSelectT42.Size = new System.Drawing.Size(75, 23);
            this.btnSelectT42.TabIndex = 5;
            this.btnSelectT42.Text = "Select...";
            this.btnSelectT42.UseVisualStyleBackColor = true;
            // 
            // lblMergedFile
            // 
            this.lblMergedFile.AutoSize = true;
            this.lblMergedFile.Location = new System.Drawing.Point(12, 104);
            this.lblMergedFile.Name = "lblMergedFile";
            this.lblMergedFile.Size = new System.Drawing.Size(84, 13);
            this.lblMergedFile.TabIndex = 6;
            this.lblMergedFile.Text = "Merged T42 File";
            // 
            // tbMergedFile
            // 
            this.tbMergedFile.AllowDrop = true;
            this.tbMergedFile.Location = new System.Drawing.Point(102, 101);
            this.tbMergedFile.Name = "tbMergedFile";
            this.tbMergedFile.Size = new System.Drawing.Size(676, 20);
            this.tbMergedFile.TabIndex = 7;
            this.tbMergedFile.Text = "C:\\Users\\jasonrob\\Documents\\Personal\\Teletext\\bbc1_1993-01-03_guesser\\BBC1-1993-0" +
    "1-03-1302-1400-sp_merged.t42";
            this.tbMergedFile.TextChanged += new System.EventHandler(this.TbMergedFile_TextChanged);
            this.tbMergedFile.DragDrop += new System.Windows.Forms.DragEventHandler(this.TbMergedFile_DragDrop);
            this.tbMergedFile.DragEnter += new System.Windows.Forms.DragEventHandler(this.TbMergedFile_DragEnter);
            // 
            // btnMergedFile
            // 
            this.btnMergedFile.Enabled = false;
            this.btnMergedFile.Location = new System.Drawing.Point(787, 99);
            this.btnMergedFile.Name = "btnMergedFile";
            this.btnMergedFile.Size = new System.Drawing.Size(75, 23);
            this.btnMergedFile.TabIndex = 8;
            this.btnMergedFile.Text = "Select...";
            this.btnMergedFile.UseVisualStyleBackColor = true;
            // 
            // btnMerge
            // 
            this.btnMerge.Location = new System.Drawing.Point(787, 261);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(75, 23);
            this.btnMerge.TabIndex = 9;
            this.btnMerge.Text = "Merge";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.btnMerge_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stLabel,
            this.stProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 299);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(874, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // stLabel
            // 
            this.stLabel.AutoSize = false;
            this.stLabel.Name = "stLabel";
            this.stLabel.Size = new System.Drawing.Size(200, 17);
            this.stLabel.Text = "Ready";
            this.stLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // stProgressBar
            // 
            this.stProgressBar.AutoSize = false;
            this.stProgressBar.Name = "stProgressBar";
            this.stProgressBar.Size = new System.Drawing.Size(450, 16);
            // 
            // cbThreaded
            // 
            this.cbThreaded.AutoSize = true;
            this.cbThreaded.Checked = true;
            this.cbThreaded.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbThreaded.Location = new System.Drawing.Point(102, 141);
            this.cbThreaded.Name = "cbThreaded";
            this.cbThreaded.Size = new System.Drawing.Size(72, 17);
            this.cbThreaded.TabIndex = 11;
            this.cbThreaded.Text = "Threaded";
            this.cbThreaded.UseVisualStyleBackColor = true;
            this.cbThreaded.CheckedChanged += new System.EventHandler(this.CbThreaded_CheckedChanged);
            // 
            // cbClearDB
            // 
            this.cbClearDB.AutoSize = true;
            this.cbClearDB.Location = new System.Drawing.Point(251, 141);
            this.cbClearDB.Name = "cbClearDB";
            this.cbClearDB.Size = new System.Drawing.Size(98, 17);
            this.cbClearDB.TabIndex = 12;
            this.cbClearDB.Text = "Clear Entire DB";
            this.cbClearDB.UseVisualStyleBackColor = true;
            this.cbClearDB.CheckedChanged += new System.EventHandler(this.CbClearDB_CheckedChanged);
            // 
            // cbTimeCodedSubpages
            // 
            this.cbTimeCodedSubpages.AutoSize = true;
            this.cbTimeCodedSubpages.Location = new System.Drawing.Point(632, 141);
            this.cbTimeCodedSubpages.Name = "cbTimeCodedSubpages";
            this.cbTimeCodedSubpages.Size = new System.Drawing.Size(146, 17);
            this.cbTimeCodedSubpages.TabIndex = 13;
            this.cbTimeCodedSubpages.Text = "Has timecoded subpages";
            this.cbTimeCodedSubpages.UseVisualStyleBackColor = true;
            this.cbTimeCodedSubpages.CheckedChanged += new System.EventHandler(this.CbTimeCodedSubpages_CheckedChanged);
            // 
            // tbMergeOnlyPage
            // 
            this.tbMergeOnlyPage.Location = new System.Drawing.Point(251, 264);
            this.tbMergeOnlyPage.Name = "tbMergeOnlyPage";
            this.tbMergeOnlyPage.Size = new System.Drawing.Size(72, 20);
            this.tbMergeOnlyPage.TabIndex = 14;
            this.tbMergeOnlyPage.TextChanged += new System.EventHandler(this.TbMergeOnlyPage_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(99, 267);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Merge Only Page...";
            // 
            // cbFixClock
            // 
            this.cbFixClock.AutoSize = true;
            this.cbFixClock.Checked = true;
            this.cbFixClock.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbFixClock.Location = new System.Drawing.Point(102, 189);
            this.cbFixClock.Name = "cbFixClock";
            this.cbFixClock.Size = new System.Drawing.Size(69, 17);
            this.cbFixClock.TabIndex = 16;
            this.cbFixClock.Text = "Fix Clock";
            this.cbFixClock.UseVisualStyleBackColor = true;
            this.cbFixClock.CheckedChanged += new System.EventHandler(this.CbFixClock_CheckedChanged);
            // 
            // cbFixHeader
            // 
            this.cbFixHeader.AutoSize = true;
            this.cbFixHeader.Checked = true;
            this.cbFixHeader.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbFixHeader.Location = new System.Drawing.Point(102, 166);
            this.cbFixHeader.Name = "cbFixHeader";
            this.cbFixHeader.Size = new System.Drawing.Size(77, 17);
            this.cbFixHeader.TabIndex = 17;
            this.cbFixHeader.Text = "Fix Header";
            this.cbFixHeader.UseVisualStyleBackColor = true;
            this.cbFixHeader.CheckedChanged += new System.EventHandler(this.CbFixHeader_CheckedChanged);
            // 
            // tbHeaderTemplate
            // 
            this.tbHeaderTemplate.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbHeaderTemplate.Location = new System.Drawing.Point(251, 164);
            this.tbHeaderTemplate.Name = "tbHeaderTemplate";
            this.tbHeaderTemplate.Size = new System.Drawing.Size(527, 18);
            this.tbHeaderTemplate.TabIndex = 18;
            this.tbHeaderTemplate.Text = "CEEFAX mpp  DAY DD MTH \\u0003";
            this.tbHeaderTemplate.TextChanged += new System.EventHandler(this.TbHeaderTemplate_TextChanged);
            // 
            // cbSubpagesAreSeparateFiles
            // 
            this.cbSubpagesAreSeparateFiles.AutoSize = true;
            this.cbSubpagesAreSeparateFiles.Location = new System.Drawing.Point(405, 141);
            this.cbSubpagesAreSeparateFiles.Name = "cbSubpagesAreSeparateFiles";
            this.cbSubpagesAreSeparateFiles.Size = new System.Drawing.Size(175, 17);
            this.cbSubpagesAreSeparateFiles.TabIndex = 19;
            this.cbSubpagesAreSeparateFiles.Text = "Subpages Are In Separate Files";
            this.cbSubpagesAreSeparateFiles.UseVisualStyleBackColor = true;
            this.cbSubpagesAreSeparateFiles.CheckedChanged += new System.EventHandler(this.CbSubpagesAreSeparateFiles_CheckedChanged);
            // 
            // nudLinePadding
            // 
            this.nudLinePadding.Location = new System.Drawing.Point(251, 231);
            this.nudLinePadding.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudLinePadding.Name = "nudLinePadding";
            this.nudLinePadding.Size = new System.Drawing.Size(60, 20);
            this.nudLinePadding.TabIndex = 21;
            this.nudLinePadding.ValueChanged += new System.EventHandler(this.NudLinePadding_ValueChanged);
            // 
            // lblLinePadding
            // 
            this.lblLinePadding.AutoSize = true;
            this.lblLinePadding.Location = new System.Drawing.Point(99, 233);
            this.lblLinePadding.Name = "lblLinePadding";
            this.lblLinePadding.Size = new System.Drawing.Size(109, 13);
            this.lblLinePadding.TabIndex = 22;
            this.lblLinePadding.Text = "Line Padding per field";
            // 
            // tbClockTemplate
            // 
            this.tbClockTemplate.Location = new System.Drawing.Point(335, 187);
            this.tbClockTemplate.Name = "tbClockTemplate";
            this.tbClockTemplate.Size = new System.Drawing.Size(100, 20);
            this.tbClockTemplate.TabIndex = 23;
            this.tbClockTemplate.Text = "Hh1Mm2Ss";
            this.tbClockTemplate.TextChanged += new System.EventHandler(this.TbClockTemplate_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(248, 190);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "Clock Template";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(493, 190);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Force Initial Clock Digits";
            // 
            // tbForceInitClockDigits
            // 
            this.tbForceInitClockDigits.Location = new System.Drawing.Point(619, 187);
            this.tbForceInitClockDigits.Name = "tbForceInitClockDigits";
            this.tbForceInitClockDigits.Size = new System.Drawing.Size(100, 20);
            this.tbForceInitClockDigits.TabIndex = 26;
            this.tbForceInitClockDigits.Text = "xx:xx/xx";
            this.tbForceInitClockDigits.TextChanged += new System.EventHandler(this.TbForceInitClockDigits_TextChanged);
            // 
            // frmMergeIntoT42
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(874, 321);
            this.Controls.Add(this.tbForceInitClockDigits);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbClockTemplate);
            this.Controls.Add(this.lblLinePadding);
            this.Controls.Add(this.nudLinePadding);
            this.Controls.Add(this.cbSubpagesAreSeparateFiles);
            this.Controls.Add(this.tbHeaderTemplate);
            this.Controls.Add(this.cbFixHeader);
            this.Controls.Add(this.cbFixClock);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbMergeOnlyPage);
            this.Controls.Add(this.cbTimeCodedSubpages);
            this.Controls.Add(this.cbClearDB);
            this.Controls.Add(this.cbThreaded);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnMerge);
            this.Controls.Add(this.btnMergedFile);
            this.Controls.Add(this.tbMergedFile);
            this.Controls.Add(this.lblMergedFile);
            this.Controls.Add(this.btnSelectT42);
            this.Controls.Add(this.tbT42File);
            this.Controls.Add(this.lblT42File);
            this.Controls.Add(this.btnSelectBinaries);
            this.Controls.Add(this.lblBinariesFolder);
            this.Controls.Add(this.tbBinariesFolder);
            this.Name = "frmMergeIntoT42";
            this.Text = "Merge Binaries Folder Into a T42 File";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLinePadding)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbBinariesFolder;
        private System.Windows.Forms.Label lblBinariesFolder;
        private System.Windows.Forms.Button btnSelectBinaries;
        private System.Windows.Forms.Label lblT42File;
        private System.Windows.Forms.TextBox tbT42File;
        private System.Windows.Forms.Button btnSelectT42;
        private System.Windows.Forms.Label lblMergedFile;
        private System.Windows.Forms.TextBox tbMergedFile;
        private System.Windows.Forms.Button btnMergedFile;
        private System.Windows.Forms.Button btnMerge;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel stLabel;
        private System.Windows.Forms.ToolStripProgressBar stProgressBar;
        private System.Windows.Forms.CheckBox cbThreaded;
        private System.Windows.Forms.CheckBox cbClearDB;
        private System.Windows.Forms.CheckBox cbTimeCodedSubpages;
        private System.Windows.Forms.TextBox tbMergeOnlyPage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbFixClock;
        private System.Windows.Forms.CheckBox cbFixHeader;
        private System.Windows.Forms.TextBox tbHeaderTemplate;
        private System.Windows.Forms.CheckBox cbSubpagesAreSeparateFiles;
        private System.Windows.Forms.NumericUpDown nudLinePadding;
        private System.Windows.Forms.Label lblLinePadding;
        private System.Windows.Forms.TextBox tbClockTemplate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbForceInitClockDigits;
    }
}
