using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoMacro
{
    public static class Extension
    {

        public static int GetMemorySize(this VariableType type)
        {
            switch (type)
            {
                case VariableType.Default: return 4;
                case VariableType.Byte: return 1;
                case VariableType.Byte2: return 2;
                case VariableType.Byte4: return 4;
                case VariableType.Byte8: return 8;
                case VariableType.Float: return 4;
                case VariableType.Double: return 8;
                case VariableType.String: return 0;
                case VariableType.End: return 4;
                default: return 4;
            }
        }

        public static int GetHexaStringLength(this VariableType type)
        {
            return GetMemorySize(type) * 2;
        }

        public static bool Validate(this VariableType varialbeType, string str, ref Type type, ref object value)
        {
            switch (varialbeType)
            {
                case VariableType.Byte:
                    if (sbyte.TryParse(str, out sbyte result3))
                    {
                        type = typeof(sbyte);
                        value = result3;
                        return true;
                    }
                    if (byte.TryParse(str, out byte result4))
                    {
                        type = typeof(byte);
                        value = result4;
                        return true;
                    }
                    break;
                case VariableType.Byte2:
                    if (short.TryParse(str, out short result5))
                    {
                        type = typeof(short);
                        value = result5;
                        return true;
                    }
                    if (ushort.TryParse(str, out ushort result6))
                    {
                        type = typeof(ushort);
                        value = result6;
                        return true;
                    }
                    break;
                case VariableType.Byte8:
                    if (long.TryParse(str, out long resul7))
                    {
                        type = typeof(long);
                        value = resul7;
                        return true;
                    }
                    if (ulong.TryParse(str, out ulong result8))
                    {
                        type = typeof(ulong);
                        value = result8;
                        return true;
                    }
                    break;
                case VariableType.Float:
                    if (float.TryParse(str, out float result9))
                    {
                        type = typeof(float);
                        value = result9;
                        return true;
                    }
                    break;
                case VariableType.Double:
                    if (double.TryParse(str, out double result10))
                    {
                        type = typeof(double);
                        value = result10;
                        return true;
                    }
                    break;
                case VariableType.Default:
                case VariableType.Byte4:
					if (uint.TryParse(str, out uint result))
					{
						type = typeof(uint);
						value = result;
						return true;
					}
					if (int.TryParse(str, out int result2))
					{
						type = typeof(int);
						value = result2;
						return true;
					}
					break;
                case VariableType.String:
                    {
                        type = typeof(string);
                        value = str;
                        return true;
                    }

                case VariableType.End:
				default:
                    break;
                    
            }

            return false;
        }

        public static byte[] GetBytes(this object self)
        {
            Type type = self.GetType();
            if(type == typeof(sbyte))
            {
                sbyte origin = (sbyte)self;
                return new byte[1] { (byte)origin };
            }
            else if (type == typeof(byte))
            {
                byte origin = (byte)self;
                return new byte[1] { origin };
            }
            else if (type == typeof(short))
            {
                short origin = (short)self;
                return BitConverter.GetBytes(origin);
            }
            else if (type == typeof(ushort))
            {
                ushort origin = (ushort)self;
                return BitConverter.GetBytes(origin);
            }
            else if (type == typeof(int))
            {
                int origin = (int)self;
                return BitConverter.GetBytes(origin);
            }
            else if (type == typeof(uint))
            {
                uint origin = (uint)self;
                return BitConverter.GetBytes(origin);
            }
            else if (type == typeof(long))
            {
                long origin = (long)self;
                return BitConverter.GetBytes(origin);
            }
            else if (type == typeof(ulong))
            {
                ulong origin = (ulong)self;
                return BitConverter.GetBytes(origin);
            }
            else if (type == typeof(float))
            {
                float origin = (float)self;
                return BitConverter.GetBytes(origin);
            }
            else if (type == typeof(double))
            {
                double origin = (double)self;
                return BitConverter.GetBytes(origin);
            }
            else if (type == typeof(string))
            {
                 string origin = (string)self;
                return Encoding.UTF8.GetBytes(origin);
            }

            return null;
        }

        public static bool IsDigit(this char c)
        {
            if (c < 0x30 || c > 0x39) return false;

            return true;
        }

        public static bool IsHexadecimal(this char c)
        {
            bool isDigit = IsDigit(c);
            bool isAtoF = c >= 'A' && c <= 'F';
            bool isatof = c >= 'a' && c <= 'f';
            if (isDigit || isAtoF || isatof) return true;

            return false;
        }

        public static bool IsDigit(this string str)
        {
            if (string.IsNullOrEmpty(str)) return false;

            int length = str.Length;
            for (int i = 0; i < length; i++)
            {
                if (IsDigit(str[i]) == false) return false;
            }

            return true;
        }

        public static bool IsHexDigit(this string hexString)
        {
            if (string.IsNullOrEmpty(hexString)) return false;

            int length = hexString.Length;
            for (int i = 0; i < length; i++)
            {
                char c = hexString[i];
                if (!IsHexadecimal(c)) return false;
            }

            return true;
        }

        public static bool CompareTo(this byte[] lhs, byte[] rhs)
        {
            if(lhs.Length != rhs.Length) return false;

            for (int i = 0; i < lhs.Length; i++)
            {
                if(lhs[i] != rhs[i]) return false;
            }

            return true;
        }
    }
}
