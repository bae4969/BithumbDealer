using System;
using System.Windows.Forms;

namespace BithumbDealer.form
{
    public partial class savingMsg : Form
    {
        public savingMsg()
        {
            InitializeComponent();
        }
        private void text_focus_disable(object sender, EventArgs e)
        {
            textBox3.Focus();
        }
    }
}
