using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoMacro
{
    public enum Protocol : byte
    {
        Off = 0,
        On = 1,
        KeyDown = 2,
        KeyUp,
        KeyPress,
    }
}
