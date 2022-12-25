using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoMacro
{
    public partial class DelayForm : Form
    {
        public int Millisecond { get; set; }
        public DelayForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Millisecond = (int)numericUpDown1.Value;
            DialogResult= DialogResult.OK;
        }
    }
}
