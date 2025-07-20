
using System.Data;
using System.Text.RegularExpressions;
using TeletextSharedResources;

namespace Teletext
{
    public partial class ExportSubtitles : Form
    {
        private Service service;
        public Service Service
        {

            get { return service; }
            set { service = value; }

        }

        private Page formPage;
        public Page Page
        {
            get { return formPage; }
            set { formPage = value; }
        }

        public ExportSubtitles()
        {
            InitializeComponent();
        }

        private void btnChooseSrtFile_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Subtitle Files (.srt)|*.srt";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbOutputFile.Text = saveFileDialog1.FileName;
                btnGo.Enabled = true;
            }
        }

        public async void exportSubtitles(string Filename, bool outputPNGs = false, string Resolution = "PAL 4:3 768 x 576")
        {
            await Task.Run(() =>
            {
                Regex reg = new Regex("[0-8][A-F,0-9][A-E,0-9]");
                Match match = reg.Match(mtbSubsPage.Text);
                if (match.Success)
                {
                    string subsPage = mtbSubsPage.Text.Substring(0, 1) == "8" ? "0" + mtbSubsPage.Text.Substring(1) : mtbSubsPage.Text;
                    int targetX = 0;
                    int targetY = 0;

                    switch (Resolution)
                    {
                        case "PAL 4:3 768 x 576":
                            {
                                targetX = 768;
                                targetY = 576;
                                break;
                            };
                        case "HD 16:9 720":
                            {
                                targetX = 1280;
                                targetY = 720;
                                break;
                            };
                        case "HD 16:9 1080":
                            {
                                targetX = 1920;
                                targetY = 1080;
                                break;
                            }

                    }

                    string saveFolder = Path.GetDirectoryName(Filename);

                    if (File.Exists(Filename))
                        File.Delete(Filename);

                    long origPosition = service.Position;

                    service.Reset();

                    //service.Position = 0x2759EE;
                    int offsetSeconds = 0;
                    offsetSeconds = 55;

                    long filenumber = 0;
                    double time = 0;

                    string subtitleFrame = "";
                    string lastContents = "";
                    string strLastTime = "00:00:00,000";

                    bool entryOpen = false;

                    TeletextRenderer renderer = new TeletextRenderer(true);

                    while (service.Revolutions == 0 && (cbAllFrames.Checked || filenumber < numFrames.Value))
                    {
                        //if (filenumber == 360)
                        //    System.Diagnostics.Debug.Write("Hey");

                        // Get page from Service object
                        long thisPagePosition = service.Position;
                        formPage = service.GetPage(subsPage);
                        System.Diagnostics.Debug.WriteLine("Page service posn: " + (service.Position - 42));

                        if (formPage.Lines[0].Flags.C7_SuppressHeader)
                            formPage.Lines[0].Text = "";

                        time = (double)(Convert.ToDouble(thisPagePosition / service.PacketSize) / 32D / 25D);

                        // Is there anything in this page apart from a header?
                        bool anyContent = false;
                        string contents = "";
                        bool eraseHeaderOnly = false;
                        int eraseHeaderOnlyCount = 0;

                        var formPageSorted = formPage.Lines.OrderBy(x => x.Row);

                        foreach (Line l in formPageSorted)
                        {
                            if (l.Type == LineTypes.Line)
                            {
                                string text = Regex.Replace(l.Text, "[\x00-\x1f ]+", " ");
                                //if (l.Text.Replace(" ", "") != "")
                                if (text.Replace(" ", "") != "")
                                {
                                    anyContent = true;
                                    eraseHeaderOnly = false;
                                    contents += text;
                                }
                                else
                                    System.Diagnostics.Debug.WriteLine("whitespace blank");
                            }
                            else if (l.Type == LineTypes.Header && (l.Flags.C4_Erase && l.Flags.C6_Subtitle))
                            {
                                anyContent = true;
                                eraseHeaderOnly = true;
                                eraseHeaderOnlyCount++;
                            }
                        }

                        if (anyContent)
                        //if ((anyContent && !eraseHeaderOnly ) || eraseHeaderOnlyCount > 1)// && !eraseHeaderOnly)
                        {
                            //eraseHeaderOnlyCount = 0;
                            // Convert time to the .SRT format
                            TimeSpan tsTime = TimeSpan.FromSeconds(time - offsetSeconds < 0 ? 0 : time - offsetSeconds);

                            string strTime = string.Format("{0:D2}:{1:D2}:{2:D2},{3:D3}",
                                tsTime.Hours,
                                tsTime.Minutes,
                                tsTime.Seconds,
                                tsTime.Milliseconds);

                            // Close the last record if there was one
                            if (entryOpen)
                            {

                                string pngFilename = filenumber.ToString().PadLeft(8, Convert.ToChar("0")) + ".png";
                                string fullpath = saveFolder + "\\" + pngFilename;

                                if (outputPNGs)
                                    lastContents = fullpath;

                                subtitleFrame = filenumber + "\r\n" + strLastTime + " --> " + strTime + "\r\n" + lastContents.Replace("  ", " ") + "\r\n";

                                async Task WriteEntry()
                                {
                                    using (StreamWriter sw = File.AppendText(saveFileDialog1.FileName))
                                    {
                                        await sw.WriteLineAsync(subtitleFrame);
                                    }
                                }
                                WriteEntry();

                                if (outputPNGs)
                                {
                                    // Render image to png
                                    RenderedLayersNova layers;
                                    layers = renderer.Render(formPage);

                                    // Combine layers to form final image
                                    Bitmap subtitle = new Bitmap(layers.Background);
                                    using (Graphics g = Graphics.FromImage(subtitle))
                                    {
                                        g.DrawImage(layers.Foreground, 0, 0);
                                    }

                                    // We don't want anti-aliasing, so multiply the bitmap up an integer number of times
                                    double origY = Convert.ToDouble(subtitle.Height);

                                    int scale = (int)Math.Floor(Convert.ToDouble(targetY) / origY);
                                    scale = scale == 0 ? 1 : scale;
                                    if (targetY == 1080) scale = 2;

                                    Bitmap subtitleOut = new Bitmap(targetX, targetY);
                                    using (Graphics g = Graphics.FromImage(subtitleOut))
                                    {
                                        g.DrawImage(subtitle,
                                            (targetX - subtitle.Width * scale) / 2,
                                            (targetY - subtitle.Height * scale) / 2,
                                            subtitle.Width * scale, subtitle.Height * scale);
                                    };

                                    subtitleOut.Save(fullpath);

                                }
                                filenumber++;
                            }





                            //subtitleFrame = filenumber.ToString() + "\r\n" + strLastTime + " --> " + strTime + "\r\n" + filename + "\r\n";
                            //subtitleFrame = filenumber.ToString() + "\r\n" + strLastTime + " --> " + strTime + "\r\n" + contents + "\r\n";
                            // subtitleFrame = filenumber.ToString() + "\r\n" + strTime + " --> ";
                            //subtitleFrame = filenumber.ToString() + "\r\n" + strTime + " --> ";
                            //

                            strLastTime = strTime;
                            lastContents = contents;
                            entryOpen = true;

                        }
                        //else
                        //{
                        //    Line nextLine = service.GetNextLine();
                        //    if (nextLine.Type != LineTypes.Header)
                        //        service.Position = service.Position - service.PacketSize;
                        //}





                        //async Task WriteEntry()
                        //{
                        //    using (StreamWriter file = new("WriteLines2.txt", append: true)) ;
                        //    await file.WriteLineAsync("Fourth line");
                        //}

                        //if (lastTime == -1)
                        //    lastTime = 0;
                    }
                    service.Position = origPosition;
                }
                else
                {
                    MessageBox.Show("Invalid page number", "Teletext Recovery Editor");
                }
            });
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            exportSubtitles(tbOutputFile.Text, !cbTextOnly.Checked, cbResolution.Text);
        }

        private void cbAllFrames_CheckedChanged(object sender, EventArgs e)
        {
            if (cbAllFrames.Checked)
                numFrames.Enabled = false;
            else
                numFrames.Enabled = true;

        }
    }


}
