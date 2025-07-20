using System;
using System.Windows.Forms;

namespace Teletext
{
    public partial class frmBase64UrlImport : Form
    {
        public frmBase64UrlImport()
        {
            InitializeComponent();
        }

        private void btnBase64UrlImport_Click(object sender, EventArgs e)
        {
            string strData;
            // Remove http etc. if it's been included
            if (tbBase64Url.Text.Contains("http"))
            {
                strData = tbBase64Url.Text.Substring(tbBase64Url.Text.IndexOf("#") + 3);
            }
            else
                strData = tbBase64Url.Text;

            // Get byte array of ASCII values
            //byte[] asciiBytes = Encoding.ASCII.GetBytes(strData);

            string strBinary = "";
            string decodeString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            foreach (char chr in strData)
            {
                strBinary += Convert.ToString(decodeString.IndexOf(chr), 2).PadLeft(6, Convert.ToChar("0"));
                //strBinary += Convert.ToString(Convert.ToByte(posn), 2);
                //strBinary += Convert.ToString(n, 2).Substring(2);
            }

            // Remove junk at end
            strBinary = strBinary.Substring(0, 7000);

            // chop up binary string into 
            byte[] bytDecodedPage = new byte[1000];
            string clipText = "";
            for (int n=0; n<1000; n++)
            {
                bytDecodedPage[n] = Convert.ToByte(strBinary.Substring(n * 7, 7), 2);

                if (n % 40 == 0 && n > 0)
                    clipText += System.Environment.NewLine;
                clipText += (Char)bytDecodedPage[n];
                
            }
            Clipboard.SetText(clipText);
            this.Hide();
            
        }
    }
}
