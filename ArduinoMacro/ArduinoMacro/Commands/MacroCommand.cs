using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoMacro.Commands
{
    public abstract class IMacroCommand
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public Func<MacroCommandArgs> ShowDialog { get; set; }

        public MacroCommandArgs Args { get; set; }

        public abstract void Execute();

        public override string ToString()
        {
            StringBuilder stringBuilder= new StringBuilder();
            stringBuilder.Append(Name);
            if(Args != null)
            {
                int count = Args.Count;
                stringBuilder.Append("(");
                for (int i=0; i<count; i++)
                {
                    if(i > 0)
                    {
                        stringBuilder.Append(", ");
                    }
                    stringBuilder.Append(Args[i].ToString());
                }
                stringBuilder.Append(")");
            }
            return stringBuilder.ToString();
        }
    }
}
