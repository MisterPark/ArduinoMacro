using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoMacro.Commands
{
    public class KeyUpCommand : IMacroCommand
    {

        public override string Name => "키보드 뗌";

        public override string Description => "Key Up";


        public override void Execute()
        {
            MacroKey key = (MacroKey)Args[0];
            Macro.Instance.KeyUp(key);
        }
    }
}
