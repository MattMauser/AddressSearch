using System;
using System.Windows.Forms;

namespace AddressSearch
{
    public partial class Form1 : Form
    {
        public string ReturnValue1 { get; set; }

        public Form1(AutoCompleteStringCollection AddressSearch)
        {
            InitializeComponent();
            textBox1.AutoCompleteCustomSource = AddressSearch;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.ReturnValue1 = textBox1.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
