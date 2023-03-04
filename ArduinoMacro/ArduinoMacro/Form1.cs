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
using System.Diagnostics;
using System.Reflection;

namespace ArduinoMacro
{
    public partial class Form1 : Form
    {
        // ========================
        // 매크로
        // ========================
        List<Func<MacroCommandArgs>> dialogMethods = new List<Func<MacroCommandArgs>>();

        Timer logTimer = new Timer();

        // ========================
        // 메모리 핵
        // ========================
        Process process = null;

        Timer scanTimer = new Timer();
        bool callbackFlag = false;

        public Form1()
        {
            InitializeComponent();

            logTimer.Interval = 20;
            logTimer.Tick += Log;
            logTimer.Start();

            scanTimer.Interval = 20;
            scanTimer.Tick += ScanProgressTimer;
            scanTimer.Start();

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

            for (int i = 0; i < (int)ScanType.End; i++)
            {
                scanTypeComboBox.Items.Add((ScanType)i);
            }
            scanTypeComboBox.SelectedIndex = 0;

            for (int i = 0; i < (int)VariableType.End; i++)
            {
                valueTypeComboBox.Items.Add((VariableType)i);
            }
            valueTypeComboBox.SelectedIndex = 0;
        }

        private void comboBox1_Click(object sender, EventArgs e) // 아두이노 포트 불러오기
        {
            comboBox1.Items.Clear();
            foreach (var item in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(item);
            }
        }

        private void button1_Click(object sender, EventArgs e) // 아두이노 연결
        {
            if (string.IsNullOrEmpty(comboBox1.Text)) return;

            try
            {
                if (Macro.Instance.IsConnected)
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            button1.Text = Macro.Instance.IsConnected ? "Disconnect" : "Connect";
            comboBox1.Enabled = !Macro.Instance.IsConnected;
        }

        private void button2_Click(object sender, EventArgs e) // 매크로 on/off
        {
            if (Macro.Instance.IsRunning)
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

        private void button3_Click(object sender, EventArgs e) // 매크로 리스트 추가
        {
            IMacroCommand command = (IMacroCommand)listBox1.SelectedItem;
            MacroCommandArgs args = null;
            if (command.ShowDialog != null)
            {
                args = command.ShowDialog();
            }

            var macroCommand = Macro.Instance.AddCommand(listBox1.SelectedIndex, args);
            listBox2.Items.Add(macroCommand);
        }

        private void button4_Click(object sender, EventArgs e) // 매크로 리스트 삭제
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

        private void button5_Click(object sender, EventArgs e) // 디버그 버튼
        {
            sbyte a = -10;
            object value = (object)a;
            Type type = value.GetType();
            byte b = (byte)((sbyte)value);
            MessageBox.Show(b.ToString());
        }

        private void Log(object sender, EventArgs e)
        {
            if (Macro.Instance.logs.Count == 0) return;

            string log = string.Empty;
            if (Macro.Instance.logs.TryDequeue(out log))
            {
                LogTextBox.Text += log + "\n";
            }
        }

        private void ScanProgressTimer(object sender, EventArgs e)
        {
            progressBar1.Value = (int)(MemoryHack.Instance.Progress * 100);
            if(callbackFlag)
            {
                callbackFlag = false;
                ScanTimerCallback();
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

        private void button8_Click(object sender, EventArgs e) // 프로세스 선택
        {
            ProcessForm form = new ProcessForm();
            int processId;

            if (form.ShowDialog() == DialogResult.OK)
            {
                processId = form.Result;
                process = Process.GetProcessById(processId);
                processNameLabel.Text = process.ProcessName + ".exe";
                MemoryHack.Instance.CurrentProcess = process;
            }
        }

        private void button9_Click(object sender, EventArgs e) // 전체 스캔
        {
            if (process == null)
            {
                MessageBox.Show("프로세스를 선택하세요.");
                return;
            }

            Type type = null;
            object value = null;
            if (CheckInputValue(ref type, ref value) == false)
            {
                return;
            }

            MemoryHack.Instance.ScanType = (ScanType)scanTypeComboBox.SelectedIndex;
            MemoryHack.Instance.VariableType = (VariableType)valueTypeComboBox.SelectedIndex;

            MemoryHack.Instance.ScanAll(value, ScanCallback);
        }

        private void button10_Click(object sender, EventArgs e) // 다음 스캔
        {
            if (process == null)
            {
                MessageBox.Show("프로세스를 선택하세요.");
                return;
            }

            Type type = null;
            object value = null;
            if (CheckInputValue(ref type, ref value) == false)
            {
                return;
            }

            MemoryHack.Instance.ScanType = (ScanType)scanTypeComboBox.SelectedIndex;
            MemoryHack.Instance.VariableType = (VariableType)valueTypeComboBox.SelectedIndex;

            MemoryHack.Instance.ScanNext(value, ScanCallback);
        }

        private bool CheckInputValue(ref Type type, ref object value)
        {
            ScanType scanType = (ScanType)scanTypeComboBox.SelectedIndex;
            VariableType varialbeType = (VariableType)valueTypeComboBox.SelectedIndex;
            string valueString = valueTextBox.Text;

            if (hexCheckBox.Checked)
            {
                if (valueString.IsHexDigit() == false)
                {
                    MessageBox.Show("잘못된 값 입력입니다.1");
                    return false;
                }
                else
                {
                    if (valueString.Length > varialbeType.GetHexaStringLength())
                    {
                        MessageBox.Show("최대 값의 범위를 넘었습니다.");
                        return false;
                    }

                    // Hex 처리

                }
            }

            if (varialbeType.Validate(valueString, ref type, ref value) == false)
            {
                MessageBox.Show("잘못된 값 입력입니다.2");
                return false;
            }


            return true;
        }

        private void ScanCallback()
        {
            callbackFlag = true;
        }

        private void ScanTimerCallback()
        {
            int count = MemoryHack.Instance.ScanElements.Count;
            scanListView.Items.Clear();
            for (int i = 0; i < count; i++)
            {
                ScanElement element = MemoryHack.Instance.ScanElements[i];

                scanListView.Items.Add(element.ToListViewItem());
            }
            scanListView.Refresh();
        }
    }
}
