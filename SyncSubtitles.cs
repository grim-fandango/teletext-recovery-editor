using TeletextSharedResources;

namespace TeletextRecoveryEditor
{
    public partial class SyncSubtitles : Form
    {
        private Service service;
        public Service Service
        {

            get { return service; }
            set { service = value; }

        }
        public SyncSubtitles()
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

                    service.Reset();

                    do
                    {
                        string strSubNumber = sr.ReadLine();
                        string strSubTimeSpan = sr.ReadLine();
                        string strSubtitle = sr.ReadLine();
                        string nextSubtitle;
                        do
                        {
                            nextSubtitle = sr.ReadLine();
                            if (nextSubtitle == null)
                                nextSubtitle = "";
                            if (nextSubtitle.Length > 0)
                                strSubtitle += nextSubtitle;
                        } while (nextSubtitle.Length > 0);

 
                        try
                        {
                            int t42PageNumber = Convert.ToInt32(strSubNumber);

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

                            // Get next page
                            Page p = service.GetPage();
                            foreach (Line line in p.Lines) 
                            {
                                if (line.Type != LineTypes.Blank && line.Type != LineTypes.Unknown)
                                    fs.Write(line.Bytes, 0, line.Bytes.Length);
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
