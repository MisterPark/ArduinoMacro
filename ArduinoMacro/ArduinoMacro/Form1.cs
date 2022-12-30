using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using ArduinoMacro.Commands;
using System.IO;

namespace ArduinoMacro
{
    public partial class Form1 : Form
    {
        List<Func<MacroCommandArgs>> dialogMethods = new List<Func<MacroCommandArgs>>();

        Timer timer = new Timer();

        public Form1()
        {
            InitializeComponent();

            timer.Interval = 20;
            timer.Tick += Log;
            timer.Start();

            dialogMethods.Add(ShowDelay);
            dialogMethods.Add(ShowKey);
            dialogMethods.Add(ShowKey);
            dialogMethods.Add(ShowKey);

            int count = Macro.Instance.CommandList.Count;
            for (int i = 0; i < count; i++)
            {
                var item = Macro.Instance.CommandList[i];
                item.ShowDialog = dialogMethods[i];
                listBox1.Items.Add(item);
            }
        }

        private void comboBox1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            foreach (var item in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(item);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox1.Text)) return;
            
            try
            {
                if(Macro.Instance.IsConnected)
                {
                    Macro.Instance.Disconnect();
                }
                else
                {
                    Macro.Instance.PortName = comboBox1.SelectedItem.ToString();
                    Macro.Instance.BaudRate = 9600;
                    Macro.Instance.DataBits = 8;
                    Macro.Instance.StopBits = StopBits.One;
                    Macro.Instance.Parity = Parity.None;
                    Macro.Instance.Connect();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            button1.Text = Macro.Instance.IsConnected ? "Disconnect" : "Connect";
            comboBox1.Enabled = !Macro.Instance.IsConnected;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(Macro.Instance.IsRunning)
            {
                Macro.Instance.Stop();
                button2.BackColor = Color.Red;
                button2.Text = "On";
            }
            else
            {
                button2.BackColor = Color.LimeGreen;
                button2.Text = "Off";
                Macro.Instance.Start();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IMacroCommand command = (IMacroCommand)listBox1.SelectedItem;
            MacroCommandArgs args = null;
            if(command.ShowDialog != null)
            {
                args = command.ShowDialog();
            }

            var macroCommand = Macro.Instance.AddCommand(listBox1.SelectedIndex, args);
            listBox2.Items.Add(macroCommand);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox2.Items.Count == 0) return;
            Macro.Instance.RemoveCommand(listBox2.SelectedIndex);
            listBox2.Items.RemoveAt(listBox2.SelectedIndex);
        }

        private MacroCommandArgs ShowDelay()
        {
            var args = new Commands.MacroCommandArgs();
            DelayForm delayForm = new DelayForm();
            if (delayForm.ShowDialog() == DialogResult.OK)
            {
                args.Enqueue(delayForm.Millisecond);
            }
            else
            {
                args.Enqueue(0);
            }
            return args;
        }

        private MacroCommandArgs ShowKey()
        {
            var args = new Commands.MacroCommandArgs();
            KeyForm form = new KeyForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                args.Enqueue(form.Key);
            }
            else
            {
                args.Enqueue(0);
            }
            return args;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Debug button
        }

        private void Log(object sender, EventArgs e)
        {
            if (Macro.Instance.logs.Count == 0) return;

            string log = string.Empty;
            if(Macro.Instance.logs.TryDequeue(out log))
            {
                LogTextBox.Text += log + "\n";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Macro.Instance.Disconnect();
        }

        private void button6_Click(object sender, EventArgs e) // Save
        {
            string path = Directory.GetCurrentDirectory() + "\\" + "macro.json";
            Macro.Instance.Save(path);
        }

        private void button7_Click(object sender, EventArgs e) // Load
        {
            string path = Directory.GetCurrentDirectory() + "\\" + "macro.json";
            Macro.Instance.Load(path);

            listBox2.Items.Clear();
            foreach (var item in Macro.Instance.Commands)
            {
                listBox2.Items.Add(item);
            }
            
        }
    }
}
