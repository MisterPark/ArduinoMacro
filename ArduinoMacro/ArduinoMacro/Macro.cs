﻿using System;
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

namespace ArduinoMacro
{

    public class Macro
    {
        private static Macro instance = new Macro();

        public static Macro Instance { get { return instance; } }

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
    }
}
