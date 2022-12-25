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
    public partial class KeyForm : Form
    {
        public MacroKey Key { get; set; }
        public KeyForm()
        {
            InitializeComponent();
            int count = MacroKeys.Keys.Length;
            for (byte i = 0; i < count; i++)
            {
                MacroKey key = MacroKeys.Keys[i];
                comboBox1.Items.Add(key);
            }
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Key = (MacroKey)comboBox1.SelectedItem;
            this.DialogResult = DialogResult.OK;
        }
    }
}
