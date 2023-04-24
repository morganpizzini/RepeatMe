using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;
using static RepeatMe.Models.Button;

namespace RepeatMe
{
    public struct LetterTime
    {
        public LetterTime(BT7 letter, int time,BT6[] modifiers) : this(letter,modifiers,time)
        {
        }

        public LetterTime(BT7 letter, BT6[] modifiers, int time = 0)
        {
            Modifiers = modifiers;
            Letter = letter;
            Time = time;
        }

        public BT7 Letter { get; set; }
        public BT6[] Modifiers { get; set; }
        public int Time { get; set; }
    }
}
