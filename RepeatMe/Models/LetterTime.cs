using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;
using static RepeatMe.Models.Button;

namespace RepeatMe
{
    public class LetterTime
    {
        public BT7 Letter { get; set; }
        public IList<BT6> Modifiers { get; set; } = new List<BT6>();
        public int Time { get; set; }
    }
}
