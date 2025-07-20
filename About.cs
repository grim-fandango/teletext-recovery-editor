using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Teletext
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void richTextBox1_VisibleChanged(object sender, EventArgs e)
        {
            richTextBox1.Text = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "ReadMe.txt");
        }
    }
}
