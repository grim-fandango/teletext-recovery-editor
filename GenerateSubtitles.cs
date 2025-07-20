using System.Text;
using TeletextSharedResources;


namespace TeletextRecoveryEditor
{
    public partial class GenerateSubtitles : Form
    {
        private Service service;
        public Service Service
        {

            get { return service; }
            set { service = value; }

        }
        public GenerateSubtitles()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Subtitle Files (.srt)|*.srt";
            openFileDialog1.CheckFileExists = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbSourceFile.Text = openFileDialog1.FileName;
                if (tbSourceFile.Text.Length > 4 && tbTargetFile.Text.Length > 4)
                    btnGo.Enabled = true;
            }
        }

        private void btnOpenTarget_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "T42 Files (.t42)|*.t42";
            openFileDialog1.CheckFileExists = false;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbTargetFile.Text = openFileDialog1.FileName;
                if (tbSourceFile.Text.Length > 4 && tbTargetFile.Text.Length > 4)
                    btnGo.Enabled = true;
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            byte[] blankLine = new byte[42];
            byte[] eraseLine = { 0x15, 0x15, 0xD0, 0xD0, 0x15, 0xD0, 0x15, 0xD0, 0x2F, 0x15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            if (File.Exists(tbTargetFile.Text))
            {
                if(MessageBox.Show(tbTargetFile.Text + " already exists - do you want to overwrite?", "Teletext Recovery Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    File.Delete(tbTargetFile.Text);
            }

            using (StreamReader sr = new StreamReader(tbSourceFile.Text))
            {
                using (FileStream fs = new FileStream(tbTargetFile.Text, FileMode.CreateNew, FileAccess.Write))
                {
                    TimeSpan tsLastTime = TimeSpan.Parse("00:00:00.00");
                    fs.Write(eraseLine, 0, 42);

                    do
                    {
                        string strSubNumber = sr.ReadLine();
                        string strSubTimeSpan = sr.ReadLine();
                        string nextSubtitle;

                        List<string> subtitles = new List<string>();
                        do
                        {
                            nextSubtitle = sr.ReadLine();
                            if (nextSubtitle == null)
                                nextSubtitle = "";
                            if (nextSubtitle.Length > 0)
                                subtitles.Add(nextSubtitle);
                        } 
                        while (nextSubtitle.Length > 0);

 
                        try
                        {
                            //int t42PageNumber = Convert.ToInt32(strSubNumber);

                            string[] strTimes = strSubTimeSpan.Replace(" --", "").Replace(" ", "").Replace(",", ".").Split(Convert.ToChar(">"));
                            TimeSpan tsStart = TimeSpan.Parse(strTimes[0]);
                            TimeSpan tsEnd = TimeSpan.Parse(strTimes[1]);

                            // if there is a gap from the last time noted, fill it with zeroes
                            if ((tsStart - tsLastTime).TotalSeconds > 0)
                            {
                                int numStartBlankLines = (int)((tsStart - tsLastTime).TotalSeconds * 25 * 32);
                                for (int n = 0; n < numStartBlankLines; n++)
                                    fs.Write(blankLine, 0, 42);
                            }

                            // Write subtitle
                            Page p = new Page();
                            Line header = new Line();
                            header.Clear();
                            header.Text = "CEEFAX 888";
                            header.Row = 0;
                            header.MagPage = "888";
                            header.Magazine = 0;
                            header.Page = "88";
                            header.TimeCode = "00:00";
                            header.Type = LineTypes.Header;
                            header.Flags.C4_Erase = true;
                            header.Flags.C6_Subtitle = true;

                            byte[] headerBytes = Encoding.ASCII.GetBytes(header.Text);
                            byte[] h = eraseLine;
                            Array.Copy(headerBytes, 0, h, 10, headerBytes.Length);
                            header.Bytes = h;

                            header.CalcParity();
                            header.CalcHammingCodes();

                            p.Lines[0] = header;

                            if (subtitles.Count > 0)
                            {
                                int subCount = 0;
                                foreach (string subtitle in subtitles)
                                {
                                    Line l = new Line();
                                    int row = 24 - (subtitles.Count * 2) + (subCount * 2);

                                    l.Text = subtitle;
                                    l.Row = row;
                                    l.Type = LineTypes.Line;

                                    int start = (40 - subtitle.Length) / 2 - 4 + 2;
                                    byte[] subtitleBytes = Encoding.ASCII.GetBytes(subtitle);

                                    byte[] b = new byte[42];
                                    byte[] controlCodesPrefix = { 0x0d, 0x06, 0x0b, 0x0b };
                                    byte[] controlCodesSuffix = { 0x0a, 0x0a };

                                    Array.Copy(controlCodesPrefix, 0, b, start, controlCodesPrefix.Length);
                                    Array.Copy(subtitleBytes, 0, b, start + controlCodesPrefix.Length, subtitleBytes.Length);
                                    if (start + controlCodesPrefix.Length + subtitleBytes.Length + controlCodesSuffix.Length < 42)
                                        Array.Copy(controlCodesSuffix, 0, b, start + controlCodesPrefix.Length + subtitleBytes.Length, controlCodesSuffix.Length);

                                    l.Bytes = b;
                                    l.CalcHammingCodes();

                                    p.Lines[row] = l;

                                    row = row + 2;
                                    subCount++;
                                }

                                foreach (Line l in p.Lines)
                                    if (l.Type != LineTypes.Blank && l.Type != LineTypes.Unknown)
                                        fs.Write(l.Bytes, 0, 42);
                            }

                            // Write blanks for the duration of the subtitle
                            int numBlankLines = (int)((tsEnd - tsStart).TotalSeconds * 25 * 32);
                            for (int n = 0; n < numBlankLines; n++)
                                fs.Write(blankLine, 0, 42);

                            tsLastTime = tsEnd;
                        }
                        catch { }

                    } while (!sr.EndOfStream);
                }

            }


        }


    }
}
