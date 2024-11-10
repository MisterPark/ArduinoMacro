using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;
using ArduinoMacro.Commands;
using System.Threading;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace ArduinoMacro
{

    public class Macro
    {
        private static Macro instance = new Macro();

        public static Macro Instance { get { return instance; } }

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte vk, byte scan, int flags, ref int extrainfo);

        const byte VK_LBUTTON = 0x01;
        const int KEYUP = 0x0002;
        int Info = 0;

        private SerialPort serialPort = new SerialPort();

        public bool IsConnected { get { return serialPort.IsOpen; } }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Parity Parity { get; set; }
        public bool IsRunning { get; private set; } = false;

        public int RunningCommand { get; set; }

        List<IMacroCommand> commandList = new List<IMacroCommand>();

        List<IMacroCommand> commands = new List<IMacroCommand>();

        public IReadOnlyList<IMacroCommand> CommandList { get { return commandList.AsReadOnly(); } }
        public IReadOnlyList<IMacroCommand> Commands { get { return commands.AsReadOnly(); } }

        Thread thread = new Thread(Execute);

        public ConcurrentQueue<string> logs = new ConcurrentQueue<string>();

        private Macro()
        {
            commandList.Add(new DelayCommand());
            commandList.Add(new KeyPressCommand());
            commandList.Add(new KeyDownCommand());
            commandList.Add(new KeyUpCommand());
        }

        public void Connect()
        {
            if (serialPort.IsOpen) return;

            serialPort.PortName = PortName;
            serialPort.BaudRate = BaudRate;
            serialPort.DataBits = DataBits;
            serialPort.Parity = Parity;
            serialPort.StopBits = StopBits;
            serialPort.Open();

            SerialBuffer buffer = new SerialBuffer();
            buffer.Write((byte)Protocol.On);
            buffer.Write((byte)0);
            serialPort.Write(buffer.Buffer, buffer.Offset, buffer.Count);
        }

        public void Disconnect()
        {
            if (serialPort.IsOpen == false) return;

            SerialBuffer buffer = new SerialBuffer();
            buffer.Write((byte)Protocol.Off);
            buffer.Write((byte)0);
            serialPort.Write(buffer.Buffer, buffer.Offset, buffer.Count);

            serialPort.Close();
        }

        public void KeyDown(MacroKey key)
        {
            if (!serialPort.IsOpen) return;

            SerialBuffer buffer = new SerialBuffer();
            buffer.Write((byte)Protocol.KeyDown);
            buffer.Write((byte)key);

            serialPort.Write(buffer.Buffer, buffer.Offset, buffer.Count);
        }

        public void KeyUp(MacroKey key)
        {
            if (!serialPort.IsOpen) return;

            SerialBuffer buffer = new SerialBuffer();
            buffer.Write((byte)Protocol.KeyUp);
            buffer.Write((byte)key);

            serialPort.Write(buffer.Buffer, buffer.Offset, buffer.Count);
        }
        public void KeyPress(MacroKey key)
        {
            if (!serialPort.IsOpen) return;

            SerialBuffer buffer = new SerialBuffer();
            buffer.Write((byte)Protocol.KeyPress);
            buffer.Write((byte)key);

            serialPort.Write(buffer.Buffer, buffer.Offset, buffer.Count);
        }

        public void KeyDownVirtual(byte virtualKey)
        {
            int info = 0;
            keybd_event(virtualKey, 0, 0, ref info);
        }

        public void KeyUpVirtual(byte virtualKey)
        {
            int info = 0;
            keybd_event(virtualKey, 0, KEYUP, ref info);
        }

        private static void Execute()
        {
            while (instance.IsRunning)
            {
                int count = instance.commands.Count;
                for (int i = 0; i < count; i++)
                {
                    if (instance.IsRunning == false) break;
                    instance.RunningCommand = i;
                    Instance.logs.Enqueue(instance.commands[i].ToString());
                    instance.commands[i].Execute();
                }
            }

            return;
        }

        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            thread = new Thread(Execute);
            thread.Start();
        }

        public void Stop()
        {
            if (IsRunning == false) return;

            SerialBuffer buffer = new SerialBuffer();
            buffer.Write((byte)Protocol.Off);
            buffer.Write((byte)0);
            serialPort.Write(buffer.Buffer, buffer.Offset, buffer.Count);

            IsRunning = false;
            thread.Join();
        }

        public IMacroCommand AddCommand(int command, MacroCommandArgs args)
        {
            Type type = CommandList[command].GetType();
            IMacroCommand macroCommand = (IMacroCommand)Activator.CreateInstance(type);
            macroCommand.Args = args;
            commands.Add(macroCommand);
            return macroCommand;
        }

        public IMacroCommand AddCommand(int at, int command, MacroCommandArgs args)
        {
            Type type = CommandList[command].GetType();
            IMacroCommand macroCommand = (IMacroCommand)Activator.CreateInstance(type);
            macroCommand.Args = args;
            commands[at] = macroCommand;
            return macroCommand;
        }

        public void RemoveCommand(int inedx)
        {
            commands.RemoveAt(inedx);
        }

        public void Save(string path)
        {
            MacroData data = new MacroData();
            int count = commands.Count;
            for (int i = 0; i < count; i++)
            {
                data.datas.Add(new CommandData(commands[i]));
            }

            string json = JsonConvert.SerializeObject(data);

            File.WriteAllText(path, json);
        }

        public void Load(string path) 
        {
            string json = File.ReadAllText(path);
            MacroData data = JsonConvert.DeserializeObject<MacroData>(json);
            if(data == null)
            {
                MessageBox.Show("매크로 데이터 불러오기 실패.");
                return;
            }

            commands.Clear();
            int count = data.datas.Count;
            for (int i = 0; i < count; i++)
            {
                var command = data.datas[i].ToCommand();
                commands.Add(command);
            }
        }
    }
}
