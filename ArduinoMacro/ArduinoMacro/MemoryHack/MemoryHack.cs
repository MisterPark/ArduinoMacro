using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace ArduinoMacro
{
    public enum AsyncScanType
    {
        ScanAll,
        ScanNext
    }
    public enum ScanType
    {
        Equal,
        NotEqual,
        End,
    }

    public enum VariableType
    {
        Default,
        Byte,
        Byte2,
        Byte4,
        Byte8,
        Float,
        Double,
        String,
        End,
    }

    public class MemoryHack
    {
        #region dll import
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
             uint processAccess,
             bool bInheritHandle,
             int processId
        );

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000,
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);
        #endregion

        private static MemoryHack instance = new MemoryHack();
        public static MemoryHack Instance { get { return instance; } }

        private const int PROCESS_WM_READ = 0x0010;

        private Process process = null;
        private IntPtr handle = IntPtr.Zero;
        private IntPtr baseAddress = IntPtr.Zero;

        private Thread scanThread;
        private Action callback;
        
        public object ScanValue { get; private set; }
        public AsyncScanType AsyncScanType { get; private set; }
        public ScanType ScanType { get; set; } = ScanType.Equal;
        public VariableType VariableType { get; set; } = VariableType.Default;
        public float Progress { get; private set; }
        public bool IsScanning { get; private set; } = false;
        public Process CurrentProcess
        {
            get
            {
                return process;
            }
            set
            {
                process = value;
                handle = OpenProcess(PROCESS_WM_READ, false, process.Id);
                baseAddress = process.MainModule.BaseAddress;
            }
        }

        public List<ScanElement> ScanElements = new List<ScanElement>();

        private MemoryHack()
        {

        }

        public static IntPtr OpenProcess(Process proc, ProcessAccessFlags flags)
        {
            return OpenProcess((uint)flags, false, proc.Id);
        }

        private static void ScanThread()
        {
            switch (instance.AsyncScanType)
            {
                case AsyncScanType.ScanAll:
                    instance.ScanAll(instance.ScanValue);
                    break;
                case AsyncScanType.ScanNext:
                    instance.ScanNext(instance.ScanValue);
                    break;
                default:
                    break;
            }

            instance.IsScanning = false;
            instance.Progress = 0;
            instance?.callback();
        }

        public void ScanAll(object value, Action callback)
        {
            if (IsScanning) return;

            IsScanning = true;
            AsyncScanType = AsyncScanType.ScanAll;
            this.callback = callback;
            ScanValue = value;

            instance.scanThread = new Thread(ScanThread);
            instance.scanThread.Start();
        }

        public void ScanNext(object value, Action callback)
        {
            if (IsScanning) return;

            IsScanning = true;
            AsyncScanType = AsyncScanType.ScanNext;
            this.callback = callback;
            ScanValue = value;

            instance.scanThread = new Thread(ScanThread);
            instance.scanThread.Start();
        }

        private void ScanAll(object value)
        {
            if (CurrentProcess == null) return;
            

            IntPtr pointer = baseAddress;
            int memorySize = process.MainModule.ModuleMemorySize;
            int unitSize = VariableType.GetMemorySize();
            byte[] buffer = new byte[unitSize];
            byte[] buf = value.GetBytes();
            if (buf == null)
            {
                MessageBox.Show("잘못된 타입.");
                return;
            }
            if (buffer.Length != buf.Length)
            {
                MessageBox.Show("일치하지 않는 타입.");
                return;
            }

            ScanElements.Clear();

            for (int i = 0; i < memorySize; i += unitSize)
            {
                ReadProcessMemory(handle, pointer, buffer, buffer.Length, out IntPtr bytesRead);

                switch (ScanType)
                {
                    case ScanType.Equal:
                        if (buffer.CompareTo(buf))
                        {
                            long offset = (long)pointer - (long)baseAddress;
                            ScanElements.Add(new ScanElement(pointer, offset, value));
                        }
                        break;
                    case ScanType.NotEqual:
                        if (buffer.CompareTo(buf) == false)
                        {
                            long offset = (long)pointer - (long)baseAddress;
                            ScanElements.Add(new ScanElement(pointer, offset, value));
                        }
                        break;
                    case ScanType.End:
                        break;
                    default:
                        break;
                }
                

                pointer = IntPtr.Add(pointer, buffer.Length);
                Progress = i / (float)memorySize;
            }
        }

        private void ScanNext(object value)
        {
            if (CurrentProcess == null) return;

            IntPtr pointer = baseAddress;
            int memorySize = process.MainModule.ModuleMemorySize;
            int unitSize = VariableType.GetMemorySize();
            byte[] buffer = new byte[unitSize];
            byte[] buf = value.GetBytes();
            if (buf == null)
            {
                MessageBox.Show("잘못된 타입.");
                return;
            }
            if (buffer.Length != buf.Length)
            {
                MessageBox.Show("일치하지 않는 타입.");
                return;
            }

            List<int> removeIndices = new List<int>();
            int count = ScanElements.Count;
            for (int i = 0; i < count; i++)
            {
                ScanElement element = ScanElements[i];
                pointer = element.Address;

                ReadProcessMemory(handle, pointer, buffer, buffer.Length, out IntPtr bytesRead);

                object readValue = null;

                switch (VariableType)
                {
                    case VariableType.Byte:
                        readValue = (byte)buffer[0];
                        break;
                    case VariableType.Byte2:
                        readValue = BitConverter.ToUInt16(buffer, 0);
                        break;
                    case VariableType.Byte8:
                        readValue = BitConverter.ToUInt64(buffer, 0);
                        break;
                    case VariableType.Float:
                        readValue = BitConverter.ToSingle(buffer, 0);
                        break;
                    case VariableType.Double:
                        readValue = BitConverter.ToDouble(buffer, 0);
                        break;
                    case VariableType.Byte4:
                    case VariableType.Default:
                    case VariableType.End:
                    default:
                        readValue = BitConverter.ToInt32(buffer, 0);
                        break;
                }


                switch (ScanType)
                {
                    case ScanType.Equal:
                        if (buffer.CompareTo(buf) == false)
                        {
                            removeIndices.Add(i);
                        }
                        break;
                    case ScanType.NotEqual:
                        if (buffer.CompareTo(buf))
                        {
                            removeIndices.Add(i);
                        }
                        else
                        {
                            element.Previous = element.Value;
                            element.Value = readValue;
                        }
                        break;
                    case ScanType.End:
                        break;
                    default:
                        break;
                }
                

                Progress = (i+1) / (float)count;
            }

            int removeCount = removeIndices.Count;
            for (int i = removeCount - 1; i >= 0; i--)
            {
                int index = removeIndices[i];
                ScanElements.RemoveAt(index);
            }
        }
    }
}
