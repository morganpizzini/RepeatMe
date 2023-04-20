using System.Collections.Generic;

namespace RepeatMe.Models
{
    public class KeyToPressDictionary
    {
        public KeyToPress Key { get; set; }
        public int[] Values { get; set; }
    }
    public class Setting
    {
        public int StandardDelayTime { get; set; }
        public int SensibilityRange { get; set; }
        public int MaxRotationNumber { get; set; }
        public string DefaultKeyPress { get; set; }
        public IList<string> DefaultKeyModifiers { get; set; } = new List<string>();

        public IList<KeyToPressDictionary> PressKeys { get; set; } = new List<KeyToPressDictionary>();
        public double Interval { get; set; }
    }

    public class KeyToPress
    {
        public string Key { get; set; }
        public IList<string> Modifiers { get; set; } = new List<string>();
    }
}
