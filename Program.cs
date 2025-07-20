using System;
using System.Windows.Forms;

namespace Teletext
{

    static class Program
    {
        public static TeletextRecoveryEditor frmMain;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
         
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(frmMain = new TeletextRecoveryEditor());
        }
    }
}
