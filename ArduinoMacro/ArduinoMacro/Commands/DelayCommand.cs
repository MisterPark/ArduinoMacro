using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoMacro.Commands
{
    public class DelayCommand : IMacroCommand
    {
        public override string Name => "딜레이";

        public override string Description => "ms 단위로 커맨드를 지연시킵니다.";

        public override void Execute()
        {
            int millisecond = (int)Args[0];
            Thread.Sleep(millisecond);
        }
    }
}
