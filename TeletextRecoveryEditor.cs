using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using ExtensionMethods;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.WindowsAPICodePack.Dialogs;
using TeletextSharedResources;
using TeletextRecoveryEditor;

namespace Teletext
{
    public partial class TeletextRecoveryEditor : Form
    {

        private bool borders = true;
        private TeletextSharedResources.TeletextRenderer renderer;
        private TeletextSharedResources.RenderedLayersNova layers;
        public Page formPage;// = new Page()
        private Page undoPage;
        private Service service = new Service();
        private Hashtable recoveries = new Hashtable();

        private Boolean changesMade = false;
        private Boolean redrawThumbnails = false;

        private Int32 boxStartX;
        private Int32 boxStartY;
        private Int32 boxEndX;
        private Int32 boxEndY;
        private Int32 boxStartXChar;
        private Int32 boxStartYChar;
        private Int32 boxEndXChar;
        private Int32 boxEndYChar;
        private Boolean mouseDragging = false;
        private Bitmap snapshot;
        private Int32 selectedBoxEndXChar, selectedBoxEndYChar;
        private String applicationName;

        private String currentBinariesFolder = "";

        private String fontName = "Mullard";

        private String currentlySelectedFile = "";
        private String currentlySelectedThumbnail = "";

        private EnhancedTextAttributes frmEnhanced = new EnhancedTextAttributes();

        private int flashRatePhaseSlow = 0;
        private int flashRatePhaseFast = 0;
        private int flashPhase = 0;

        private string dropType = "";

        private string backdrop = "";

        public TeletextRecoveryEditor()
        {
            InitializeComponent();
            service.RowLength = 42;
            applicationName = Text;

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog folderBrowser = new CommonOpenFileDialog();
            folderBrowser.IsFolderPicker = true;

            CommonFileDialogResult result = folderBrowser.ShowDialog();
            string[] fileList;

            renderer = new TeletextRenderer(borders);
            renderer.DeviceDPI = this.DeviceDpi;

            if (result == CommonFileDialogResult.Ok)
            {
                currentBinariesFolder = folderBrowser.FileName;
                this.Text = applicationName + " : " + currentBinariesFolder;
                recoveredPages.Items.Clear();

                fileList = Directory.GetFiles(folderBrowser.FileName);
                Array.Sort(fileList);



                if (fileList.Length > 0)
                {
                    recoveredPages.Name = fileList[0].Substring(0, fileList[0].LastIndexOf("\\")) + "\\";
                    foreach (string file in fileList)
                    {
                        recoveredPages.Items.Add(file.Substring(file.LastIndexOf("\\") + 1));
                    }

                    dropType = "folder";

                    lvThumbnails.Clear();
                    ilThumbnails.Images.Clear();
                    recoveries.Clear();

                    EnableDisableMenus();
                }
                else
                    MessageBox.Show("Folder was empty, please choose another.", "Teletext Recovery Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private System.Drawing.Bitmap CreatePlaceholderBitmap()
        {
            // Create a simple placeholder bitmap for missing resources
            var bitmap = new System.Drawing.Bitmap(32, 32);
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                graphics.FillRectangle(System.Drawing.Brushes.LightGray, 0, 0, 32, 32);
                graphics.DrawRectangle(System.Drawing.Pens.Black, 0, 0, 31, 31);
            }
            return bitmap;
        }

        private System.Drawing.Bitmap LoadBitmapResource(string filename)
        {
            try
            {
                string resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", filename);
                if (File.Exists(resourcePath))
                {
                    return new System.Drawing.Bitmap(resourcePath);
                }
                else
                {
                    // Fallback to placeholder if file not found
                    return CreatePlaceholderBitmap();
                }
            }
            catch
            {
                // Return placeholder on any error
                return CreatePlaceholderBitmap();
            }
        }

        private void EnableDisableMenus()
        {
            if (dropType == "folder")
            {
                this.saveT42ServiceToolStripMenuItem.Enabled = false;
                this.carouselToolStripMenuItem.Enabled = true;
                this.combineCarouselsToT42ToolStripMenuItem.Enabled = true;
                this.recoveredPagesContextMenuStrip.Enabled = true;
                this.subPagesContextMenuStrip.Enabled = true;
                this.pageToolStripMenuItem.Enabled = true;
                this.exportToolStripMenuItem.Enabled = false;
                this.deleteFileToolStripMenuItem.Enabled = true;
                this.exportSubtitlesToolStripMenuItem.Enabled = false;
            }
            else
            {
                this.saveT42ServiceToolStripMenuItem.Enabled = true;
                this.carouselToolStripMenuItem.Enabled = false;
                this.combineCarouselsToT42ToolStripMenuItem.Enabled = false;
                this.recoveredPagesContextMenuStrip.Enabled = true;
                this.subPagesContextMenuStrip.Enabled = false;
                this.pageToolStripMenuItem.Enabled = true;
                this.exportToolStripMenuItem.Enabled = true;
                this.deleteFileToolStripMenuItem.Enabled = false;
                this.exportSubtitlesToolStripMenuItem.Enabled = true;
            }
        }

        private void recoveredPages_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if anything has been altered before we move
            DialogResult drResult = DialogResult.No;

            if (changesMade && currentlySelectedFile != recoveredPages.SelectedItem.ToString())
            {
                drResult = MessageBox.Show("Changes have been made to the current carousel; would you like to save before you move?", "Teletext Editor", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            }

            if (recoveredPages.SelectedItem != null)
            {
                if (currentlySelectedFile == recoveredPages.SelectedItem.ToString())
                {
                    if (formPage != null && currentlySelectedThumbnail != "")
                    {
                        //recoveries[Convert.ToInt32(currentlySelectedThumbnail)] = formPage;
                        refreshThumbnails();
                    }
                }
            }


            if (dropType == "file")
            {
                // Don't save and load new page
                if (drResult == DialogResult.No && recoveredPages.SelectedItem != null && currentlySelectedFile != recoveredPages.SelectedItem.ToString())
                {
                    //string fileName = recoveredPages.SelectedItem.ToString();
                    currentlySelectedFile = recoveredPages.SelectedItem.ToString();

                    if (currentlySelectedFile == "Magazine 8")
                    {
                        currentlySelectedFile = "8xx";
                    }

                    // Draw thumnails for the sub-pages view
                    RenderThumbnails(Convert.ToInt32(currentlySelectedFile.Substring(0, 1)), currentlySelectedFile.Substring(1, 2));
                    changesMade = false;
                }

                //Save and load the pages currently in the list view back to the service
                if (drResult == DialogResult.Yes && recoveredPages.SelectedItem != null)
                {
                    if (changesMade)
                    {
                        saveCarousel();
                        service.SaveService(service.Filename);
                    }

                    currentlySelectedFile = recoveredPages.SelectedItem.ToString();

                    // Draw thumnails for the sub-pages view
                    RenderThumbnails(Convert.ToInt32(currentlySelectedFile.Substring(0, 1)), currentlySelectedFile.Substring(1));
                    changesMade = false;

                }

            }
            else
            {
                FileInfo f;
                if (recoveredPages.SelectedItem != null)
                {
                    f = new FileInfo(recoveredPages.Name + recoveredPages.SelectedItem.ToString());
                    if (f.Length < 400000)
                    {


                        // Daon't save and load new page
                        if (drResult == DialogResult.No && recoveredPages.SelectedItem != null && currentlySelectedFile != recoveredPages.SelectedItem.ToString())
                        {
                            //string fileName = recoveredPages.SelectedItem.ToString();
                            currentlySelectedFile = recoveredPages.SelectedItem.ToString();

                            // Draw thumnails for the sub-pages view
                            RenderThumbnails(currentlySelectedFile);
                            changesMade = false;
                        }

                        //Save and load new page
                        if (drResult == DialogResult.Yes && recoveredPages.SelectedItem != null)
                        {
                            if (changesMade)
                                saveCarousel(recoveredPages.Name + currentlySelectedFile, recoveries);

                            currentlySelectedFile = recoveredPages.SelectedItem.ToString();

                            // Draw thumnails for the sub-pages view
                            RenderThumbnails(currentlySelectedFile);
                            changesMade = false;

                        }
                    }
                    else
                    {
                        MessageBox.Show("File is too large - use only individual carousels and not entire service files.", "Teletext Recovery Editor", MessageBoxButtons.OK);
                    }
                }
            }

            if (lvThumbnails.Items.Count > 0)
            {
                lvThumbnails.Items[dropType == "folder" ? 0 : 1].Selected = true;
                lvThumbnails_SelectedIndexChanged(null, null);
            }
        }

        private void lvThumbnails_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewItem selectedItem = null;

            if (lvThumbnails.SelectedItems.Count > 0)
            {
                selectedItem = lvThumbnails.SelectedItems[0];

                // CHeck if the navigation images have been clicked
                if (selectedItem.Text == "Back" || selectedItem.Text == "Forward")
                {
                    //long position = Convert.ToInt64(selectedItem.Name.Substring(4));

                    // Work out the position of the first page in the list
                    Page firstPage = (Page)recoveries[0];
                    long position = firstPage.Lines[0].StartPos;

                    int mag = Convert.ToInt32(selectedItem.Name.Substring(0, 1));
                    string pageNum = Convert.ToString(selectedItem.Name.Substring(1, 2));

                    if (selectedItem.Text == "Back")
                    {
                        if (changesMade) saveCarousel();

                        ListViewItem lastImage = lvThumbnails.Items[lvThumbnails.Items.Count - 1];
                        //find position held in the 
                        long previousPos = position - (service.Position - position); // Convert.ToInt64(lastImage.ImageKey);
                        service.Position = previousPos >= 0 ? previousPos : 0;

                        //save service bytes to the user's temp folder
                        //service.SaveService(Environment.GetEnvironmentVariable("temp") + "\\teletext\\service.t42");

                        if (recoveredPages.SelectedItem.ToString().Contains("Magazine"))
                            pageNum = "xx";
                        RenderThumbnails(mag, pageNum);
                    }
                    if (lvThumbnails.SelectedItems.Count > 0)
                    {
                        if (selectedItem.Text == "Forward")
                        {
                            if (changesMade) saveCarousel();

                            //save service bytes to the user's temp folder
                            //service.SaveService(Environment.GetEnvironmentVariable("temp") + "\\teletext\\service.bin");

                            if (recoveredPages.SelectedItem.ToString().Contains("Magazine"))
                                pageNum = "xx";
                            RenderThumbnails(mag, pageNum);
                        }
                    }
                }
                else
                {
                    if (selectedItem != null)
                        if (selectedItem.Text != "Back" && selectedItem.Text != "Forward")
                        {
                            currentlySelectedThumbnail = selectedItem.ImageKey;
                            renderer.NationalOptionSelectionBits = "";
                            RenderItemToCharMap(selectedItem.ImageKey);
                            tbPosition.Text = Convert.ToString(formPage.Lines[0].StartPos, 16).ToUpper();
                        }
                }
            }
            gbPosition.Enabled = true;
            gbTimeCode.Enabled = true;
            gbPage.Enabled = true;
            gbCursorHex.Enabled = true;
            gbControlFlags.Enabled = true;
            gbDynamics.Enabled = true;
        }



        private void RenderThumbnails(string f)
        {
            // This function renders a single carousel 

            ilThumbnails.Images.Clear();
            lvThumbnails.Items.Clear();
            recoveries.Clear();
            ilThumbnails.Images.Clear();

            // Load file using Service object
            String status = service.OpenService(recoveredPages.Name + f);

            if (status == "")
            {
                //tag listview with name of file
                lvThumbnails.Tag = f;

                // Get all subpages from the recovery file
                int key = 0;
                while (service.Revolutions == 0)
                {

                    // Get page from Service object
                    formPage = service.GetPage();
                    System.Diagnostics.Debug.WriteLine("Page service posn: " + (service.Position - 42));

                    service.Position = service.Position - service.PacketSize;

                    formPage.Lines[0].Text = "   P" + formPage.Lines[0].Magazine + formPage.Lines[0].Page + " " + formPage.Lines[0].Text;

                    // Render image to thumbnail
                    TeletextRenderer rendererNoBorders = new TeletextRenderer(false);
                    rendererNoBorders.NationalOptionSelectionBits = renderer.NationalOptionSelectionBits;
                    rendererNoBorders.PresentationLevel = renderer.PresentationLevel;
                    rendererNoBorders.Font = renderer.Font;
                    rendererNoBorders.RevealPressed = renderer.RevealPressed;
                    rendererNoBorders.DeviceDPI = this.DeviceDpi;
                    layers = rendererNoBorders.Render(formPage, true, true);

                    int tWidth = lvThumbnails.Width - 50;
                    if (tWidth > 256)
                        tWidth = 256;
                    int tHeight = Convert.ToInt32(tWidth * 0.9);


                    //Bitmap thumbnail = new Bitmap(layers.Background, new Size(tWidth, tHeight));
                    Bitmap thumbnail = new Bitmap(layers.Background);
                    Graphics g = Graphics.FromImage(thumbnail);
                    g.FillRectangle(new SolidBrush(Color.Black), 0, 0, thumbnail.Width, thumbnail.Height);

                    g.DrawImage(layers.Background, 0, 0);
                    g.DrawImage(layers.Foreground, 0, 0);

                    ilThumbnails.ImageSize = new Size(tWidth, tHeight);

                    ilThumbnails.Images.Add(key.ToString(), thumbnail);
                    ListViewItem lvItem = lvThumbnails.Items.Add("Key: " + key.ToString());
                    lvItem.Name = key.ToString();
                    lvItem.ImageKey = key.ToString();

                    // Add Page object to hashtable for this recovery file
                    recoveries[key] = formPage;

                    g.Dispose();
                    key++;

                }

                lvThumbnails.Invalidate();
                lvThumbnails.Refresh();
            }
            else
            {
                if (status != "File is blank.")
                    MessageBox.Show(status, "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderThumbnails(int Mag, string PageNum)
        {
            // This renders thumbnails for a T42 service file

            string origMagPage = Mag.ToString() + PageNum;

            //ilThumbnails.Images.Clear();
            //lvThumbnails.Items.Clear();
            recoveries.Clear();
            //lvThumbnails.Items.Add("Back", origMagPage + "-" + service.Position.ToString());
            //ilThumbnails.Images.Add(origMagPage + "-" + service.Position.ToString(), Resources.back2);

            // Get all subpages from the recovery file
            string magPage = "";
            int key = 0;
            service.Revolutions = 0;
            while (service.Revolutions == 0 && key < 16)
            {
                magPage = origMagPage;

                // if fetching a magazine rather than a page, get the next page in the magazine
                if (PageNum == "xx")
                {
                    Line l = service.GetNextHeader(Mag);
                    magPage = l.MagPage;

                    System.Diagnostics.Debug.WriteLine("Next header " + Convert.ToString(Mag, 16) + " start: " + Convert.ToString(l.StartPos, 16));

                    service.Position = l.StartPos;

                }

                //service.Position = service.Position - service.PacketSize;

                // Get page from Service object
                formPage = service.GetPage(magPage);
                //System.Diagnostics.Debug.WriteLine("Service posn: " + Convert.ToString(service.Position , 16));
                System.Diagnostics.Debug.WriteLine("Page header start posn: " + Convert.ToString(formPage.Lines[0].StartPos, 16));
                //System.Diagnostics.Debug.WriteLine(formPage.Lines[0].Text + "\r\n");


                formPage.Lines[0].Text = "   P" + formPage.Lines[0].Magazine + formPage.Lines[0].Page + " " + formPage.Lines[0].Text;

                // Is there anything in this page apart from a header?
                bool anyContent = false;
                foreach (Line l in formPage.Lines)
                {
                    if (l.Type == LineTypes.Line)
                        anyContent = true;
                }

                if (anyContent)
                {
                    // Render image to thumbnail
                    //TeletextRenderer rendererNoBorders = new TeletextRenderer(false);
                    //rendererNoBorders.NationalOptionSelectionBits = renderer.NationalOptionSelectionBits;
                    //rendererNoBorders.PresentationLevel = renderer.PresentationLevel;
                    //rendererNoBorders.Font = renderer.Font;
                    //rendererNoBorders.RevealPressed = renderer.RevealPressed;
                    //layers = rendererNoBorders.Render(formPage);

                    //int tWidth = lvThumbnails.Width - 50;
                    //if (tWidth > 256)
                    //    tWidth = 256;
                    //int tHeight = Convert.ToInt32(tWidth * 0.9);


                    ////Bitmap thumbnail = new Bitmap(layers.Background, new Size(tWidth, tHeight));
                    //Bitmap thumbnail = new Bitmap(layers.Background);
                    //Graphics g = Graphics.FromImage(thumbnail);
                    //g.FillRectangle(new SolidBrush(Color.Black), 0, 0, thumbnail.Width, thumbnail.Height);

                    //g.DrawImage(layers.Background, 0, 0);
                    //g.DrawImage(layers.Foreground, 0, 0);

                    //ilThumbnails.ImageSize = new Size(tWidth, tHeight);

                    //ilThumbnails.Images.Add(key.ToString(), thumbnail);
                    //ListViewItem lvItem = lvThumbnails.Items.Add("Key: " + key.ToString());
                    //lvItem.Name = key.ToString();
                    //lvItem.ImageKey = key.ToString();

                    //g.Dispose();

                    recoveries[key] = formPage;
                    key++;

                }
                else
                //  service.Position = service.Position + service.PacketSize;
                {
                    //Line nextLine = service.GetNextLine();
                    //    if (nextLine.Type != LineTypes.Header)
                    //        service.Position = service.Position - service.PacketSize;
                }

            }

            //lvThumbnails.Items.Add("Forward", magPage + "-" + service.Position.ToString());
            //ilThumbnails.Images.Add(magPage + "-" + service.Position.ToString(), Resources.forward2);

            //lvThumbnails.Invalidate();
            //lvThumbnails.Refresh();

            refreshThumbnails();

        }

        private void RenderItemToCharMap(String strImageKey = "")
        {
            charMap.DeallocateCursor();

            if (charMap.BackgroundImage != null)
                charMap.BackgroundImage.Dispose();
            if (charMap.Image != null)
                charMap.Image.Dispose();


            // Render page in actual size.  If we have an imagekey, draw that, if not, redraw the current page
            if (strImageKey != "")
                formPage = (Page)recoveries[Convert.ToInt32(strImageKey)];

            // Suppress the header if appropriate
            //if (formPage.Lines[0].Flags.C7_SuppressHeader)
            //    formPage.Lines[0].Text = "";

            layers = renderer.Render(formPage, !cbSuppress.Checked, !cbSuppress.Checked);

            //bool zeroBlack = renderer.ZeroBlack;
            //charMap.Width = borders ? 666 : Convert.ToInt32(480 * 1.12);
            //charMap.Height = borders ? 500 : 500;

            if (backdrop != "")
            {
                Image bgImage = new Bitmap(backdrop);
                charMap.BackgroundImage = new Bitmap(bgImage, borders ? 700 : 480, borders ? 600 : 500);
            }
            else
            {
                charMap.BackgroundImage = new Bitmap(borders ? 700 : 480, borders ? 600 : 500, PixelFormat.Format32bppArgb);
            }

            charMap.Image = new Bitmap(borders ? 700 : 480, borders ? 600 : 500, PixelFormat.Format32bppArgb);
            RenderCharMap();

            // Decode Fastext
            // Find fastext packet
            byte fastextPkt = 255;
            fastextPkt = formPage.GetPacketIndex(27);

            if (formPage.Lines[fastextPkt].Bytes != null)
            {
                FastextState fastext = new FastextState();
                fastext.Decode(formPage.Lines[fastextPkt].Bytes, formPage.Lines[fastextPkt].Magazine);
                tbRed.Text = fastext.Red.Page;
                tbGreen.Text = fastext.Green.Page;
                tbYellow.Text = fastext.Yellow.Page;
                tbBlue.Text = fastext.Blue.Page;
                tbPurple.Text = fastext.Four.Page;
                tbIndex.Text = fastext.Five.Page;
                //tbStatus.Text = " Red: " + fastext.Red.Page + " Green: " + fastext.Green.Page + " Yellow: " + fastext.Yellow.Page + " Blue: " + fastext.Blue.Page + " Four: " + fastext.Four.Page + " Five: " + fastext.Five.Page + " RedMM: " + fastext.Red.MagModifier + " GreenMM: " + fastext.Green.MagModifier + " YellowMM: " + fastext.Yellow.MagModifier + " BlueMM: " + fastext.Blue.MagModifier;
            }


            // Start the cursor
            charMap.Borders = borders;
            charMap.InitCursor();

            //Set flag displays
            rbC4.Checked = formPage.Lines[0].Flags.C4_Erase;
            rbC5.Checked = formPage.Lines[0].Flags.C5_Newsflash;
            rbC6.Checked = formPage.Lines[0].Flags.C6_Subtitle;
            rbC7.Checked = formPage.Lines[0].Flags.C7_SuppressHeader;
            rbC8.Checked = formPage.Lines[0].Flags.C8_Update;
            rbC9.Checked = formPage.Lines[0].Flags.C9_InterruptedSequence;
            rbC10.Checked = formPage.Lines[0].Flags.C10_InhibitDisplay;
            rbC11.Checked = formPage.Lines[0].Flags.C11_MagazineSerial;
            rbC12.Checked = formPage.Lines[0].Flags.C12;
            rbC13.Checked = formPage.Lines[0].Flags.C13;
            rbC14.Checked = formPage.Lines[0].Flags.C14;
            tbTimeCode.Text = formPage.Lines[0].TimeCode;

            tbPage.Text = (formPage.Lines[0].Magazine == 0 ? "8" : formPage.Lines[0].Magazine.ToString()) + formPage.Lines[0].Page;

            // Get hex value of character under the cursor
            Byte bytPktPosnInArray = formPage.GetPacketIndex(charMap.cursorY);
            UpdateHexUnderCursor(bytPktPosnInArray);

            ResolveCursorType(bytPktPosnInArray);
        }

        private void pNG11TransparentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            charMap.DeallocateCursor();

            // Derive file name base name
            String strBaseNameNoBinaries = currentBinariesFolder.Replace("\\binaries", "");
            String strBaseName = strBaseNameNoBinaries.Substring(strBaseNameNoBinaries.LastIndexOf(Convert.ToChar("\\")) + 1);

            if (recoveredPages.SelectedItems.Count > 0)
            {
                String pageName = recoveredPages.SelectedItem.ToString();
                ListView.SelectedListViewItemCollection lvSelectedThumbnail = lvThumbnails.SelectedItems;

                if (lvSelectedThumbnail.Count > 0)
                {
                    Int32 intThumbnailIndex = lvSelectedThumbnail[0].Index;

                    saveFileDialog1.FileName = strBaseName + "_" + pageName + "_" + intThumbnailIndex.ToString() + ".png";
                    saveFileDialog1.ShowDialog();


                    if (saveFileDialog1.FileName != "")
                    {
                        int x = (int)((borders ? 700 : 480) * DeviceDpi / 96);
                        int y = (int)((borders ? 600 : 500) * DeviceDpi / 96);
                        Rectangle rect = new Rectangle(0, 0, x, y);
                        Bitmap bmpExport = new Bitmap(x, y);
                        charMap.DrawToBitmap(bmpExport, rect);

                        // Change black background to transparent
                        bmpExport.MakeTransparent(Color.Black);                        
                        bmpExport.Save(saveFileDialog1.FileName);
                    }
                }
                else
                    MessageBox.Show("Select a thumbnail first.", "Teletext Stream Editor");
            }
            else
                MessageBox.Show("Select a page first.", "Teletext Stream Editor");

            charMap.InitCursor();
        }

        private void windowsBitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            charMap.DeallocateCursor();

            // Derive file name base name
            String strBaseNameNoBinaries = currentBinariesFolder.Replace("\\binaries", "");
            String strBaseName = strBaseNameNoBinaries.Substring(strBaseNameNoBinaries.LastIndexOf(Convert.ToChar("\\")) + 1);

            if (recoveredPages.SelectedItems.Count > 0)
            {
                String pageName = recoveredPages.SelectedItem.ToString();
                ListView.SelectedListViewItemCollection lvSelectedThumbnail = lvThumbnails.SelectedItems;

                if (lvSelectedThumbnail.Count > 0)
                {
                    Int32 intThumbnailIndex = lvSelectedThumbnail[0].Index;

                    saveFileDialog1.FileName = strBaseName + "_" + pageName + "_" + intThumbnailIndex.ToString() + ".png";
                    saveFileDialog1.ShowDialog();

                    Rectangle rect = new Rectangle(0, 0, charMap.Size.Width, charMap.Size.Height);
                    Bitmap bmpExport = new Bitmap(charMap.Size.Width, charMap.Size.Height);
                    charMap.DrawToBitmap(bmpExport, rect);
                    if (saveFileDialog1.FileName != "")
                        bmpExport.Save(saveFileDialog1.FileName);
                }
                else
                    MessageBox.Show("Select a thumbnail first.", "Teletext Stream Editor");
            }
            else
                MessageBox.Show("Select a page first.", "Teletext Stream Editor");

            charMap.InitCursor();
        }


        private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            charMap.DeallocateCursor();



            // Derive file name base name
            String strBaseNameNoBinaries = currentBinariesFolder.Replace("\\binaries", "");
            String strBaseName = strBaseNameNoBinaries.Substring(strBaseNameNoBinaries.LastIndexOf(Convert.ToChar("\\")) + 1);

            // Get destination folder
            CommonOpenFileDialog folderBrowser = new CommonOpenFileDialog();
            folderBrowser.IsFolderPicker = true;
            folderBrowser.ShowDialog();

            Int32 intSelectedItem = -1;

            if (recoveredPages.SelectedItems.Count > 0 && folderBrowser.FileName != "")
            {
                String pageName = recoveredPages.SelectedItem.ToString();
                ListView.ListViewItemCollection lvAllThumbnails = lvThumbnails.Items;



                foreach (ListViewItem lvItem in lvAllThumbnails)
                {

                    String strThumbnailIndex = lvItem.Index.ToString();

                    // Save the index of the selected item so we can put it back later
                    if (lvItem.Selected)
                        intSelectedItem = lvItem.Index;

                    //Open item on charmap for export
                    RenderItemToCharMap(strThumbnailIndex);

                    String itemFilename = folderBrowser.FileName + "\\" + strBaseName + "_" + pageName + "_" + strThumbnailIndex + ".png";

                    int x = (int)((borders ? 700 : 480) * DeviceDpi / 96);
                    int y = (int)((borders ? 600 : 500) * DeviceDpi / 96);
                    Rectangle rect = new Rectangle(0, 0, x, y);
                    Bitmap bmpExport = new Bitmap(x, y);

                    charMap.DrawToBitmap(bmpExport, rect);

                    try
                    {
                        bmpExport.Save(itemFilename);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Teletext Recovery Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
            else
            {
                if (folderBrowser.Controls.Count() > 0)
                    MessageBox.Show("Select a page first.", "Teletext Stream Editor");
                else
                    MessageBox.Show("Operation cancelled.", "Teletext Stream Editor");
            }

            if (intSelectedItem != -1)
                RenderItemToCharMap(intSelectedItem.ToString());

            //charMap.InitCursor();
        }
        private void pNG11TransparentToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            charMap.DeallocateCursor();



            // Derive file name base name
            String strBaseNameNoBinaries = currentBinariesFolder.Replace("\\binaries", "");
            String strBaseName = strBaseNameNoBinaries.Substring(strBaseNameNoBinaries.LastIndexOf(Convert.ToChar("\\")) + 1);

            // Get destination folder
            CommonOpenFileDialog folderBrowser = new CommonOpenFileDialog();
            folderBrowser.IsFolderPicker = true;
            folderBrowser.ShowDialog();

            Int32 intSelectedItem = -1;

            if (folderBrowser.FileName != "")
            {
                String pageName = recoveredPages.SelectedItem.ToString();
                ListView.ListViewItemCollection lvAllThumbnails = lvThumbnails.Items;



                foreach (ListViewItem lvItem in lvAllThumbnails)
                {

                    String strThumbnailIndex = lvItem.Index.ToString();

                    // Save the index of the selected item so we can put it back later
                    if (lvItem.Selected)
                        intSelectedItem = lvItem.Index;

                    //Open item on charmap for export
                    RenderItemToCharMap(strThumbnailIndex);

                    String itemFilename = folderBrowser.FileName + "\\" + strBaseName + "_" + pageName + "_" + strThumbnailIndex + ".png";

                    int x = (int)((borders ? 700 : 480) * DeviceDpi / 96);
                    int y = (int)((borders ? 600 : 500) * DeviceDpi / 96);
                    Rectangle rect = new Rectangle(0, 0, x, y);
                    Bitmap bmpExport = new Bitmap(x, y);
                    charMap.DrawToBitmap(bmpExport, rect);

                    // Change black background to transparent
                    bmpExport.MakeTransparent(Color.Black);

                    try
                    {
                        bmpExport.Save(itemFilename);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Teletext Recovery Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
            else
            {
                if (folderBrowser.Controls.Count() > 0)
                    MessageBox.Show("Select a page first.", "Teletext Stream Editor");
                else
                    MessageBox.Show("Operation cancelled.", "Teletext Stream Editor");
            }

            if (intSelectedItem != -1)
                RenderItemToCharMap(intSelectedItem.ToString());

            //charMap.InitCursor();

        }

        private void charMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (formPage != null)
            {

                byte[] canvasXY = ConvertMousePosToCharMapXY(e.X, e.Y);

                charMap.cursorX = canvasXY[0];
                charMap.cursorY = canvasXY[1];

                Byte bytPktPosnInArray = formPage.GetPacketIndex(charMap.cursorY);
                //Line lineEdited = new Line();

                //for (Int32 i = 0; i < 256 && formPage.Lines[i].Type == ""; i++)
                //{
                //    Line l = formPage.Lines[i];
                //    if (l.Row == charMap.cursorY)
                //    {
                //        //lineEdited = l;
                //        bytPktPosnInArray = (Byte)i;
                //    }
                //}

                //Check the line above for a double height code - if so, nove cursor up again
                for (Int32 i = 0; i < 256; i++)
                {
                    Line l = formPage.Lines[i];
                    if (l.Row == charMap.cursorY - 1)
                    {
                        if (formPage.Lines[i].Text != null)
                        {
                            if (formPage.Lines[i].Text.Contains((char)0x0d) && renderer.PresentationLevel >= 1)
                            {
                                charMap.cursorY--;
                                for (Int32 j = 0; j < 256; j++)
                                {
                                    Line m = formPage.Lines[j];
                                    if (m.Row == charMap.cursorY)
                                    {
                                        //lineEdited = m;
                                        bytPktPosnInArray = (Byte)j;
                                    }
                                }
                            }
                        }
                    }
                }

                ResolveCursorType(bytPktPosnInArray);

                charMap.MoveCursor();

                // Get hex value of character under the cursor
                UpdateHexUnderCursor();
            }
        }

        private byte[] ConvertMousePosToCharMapXY(int MouseXPos, int MouseYPos)
        {
            byte[] returnArray = new byte[2];

            //convert this to a character on the char mapped bitmap
            Double xStretchRatio = (borders ? 700D : 480D) / charMap.Size.Width;
            Double yStretchRatio = (borders ? 600D : 500D) / charMap.Size.Height;

            double mousePxOnCanvasX = (double)MouseXPos - (charMap.l1CanvasXOrigin / xStretchRatio);
            mousePxOnCanvasX = mousePxOnCanvasX < 0 ? 0 : mousePxOnCanvasX > (40D * 12D) / xStretchRatio ? (40D * 12D) / xStretchRatio : mousePxOnCanvasX;
            returnArray[0] = (Byte)((mousePxOnCanvasX / (40D * 12D / xStretchRatio) * 40D) > 39 ? 39 : (mousePxOnCanvasX / (40D * 12D / xStretchRatio) * 40D));


            double mousePxOnCanvasY = (double)MouseYPos - (charMap.l1CanvasYOrigin / yStretchRatio);
            mousePxOnCanvasY = mousePxOnCanvasY < 0 ? 0 : mousePxOnCanvasY > (25D * 20D) / yStretchRatio ? (25D * 20D) / yStretchRatio : mousePxOnCanvasY;
            returnArray[1] = (Byte)((mousePxOnCanvasY / (25D * 20D / yStretchRatio) * 25D) > 24 ? 24 : (mousePxOnCanvasY / (25D * 20D / yStretchRatio) * 25D));

            return returnArray;
        }



        private Byte createPacket(Byte packetNo, Byte rowNo, String pktText)
        {
            Boolean needANewPacket = false;
            Byte bytPktPosnInArray = 0;

            // Is the packet free?
            if (formPage.Lines[packetNo].Type != LineTypes.Blank)
                needANewPacket = true;
            else
                bytPktPosnInArray = packetNo;


            // Find next empty array element
            if (needANewPacket)
            {
                do
                {
                    bytPktPosnInArray++;
                }
                while (formPage.Lines[bytPktPosnInArray].Type != LineTypes.Blank && bytPktPosnInArray <= 255);
            }

            if (bytPktPosnInArray != 255)
            {
                Line newline = service.CreatePacket(formPage.Lines[0].Magazine, formPage.Lines[0].Page, "00:00", packetNo, rowNo, pktText);
                formPage.Lines[bytPktPosnInArray] = newline;
            }
            return bytPktPosnInArray;
        }



        private Int32 KeyCodeToASCII(String code, Int32 value, String modifiers)
        {
            Int32 output = -1;

            //Space
            if (value == 0x20)
                output = 0x20;

            //Output alphabetic characters as upper case if shift pressed
            if (modifiers == "Shift" && value > 0x40 && value < 0x80 && !code.Contains("NumPad"))
                output = value;

            //Convert alphabetic characters to lower case if no modifiers pressed
            if (modifiers == "None" && value > 0x40 && value < 0x80 && !code.Contains("NumPad"))
                output = value | 0x20;

            // Main keyboard numerics
            if (modifiers == "None" && code.Length == 2 && code.Substring(0, 1) == "D")
                output = value;

            // If shift and main keyboard numerics, convert to shifted symbols
            if (modifiers == "Shift" && value >= 0x30 && value <= 0x39)
            {
                changesMade = true; redrawThumbnails = true;
                switch (value)
                {
                    case 0x31:
                        output = 0x21;
                        break;
                    case 0x32:
                        output = 0x22;
                        break;
                    case 0x33:
                        output = 0x23;
                        break;
                    case 0x34:
                        output = 0x24;
                        break;
                    case 0x35:
                        output = 0x25;
                        break;
                    case 0x36:
                        output = 0x7c;
                        break;
                    case 0x37:
                        output = 0x26;
                        break;
                    case 0x38:
                        output = 0x2a;
                        break;
                    case 0x39:
                        output = 0x28;
                        break;
                    case 0x30:
                        output = 0x29;
                        break;

                }

            }

            // Special characters
            if (modifiers == "None" && value > 0x7f)
            {
                changesMade = true; redrawThumbnails = true;
                switch (value)
                {
                    case 0xBD:
                        output = 0x2d;
                        break;

                    case 0xBB:
                        output = 0x3D;
                        break;

                    case 0xBC:
                        // ,
                        output = 0x2c;
                        break;

                    case 0xBE:
                        // .
                        output = 0x2e;
                        break;

                    case 0xc0:
                        // '
                        output = 0x27;
                        break;

                    case 0xba:
                        // ;
                        output = 0x3b;
                        break;

                    case 0xbf:
                        // /
                        output = 0x2f;
                        break;

                    case 0xde:
                        // #
                        output = 0x5f;
                        break;

                    case 0xdb:
                        // [
                        output = 0x7b; // 1/4
                        break;

                    case 0xdd:
                        // ]
                        output = 0x5c; // 1/2
                        break;

                }
            }

            // Shifted special characters
            if (modifiers == "Shift" && value > 0x7f)
            {
                changesMade = true; redrawThumbnails = true;
                switch (value)
                {
                    case 0xBD:
                        output = 0x60;
                        break;

                    case 0xBB:
                        output = 0x2B;
                        break;

                    case 0xBC:
                        // <
                        output = 0x3c;
                        break;

                    case 0xBE:
                        // >
                        output = 0x3e;
                        break;

                    case 0xc0:
                        // @
                        output = 0x40;
                        break;

                    case 0xba:
                        // :
                        output = 0x3a;
                        break;

                    case 0xbf:
                        // ?
                        output = 0x3f;
                        break;

                    case 0xdb:
                        // [
                        output = 0x7e; // ./.
                        break;

                    case 0xdd:
                        // ]
                        output = 0x7d; // 3/4
                        break;
                }
            }

            // Directional arrows
            if (modifiers == "Control" && code.Contains("NumPad"))
            {
                switch (code.ToString())
                {
                    case "NumPad4":
                        // left arrow
                        output = 0x5b;
                        break;

                    case "NumPad8":
                        // up arrow
                        output = 0x5e;
                        break;

                    case "NumPad6":
                        // right arrow
                        output = 0x5d;
                        break;

                    case "NumPad2":
                        // down arrow
                        //output = 0x5b;
                        break;
                }
            }

            //Output numpad numbers
            if (modifiers == "None" && code.Contains("NumPad") && !charMap.GraphicsCursor)
            {
                changesMade = true; redrawThumbnails = true;
                switch (code.ToString())
                {
                    case "NumPad0":
                        // 0
                        output = 0x30;
                        break;

                    case "NumPad1":
                        // 1
                        output = 0x31;
                        break;

                    case "NumPad2":
                        // 2
                        output = 0x32;
                        break;

                    case "NumPad3":
                        // 3
                        output = 0x33;
                        break;

                    case "NumPad4":
                        // 4
                        output = 0x34;
                        break;

                    case "NumPad5":
                        // 5
                        output = 0x35;
                        break;

                    case "NumPad6":
                        // 6
                        output = 0x36;
                        break;

                    case "NumPad7":
                        // 7
                        value = 0x37;
                        break;

                    case "NumPad8":
                        // 8
                        output = 0x38;
                        break;

                    case "NumPad9":
                        // 9
                        output = 0x39;
                        break;
                }
            }

            // set/unset pixels in the current character using the numpad
            if (modifiers == "None" && code.Contains("NumPad") && charMap.GraphicsCursor)
            {
                changesMade = true; redrawThumbnails = true;
                switch (code.ToString())
                {
                    case "NumPad0":
                        // left arrow
                        //output = 0x30;
                        break;

                    case "NumPad1":
                        // set/unset bottom left pixel
                        output = Convert.ToInt32(tbCursorHex.Text.Substring(2, 2), 16) ^ 16;
                        break;

                    case "NumPad2":
                        // bottom right
                        output = Convert.ToInt32(tbCursorHex.Text.Substring(2, 2), 16) ^ 64;
                        break;

                    case "NumPad3":
                        // down arrow
                        //output = Convert.ToInt32(tbCursorHex.Text.Substring(2, 2), 16) ^ 16; 
                        break;

                    case "NumPad4":
                        // middle left
                        output = Convert.ToInt32(tbCursorHex.Text.Substring(2, 2), 16) ^ 4;
                        break;

                    case "NumPad5":
                        // middle right
                        output = Convert.ToInt32(tbCursorHex.Text.Substring(2, 2), 16) ^ 8;
                        break;

                    case "NumPad6":
                        // right arrow
                        //output = 0x36;
                        break;

                    case "NumPad7":
                        // down arrow
                        output = Convert.ToInt32(tbCursorHex.Text.Substring(2, 2), 16) ^ 1;
                        break;

                    case "NumPad8":
                        // right arrow
                        output = Convert.ToInt32(tbCursorHex.Text.Substring(2, 2), 16) ^ 2;
                        break;

                    case "NumPad9":
                        // down arrow
                        //output = 0x39;
                        break;
                }

            }

            System.Diagnostics.Debug.Print("ASCII value: 0x" + output.ToString("X") + " Character: " + (char)output);

            return output;
        }


        private void saveCarouselToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveCarousel(recoveredPages.Name + currentlySelectedFile, recoveries);
        }


        private void saveCarouselAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialogCarousel.ShowDialog();

            if (saveFileDialogCarousel.FileName != "")
            {

                saveCarousel(saveFileDialogCarousel.FileName, recoveries);

            }

        }


        private void saveCarousel(String filename, Hashtable carousel, Boolean Append = false)
        {
            FileStream fs = null;
            //try
            //{
            fs = new FileStream(filename, Append ? FileMode.Append : FileMode.Create);
            //}
            //catch
            //{
            //    MessageBox.Show("File error.  Has the folder been renamed or moved?", "Teletext Recovery Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

            if (fs != null)
            {
                BinaryWriter writer = new BinaryWriter(fs);
                Page writePage;

                // Get list of keys in recoveries (in case some keys are missing)
                List<Int32> keys = new List<Int32>();
                foreach (DictionaryEntry p in carousel)
                {
                    keys.Add((Int32)p.Key);
                }

                Byte[] parityBitSetList = { 0, 3, 5, 6, 9, 10, 12, 15, 17, 18, 20, 23, 24, 27, 29, 30, 33, 34, 36, 39, 40, 43, 45, 46, 48, 51, 53, 54, 57, 58, 60, 63, 65, 66, 68, 71, 72, 75, 77, 78, 80, 83, 85, 86, 89, 90, 92, 95, 96, 99, 101, 102, 105, 106, 108, 111, 113, 114, 116, 119, 120, 123, 125, 126 };

                for (Int32 n = keys.Count - 1; n >= 0; n--)
                {
                    writePage = (Page)carousel[keys[n]];

                    if (writePage != null)
                    {
                        for (Int32 i = 0; i < writePage.Lines.Count(); i++)
                        {
                            if (writePage.Lines[i].Type != LineTypes.Blank)
                            {
                                if (writePage.Lines[i].Type == LineTypes.Header)
                                {

                                    writePage.Lines[i].Bytes[0] = writePage.Lines[i].MRAG1;
                                    writePage.Lines[i].Bytes[1] = writePage.Lines[i].MRAG2;
                                    writePage.Lines[i].Bytes[2] = writePage.Lines[i].PU;
                                    writePage.Lines[i].Bytes[3] = writePage.Lines[i].PT;
                                    writePage.Lines[i].Bytes[4] = writePage.Lines[i].MU;
                                    writePage.Lines[i].Bytes[5] = writePage.Lines[i].MT;
                                    writePage.Lines[i].Bytes[6] = writePage.Lines[i].HU;
                                    writePage.Lines[i].Bytes[7] = writePage.Lines[i].HT;
                                    writePage.Lines[i].Bytes[8] = writePage.Lines[i].CA;
                                    writePage.Lines[i].Bytes[9] = writePage.Lines[i].CB;

                                }
                                else
                                {

                                    writePage.Lines[i].Bytes[0] = writePage.Lines[i].MRAG1;
                                    writePage.Lines[i].Bytes[1] = writePage.Lines[i].MRAG2;

                                }

                                writer.Write(writePage.Lines[i].Bytes, 0, service.PacketSize);


                            }
                        }
                    }
                }
                writer.Close();
                writer.Dispose();
                fs.Close();
                fs.Dispose();

                changesMade = false;
            }
        }

        private void saveCarousel()
        {

            //for (Int32 n = keys.Count - 1; n >= 0; n--)
            foreach (Page writePage in recoveries.Values)
            {
                if (writePage != null)
                {
                    if (writePage.Lines[0].Type != LineTypes.Header)
                    {
                        System.Diagnostics.Debug.WriteLine("Header not this first line in page: ");
                    }
                    for (Int32 i = 0; i < writePage.Lines.Count(); i++)
                    {
                        if (writePage.Lines[i].Text != "" && writePage.Lines[i].Type != LineTypes.Blank)
                        {
                            if (writePage.Lines[i].Type == LineTypes.Header)
                            {

                                writePage.Lines[i].Bytes[0] = writePage.Lines[i].MRAG1;
                                writePage.Lines[i].Bytes[1] = writePage.Lines[i].MRAG2;
                                writePage.Lines[i].Bytes[2] = writePage.Lines[i].PU;
                                writePage.Lines[i].Bytes[3] = writePage.Lines[i].PT;
                                writePage.Lines[i].Bytes[4] = writePage.Lines[i].MU;
                                writePage.Lines[i].Bytes[5] = writePage.Lines[i].MT;
                                writePage.Lines[i].Bytes[6] = writePage.Lines[i].HU;
                                writePage.Lines[i].Bytes[7] = writePage.Lines[i].HT;
                                writePage.Lines[i].Bytes[8] = writePage.Lines[i].CA;
                                writePage.Lines[i].Bytes[9] = writePage.Lines[i].CB;

                            }
                            else
                            {
                                writePage.Lines[i].Bytes[0] = writePage.Lines[i].MRAG1;
                                writePage.Lines[i].Bytes[1] = writePage.Lines[i].MRAG2;
                            }

                            if (writePage.Lines[i].StartPos == 0)
                            // this line isn't in the original T42, find space
                            {
                                long origPosition = service.Position;

                                // start from the packet before's location - this must have a value as there must be a header
                                service.Position = writePage.Lines[0].StartPos;
                                service.GetNextHeader();
                                long lastPosition = service.GetNextHeader().StartPos;
                                service.Position = writePage.Lines[0].StartPos;
                                // keep looking until we get to the next page's startPos (if there is one)
                                while (service.Position < lastPosition && writePage.Lines[i].StartPos == 0)
                                {
                                    Line l = service.GetNextLine();
                                    // We can put the new row into a blank line as this won't impact the timing - otherwise we'd have to try and
                                    // identify the rogue MRAG == tricky
                                    if (l.Type == LineTypes.Blank)
                                        writePage.Lines[i].StartPos = l.StartPos;
                                }
                                if (writePage.Lines[i].StartPos == 0)
                                    System.Diagnostics.Debug.WriteLine("Line not inserted: row: " + i + "range: " + (i <= 0 ? 0 : writePage.Lines[i - 1].StartPos) + "-" + writePage.Lines[i + 1].StartPos);
                                else
                                    System.Diagnostics.Debug.WriteLine("Line inserted at: " + writePage.Lines[i].StartPos);


                                service.Position = origPosition;
                            }

                            service.WriteLine(writePage.Lines[i].Bytes, writePage.Lines[i].StartPos);

                        }
                    }
                }
            }
        }



        private void UpdateHexUnderCursor(byte bytPktPosnInArray = 255)
        {
            if (formPage != null)
            {
                //tbCursorHex.Text = "0x" + Convert.ToString(formPage.modeMap[charMap.cursorY, charMap.cursorX].Character, 16);
                if (bytPktPosnInArray == 255)
                    bytPktPosnInArray = formPage.GetPacketIndex(charMap.cursorY);

                if (bytPktPosnInArray != 255)
                {
                    tbCursorHex.Text = GetHex(bytPktPosnInArray, charMap.cursorX);
                    if (tbCursorHex.Text != "--")
                    {
                        TeletextTools tools = new TeletextTools();
                        lblHexParity.Text = "0x" + Convert.ToString(tools.CalcParity(Convert.ToByte(tbCursorHex.Text.Substring(2), 16)), 16).ToUpper().PadLeft(2, Convert.ToChar("0"));
                        lblCharUnderCursor.Text = formPage.modeMap[charMap.cursorY, charMap.cursorX].Character > 0x20 ? Convert.ToString((Char)formPage.modeMap[charMap.cursorY, charMap.cursorX].Character) : " ";
                    }
                }
            }
        }

        private String GetHex(Byte bytPktPosnInArray, int x = -99)
        {
            byte cMapX = charMap.cursorX;

            if (x >= 0) cMapX = Convert.ToByte(x);

            if (cMapX < formPage.Lines[bytPktPosnInArray].Text.Length && cMapX >= 0 && x != -1)
                return (formPage.Lines[bytPktPosnInArray].Text != null && formPage.Lines[bytPktPosnInArray].Text != "") ? "0x" + ((Byte)Convert.ToChar(formPage.Lines[bytPktPosnInArray].Text.Substring(cMapX, 1))).ToString("X2") : "";
            else
                return "--";
        }

        private void btnUpdateHex_Click(object sender, EventArgs e)
        {
            //((Byte)Convert.ToChar(formPage.Lines[charMap.cursorY].Text.Substring(charMap.cursorX, 1))).ToString("X2");

            // get the array element index which holds the packet
            Byte bytPktPosnInArray = formPage.GetPacketIndex(charMap.cursorY);

            String currentText = formPage.Lines[bytPktPosnInArray].Text;
            tbCursorHex.Text = (tbCursorHex.Text.Contains("0x") == false) ? "0x" + tbCursorHex.Text : tbCursorHex.Text;

            if (tbCursorHex.Text.Substring(0, 2) == "0x" && tbCursorHex.Text.Length == 4)
            {
                Boolean hexIsValid = true;
                char newChar = (Char)0x20;
                try
                {
                    newChar = (Char)Convert.ToByte(tbCursorHex.Text.Substring(2), 16);
                }
                catch
                {
                    hexIsValid = false;
                    MessageBox.Show("The supplied hex value is invalid.", "Teletext Recovery Editor", MessageBoxButtons.OK);
                }

                if (hexIsValid)
                {
                    TeletextTools tools = new TeletextTools();
                    //            if (tbCursorHex.Text != "--")
                    lblHexParity.Text = "0x" + Convert.ToString(tools.CalcParity(Convert.ToByte(tbCursorHex.Text.Substring(2), 16)), 16).ToUpper().PadLeft(2, Convert.ToChar("0"));
                    try
                    {
                        formPage.Lines[bytPktPosnInArray].Text = currentText.Substring(0, charMap.cursorX) + newChar + currentText.Substring(charMap.cursorX + 1);
                        formPage.Lines[bytPktPosnInArray].Bytes[charMap.cursorX + 2] = (byte)newChar;

                        RenderItemToCharMap();

                        /*layers = renderer.Render(formPage, true);
                        //charMap.BackgroundImage = layers.Background;
                        //charMap.Image = layers.Foreground;

                        Graphics g = Graphics.FromImage(charMap.BackgroundImage);
                        //g.Clear(Color.FromArgb(0,0,0,0));
                        //g.DrawImage(layers.L25Background, 0, 0);
                        g.DrawImage(layers.Background, 0, 0);
                        g.DrawImage(layers.Foreground, 0, 0);
                        g.Dispose();

                        charMap.Invalidate();
                        charMap.Update();
                        charMap.Refresh();*/

                        charMap.Focus();
                        changesMade = true; redrawThumbnails = true;
                    }
                    catch
                    {
                        //MessageBox.Show("An error occurred whilst updating the hex value", "Teletext Recovery Editor", MessageBoxButtons.OK);
                    }
                }
            }
            else
                MessageBox.Show("The supplied hex value is invalid.", "Teletext Recovery Editor", MessageBoxButtons.OK);

        }



        private Boolean InGraphicsMode(Byte bytPktPosnInArray)
        {
            //Scans the line the cursor is in to see if graphics mode is on or not at the cursor
            //Boolean outcome = false;

            //Int32 limit = charMap.cursorX > formPage.Lines[bytPktPosnInArray].Text.Length - 1 ? formPage.Lines[bytPktPosnInArray].Text.Length - 1 : charMap.cursorX;
            //for (Int32 n = 0; n <= limit; n++)
            //{
            //    Byte col = Convert.ToByte(Convert.ToChar(formPage.Lines[bytPktPosnInArray].Text.Substring(n, 1)));
            //    if (col != 0x00 && col < 0x20)
            //    {
            //        if ((col >= 0x11 && col <= 0x1f) || (col == 0x08))
            //            outcome = true;
            //        else
            //            outcome = false;
            //    }
            //}

            //return outcome;

            if (bytPktPosnInArray < 255)
                return formPage.modeMap[charMap.cursorY, charMap.cursorX].Graphics;
            else
                return false;

        }

        private void ResolveCursorType(Byte bytPktPosnInArray)
        {
            if (formPage.Lines[bytPktPosnInArray].Text != null)
            {
                //Double or single height cursor
                if (formPage.Lines[bytPktPosnInArray].Text.Contains((char)13) && renderer.PresentationLevel >= 1)
                    charMap.CursorHeightMultiplier = 2;
                else
                    charMap.CursorHeightMultiplier = 1;

                //Change cursor type
                charMap.GraphicsCursor = InGraphicsMode(bytPktPosnInArray);
            }
        }

        private void lvThumbnails_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                subPagesContextMenuStrip.Show(Cursor.Position);
            }

        }




        private void refreshRecoveries()
        {
            // Refresh recoveries so that we have consecutive image keys
            List<Page> newPages = new List<Page>();
            foreach (ListViewItem l in lvThumbnails.Items)
            {
                if (recoveries[Convert.ToInt32(l.Index)] != null)
                    newPages.Add((Page)recoveries[Convert.ToInt32(l.ImageKey)]);
            }

            Hashtable newRecoveries = new Hashtable();
            for (int n = 0; n < newPages.Count; n++)
                newRecoveries.Add(n, newPages[n]);

            recoveries = newRecoveries;
        }

        private void refreshThumbnails()
        {
            lvThumbnails.Clear();
            ilThumbnails.Images.Clear();

            int tWidth = lvThumbnails.Width - 50;
            if (tWidth > 256)
                tWidth = 256;
            int tHeight = Convert.ToInt32(tWidth * 0.9);
            ilThumbnails.ImageSize = new Size(tWidth, tHeight);

            String pageNum = formPage.Lines[0].MagPage;

            if (dropType == "file")
            {
                ilThumbnails.Images.Add(pageNum + "-" + service.Position.ToString(), LoadBitmapResource("back2.bmp"));
                ListViewItem lvItem = lvThumbnails.Items.Add(pageNum + "-" + service.Position.ToString(), "Back", 0);
                lvItem.ImageIndex = 0;
            }

            // Make list of subpages
            List<int> subpages = new List<int>();
            foreach (var o in recoveries.Keys)
            {
                subpages.Add((int)o);
            }
            subpages.Sort();

            int key = 0;

            // Get all subpages from the recovery file
            for (int index = 0; index < subpages.Count; index++)

            {
                // Get the next key from the recoveries table
                //int key = subpages[index];

                if (recoveries[index] != null)
                {
                    formPage = (Page)recoveries[key];
                    formPage.Lines[0].Text = "   P" + formPage.Lines[0].Magazine + formPage.Lines[0].Page + " " + formPage.Lines[0].Text.Substring(8);


                    // Render image to thumbnail
                    TeletextRenderer rendererNoBorders = new TeletextRenderer(false);
                    rendererNoBorders.NationalOptionSelectionBits = renderer.NationalOptionSelectionBits;
                    rendererNoBorders.PresentationLevel = renderer.PresentationLevel;
                    rendererNoBorders.Font = renderer.Font;
                    rendererNoBorders.RevealPressed = renderer.RevealPressed;
                    rendererNoBorders.DeviceDPI = this.DeviceDpi;
                    layers = rendererNoBorders.Render(formPage, true, true);



                    //Bitmap thumbnail = new Bitmap(layers.Background, new Size(tWidth, tHeight));
                    Bitmap thumbnail = new Bitmap(layers.Background);
                    Graphics g = Graphics.FromImage(thumbnail);
                    g.FillRectangle(new SolidBrush(Color.Black), 0, 0, thumbnail.Width, thumbnail.Height);

                    g.DrawImage(layers.Background, 0, 0);
                    g.DrawImage(layers.Foreground, 0, 0);



                    ilThumbnails.Images.Add(key.ToString(), thumbnail);
                    ListViewItem lvItem = lvThumbnails.Items.Add("Key: " + key.ToString());
                    lvItem.Name = key.ToString();
                    lvItem.ImageKey = key.ToString();




                    //Graphics g = Graphics.FromImage(thumbnail);
                    //g.FillRectangle(new SolidBrush(Color.Black), 0, 0, thumbnail.Width, thumbnail.Height);

                    //g.DrawImage(layers.Background, 0, 0);
                    //g.DrawImage(layers.Foreground, 0, 0);

                    //ilThumbnails.Images.Add(key.ToString(), thumbnail);
                    //ListViewItem lvItem = lvThumbnails.Items.Add("Key: " + key.ToString());
                    ////ListViewItem lvItem = lvThumbnails.Items.Add(key.ToString());
                    //lvItem.ImageKey = key.ToString();

                    g.Dispose();

                    key++;
                }
            }
            if (dropType == "file")
            {
                ilThumbnails.Images.Add(pageNum + "-" + service.Position.ToString(), LoadBitmapResource("forward2.bmp"));
                ListViewItem lvItem = lvThumbnails.Items.Add(pageNum + "-" + service.Position.ToString(), "Forward", key + 1);
                //lvItem.ImageKey = pageNum + "-" + service.Position.ToString();
                lvItem.ImageIndex = key + 1;
            }

            lvThumbnails.Invalidate();
            lvThumbnails.Refresh();

            redrawThumbnails = false;
        }

        private void deleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*Int32 i = 0;
            while (i < recoveredPages.SelectedItems.Count)
            {
                if (!recoveredPages.Items.selected)
                {
                    lvThumbnails.Items.RemoveAt(i);
                    ilThumbnails.Images.RemoveAt(i);
                    i = 0;
                }
                else
                    i++;
            }*/

            foreach (Int32 i in recoveredPages.SelectedIndices)
            {
                //recoveredPages.Items.RemoveAt(recoveredPages.SelectedIndices[i]);
                DialogResult dr = MessageBox.Show("This will delete file: " + recoveredPages.Items[i].ToString() + ". \n\n Are you sure?", "Delete carousel", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == System.Windows.Forms.DialogResult.Yes)
                {
                    try
                    {
                        File.Delete(recoveredPages.Name + recoveredPages.Items[i].ToString());
                        recoveredPages.Items.RemoveAt(i);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred whilst attempting to deleting the file:" + ex.Message, "Delete carousel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }


        }

        private void charMap_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDragging = true;
            //charMap.DeallocateCursor();
            //snapshot = (Bitmap)charMap.Image.Clone();
            boxStartX = e.X;
            boxStartY = e.Y;

            //convert this to a character on the char mapped bitmap
            //boxStartXChar = (Byte)((Double)e.X / (Double)charMap.Size.Width * charMap.CharWidth);
            //boxStartYChar = (Byte)((Double)e.Y / (Double)charMap.Size.Height * charMap.CharHeight);

            byte[] canvasXY = ConvertMousePosToCharMapXY(e.X, e.Y);

            boxStartXChar = canvasXY[0];
            boxStartYChar = canvasXY[1];

            // Copy image
            /*Rectangle rect = new Rectangle(0, 0, charMap.Size.Width, charMap.Size.Height);
            snapshot = (Bitmap)charMap.Image.Clone();
            //snapshot = wholeThing.Clone(rect, wholeThing.PixelFormat);
            //wholeThing.Dispose();

            System.Drawing.Imaging.BitmapData bmd = snapshot.LockBits(new Rectangle(0, 0, snapshot.Width, snapshot.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, snapshot.PixelFormat);

            snapshot.UnlockBits(bmd);*/

            if (snapshot == null && charMap.Image != null)
                snapshot = new Bitmap(charMap.Image);
            //snapshot = CopyBitmap((Bitmap)charMap.Image);
        }



        private void charMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (layers != null)
            {
                mouseDragging = false;

                boxEndX = e.X;
                boxEndY = e.Y;

                byte[] canvasXY = ConvertMousePosToCharMapXY(e.X, e.Y);

                boxEndXChar = canvasXY[0];
                boxEndYChar = canvasXY[1];

                if (boxEndXChar == boxStartXChar && boxEndYChar == boxStartYChar)
                {
                    selectedBoxEndXChar = boxEndXChar;
                    selectedBoxEndYChar = boxEndYChar;
                }

                //MessageBox.Show("sX: " + boxStartX + "  sY: " + boxStartY + "\neX: " + boxEndX + "   eY: " + boxEndY + "\n Width: " + ((boxEndX - boxStartX) / charMap.Width / 40 * 12).ToString() + "    Height: " + (boxEndY - boxStartY).ToString());
                charMap.InitCursor();
                System.Diagnostics.Debug.Print(selectedBoxEndXChar + ", " + selectedBoxEndYChar);
                lblXpos.Text = "X: " + charMap.cursorX;
                lblYPos.Text = "Y: " + charMap.cursorY;


            }
        }

        private void charMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDragging && e.Button == MouseButtons.Left && layers != null)
            {


                //boxEndX = (Int32)((Double)e.X / charMap.CharPixelWidth) * charMap.CharPixelWidth;
                //boxStartX = (Int32)((Double)boxStartX / charMap.CharPixelWidth) * charMap.CharPixelWidth;
                boxEndX = e.X;
                boxEndY = e.Y;

                //convert this to a character on the char mapped bitmap

                //boxEndXChar = (Byte)((Double)e.X / (Double)charMap.Size.Width * charMap.CharWidth);
                //boxEndYChar = (Byte)((Double)e.Y / (Double)charMap.Size.Height * charMap.CharHeight);

                byte[] canvasXY = ConvertMousePosToCharMapXY(e.X, e.Y);

                boxEndXChar = canvasXY[0] + 1;
                boxEndYChar = canvasXY[1] + 1;

                // If mouse position has moved over a character border then redraw
                if (boxEndXChar != selectedBoxEndXChar || boxEndYChar != selectedBoxEndYChar)
                {
                    charMap.Image = new Bitmap(snapshot);
                    charMap.Invalidate();
                    charMap.Update();
                    charMap.Refresh();

                    selectedBoxEndXChar = boxEndXChar;
                    selectedBoxEndYChar = boxEndYChar;


                    System.Diagnostics.Debug.Print("StartX: " + boxStartXChar + ", StartY: " + boxStartYChar + ", EndX: " + boxEndXChar + ", EndY: " + boxEndYChar);
                    System.Diagnostics.Debug.Print("StartX: " + boxStartX + ", StartY: " + boxStartY + ", EndX: " + boxEndX + ", EndY: " + boxEndY);

                    //The image is scaled; what is the width of a character?
                    //Double scaledCharPixelWidth = charMap.CharPixelWidth / ((Double)(charMap.CharWidth * charMap.CharPixelWidth) / charMap.Size.Width);
                    //scaledCharPixelWidth = charMap.Size.Width / (Double)charMap.CharWidth;
                    //scaledCharPixelWidth = 12D;

                    Bitmap bmpSelected = new Bitmap(480, 500, PixelFormat.Format32bppArgb);
                    Graphics gSelected = Graphics.FromImage(bmpSelected);
                    Brush b = new SolidBrush(Color.FromArgb(64, Color.White));
                    Pen p = new Pen(b);

                    //System.Diagnostics.Debug.Print("Boomshanka: " + Convert.ToDouble(charMap.Width) / (charMap.CharWidth * charMap.CharPixelWidth));
                    //Rectangle r = new Rectangle(boxStartX * charMap.CharPixelWidth, boxStartY * charMap.CharPixelHeight, (boxEndX - boxStartX) * charMap.CharPixelWidth, (boxEndY - boxStartY) * charMap.CharPixelHeight);
                    //Rectangle r = new Rectangle(boxStartX, boxStartY, Convert.ToInt32(Convert.ToDouble(boxEndX - boxStartX) * Convert.ToDouble(charMap.Width) / (charMap.CharPixelWidth * charMap.CharWidth)), boxEndY - boxStartY);
                    //Rectangle r = new Rectangle(boxStartX, boxStartY, (Int32)( ((boxEndX - boxStartX) * ((Double)(charMap.CharWidth * charMap.CharPixelWidth) / charMap.Size.Width )) ), boxEndY - boxStartY);
                    //Rectangle r = new Rectangle((Int32)(boxStartXChar * scaledCharPixelWidth), boxStartY, (Int32)((boxEndXChar - boxStartXChar) * charMap.CharPixelWidth), boxEndY - boxStartY);
                    Rectangle r = new Rectangle(boxStartXChar * charMap.CharPixelWidth, boxStartYChar * charMap.CharPixelHeight, (Int32)((boxEndXChar - boxStartXChar) * charMap.CharPixelWidth), (Int32)((boxEndYChar - boxStartYChar) * charMap.CharPixelHeight));


                    gSelected.DrawImage(snapshot, 0, 0);
                    gSelected.FillRectangle(b, r);

                    RenderItemToCharMap();

                    //charMap.BackgroundImage = new Bitmap(borders ? 700 : 480, borders ? 600 : 500, PixelFormat.Format32bppArgb);
                    //charMap.Image = new Bitmap(borders ? 700 : 480, borders ? 600 : 500, PixelFormat.Format32bppArgb);
                    Graphics g = Graphics.FromImage(charMap.BackgroundImage);

                    ////g.DrawImage(layers.L25Background, 0, 0);
                    //g.DrawImage(layers.Background, 0, 0);
                    //g.DrawImage(layers.Foreground, 0, 0);



                    g.DrawImage(bmpSelected, charMap.l1CanvasXOrigin, charMap.l1CanvasYOrigin);
                    g.Dispose();

                    charMap.Invalidate();
                    charMap.Update();
                    charMap.Refresh();
                }
            }
        }





        private void tbPage_Leave(object sender, EventArgs e)
        {
            //Recalculate Header and pages codes
            if (tbPage.Text.Length == 3)
            {
                string p = tbPage.Text.Substring(0, 1) == "8" ? "0" + tbPage.Text.Substring(1, 2) : tbPage.Text;
                formPage.Lines[0].Magazine = Convert.ToInt32(p.Substring(0, 1));
                formPage.Lines[0].Page = p.Substring(1, 2);
                formPage.Lines[0].MagPage = p;
                formPage.Lines[0].CalcHammingCodes();
                // Change MRAG magazine to match
                for (int n = 1; n < 256; n++)
                {
                    if (formPage.Lines[n].Type != LineTypes.Blank)
                    {
                        formPage.Lines[n].Magazine = Convert.ToInt32(p.Substring(0, 1));
                        formPage.Lines[n].CalcHammingCodes();
                    }
                }
            }
            changesMade = true;
        }

        private void importBase64URLLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form b64 = new frmBase64UrlImport();
            b64.Show();
        }

        private void createTeletextStreamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form frmGenerate = new frmCreateTeletextStream();
            frmGenerate.Show();
        }

        private void TeletextRecoveryEditor_DragDrop(object sender, DragEventArgs e)
        {
            borders = mnuShowBorders.Checked;
            //charMap.DeallocateCursor();
            string nationalOptionSelectionBits = "";
            double presentationLevel = 0;
            string font = "";
            bool rendererExisted = false;

            lvThumbnails.Clear();
            ilThumbnails.Images.Clear();
            recoveries.Clear();

            if (renderer != null)
            {
                nationalOptionSelectionBits = renderer.NationalOptionSelectionBits;
                presentationLevel = renderer.PresentationLevel;
                font = renderer.Font;
                rendererExisted = true;
                renderer.DeviceDPI = this.DeviceDpi;
            }

            string[] folder = (string[])e.Data.GetData(DataFormats.FileDrop);
            string[] fileList = new string[1];
            Boolean blnError = false;

            dropType = folder[0].EndsWith(".t42") ? "file" : "folder";

            if (dropType == "folder")
            {
                currentBinariesFolder = folder[0];
                this.Text = applicationName + " : " + folder[0];
                recoveredPages.Items.Clear();

                renderer = new TeletextRenderer(borders);
                if (rendererExisted)
                {
                    renderer.NationalOptionSelectionBits = nationalOptionSelectionBits;
                    renderer.PresentationLevel = presentationLevel;
                    renderer.Font = font;
                }

                renderer.DeviceDPI = this.DeviceDpi;
                layers = new RenderedLayersNova(renderer.DeviceDPI, borders);

                try
                {
                    fileList = Directory.GetFiles(folder[0]);
                }
                catch
                {
                    blnError = true;
                    MessageBox.Show("Item is not an openable folder.", "Teletext Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (!blnError)
                {
                    recoveredPages.Name = fileList[0].Substring(0, fileList[0].LastIndexOf("\\")) + "\\";
                    foreach (string file in fileList)
                    {
                        recoveredPages.Items.Add(file.Substring(file.LastIndexOf("\\") + 1));
                    }
                }
            }
            else
            {
                renderer = new TeletextRenderer(borders);
                renderer.DeviceDPI = this.DeviceDpi;
                ReadT42File(folder[0]);
                service.Reset();
            }
            EnableDisableMenus();
        }

        private void TeletextRecoveryEditor_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }



        #region MenuItems
        private void tTIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Export page to TTI format
            String strPageNumber = "PN," + formPage.Lines[0].Magazine.ToString() + formPage.Lines[0].Page + formPage.Lines[0].TimeCode.Substring(formPage.Lines[0].TimeCode.Length - 2, 2);
            String strCycleTime = "CT,1,C";
            String strDescription = "DE,Saved using Teletext Recovery Editor by @grim_fandango";
            String strPageStatus = "PS,8040";
            String strSubCode = "SC," + formPage.Lines[0].TimeCode.Substring(0, 2) + formPage.Lines[0].TimeCode.Substring(formPage.Lines[0].TimeCode.Length - 2, 2);

            sfdTti.ShowDialog();
            String strPath = sfdTti.FileName;

            if (strPath != "")
            {
                //StreamWriter twWriter = new StreamWriter(strPath, false, Encoding.Default);
                BinaryWriter twWriter = new BinaryWriter(File.Open(strPath, FileMode.Create));
                byte[] newLine = { 0x0d, 0x0a };
                twWriter.Write(Encoding.ASCII.GetBytes(strPageNumber));
                twWriter.Write(newLine);
                twWriter.Write(Encoding.ASCII.GetBytes(strCycleTime));
                twWriter.Write(newLine);
                twWriter.Write(Encoding.ASCII.GetBytes(strDescription));
                twWriter.Write(newLine);
                twWriter.Write(Encoding.ASCII.GetBytes(strPageStatus));
                twWriter.Write(newLine);
                twWriter.Write(Encoding.ASCII.GetBytes(strSubCode));
                twWriter.Write(newLine);

                for (int row = 0; row < 25; row++)
                {
                    if (formPage.Lines[row].Type != LineTypes.Blank)
                    {
                        // Create a line in the TTI OL format
                        byte[] rowHeader = Encoding.ASCII.GetBytes("OL," + row + ",");
                        twWriter.Write(rowHeader);

                        for (int b = 2; b < 42; b++)
                        {
                            Byte bytCharByte = Convert.ToByte(formPage.Lines[row].Bytes[b] & Convert.ToByte(0x7f));
                            if (bytCharByte < 0x20)
                            {
                                bytCharByte += 0x80;
                            }
                            twWriter.Write(bytCharByte);
                        }
                        twWriter.Write(newLine);
                    }
                }
                twWriter.Close();
            }

        }


        private void eP1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sfdEp1.ShowDialog();
            String strPath = sfdEp1.FileName;

            if (sfdEp1.FileName != "")
            {
                BinaryWriter bwEp1 = new BinaryWriter(new FileStream(sfdEp1.FileName, FileMode.Create));

                byte[] bytEp1Header = { 0xFE, 0x01, 0x09, 0x00, 0x00, 0x00 };
                bwEp1.Write(bytEp1Header);

                // Blank line in case the packet is null
                Byte[] bytBlankLine40 = new Byte[40];
                for (int n = 0; n < 40; n++)
                    bytBlankLine40[n] = 0x20;

                for (int row = 0; row < 25; row++)
                {
                    Byte[] bytLine40 = new Byte[40];

                    Line l = formPage.GetRow(formPage.GetPacketIndex(row));
                    if (l.Bytes != null)
                    {
                        // Bitwise AND 0x7f
                        for (Byte b = 0; b < bytLine40.Length; b++)
                        {
                            bwEp1.Write(Convert.ToByte(l.Bytes[b + 2] & (Byte)0x7f));
                        }
                    }
                    else
                        bwEp1.Write(bytBlankLine40);
                }

                byte[] bytEp1Footer = { 0x00, 0x00 };
                bwEp1.Write(bytEp1Footer);

                bwEp1.Close();
            }
        }

        private void openCarouselToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName != "")
            {
                recoveredPages.Items.Clear();

                if (openFileDialog1.FileName.EndsWith("43bp") || openFileDialog1.FileName.EndsWith("t43"))
                    service.RowLength = 43;

                if (openFileDialog1.FileName.EndsWith("bin") || openFileDialog1.FileName.EndsWith("t42"))
                    service.RowLength = 42;

                if (openFileDialog1.FileName.EndsWith("TTX"))
                    service.RowLength = 40;

                if (openFileDialog1.FileName.EndsWith("EP1"))
                    service.RowLength = 40;

                //service.RowLength = openFileDialog1.FileName.EndsWith("43bp") ? (Byte)43 : (Byte)42;

                recoveredPages.Name = openFileDialog1.FileName.Substring(0, openFileDialog1.FileName.LastIndexOf("\\")) + "\\";
                recoveredPages.Items.Add(openFileDialog1.FileName.Substring(openFileDialog1.FileName.LastIndexOf("\\") + 1));
            }
        }

        private void mergeBinariesIntoAT42ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form frmMerge = new frmMergeIntoT42();
            frmMerge.Show();
        }

        private void mullardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mullardToolStripMenuItem.Checked = true;
            etsToolStripMenuItem.Checked = false;
            tiFaxToolStripMenuItem.Checked = false;
            genericASCIIToolStripMenuItem.Checked = false;
            renderer.Font = "Mullard";
            RenderItemToCharMap();
        }

        private void etsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mullardToolStripMenuItem.Checked = false;
            etsToolStripMenuItem.Checked = true;
            tiFaxToolStripMenuItem.Checked = false;
            genericASCIIToolStripMenuItem.Checked = false;
            renderer.Font = "ETS";
            RenderItemToCharMap();
        }

        private void tiFaxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mullardToolStripMenuItem.Checked = false;
            etsToolStripMenuItem.Checked = false;
            tiFaxToolStripMenuItem.Checked = true;
            genericASCIIToolStripMenuItem.Checked = false;
            renderer.Font = "TiFax";
            RenderItemToCharMap();
        }

        private void genericASCIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mullardToolStripMenuItem.Checked = false;
            etsToolStripMenuItem.Checked = false;
            tiFaxToolStripMenuItem.Checked = false;
            genericASCIIToolStripMenuItem.Checked = true;
            renderer.Font = "1975";
            RenderItemToCharMap();
        }
        private void edittfToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Create a byte array of all of the page packets
            byte[] bytPage = new byte[1000];
            int intPagePointer = 0;
            for (int row = 0; row < 25; row++)
            {
                Line l = formPage.GetRow(formPage.GetPacketIndex(row));
                int intBytesLength = 0;

                if (l.Bytes != null)
                {
                    intBytesLength = l.Bytes.Length - 1;
                    for (Byte b = 3; b < intBytesLength; b++)
                    {
                        bytPage[intPagePointer] = l.Bytes[b];
                        intPagePointer++;
                    }
                }

                //If the packet isn't full, fill it
                /*for (int n=0; n < 40 - intBytesLength - 2;n++)
                {
                    bytPage[intPagePointer] = 0x20;
                    intPagePointer++;
                }*/
            }

            string binString = "";
            // Convert bytes to binary string
            for (int n = 0; n < 1000; n++)
            {
                binString += Convert.ToString(bytPage[n], 2).PadLeft(8, Convert.ToChar("0")).Substring(1);
            }
            binString += "00";

            string strEncoded = "http://www.uniquecodeanddata.co.uk/editor/#0:";

            // Loop 5-bit segments
            for (int n = 0; n < binString.Length / 6; n++)
            {
                // Get substring
                string strSegment = binString.Substring(n * 6, 6);
                int index = Convert.ToInt32(strSegment, 2);
                strEncoded += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".Substring(index, 1);
            }
        }

        private void c64TeletextReaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sfdC64.ShowDialog();

            if (sfdC64.FileName != "")
            {
                Byte[] bytC64 = new Byte[0x0783];
                bytC64[0] = 0x00;
                bytC64[1] = 0x40;

                int bytPosition = 2;
                for (int y = 0; y < 24; y++)
                {
                    for (int x = 0; x < 40; x++)
                    {
                        if (y == 23)
                            System.Diagnostics.Debug.Write("_" + formPage.modeMap[y, x].ForeColourCode + ", ");
                        Byte c = (byte)(formPage.modeMap[y, x].Character & 0x7f);
                        System.Diagnostics.Debug.Write((char)(formPage.modeMap[y, x].Character & 0x7f));
                        if (c < 0x0f)
                            c = 0x20;
                        bytC64[bytPosition] = c;
                        bytPosition++;
                    }
                    System.Diagnostics.Debug.WriteLine("");

                }

                // Separator between text and attribute data
                bytC64[bytPosition] = 0;
                bytPosition++;

                // Attribute data
                for (int y = 0; y < 24; y++)
                {
                    for (int x = 0; x < 40; x++)
                    {
                        byte bytAttribute = 0;

                        // Is double height?
                        if (formPage.modeMap[y, x].DoubleHeight)
                            bytAttribute = (Byte)(bytAttribute | 0x80);

                        // Foreground colour code
                        switch (formPage.modeMap[y, x].ForeColourCode)
                        {
                            case 0:
                                {
                                    // black
                                    bytAttribute = (Byte)(bytAttribute | 0);
                                    break;
                                }
                            case 1:
                                {
                                    // red = cbm colour 2
                                    bytAttribute = (Byte)(bytAttribute | 0x20);
                                    break;
                                }
                            case 2:
                                {
                                    // green = cbm colour 5
                                    bytAttribute = (Byte)(bytAttribute | 0x50);
                                    break;
                                }
                            case 3:
                                {
                                    // yellow = cbm colour 7
                                    bytAttribute = (Byte)(bytAttribute | 0x70);
                                    break;
                                }
                            case 4:
                                {
                                    // blue = cbm colour 6
                                    bytAttribute = (Byte)(bytAttribute | 0x60);
                                    break;
                                }
                            case 5:
                                {
                                    // magenta = cbm colour 4
                                    bytAttribute = (Byte)(bytAttribute | 0x40);
                                    break;
                                }
                            case 6:
                                {
                                    // cyan = cbm colour 3
                                    bytAttribute = (Byte)(bytAttribute | 0x30);
                                    break;
                                }
                            case 7:
                                {
                                    // white = cbm colour 1
                                    bytAttribute = (Byte)(bytAttribute | 0x10);
                                    break;
                                }
                        }

                        // Is double width?
                        if (formPage.modeMap[y, x].DoubleWidth)
                            bytAttribute = (Byte)(bytAttribute | 8);

                        // Background colour code
                        switch (formPage.modeMap[y, x].BgndColourCode)
                        {
                            case 0:
                                {
                                    // black
                                    bytAttribute = (Byte)(bytAttribute | 0x00);
                                    break;
                                }
                            case 1:
                                {
                                    // red = cbm colour 2
                                    bytAttribute = (Byte)(bytAttribute | 0x02);
                                    break;
                                }
                            case 2:
                                {
                                    // green = cbm colour 5
                                    bytAttribute = (Byte)(bytAttribute | 0x05);
                                    break;
                                }
                            case 3:
                                {
                                    // yellow = cbm colour 7
                                    bytAttribute = (Byte)(bytAttribute | 0x07);
                                    break;
                                }
                            case 4:
                                {
                                    // blue = cbm colour 6
                                    bytAttribute = (Byte)(bytAttribute | 0x06);
                                    break;
                                }
                            case 5:
                                {
                                    // magenta = cbm colour 4
                                    bytAttribute = (Byte)(bytAttribute | 0x04);
                                    break;
                                }
                            case 6:
                                {
                                    // cyan = cbm colour 3
                                    bytAttribute = (Byte)(bytAttribute | 0x03);
                                    break;
                                }
                            case 7:
                                {
                                    // white = cbm colour 1
                                    bytAttribute = (Byte)(bytAttribute | 0x01);
                                    break;
                                }
                        }

                        bytC64[bytPosition] = bytAttribute;
                        bytPosition++;
                    }
                }

                // Save file
                FileStream fs = new FileStream(sfdC64.FileName, FileMode.Create);
                BinaryWriter writer = new BinaryWriter(fs);

                writer.Write(bytC64);

                writer.Close();
                writer.Dispose();
                fs.Close();
                fs.Dispose();
            }
        }

        private void aRDTextHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Load HTML page 
            HttpWebRequest hwrRequest = (HttpWebRequest)WebRequest.Create("http://www.ard-text.de/index.php?page=100");
            hwrRequest.Method = "GET";
            HttpWebResponse hwrResponse = (HttpWebResponse)hwrRequest.GetResponse();
            StreamReader htmlStream = new StreamReader(hwrResponse.GetResponseStream(), System.Text.Encoding.UTF8);

            string html = htmlStream.ReadToEnd();
            htmlStream.Close();




        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About a = new About();
            a.Show();
        }

        private void combineCarouselsToT42ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string suggestedFilename = currentBinariesFolder.Substring(0, currentBinariesFolder.LastIndexOf("\\"));
            suggestedFilename = suggestedFilename.Substring(suggestedFilename.LastIndexOf("\\") + 1);
            saveFileDialogCarousel.FileName = suggestedFilename;

            if (saveFileDialogCarousel.ShowDialog() == DialogResult.OK)
            {

                if (saveFileDialogCarousel.FileName != "")
                {
                    if (File.Exists(saveFileDialogCarousel.FileName))
                    {
                        try
                        {
                            File.Delete(saveFileDialogCarousel.FileName);
                        }
                        catch { }
                    }

                    String errors = "";

                    // Loop carousels in the binaries folder
                    foreach (string filename in recoveredPages.Items)
                    {
                        Hashtable carousel = new Hashtable();

                        // Load binary using Service object
                        String status = service.OpenService(recoveredPages.Name + filename);

                        if (status == "")
                        {
                            // Get all subpages from the recovery file
                            int key = 0;
                            while (service.Revolutions == 0)
                            {
                                // Get page from Service object
                                formPage = service.GetPage();
                                service.Position = service.Position - service.PacketSize;
                                formPage.Lines[0].Text = "   P" + formPage.Lines[0].Magazine + formPage.Lines[0].Page + " " + formPage.Lines[0].Text;

                                // Add Page object to hashtable for this recovery file
                                carousel[key] = formPage;

                                key++;
                            }

                        }
                        else
                        {
                            if (errors == "")
                                errors = "Errors found:\r\n\r\n";

                            errors += status + Environment.NewLine;
                        }

                        TrimCarousel trimmed = new TrimCarousel();
                        Hashtable cleanedCarousel = trimmed.Trim(carousel);

                        saveCarousel(saveFileDialogCarousel.FileName, cleanedCarousel, true);

                    }
                    MessageBox.Show("File " + saveFileDialogCarousel.FileName + " saved. \r\n\r\n" + errors, "Teletext Recovery Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void openT42ServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Populate 'recovered pages' pane with list of pages in the service
            CommonOpenFileDialog folderBrowser = new CommonOpenFileDialog();

            CommonFileDialogResult result = folderBrowser.ShowDialog();
            string[] fileList;

            renderer = new TeletextRenderer(borders);
            renderer.DeviceDPI = this.DeviceDpi;

            if (result == CommonFileDialogResult.Ok)
            {
                string filename = folderBrowser.FileName;
                dropType = "file";

                lvThumbnails.Clear();
                ilThumbnails.Images.Clear();
                recoveries.Clear();

                ReadT42File(filename);
                EnableDisableMenus();
            }

        }

        private void ReadT42File(string filename)
        {
            this.Text = applicationName + " : " + filename;
            recoveredPages.Items.Clear();

            service.OpenService(filename);
            service.CheckHorizon = 0x200000;

            var carousels = service.ListCarousels();
            foreach (string item in carousels)
            {
                recoveredPages.Items.Add(item);
            }

            recoveredPages.Sorted = true;
            recoveredPages.Items.Add("Magazine 8");
            recoveredPages.Refresh();
            currentlySelectedFile = "";
        }

        private void saveT42ServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {

            saveFileDialog1.FileName = service.Filename;
            saveFileDialog1.Filter = "T42 files|*.t42|All files|*.*";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (changesMade) saveCarousel();
                service.SaveService(saveFileDialog1.FileName);
                MessageBox.Show(String.Format("File {0} saved.", saveFileDialog1.FileName), "Teletext Recovery Editor");
            }
            changesMade = false;

        }

        #endregion

        #region Language_MenuItems

        private void english0000000ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            english0000000ToolStripMenuItem.Checked = true;
            german0000001ToolStripMenuItem.Checked = false;
            swedishFinnishHungarian0000010ToolStripMenuItem.Checked = false;
            italian0000011ToolStripMenuItem1.Checked = false;
            french0000100ToolStripMenuItem1.Checked = false;
            portugueseSpanish0000101ToolStripMenuItem1.Checked = false;
            czechSlovak0000111ToolStripMenuItem1.Checked = false;
            polish0001000ToolStripMenuItem1.Checked = false;
            turkish0010110ToolStripMenuItem1.Checked = false;
            serbianCroatianCyrillic0100000ToolStripMenuItem1.Checked = false;
            rumanian0011111ToolStripMenuItem1.Checked = false;
            serbianCroatianSlovenian0011101ToolStripMenuItem1.Checked = false;
            estonian0100010ToolStripMenuItem1.Checked = false;
            lettishLithuanian0100011ToolStripMenuItem1.Checked = false;
            russianBulgarianCyrillic0100100ToolStripMenuItem1.Checked = false;
            ukranianCyrillic0100101ToolStripMenuItem1.Checked = false;
            greek0110111ToolStripMenuItem1.Checked = false;

            renderer.NationalOptionSelectionBits = "0000000";
            RenderItemToCharMap();
        }

        private void german0000001ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            english0000000ToolStripMenuItem.Checked = false;
            german0000001ToolStripMenuItem.Checked = true;
            swedishFinnishHungarian0000010ToolStripMenuItem.Checked = false;
            italian0000011ToolStripMenuItem1.Checked = false;
            french0000100ToolStripMenuItem1.Checked = false;
            portugueseSpanish0000101ToolStripMenuItem1.Checked = false;
            czechSlovak0000111ToolStripMenuItem1.Checked = false;
            polish0001000ToolStripMenuItem1.Checked = false;
            turkish0010110ToolStripMenuItem1.Checked = false;
            serbianCroatianCyrillic0100000ToolStripMenuItem1.Checked = false;
            rumanian0011111ToolStripMenuItem1.Checked = false;
            serbianCroatianSlovenian0011101ToolStripMenuItem1.Checked = false;
            estonian0100010ToolStripMenuItem1.Checked = false;
            lettishLithuanian0100011ToolStripMenuItem1.Checked = false;
            russianBulgarianCyrillic0100100ToolStripMenuItem1.Checked = false;
            ukranianCyrillic0100101ToolStripMenuItem1.Checked = false;
            greek0110111ToolStripMenuItem1.Checked = false;

            renderer.NationalOptionSelectionBits = "0000001";
            RenderItemToCharMap();
        }

        private void polish0001000ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            english0000000ToolStripMenuItem.Checked = false;
            german0000001ToolStripMenuItem.Checked = false;
            swedishFinnishHungarian0000010ToolStripMenuItem.Checked = false;
            italian0000011ToolStripMenuItem1.Checked = false;
            french0000100ToolStripMenuItem1.Checked = false;
            portugueseSpanish0000101ToolStripMenuItem1.Checked = false;
            czechSlovak0000111ToolStripMenuItem1.Checked = false;
            polish0001000ToolStripMenuItem1.Checked = true;
            turkish0010110ToolStripMenuItem1.Checked = false;
            serbianCroatianCyrillic0100000ToolStripMenuItem1.Checked = false;
            rumanian0011111ToolStripMenuItem1.Checked = false;
            serbianCroatianSlovenian0011101ToolStripMenuItem1.Checked = false;
            estonian0100010ToolStripMenuItem1.Checked = false;
            lettishLithuanian0100011ToolStripMenuItem1.Checked = false;
            russianBulgarianCyrillic0100100ToolStripMenuItem1.Checked = false;
            ukranianCyrillic0100101ToolStripMenuItem1.Checked = false;
            greek0110111ToolStripMenuItem1.Checked = false;

            renderer.NationalOptionSelectionBits = "0001000";
            RenderItemToCharMap();
        }

        private void swedishFinnishHungarian0000010ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            english0000000ToolStripMenuItem.Checked = false;
            german0000001ToolStripMenuItem.Checked = false;
            swedishFinnishHungarian0000010ToolStripMenuItem.Checked = true;
            italian0000011ToolStripMenuItem1.Checked = false;
            french0000100ToolStripMenuItem1.Checked = false;
            portugueseSpanish0000101ToolStripMenuItem1.Checked = false;
            czechSlovak0000111ToolStripMenuItem1.Checked = false;
            polish0001000ToolStripMenuItem1.Checked = false;
            turkish0010110ToolStripMenuItem1.Checked = false;
            serbianCroatianCyrillic0100000ToolStripMenuItem1.Checked = false;
            rumanian0011111ToolStripMenuItem1.Checked = false;
            serbianCroatianSlovenian0011101ToolStripMenuItem1.Checked = false;
            estonian0100010ToolStripMenuItem1.Checked = false;
            lettishLithuanian0100011ToolStripMenuItem1.Checked = false;
            russianBulgarianCyrillic0100100ToolStripMenuItem1.Checked = false;
            ukranianCyrillic0100101ToolStripMenuItem1.Checked = false;
            greek0110111ToolStripMenuItem1.Checked = false;

            renderer.NationalOptionSelectionBits = "0000010";
            RenderItemToCharMap();
        }

        private void italian0000011ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            english0000000ToolStripMenuItem.Checked = false;
            german0000001ToolStripMenuItem.Checked = false;
            swedishFinnishHungarian0000010ToolStripMenuItem.Checked = false;
            italian0000011ToolStripMenuItem1.Checked = true;
            french0000100ToolStripMenuItem1.Checked = false;
            portugueseSpanish0000101ToolStripMenuItem1.Checked = false;
            czechSlovak0000111ToolStripMenuItem1.Checked = false;
            polish0001000ToolStripMenuItem1.Checked = false;
            turkish0010110ToolStripMenuItem1.Checked = false;
            serbianCroatianCyrillic0100000ToolStripMenuItem1.Checked = false;
            rumanian0011111ToolStripMenuItem1.Checked = false;
            serbianCroatianSlovenian0011101ToolStripMenuItem1.Checked = false;
            estonian0100010ToolStripMenuItem1.Checked = false;
            lettishLithuanian0100011ToolStripMenuItem1.Checked = false;
            russianBulgarianCyrillic0100100ToolStripMenuItem1.Checked = false;
            ukranianCyrillic0100101ToolStripMenuItem1.Checked = false;
            greek0110111ToolStripMenuItem1.Checked = false;

            renderer.NationalOptionSelectionBits = "0000011";
            RenderItemToCharMap();
        }



        #endregion

        #region CheckBoxes

        private void mnuOracle76ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (mnuTo19750901.Checked)
            {

                mnu19760101.Checked = false;
                mnuLevel10.Checked = false;
                mnuLevel15.Checked = false;
                mnuLevel20.Checked = false;
                mnuLevel25.Checked = false;
                mnuLevel40.Checked = false;


                if (renderer != null)
                {
                    renderer.PresentationLevel = 0.19750901;
                    layers = renderer.Render(formPage);

                    if (charMap.BackgroundImage != null)
                    {
                        RenderCharMap();

                        RenderItemToCharMap();
                    }
                }
            }
        }

        private void mnu19760101_CheckedChanged(object sender, EventArgs e)
        {
            if (mnu19760101.Checked)
            {
                mnuTo19750901.Checked = false;
                mnuLevel10.Checked = false;
                mnuLevel15.Checked = false;
                mnuLevel20.Checked = false;
                mnuLevel25.Checked = false;
                mnuLevel40.Checked = false;


                if (renderer != null)
                {
                    renderer.PresentationLevel = 0.19760101;
                    layers = renderer.Render(formPage);
                    if (charMap.BackgroundImage != null)
                    {
                        RenderCharMap();

                        RenderItemToCharMap();
                    }
                }
            }
        }

        private void mnuLevel15_CheckedChanged(object sender, EventArgs e)
        {
            if (mnuLevel15.Checked)
            {
                mnuTo19750901.Checked = false;
                mnu19760101.Checked = false;
                mnuLevel10.Checked = false;
                mnuLevel20.Checked = false;
                mnuLevel25.Checked = false;
                mnuLevel40.Checked = false;


                if (renderer != null)
                {
                    renderer.PresentationLevel = 1.5;
                    layers = renderer.Render(formPage);

                    if (charMap.BackgroundImage != null)
                    {
                        RenderCharMap();

                        RenderItemToCharMap();
                    }
                }
            }
        }

        private void mnuLevel10_CheckedChanged(object sender, EventArgs e)
        {
            if (mnuLevel10.Checked)
            {
                mnuTo19750901.Checked = false;
                mnu19760101.Checked = false;
                mnuLevel15.Checked = false;
                mnuLevel20.Checked = false;
                mnuLevel25.Checked = false;
                mnuLevel40.Checked = false;

                if (renderer != null)
                {
                    renderer.PresentationLevel = 1;

                    layers = renderer.Render(formPage);

                    if (charMap.BackgroundImage != null)
                    {

                        RenderCharMap();

                        RenderItemToCharMap();
                    }
                }
            }
        }


        private void mnuLevel20_CheckStateChanged(object sender, EventArgs e)
        {
            if (mnuLevel20.Checked)
            {
                mnuTo19750901.Checked = false;
                mnu19760101.Checked = false;
                mnuLevel10.Checked = false;
                mnuLevel25.Checked = false;
                mnuLevel15.Checked = false;
                mnuLevel40.Checked = false;


                if (renderer != null)
                {
                    renderer.PresentationLevel = 2.0;


                    if (charMap.BackgroundImage != null)
                    {
                        layers = renderer.Render(formPage);

                        if (charMap.BackgroundImage != null)
                        {
                            RenderCharMap();

                            RenderItemToCharMap();
                        }
                    }
                }

            }
        }

        private void mnuLevel25_CheckedChanged(object sender, EventArgs e)
        {
            if (mnuLevel25.Checked)
            {
                mnuTo19750901.Checked = false;
                mnu19760101.Checked = false;
                mnuLevel10.Checked = false;
                mnuLevel20.Checked = false;
                mnuLevel15.Checked = false;
                mnuLevel40.Checked = false;


                if (renderer != null)
                {
                    renderer.PresentationLevel = 2.5;
                    layers = renderer.Render(formPage);

                    if (charMap.BackgroundImage != null)
                    {
                        RenderCharMap();

                        RenderItemToCharMap();
                    }
                }
            }
        }

        private void mnuLevel40_CheckedChanged(object sender, EventArgs e)
        {
            if (mnuLevel40.Checked)
            {
                mnuTo19750901.Checked = false;
                mnu19760101.Checked = false;
                mnuLevel10.Checked = false;
                mnuLevel20.Checked = false;
                mnuLevel15.Checked = false;
                mnuLevel25.Checked = false;


                if (renderer != null)
                {
                    renderer.PresentationLevel = 4;
                    layers = renderer.Render(formPage);

                    if (charMap.BackgroundImage != null)
                    {
                        RenderCharMap();

                        RenderItemToCharMap();
                    }
                }
            }
        }
        private void MnuObserveParity_CheckStateChanged(object sender, EventArgs e)
        {
            service.DoParityCheck = mnuObserveParity.Checked;
        }

        private void cbReveal_CheckedChanged(object sender, EventArgs e)
        {
            renderer.RevealPressed = !renderer.RevealPressed;
            layers = renderer.Render(formPage);

            using (Graphics g = Graphics.FromImage(charMap.BackgroundImage))
            {

                RenderCharMap();
            }
                //g.Dispose();

            charMap.Invalidate();
            charMap.Update();
            charMap.Refresh();
        }

        private void mnuShowBorders_CheckedChanged(object sender, EventArgs e)
        {
            borders = mnuShowBorders.Checked;
            //charMap.DeallocateCursor();
            string nationalOptionSelectionBits = "";
            double presentationLevel = 0;
            string font = "";
            bool revealPressed = false;
            bool rendererExisted = false;

            if (renderer != null)
            {
                nationalOptionSelectionBits = renderer.NationalOptionSelectionBits;
                presentationLevel = renderer.PresentationLevel;
                font = renderer.Font;
                revealPressed = renderer.RevealPressed;
                rendererExisted = true;
            }
            renderer = new TeletextRenderer(borders);
            if (rendererExisted)
            {
                renderer.NationalOptionSelectionBits = nationalOptionSelectionBits;
                renderer.PresentationLevel = presentationLevel;
                renderer.Font = font;
                renderer.RevealPressed = revealPressed;
                RenderItemToCharMap();
                
            }
            renderer.DeviceDPI = this.DeviceDpi;
            //layers = renderer.Render(formPage);
            //charMap.Borders = borders;
            //charMap.InitCursor();        
        }


        private void cbMix_CheckedChanged(object sender, EventArgs e)
        {
            if (cbMix.Checked)
            {
                renderer.Mix = true;
                RenderItemToCharMap();
            }
            else
            {
                renderer.Mix = false;
                RenderItemToCharMap();
            }
        }

        private void cbFlash_CheckedChanged(object sender, EventArgs e)
        {
            if (cbFlash.Checked)
                flash.Start();
            else
                flash.Stop();
        }

        private void cbSuppress_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSuppress.Checked)
            {
                renderer.Suppress = true;
                RenderItemToCharMap();
            }
            else
            {
                renderer.Suppress = false;
                RenderItemToCharMap();
            }
        }


        #endregion

        #region Context_Menu_Item_Clicks
        private void AddNewSubpageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewSubpage(formPage);
        }

        private void AddNewSubpage(Page template = null)
        {
            Page newPage = new Page();
            Int32 key = lvThumbnails.Items.Count;

            // Add a header from the template if supplied
            if (template == null)
            {
                template = new Page();
                template.Clear();
                String magPage = recoveredPages.SelectedItem.ToString().Substring(0, recoveredPages.SelectedItem.ToString().Length >= 3 ? 3 : recoveredPages.SelectedItem.ToString().Length);
                magPage = magPage.ValidateMagPage();
                template.Lines[0].Text = "         HEADER MPP DAY DD MTH  HH:MM/SS";
                template.Lines[0].MagPage = magPage;
                template.Lines[0].TimeCode = "00:00";
                template.Lines[0].Magazine = Convert.ToInt32(magPage.Substring(0, 1));
                template.Lines[0].Page = magPage.Substring(1, 2);
                template.Lines[0].Flags = new ControlFlags();
                template.Lines[0].Bytes = new byte[42];
                template.Lines[0].CalcHammingCodes();

            }


            // Set default header text from other frames
            newPage.Lines[0].Text = template.Lines[0].Text;
            newPage.Lines[0].MagPage = template.Lines[0].MagPage;
            newPage.Lines[0].TimeCode = template.Lines[0].TimeCode;
            newPage.Lines[0].Magazine = template.Lines[0].Magazine;
            newPage.Lines[0].Page = template.Lines[0].Page;
            newPage.Lines[0].Flags = template.Lines[0].Flags;
            newPage.Lines[0].MRAG1 = template.Lines[0].MRAG1;
            newPage.Lines[0].MRAG2 = template.Lines[0].MRAG2;
            newPage.Lines[0].Bytes = template.Lines[0].Bytes;

            // Render image to thumbnail
            layers = renderer.Render(newPage);

            Bitmap thumbnail = new Bitmap(layers.Background);

            Graphics g = Graphics.FromImage(thumbnail);

            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, thumbnail.Width, thumbnail.Height);

            g.DrawImage(layers.Background, 0, 0);
            g.DrawImage(layers.Foreground, 0, 0);

            //ilThumbnails.Images.Add(key.ToString(), thumbnail);
            //ListViewItem lvItem = lvThumbnails.Items.Add("Key: " + key.ToString());
            //lvItem.Name = key.ToString();
            //lvItem.ImageKey = key.ToString();

            // Add Page object to hashtable for this recovery file
            recoveries.Add(key, newPage);

            g.Dispose();

            //lvThumbnails.Invalidate();
            //lvThumbnails.Refresh();

            refreshThumbnails();

            changesMade = true;
        }

        private void ClearCarouselToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear the carousel?  This will erase all sub-pages.", "Teletext Recovery Editor", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // Clear thumbnails

                lvThumbnails.Clear();
                ilThumbnails.Images.Clear();
                recoveries.Clear();

                // Now add a new subpage

                AddNewSubpage();

                changesMade = true; redrawThumbnails = true;
            }
        }

        private void RemoveAllButThoseWithMostPacketsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Initialise the subpage map.  This stores the relationship between the packets on the different pages in the 
            // carousel before subpages are assigned to each page.  This prevents duplicate comparisons being made if two
            // packets have already been compared, and gives us something to assess which pages belong to which subpage.


            var trimmer = new TrimCarousel();
            recoveries = trimmer.Trim(recoveries);


            //recoveries = newRecoveries;

            changesMade = true; redrawThumbnails = true;
            refreshRecoveries();
            refreshThumbnails();
        }

        private void MoveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Page> newPages = new List<Page>();
            foreach (ListViewItem l in lvThumbnails.Items)
            {
                newPages.Add((Page)recoveries[Convert.ToInt32(l.Index)]);
            }

            foreach (ListViewItem toBeMoved in lvThumbnails.SelectedItems)
            {
                newPages.Insert(toBeMoved.Index - 1, (Page)recoveries[toBeMoved.Index]);
                newPages.RemoveAt(toBeMoved.Index + 1);
            }

            Hashtable newRecoveries = new Hashtable();
            for (int n = 0; n < newPages.Count; n++)
                newRecoveries.Add(n, newPages[n]);

            recoveries = newRecoveries;
            refreshRecoveries();
            refreshThumbnails();

            changesMade = true; redrawThumbnails = true;
        }

        private void MoveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Page> newPages = new List<Page>();
            foreach (ListViewItem l in lvThumbnails.Items)
            {
                newPages.Add((Page)recoveries[l.Index]);
            }

            //for (int n = 0; n < newPages.Count; n++)
            //{
            //    System.Diagnostics.Debug.WriteLine(n + "  " + newPages[n].Lines[2].Text);
            //}
            //System.Diagnostics.Debug.WriteLine("");


            foreach (ListViewItem toBeMoved in lvThumbnails.SelectedItems)
            {
                newPages.Insert(toBeMoved.Index + 2, (Page)recoveries[toBeMoved.Index]);

                //for (int n = 0; n < newPages.Count; n++)
                //{
                //    System.Diagnostics.Debug.WriteLine(n + "  " + newPages[n].Lines[2].Text);
                //}
                //System.Diagnostics.Debug.WriteLine("");

                newPages.RemoveAt(toBeMoved.Index);
            }

            //for (int n = 0; n < newPages.Count; n++)
            //{
            //    System.Diagnostics.Debug.WriteLine(n + "  " + newPages[n].Lines[2].Text);
            //}
            //System.Diagnostics.Debug.WriteLine("");

            // I think this inverses the items in the hash table to be the right way around, can't remember now
            Hashtable newRecoveries = new Hashtable();
            for (int n = 0; n < newPages.Count; n++)
                newRecoveries.Add(n, newPages[n]);

            recoveries = newRecoveries;
            refreshRecoveries();
            refreshThumbnails();

            changesMade = true; redrawThumbnails = true;
        }
        private void moveToTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Make copy of items to work on
            List<Page> newPages = new List<Page>();
            foreach (ListViewItem l in lvThumbnails.Items)
            {
                newPages.Add((Page)recoveries[Convert.ToInt32(l.Index)]);
            }

            // Move selected items
            foreach (ListViewItem toBeMoved in lvThumbnails.SelectedItems)
            {
                newPages.RemoveAt(toBeMoved.Index);
                newPages.Insert(0, (Page)recoveries[toBeMoved.Index]);
            }

            // Copy items back into main table
            Hashtable newRecoveries = new Hashtable();
            for (int n = 0; n < newPages.Count; n++)
                newRecoveries.Add(n, newPages[n]);

            recoveries = newRecoveries;
            refreshRecoveries();
            refreshThumbnails();

            changesMade = true; redrawThumbnails = true;
        }

        private void moveToBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Make copy of items to work on
            List<Page> newPages = new List<Page>();
            foreach (ListViewItem l in lvThumbnails.Items)
            {
                newPages.Add((Page)recoveries[Convert.ToInt32(l.ImageKey)]);
            }

            // Move selected items
            foreach (ListViewItem toBeMoved in lvThumbnails.SelectedItems)
            {
                newPages.RemoveAt(toBeMoved.Index);
                newPages.Insert(newPages.Count, (Page)recoveries[toBeMoved.Index]);
            }

            // Copy items back into main table
            Hashtable newRecoveries = new Hashtable();
            for (int n = 0; n < newPages.Count; n++)
                newRecoveries.Add(n, newPages[n]);

            recoveries = newRecoveries;
            refreshRecoveries();
            refreshThumbnails();

            changesMade = true; redrawThumbnails = true;
        }

        private void toolStripMenuItem1pkt_Click(object sender, EventArgs e)
        {
            RemoveSubpagesWithNFramesMissing(e, 1);
        }

        private void toolStripMenuItem2pkt_Click(object sender, EventArgs e)
        {
            RemoveSubpagesWithNFramesMissing(e, 2);
        }

        private void toolStripMenuItem3pkt_Click(object sender, EventArgs e)
        {
            RemoveSubpagesWithNFramesMissing(e, 3);
        }

        private void toolStripMenuItem4pkt_Click(object sender, EventArgs e)
        {
            RemoveSubpagesWithNFramesMissing(e, 4);
        }

        private void toolStripMenuItem5pkts_Click(object sender, EventArgs e)
        {
            RemoveSubpagesWithNFramesMissing(e, 5);
        }

        private void toolStripMenuItem6pkt_Click(object sender, EventArgs e)
        {
            RemoveSubpagesWithNFramesMissing(e, 6);
        }

        private void toolStripMenuItem7pkt_Click(object sender, EventArgs e)
        {
            RemoveSubpagesWithNFramesMissing(e, 7);
        }

        private void toolStripMenuItem8pkt_Click(object sender, EventArgs e)
        {
            RemoveSubpagesWithNFramesMissing(e, 8);
        }

        private void toolStripMenuItem9pkt_Click(object sender, EventArgs e)
        {
            RemoveSubpagesWithNFramesMissing(e, 9);
        }

        private void toolStripMenuItem10pkt_Click(object sender, EventArgs e)
        {
            RemoveSubpagesWithNFramesMissing(e, 10);
        }

        private void removeFromCarouselToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Hashtable newRecoveries;
            //newRecoveries = (Hashtable)recoveries.Clone();


            // Copy recoveries array to another array, omitting the key if in the selecteditems list

            foreach (ListViewItem l in lvThumbnails.Items)
            {
                // is this item in the selected list?
                Boolean selected = false;
                foreach (ListViewItem m in lvThumbnails.SelectedItems)
                {
                    if (l.ImageKey == m.ImageKey)
                        selected = true;


                    // if not, copy to newRecoveries
                    if (selected)
                    {
                        //newRecoveries.Add(l.Index, (Page)recoveries[l]);
                        recoveries.Remove(Convert.ToInt32(l.ImageKey));
                    }
                }
            }

            changesMade = true; redrawThumbnails = true;
            refreshRecoveries();
            refreshThumbnails();
        }

        private void removeCheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hashtable newRecoveries;
            newRecoveries = (Hashtable)recoveries.Clone();


            // Copy recoveries array to another array, omitting the key if in the checkeditems list

            foreach (ListViewItem m in lvThumbnails.CheckedItems)
            {
                //if (l.ImageKey == m.ImageKey)
                newRecoveries.Remove(Convert.ToInt32(m.ImageKey));
            }


            recoveries = newRecoveries;

            // Remove the deleted pages from the thumbnails too
            Int32 i = 0;
            while (i < lvThumbnails.Items.Count)
            {
                if (lvThumbnails.Items[i].Checked)
                {
                    lvThumbnails.Items.RemoveAt(i);
                    ilThumbnails.Images.RemoveAt(i);
                    i = 0;
                }
                else
                    i++;
            }

            changesMade = true; redrawThumbnails = true;
            refreshRecoveries();
            refreshThumbnails();
        }

        private void removeAllButCheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hashtable newRecoveries;
            newRecoveries = (Hashtable)recoveries.Clone();



            // Copy recoveries array to another array, omitting the key if in the checkeditems list

            foreach (ListViewItem m in lvThumbnails.Items)
            {
                if (!m.Checked)
                    newRecoveries.Remove(Convert.ToInt32(m.Index));
            }


            recoveries = newRecoveries;

            // Remove the deleted pages from the thumbnails too

            //Int32 i = 0;
            //while (i < lvThumbnails.Items.Count)
            //{
            //    if (!lvThumbnails.Items[i].Checked)
            //    {
            //        lvThumbnails.Items.RemoveAt(i);
            //        ilThumbnails.Images.RemoveAt(i);
            //        i = 0;
            //    }
            //    else
            //        i++;
            //}

            changesMade = true; redrawThumbnails = true;
            refreshRecoveries();
            refreshThumbnails();
        }

        private void RemoveSubpagesWithNFramesMissing(EventArgs e, Int32 numPkts)
        {
            Hashtable newRecoveries;
            newRecoveries = (Hashtable)recoveries.Clone();



            // If there are too many packets missing from a subpage then tick them
            foreach (ListViewItem m in lvThumbnails.Items)
            {
                Page loopPage = (Page)recoveries[Convert.ToInt32(m.ImageKey)];
                Int32 missingPackets = 0;
                for (Int32 pkt = 0; pkt < 25; pkt++)
                {
                    if (loopPage.Lines[pkt].Type == LineTypes.Blank)
                        missingPackets++;
                }
                if (missingPackets >= numPkts)
                {
                    lvThumbnails.Items[m.Index].Checked = true;
                }
                else
                {
                    lvThumbnails.Items[m.Index].Checked = false;
                }
            }


            //Remove the ticked pages
            foreach (ListViewItem m in lvThumbnails.Items)
            {
                if (m.Checked)
                    newRecoveries.Remove(Convert.ToInt32(m.Index));
            }


            recoveries = newRecoveries;

            // Remove the deleted pages from the thumbnails too
            //Int32 i = 0;
            //while (i < lvThumbnails.Items.Count)
            //{
            //    if (lvThumbnails.Items[i].Checked)
            //    {
            //        lvThumbnails.Items.RemoveAt(i);
            //        ilThumbnails.Images.RemoveAt(i);
            //        i = 0;
            //    }
            //    else
            //        i++;
            //}

            refreshRecoveries();
            refreshThumbnails();
            changesMade = true; redrawThumbnails = true;

        }

        #endregion

        private void charMap_DoubleClick(object sender, EventArgs e)
        {
            if (layers != null)
            {
                if (renderer.PresentationLevel > 1.5)
                {
                    Mode m = formPage.modeMapL2[charMap.cursorY, charMap.cursorX];
                    System.Diagnostics.Debug.WriteLine(m.CharacterSet + ", $" + m.Character.toHex(2));

                    frmEnhanced.modeMapL2 = formPage.modeMapL2;
                    frmEnhanced.dgAttributes.Rows.Clear();
                    frmEnhanced.dgAttributes.Rows.Add("Cursor X", charMap.cursorX);
                    frmEnhanced.dgAttributes.Rows.Add("Cursor Y", charMap.cursorY);
                    frmEnhanced.dgAttributes.Rows.Add("Character Set", m.CharacterSet);
                    frmEnhanced.dgAttributes.Rows.Add("Character Code", "$" + m.Character.toHex(2));
                    frmEnhanced.dgAttributes.Rows.Add("Foreground Colour", "CLUT: " + m.ForeCLUT + ", Code: " + m.ForeColourCode + ", RGB: " + renderer.ColourMap[m.ForeCLUT, m.ForeColourCode].ToString("X3"));
                    frmEnhanced.dgAttributes.Rows.Add("Background Colour", "CLUT: " + m.BgndCLUT + ", Code: " + m.BgndColourCode + ", RGB: " + renderer.ColourMap[m.BgndCLUT, m.BgndColourCode].ToString("X3"));
                    frmEnhanced.ValueChanged += new EventHandler(frmEnhanced_ValueChanged);
                    frmEnhanced.Show();
                    frmEnhanced.Focus();
                }
            }
        }


        private void mnuSearch_Click(object sender, EventArgs e)
        {

        }

        private void tbTimeCode_TextChanged(object sender, EventArgs e)
        {
            if (tbTimeCode.Text.Length == 5)
            {
                formPage.Lines[0].TimeCode = tbTimeCode.Text;
                formPage.Lines[0].CalcHammingCodes();
            }
            //changesMade = true;
        }


        #region Radio_Buttons

        private void rbC4_Click(object sender, EventArgs e)
        {
            rbC4.Checked = !rbC4.Checked;
            formPage.Lines[0].Flags.C4_Erase = rbC4.Checked;
            formPage.Lines[0].CalcHammingCodes();
            RenderItemToCharMap();
            changesMade = true;
        }

        private void rbC5_Click(object sender, EventArgs e)
        {
            rbC5.Checked = !rbC5.Checked;
            formPage.Lines[0].Flags.C5_Newsflash = rbC5.Checked;
            formPage.Lines[0].CalcHammingCodes();
            RenderItemToCharMap();
            changesMade = true;
        }

        private void rbC6_Click(object sender, EventArgs e)
        {
            rbC6.Checked = !rbC6.Checked;
            formPage.Lines[0].Flags.C6_Subtitle = rbC6.Checked;
            formPage.Lines[0].CalcHammingCodes();
            RenderItemToCharMap();
            changesMade = true;
        }

        private void rbC7_Click(object sender, EventArgs e)
        {
            rbC7.Checked = !rbC7.Checked;
            formPage.Lines[0].Flags.C7_SuppressHeader = rbC7.Checked;
            formPage.Lines[0].CalcHammingCodes();
            RenderItemToCharMap();
            changesMade = true;
        }

        private void rbC8_Click(object sender, EventArgs e)
        {
            rbC8.Checked = !rbC8.Checked;
            formPage.Lines[0].Flags.C8_Update = rbC8.Checked;
            formPage.Lines[0].CalcHammingCodes();
            RenderItemToCharMap();
            changesMade = true;
        }

        private void rbC9_Click(object sender, EventArgs e)
        {
            rbC9.Checked = !rbC9.Checked;
            formPage.Lines[0].Flags.C9_InterruptedSequence = rbC9.Checked;
            formPage.Lines[0].CalcHammingCodes();
            RenderItemToCharMap();
            changesMade = true;
        }

        private void rbC10_Click(object sender, EventArgs e)
        {
            rbC10.Checked = !rbC10.Checked;
            formPage.Lines[0].Flags.C10_InhibitDisplay = rbC10.Checked;
            formPage.Lines[0].CalcHammingCodes();
            RenderItemToCharMap();
            changesMade = true;
        }

        private void rbC11_Click(object sender, EventArgs e)
        {
            rbC11.Checked = !rbC11.Checked;
            formPage.Lines[0].Flags.C11_MagazineSerial = rbC11.Checked;
            formPage.Lines[0].CalcHammingCodes();
            RenderItemToCharMap();
            changesMade = true;
        }

        private void rbC12_Click(object sender, EventArgs e)
        {
            rbC12.Checked = !rbC12.Checked;
            formPage.Lines[0].Flags.C12 = rbC12.Checked;
            formPage.Lines[0].CalcHammingCodes();
            RenderItemToCharMap();
            changesMade = true;
        }

        private void rbC13_Click(object sender, EventArgs e)
        {
            rbC13.Checked = !rbC13.Checked;
            formPage.Lines[0].Flags.C13 = rbC13.Checked;
            formPage.Lines[0].CalcHammingCodes();
            RenderItemToCharMap();
            changesMade = true;
        }

        private void rbC14_Click(object sender, EventArgs e)
        {
            rbC14.Checked = !rbC14.Checked;
            formPage.Lines[0].Flags.C14 = rbC14.Checked;
            formPage.Lines[0].CalcHammingCodes();
            RenderItemToCharMap();
            changesMade = true;
        }

        #endregion


        private void flash_Tick(object sender, EventArgs e)
        {
            bool changed = false;
            if (formPage != null && cbFlash.Checked)
            {

                flashRatePhaseSlow++;
                flashRatePhaseSlow %= 12;

                //flashRatePhaseFast++;
                //flashRatePhaseFast %= 4;

                //if (flashRatePhaseFast % 2 == 0)
                //{
                flashPhase++;
                flashPhase %= 3;
                //}


                for (int y = 0; y < 25; y++)
                {
                    for (int x = 0; x < 40; x++)
                    {
                        Byte pIndex = formPage.GetPacketIndex(y);
                        if (pIndex != 255)
                        {
                            if (formPage.Lines[pIndex].Bytes[x] == 0x08)

                            {
                                renderer.FlashTextOn = (flashRatePhaseSlow > 2 ? true : false);
                                layers = renderer.RenderLine(formPage, y, layers);
                                changed = true;
                            }

                            // X/26 normal flash
                            if (formPage.modeMapL2[y, x].FlashMode == "01")
                            {
                                int initialDoubleWidthX = formPage.modeMapL2[y, x].DoubleWidth ? 1 : -1;
                                renderer.FlashTextOn = (flashRatePhaseSlow > 2 ? true : false);
                                renderer.RenderL2Character(ref formPage, ref layers, ref initialDoubleWidthX, x, y, false);
                                changed = true;
                            }

                            // X/26 Three phase flash
                            if (formPage.modeMapL2[y, x].FlashMode == "11" && formPage.modeMapL2[y, x].FlashRateAndPhase != "000" && formPage.modeMapL2[y, x].FlashRateAndPhase.StartsWith("0"))
                            {
                                var a = formPage.modeMapL2[y, x].FlashRateAndPhase;
                                var b = Convert.ToInt32(formPage.modeMapL2[y, x].FlashRateAndPhase.Substring(1, 2), 2);

                                int currentCLUT = formPage.modeMapL2[y, x].ForeCLUT;
                                if (flashPhase + 1 == Convert.ToInt32(formPage.modeMapL2[y, x].FlashRateAndPhase.Substring(1, 2), 2))
                                {

                                    switch (currentCLUT)
                                    {
                                        /*case 0:
                                            {
                                                formPage.modeMapL2[y, x].ForeCLUT = 1;
                                                break;
                                            }*/
                                        case 1:
                                            {
                                                formPage.modeMapL2[y, x].ForeCLUT = 0;
                                                break;
                                            }
                                        /*case 2:
                                            {
                                                formPage.modeMapL2[y, x].ForeCLUT = 3;
                                                break;
                                            }*/
                                        case 3:
                                            {
                                                formPage.modeMapL2[y, x].ForeCLUT = 2;
                                                break;
                                            }
                                    }

                                }
                                else
                                {
                                    switch (currentCLUT)
                                    {
                                        case 0:
                                            {
                                                formPage.modeMapL2[y, x].ForeCLUT = 1;
                                                break;
                                            }
                                        /*case 1:
                                            {
                                                formPage.modeMapL2[y, x].ForeCLUT = 0;
                                                break;
                                            }*/
                                        case 2:
                                            {
                                                formPage.modeMapL2[y, x].ForeCLUT = 3;
                                                break;
                                            }
                                            /*case 3:
                                                {
                                                    formPage.modeMapL2[y, x].ForeCLUT = 2;
                                                    break;
                                                }*/
                                    }
                                }

                                // find initialDoubleWidthX (must get rid of this)
                                int initialDoubleWidthX = -1;
                                for (int x1 = 0; x1 < 40 && initialDoubleWidthX == -1; x1++)
                                {
                                    if (formPage.modeMapL2[y, x1].DoubleWidth || formPage.modeMapL2[y, x1].DoubleSize)
                                        initialDoubleWidthX = x1;
                                }
                                renderer.RenderL2Character(ref formPage, ref layers, ref initialDoubleWidthX, x, y, false);
                                changed = true;

                            }
                        }

                    }
                }
            }

            if (changed)
            {
                if (backdrop != "")
                {
                    Image bgImage = new Bitmap(backdrop);
                    charMap.BackgroundImage = new Bitmap(bgImage, borders ? 700 : 480, borders ? 600 : 500);
                }
                else
                {
                    charMap.BackgroundImage = new Bitmap(borders ? 700 : 480, borders ? 600 : 500, PixelFormat.Format32bppArgb);
                }

                //charMap.Image = new Bitmap(borders ? 700 : 480, borders ? 600 : 500, PixelFormat.Format32bppArgb);

                RenderCharMap();

                //charMap.BackgroundImage.Save(Environment.GetEnvironmentVariable("temp") + "\\teletext\\charmapBGafter.png");
            }

            if (cbFlash.Checked)
            {
                flash.Interval = 150;
                flash.Start();
            }
            else
                flash.Stop();
        }


        private void charMap_SizeChanged(object sender, EventArgs e)
        {
            if (charMap.Height > 0)
                charMap.Width = Convert.ToInt32(charMap.Height * 1.333);
        }


        private void frmEnhanced_ValueChanged(object sender, EventArgs e)
        {

            layers = renderer.Render(formPage);
            //RenderL2Character(ref formPage, ref layers, 0, charMap.cursorX, charMap.cursorY);
        }

        private void tbPosition_Leave(object sender, EventArgs e)
        {
            try
            {
                if (service.ServiceType == ServiceType.Service && changesMade)
                    saveCarousel();
                long p = Convert.ToInt64(tbPosition.Text, 16);
                long rounded = Convert.ToInt64(Math.Floor((double)p / 42) * 42);
                service.Position = rounded;
                RenderThumbnails(formPage.Lines[0].Magazine, formPage.Lines[0].Page);
                RenderItemToCharMap(currentlySelectedThumbnail);
            }
            catch
            {
                MessageBox.Show("Invalid position supplied.  Please provide a valid hexadecimal number.", "Teletext Recovery Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void subtitlesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSubtitles es = new ExportSubtitles();
            es.Service = service;
            es.Page = formPage;
            es.Show();
        }

        private void syncSubtitlesToSRTFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SyncSubtitles es = new SyncSubtitles();
            es.Service = service;
            es.Show();
        }

        private void generateSubtitlesFromSRTFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateSubtitles gs = new GenerateSubtitles();
            gs.Show();
        }

        private void selectImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image files (JPEG)|*.jpg|(PNG)|*.png|(BMP)|*.bmp";
            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName != "")
            {
                if (File.Exists(openFileDialog1.FileName))
                {
                    backdrop = openFileDialog1.FileName;
                    try
                    {
                        Image bgImage = new Bitmap(backdrop);
                        charMap.BackgroundImage = new Bitmap(bgImage, borders ? 700 : 480, borders ? 600 : 500);
                        if (layers != null)
                        {
                            Graphics g = Graphics.FromImage(charMap.BackgroundImage);

                            //g.DrawImage(layers.L25Background, 0, 0);
                            g.DrawImage(layers.Background, 0, 0, borders ? 700 : 480, borders ? 600 : 500);
                            g.DrawImage(layers.Foreground, 0, 0, borders ? 700 : 480, borders ? 600 : 500);
                            g.Dispose();
                            MessageBox.Show("Background image loaded.", "Teletext Recovery Editor", MessageBoxButtons.OK);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("There was a problem loading the background image.", "Teletext Recovery Editor", MessageBoxButtons.OK);
                        backdrop = "";
                    }
                }

                else
                    MessageBox.Show("File does not exist.", "Teletext Recovery Editor", MessageBoxButtons.OK);
            }
        }

        private void clearImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            charMap.BackgroundImage = new Bitmap(borders ? 700 : 480, borders ? 600 : 500, PixelFormat.Format32bppArgb);
            backdrop = "";
            if (layers != null)
            {
                RenderCharMap();
            }
        }

        private void RenderCharMap()
        {
            using (Graphics g = Graphics.FromImage(charMap.BackgroundImage))
            {

                //g.DrawImage(layers.L25Background, 0, 0);

                // Retrieve the DPI of the display device
                float dpiX, dpiY;
                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    dpiX = graphics.DpiX;
                    dpiY = graphics.DpiY;
                }

                // Calculate the scaling factor
                float scaleX = dpiX / 96f;
                float scaleY = dpiY / 96f;

                // Apply scaling to the rendering
                g.DrawImage(layers.Background, 0, 0, layers.Background.Width / scaleX, layers.Background.Height / scaleY);
                g.DrawImage(layers.Foreground, 0, 0, layers.Foreground.Width / scaleX, layers.Foreground.Height / scaleY);
            }
        }

        private String FormatPosition(long position)
        {
            try
            {
                return ("$" + "00000000".Substring(0, 8 - Convert.ToString(position, 16).Length)
                    + Convert.ToString(position, 16)).ToUpper();
            }
            catch
            {
                return "$########";
            }
        }


    }



    struct subPageMeta
    {
        public Int32 Key;
        public Int32 Count;
        public Double Percentage;
    }

}
