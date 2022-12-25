using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoMacro.Commands
{
    public class MacroCommandArgs
    {
        List<object> args = new List<object>();

        public object this[int index]
        {
            get { return args[index]; } set { args[index] = value; }
        }

        public int Count { get { return args.Count; } }

        public void Enqueue(object item)
        {
            args.Add(item);
        }

    }
}
