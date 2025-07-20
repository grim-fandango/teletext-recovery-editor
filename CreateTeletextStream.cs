
using TeletextSharedResources;

namespace Teletext
{
    public partial class frmCreateTeletextStream : Form
    {
        public frmCreateTeletextStream()
        {
            InitializeComponent();
        }

        private void btnGenerateTextStream_Click(object sender, EventArgs e)
        {
            int[] carousels = new int[8 * 255];
            // Loop list of files, load carousel details to an array, i.e. how many pages in the carousel, which subpage was last added
            foreach (string s in Program.frmMain.recoveredPages.Items)
            {
                //Load file, count headers
                Service srvHeaders = new Service();
                srvHeaders.OpenService(s);
                srvHeaders.Reset();

                long posn = -1;
                int headerCount = 0;
                Line lineNext = new Line();
                while (srvHeaders.Position > posn)
                {
                    posn = srvHeaders.Position;
                    lineNext = srvHeaders.GetNextHeader();
                    if (lineNext.Bytes != null)
                        headerCount++;
                }
                
            }

            

            // Loop array, open first carousel file, navigate to the next carousel page as per the array, read it and dump it to the stream file

        }
    }
}
