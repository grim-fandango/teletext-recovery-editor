using TeletextSharedResources;

namespace Teletext
{
    public partial class EnhancedTextAttributes : Form
    {
        public Mode[,] modeMapL2 = new Mode[40, 25];

        public EnhancedTextAttributes()
        {
            InitializeComponent();
        }

        // Define new event
        public event EventHandler ValueChanged;

        private void dgAttributes_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Int32 x = Convert.ToInt32(dgAttributes.Rows[0].Cells[1].Value);
                Int32 y = Convert.ToInt32(dgAttributes.Rows[1].Cells[1].Value);
                System.Diagnostics.Debug.WriteLine(dgAttributes.Rows[e.RowIndex].Cells[0].Value);
                System.Diagnostics.Debug.WriteLine(dgAttributes.Rows[e.RowIndex].Cells[1].Value);



                if ((String)dgAttributes.Rows[e.RowIndex].Cells[0].Value == "Character Code")
                {
                    String hexValue = dgAttributes.Rows[e.RowIndex].Cells[1].Value.ToString().Substring(dgAttributes.Rows[e.RowIndex].Cells[1].Value.ToString().Length - 2, 2);
                    modeMapL2[y, x].Character = Convert.ToByte(hexValue, 16);
                
                }

                // Trigger custom event
                ValueChanged(this, e);    
            }
        }
    }
}
