using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace ArduinoMacro
{
    public partial class ProcessForm : Form
    {
        public int Result { get; private set; }
        public ProcessForm()
        {
            InitializeComponent();
            Initialize();
        }

        public void Initialize()
        {
            Process[] processes = Process.GetProcesses();

            int count = processes.Length;
            for (int i = 0; i < count; i++)
            {
                ListViewItem item = new ListViewItem(processes[i].ProcessName);
                item.SubItems.Add(processes[i].Id.ToString());
                ProcessListView.Items.Add(item);
            }

            
        }

        private int CompareProcessByName(Process process1, Process process2)
        {
            return string.Compare(process1.ProcessName, process2.ProcessName);
        }

        private int CompareProcessById(Process process1, Process process2)
        {
            if(process1.Id == process2.Id)
            {
                return 0;
            }
            else if(process1.Id > process2.Id)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        private void ProcessListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int index = e.Column;

            List<Process> processes = Process.GetProcesses().ToList();
            int count = processes.Count;

            switch (index)
            {
                case 0:
                    processes.Sort(CompareProcessByName);
                    break;
                case 1:
                    processes.Sort(CompareProcessById);
                    break;
            }

            ProcessListView.Items.Clear();

            for (int i = 0; i < count; i++)
            {
                ListViewItem item = new ListViewItem(processes[i].ProcessName);
                item.SubItems.Add(processes[i].Id.ToString());
                ProcessListView.Items.Add(item);
            }
        }


        private void ProcessListView_DoubleClick(object sender, EventArgs e)
        {
            string processId = ProcessListView.SelectedItems[0].SubItems[1].Text;
            Result = int.Parse(processId);
            
            DialogResult = DialogResult.OK;
        }
    }
}
