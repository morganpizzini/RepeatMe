using System.Collections.Generic;

namespace RepeatMe.Models
{
    public class Setting
    {
        public int DefaultDelayTime { get; set; }
        public int SensibilityRange { get; set; }
        public int MaxRotationNumber { get; set; }
        public string DefaultKeyPress { get; set; }
        public IList<string> DefaultKeyModifiers { get; set; } = new List<string>();

        public IList<KeyToPressDictionary> PressKeys { get; set; } = new List<KeyToPressDictionary>();
        public double Interval { get; set; }
    }
    public class KeyToPressDictionary
    {
        public string Key { get; set; }
        public IList<string> Modifiers { get; set; } = new List<string>();
        public int[] Values { get; set; }
    }
}
