using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

        public string ToJson()
        {
            return JsonConvert.SerializeObject(new CommandData(this));
        }
    }

    public class MacroData
    {
        public List<CommandData> datas = new List<CommandData>();
    }

    public class CommandData
    {
        public Type type;
        public List<object> args = new List<object>();

        public CommandData()
        {

        }
        public CommandData(IMacroCommand command)
        {
            type = command.GetType();
            if(command.Args != null)
            {
                int count = command.Args.Count;
                for (int i = 0; i < count; i++)
                {
                    args.Add(command.Args[i]);
                }
            }
        }

        public IMacroCommand ToCommand()
        {
            IMacroCommand command = Activator.CreateInstance(type) as IMacroCommand;
            if(command == null)
            {
                throw new Exception("잘못된 커맨드 데이터.");
            }

            KeyDownCommand kd = new KeyDownCommand();
            KeyUpCommand ku = new KeyUpCommand();
            KeyPressCommand kp = new KeyPressCommand();


            command.Args = new MacroCommandArgs();
            int count = args.Count;
            for (int i = 0; i < count; i++)
            {
                object arg = args[i];
                long d = (long)arg;

                if (type == kd.GetType() || type == ku.GetType() || type == kp.GetType())
                {
                    command.Args.Enqueue((MacroKey)d);
                }
                else
                {
                    command.Args.Enqueue((int)d);
                }
            }

            return command;
        }
    }

}
