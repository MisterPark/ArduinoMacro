using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoMacro
{
    public class SerialBuffer
    {
        private byte[] buffer;
        private int memorySize;
        private int read;
        private int write;

        public byte[] Buffer { get { return buffer; } }
        public int Count { get { return write - read; } }
        public int Writable { get { return memorySize - write; } }
        public int Readable { get { return write - read; } }

        public int Written { get { return write; } }
        public int Offset { get { return read; } }

        public SerialBuffer()
        {
            buffer = new byte[1024];
            memorySize = 1024;
            read = 0;
            write = 0;
        }

        private void Upsize()
        {
            byte[] temp = new byte[memorySize * 2];
            System.Buffer.BlockCopy(buffer,0,temp,0, memorySize);
            buffer = temp;
            memorySize = memorySize * 2;
        }

        public void Write(bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (write + count > memorySize)
            {
                Upsize();
            }
            System.Buffer.BlockCopy(bytes, 0, buffer, write, count);
            write += count;
        }

        public void Write(byte value)
        {
            byte[] bytes = new byte[1];
            bytes[0] = value;
            int count = bytes.Length;
            if(write + count > memorySize)
            {
                Upsize();
            }
            System.Buffer.BlockCopy(bytes, 0, buffer, write, count);
            write += count;
        }

        public void Write(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (write + count > memorySize)
            {
                Upsize();
            }
            System.Buffer.BlockCopy(bytes, 0, buffer, write, count);
            write += count;
        }

        public void Write(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (write + count > memorySize)
            {
                Upsize();
            }
            System.Buffer.BlockCopy(bytes, 0, buffer, write, count);
            write += count;
        }

        public void Write(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (write + count > memorySize)
            {
                Upsize();
            }
            System.Buffer.BlockCopy(bytes, 0, buffer, write, count);
            write += count;
        }

        public void Write(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (write + count > memorySize)
            {
                Upsize();
            }
            System.Buffer.BlockCopy(bytes, 0, buffer, write, count);
            write += count;
        }

        public void Write(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (write + count > memorySize)
            {
                Upsize();
            }
            System.Buffer.BlockCopy(bytes, 0, buffer, write, count);
            write += count;
        }

        public void Write(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (write + count > memorySize)
            {
                Upsize();
            }
            System.Buffer.BlockCopy(bytes, 0, buffer, write, count);
            write += count;
        }

        public void Write(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (write + count > memorySize)
            {
                Upsize();
            }
            System.Buffer.BlockCopy(bytes, 0, buffer, write, count);
            write += count;
        }

        public void Write(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (write + count > memorySize)
            {
                Upsize();
            }
            System.Buffer.BlockCopy(bytes, 0, buffer, write, count);
            write += count;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (write + count > memorySize)
            {
                Upsize();
            }
            System.Buffer.BlockCopy(buffer, offset, this.buffer, write, count);
            write += count;
        }

        //public void Write(string value)
        //{
        //    byte[] bytes = Encoding.Unicode.GetBytes(value);
        //    int count = bytes.Length;
        //    if (write + count > memorySize)
        //    {
        //        Upsize();
        //    }
        //    System.Buffer.BlockCopy(bytes, 0, buffer, write, count);
        //    write += count;
        //}

        public bool Read(ref bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (Readable < count)
            {
                return false;
            }

            var result = BitConverter.ToBoolean(buffer, read);
            value = result;
            read += count;
            return true;
        }

        public bool Read(ref byte value)
        {
            int count = 1;
            if (Readable < count)
            {
                return false;
            }

            var result = buffer[read];
            value = result;
            read += count;
            return true;
        }
        public bool Read(ref short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (Readable < count)
            {
                return false;
            }

            var result = BitConverter.ToInt16(buffer, read);
            value = result;
            read += count;
            return true;
        }
        public bool Read(ref int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (Readable < count)
            {
                return false;
            }

            var result = BitConverter.ToInt32(buffer, read);
            value = result;
            read += count;
            return true;
        }
        public bool Read(ref long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (Readable < count)
            {
                return false;
            }

            var result = BitConverter.ToInt64(buffer, read);
            value = result;
            read += count;
            return true;
        }
        public bool Read(ref float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (Readable < count)
            {
                return false;
            }

            var result = BitConverter.ToSingle(buffer, read);
            value = result;
            read += count;
            return true;
        }
        public bool Read(ref double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (Readable < count)
            {
                return false;
            }

            var result = BitConverter.ToDouble(buffer, read);
            value = result;
            read += count;
            return true;
        }
        public bool Read(ref ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (Readable < count)
            {
                return false;
            }

            var result = BitConverter.ToUInt16(buffer, read);
            value = result;
            read += count;
            return true;
        }
        public bool Read(ref uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (Readable < count)
            {
                return false;
            }

            var result = BitConverter.ToUInt32(buffer, read);
            value = result;
            read += count;
            return true;
        }
        public bool Read(ref ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int count = bytes.Length;
            if (Readable < count)
            {
                return false;
            }

            var result = BitConverter.ToUInt64(buffer, read);
            value = result;
            read += count;
            return true;
        }

        public bool Read(byte[] buffer, int offset, int count)
        {
            if (Readable < count)
            {
                return false;
            }

            System.Buffer.BlockCopy(this.buffer, read, buffer, offset, count);
            read += count;

            return true;
        }
    }
}
