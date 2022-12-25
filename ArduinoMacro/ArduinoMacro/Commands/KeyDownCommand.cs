using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoMacro.Commands
{
    public class KeyDownCommand : IMacroCommand
    {

        public override string Name => "키보드 누름";

        public override string Description => "Key Down";


        public override void Execute()
        {
            MacroKey key = (MacroKey)Args[0];
            Macro.Instance.KeyDown(key);
        }
    }
}
